using Microsoft.EntityFrameworkCore;

using TestPlatform.Domain.Entity;

namespace TestPlatform.Domain
{
    public class TestPlatrormDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionAnswer> Answers { get; set; }
        public DbSet<StudentAnswers> StudentAnswers { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<AssignedTest> AssignedTests { get; set; }

        public TestPlatrormDbContext(DbContextOptions<TestPlatrormDbContext> options)
            : base(options) 
        { 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Test>()
            .HasOne(t => t.Mentor)
            .WithMany(u => u.Tests)
            .HasForeignKey(t => t.MentorId);

            modelBuilder.Entity<Journal>().HasOne(j => j.Test)
                .WithMany(t => t.Journals)
                .HasForeignKey(j => j.TestId);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId);
            modelBuilder.Entity<Test>()
                .HasMany(t => t.Questions)
                .WithOne(q => q.Test)
                .HasForeignKey(q => q.TestId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
