namespace StudyTestSimulator.Web.Models;

public class QuestionFlag
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public required string FlaggedBy { get; set; }
    public required string FlaggedByEmail { get; set; }
    public DateTime FlaggedDate { get; set; }
    public string? Comments { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedDate { get; set; }

    // Navigation properties
    public Question Question { get; set; } = null!;
}
