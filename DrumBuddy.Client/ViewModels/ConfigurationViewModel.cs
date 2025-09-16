using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Abstractions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.Client.ViewModels;

public partial class ConfigurationViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly ConfigurationService _config;
    private readonly IMidiService _midiService;
    private IDisposable? _beatsSubscription;

    public ObservableCollection<DrumMappingItem> DrumMappings { get; } = new();

    public ConfigurationViewModel(IScreen hostScreen, IMidiService midiService, ConfigurationService config)
    {
        HostScreen = hostScreen;
        _midiService = midiService;
        _config = config;
        var mainVm = hostScreen as MainViewModel;
        foreach (var kvp in _config.Mapping)
            DrumMappings.Add(new DrumMappingItem(kvp.Key, kvp.Value));
        MappingChanged.Subscribe(_ => UpdateDrumMappings());
        
        this.WhenAnyValue(vm => vm.KeyboardInput)
            .Subscribe(_ => ChangeSubscription(mainVm?.NoConnection ?? true)); 
        mainVm?.WhenAnyValue(vm => vm.NoConnection)
            .Subscribe(ChangeSubscription);
    }

    private void UpdateDrumMappings()
    {
        foreach (var item in DrumMappings)
        {
            if (_config.Mapping.TryGetValue(item.Drum, out var note))
                item.Note = note;
        }
    }

    public IObservable<int> KeyboardBeats { get; set; }
    private void ChangeSubscription(bool noConnection)
    {
        _beatsSubscription?.Dispose();
        if (!noConnection)
            _beatsSubscription = _midiService
                .GetRawNoteObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(OnMidiNoteReceived);
        else if (KeyboardInput == true)
            _beatsSubscription = KeyboardBeats!.ObserveOn(RxApp.MainThreadScheduler).Subscribe(OnMidiNoteReceived);
    }
    [Reactive] private bool _keyboardInput;
    [ReactiveCommand]
    private void StartListening(Drum drum)
    {
        _config.StartListening(drum);
    }

    [ReactiveCommand]
    private void StopListening()
    {
        _config.StopListening();
    }

    public IReadOnlyDictionary<Drum, int> Mapping => _config.Mapping;
    public IObservable<Drum?> ListeningDrumChanged => _config.ListeningDrumChanged;
    private readonly Subject<Unit> _mappingChanged = new();
    public IObservable<Unit> MappingChanged => _mappingChanged.AsObservable();

    private void OnMidiNoteReceived(int noteNumber)
    {
        if (_config.ListeningDrum is null)
        {
            var drum = _config.Mapping.FirstOrDefault(kvp => kvp.Value == noteNumber).Key;
            if (drum != default)
            {
                HighlightDrumTemporarily(drum);
            }
            return;
        }
        _config.MapDrum(noteNumber); 
        _mappingChanged.OnNext(Unit.Default);
    }

    private void HighlightDrumTemporarily(Drum drum)
    {
        var item = DrumMappings.FirstOrDefault(d => d.Drum == drum);
        if (item == null) return;

        item.IsHighlighted = true;
        Task.Delay(500).ContinueWith(_ => item.IsHighlighted = false);
    }

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
}