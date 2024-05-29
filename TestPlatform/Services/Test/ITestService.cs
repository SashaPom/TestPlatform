using TestPlatform.Models.TestModels;

namespace TestPlatform.Services.Test;

public interface ITestService
{
    public String SerializeQuestions(List<QuestionModel> questions);
    public List<QuestionModel> DeserializeQuestions(string serializedQuestions);
}