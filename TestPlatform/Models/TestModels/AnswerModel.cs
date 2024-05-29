namespace TestPlatform.Models.TestModels; 

public class AnswerModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? QuestionId { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; }
    public bool IsRight { get; set; }
}