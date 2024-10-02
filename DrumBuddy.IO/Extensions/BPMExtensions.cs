using DrumBuddy.IO.Models;

namespace DrumBuddy.IO.Extensions
{
    public static class BPMExtensions
    {
        public static TimeSpan QuarterNoteDuration(this BPM bpm) => TimeSpan.FromSeconds(60.0 / bpm);
        public static TimeSpan EighthNoteDuration(this BPM bpm) => bpm.QuarterNoteDuration() / 2;
        public static TimeSpan SixteenthNoteDuration(this BPM bpm) => bpm.EighthNoteDuration() / 2;

    }
}
