using DrumBuddy.Core.Models;

namespace DrumBuddy.Endpoint.Models;

public class SheetOnServer
{
    public int Id { get; set; }
    public Sheet Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}