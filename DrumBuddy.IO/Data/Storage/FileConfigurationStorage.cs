using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Enums;

namespace DrumBuddy.IO.Services;

public class FileConfigurationStorage
{
    private readonly ISerializationService _serializationService;
    private readonly string _saveDirectory;
    private const string FileName = "config";
    private const string FileExtension = ".txt";
    private string GetFullConfigPath() => Path.Combine(_saveDirectory, FileName + FileExtension);
    public FileConfigurationStorage(ISerializationService serializationService)
    {
        _serializationService = serializationService;
        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DrumBuddy", "SavedFiles");

        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }
    public void SaveConfig(Dictionary<Drum, int> mapping)
    {
        var path = GetFullConfigPath();
        var data = _serializationService.SerializeDrumMappingData(mapping);
        File.WriteAllText(path,data);

    }
    public Dictionary<Drum, int> LoadConfig()
    {
        var path = GetFullConfigPath();
        if (!File.Exists(path)) 
            return new();
        return _serializationService.DeserializeDrumMappingData(File.ReadAllText(path)) ?? new();
    }
}