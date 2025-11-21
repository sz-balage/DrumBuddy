using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using DrumBuddy.Core.Enums;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public partial class ConfigurationViewModel : ReactiveObject, IRoutableViewModel
{
    // TODO: make drum positions configurable (simple combobox will do)
    private readonly ConfigurationService _configService;
    private readonly MainViewModel _mainVm;
    private readonly MidiService _midiService;
    private readonly NotificationService _notificationService;
    private readonly ThemePreferenceService _themePreferenceService;
    private IDisposable? _beatsSubscription;
    [Reactive] private bool _canSyncToServer;

    [Reactive] private ThemeVariant? _currentTheme;

    [Reactive] private bool _drumMappingTabSelected;

    private IObservable<int>? _keyboardBeats;
    [Reactive] private bool _keyboardInput;
    [Reactive] private int _metronomeVolume = 8000;
    [Reactive] private ThemeMode _selectedThemeMode;

    public ConfigurationViewModel(IScreen hostScreen,
        MidiService midiService,
        ConfigurationService configService,
        ThemePreferenceService themePreferenceService)
    {
        HostScreen = hostScreen;
        _midiService = midiService;
        _configService = configService;
        _themePreferenceService = themePreferenceService;
        var mainViewModel = Locator.Current.GetRequiredService<MainViewModel>();
        _notificationService = Locator.Current.GetRequiredService<NotificationService>("MainWindowNotificationService");
        _mainVm = hostScreen as MainViewModel;
        this.WhenAnyValue(vm => vm.SelectedThemeMode)
            .Skip(1)
            .Subscribe(async mode =>
            {
                await ApplyThemeAsync(mode);
                await _themePreferenceService.SavePreferenceAsync(mode);
            });
        this.WhenAnyValue(vm => vm.MetronomeVolume)
            .Subscribe(vol => _configService.MetronomeVolume = vol);
        this.WhenAnyValue(vm => vm.KeyboardInput)
            .Subscribe(ki =>
            {
                ChangeSubscription(ki);
                _mainVm!.IsKeyboardInput = ki;
                _configService.KeyboardInput = ki;
                UpdateDrumMappings();
            });
        _mainVm?.WhenAnyValue(vm => vm.NoConnection)
            .Subscribe(ChangeSubscription);
        ListeningDrumChanged
            .Subscribe(UpdateListeningDrum);
        this.WhenAnyValue(vm => vm.MetronomeVolume)
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Subscribe(async void (vol) =>
            {
                try
                {
                    _configService.MetronomeVolume = vol;
                    await _configService.SaveAsync();
                }
                catch (Exception e)
                {
                    // ignored
                }
            });
        this.WhenAnyValue(vm => vm.KeyboardInput)
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Subscribe(async void (enabled) =>
            {
                try
                {
                    _configService.KeyboardInput = enabled;
                    await _configService.SaveAsync();
                }
                catch (Exception e)
                {
                    // ignored
                }
            });
        LoadConfig().Wait();
    }

    public ObservableCollection<DrumMappingItem> DrumMappings { get; } = new();

    public ObservableCollection<ThemeMode> AvailableThemes { get; } =
        new(Enum.GetValues(typeof(ThemeMode)).Cast<ThemeMode>());

    public IObservable<int>? KeyboardBeats
    {
        get => _keyboardBeats;
        set
        {
            _keyboardBeats = value;
            if (KeyboardInput) ChangeSubscription(true); // retrigger when beats source is ready
        }
    }

    public IReadOnlyDictionary<Drum, int> Mapping => _configService.Mapping;
    public IObservable<Drum?> ListeningDrumChanged => _configService.ListeningDrumChanged;

    private IObservable<bool> CanRecheckMidiDevices => this.WhenAnyValue(vm => vm.KeyboardInput).Select(ki => !ki);

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }

    private async Task ApplyThemeAsync(ThemeMode mode)
    {
        var themeVariant = mode switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        var app = (App)Application.Current!;
        app.SetTheme(themeVariant);
    }

    public void CancelMapping()
    {
        StopListening();
    }

    public async Task LoadConfig()
    {
        await _configService.LoadConfig();

        CanSyncToServer = _configService.CanSyncToServer;
        DrumMappings.Clear();
        foreach (var kvp in _configService.Mapping)
            DrumMappings.Add(new DrumMappingItem(kvp.Key, kvp.Value));
        MetronomeVolume = _configService.MetronomeVolume;
        KeyboardInput = _configService.KeyboardInput;

        SelectedThemeMode = _themePreferenceService.Preference.Mode;

        UpdateDrumMappings();
    }

    private void UpdateDrumMappings()
    {
        var currentMapping = _keyboardInput
            ? _configService.GetKeyboardMapping()
            : _configService.GetDrumMapping();
        foreach (var item in DrumMappings)
        {
            if (currentMapping.TryGetValue(item.Drum, out var note))
                item.Note = note;
        }
    }

    private void UpdateListeningDrum(Drum? drum)
    {
        foreach (var item in DrumMappings)
            item.IsListening = item.Drum == drum;
    }

    private void ChangeSubscription(bool keyboardInput)
    {
        _beatsSubscription?.Dispose();
        if (!keyboardInput)
            _beatsSubscription = _midiService
                .GetRawNoteObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async void (n) =>
                {
                    try
                    {
                        await OnMidiNoteReceivedAsync(n);
                    }
                    catch (Exception e)
                    {
                        //ignored
                    }
                });
        else if (KeyboardBeats is not null)
            _beatsSubscription = KeyboardBeats.ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async void (n) =>
                {
                    try
                    {
                        await OnMidiNoteReceivedAsync(n);
                    }
                    catch (Exception e)
                    {
                        //ignored
                    }
                });
    }

    [ReactiveCommand]
    private void StartListening(Drum drum)
    {
        _configService.StartListening(drum);
        UpdateListeningDrum(drum);
    }

    [ReactiveCommand(CanExecute = nameof(CanRecheckMidiDevices))]
    private async Task RecheckMIDIDevices()
    {
        await _mainVm.ForceRecheckMidiDevices();
    }

    [ReactiveCommand]
    private void StopListening()
    {
        _configService.StopListening();
        UpdateListeningDrum(null);
    }

    [ReactiveCommand]
    private async Task RevertToDefaultMappings()
    {
        if (KeyboardInput)
            await _configService.SetDefaultKeyboardMappings();
        else
            await _configService.SetDefaultDrumMappings();
        UpdateDrumMappings();
    }

    private async Task OnMidiNoteReceivedAsync(int noteNumber)
    {
        if (_configService.ListeningDrum is null)
        {
            var drum = _configService.Mapping.FirstOrDefault(kvp => kvp.Value == noteNumber).Key;
            if (drum != default) HighlightDrumTemporarily(drum);
            return;
        }

        await _configService.MapDrumAsync(noteNumber);
        UpdateDrumMappings();
    }

    private void HighlightDrumTemporarily(Drum drum)
    {
        var item = DrumMappings.FirstOrDefault(d => d.Drum == drum);
        if (item == null) return;

        item.IsHighlighted = true;
        Task.Delay(500).ContinueWith(_ => item.IsHighlighted = false);
    }
}