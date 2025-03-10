using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Helpers.Services;
using DrumBuddy.IO.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static DrumBuddy.Helpers.Services.DrawHelper;
namespace DrumBuddy.ViewModels.HelperViewModels;

public partial class RythmicGroupViewModel : ReactiveObject
{
    private const double SectionWidth = 31.25; //125/4
    private const double NoteRadius = 10.0;
    private const double RestLineHeight = 20.0;

    public RythmicGroupViewModel(RythmicGroup rg)
    {
        RythmicGroup = rg;
        DrawNotes();
    }

    [Reactive] private RythmicGroup _rythmicGroup;

    public ObservableCollection<Geometry> Geometries { get; } = new();
    
    private double GetDisplacementForNoteValue(NoteValue beat) => beat switch
    {
        NoteValue.Quarter => 4 * SectionWidth,
        NoteValue.Eighth => 2 * SectionWidth,
        NoteValue.Sixteenth => SectionWidth
    };
    private void DrawNotes()
    {
        if (!_rythmicGroup.IsEmpty)
        {
            double x = 0;
            var noteGroups = _rythmicGroup.NoteGroups;
            for (int i = 0; i < noteGroups.Length; i++)
            {
                var noteGroup = noteGroups[i];
                //if rest -> eighth rest can only be in first position
                // if (noteGroup.IsRest || noteGroup.Single().Value == NoteValue.Sixteenth)
                // {
                //     //sixteenth rest -> x position is the same as the previous note (if it is the first then of course ignore this) + couple of pixels -> draw a point
                //     
                // }
                noteGroup.Sort((n1, n2) => GetPositionForDrum(n1.Drum).CompareTo(GetPositionForDrum(n2.Drum))); 
                for (int j = 0; j < noteGroups[i].Count; j++)
                {
                    var note = noteGroups[i][j];
                    var y = GetPositionForDrum(note.Drum);
                    if (j == 1 && note.Drum.IsOneDrumAwayFrom(noteGroups[i][0].Drum))
                    {
                        //TODO draw with offset and continue
                        continue;
                    }
                    //TODO draw without offset
                }
                x += GetDisplacementForNoteValue(noteGroup.Value);
            }
        }
        else
        {
            //quarter rest 
            var line = new LineGeometry(new Point(62.5, 20), new Point(62.5, 100));
            Geometries.Add(line);
        }
    }
}