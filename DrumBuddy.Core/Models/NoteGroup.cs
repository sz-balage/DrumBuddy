using System.Text.Json.Serialization;
using DrumBuddy.Core.Enums;

namespace DrumBuddy.Core.Models;
/// <summary>
/// Represents a group of notes that are played at the same time.
/// </summary>
public class NoteGroup : List<Note>
{
    private const int MaxSize = 3;

    public NoteGroup()
    {
    }

    public NoteGroup(List<Note> notes) : base(notes)
    {
        if (notes.Count > MaxSize) throw new InvalidOperationException("How many limbs you got?");
        Value = notes.First().Value;
    }

    public NoteGroup(IEnumerable<Note> notes) : base(notes)
    {
        if (notes.Count() > MaxSize) throw new InvalidOperationException("How many limbs you got?");
        Value = notes.First().Value;
    }

    public NoteValue Value { get; private set; }
    
    [JsonIgnore]

    public bool IsRest => Count == 0 || (Count == 1 && this[0].Drum == Drum.Rest);

    public new void Add(Note note)
    {
        if (Count == MaxSize) throw new InvalidOperationException("How many limbs you got?");
        // If this is the first note, set the Value property
        if (Count == 0)
            Value = note.Value;
        base.Add(note);
    }

    public bool Contains(Drum drum)
    {
        return this.Any(note => note.Drum == drum);
    }

    public void RefreshValue()
    {
        Value = this.First().Value;
    }
    public NoteGroup ChangeValues(NoteValue value)
    {
        Value = value;
        return new NoteGroup(this.Select(note => new Note(note.Drum, value)));
    }
}