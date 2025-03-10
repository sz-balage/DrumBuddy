using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;
using static DrumBuddy.Core.Helpers.CollectionInitializers;
[assembly: InternalsVisibleTo("DrumBuddy.Core.Unit")]

namespace DrumBuddy.Core.Services;

public static class RecordingService
{
    /// <summary>
    ///     Returns a sequence of metronome beeps, and starts the stopwatch.
    /// </summary>
    /// <returns>The index of the current beep from 0-3, resetting on each measure.</returns>
    public static IObservable<long> GetMetronomeBeeping(Bpm bpm)
    {
        return Observable.Interval(bpm.QuarterNoteDuration())
            .Select(i => i % 4)
            .Publish()
            .AutoConnect(2);
    }

    public static IObservable<IList<Note>> GetNotes(Bpm bpm, IObservable<Drum> beats)
    {
        return beats.Select(b => new Note(b, NoteValue.Sixteenth))
            .Buffer(bpm.SixteenthNoteDuration())
            .Select(notes =>
                notes.Count == 0 ? CreateList(new Note(Drum.Rest, NoteValue.Sixteenth)) : notes);
    }

    /// <summary>
    ///     Upscales notes to the next higher note value if possible.
    /// </summary>
    /// <param name="noteGroups">Note groups to upscale.</param>
    /// <returns>Upscaled note groups.</returns>
    public static List<NoteGroup> UpscaleNotes(List<NoteGroup> noteGroups)
    {
        if (noteGroups == null! || !noteGroups.Any())
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
                    result.Add([new Note(Drum.Rest, NoteValue.Quarter)]);
                    position += 4;
                    // Handle any remaining rests
                    consecutiveRests -= 4;
                }
                else if (consecutiveRests >= 2)
                {
                    result.Add([new Note(Drum.Rest, NoteValue.Eighth)]);
                    position += 2;
                    consecutiveRests -= 2;
                }

                // Add any remaining single rests
                while (consecutiveRests > 0)
                {
                    result.Add([new Note(Drum.Rest, NoteValue.Sixteenth)]);
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
                // Use ChangeValues to update all notes in the group
                result.Add(noteValue != NoteValue.Sixteenth
                    ? noteGroups[position].ChangeValues(noteValue)
                    // If no upscaling, add the original group
                    : new NoteGroup(noteGroups[position]));

                // Advance position past this note group
                position++;

                // Advance past the consumed rests
                position += restsToConsume;

                // If we had exactly 2 rests but only consumed 1 for the eighth note,
                // add the remaining rest as sixteenth
                if (followingRests == 2 && restsToConsume == 1)
                {
                    result.Add([new Note(Drum.Rest, NoteValue.Sixteenth)]);
                    position++;
                }
            }

        return result;
    }
}