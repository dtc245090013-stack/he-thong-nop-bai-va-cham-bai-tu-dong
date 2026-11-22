
using EduSubmit.Submission.Api.Models;
using Microsoft.EntityFrameworkCore;
using SubmissionEntity = EduSubmit.Submission.Api.Models.Submission;

namespace EduSubmit.Submission.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SubmissionEntity> Submissions => Set<SubmissionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StudentCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.HasIndex(x => x.StudentCode).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.ToTable("Assignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.DueDate).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<SubmissionEntity>(entity =>
        {
            entity.ToTable("Submissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RepositoryUrl).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.SubmittedAt).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Score).HasPrecision(5, 2);
            entity.Property(x => x.Feedback).HasMaxLength(2000);

            entity.HasOne(x => x.Assignment)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Student)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.StudentId, x.SubmittedAt });
            entity.HasIndex(x => new { x.AssignmentId, x.StudentId });
        });
    }
}
