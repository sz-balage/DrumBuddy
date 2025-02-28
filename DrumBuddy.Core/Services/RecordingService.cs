using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;
using LanguageExt;

[assembly: InternalsVisibleTo("DrumBuddy.Core.Unit")]

namespace DrumBuddy.Core.Services;

public static class RecordingService
{
    /// <summary>
    ///     Returns a sequence of metronome beeps, and starts the stopwatch.
    /// </summary>
    /// <returns>The index of the current beep from 0-3, resetting on each measure.</returns>
    public static IObservable<long> GetMetronomeBeeping(BPM bpm)
    {
        return Observable.Interval(bpm.QuarterNoteDuration())
            .Select(i => i % 4)
            .Publish()
            .AutoConnect(2);
    }

    public static IObservable<IList<Note>> GetNotes(BPM bpm, IObservable<Beat> Beats)
    {
        return Beats.Select(b => new Note(b, NoteValue.Sixteenth))
            .Buffer(bpm.SixteenthNoteDuration())
            .Select(notes =>
                notes.Count == 0 ? Prelude.List(new Note(Beat.Rest, NoteValue.Sixteenth)).ToList() : notes);
    }

    /// <summary>
    ///     Upscales notes to the next higher note value if possible.
    /// </summary>
    /// <param name="noteGroups">Note groups to upscale.</param>
    /// <returns>Upscaled note groups.</returns>
    public static List<NoteGroup> UpscaleNotes(List<NoteGroup> noteGroups)
    {
        if (noteGroups == null || !noteGroups.Any())
            return new List<NoteGroup>();

        var result = new List<NoteGroup>();
        var position = 0;

        while (position < noteGroups.Count)
            // Handle rest sequences
            if (noteGroups[position].IsRest)
            {
                var consecutiveRests = 1;
                while (position + consecutiveRests < noteGroups.Count &&
                       noteGroups[position + consecutiveRests].IsRest)
                    consecutiveRests++;

                // Create appropriate rest notes based on the count
                if (consecutiveRests >= 4)
                {
                    result.Add(new NoteGroup { new Note(Beat.Rest, NoteValue.Quarter) });
                    position += 4;
                    // Handle any remaining rests
                    consecutiveRests -= 4;
                }
                else if (consecutiveRests >= 2)
                {
                    result.Add(new NoteGroup { new Note(Beat.Rest, NoteValue.Eighth) });
                    position += 2;
                    consecutiveRests -= 2;
                }

                // Add any remaining single rests
                while (consecutiveRests > 0)
                {
                    result.Add(new NoteGroup { new Note(Beat.Rest, NoteValue.Sixteenth) });
                    position++;
                    consecutiveRests--;
                }
            }
            // Handle non-rest note groups
            else
            {
                // Count following rests (up to 3 at most)
                var followingRests = 0;
                for (var i = position + 1; i < noteGroups.Count && i < position + 4; i++)
                    if (noteGroups[i].IsRest)
                        followingRests++;
                    else
                        break;

                // Determine note value based on following rests
                NoteValue noteValue;
                int restsToConsume;

                switch (followingRests)
                {
                    case 3: // Note followed by 3 rests = quarter note
                        noteValue = NoteValue.Quarter;
                        restsToConsume = 3;
                        break;
                    case 1: // Note followed by 1 rest = eighth note
                        noteValue = NoteValue.Eighth;
                        restsToConsume = 1;
                        break;
                    case 2: // Note followed by 2 rests = eighth note + sixteenth
                        noteValue = NoteValue.Eighth;
                        restsToConsume = 1; // Only consume one rest for eighth note
                        break;
                    default: // No following rests = sixteenth note
                        noteValue = NoteValue.Sixteenth;
                        restsToConsume = 0;
                        break;
                }

                // Add the note group with upscaled value if applicable
                if (noteValue != NoteValue.Sixteenth)
                    // Use ChangeValues to update all notes in the group
                    result.Add(noteGroups[position].ChangeValues(noteValue));
                else
                    // If no upscaling, add the original group
                    result.Add(new NoteGroup(noteGroups[position]));

                // Advance position past this note group
                position++;

                // Advance past the consumed rests
                position += restsToConsume;

                // If we had exactly 2 rests but only consumed 1 for the eighth note,
                // add the remaining rest as sixteenth
                if (followingRests == 2 && restsToConsume == 1)
                {
                    result.Add(new NoteGroup { new Note(Beat.Rest, NoteValue.Sixteenth) });
                    position++;
                }
            }

        return result;
    }

    /// <summary>
    ///     Checks if the beats at all specified positions are identical
    /// </summary>
    private static bool AreBeatsIdentical(Dictionary<int, List<Beat>> groupedBeats, params int[] positions)
    {
        if (positions.Length <= 1)
            return true;

        // Check if all positions have exactly one beat and they're all the same
        if (positions.Any(p => !groupedBeats.ContainsKey(p) || groupedBeats[p].Count != 1))
            return false;

        var firstBeat = groupedBeats[positions[0]][0];
        return positions.All(p => groupedBeats[p][0] == firstBeat);
    }
}