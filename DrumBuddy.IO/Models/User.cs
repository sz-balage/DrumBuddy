using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;
using Microsoft.AspNetCore.Identity;

namespace DrumBuddy.IO.Models;

public class User : IdentityUser
{
    public ICollection<SheetRecord> Sheets { get; set; } = new List<SheetRecord>();
}