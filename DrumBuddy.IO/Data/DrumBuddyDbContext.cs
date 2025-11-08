using DrumBuddy.IO.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.IO.Data;

public class DrumBuddyDbContext : IdentityDbContext<User>
{
    public DrumBuddyDbContext(DbContextOptions<DrumBuddyDbContext> options)
        : base(options)
    {
    }

    public DbSet<SheetRecord> Sheets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<SheetRecord>(entity =>
        {
            entity.HasKey(s => s.Id);
            
            entity.Property(s => s.Id)
                .IsRequired();
                
            entity.Property(s => s.MeasureBytes)
                .HasColumnType("bytea")
                .IsRequired();

            entity.Property(s => s.Name)
                .IsRequired();
                
            entity.Property(s => s.Description)
                .IsRequired();
                
            entity.Property(s => s.Tempo)
                .IsRequired();

            entity.Property(s => s.UserId)
                .IsRequired();

            entity.Property(s => s.LastSyncedAt)
                .IsRequired();

            entity.HasOne(s => s.User)
                .WithMany(u => u.Sheets)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(s => new { s.UserId, s.Id })
                .IsUnique();
                
            entity.HasIndex(s => new { s.UserId, s.Name })
                .IsUnique();
                
            entity.HasIndex(s => s.LastSyncedAt);
        });
    }
}