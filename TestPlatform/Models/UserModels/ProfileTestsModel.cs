using TestPlatform.Domain.Entity;

namespace TestPlatform.Models.UserModels;

public class ProfileTestsModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String       Login              { get; set; } = null!;
    public String       RealName           { get; set; } = null!;
    public String Role { get; set; }
    public String       Email              { get; set; } = null!;
    public String?      Avatar             { get; set; } = null!;
    public bool IsPersonal { get; set; }
    public String? EditingTestId { get; set; } = Guid.NewGuid().ToString();
    public String EditingTestName { get; set; }
    public String? EditingTestDescription { get; set; }
    public String EditingTestIcon { get; set; }
    public Question EditingQuestion { get; set; } = null!;
    public List<MentorTestModel> Tests { get; set; } = null!;
    public List<Question> Questions { get; set; } = null!;
    public List<QuestionAnswer> Answers { get; set; } = null!;


    public ProfileTestsModel(User user)
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

    public ProfileTestsModel()
    {
        Login = RealName = Email = null!;
    }
}