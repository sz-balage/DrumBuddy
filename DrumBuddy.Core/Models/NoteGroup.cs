using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Core.Models;

public class NoteGroup : List<Note>
{
    private const int _maxSize = 3;

    public NoteGroup()
    {
    }

    public NoteGroup(List<Note> notes) : base(notes)
    {
        if (notes.Count > _maxSize) throw new InvalidOperationException("How many limbs you got?");
        Value = notes.First().Value;
    }

    public NoteGroup(IEnumerable<Note> notes) : base(notes)
    {
        if (notes.Count() > _maxSize) throw new InvalidOperationException("How many limbs you got?");
        Value = notes.First().Value;
    }

    public NoteValue Value { get; private set; }

    public bool IsRest => Count == 0 || (Count == 1 && this[0].Drum == Drum.Rest);

    public new void Add(Note note)
    {
        if (Count == _maxSize) throw new InvalidOperationException("How many limbs you got?");
        base.Add(note);
    }

    public bool Contains(Drum drum)
    {
        return this.Any(note => note.Drum == drum);
    }

    public NoteGroup ChangeValues(NoteValue value)
    {
        Value = value;
        return new NoteGroup(this.Select(note => new Note(note.Drum, value)));
    }
}