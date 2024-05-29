namespace TestPlatform.Domain.Entity;

public class QuestionAnswer
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? QuestionId { get; set; } = Guid.NewGuid().ToString();
    public String Text { get; set; } = null!;
    public Boolean IsRight { get; set; }

    public Question Question { get; set; }
}