namespace TestPlatform.Domain.Entity;

public class Journal
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? UserId { get; set; } = Guid.NewGuid().ToString();
    public String? TestId { get; set; } = Guid.NewGuid().ToString();
    public String? Result { get; set; } = null!;
    public Boolean IsPassed { get; set; }

    public Test? Test { get; set; }
}