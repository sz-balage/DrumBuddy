namespace DrumBuddy.Core.Models
{
    /// <summary>
    /// Represents a single measure of music.
    /// </summary>
    /// <param name="Notes">The 4 notes that make up the measure.</param>
    public record Measure(List<RythmicGroup> Groups)
    {
        public bool IsEmpty => Groups.Count == 0;
    }
}
