using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls.Shapes;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DrumBuddy.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static DrumBuddy.Services.DrawHelper;

namespace DrumBuddy.ViewModels.HelperViewModels;

public partial class RythmicGroupViewModel : ReactiveObject
{
    public RythmicGroupViewModel(RythmicGroup rg)
    {
        RythmicGroup = rg;
        DrawNotes();
    }

    [Reactive] private RythmicGroup _rythmicGroup;

    public ObservableCollection<NoteImageAndBounds> NotesImageAndBoundsList { get; } = new();
    public ObservableCollection<Line> Lines { get; } = new();

    private double GetDisplacementForNoteValue(NoteValue beat)
    {
        return beat switch
        {
            NoteValue.Quarter => NoteGroupWidth * 4,
            NoteValue.Eighth => NoteGroupWidth * 2,
            NoteValue.Sixteenth => NoteGroupWidth
        };
    }

    private void DrawNotes()
    {
        if (!_rythmicGroup.IsEmpty)
        {
            double x = 0;
            var noteGroups = _rythmicGroup.NoteGroups;
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
                        NotesImageAndBoundsList.Add(restImage);
                        x += GetDisplacementForNoteValue(NoteValue.Eighth);
                    }
                    else if (i != 0)
                    {
                        var restPathAndSize = GetSingleRestImagePathAndSize(NoteValue.Sixteenth);
                        var point = new Point(x, GetPositionForDrum(Drum.Rest));
                        var restImage = new NoteImageAndBounds(restPathAndSize.Path,
                            new Rect(point, restPathAndSize.ImageSize));
                        NotesImageAndBoundsList.Add(restImage);
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
                    NotesImageAndBoundsList.Add(noteImage);
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
            NotesImageAndBoundsList.Add(restImage);
        }
    }
}