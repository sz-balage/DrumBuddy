using DrumBuddy.IO.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DrumBuddy.Data;

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