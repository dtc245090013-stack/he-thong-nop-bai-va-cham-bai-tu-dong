namespace EduSubmit.Submission.Api.Models;

public sealed class Submission
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public required string RepositoryUrl { get; set; }
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
}
