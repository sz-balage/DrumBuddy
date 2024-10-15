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
        [Fact]

        public void New_test()
        {
            BPM tempo = (BPM)BPM.From(100); //4: 0.6s (in ticks: 6000000), 8: 0.3s (in ticks: 3000000), 16: 0.15s (in ticks: 1500000)
            _recordingService.Tempo = tempo;
            var observable = _testScheduler.CreateColdObservable(
                new Recorded<Notification<Beat>>(TimeSpan.FromMilliseconds(150).Ticks, Notification.CreateOnNext(new Beat(DrumType.Bass))) //0.15s
                ); //0.3s
            _midiService.GetBeatsObservable().Returns(observable.AsObservable());
            var length = tempo.QuarterNoteDuration().Ticks*2;
            var observer = _testScheduler.Start(
                               () => _recordingService.GetNotesObservable(),
                                              0,
                                              0,
                                              length);
            ;
        }

        [Fact]
        public void Testing()
        {
            BPM tempo = (BPM)BPM.From(100); //4: 0.6s (in ticks: 6000000), 8: 0.3s (in ticks: 3000000), 16: 0.15s (in ticks: 1500000)
            //set the tempo of recording
            _recordingService.Tempo = tempo;
            var beatsFromMidiSim = _testScheduler.CreateColdObservable(
                new Recorded<Notification<Beat>>(TimeSpan.FromMilliseconds(0).Ticks, Notification.CreateOnNext(new Beat(DrumType.Bass))), //0.15s
                new Recorded<Notification<Beat>>(TimeSpan.FromMilliseconds(150).Ticks, Notification.CreateOnNext(new Beat(DrumType.Bass))) //0.15s
            );

            _midiService.GetBeatsObservable().Returns(beatsFromMidiSim.AsObservable());
            var metronomeObserver = _testScheduler.Start(
                () => _recordingService.GetMetronomeBeeping(),
                0,
                0,
                tempo.QuarterNoteDuration().Ticks*4
            );
            var observer = _testScheduler.Start(
                () => _recordingService.GetBeatsBuffered(),
                0,
                (tempo.SixteenthNoteDuration() / 2).Ticks, //starts 1/32th after the metronome starts
                tempo.QuarterNoteDuration().Ticks*4 //only observer for 1 quarter note
            );
        }
        [Fact]
        public void GetBeatsBuffered_ShouldReturnBufferedBeats()
        {
            // Arrange
            var beatsSubject = new Subject<Beat>();
            var beatsObservable = beatsSubject.AsObservable();
            _midiService.GetBeatsObservable().Returns(beatsObservable);

            var expectedBeats = new List<(Beat, long)>
            {
                (new Beat(DrumType.Snare), 100),
                (new Beat (DrumType.Bass), 200)
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _recordingService.StopWatch.Start();

            // Set the BPM
            _recordingService.Tempo = (BPM)BPM.From(120); // Example BPM value

            // Act
            var bufferedBeats = new List<IList<(Beat, long)>>();
            var subscription = _recordingService.GetBeatsBuffered().Subscribe(bufferedBeats.Add);


            beatsSubject.OnCompleted();
            subscription.Dispose();

            // Assert
            Assert.Single(bufferedBeats);
            Assert.Equal(expectedBeats.Count, bufferedBeats[0].Count);
            for (int i = 0; i < expectedBeats.Count; i++)
            {
                Assert.Equal(expectedBeats[i].Item1.Drum, bufferedBeats[0][i].Item1.Drum);
                Assert.True(Math.Abs(expectedBeats[i].Item2 - bufferedBeats[0][i].Item2) < 50); // Allow some tolerance for timing
            }
        }
    }
}
