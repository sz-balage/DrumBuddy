using DrumBuddy.Core.Enums;
using DrumBuddy.IO.Enums;

namespace DrumBuddy.Core.Models;

public record Note(Drum Drum, NoteValue Value);