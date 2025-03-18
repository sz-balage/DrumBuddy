using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Abstractions;

public interface ISerializationService
{
    string SerializeSheet(Sheet sheet);
    Sheet DeserializeSheet(string json);
}
