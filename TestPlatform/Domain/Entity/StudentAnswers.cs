namespace TestPlatform.Domain.Entity;

public class StudentAnswers
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? UserId { get; set; } = Guid.NewGuid().ToString();
    public String? QuestionId { get; set; } = Guid.NewGuid().ToString();
    public String? AnswerId { get; set; } = Guid.NewGuid().ToString();
    public String? TestId { get; set; } = Guid.NewGuid().ToString();
    public bool IsRight { get; set; }
}