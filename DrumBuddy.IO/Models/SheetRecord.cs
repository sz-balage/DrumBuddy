using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Models;

public class SheetRecord
{
    public Guid Id { get; set; }
    public byte[] MeasureBytes { get; set; } = []; //protobuf binary
    public string Name { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty; 
    public int Tempo { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public virtual User? User { get; set; }
}