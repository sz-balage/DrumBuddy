using System.Text.Json;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Helpers;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Services;


public class SerializationService : ISerializationService
{
    private readonly JsonSerializerOptions _options;

    public SerializationService()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        _options.Converters.Add(new BpmJsonConverter());
    }

    public string SerializeSheet(Sheet sheet)
    {
        return JsonSerializer.Serialize(sheet, _options);
    }

    public Sheet DeserializeSheet(string json)
    {
        return JsonSerializer.Deserialize<Sheet>(json, _options)
               ?? throw new InvalidOperationException("Failed to deserialize sheet");
    }
}