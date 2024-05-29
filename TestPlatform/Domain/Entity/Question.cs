namespace TestPlatform.Domain.Entity;

public class Question
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = null!;
    public String? TestId { get; set; } = Guid.NewGuid().ToString();

    public List<QuestionAnswer> Answers { get; set; }
    public Test Test { get; set; } = null!;
}