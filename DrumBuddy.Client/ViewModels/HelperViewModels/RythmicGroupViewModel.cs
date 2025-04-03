using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.Services;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

[assembly: InternalsVisibleTo("DrumBuddy.Client.Unit")]
[assembly: InternalsVisibleTo("DrumBuddy.Benchmark")]

namespace DrumBuddy.Client.ViewModels.HelperViewModels;

public partial class RythmicGroupViewModel : ReactiveObject
{
    [Reactive] private RythmicGroup _rythmicGroup;
    [Reactive] private double _width;
    private  NoteDrawHelper _drawHelper;

    public RythmicGroupViewModel(RythmicGroup rg, double hostScreenWidth, double hostScreenHeight)
    {
        //TODO: look at buggy note rendering when editing
        Width = hostScreenWidth / 4;
        RythmicGroup = rg;
        _drawHelper = new NoteDrawHelper(Width, hostScreenHeight);
        this.WhenAnyValue(x => x.RythmicGroup)
            .Subscribe(rythmicGroup =>
            {
                _drawHelper = new NoteDrawHelper(Width, hostScreenHeight);
                var hihatNote = new Note(Drum.HiHat, NoteValue.Sixteenth);
                var snareNote = new Note(Drum.Snare, NoteValue.Sixteenth);
                var kickNote = new Note(Drum.Kick, NoteValue.Sixteenth);
                var tom1Note = new Note(Drum.Tom1, NoteValue.Sixteenth);
                var tom2Note = new Note(Drum.Tom2, NoteValue.Sixteenth);
                var sixteenthRest = new Note(Drum.Rest, NoteValue.Sixteenth);
                var eighthRest = new Note(Drum.Rest, NoteValue.Eighth);
                var quarterRest = new Note(Drum.Rest, NoteValue.Quarter);
                List<NoteGroup> noteGroups =
                [
                    //new([hihatNote with{Value = NoteValue.Eighth}]),
                    // new([hihatNote]),
                    // new([hihatNote]),
                    //new([eighthRest]),
                    new([snareNote with{Value = NoteValue.Eighth}]),
                    new([sixteenthRest]),
                    new([tom1Note])
                    //new([hihatNote with{Value = NoteValue.Eighth}]),
                    //new([hihatNote with {Value = NoteValue.Eighth}]),
                    // new([sixteenthRest]),
                    //
                    // new([hihatNote with{Value = NoteValue.Eighth}]),
                    // new([sixteenthRest])
                    //new([hihatNote with {Value = NoteValue.Eighth}, tom1Note with {Value = NoteValue.Eighth}, tom2Note with {Value = NoteValue.Eighth}]),
                ];
                var testRythmicGroup = new RythmicGroup([..noteGroups]);
                DrawNotes(rythmicGroup); //modify to rythmicGroup
            });
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