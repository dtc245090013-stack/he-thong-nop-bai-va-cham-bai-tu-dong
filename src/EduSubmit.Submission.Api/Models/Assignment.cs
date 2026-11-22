namespace EduSubmit.Submission.Api.Models;

public sealed class Assignment
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
