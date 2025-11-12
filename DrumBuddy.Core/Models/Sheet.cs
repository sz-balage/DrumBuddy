using System.Collections.Immutable;
using System.Text.Json.Serialization;
using DrumBuddy.Core.Extensions;

namespace DrumBuddy.Core.Models;

public class Sheet
{
    // Parameterless constructor for JSON deserialization
    [JsonConstructor]
    public Sheet() { }

    // Original constructor for code usage
    public Sheet(Bpm tempo, ImmutableArray<Measure> measures, string name, string description, Guid? id = null, DateTime? updatedAt = null)
    {
        Name = name;
        Description = description;
        Tempo = tempo;
        Measures = measures;
        Id = id ?? Guid.NewGuid();
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("lastSyncedAt")]
    public DateTime? LastSyncedAt { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("tempo")]
    public Bpm Tempo { get; set; }

    [JsonPropertyName("measures")]
    public ImmutableArray<Measure> Measures { get; set; }

    // client specific
    [JsonPropertyName("isSyncEnabled")]
    public bool IsSyncEnabled { get; set; }
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } 

    [JsonIgnore]
    public TimeSpan Length => CalculateLength();

    private TimeSpan CalculateLength()
    {
        if (Measures.Length > 0)
            return (Measures.Length - 1) * (4 * Tempo.QuarterNoteDuration()) +
                   Measures.Last().Groups.Count(g => g.NoteGroups != null) * Tempo.QuarterNoteDuration();
        return TimeSpan.Zero;
    }

    public Sheet RenameSheet(string newName, string newDescription)
    {
        return new Sheet(Tempo, Measures, newName, newDescription, Id);
    }

    public Sheet Sync()
    {
        IsSyncEnabled = true;
        LastSyncedAt = DateTime.UtcNow;
        return this;
    }
    public Sheet UnSync()
    {
        IsSyncEnabled = false;
        return this;
    }
}