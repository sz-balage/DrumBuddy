using DrumBuddy.IO.Models;

namespace DrumBuddy.IO.Extensions
{
    public static class BpmExtensions
    {
        public static TimeSpan QuarterNoteDuration(this Bpm bpm) => TimeSpan.FromSeconds(60.0 / bpm);
        public static TimeSpan EighthNoteDuration(this Bpm bpm) => bpm.QuarterNoteDuration() / 2;
        public static TimeSpan SixteenthNoteDuration(this Bpm bpm) => bpm.EighthNoteDuration() / 2;

    }
}
