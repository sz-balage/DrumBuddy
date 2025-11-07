using DrumBuddy.Core.Models;
using DrumBuddy.Endpoint.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.Endpoint.Data;

public class DrumBuddyDbContext : IdentityDbContext<User>
{
    public DrumBuddyDbContext(DbContextOptions<DrumBuddyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sheet> Sheets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SheetOnServer>()
            .HasOne(s => s.User)
            .WithMany(u => u.Sheets)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
