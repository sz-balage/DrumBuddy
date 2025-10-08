using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using DrumBuddy.Core.Models;

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
        return JsonSerializer.SerializeToUtf8Bytes(measures);
    }

    public ImmutableArray<Measure> DeserializeMeasurementData(byte[] json)
    {
        var measures = JsonSerializer.Deserialize<IEnumerable<Measure>>(json)
                       ?? throw new InvalidOperationException("Failed to deserialize sheet");
        return [..measures];
    }

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

    public Sheet? DeserializeSheet(string sheetJson, string fileName)
    {
        var sheetData = JsonSerializer.Deserialize<SheetData>(sheetJson, _options)
                        ?? throw new InvalidOperationException("Failed to deserialize sheet");

        const int DefaultTempo = 100;

        Bpm tempo;
        try
        {
            tempo = new Bpm(sheetData.Tempo);
        }
        catch (ArgumentException)
        {
            tempo = new Bpm(DefaultTempo);
        }

        return new Sheet(tempo, sheetData.Measures, fileName, sheetData.Description);
    }

    private record SheetData(int Tempo, ImmutableArray<Measure> Measures, string Description);
}