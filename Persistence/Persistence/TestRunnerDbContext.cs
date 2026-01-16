using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class TestRunnerDbContext : DbContext
{
    public DbSet<TestLog> TestLogs { get; set; }

    public TestRunnerDbContext(DbContextOptions<TestRunnerDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=test_logs.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TestRunId).IsRequired();
            entity.Property(e => e.TestName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TestType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Command).HasMaxLength(500);
            entity.Property(e => e.ExpectedResponse).HasMaxLength(1000);
            entity.Property(e => e.ActualResponse).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.Timestamp).IsRequired();
        });
    }
}
