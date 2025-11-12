using System.Text.Json.Serialization;

namespace DrumBuddy.IO.Models;

public class SheetDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("tempo")]
    public int Tempo { get; set; }

    [JsonPropertyName("measureBytes")]
    public byte[] MeasureBytes { get; set; } // Base64 encoded in JSON

    [JsonPropertyName("isSyncEnabled")]
    public bool IsSyncEnabled { get; set; }
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}