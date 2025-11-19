using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace DrumBuddy.Core.Models;

/// <summary>
///     Represents a group of notegroups that are played in a time window of one quarter of a measure.
/// </summary>
/// <param name="NoteGroups">Note groups inside the rythmic group</param>
public record RythmicGroup(ImmutableArray<NoteGroup> NoteGroups) : IEquatable<RythmicGroup>
{
    [JsonIgnore] public bool IsEmpty => NoteGroups.All(n => n.IsRest);

    public virtual bool Equals(RythmicGroup? other)
    {
        if (other is null)
            return false;

        if (NoteGroups.Length != other.NoteGroups.Length)
            return false;
        for (var i = 0; i < NoteGroups.Length; i++)
        {
            var thisNg = NoteGroups[i];
            var otherNg = other.NoteGroups[i];
            if (otherNg.Count != thisNg.Count)
                return false;
            for (var j = 0; j < thisNg.Count; j++)
                if (thisNg[j] != otherNg[j])
                    return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var noteGroup in NoteGroups)
        foreach (var note in noteGroup)
            hash.Add(note);
        return hash.ToHashCode();
    }
}