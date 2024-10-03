using DrumBuddy.IO.Models;

namespace DrumBuddy.IO.Abstractions;

public interface IMidiService
{
    IObservable<Beat> GetBeatsObservable();
}