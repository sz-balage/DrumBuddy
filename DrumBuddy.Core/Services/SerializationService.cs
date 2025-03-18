using System.Text.Json;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Services;


public class SerializationService : ISerializationService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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