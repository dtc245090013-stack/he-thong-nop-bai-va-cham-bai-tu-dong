
using EduSubmit.Submission.Api.Contracts;
using EduSubmit.Submission.Api.Data;
using EduSubmit.Submission.Api.Models;
using Microsoft.EntityFrameworkCore;
using SubmissionEntity = EduSubmit.Submission.Api.Models.Submission;

namespace EduSubmit.Submission.Api.Endpoints;

public static class SubmissionEndpoints
{
    public static IEndpointRouteBuilder MapSubmissionEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/assignments/{assignmentId:int}/submissions",
                CreateSubmissionAsync)
            .WithName("CreateSubmission")
            .WithTags("Submissions")
            .WithSummary("Sinh viên nộp bài tập");

        app.MapGet(
                "/api/submissions/{submissionId:int}",
                GetSubmissionAsync)
            .WithName("GetSubmission")
            .WithTags("Submissions")
            .WithSummary("Tra cứu một bài nộp");

        app.MapGet(
                "/api/students/{studentId:int}/submissions",
                GetStudentSubmissionsAsync)
            .WithName("GetStudentSubmissions")
            .WithTags("Submissions")
            .WithSummary("Tra cứu bài nộp theo sinh viên");

        return app;
    }

    private static async Task<IResult> CreateSubmissionAsync(
        int assignmentId,
        CreateSubmissionRequest request,
        AppDbContext db,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var errors = ValidateRequest(request);

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var assignment = await db.Assignments
            .FirstOrDefaultAsync(
                x => x.Id == assignmentId,
                cancellationToken);

        if (assignment is null)
        {
            return Results.NotFound(new
            {
                message = $"Không tìm thấy bài tập có ID {assignmentId}."
            });
        }

        var studentCode = request.StudentCode!.Trim();

        var student = await db.Students
            .FirstOrDefaultAsync(
                x => x.StudentCode == studentCode,
                cancellationToken);

        if (student is null)
        {
            return Results.NotFound(new
            {
                message = $"Không tìm thấy sinh viên có mã {studentCode}."
            });
        }

        var submission = new SubmissionEntity
        {
            AssignmentId = assignment.Id,
            Assignment = assignment,
            StudentId = student.Id,
            Student = student,
            RepositoryUrl = request.RepositoryUrl!.Trim(),
            SubmittedAt = DateTimeOffset.UtcNow,
            Status = SubmissionStatus.Submitted
        };

        db.Submissions.Add(submission);

        await db.SaveChangesAsync(cancellationToken);

        loggerFactory.CreateLogger(
                "EduSubmit.Submission.Api.SubmissionEndpoints")
            .LogInformation(
                "Created submission {SubmissionId} for assignment {AssignmentId} by student {StudentCode}",
                submission.Id,
                assignment.Id,
                student.StudentCode);

        var response = ToResponse(
            submission,
            assignment,
            student);

        return Results.Created(
            $"/api/submissions/{submission.Id}",
            response);
    }

    private static async Task<IResult> GetSubmissionAsync(
        int submissionId,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var submission = await db.Submissions
            .AsNoTracking()
            .Include(x => x.Assignment)
            .Include(x => x.Student)
            .FirstOrDefaultAsync(
                x => x.Id == submissionId,
                cancellationToken);

        if (submission is null)
        {
            return Results.NotFound(new
            {
                message = $"Không tìm thấy bài nộp có ID {submissionId}."
            });
        }

        return Results.Ok(
            ToResponse(
                submission,
                submission.Assignment,
                submission.Student));
    }

    private static async Task<IResult> GetStudentSubmissionsAsync(
        int studentId,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var studentExists = await db.Students
            .AnyAsync(
                x => x.Id == studentId,
                cancellationToken);

        if (!studentExists)
        {
            return Results.NotFound(new
            {
                message = $"Không tìm thấy sinh viên có ID {studentId}."
            });
        }

        var submissions = await db.Submissions
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Include(x => x.Assignment)
            .Include(x => x.Student)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);

        var response = submissions
            .Select(x => ToResponse(
                x,
                x.Assignment,
                x.Student))
            .ToList();

        return Results.Ok(response);
    }

    private static Dictionary<string, string[]> ValidateRequest(
        CreateSubmissionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.StudentCode))
        {
            errors[nameof(request.StudentCode)] =
                ["StudentCode không được để trống."];
        }
        else if (request.StudentCode.Trim().Length > 50)
        {
            errors[nameof(request.StudentCode)] =
                ["StudentCode không được vượt quá 50 ký tự."];
        }

        if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
        {
            errors[nameof(request.RepositoryUrl)] =
                ["RepositoryUrl không được để trống."];
        }
        else if (request.RepositoryUrl.Trim().Length > 2048)
        {
            errors[nameof(request.RepositoryUrl)] =
                ["RepositoryUrl không được vượt quá 2048 ký tự."];
        }
        else if (!Uri.TryCreate(
                     request.RepositoryUrl.Trim(),
                     UriKind.Absolute,
                     out var uri)
                 || (uri.Scheme != Uri.UriSchemeHttp
                     && uri.Scheme != Uri.UriSchemeHttps))
        {
            errors[nameof(request.RepositoryUrl)] =
                ["RepositoryUrl phải là URL tuyệt đối bắt đầu bằng http:// hoặc https://."];
        }

        return errors;
    }

    private static SubmissionResponse ToResponse(
        SubmissionEntity submission,
        Assignment assignment,
        Student student) => new(
            submission.Id,
            assignment.Id,
            assignment.Title,
            student.Id,
            student.StudentCode,
            submission.RepositoryUrl,
            submission.SubmittedAt,
            submission.Status.ToString(),
            submission.Score,
            submission.Feedback);
}
