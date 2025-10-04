using System.Text.Json.Serialization;
using DrumBuddy.Core.Enums;

namespace DrumBuddy.Core.Models;

/// <summary>
///     Represents a group of notes that are played at the same time.
/// </summary>
public class NoteGroup : List<Note>
{
    private const int MaxSize = 4;

    public NoteGroup()
    {
    }

    //TODO: constructor should not crash application
    public NoteGroup(List<Note> notes) : base(notes)
    {
        if (notes.Count > MaxSize) throw new InvalidOperationException("How many limbs you got?");
    }

    public NoteGroup(IEnumerable<Note> notes) : base(notes)
    {
        if (notes.Count() > MaxSize) throw new InvalidOperationException("How many limbs you got?");
    }

    public NoteValue Value => this.First().Value;

    [JsonIgnore] public bool IsRest => Count == 0 || (Count == 1 && this[0].Drum == Drum.Rest);

    public new void Add(Note note)
    {
        if (Count == MaxSize) throw new InvalidOperationException("How many limbs you got?");
        base.Add(note);
    }

    public bool Contains(Drum drum)
    {
        return this.Any(note => note.Drum == drum);
    }

    public NoteGroup ChangeValues(NoteValue value)
    {
        return new NoteGroup(this.Select(note => new Note(note.Drum, value)));
    }
}