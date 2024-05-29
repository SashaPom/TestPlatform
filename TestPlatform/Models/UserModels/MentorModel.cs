using TestPlatform.Domain.Entity;

namespace TestPlatform.Models.UserModels;

public class MentorModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String Realname { get; set; } = null!;
    public String Login { get; set; } = null!;
    public String Avatar { get; set; } = null!;
    public String Role { get; set; } = null!;
}