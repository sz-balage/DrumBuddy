using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.Client.ViewModels;

public sealed record ManualSheetDraft(IReadOnlyList<Drum> Drums, bool[,] Steps);

public sealed partial class ManualViewModel : ReactiveObject, IRoutableViewModel
{
    public const int Columns = 16; // one measure, 16 sixteenth steps
    private readonly List<bool[,]> _measureSteps; 
    private ManualSheetDraft _draft;
    private Sheet? _currentSheet;
    private int _currentMeasureIndex = 0;

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
    public bool CanGoForward =>  CurrentMeasureIndex < _measureSteps?.Count - 1;
    public string MeasureDisplayText => $"Measure {CurrentMeasureIndex + 1} of {_measureSteps.Count}";

    public readonly ReadOnlyObservableCollection<MeasureViewModel> Measures;
    private readonly SourceList<MeasureViewModel> _measureSource = new();

    public ManualViewModel(IScreen host)
    {
        HostScreen = host;
        UrlPathSegment = "manual-editor";
        _measureSource.Connect()
            .Bind(out Measures)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
        
        _measureSteps = new List<bool[,]>
        {
            new bool[Drums.Length, Columns]
        };
        _draft = BuildDraft();
        CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
        DrawMeasures();
    }

    public ManualSheetDraft Draft
    {
        get => _draft;
        private set => this.RaiseAndSetIfChanged(ref _draft, value);
    }

    public IScreen HostScreen { get; }
    public string? UrlPathSegment { get; }

    public void ToggleStep(int row, int col)
    {
        if (row < 0 || row >= Drums.Length) return;
        if (col < 0 || col >= Columns) return;
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return;

        _measureSteps[CurrentMeasureIndex][row, col] = !_measureSteps[CurrentMeasureIndex][row, col];
        Draft = BuildDraft();
        CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
        DrawMeasures();
    }

    public bool GetStep(int row, int col)
    {
        if (row < 0 || row >= Drums.Length) return false;
        if (col < 0 || col >= Columns) return false;
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return false;
        return _measureSteps[CurrentMeasureIndex][row, col];
    }
    [ReactiveCommand]
    private void AddMeasure()
    {
        var newMeasure = new bool[Drums.Length, Columns];
        _measureSteps.Add(newMeasure);
        CurrentMeasureIndex = _measureSteps.Count - 1; 
        
        Draft = BuildDraft();
        CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
        DrawMeasures();
    }
    [ReactiveCommand]
    public void GoToPreviousMeasure()
    {
        if (CanGoBack)
        {
            CurrentMeasureIndex--;
        }
    }
    [ReactiveCommand]
    public void GoToNextMeasure()
    {
        if (CanGoForward)
        {
            CurrentMeasureIndex++;
        }
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

        Draft = BuildDraft();
        CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
        DrawMeasures();
    }

    public void LoadSheet(Sheet sheet)
    {
        _measureSteps.Clear();

        if (sheet is null || sheet.Measures.Length == 0)
        {
            _measureSteps.Add(new bool[Drums.Length, Columns]);
            CurrentMeasureIndex = 0;
            Draft = BuildDraft();
            CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
            DrawMeasures();
            return;
        }

        foreach (var measure in sheet.Measures)
        {
            var measureMatrix = new bool[Drums.Length, Columns];
            _measureSteps.Add(measureMatrix);
        }

        CurrentMeasureIndex = 0;
        Draft = BuildDraft();
        CurrentSheet = BuildSheet(sheet.Tempo, sheet.Name, sheet.Description);
        DrawMeasures();
    }

    private Sheet BuildSheet(Bpm tempo, string name, string description)
    {
        var allMeasures = new List<Measure>();

        foreach (var measureSteps in _measureSteps)
        {
            var groups = new List<RythmicGroup>();

            // 16 columns = 4 rhythmic groups
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
                groups.Add(new RythmicGroup(upscaled.ToImmutableArray()));
            }

            allMeasures.Add(new Measure(groups));
        }

        return new Sheet(tempo, allMeasures.ToImmutableArray(), name, description);
    }

    private void DrawMeasures()
    {
        _measureSource.Clear();
        _measureSource.AddRange(CurrentSheet?.Measures.Select(m => new MeasureViewModel(m)) ?? Array.Empty<MeasureViewModel>());
        var idx = CurrentMeasureIndex;
        CurrentMeasureIndex = -1;
        CurrentMeasureIndex = idx;
    }

    private ManualSheetDraft BuildDraft()
    {
        var currentSteps = CurrentMeasureIndex >= 0 && CurrentMeasureIndex < _measureSteps.Count 
            ? _measureSteps[CurrentMeasureIndex] 
            : new bool[Drums.Length, Columns];
        
        return new ManualSheetDraft(Array.AsReadOnly(Drums), CloneMatrix(currentSteps));
    }

    private static bool[,] CloneMatrix(bool[,] src)
    {
        var r = src.GetLength(0);
        var c = src.GetLength(1);
        var dst = new bool[r, c];
        Array.Copy(src, dst, src.Length);
        return dst;
    }

    public readonly Drum[] Drums = new[]
    {
        Drum.Kick,
        Drum.Snare,
        Drum.HiHat,
        Drum.Tom1,
        Drum.Tom2,
        Drum.FloorTom,
        Drum.Ride,
        Drum.Crash1
    };
}