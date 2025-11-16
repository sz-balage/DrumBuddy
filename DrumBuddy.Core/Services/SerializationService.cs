using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using ProtoBuf;

namespace DrumBuddy.Core.Services;

public class SerializationService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public byte[] SerializeMeasurementData(ImmutableArray<Measure> measures)
    {
        var protoSheet = ConvertToProto(measures);
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, protoSheet);
        return ms.ToArray();
    }

    public ImmutableArray<Measure> DeserializeMeasurementData(byte[] data)
    {
        using var ms = new MemoryStream(data);
        var protoMeasures = Serializer.Deserialize<ImmutableArray<MeasureProto>>(ms);
        return ConvertFromProto(protoMeasures);
    }

    private ImmutableArray<MeasureProto> ConvertToProto(ImmutableArray<Measure> measures) =>
    [
        ..measures
            .Select(m => new MeasureProto
            {
                Groups = m.Groups
                    .Select(g => new RythmicGroupProto
                    {
                        NoteGroups = g.NoteGroups
                            .Select(ng => new NoteGroupProto
                            {
                                Notes = ng
                                    .Select(n => new NoteProto
                                    {
                                        Drum = (int)n.Drum,
                                        NoteValue = (int)n.Value
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
    ];

    private ImmutableArray<Measure> ConvertFromProto(ImmutableArray<MeasureProto> protoMeasures) =>
    [
        ..protoMeasures
            .Select(m => new Measure(
                m.Groups
                    .Select(g => new RythmicGroup(
                        [
                            ..g.NoteGroups.Select(ng =>
                                new NoteGroup(ng.Notes.Select(n => new Note((Drum)n.Drum, (NoteValue)n.NoteValue))))
                        ]
                    ))
                    .ToList()
            ))
    ];

    public string SerializeAppConfiguration(AppConfiguration appConfig)
    {
        return JsonSerializer.Serialize(appConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public AppConfiguration? DeserializeAppConfiguration(string appConfigJson)
    {
        return JsonSerializer.Deserialize<AppConfiguration>(appConfigJson);
    }

    public string SerializeSheet(Sheet sheet)
    {
        var sheetData = new SheetData(sheet.Tempo, sheet.Measures, sheet.Description);
        return JsonSerializer.Serialize(sheetData, _options);
    }

    public Sheet? DeserializeDbSheetFile(string sheetJson, string fileName)
    {
        var sheetData = JsonSerializer.Deserialize<SheetData>(sheetJson, _options)
                        ?? throw new InvalidOperationException("Failed to deserialize sheet");
        const int defaultTempo = 100;

        Bpm tempo;
        try
        {
            tempo = new Bpm(sheetData.Tempo);
        }
        catch (ArgumentException)
        {
            tempo = new Bpm(defaultTempo);
        }

        return new Sheet(tempo, sheetData.Measures, fileName, sheetData.Description);
    }

    private record SheetData(int Tempo, ImmutableArray<Measure> Measures, string Description);
}