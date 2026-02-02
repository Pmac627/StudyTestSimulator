namespace StudyTestSimulator.Web.Models;

public class TestCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public required string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
