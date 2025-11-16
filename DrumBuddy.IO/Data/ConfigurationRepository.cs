using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Models;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.IO.Storage;

public class ConfigurationRepository
{
    private readonly DrumBuddyDbContext _context;
    private readonly SerializationService _serializationService;

    public ConfigurationRepository(DrumBuddyDbContext context, SerializationService serializationService)
    {
        _context = context;
        _serializationService = serializationService;
    }

    public async Task SaveConfigAsync(AppConfiguration config, string? userId = null)
    {
        var json = _serializationService.SerializeAppConfiguration(config);

        var existing = await _context.Configurations
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (existing != null)
        {
            existing.ConfigurationData = json;
            existing.LastUpdated = DateTime.UtcNow;
            _context.Configurations.Update(existing);
        }
        else
        {
            var record = new ConfigurationRecord
            {
                Id = Guid.NewGuid(),
                ConfigurationData = json,
                LastUpdated = DateTime.UtcNow,
                UserId = userId
            };
            _context.Configurations.Add(record);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<AppConfiguration> LoadConfigAsync(string? userId = null)
    {
        var record = await _context.Configurations
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (record == null)
            return new AppConfiguration();

        try
        {
            var appConfig = _serializationService.DeserializeAppConfiguration(record.ConfigurationData);
            return appConfig ?? new AppConfiguration();
        }
        catch
        {
            // Return default on error
            return new AppConfiguration();
        }
    }

    // Backward compatible method names
    public void SaveConfig(AppConfiguration config)
    {
        SaveConfigAsync(config).Wait();
    }

    public AppConfiguration LoadConfig()
    {
        return LoadConfigAsync().Result;
    }
}