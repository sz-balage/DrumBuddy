using System.Collections.Immutable;
using System.Xml.Linq;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Services;

public static class MusicXmlExporter
{
    public static void ExportSheetToMusicXml(Sheet sheet, string filePath)
    {
        if (sheet is null) throw new ArgumentNullException(nameof(sheet));
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

        const int divisions = 16;
        var version = "3.1";

        XNamespace ns = "";

        var scorePartwise = new XElement("score-partwise",
            new XAttribute("version", version),
            // part-list
            new XElement("part-list",
                new XElement("score-part",
                    new XAttribute("id", "P1"),
                    new XElement("part-name", "Percussion")
                )
            )
        );

        var part = new XElement("part", new XAttribute("id", "P1"));

        for (var measureIndex = 0; measureIndex < sheet.Measures.Length; measureIndex++)
        {
            var measureNumber = measureIndex + 1;
            var measureElem = new XElement("measure", new XAttribute("number", measureNumber));

            if (measureIndex == 0)
            {
                var attributes = new XElement("attributes",
                    new XElement("divisions", divisions),
                    new XElement("time",
                        new XElement("beats", 4),
                        new XElement("beat-type", 4)
                    ),
                    new XElement("clef",
                        new XElement("sign", "percussion"),
                        new XElement("line", 2)
                    )
                );
                measureElem.Add(attributes);

                var direction = new XElement("direction",
                    new XElement("direction-type",
                        new XElement("metronome",
                            new XElement("beat-unit", "quarter"),
                            new XElement("per-minute", sheet.Tempo.Value)
                        )
                    ),
                    new XElement("sound", new XAttribute("tempo", sheet.Tempo.Value))
                );
                measureElem.Add(direction);
            }

            var measure = sheet.Measures[measureIndex];
            foreach (var rg in measure.Groups)
            foreach (var noteGroup in rg.NoteGroups)
            {
                if (noteGroup == null)
                    continue;

                var dur = NoteValueToDivisions(noteGroup.Value, divisions);
                var typeStr = NoteValueToTypeString(noteGroup.Value);

                var first = true;
                foreach (var note in noteGroup)
                {
                    var noteElem = new XElement("note");

                    if (!first)
                        noteElem.Add(new XElement("chord"));

                    if (note.Drum == Drum.Rest)
                    {
                        noteElem.Add(new XElement("rest"));
                    }
                    else
                    {
                        var unpitched = new XElement("unpitched",
                            new XElement("display-step", DrumToDisplayStep(note.Drum)),
                            new XElement("display-octave", DrumToDisplayOctave(note.Drum))
                        );
                        noteElem.Add(unpitched);

                        noteElem.Add(new XElement("instrument", new XAttribute("id", "P1")));
                    }

                    noteElem.Add(new XElement("duration", dur));
                    noteElem.Add(new XElement("voice", 1));
                    noteElem.Add(new XElement("type", typeStr));

                    noteElem.Add(new XElement("staff", 1));

                    measureElem.Add(noteElem);
                    first = false;
                }
            }

            part.Add(measureElem);
        }

        scorePartwise.Add(part);

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            scorePartwise
        );

