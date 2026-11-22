namespace EduSubmit.Submission.Api.Models;

public sealed class Student
{
    public int Id { get; set; }
    public required string StudentCode { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
