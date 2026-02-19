namespace StudyTestSimulator.Web.Models;

public class TestAttempt
{
    public int Id { get; set; }
    public int TestCategoryId { get; set; }
    public required string UserId { get; set; }
    public required string UserEmail { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal PercentageScore { get; set; }
    public bool IsCompleted { get; set; }
    public int SkippedQuestions { get; set; }
    public bool WasAbandoned { get; set; }

    // Navigation properties
    public TestCategory TestCategory { get; set; } = null!;
    public ICollection<TestAttemptQuestion> TestAttemptQuestions { get; set; } = new List<TestAttemptQuestion>();
}
