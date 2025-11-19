using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Storage;
using Shouldly;

namespace DrumBuddy.IO.Integration;

public class ConfigurationRepositoryTests : DbTestBase
{
    [Fact]
    public async Task SaveAndLoadConfig_ShouldRoundTrip()
    {
        var repo = new ConfigurationRepository(Db, new SerializationService());

        var config = new AppConfiguration
        {
            MetronomeVolume = 1234
        };

        await repo.UpdateConfigAsync(config, UserId, DateTime.UtcNow);

        var (loaded, _) = await repo.LoadConfigAsync(UserId);

        loaded.MetronomeVolume.ShouldBe(1234);
    }
}