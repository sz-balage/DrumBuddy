using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.HelperViewModels;

public partial class MeasureViewModel : ReactiveObject
{
    [Reactive] private double _height = 200;
    [Reactive] private bool _isPointerVisible;
    private double _pointerPosition = -1;

    [Reactive] private double _width = 1200;

    public Measure Measure = new(new List<RythmicGroup>(4));

    public MeasureViewModel(Measure? measure = null)
    {
        if (measure is not null)
        {
            foreach (var rg in measure.Groups)
            {
                Measure.Groups.Add(rg);
                RythmicGroups.Add(new RythmicGroupViewModel(rg, Width, Height));
            }
        }

        IsPointerVisible = true;
    }

    public ObservableCollection<EvaluationBox> EvaluationBoxes { get; } = new();

    public double PointerPosition
    {
        get => _pointerPosition;
        set => this.RaiseAndSetIfChanged(ref _pointerPosition, value);
    }

    public bool IsEmpty => Measure.IsEmpty;
    public ObservableCollection<RythmicGroupViewModel> RythmicGroups { get; private set; } = new();

    public void EvaluateMeasure(List<EvaluationBox> boxesToDraw)
    {
        EvaluationBoxes.Clear();
        foreach (var box in boxesToDraw) EvaluationBoxes.Add(box);
    }

    public void AddRythmicGroupFromNotes(List<NoteGroup> notes, int index)
    {
        var rg = new RythmicGroup([..RecordingService.UpscaleNotes(notes)]);
        if (Measure.Groups.Count >= index + 1)
        {
            Measure.Groups[index] = rg;
            RythmicGroups[index] = new RythmicGroupViewModel(rg, Width, Height);
            return;
        }

        Measure.Groups.Add(rg);
        RythmicGroups.Add(new RythmicGroupViewModel(rg, Width, Height));
    }

    public void MovePointerToRg(long rythmicGroupIndex)
    {
        PointerPosition = rythmicGroupIndex * (Width / 4) + 35;
    }

    public void UpdateFrom(Measure measure)
    {
        RythmicGroups = new ObservableCollection<RythmicGroupViewModel>(
            measure.Groups.Select(g => new RythmicGroupViewModel(g, Width, Height))
        );
    }
}