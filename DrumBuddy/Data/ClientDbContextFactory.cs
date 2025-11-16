using DrumBuddy.IO.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DrumBuddy.Data;

/// <summary>
/// Design-time factory for CLIENT (SQLite) migrations
/// Used by: dotnet ef migrations add ... --project DrumBuddy.IO
/// </summary>
public class ClientDbContextFactory : IDesignTimeDbContextFactory<DrumBuddyDbContext>
{
    public DrumBuddyDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Data Source=sheet_db.db;";
        
        var optionsBuilder = new DbContextOptionsBuilder<DrumBuddyDbContext>();
        optionsBuilder.UseSqlite(connectionString);
        
        return new DrumBuddyDbContext(optionsBuilder.Options);
    }
}