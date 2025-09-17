using System;
using System.Linq;
using System.Reactive.Linq;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Services;
using DrumBuddy.IO;
using DrumBuddy.IO.Abstractions;

namespace DrumBuddy.Client.Extensions;

public static class MidiExtensions
{
    public static IObservable<Drum> GetMappedBeatsObservable(
        this IMidiService midiService, ConfigurationService config)
    {
        return midiService.GetRawNoteObservable() // returns int note numbers
            .Select(noteNumber =>
            {
                var match = config.Mapping.FirstOrDefault(kvp => kvp.Value == noteNumber);
                return match.Key == default ? Drum.Rest : match.Key;
            });
    }
    public static IObservable<Drum> GetMappedBeatsForKeyboard(
        this IObservable<int> keyboardBeats, ConfigurationService config)
    {
        return keyboardBeats 
            .Select(noteNumber =>
            {
                var match = config.Mapping.FirstOrDefault(kvp => kvp.Value == noteNumber);
                return match.Key == default ? Drum.Rest : match.Key;
            });
    }
}