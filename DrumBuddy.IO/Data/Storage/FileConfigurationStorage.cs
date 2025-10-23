using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;

namespace DrumBuddy.IO.Data.Storage;

public class FileConfigurationStorage
{
    private const string FileName = "config.json";
    private readonly string _saveDirectory;
    private readonly SerializationService _serializationService;

    public FileConfigurationStorage(SerializationService serializationService, string saveDirectory)
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
        try
        {
            var appConfig = _serializationService.DeserializeAppConfiguration(json);
            return appConfig ?? new AppConfiguration();
        }
        catch (Exception e)
        {
            if (File.Exists(path))
                File.Delete(path);
            return new AppConfiguration();
        }
    }
}