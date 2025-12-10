using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using DrumBuddy.Core.Extensions;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels.HelperViewModels;
using DrumBuddy.Views;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public partial class RecordingViewModel : ReactiveObject, IRoutableViewModel, IDisposable
{
    private static int _globalPointerIdx;
    private readonly ConfigurationService _configService;
    private readonly ReadOnlyObservableCollection<MeasureViewModel> _measures;
    private readonly SourceList<MeasureViewModel> _measureSource = new();
    private readonly MetronomePlayer _metronomePlayer;
    private readonly MidiService _midiService;
    private readonly NotificationService _notificationService;
    private readonly SheetService _sheetService;
    private readonly IObservable<bool> _stopRecordingCanExecute;
    private Bpm _bpm;
    [Reactive] private decimal _bpmDecimal;
    [Reactive] private int _countDown;
    [Reactive] private bool _countDownVisibility;
    [Reactive] private MeasureViewModel _currentMeasure;
    [Reactive] private bool _isLoadingSheets;
    [Reactive] private bool _isPaused;
    [Reactive] private bool _isRecording;
    [Reactive] private bool _overlayVisible;
    private Sheet _selectedSheet;

    private SheetOption _selectedSheetOption;
    private CompositeDisposable _subs = new();
    private long _tick;

    [Reactive] private string _timeElapsed;
    private DispatcherTimer _timer;

    public RecordingViewModel(IScreen hostScreen,
        MidiService midiService,
        ConfigurationService configService,
        SheetService sheetService,
        MetronomePlayer metronomePlayer)
    {
        HostScreen = hostScreen;
        _midiService = midiService;
        _configService = configService;
        _sheetService = sheetService;
        _notificationService = Locator.Current.GetRequiredService<NotificationService>("MainWindowNotificationService");
        _metronomePlayer = metronomePlayer;

        //binding measuresource
        _measureSource.Connect()
            .Bind(out _measures)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
        _measureSource.AddRange(Enumerable.Range(1, 20).ToList().Select(_ => new MeasureViewModel()));
        _stopRecordingCanExecute = this.WhenAnyValue(vm => vm.IsRecording, vm => vm.CurrentMeasure,
            (recording, currentMeasure) => recording && currentMeasure != null);
        //bpm changes should update the _bpm prop
        this
            .WhenAnyValue(vm =>
                vm.BpmDecimal) //when the bpm changes, update the _bpm prop (should never be invalid due to the NumericUpDown control)
            .Skip(1)
            .Subscribe(i =>
            {
                var value = Convert.ToInt32(i);
                _bpm = new Bpm(value);
            });
        //default values
        BpmDecimal = 100;
        TimeElapsed = "0:0:0";
        IsRecording = false;
        IsPaused = false;
        CurrentMeasure = null!;
        SheetOptions = new ObservableCollection<SheetOption>();
    }

    public Sheet SelectedSheet
    {
        get => _selectedSheet;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSheet, value);
            if (value == null)
            {
                OverlayVisible = false;
                OverlayMeasures.Clear();
            }
            else
            {
                OverlayVisible = true;
                OverlayMeasures.Clear();
                OverlayMeasures.Add(value.Measures.Select(m => new MeasureViewModel(m)));
            }
        }
    }

    public SheetOption SelectedSheetOption
    {
        get => _selectedSheetOption;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSheetOption, value);
            SelectedSheet = value?.Sheet;  
        }
    }

    private bool _keyboardInputEnabled => _configService.KeyboardInput;

    public IObservable<int> KeyboardBeats { get; set; }
    public Interaction<SheetCreationData, SheetNameAndDescription> ShowSaveDialog { get; } = new();

    public ReadOnlyObservableCollection<MeasureViewModel> Measures => _measures;
    public ObservableCollection<SheetOption> SheetOptions { get; } = new();
    public ObservableCollection<MeasureViewModel> OverlayMeasures { get; } = new();

    public void Dispose()
    {
        _measureSource.Dispose();
        _subs.Dispose();
        _startRecordingCommand?.Dispose();
        _stopRecordingCommand?.Dispose();
        _pauseRecordingCommand?.Dispose();
        _resumeRecordingCommand?.Dispose();
    }

    public string? UrlPathSegment { get; } = "recording-view";
    public IScreen HostScreen { get; }

    public async Task LoadSheets()
    {
        IsLoadingSheets = true;
        var sheets = await _sheetService?.LoadSheetsAsync();
        SheetOptions.Clear();
        SheetOptions.Add(new SheetOption("None", null));
        foreach (var sheet in sheets)
            SheetOptions.Add(new SheetOption(sheet.Name, sheet));
        SelectedSheetOption = SheetOptions[0];
        IsLoadingSheets = false;
    }


    private void InitTimer()
    {
        _tick = 0;
        _subs = new CompositeDisposable();
        _timer = new DispatcherTimer();
        _timer.Tick += (s, e) =>
        {
            _tick++; //increments every second
            TimeElapsed = $"{_tick / 6000 % 60:D2}:{_tick / 100 % 60:D2}:{_tick % 100:D2}";
        };
        _timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
    }

    private void InitMetronomeSubs()
    {
        var metronomeObs = RecordingService.GetMetronomeBeeping(_bpm);
        _subs.Add(metronomeObs
            .Take(4)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(HandleCountDown, () => _timer.Start()));
        _subs.Add(metronomeObs
            .Skip(4)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(MovePointerOnMetronomeBeeps));
    }

    private void InitBeatSub()
    {
        //drum sub
        var measureIdx = -1;
        var rythmicGroupIndex = -1;
        var delay = 5 * _bpm.QuarterNoteDuration();
        var tempNotes = new List<NoteGroup>();
        _subs.Add(RecordingService
            .GetNotes(_bpm,
                _keyboardInputEnabled
                    ? KeyboardBeats.GetMappedBeatsForKeyboard(_configService)
                    : _midiService.GetMappedBeatsObservable(_configService))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select((notes, idx) => (notes, idx))
            .DelaySubscription(delay)
            .Subscribe(data =>
            {
                //idx is the current sixteenth note 
                var localMIdx = data.idx / 16;
                var localRgIdx = data.idx % 16 / 4;
                measureIdx = localMIdx;
                if (rythmicGroupIndex != localRgIdx)
                {
                    rythmicGroupIndex = localRgIdx;
                    if (rythmicGroupIndex == 0)
                    {
                        if (measureIdx != 0)
                        {
                            var actualIdx = measureIdx - 1;
                            var actualMeasure = Measures[actualIdx];
                            actualMeasure.AddRythmicGroupFromNotes(tempNotes, 3);
                            //add evaluation for the rythmicgroup and remove measure for overlay (if present, and overlay is enabled)
                            if (OverlayVisible && OverlayMeasures.Count > actualIdx)
                            {
                                var overlayMeasure = OverlayMeasures[actualIdx];
                                if (overlayMeasure.RythmicGroups.Count == 4)
                                {
                                    var overlayRg = overlayMeasure.RythmicGroups[3];
                                    var eval = MeasureEvaluator.EvaluateGroup(actualIdx,
                                        3,
                                        overlayRg.RythmicGroup,
                                        actualMeasure.RythmicGroups[3].RythmicGroup);
                                    actualMeasure.EvaluationBoxes.Add(eval);
                                    overlayMeasure.RythmicGroups[3].Hide();
                                }
                            }
                        }
                    }
                    else
                    {
                        //add evaluation for the rythmicgroup and remove measure for overlay (if present, and overlay is enabled)0
                        Measures[measureIdx].AddRythmicGroupFromNotes(tempNotes, rythmicGroupIndex);
                        if (OverlayVisible && OverlayMeasures.Count > measureIdx)
                        {
                            var overlayMeasure = OverlayMeasures[measureIdx];
                            if (overlayMeasure.RythmicGroups.Count >= rythmicGroupIndex)
                            {
                                var overlayRg = overlayMeasure.RythmicGroups[rythmicGroupIndex - 1];
                                var eval = MeasureEvaluator.EvaluateGroup(measureIdx,
                                    rythmicGroupIndex - 1,
                                    overlayRg.RythmicGroup,
                                    Measures[measureIdx].RythmicGroups[rythmicGroupIndex - 1].RythmicGroup);
                                Measures[measureIdx].EvaluationBoxes.Add(eval);
                                overlayMeasure.RythmicGroups[rythmicGroupIndex - 1].Hide();
                            }
                        }
                    }

                    tempNotes.Clear();
                }

                tempNotes.Add(new NoteGroup(data.notes));
            }));
    }

    private void HandleCountDown(long idx)
    {
        if (idx == 0)
        {
            CountDownVisibility = true;
            _metronomePlayer.PlayHighBeep();
        }
        else
        {
            _metronomePlayer.PlayNormalBeep();
        }

        CountDown--;
    }

    private void MovePointerOnMetronomeBeeps(long idx)
    {
        if (idx == 0)
        {
            _metronomePlayer.PlayHighBeep();
            if (CurrentMeasure == null!)
            {
                CountDownVisibility = false;
                CurrentMeasure = Measures[0];
            }
            else
            {
                CurrentMeasure.IsPointerVisible = false;
                CurrentMeasure = Measures[Measures.IndexOf(CurrentMeasure) + 1];
            }
        }
        else
        {
            _metronomePlayer.PlayNormalBeep();
        }

        if (Measures.Count - Measures.IndexOf(CurrentMeasure) < 20) //always have 20 measures in advance
            _measureSource.AddRange(Enumerable.Range(1, 20).ToList().Select(_ => new MeasureViewModel()));
        CurrentMeasure?.MovePointerToRg(idx);
        if (_globalPointerIdx == 0)
            _timer.Start();
        _globalPointerIdx++;
    }

    [ReactiveCommand]
    private void StartRecording()
    {
        if (!_configService.KeyboardInput && !_midiService.IsConnected)
        {
            _notificationService.ShowNotification(new Notification("No input device found.",
                "Please connect your midi device and try again, or enable keyboard input in the settings.",
                NotificationType.Error));
            return;
        }

        InitTimer();
        CountDown = 5;
        InitMetronomeSubs();
        InitBeatSub();
        _globalPointerIdx = 0;

        #region UI buttons

        IsRecording = true;
        IsPaused = false;

        #endregion
    }


    [ReactiveCommand(CanExecute = nameof(_stopRecordingCanExecute))]
    private async Task StopRecording()
    {
        _subs.Dispose();
        _timer.Stop();
        ResetPointer();
        IsRecording = false;
        IsPaused = false;
        TimeElapsed = "0:0:0";

        var measures = Measures.Where(m => !m.IsEmpty).Select(vm => vm.Measure).ToList();
        if (measures.Count == 0)
        {
            _notificationService.ShowNotification(new Notification("Recording stopped.", "No notes captured.",
                NotificationType.Warning));
            ClearMeasures();
            return;
        }

        if (OverlayVisible)
            foreach (var measureViewModel in OverlayMeasures)
                measureViewModel.RythmicGroups.ToList().ForEach(rg => rg.Show());

        //ask user if sheet should be saved
        var dialogResult = await ShowSaveDialog.Handle(new SheetCreationData(_bpm, [..measures]));
        ClearMeasures();
        if (dialogResult.Name != null)
        {
            _notificationService.ShowNotification(new Notification("Successful save.",
                $"The sheet {dialogResult.Name} successfully saved.",
                NotificationType.Success));
            var mainVm = HostScreen as MainViewModel;
            mainVm!.NavigateFromCode(Locator.Current.GetRequiredService<LibraryViewModel>());
        }
    }

    private void ClearMeasures()
    {
        _measureSource.Clear();
        _measureSource.AddRange(Enumerable.Range(1, 70).ToList().Select(i => new MeasureViewModel()));
    }

    [ReactiveCommand(CanExecute = nameof(_stopRecordingCanExecute))]
    private void PauseRecording() //not implemented for now
    {
        IsPaused = true;
    }

    [ReactiveCommand]
    private void ResumeRecording() //not implemented for now
    {
        IsPaused = false;
    }

    private void ResetPointer()
    {
        CurrentMeasure.IsPointerVisible = false;
        CurrentMeasure = null;
    }
}