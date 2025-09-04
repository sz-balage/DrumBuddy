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

namespace DrumBuddy.Client.ViewModels;

public sealed record ManualSheetDraft(IReadOnlyList<Drum> Drums, bool[,] Steps);

public sealed class ManualViewModel : ReactiveObject, IRoutableViewModel
{
    public const int Columns = 16; // one measure, 16 sixteenth steps

    private readonly Drum[] _drums;
    private readonly bool[,] _steps; // [row, col]
    private ManualSheetDraft _draft;
    private Sheet? _currentSheet;

    public Sheet? CurrentSheet
    {
        get => _currentSheet;
        private set => this.RaiseAndSetIfChanged(ref _currentSheet, value);
    }

    public readonly ReadOnlyObservableCollection<MeasureViewModel> Measures;
    private readonly SourceList<MeasureViewModel> _measureSource = new();
    public ManualViewModel(IScreen host, IEnumerable<Drum>? drums = null)
    {
        HostScreen = host;
        UrlPathSegment = "manual-editor";
        _measureSource.Connect()
            .Bind(out Measures)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
        _drums = (drums ?? DefaultDrums).ToArray();
        _steps = new bool[_drums.Length, Columns];
        _draft = BuildDraft();
        CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
        
    }

    public IReadOnlyList<Drum> Drums => Array.AsReadOnly(_drums);

    public ManualSheetDraft Draft
    {
        get => _draft;
        private set => this.RaiseAndSetIfChanged(ref _draft, value);
    }

    public IScreen HostScreen { get; }
    public string? UrlPathSegment { get; }

    public void ToggleStep(int row, int col)
    {
        //TODO: consider only swapping out changed measures, not whole sheet
        if (row < 0 || row >= _drums.Length) return;
        if (col < 0 || col >= Columns) return;

        _steps[row, col] = !_steps[row, col];
        Draft = BuildDraft();

        CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
        DrawMeasures();
    }

    public bool GetStep(int row, int col)
    {
        if (row < 0 || row >= _drums.Length) return false;
        if (col < 0 || col >= Columns) return false;
        return _steps[row, col];
    }

    public void LoadMatrix(bool[,] matrix)
    {
        if (matrix.GetLength(0) != _drums.Length || matrix.GetLength(1) != Columns)
            throw new ArgumentException("Matrix size must be [drums x 16].");

        Array.Clear(_steps, 0, _steps.Length);
        Array.Copy(matrix, _steps, matrix.Length);

        Draft = BuildDraft();
        CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
    }

    public void LoadSheet(Sheet sheet)
    {
        Array.Clear(_steps, 0, _steps.Length);

        if (sheet is null || sheet.Measures.Length == 0)
        {
            Draft = BuildDraft();
            CurrentSheet = BuildSheet(new Bpm(120), "Untitled", "");
            return;
        }

        // populate _steps from first measure (same as before) ...
        // [omitted here for brevity]

        Draft = BuildDraft();
        CurrentSheet = BuildSheet(sheet.Tempo, sheet.Name, sheet.Description);
    }

    private Sheet BuildSheet(Bpm tempo, string name, string description)
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

                for (var r = 0; r < _drums.Length; r++)
                    if (_steps[r, col])
                        notes.Add(new Note(_drums[r], NoteValue.Sixteenth));

                noteGroups.Add(notes.Count > 0 ? new NoteGroup(notes) : new NoteGroup());
            }

            var upscaled = RecordingService.UpscaleNotes(noteGroups);

            groups.Add(new RythmicGroup(upscaled.ToImmutableArray()));
        }

        var measure = new Measure(groups);
        return new Sheet(tempo, ImmutableArray.Create(measure), name, description);
    }

    private void DrawMeasures()
    {
        _measureSource.Clear();
        _measureSource.AddRange(CurrentSheet?.Measures.Select(m => new MeasureViewModel(m)) ?? Array.Empty<MeasureViewModel>());
    }

    private ManualSheetDraft BuildDraft()
    {
        return new ManualSheetDraft(Array.AsReadOnly(_drums), CloneMatrix(_steps));
    }

    private static bool[,] CloneMatrix(bool[,] src)
    {
        var r = src.GetLength(0);
        var c = src.GetLength(1);
        var dst = new bool[r, c];
        Array.Copy(src, dst, src.Length);
        return dst;
    }

    private static readonly Drum[] DefaultDrums = new[]
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