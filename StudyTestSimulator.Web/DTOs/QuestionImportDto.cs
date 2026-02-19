namespace StudyTestSimulator.Web.DTOs;

public class QuestionImportDto
{
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string? ImageBase64 { get; set; }
    public string? ImageUrl { get; set; }
    public string? Explanation { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public class AnswerDto
{
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
}
