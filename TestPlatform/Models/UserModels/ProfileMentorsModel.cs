using Microsoft.AspNetCore.Mvc.Rendering;
using TestPlatform.Domain.Entity;

namespace TestPlatform.Models.UserModels;

public class ProfileMentorsModel 
{
    public String?      Id                 { get; set; } = Guid.NewGuid().ToString();
    public String       Login              { get; set; } = null!;
    public String       RealName           { get; set; } = null!;
    public String       Role { get; set; }
    public String       Email              { get; set; } = null!;
    public String?      Avatar             { get; set; } = null!;
    public bool         IsPersonal { get; set; }

    public List<MentorModel> Mentors { get; set; } = null!;
    
    public ProfileMentorsModel(User user)
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

    public ProfileMentorsModel()
    {
        Login = RealName = Email = null!;
    }
}