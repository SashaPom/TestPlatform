﻿namespace TestPlatform.Domain.Entity;

public class User
{
    public String? Id { get; set; } = Guid.NewGuid().ToString();
    public String Login { get; set; } = null!;
    public String RealName { get; set; } = null!;
    public String Email { get; set; } = null!;
    public String? EmailCode { get; set; } = null!;
    public String PasswordHash { get; set; } = null!;
    public String PasswordSalt { get; set; } = null!;
    public String? Avatar { get; set; } = null!;
    public String Role { get; set; } = "Student";
    public DateTime RegisterDt { get; set; }
    public DateTime? LastEnterDt { get; set; }


    public ICollection<Test>? Tests { get; set; } = null!;
}
