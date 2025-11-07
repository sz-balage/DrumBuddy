using System.Text.Json;
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

    public DbSet<SheetOnServer> Sheets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<SheetOnServer>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Content)
                .HasConversion(
                    sheet => JsonSerializer.Serialize(sheet, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<Sheet>(json, (JsonSerializerOptions?)null)!)
                .HasColumnType("jsonb"); 

            entity.Property(s => s.UserId)
                .IsRequired();

            entity.Property(s => s.CreatedAt)
                .IsRequired();

            entity.Property(s => s.UpdatedAt)
                .IsRequired();

            entity.HasOne(s => s.User)
                .WithMany(u => u.Sheets)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(s => new { s.UserId, s.Name })
                .IsUnique();
            entity.HasIndex(s => s.CreatedAt);
            
        });
    }
}
