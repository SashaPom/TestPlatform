namespace TestPlatform.Models.TestModels;

public class TestModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? MentorId { get; set; } = Guid.NewGuid().ToString();
    public String MentorName { get; set; } = null!;
    public String? Icon { get; set; } = null!;
    public String Name { get; set; } = null!;
    public String Description { get; set; } = null!;
    public int QuestionsCount { get; set; }
}