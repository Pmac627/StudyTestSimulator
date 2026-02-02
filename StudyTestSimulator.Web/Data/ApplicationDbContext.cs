using Microsoft.EntityFrameworkCore;
using StudyTestSimulator.Web.Models;

namespace StudyTestSimulator.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestCategory> TestCategories { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Answer> Answers { get; set; } = null!;
    public DbSet<TestAttempt> TestAttempts { get; set; } = null!;
    public DbSet<TestAttemptQuestion> TestAttemptQuestions { get; set; } = null!;
    public DbSet<QuestionFlag> QuestionFlags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TestCategory configuration
        modelBuilder.Entity<TestCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.HasIndex(e => e.Name);
        });

        // Question configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);

            entity.HasOne(e => e.TestCategory)
                .WithMany(c => c.Questions)
                .HasForeignKey(e => e.TestCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TestCategoryId);
            entity.HasIndex(e => e.IsActive);
        });

        // Answer configuration
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnswerText).IsRequired();

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuestionId);
        });

        // TestAttempt configuration
        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PercentageScore).HasPrecision(5, 2);

            entity.HasOne(e => e.TestCategory)
                .WithMany()
                .HasForeignKey(e => e.TestCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TestCategoryId);
            entity.HasIndex(e => e.StartTime);
        });

        // TestAttemptQuestion configuration
        modelBuilder.Entity<TestAttemptQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.TestAttempt)
                .WithMany(ta => ta.TestAttemptQuestions)
                .HasForeignKey(e => e.TestAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Question)
                .WithMany(q => q.TestAttemptQuestions)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TestAttemptId);
            entity.HasIndex(e => e.QuestionId);
        });

        // QuestionFlag configuration
        modelBuilder.Entity<QuestionFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FlaggedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FlaggedByEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ResolvedBy).HasMaxLength(200);

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Flags)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => e.IsResolved);
        });
    }
}
