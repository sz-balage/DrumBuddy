using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;

namespace DrumBuddy.Core.Models
{
    public class Sheet(BPM tempo, List<Measure> measures, string name)
    {
        public TimeSpan Length => CalculateLength();
        private TimeSpan CalculateLength()
        {
            if (Measures.Count>0)
            {
                return (Measures.Count - 1) * (4 * Tempo.QuarterNoteDuration()) +
                       (Measures.Last().Groups.Count(g => g.Notes != null) * Tempo.QuarterNoteDuration());
            }
            return TimeSpan.Zero;
        }
        public string Name { get; init; } = name;

        public BPM Tempo { get; } = tempo;

        public List<Measure> Measures { get; } = measures;
    }
}
