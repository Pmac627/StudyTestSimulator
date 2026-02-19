namespace StudyTestSimulator.Web.Models;

public class TestAttemptQuestion
{
    public int Id { get; set; }
    public int TestAttemptId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedAnswerId { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsSkipped { get; set; }
    public DateTime QuestionStartTime { get; set; }
    public DateTime? QuestionEndTime { get; set; }
    public int TimeSpentSeconds { get; set; }
    public int QuestionOrder { get; set; }

    // Navigation properties
    public TestAttempt TestAttempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
