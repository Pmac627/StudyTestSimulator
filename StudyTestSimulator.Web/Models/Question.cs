namespace StudyTestSimulator.Web.Models;

public class Question
{
    public int Id { get; set; }
    public int TestCategoryId { get; set; }
    public required string QuestionText { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImageUrl { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public required string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public TestCategory TestCategory { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<TestAttemptQuestion> TestAttemptQuestions { get; set; } = new List<TestAttemptQuestion>();
    public ICollection<QuestionFlag> Flags { get; set; } = new List<QuestionFlag>();
}
