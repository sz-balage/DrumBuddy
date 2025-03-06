using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using DrumBuddy.Core.Models;
using DrumBuddy.Helpers.Services;
using DrumBuddy.IO.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

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

    private void DrawNotes()
    {
        if (!_rythmicGroup.IsEmpty)
        {
            var noteToDraw = _rythmicGroup.NoteGroups.Where(ng => !ng.IsRest)?.First();
            var yPosition = noteToDraw != null ? DrawHelper.GetPoisitionForBeat(noteToDraw.First().Beat)
                    : 0;
            // Use fixed X position (e.g., 62.5) and variable Y position
            var noteHead = new EllipseGeometry(new Rect(new Point(62.5, yPosition), new Size(20, 20)));
            Geometries.Add(noteHead);
        }
        else
        {
            var line = new LineGeometry(new Point(62.5, 20), new Point(62.5, 100));
            Geometries.Add(line);
        }
        // var noteGroups = RythmicGroup.NoteGroups;
        // for (var i = 0; i < noteGroups.Length && i < 4; i++)
        // {
        //     var xPosition = i * SectionWidth;
        //     var centerX = xPosition + SectionWidth / 2;
        //
        //     var noteGroup = noteGroups[i];
        //
        //     if (noteGroup.IsRest)
        //     {
        //         var line = new LineGeometry(
        //             new Point(centerX, 40),
        //             new Point(centerX, 40 + RestLineHeight));
        //         // Draw a vertical line for rest
        //         Geometries.Add(line);
        //         continue;
        //     }
        //
        //     foreach (var note in noteGroup)
        //         if (note.Beat == Beat.Snare || note.Beat == Beat.Bass)
        //         {
        //             double yPosition = note.Beat == Beat.Snare ? 60 : 30;
        //             var ellipse = new EllipseGeometry(new Rect(
        //                 centerX - NoteRadius,
        //                 yPosition - NoteRadius,
        //                 NoteRadius * 2,
        //                 NoteRadius * 2));
        //             Geometries.Add(ellipse);
        //         }
        // }
    }
}