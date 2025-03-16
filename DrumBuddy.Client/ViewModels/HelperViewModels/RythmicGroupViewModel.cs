using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.Shapes;
using DrumBuddy.Client.Models;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static DrumBuddy.Client.Services.DrawHelper;

[assembly: InternalsVisibleTo("DrumBuddy.Client.Unit")]
[assembly: InternalsVisibleTo("DrumBuddy.Benchmark")]
namespace DrumBuddy.Client.ViewModels.HelperViewModels;
public partial class RythmicGroupViewModel : ReactiveObject
{
    [Reactive] private RythmicGroup _rythmicGroup;
    [Reactive] private int _width;

    public RythmicGroupViewModel(RythmicGroup rg, int hostScreenWidth)
    {
        Width = hostScreenWidth / 4;
        RythmicGroup = rg;
        this.WhenAnyValue(x => x.RythmicGroup)
            .Subscribe(rythmicGroup =>
            {
                var
                    startingXPos =
                        -1 * (Width / 2) +
                        20; //since 0 is middle, we need to set it to left by decrementing it by half the width, and then add a minimal offset so it is not on the edge
                var noteGroupWidth = Width / 4;

                var hihatNote = new Note(Drum.HiHat, NoteValue.Sixteenth);
                var kickNote = new Note(Drum.Kick, NoteValue.Sixteenth);
                var tom1Note = new Note(Drum.Tom1, NoteValue.Sixteenth);
                var tom2Note = new Note(Drum.Tom2, NoteValue.Sixteenth);
                var sixteenthRest = new Note(Drum.Rest, NoteValue.Sixteenth);
                var eighthRest = new Note(Drum.Rest, NoteValue.Eighth);
                var quarterRest = new Note(Drum.Rest, NoteValue.Quarter);
                List<NoteGroup> noteGroups =
                [
                    new([quarterRest]),
                    // new([sixteenthRest]),
                    // // new([hihatNote]),
                    // new([hihatNote])
                ];
                var testRythmicGroup = new RythmicGroup([..noteGroups]);
                Func<Note, Point, NoteImageAndBounds> getNoteImage = (note, point) =>
                {
                    var noteHeadPathAndSize = note.NoteHeadImagePathAndSize();
                    var noteImage = new NoteImageAndBounds(noteHeadPathAndSize.Path,
                         new Rect(point, noteHeadPathAndSize.ImageSize));
                    return noteImage;
                };
                Func<Point, NoteImageAndBounds> getCircleImage = point =>
                {
                    var pathAndSize = GetCircleImagePathAndSize();
                    var circleImage = new NoteImageAndBounds(pathAndSize.Path,
                        new Rect(point, pathAndSize.ImageSize));
                    return circleImage;
                };
                DrawNotes(() => GenerateLinesAndNoteImages(getNoteImage, getCircleImage, testRythmicGroup, noteGroupWidth, startingXPos));
                //DrawNotes(() => GenerateLinesAndNoteImages(rythmicGroup,noteGroupWidth, -1 * (noteGroupWidth/2)));
            });
    }

    public ObservableCollection<NoteImageAndBounds> NotesImageAndBoundsList { get; } = new();
    public ObservableCollection<Line> Lines { get; } = new();

    private static double GetDisplacementForNoteValue(NoteValue beat, int noteGroupWidth)
    {
        return beat switch
        {
            NoteValue.Quarter => noteGroupWidth * 4,
            NoteValue.Eighth => noteGroupWidth * 2,
            _ => noteGroupWidth
        };
    }

    private void DrawNotes(Func<(ImmutableArray<Line> Lines, ImmutableArray<NoteImageAndBounds> Images)> generate)
    {
        var result = generate();
        Lines.AddRange(result.Lines);
        NotesImageAndBoundsList.AddRange(result.Images);
    }
    /// <summary>
    /// Generates lines and note images for the given rythmic group.
    /// </summary>
    /// <param name="getCircleImage">Function for creating a circle image.</param>
    /// <param name="rythmicGroup">Containing the note groups to draw.</param>
    /// <param name="noteGroupWidth">Used for calculating displacement for each notegroup.</param>
    /// <param name="startingXPosition">Determines the position of the first notegroup horizontally.</param>
    /// <param name="getNoteImage">Function for creating a note image</param>
    /// <returns>The lines and images to draw.</returns>
    internal static (ImmutableArray<Line> Lines, ImmutableArray<NoteImageAndBounds> Images) GenerateLinesAndNoteImages(
        Func<Note, Point, NoteImageAndBounds> getNoteImage,
        Func<Point, NoteImageAndBounds> getCircleImage,
        RythmicGroup rythmicGroup,
        int noteGroupWidth,
        int startingXPosition)
    {
        var lines = new List<Line>(); //TODO: draw lines
        var images = new List<NoteImageAndBounds>();
        var noteGroups = rythmicGroup.NoteGroups;
        double x = startingXPosition;
        for (var i = 0; i < noteGroups.Length; i++)
        {
            var noteGroup = noteGroups[i];
            noteGroup.Sort((n1, n2) => GetYPositionForDrum(n2.Drum).CompareTo(GetYPositionForDrum(n1.Drum)));
            if (noteGroup is { IsRest: true, Value: NoteValue.Sixteenth } && i != 0)
            {
                var previousX = images[i - 1].Bounds.X;
                var previousY = images[i - 1].Bounds.Y;
                var point = new Point(previousX + 20, previousY);
                var circleImage = getCircleImage(point);
                images.Add(circleImage);
                continue;
            }

            for (var j = 0; j < noteGroup.Count; j++)
            {
                var note = noteGroup[j];
                var y = GetYPositionForDrum(note.Drum);
                var point = new Point(x, y);
                if (j == 1 && note.Drum.IsOneDrumAwayFrom(noteGroup[0].Drum))
                    point = point.WithX(x + NoteHeadSize.Width);
                var noteImage = getNoteImage(note, point);
                images.Add(noteImage);
            }

            x += GetDisplacementForNoteValue(noteGroup.Value, noteGroupWidth);
        }

        return ([..lines], [..images]);
    }
}