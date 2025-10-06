using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels;

public partial class ConfigurationViewModel : ReactiveObject, IRoutableViewModel
{
    // TODO: make drum positions configurable (simple combobox will do)
    // TODO: make default save directory configurable
    // TODO: add option to revert to default drum mappings, and drum positions
    private readonly ConfigurationService _configService;
    private readonly MidiService _midiService;
    private IDisposable? _beatsSubscription;

    [Reactive] private bool _drumMappingTabSelected;

    private IObservable<int>? _keyboardBeats;
    [Reactive] private bool _keyboardInput;
    private readonly MainViewModel _mainVm;
    [Reactive] private int _metronomeVolume = 8000;

    public ConfigurationViewModel(IScreen hostScreen,
        MidiService midiService,
        ConfigurationService configService)
    {
        HostScreen = hostScreen;
        _midiService = midiService;
        _configService = configService;
        _mainVm = hostScreen as MainViewModel;
        InitConfig();
        this.WhenAnyValue(vm => vm.MetronomeVolume)
            .Subscribe(vol => _configService.MetronomeVolume = vol);
        this.WhenAnyValue(vm => vm.KeyboardInput)
            .Subscribe(ki =>
            {
                ChangeSubscription(_mainVm?.NoConnection ?? true);
                _mainVm!.IsKeyboardInput = ki;
                _configService.KeyboardInput = ki;
                UpdateDrumMappings();
            });
        _mainVm?.WhenAnyValue(vm => vm.NoConnection)
            .Subscribe(ChangeSubscription);
        ListeningDrumChanged
            .Subscribe(UpdateListeningDrum);
        this.WhenAnyValue(vm => vm.MetronomeVolume)
            .Subscribe(vol => _configService.MetronomeVolume = vol);
        this.WhenAnyValue(vm => vm.KeyboardInput)
            .Subscribe(enabled => _configService.KeyboardInput = enabled);
    }

    public ObservableCollection<DrumMappingItem> DrumMappings { get; } = new();

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

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }

    public void CancelMapping()
    {
        StopListening();
    }

    private void InitConfig()
    {
        foreach (var kvp in _configService.Mapping)
            DrumMappings.Add(new DrumMappingItem(kvp.Key, kvp.Value));
        _metronomeVolume = _configService.MetronomeVolume;
        _keyboardInput = _configService.KeyboardInput;
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

    private void ChangeSubscription(bool noConnection)
    {
        _beatsSubscription?.Dispose();
        if (!noConnection)
            _beatsSubscription = _midiService
                .GetRawNoteObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(OnMidiNoteReceived);
        else if (KeyboardInput && KeyboardBeats is not null)
            _beatsSubscription = KeyboardBeats.ObserveOn(RxApp.MainThreadScheduler).Subscribe(OnMidiNoteReceived);
    }

    [ReactiveCommand]
    private void StartListening(Drum drum)
    {
        _configService.StartListening(drum);
        UpdateListeningDrum(drum);
    }

    [ReactiveCommand]
    private void StopListening()
    {
        _configService.StopListening();
        UpdateListeningDrum(null);
    }

    private void OnMidiNoteReceived(int noteNumber)
    {
        if (_configService.ListeningDrum is null)
        {
            var drum = _configService.Mapping.FirstOrDefault(kvp => kvp.Value == noteNumber).Key;
            if (drum != default) HighlightDrumTemporarily(drum);
            return;
        }

        _configService.MapDrum(noteNumber);
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