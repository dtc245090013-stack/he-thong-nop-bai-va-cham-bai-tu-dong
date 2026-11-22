namespace EduSubmit.Submission.Api.Contracts;

public sealed record SubmissionResponse(
    int Id,
    int AssignmentId,
    string AssignmentTitle,
    int StudentId,
    string StudentCode,
    string RepositoryUrl,
    DateTimeOffset SubmittedAt,
    string Status,
    decimal? Score,
    string? Feedback);
