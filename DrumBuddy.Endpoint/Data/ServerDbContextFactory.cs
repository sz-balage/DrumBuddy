using DrumBuddy.IO.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DrumBuddy.Endpoint.Data;

/// <summary>
/// Design-time factory for SERVER (PostgreSQL) migrations
/// Used by: dotnet ef migrations add ... --project DrumBuddy.Endpoint
/// </summary>
public class ServerDbContextFactory : IDesignTimeDbContextFactory<DrumBuddyDbContext>
{
    public DrumBuddyDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Database=drumbuddy;Username=postgres;Password=postgres123";
        
        var optionsBuilder = new DbContextOptionsBuilder<DrumBuddyDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        return new DrumBuddyDbContext(optionsBuilder.Options);
    }
}