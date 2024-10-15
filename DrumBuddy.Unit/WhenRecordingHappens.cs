using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using DrumBuddy.Core.Models;
using Microsoft.Reactive.Testing;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Enums;
using DrumBuddy.IO.Extensions;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Services;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using System.Reactive.Subjects;

namespace DrumBuddy.Core.Unit
{
    public class WhenRecordingHappens
    {
        private readonly TestScheduler _testScheduler;
        private readonly IMidiService _midiService;
        private readonly RecordingService _recordingService;
        public WhenRecordingHappens()
        {
            _testScheduler = new TestScheduler();
            _midiService = Substitute.For<IMidiService>();
            _recordingService = new RecordingService(_midiService,_testScheduler);
        }
    }
}
