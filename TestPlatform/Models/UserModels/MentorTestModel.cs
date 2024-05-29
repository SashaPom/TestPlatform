using TestPlatform.Domain.Entity;
using TestPlatform.Models.TestModels;

namespace TestPlatform.Models.UserModels;

public class MentorTestModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? MentorId { get; set; } = Guid.NewGuid().ToString();
    public String? TestId { get; set; } = Guid.NewGuid().ToString();
    public String?             TestIcon           { get; set; }
    public String              TestTitle          { get; set; } = null!;
    public String              TestDescription    { get; set; } = null!;
    public String              Login              { get; set; } = null!;
    public String              RealName           { get; set; } = null!;
    public String              Role               { get; set; }
    public String?             Avatar             { get; set; }
    
    public List<QuestionModel> Questions          { get; set; } = null!;
    public List<AnswerModel>  Answers             { get; set; } = null!;


    public MentorTestModel(User user)
    {
        // object mapping 
        var userProps = user.GetType().GetProperties();
        var thisProps = this.GetType().GetProperties();
        foreach (var thisProp in thisProps)
        {
            var prop = userProps.FirstOrDefault(
                userProp => userProp.Name == thisProp.Name && 
                            userProp.PropertyType.IsAssignableTo(thisProp.PropertyType)
            );
            if (prop is not null)
            {
                thisProp.SetValue(this, prop.GetValue(user));
            }
        }
    }

    public MentorTestModel()
    {
        Login = RealName = null!;
    }
}