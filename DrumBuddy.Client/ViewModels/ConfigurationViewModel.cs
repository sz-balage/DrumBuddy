using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DrumBuddy.Client.Extensions;
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
    private readonly Subject<Drum> _highlightDrumSubject = new();

    public ConfigurationViewModel(IScreen hostScreen, IMidiService midiService, ConfigurationService config)
    {
        HostScreen = hostScreen;
        _midiService = midiService;
        _config = config;
        var mainVm = hostScreen as MainViewModel;
        this.WhenAnyValue(vm => vm.KeyboardInput)
            .Subscribe(_ => ChangeSubscription(mainVm?.NoConnection ?? true));
        mainVm?.WhenAnyValue(vm => vm.NoConnection).Subscribe(ChangeSubscription);
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
    public IObservable<Drum> HighlightDrum => _highlightDrumSubject.AsObservable();
    private readonly Subject<Unit> _mappingChanged = new();
    public IObservable<Unit> MappingChanged => _mappingChanged.AsObservable();

    private void OnMidiNoteReceived(int noteNumber)
    {
        if (_config.ListeningDrum is null)
        {
            var drum = _config.Mapping.FirstOrDefault(kvp => kvp.Value == noteNumber).Key;
            if (drum != default)
                _highlightDrumSubject.OnNext(drum);
            return;
        }
        _config.MapDrum(noteNumber); 
        _mappingChanged.OnNext(Unit.Default);
    }

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
}