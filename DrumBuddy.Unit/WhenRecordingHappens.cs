using DrumBuddy.Core.Models;
using Microsoft.Reactive.Testing;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Enums;
using DrumBuddy.Core.Enums;
using Shouldly;

namespace DrumBuddy.Core.Unit
{
    public class WhenRecordingHappens
    {
        private readonly TestScheduler _testScheduler;

        public WhenRecordingHappens()
        {
            _testScheduler = new TestScheduler();
        }
    }
}