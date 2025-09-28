using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using DrumBuddy.Core.Models;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

[assembly: InternalsVisibleTo("DrumBuddy.Unit")]
[assembly: InternalsVisibleTo("DrumBuddy.Benchmark")]

namespace DrumBuddy.ViewModels.HelperViewModels;

public partial class RythmicGroupViewModel : ReactiveObject
{
    private readonly NoteDrawHelper _drawHelper;
    [Reactive] private RythmicGroup _rythmicGroup;
    [Reactive] private double _width;

    public RythmicGroupViewModel(RythmicGroup rg, double hostScreenWidth, double hostScreenHeight)
    {
        Width = hostScreenWidth / 4;
        RythmicGroup = rg;
        _drawHelper = new NoteDrawHelper(Width, hostScreenHeight);
        this.WhenAnyValue(x => x.RythmicGroup)
            .Subscribe(DrawNotes);
    }

    public ObservableCollection<LineAndStroke> LinesCollection { get; } = new();
    public ObservableCollection<NoteImageAndBounds> NotesImageAndBoundsList { get; } = new();

    private void DrawNotes(RythmicGroup rythmicGroup)
    {
        var data = _drawHelper.GetLinesAndImagesToDraw(rythmicGroup);
        LinesCollection.Add(data.LineAndStrokes);
        NotesImageAndBoundsList.AddRange(data.Images);
    }
}