using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Styling;

namespace DrumBuddy.Services;

public class ThemePreferenceService
{
    private readonly string _preferencesPath;
    private bool _initialized;

    public ThemePreferenceService()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DrumBuddy"
        );

        Directory.CreateDirectory(appDataDir);
        _preferencesPath = Path.Combine(appDataDir, "theme_preference.json");
    }

    public ThemePreference Preference { get; set; } = new();

    public void Initialize()
    {
        if (_initialized) return;

        try
        {
            if (File.Exists(_preferencesPath))
            {
                var json = File.ReadAllText(_preferencesPath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    var preference = JsonSerializer.Deserialize<ThemePreference>(json);
                    Preference = preference ?? new ThemePreference();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load theme preference: {e.Message}");
            Preference = new ThemePreference();
        }
        finally
        {
            _initialized = true;
        }
    }

    public async Task SavePreferenceAsync(ThemeMode mode)
    {
        try
        {
            Preference = new ThemePreference
            {
                Mode = mode
            };

            var directory = Path.GetDirectoryName(_preferencesPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Preference, options);
            await File.WriteAllTextAsync(_preferencesPath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to save theme preference: {e.Message}");
        }
    }

    public ThemeVariant? ResolveThemeVariant()
    {
        return Preference.Mode switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => null
        };
    }
}

public enum ThemeMode
{
    Light,
    Dark
}

public class ThemePreference
{
    [JsonPropertyName("mode")] public ThemeMode Mode { get; set; } = ThemeMode.Light;
}