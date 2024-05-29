namespace TestPlatform.Models.UserModels;

public class AssignTestModel
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();    
    public String? TestId { get; set; } = Guid.NewGuid().ToString();
    public String? UserId { get; set; } = Guid.NewGuid().ToString();
}