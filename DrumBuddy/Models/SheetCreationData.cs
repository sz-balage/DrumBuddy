using System.Collections.Immutable;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Models;

public record SheetCreationData(Bpm Bpm, ImmutableArray<Measure> Measures);