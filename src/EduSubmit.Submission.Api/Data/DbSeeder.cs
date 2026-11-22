using EduSubmit.Submission.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSubmit.Submission.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Students.AnyAsync(cancellationToken))
        {
            db.Students.AddRange(
                new Student
                {
                    StudentCode = "SV001",
                    FullName = "Nguyen Van An",
                    Email = "sv001@example.test"
                },
                new Student
                {
                    StudentCode = "SV002",
                    FullName = "Tran Thi Binh",
                    Email = "sv002@example.test"
                });
        }

        if (!await db.Assignments.AnyAsync(cancellationToken))
        {
            var now = DateTimeOffset.UtcNow;
            db.Assignments.AddRange(
                new Assignment
                {
                    Title = "Bai tap C# co ban",
                    Description = "Lam quen voi bien, kieu du lieu, dieu kien va vong lap.",
                    DueDate = now.AddDays(7),
                    CreatedAt = now
                },
                new Assignment
                {
                    Title = "Lap trinh huong doi tuong",
                    Description = "Thuc hanh class, object, dong goi va ke thua.",
                    DueDate = now.AddDays(14),
                    CreatedAt = now
                },
                new Assignment
                {
                    Title = "Phat trien ung dung phan tan",
                    Description = "Xay dung RESTful API bang ASP.NET Core Minimal API.",
                    DueDate = now.AddDays(21),
                    CreatedAt = now
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
