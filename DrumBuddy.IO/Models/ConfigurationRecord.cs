using DrumBuddy.IO.Models;

namespace DrumBuddy.IO.Models;

public class ConfigurationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ConfigurationData { get; set; } = string.Empty; 
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public string? UserId { get; set; }
    public virtual User? User { get; set; }
}