namespace EduSubmit.Submission.Api.Contracts;

public sealed record AssignmentResponse(
    int Id,
    string Title,
    string Description,
    DateTimeOffset DueDate,
    DateTimeOffset CreatedAt);
