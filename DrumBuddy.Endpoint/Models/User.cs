using DrumBuddy.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace DrumBuddy.Endpoint.Models;

public class User : IdentityUser
{
    public ICollection<SheetOnServer> Sheets { get; set; } = new List<SheetOnServer>();
}