using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.Extensions;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels.HelperViewModels;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace DrumBuddy.ViewModels;

public partial class ManualEditorViewModel : ReactiveObject, IRoutableViewModel
{
    //TODO: make auto save checkbox
    public const int Columns = 16; //one measure, 16 sixteenth steps
    public const int MaxNotesPerColumn = 4; 
    private readonly SourceList<MeasureViewModel> _measureSource = new();
    private readonly List<bool[,]> _measureSteps;
    private readonly NotificationService _notificationService;
    private readonly Func<Task> _onClose;
    private readonly SheetService _sheetService;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);

    public readonly Drum[] Drums = new[]
    {
        Drum.Kick,
        Drum.Snare,
        Drum.HiHat,
        Drum.HiHat_Open,
        Drum.HiHat_Pedal,
        Drum.Tom1,
        Drum.Tom2,
        Drum.FloorTom,
        Drum.Ride,
        Drum.Crash1,
        Drum.Crash2
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
    private Guid _sheetId;

    public ManualEditorViewModel(IScreen host, SheetService sheetService,
        Func<Task> onClose)
    {
        _sheetService = sheetService;
        _notificationService = Locator.Current.GetRequiredService<NotificationService>("MainWindowNotificationService");
        HostScreen = host;
        UrlPathSegment = "manual-editor";
        _sheetId = Guid.NewGuid();
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
                IsSaved = false;
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
        BuildMatrixRowsFromCurrentMeasure();

        this.WhenAnyValue(vm => vm.CurrentMeasureIndex)
            .Subscribe(_ => BuildMatrixRowsFromCurrentMeasure());
        DrawSheet();
        SaveCommand.ThrownExceptions.Subscribe(ex => Console.WriteLine(ex.Message));
        _onClose = onClose;
        IsSaved = true;
    }

    public ObservableCollection<RowViewModel> MatrixRows { get; } = new();

    public ReadOnlyCollection<int> StepHeaders { get; } =
        Enumerable.Range(1, Columns).ToList().AsReadOnly();

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

    private Unit ToggleCellInternal(int row, int col)
    {
        if (row < 0 || row >= Drums.Length) return Unit.Default;
        if (col < 0 || col >= Columns) return Unit.Default;
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return Unit.Default;

        IsSaved = false;
        var matrix = _measureSteps[CurrentMeasureIndex];
        var current = matrix[row, col];

        var hatDrums = new[] { Drum.HiHat, Drum.HiHat_Open, Drum.HiHat_Pedal };
        var hatIndices = hatDrums
            .Select(d => Array.IndexOf(Drums, d))
            .Where(i => i >= 0)
            .ToArray();
        var isHatRow = hatIndices.Contains(row);

        if (!current)
        {
            var currentCount = 0;
            for (var r = 0; r < Drums.Length; r++)
                if (matrix[r, col])
                    currentCount++;

            if (isHatRow)
            {
                var otherHatsChecked = hatIndices.Count(i => i != row && matrix[i, col]);
                var prospective = currentCount - otherHatsChecked + 1;
                if (prospective > MaxNotesPerColumn) return Unit.Default;

                foreach (var i in hatIndices)
                    if (i != row)
                        matrix[i, col] = false;

                matrix[row, col] = true;
            }
            else
            {
                if (currentCount >= MaxNotesPerColumn) return Unit.Default;
                matrix[row, col] = true;
            }
        }
        else
        {
            matrix[row, col] = false;
        }

        CurrentSheet = BuildSheet();
        SyncCellsFromMeasure();
        RedrawMeasureAt();
        return Unit.Default;
    }

    /// <summary>
    ///     Create/refresh MatrixRows based on _measureSteps[CurrentMeasureIndex]
    ///     Called when measure selection changes or when reloading a sheet
    /// </summary>
    private void BuildMatrixRowsFromCurrentMeasure()
    {
        MatrixRows.Clear();

        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count)
        {
            for (var r = 0; r < Drums.Length; r++)
            {
                var rowVm = new RowViewModel(Drums[r]);
                for (var c = 0; c < Columns; c++)
                {
                    var cellVm = new StepCellViewModel(r, c, ToggleCellInternal)
                        { IsChecked = false, IsEnabled = true };
                    rowVm.Cells.Add(cellVm);
                }

                MatrixRows.Add(rowVm);
            }

            return;
        }

        var matrix = _measureSteps[CurrentMeasureIndex];

        for (var r = 0; r < Drums.Length; r++)
        {
            var rowVm = new RowViewModel(Drums[r]);
            for (var c = 0; c < Columns; c++)
            {
                var cellVm = new StepCellViewModel(r, c, ToggleCellInternal)
                {
                    IsChecked = matrix[r, c],
                    IsEnabled = true 
                };
                rowVm.Cells.Add(cellVm);
            }

            MatrixRows.Add(rowVm);
        }

        for (var c = 0; c < Columns; c++)
            UpdateColumnEnabledState(c);
    }

    /// <summary>
    ///     Called after we mutate _measureSteps to sync StepCellViewModels
    ///     Only updates IsChecked / IsEnabled properties (no recreation)
    /// </summary>
    private void SyncCellsFromMeasure()
    {
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return;
        var matrix = _measureSteps[CurrentMeasureIndex];

        for (var r = 0; r < Drums.Length; r++)
        {
            var rowVm = MatrixRows.ElementAtOrDefault(r);
            if (rowVm == null) continue;
            for (var c = 0; c < Columns; c++)
            {
                var cellVm = rowVm.Cells.ElementAtOrDefault(c);
                if (cellVm == null) continue;
                var newChecked = matrix[r, c];
                if (cellVm.IsChecked != newChecked)
                    cellVm.IsChecked = newChecked;
            }
        }

        for (var c = 0; c < Columns; c++)
            UpdateColumnEnabledState(c);
    }

    private void UpdateColumnEnabledState(int col)
    {
        if (col < 0 || col >= Columns) return;
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count) return;

        var count = 0;
        var matrix = _measureSteps[CurrentMeasureIndex];
        for (var r = 0; r < Drums.Length; r++)
            if (matrix[r, col])
                count++;

        var limitReached = count >= MaxNotesPerColumn;

        //if limit reached, disable unchecked cells
        for (var r = 0; r < Drums.Length; r++)
        {
            var rowVm = MatrixRows.ElementAtOrDefault(r);
            if (rowVm == null) continue;
            var cellVm = rowVm.Cells.ElementAtOrDefault(col);
            if (cellVm == null) continue;

            if (cellVm.IsChecked)
                cellVm.IsEnabled = true;
            else
                cellVm.IsEnabled = !limitReached;
        }
    }

    [ReactiveCommand]
    private async Task NavigateBack()
    {
        if (!IsSaved)
        {
            var result = await ShowConfirmation.Handle(Unit.Default);
            if (result == Confirmation.Discard)
            {
                IsSaved = true;
            }
            else if (result == Confirmation.Confirm)
            {
                await Save();
            }
            else
            {
                return;
            }
        }

        await _onClose();
    }

    public void MoveSelectedMeasureLeft()
    {
        if (CurrentMeasureIndex <= 0 || CurrentMeasureIndex >= _measureSteps.Count) return;

        var idx = CurrentMeasureIndex;
        (_measureSteps[idx - 1], _measureSteps[idx]) = (_measureSteps[idx], _measureSteps[idx - 1]);

        CurrentMeasureIndex = idx - 1;
        CurrentSheet = BuildSheet();
        BuildMatrixRowsFromCurrentMeasure();
        DrawSheet();
        IsSaved = false;
    }

    public void MoveSelectedMeasureRight()
    {
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count - 1) return;

        var idx = CurrentMeasureIndex;
        (_measureSteps[idx + 1], _measureSteps[idx]) = (_measureSteps[idx], _measureSteps[idx + 1]);

        CurrentMeasureIndex = idx + 1;
        CurrentSheet = BuildSheet();
        BuildMatrixRowsFromCurrentMeasure();
        DrawSheet();
        IsSaved = false;
    }

    [ReactiveCommand]
    private async Task Save()
    {
        if (IsSaved) return;
        if (Name is null)
        {
            var dialogResult = await ShowSaveDialog.Handle(new SheetCreationData(_bpm,
                [..CurrentSheet?.Measures ?? ImmutableArray<Measure>.Empty]));
            if (dialogResult.Name != null)
            {
                Name = dialogResult.Name;
                Description = dialogResult.Description;
                CurrentSheet = BuildSheet();
                _notificationService.ShowNotification(new Notification("Sheet saved.",
                    $"The sheet {Name} successfully saved.", NotificationType.Success));
                IsSaved = true;
            }
        }
        else
        {
            CurrentSheet = BuildSheet();
            await _sheetService.UpdateSheetAsync(CurrentSheet!);

            _notificationService.ShowNotification(new Notification("Sheet saved.",
                $"The sheet {Name} successfully saved.", NotificationType.Success));
            IsSaved = true;
        }
    }


    [ReactiveCommand]
    private void AddMeasure()
    {
        //TODO: add the measure after the currently selected measure instead
        var newMeasure = new bool[Drums.Length, Columns];
        _measureSteps.Add(newMeasure);
        CurrentMeasureIndex = _measureSteps.Count - 1;
        CurrentSheet = BuildSheet();
        BuildMatrixRowsFromCurrentMeasure();
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

    public void LoadSheet(Sheet sheet)
    {
        _measureSteps.Clear();

        if (sheet is null || sheet.Measures.Length == 0)
        {
            _measureSteps.Add(new bool[Drums.Length, Columns]);
            CurrentMeasureIndex = 0;
            CurrentSheet = BuildSheet();
            BuildMatrixRowsFromCurrentMeasure();
            DrawSheet();
            return;
        }

        foreach (var measure in sheet.Measures)
        {
            var measureMatrix = new bool[Drums.Length, Columns];

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

                col += NoteValueToColumns(noteGroup.Value);
            }

            _measureSteps.Add(measureMatrix);
        }

        _sheetId = sheet.Id;
        CurrentMeasureIndex = 0;
        Name = sheet.Name;
        Description = sheet.Description;
        BpmDecimal = sheet.Tempo.Value;
        CurrentSheet = BuildSheet();
        BuildMatrixRowsFromCurrentMeasure();
        DrawSheet();
        IsSaved = true;
    }

    private int NoteValueToColumns(NoteValue value)
    {
        return value switch
        {
            NoteValue.Quarter => 4,
            NoteValue.Eighth => 2,
            _ => 1
        };
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
        // Use the provided ID, or the current sheet's ID if editing, or generate new if creating

        return new Sheet(_bpm, [..allMeasures], Name ?? "Untitled", Description ?? "", _sheetId);
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
        _measureSource.ReplaceAt(CurrentMeasureIndex, updatedMeasure);

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
        if (idx != 0)
            CurrentMeasureIndex = idx - 1;
        else
            CurrentMeasureIndex = 0;
        IsSaved = false;

        BuildMatrixRowsFromCurrentMeasure();
    }

    public void DuplicateSelectedMeasure()
    {
        if (CurrentMeasureIndex < 0 || CurrentMeasureIndex >= _measureSteps.Count)
            return;

        var current = _measureSteps[CurrentMeasureIndex];
        var copy = new bool[Drums.Length, Columns];
        Array.Copy(current, copy, current.Length);

        _measureSteps.Insert(CurrentMeasureIndex + 1, copy);

        CurrentSheet = BuildSheet();
        DrawSheet();

        CurrentMeasureIndex += 1;
        IsSaved = false;

        BuildMatrixRowsFromCurrentMeasure();
    }
}