        doc.Save(filePath);
    }

    private static int NoteValueToDivisions(NoteValue nv, int divisions)
    {
        return nv switch
        {
            NoteValue.Quarter => divisions,
            NoteValue.Eighth => divisions / 2,
            NoteValue.Sixteenth => divisions / 4,
            _ => divisions
        };
    }

    private static string NoteValueToTypeString(NoteValue nv)
    {
        return nv switch
        {
            NoteValue.Quarter => "quarter",
            NoteValue.Eighth => "eighth",
            NoteValue.Sixteenth => "16th",
            _ => "quarter"
        };
    }

    private static string DrumToDisplayStep(Drum drum)
    {
        return drum switch
        {
            Drum.Kick => "F",
            Drum.FloorTom => "D",
            Drum.Snare => "C",
            Drum.Tom2 => "B",
            Drum.Tom1 => "A",
            Drum.Ride => "G",
            Drum.HiHat => "G",
            Drum.HiHat_Open => "G",
            Drum.HiHat_Pedal => "G",
            Drum.Crash1 => "A",
            Drum.Crash2 => "A",
            _ => "C"
        };
    }

    private static int DrumToDisplayOctave(Drum drum)
    {
        return drum switch
        {
            Drum.Kick => 2,
            Drum.FloorTom => 3,
            Drum.Snare => 3,
            Drum.Tom2 => 4,
            Drum.Tom1 => 4,
            Drum.Ride => 5,
            Drum.HiHat => 5,
            Drum.HiHat_Open => 5,
            Drum.HiHat_Pedal => 5,
            Drum.Crash1 => 5,
            Drum.Crash2 => 5,
            _ => 4
        };
    }

    public static Sheet ImportMusicXmlToSheet(string filePath, string name = "Imported",
        string description = "Imported from MusicXML")
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("MusicXML file not found", filePath);

        var doc = XDocument.Load(filePath);
        var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

        var part = doc.Descendants("part").FirstOrDefault();
        if (part == null)
            throw new InvalidDataException("Invalid MusicXML: no <part> found.");

        var tempo = 120; //default
        var tempoNode = doc.Descendants("sound").FirstOrDefault(e => e.Attribute("tempo") != null);
        if (tempoNode != null && int.TryParse(tempoNode.Attribute("tempo")?.Value, out var bpm))
        {
            tempo = bpm;
        }
        else
        {
            var perMin = doc.Descendants("per-minute").FirstOrDefault();
            if (perMin != null && int.TryParse(perMin.Value, out bpm))
                tempo = bpm;
        }

        var measures = new List<Measure>();
        foreach (var measureElem in part.Elements("measure"))
        {
            var rhythmicGroups = new List<RythmicGroup>();

            var noteElements = measureElem.Elements("note").ToList();
            if (noteElements.Count == 0)
            {
                rhythmicGroups.AddRange(CreateEmptyRythmicGroups(4));
                measures.Add(new Measure(rhythmicGroups));
                continue;
            }

            var noteGroups = new List<NoteGroup>();
            List<Note>? currentChord = null;

            foreach (var noteElem in noteElements)
            {
                var isChord = noteElem.Element("chord") != null;
                var isRest = noteElem.Element("rest") != null;

                var typeStr = noteElem.Element("type")?.Value ?? "quarter";
                var value = ParseNoteValue(typeStr);

                Drum drum;
                if (isRest)
                {
                    drum = Drum.Rest;
                }
                else
                {
                    var displayStep = noteElem.Element("unpitched")?.Element("display-step")?.Value ?? "C";
                    drum = DisplayStepToDrum(displayStep);
                }

                var note = new Note(drum, value);

                if (isChord)
                {
                    currentChord ??= new List<Note>();
                    currentChord.Add(note);
                }
                else
                {
                    if (currentChord != null && currentChord.Count > 0)
                    {
                        noteGroups.Add(new NoteGroup(currentChord));
                        currentChord = null;
                    }

                    currentChord = new List<Note> { note };
                }
            }

            if (currentChord != null && currentChord.Count > 0)
                noteGroups.Add(new NoteGroup(currentChord));

            const int divisions = 16;
            var rgList = new List<RythmicGroup>();
            var currentRGNotes = new List<NoteGroup>();
            var currentRGDuration = 0;

            foreach (var ng in noteGroups)
            {
                var dur = NoteValueToDivisions(ng.Value, divisions);
                currentRGNotes.Add(ng);
                currentRGDuration += dur;

                if (currentRGDuration >= divisions)
                {
                    rgList.Add(new RythmicGroup(currentRGNotes.ToImmutableArray()));
                    currentRGNotes = new List<NoteGroup>();
                    currentRGDuration = 0;
                }
            }
            while (rgList.Count < 4)
                rgList.Add(CreateRestRythmicGroup(NoteValue.Quarter));

            measures.Add(new Measure(rgList));
        }

        var sheet = new Sheet(new Bpm(tempo), [..measures], name, description);
        return sheet;
    }

    private static IEnumerable<RythmicGroup> CreateEmptyRythmicGroups(int count)
    {
        for (var i = 0; i < count; i++)
            yield return CreateRestRythmicGroup(NoteValue.Quarter);
    }

    private static RythmicGroup CreateRestRythmicGroup(NoteValue value)
    {
        var restNote = new Note(Drum.Rest, value);
        var ng = new NoteGroup(new[] { restNote });
        return new RythmicGroup(ImmutableArray.Create(ng));
    }

    private static NoteValue ParseNoteValue(string s)
    {
        return s.ToLowerInvariant() switch
        {
            "quarter" => NoteValue.Quarter,
            "eighth" => NoteValue.Eighth,
            "16th" or "sixteenth" => NoteValue.Sixteenth,
            _ => NoteValue.Quarter
        };
    }

    private static Drum DisplayStepToDrum(string step)
    {
        return step.ToUpperInvariant() switch
        {
            "F" => Drum.Kick,
            "D" => Drum.FloorTom,
            "C" => Drum.Snare,
            "B" => Drum.Tom2,
            "A" => Drum.Tom1,
            "G" => Drum.Ride,
            _ => Drum.Snare
        };
    }
}