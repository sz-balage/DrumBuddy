using DrumBuddy.IO.Models;

namespace DrumBuddy.Core.Models
{
    public class Sheet
    {
        public Sheet(BPM tempo, List<Measure> measures)
        {
            Tempo = tempo;
            Measures = measures;
        }

        public BPM Tempo { get; }

        public List<Measure> Measures { get; }
    }
}
