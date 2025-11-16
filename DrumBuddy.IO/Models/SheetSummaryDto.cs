namespace DrumBuddy.IO.Models;

public class SheetSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public int Tempo { get; set; }
}