using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Core.Models
{
    public record Note(DrumType DrumType, NoteValue Value, Timing Timing);
}
