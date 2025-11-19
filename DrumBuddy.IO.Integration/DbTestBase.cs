using DrumBuddy.Core.Services;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Models;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.IO.Integration;

public abstract class DbTestBase : IDisposable
{
    protected readonly DrumBuddyDbContext Db;
    protected readonly SerializationService SerializationService;
    protected readonly string UserId;

    protected DbTestBase()
    {
        SerializationService = new SerializationService();
        var options = new DbContextOptionsBuilder<DrumBuddyDbContext>()
            .UseSqlite("Filename=:memory:")
            .EnableSensitiveDataLogging()
            .Options;

        Db = new DrumBuddyDbContext(options);
        Db.Database.OpenConnection();
        Db.Database.EnsureCreated();
        UserId = Guid.NewGuid().ToString();
        Db.Users.Add(new User { Id = UserId, UserName = "user1" });
        Db.SaveChanges();
    }

    public void Dispose()
    {
        Db.Database.CloseConnection();
        Db.Dispose();
    }
}