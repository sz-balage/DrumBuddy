using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Shapes;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
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
            NoteValue.Quarter => 4 * SectionWidth,
            NoteValue.Eighth => 2 * SectionWidth,
            NoteValue.Sixteenth => SectionWidth
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
                // if (noteGroup.IsRest || noteGroup.Single().Value == NoteValue.Sixteenth)
                // {
                //     //sixteenth rest -> x position is the same as the previous note (if it is the first then of course ignore this) + couple of pixels -> draw a point
                //     
                // }
                noteGroup.Sort((n1, n2) => GetPositionForDrum(n1.Drum).CompareTo(GetPositionForDrum(n2.Drum)));
                for (var j = 0; j < noteGroups[i].Count; j++)
                {
                    var note = noteGroups[i][j];
                    var y = GetPositionForDrum(note.Drum);
                    if (j == 1 && note.Drum.IsOneDrumAwayFrom(noteGroups[i][0].Drum))
                    {
                        //TODO draw with offset and continue
                    }
                    //TODO draw without offset
                }

                x += GetDisplacementForNoteValue(noteGroup.Value);
            }
        }
        else
        {
            //quarter rest 
            var quarterRestPathAndSize = GetSingleRestImagePathAndSize(NoteValue.Quarter);
            var point = new Point(0, 0);
            var restImage = new NoteImageAndBounds(quarterRestPathAndSize.Path,
                new Rect(point, quarterRestPathAndSize.ImageSize));
            NotesImageAndBoundsList.Add(restImage);
            // var line = new Line() { StartPoint = new Point(62.5, 20), EndPoint = new Point(62.5, 100), StrokeThickness = 2, Stroke = Brushes.Black};
            // Lines.Add(line);
        }
    }
}