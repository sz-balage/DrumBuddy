using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.Services;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Abstractions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class ManualEditorViewModel : ReactiveObject, IRoutableViewModel
{
    public const int Columns = 16; // one measure, 16 sixteenth steps
    private readonly SourceList<MeasureViewModel> _measureSource = new();
    private readonly List<bool[,]> _measureSteps;
    private readonly Func<Task> _onClose;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private readonly ISheetStorage _sheetStorage;

    public readonly Drum[] Drums = new[]
    {
        Drum.Kick,
        Drum.Snare,
        Drum.HiHat,
        Drum.Tom1,
        Drum.Tom2,
        Drum.FloorTom,
        Drum.Ride,
        Drum.Crash
    };

    public readonly ReadOnlyObservableCollection<MeasureViewModel> Measures;
    public readonly ReadOnlyObservableCollection<Sheet> Sheets;
    private Bpm _bpm;
    [Reactive] private decimal _bpmDecimal;
    private int _currentMeasureIndex;
    private Sheet? _currentSheet;
    [Reactive] private string? _description;
    [Reactive] private bool _editorVisible;

    [Reactive] private bool _isSaved = true;
    [Reactive] private string? _name;
    private NotificationManager _notificationManager;
    public ManualEditorViewModel(IScreen host, Func<Task> onClose)
    {
        _sheetStorage = Locator.Current.GetRequiredService<ISheetStorage>();
        HostScreen = host;
        UrlPathSegment = "manual-editor";
        _measureSource.Connect()
            .Bind(out Measures)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
        _sheetSource.Connect()
            .SortBy(s => s.Name)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out Sheets)
            .Subscribe();
        this.WhenAnyValue(vm => vm.BpmDecimal)
            .Skip(1)
            .Subscribe(i =>
            {
                var value = Convert.ToInt32(i);
                _bpm = new Bpm(value);
                CurrentSheet!.Tempo = _bpm;
            });
        _measureSteps = new List<bool[,]>
        {
            new bool[Drums.Length, Columns]
        };
        _bpm = 100;
        CurrentSheet = BuildSheet();
        BpmDecimal = CurrentSheet.Tempo.Value;
        DrawSheet();
        SaveCommand.ThrownExceptions.Subscribe(ex => Console.WriteLine(ex.Message));
        _onClose = onClose;
        _notificationManager = Locator.Current.GetRequiredService<NotificationManager>();
    }

    public Sheet? CurrentSheet
    {
        get => _currentSheet;
        private set => this.RaiseAndSetIfChanged(ref _currentSheet, value);
    }

    public int CurrentMeasureIndex
    {
        get => _currentMeasureIndex;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentMeasureIndex, value);
            this.RaisePropertyChanged(nameof(CanGoBack));
            this.RaisePropertyChanged(nameof(CanGoForward));
            this.RaisePropertyChanged(nameof(MeasureDisplayText));
        }
    }

    public bool CanGoBack => CurrentMeasureIndex > 0;
    public bool CanGoForward => CurrentMeasureIndex < _measureSteps?.Count - 1;
    public string MeasureDisplayText => $"Measure {CurrentMeasureIndex + 1} of {_measureSteps.Count}";
    public Interaction<SheetCreationData, SheetNameAndDescription> ShowSaveDialog { get; } = new();
    public Interaction<Unit, Confirmation> ShowConfirmation { get; } = new();

    public IScreen HostScreen { get; }
    public string? UrlPathSegment { get; }

    [ReactiveCommand]
    private async Task NavigateBack()
    { 
        if (!IsSaved)
        {
            var result = await ShowConfirmation.Handle(Unit.Default);
            if (result == Confirmation.Close)
                await _onClose();
        }
        else
        {
            await _onClose();
        }
    }

    public void ToggleStep(int row, int col)
    {
        IsSaved = false;
        if (row < 0 || row >= Drums.Length) return;
        if (col < 0 || col >= Columns) return;
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return;

        _measureSteps[CurrentMeasureIndex][row, col] = !_measureSteps[CurrentMeasureIndex][row, col];
        CurrentSheet = BuildSheet();
        RedrawMeasureAt();
    }

    public bool GetStep(int row, int col)
    {
        if (row < 0 || row >= Drums.Length) return false;
        if (col < 0 || col >= Columns) return false;
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return false;
        return _measureSteps[CurrentMeasureIndex][row, col];
    }

    [ReactiveCommand]
    private async Task Save()
    {
        if (Name is null)
        {
            var dialogResult = await ShowSaveDialog.Handle(new SheetCreationData(_bpm,
                [..CurrentSheet?.Measures ?? ImmutableArray<Measure>.Empty]));
            Name = dialogResult.Name;
            Description = dialogResult.Description;
            CurrentSheet = BuildSheet();
        }
        else
        {
            await _sheetStorage.UpdateSheetAsync(CurrentSheet!);
        }
        _notificationManager.ShowSuccessNotification($"The sheet {Name} successfully saved.");
        IsSaved = true;
    }

    [ReactiveCommand]
    private void AddMeasure()
    {
        var newMeasure = new bool[Drums.Length, Columns];
        _measureSteps.Add(newMeasure);
        CurrentMeasureIndex = _measureSteps.Count - 1;
        CurrentSheet = BuildSheet();
        DrawSheet();
    }

    [ReactiveCommand]
    private void GoToPreviousMeasure()
    {
        if (CanGoBack) CurrentMeasureIndex--;
    }

    [ReactiveCommand]
    private void GoToNextMeasure()
    {
        if (CanGoForward) CurrentMeasureIndex++;
    }
    public void SelectMeasure(int index)
    {
        if (index < 0 || index >= _measureSteps.Count)
            return;

        CurrentMeasureIndex = index;
    }

    public void LoadMatrix(bool[,] matrix)
    {
        if (matrix.GetLength(0) != Drums.Length || matrix.GetLength(1) != Columns)
            throw new ArgumentException("Matrix size must be [drums x 16].");

        if (CurrentMeasureIndex >= 0 && CurrentMeasureIndex < _measureSteps.Count)
        {
            Array.Clear(_measureSteps[CurrentMeasureIndex], 0, _measureSteps[CurrentMeasureIndex].Length);
            Array.Copy(matrix, _measureSteps[CurrentMeasureIndex], matrix.Length);
        }

        CurrentSheet = BuildSheet();
        DrawSheet();
    }

    public void LoadSheet(Sheet sheet)
    {
        _measureSteps.Clear();

        if (sheet is null || sheet.Measures.Length == 0)
        {
            _measureSteps.Add(new bool[Drums.Length, Columns]);
            CurrentMeasureIndex = 0;
            CurrentSheet = BuildSheet();
            DrawSheet();
            return;
        }

        foreach (var measure in sheet.Measures)
        {
            var measureMatrix = new bool[Drums.Length, Columns];

            // Convert measure -> bool[,] matrix
            var col = 0;
            foreach (var group in measure.Groups)
            foreach (var noteGroup in group.NoteGroups)
            {
                foreach (var note in noteGroup)
                {
                    var drumIndex = Array.IndexOf(Drums, note.Drum);
                    if (drumIndex >= 0 && col < Columns)
                        measureMatrix[drumIndex, col] = true;
                }

                col++;
            }

            _measureSteps.Add(measureMatrix);
        }

        CurrentMeasureIndex = 0;
        Name = sheet.Name;
        Description = sheet.Description;
        BpmDecimal = sheet.Tempo.Value;
        CurrentSheet = BuildSheet();
        DrawSheet();
    }

    private Sheet BuildSheet()
    {
        var allMeasures = new List<Measure>();

        foreach (var measureSteps in _measureSteps)
        {
            var groups = new List<RythmicGroup>();
            for (var g = 0; g < 4; g++)
            {
                var noteGroups = new List<NoteGroup>();

                for (var c = 0; c < 4; c++)
                {
                    var col = g * 4 + c;
                    var notes = new List<Note>();

                    for (var r = 0; r < Drums.Length; r++)
                        if (measureSteps[r, col])
                            notes.Add(new Note(Drums[r], NoteValue.Sixteenth));

                    noteGroups.Add(notes.Count > 0 ? new NoteGroup(notes) : new NoteGroup());
                }

                var upscaled = RecordingService.UpscaleNotes(noteGroups);
                groups.Add(new RythmicGroup([..upscaled]));
            }

            allMeasures.Add(new Measure(groups));
        }

        return new Sheet(_bpm, [..allMeasures], Name ?? "Untitled", Description ?? "");
    }

    private void DrawSheet()
    {
        _measureSource.Clear();
        _measureSource.AddRange(CurrentSheet?.Measures.Select(m => new MeasureViewModel(m)) ??
                                Array.Empty<MeasureViewModel>());
        var idx = CurrentMeasureIndex;
        CurrentMeasureIndex = -1;
        CurrentMeasureIndex = idx;
    }
    private void RedrawMeasureAt()
    {
        if (CurrentSheet == null || CurrentMeasureIndex < 0 || CurrentMeasureIndex >= CurrentSheet.Measures.Length)
            return;
        var idx = CurrentMeasureIndex;
        var updatedMeasure = new MeasureViewModel(CurrentSheet.Measures[CurrentMeasureIndex]);
        _measureSource.ReplaceAt(CurrentMeasureIndex,updatedMeasure); 
        
        CurrentMeasureIndex = -1;
        CurrentMeasureIndex = idx;
    }
    public void DeleteSelectedMeasure()
    {
        if (_measureSteps.Count <= 1) return; 
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return;
        var idx = CurrentMeasureIndex;

        _measureSteps.RemoveAt(CurrentMeasureIndex);

        _measureSource.RemoveAt(CurrentMeasureIndex);
        CurrentMeasureIndex = -1;
        if (CurrentMeasureIndex >= _measureSteps.Count)
            CurrentMeasureIndex = _measureSteps.Count - 1;
        else
            CurrentMeasureIndex = idx;
        IsSaved = false;
    }
}