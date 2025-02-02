namespace TestPlatform.Models.UserModels;

public class UserRegistrationModel
{
    public String?      Id                 { get; set; } = Guid.NewGuid().ToString();
    public String       Login              { get; set; } = null!;
    public String       RealName           { get; set; } = null!;
    public String       Email              { get; set; } = null!;
    public String       Password           { get; set; } = null!;
    public String       RepeatPassword     { get; set; } = null!;
    public IFormFile?   Avatar             { get; set; } = null!;
    public bool         IsAgree            { get; set; }
}