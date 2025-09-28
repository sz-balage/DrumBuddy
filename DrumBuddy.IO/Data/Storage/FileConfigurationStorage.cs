using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Enums;

namespace DrumBuddy.IO.Services;

public class FileConfigurationStorage
{
    private const string FileName = "config";
    private const string FileExtension = ".txt";
    private readonly string _saveDirectory;
    private readonly ISerializationService _serializationService;

    public FileConfigurationStorage(ISerializationService serializationService, string saveDirectory)
    {
        _serializationService = serializationService;
        _saveDirectory = saveDirectory;
        // _saveDirectory = Path.Combine(FilePathProvider.GetPathForFileStorage(), "config");

        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }

    private string GetFullConfigPath()
    {
        return Path.Combine(_saveDirectory, FileName + FileExtension);
    }

    public void SaveConfig(Dictionary<Drum, int> mapping)
    {
        var path = GetFullConfigPath();
        var data = _serializationService.SerializeDrumMappingData(mapping);
        File.WriteAllText(path, data);
    }

    public Dictionary<Drum, int> LoadConfig()
    {
        var path = GetFullConfigPath();
        if (!File.Exists(path))
            return new Dictionary<Drum, int>();
        return _serializationService.DeserializeDrumMappingData(File.ReadAllText(path)) ?? new Dictionary<Drum, int>();
    }

    public Dictionary<Drum, double> LoadDrumPositions()
    {
        return new Dictionary<Drum, double>();
    }

    public void SaveDrumPositions(Dictionary<Drum, double> drumPositions)
    {
    }
}