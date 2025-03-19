using System.Collections.Immutable;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Client.Models;

public record SheetCreationData(Bpm Bpm, ImmutableArray<Measure> Measures);