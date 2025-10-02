using System.Text.Json;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Data.Storage;

public class FileConfigurationStorage
{
    private const string FileName = "config.json";
    private readonly string _saveDirectory;
    private readonly ISerializationService _serializationService;

    public FileConfigurationStorage(ISerializationService serializationService, string saveDirectory)
    {
        _serializationService = serializationService;
        _saveDirectory = saveDirectory;
        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }

    private string GetFullPath() => Path.Combine(_saveDirectory, FileName);

    public void SaveConfig(AppConfiguration config)
    {
        var json = _serializationService.SerializeAppConfiguration(config);
        File.WriteAllText(GetFullPath(), json);
    }

    public AppConfiguration LoadConfig()
    {
        var path = GetFullPath();
        if (!File.Exists(path))
            return new AppConfiguration();

        var json = File.ReadAllText(path);
        return _serializationService.DeserializeAppConfiguration(json) ?? new AppConfiguration();
    }
}
