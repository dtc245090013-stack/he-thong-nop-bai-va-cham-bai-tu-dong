using EduSubmit.Submission.Api.Contracts;
using EduSubmit.Submission.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSubmit.Submission.Api.Endpoints;

public static class AssignmentEndpoints
{
    public static IEndpointRouteBuilder MapAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assignments", async (AppDbContext db, CancellationToken cancellationToken) =>
        {
            var assignments = await db.Assignments
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new AssignmentResponse(
                    x.Id,
                    x.Title,
                    x.Description,
                    x.DueDate,
                    x.CreatedAt))
                .ToListAsync(cancellationToken);

            return Results.Ok(assignments);
        })
        .WithName("GetAssignments")
        .WithTags("Assignments")
        .WithSummary("Lấy danh sách bài tập");

        return app;
    }
}
