using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TestPlatform.Domain.Entity;
using TestPlatform.Services.Validation;

namespace TestPlatform.Models.UserModels;

public class ProfileModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String Login { get; set; } = null!;
    public String RealName { get; set; } = null!;
    public String Role { get; set; }
    public String Email { get; set; } = null!;
    public String? Avatar { get; set; } = null!;
    public bool IsPersonal { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmNewPassword { get; set; }
    public int Students { get; set; }

    public ProfileModel(User user)
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

    public ProfileModel()
    {
        Login = RealName = Email = null!;
    }
}
