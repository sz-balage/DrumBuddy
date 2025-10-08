using System.Collections.Immutable;
using System.Text.Json;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Services;

public class SerializationService
{
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

    public async Task<string> SerializeSheet(Sheet sheet)
    {
        return JsonSerializer.Serialize(sheet, new JsonSerializerOptions { WriteIndented = true });
    }
}