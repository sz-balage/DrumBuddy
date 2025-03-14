using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls.Shapes;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DrumBuddy.Models;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static DrumBuddy.Services.DrawHelper;

namespace DrumBuddy.ViewModels.HelperViewModels;

public partial class RythmicGroupViewModel : ReactiveObject
{
    public RythmicGroupViewModel(RythmicGroup rg)
    {
        RythmicGroup = rg;
        this.WhenAnyValue(x => x.RythmicGroup)
            .Subscribe(rythmicGroup =>
            {
                DrawNotes(() => TestData(rythmicGroup));
                //DrawNotes(() => GenerateLinesAndNoteImages(rythmicGroup));
            });
    }

    [Reactive] private RythmicGroup _rythmicGroup;

    public ObservableCollection<NoteImageAndBounds> NotesImageAndBoundsList { get; } = new();
    public ObservableCollection<Line> Lines { get; } = new();

    private static double GetDisplacementForNoteValue(NoteValue beat) => beat switch
    {
        NoteValue.Quarter => NoteGroupWidth * 4,
        NoteValue.Eighth => NoteGroupWidth * 2,
        _ => NoteGroupWidth
    };

    private void DrawNotes(Func<(ImmutableArray<Line> Lines, ImmutableArray<NoteImageAndBounds> Images)> generate)
    {
        var result = generate();
        Lines.AddRange(result.Lines);
        NotesImageAndBoundsList.AddRange(result.Images);
    }
    private static (ImmutableArray<Line> Lines, ImmutableArray<NoteImageAndBounds> Images) GenerateLinesAndNoteImages(RythmicGroup rythmicGroup)
    {
        var linesBuilder = new List<Line>();
        var imagesBuilder = new List<NoteImageAndBounds>();
        if (!rythmicGroup.IsEmpty)
        {
            double x = 0;
            var noteGroups = rythmicGroup.NoteGroups;
            for (var i = 0; i < noteGroups.Length; i++)
            {
                var noteGroup = noteGroups[i];
                //if rest -> eighth rest can only be in first position
                if (noteGroup.IsRest)
                {
                    x += GetDisplacementForNoteValue(noteGroup.Value);
                    continue;
                    //sixteenth rest -> x position is the same as the previous note (if it is the first then of course ignore this) + couple of pixels -> draw a point
                    if(noteGroup.Value == NoteValue.Eighth)
                    {
                        var restPathAndSize = GetSingleRestImagePathAndSize(NoteValue.Eighth);
                        var point = new Point(x, GetPositionForDrum(Drum.Rest));
                        var restImage = new NoteImageAndBounds(restPathAndSize.Path,
                            new Rect(point, restPathAndSize.ImageSize));
                        imagesBuilder.Add(restImage);
                        x += GetDisplacementForNoteValue(NoteValue.Eighth);
                    }
                    else if (i != 0)
                    {
                        var restPathAndSize = GetSingleRestImagePathAndSize(NoteValue.Sixteenth);
                        var point = new Point(x, GetPositionForDrum(Drum.Rest));
                        var restImage = new NoteImageAndBounds(restPathAndSize.Path,
                            new Rect(point, restPathAndSize.ImageSize));
                        imagesBuilder.Add(restImage);
                        x += GetDisplacementForNoteValue(NoteValue.Sixteenth);
                    }
                    else
                    {
                        x += GetDisplacementForNoteValue(NoteValue.Sixteenth);
                    }
                }
                noteGroup.Sort((n1, n2) => GetPositionForDrum(n1.Drum).CompareTo(GetPositionForDrum(n2.Drum)));
                for (var j = 0; j < noteGroup.Count; j++)
                {
                    var note = noteGroup[j];
                    var y = GetPositionForDrum(note.Drum);
                    var point = new Point(x, y);
                    if (j == 1 && note.Drum.IsOneDrumAwayFrom(noteGroup[0].Drum))
                    {
                        //TODO draw with offset and continue
                        point = point.WithX(x + NoteHeadSize.Width);
                    }
                    //TODO draw without offset
                    var noteHeadPathAndSize = note.Drum.NoteHeadImagePathAndSize();
                    var noteImage = new NoteImageAndBounds(noteHeadPathAndSize.Path,
                        new Rect(point, noteHeadPathAndSize.ImageSize));
                    imagesBuilder.Add(noteImage);
                }
                x += GetDisplacementForNoteValue(noteGroup.Value);
            }
        }
        else
        {
            //quarter rest 
            var quarterRestPathAndSize = GetSingleRestImagePathAndSize(NoteValue.Quarter);
            var point = new Point(0, GetPositionForDrum(Drum.Rest));
            var restImage = new NoteImageAndBounds(quarterRestPathAndSize.Path,
                new Rect(point, quarterRestPathAndSize.ImageSize));
            imagesBuilder.Add(restImage);
        }
        return ([..linesBuilder], [..imagesBuilder]);
    }

    private static (ImmutableArray<Line> Lines, ImmutableArray<NoteImageAndBounds> Images) TestData(RythmicGroup rythmicGroup)
    {
        var lines = new List<Line>();
        var images = new List<NoteImageAndBounds>();
        var hihatNote = new Note(Drum.HiHat, NoteValue.Sixteenth);
        var kickNote = new Note(Drum.Kick, NoteValue.Quarter);
        List<NoteGroup> noteGroups =
        [
            new([kickNote, hihatNote]),
            new([hihatNote]),
            new([hihatNote]),
            new([hihatNote])
        ];
        
        double x = 0;
        var y = GetPositionForDrum(kickNote.Drum);
        
        images.Add(new NoteImageAndBounds(kickNote.Drum.NoteHeadImagePathAndSize().Path,
            new Rect(new Point(x, GetPositionForDrum(kickNote.Drum)), kickNote.Drum.NoteHeadImagePathAndSize().ImageSize)));
        
        images.Add(new(hihatNote.Drum.NoteHeadImagePathAndSize().Path,
            new Rect(new Point(x, GetPositionForDrum(hihatNote.Drum)), hihatNote.Drum.NoteHeadImagePathAndSize().ImageSize)));
        x = 31.25;
        images.Add(new NoteImageAndBounds(hihatNote.Drum.NoteHeadImagePathAndSize().Path,
            new Rect(new Point(x, GetPositionForDrum(hihatNote.Drum)), hihatNote.Drum.NoteHeadImagePathAndSize().ImageSize)));
        x = 62.5;
        images.Add(new NoteImageAndBounds(hihatNote.Drum.NoteHeadImagePathAndSize().Path,
            new Rect(new Point(x, GetPositionForDrum(hihatNote.Drum)), hihatNote.Drum.NoteHeadImagePathAndSize().ImageSize)));
        x = 93.75;
        images.Add(new NoteImageAndBounds(hihatNote.Drum.NoteHeadImagePathAndSize().Path,
            new Rect(new Point(x, GetPositionForDrum(hihatNote.Drum)), hihatNote.Drum.NoteHeadImagePathAndSize().ImageSize)));
        return ([..lines], [..images]);
    }
}