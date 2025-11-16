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
    public DbSet<ConfigurationRecord> Configurations { get; set; }

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
            
            entity.Property(s => s.UserId).IsRequired(false);

            entity.HasOne(s => s.User)
                .WithMany(u => u.Sheets)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // FK is optional
            
            entity.HasIndex(s => new { s.UserId, s.Id })
                .IsUnique();
                
            entity.HasIndex(s => new { s.UserId, s.Name })
                .IsUnique();
                
        });
        builder.Entity<ConfigurationRecord>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.Property(c => c.Id).IsRequired();
            entity.Property(c => c.ConfigurationData).IsRequired();
            entity.Property(c => c.LastUpdated).IsRequired();
            entity.Property(c => c.UserId).IsRequired(false);

            entity.HasOne(c => c.User)
                .WithOne() // One config per user
                .HasForeignKey<ConfigurationRecord>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
            
            entity.HasIndex(c => c.UserId).IsUnique();
        });
    }
}