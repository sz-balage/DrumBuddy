using System.Collections.Immutable;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Note = DrumBuddy.Core.Models.Note;

namespace DrumBuddy.IO.Services;

public static class MidiExporter
{
    public static void ExportSheetToMidi(Sheet sheet, string filePath)
    {
        var midiFile = new MidiFile();
        var trackChunk = new TrackChunk();
        midiFile.Chunks.Add(trackChunk);

        var bpm = sheet.Tempo.Value;
        var microsecondsPerQuarter = (int)(60000000.0 / bpm);
        trackChunk.Events.Add(new SetTempoEvent(microsecondsPerQuarter));

        var tempoMap = TempoMap.Create(new Tempo(microsecondsPerQuarter));

        var notesManager = trackChunk.ManageNotes();
        var notesCollection = notesManager.Objects;

        long currentTick = 0;

        foreach (var measure in sheet.Measures)
        foreach (var rg in measure.Groups)
        {
            if (rg.NoteGroups == null || rg.NoteGroups.Length == 0)
            {
                var quarterTicks = TimeConverter.ConvertFrom(MusicalTimeSpan.Quarter, tempoMap);
                currentTick += quarterTicks;
                continue;
            }

            foreach (var ng in rg.NoteGroups)
            {
                var durationTicks = TimeConverter.ConvertFrom(NoteValueToMusicalTimeSpan(ng.Value), tempoMap);

                if (ng.IsRest)
                {
                    currentTick += durationTicks;
                    continue;
                }

                var groupStartTick = currentTick;

                foreach (var note in ng)
                {
                    var midiNoteNumber = (SevenBitNumber)(int)note.Drum;

                    notesCollection.Add(new Melanchall.DryWetMidi.Interaction.Note(
                        midiNoteNumber, durationTicks, groupStartTick));
                }

                currentTick += durationTicks;
            }
        }

        notesManager.SaveChanges();

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        midiFile.Write(filePath, true);
    }

    public static Sheet ImportMidiToSheet(string filePath, string name = "Imported", string description = "")
    {
        var midiFile = MidiFile.Read(filePath);
        var tempoMap = midiFile.GetTempoMap();

        var tempo = tempoMap.GetTempoAtTime(new MetricTimeSpan(0, 0, 0));
        var bpm = tempo.MicrosecondsPerQuarterNote > 0
            ? (int)Math.Round(60000000.0 / tempo.MicrosecondsPerQuarterNote)
            : 120;
        var sheetTempo = new Bpm(bpm);

        var midiNotes = midiFile.GetNotes().ToList(); // materialize
        if (midiNotes.Count == 0)
            return new Sheet(sheetTempo, ImmutableArray<Measure>.Empty, name, description);

        var sixteenthTicks = TimeConverter.ConvertFrom(MusicalTimeSpan.Sixteenth, tempoMap);
        var measureTicks = TimeConverter.ConvertFrom(MusicalTimeSpan.Whole, tempoMap);

        foreach (var n in midiNotes)
        {
            var quantized = Math.Round(n.Time / (double)sixteenthTicks) * sixteenthTicks;
            n.Time = (long)quantized;
        }

        var lastNoteEndTick = midiNotes.Max(n => n.Time + n.Length);
        var measuresCount = (int)((lastNoteEndTick + measureTicks - 1) / measureTicks);

        var measures = new List<Measure>(measuresCount);

        for (var measureIdx = 0; measureIdx < measuresCount; measureIdx++)
        {
            var measureStartTick = measureIdx * measureTicks;
            var notesInMeasure = midiNotes
                .Where(n => n.Time >= measureStartTick && n.Time < measureStartTick + measureTicks)
                .ToList();

            measures.Add(CreateMeasureFromMidiMeasure(notesInMeasure, measureStartTick, tempoMap));
        }

        return new Sheet(sheetTempo, measures.ToImmutableArray(), name, description);
    }

    private static Measure CreateMeasureFromMidiMeasure(List<Melanchall.DryWetMidi.Interaction.Note> notesInMeasure,
        long measureStartTick, TempoMap tempoMap)
    {
        var quarterTicks = TimeConverter.ConvertFrom(MusicalTimeSpan.Quarter, tempoMap);
        var eighthTicks = TimeConverter.ConvertFrom(MusicalTimeSpan.Eighth, tempoMap);
        var sixteenthTicks = TimeConverter.ConvertFrom(MusicalTimeSpan.Sixteenth, tempoMap);

        var rgList = new List<RythmicGroup>(4);

        for (var rgIndex = 0; rgIndex < 4; rgIndex++)
        {
            var rgStart = measureStartTick + rgIndex * quarterTicks;
            var slotIndex = 0;
            var noteGroupsForRg = new List<NoteGroup>();

            while (slotIndex < 4)
            {
                var slotStart = rgStart + slotIndex * sixteenthTicks;
                var slotEnd = slotStart + sixteenthTicks;

                var starters = notesInMeasure
                    .Where(n => n.Time >= slotStart && n.Time < slotEnd)
                    .GroupBy(n => n.Time)
                    .OrderBy(g => g.Key)
                    .FirstOrDefault()?.ToList() ?? new List<Melanchall.DryWetMidi.Interaction.Note>();

                if (starters.Count == 0)
                {
                    var restNote = new Note(Drum.Rest, NoteValue.Sixteenth);
                    noteGroupsForRg.Add(new NoteGroup(new List<Note> { restNote }));
                    slotIndex += 1;
                    continue;
                }

                var groupNotes = new List<Note>(starters.Count);
                long chosenDurationTicks = 0;

                foreach (var mn in starters)
                {
                    var drumEnum = Enum.IsDefined(typeof(Drum), (int)mn.NoteNumber)
                        ? (Drum)(int)mn.NoteNumber
                        : Drum.Rest;

                    if (mn.Length > chosenDurationTicks)
                        chosenDurationTicks = mn.Length;

                    groupNotes.Add(new Note(drumEnum, NoteValue.Sixteenth));
                }

                NoteValue groupValue;
                if (chosenDurationTicks >= quarterTicks) groupValue = NoteValue.Quarter;
                else if (chosenDurationTicks >= eighthTicks) groupValue = NoteValue.Eighth;
                else groupValue = NoteValue.Sixteenth;

                var finalizedNotes = groupNotes
                    .Select(n => new Note(n.Drum, groupValue))
                    .ToList();

                noteGroupsForRg.Add(new NoteGroup(finalizedNotes));

                var consumedSlots = groupValue switch
                {
                    NoteValue.Quarter => 4,
                    NoteValue.Eighth => 2,
                    NoteValue.Sixteenth => 1,
                    _ => 1
                };

                slotIndex += consumedSlots;
            }

            noteGroupsForRg = RecordingService.UpscaleNotes(noteGroupsForRg);
            rgList.Add(new RythmicGroup([..noteGroupsForRg]));
        }

        return new Measure(rgList);
    }

    private static ITimeSpan NoteValueToMusicalTimeSpan(NoteValue value)
    {
        return value switch
        {
            NoteValue.Quarter => MusicalTimeSpan.Quarter,
            NoteValue.Eighth => MusicalTimeSpan.Eighth,
            NoteValue.Sixteenth => MusicalTimeSpan.Sixteenth,
            _ => MusicalTimeSpan.Quarter
        };
    }
}