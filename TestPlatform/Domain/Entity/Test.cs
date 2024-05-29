namespace TestPlatform.Domain.Entity;

public class Test
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String? MentorId { get; set; } = Guid.NewGuid().ToString();
    public String? Icon { get; set; } = null!;
    public String? Name { get; set; } = null!;
    public String? Description { get; set; } = null!;

    public User? Mentor { get; set; }
    public List<Question> Questions { get; set; }
    public ICollection<Journal> Journals { get; set; }
}