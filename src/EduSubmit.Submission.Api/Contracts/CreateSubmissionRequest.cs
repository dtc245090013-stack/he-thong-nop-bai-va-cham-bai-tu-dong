namespace EduSubmit.Submission.Api.Contracts;

public sealed record CreateSubmissionRequest(
    string? StudentCode,
    string? RepositoryUrl);
