using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Core.Models;
public class NoteGroup : List<Note>
{
    public NoteGroup()
    { }
        
    public NoteGroup(List<Note> notes) : base(notes) { }
        
    public NoteGroup(IEnumerable<Note> notes) : base(notes) { }
        
    public bool IsRest => Count == 0 || (Count == 1 && this[0].Beat == Beat.Rest);
        
    public bool Contains(Beat beat) => this.Any(note => note.Beat == beat);

    public NoteGroup ChangeValues(NoteValue value)
    {
        return new(this.Select(note => new Note(note.Beat, value)));
    }
}