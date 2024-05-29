namespace TestPlatform.Models.TestModels;

public class QuestionModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String TestId { get; set; } = Guid.NewGuid().ToString();
    public String Text { get; set; } = null!;
    public List<AnswerModel> Answers { get; set; }
    
    public String CheckedAnswer { get; set; }

    public int Index { get; set; }

}