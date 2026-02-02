namespace StudyTestSimulator.Web.Models;

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public required string AnswerText { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation properties
    public Question Question { get; set; } = null!;
}
