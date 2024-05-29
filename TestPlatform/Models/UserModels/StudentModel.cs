using TestPlatform.Domain.Entity;

namespace TestPlatform.Models.UserModels;

public class StudentModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String Realname { get; set; } = null!;
    public String Login { get; set; } = null!;
    public String Avatar { get; set; } = null!;
    public String Role { get; set; } = null!;
    public String? AssignTestId { get; set; }
    public bool IsTestAssigned { get; set; }
    public List<Test> Tests { get; set; } = null!;
}