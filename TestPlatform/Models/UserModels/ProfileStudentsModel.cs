using Microsoft.AspNetCore.Mvc.Rendering;
using TestPlatform.Domain.Entity;

namespace TestPlatform.Models.UserModels;

public class ProfileStudentsModel
{
    public String?         Id                 { get; set; } = Guid.NewGuid().ToString();
    public String       Login              { get; set; } = null!;
    public String       RealName           { get; set; } = null!;
    public String       Role { get; set; }
    public String       Email              { get; set; } = null!;
    public String?      Avatar             { get; set; } = null!;
    public bool         IsPersonal { get; set; }
    public String? TestName { get; set; } = null!;

    public List<StudentModel> Students { get; set; } = null!;
    public int SelectedOption { get; set; }
    public List<SelectListItem> Options { get; set; }
    
    public ProfileStudentsModel(User user)
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

    public ProfileStudentsModel()
    {
        Login = RealName = Email = null!;
    }
}