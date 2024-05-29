using TestPlatform.Domain.Entity;

namespace TestPlatform.Models.UserModels;

public class QuestionEditingModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? TestId { get; set; } = Guid.NewGuid().ToString();    
    public string Text { get; set; } = string.Empty;
    public List<QuestionAnswer> Answers { get; set; } = new();
}