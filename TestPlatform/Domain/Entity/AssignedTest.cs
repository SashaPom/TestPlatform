namespace TestPlatform.Domain.Entity;

public class AssignedTest
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? MentorId { get; set; } = Guid.NewGuid().ToString();
    public String? StudentId { get; set; } = Guid.NewGuid().ToString();
    public String? TestId { get; set; } = Guid.NewGuid().ToString();
    public bool IsPassed { get; set; } 
}