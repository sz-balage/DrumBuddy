using Microsoft.AspNetCore.Identity;

namespace DrumBuddy.IO.Models;

public class User : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public ICollection<SheetRecord> Sheets { get; set; } = new List<SheetRecord>();
}