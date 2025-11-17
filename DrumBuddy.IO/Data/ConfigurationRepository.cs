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

    public async Task SaveConfigAsync(AppConfiguration config, string? userId, DateTime updatedAt)
    {
        var json = _serializationService.SerializeAppConfiguration(config);

        var existing = await _context.Configurations
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (existing != null)
        {
            existing.ConfigurationData = json;
            existing.LastUpdated = updatedAt;
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

    public async Task<(AppConfiguration Config, DateTime UpdatedAt)> LoadConfigAsync(string? userId)
    {
        var record = await _context.Configurations
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (record == null)
            return (new AppConfiguration(), DateTime.MinValue);

        try
        {
            var appConfig = _serializationService.DeserializeAppConfiguration(record.ConfigurationData);
            return (appConfig ?? new AppConfiguration(), record.LastUpdated);
        }
        catch
        {
            // Return default on error
            return (new AppConfiguration(), DateTime.MinValue);
        }
    }

    // Backward compatible method names
    public void SaveConfig(AppConfiguration config, string? userId, DateTime updatedAt)
    {
        SaveConfigAsync(config, userId,updatedAt).Wait();
    }

    public (AppConfiguration Config, DateTime UpdatedAt) LoadConfig(string? userId)
    {
        return LoadConfigAsync(userId).Result;
    }
}