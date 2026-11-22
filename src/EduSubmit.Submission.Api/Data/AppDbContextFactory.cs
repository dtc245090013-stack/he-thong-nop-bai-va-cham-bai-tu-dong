using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduSubmit.Submission.Api.Data;

// Factory này cho phép dotnet-ef tạo migration mà không cần khởi chạy Web API.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__EduSubmit")
            ?? "Server=localhost,1433;Database=EduSubmitDb;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        optionsBuilder.UseSqlServer(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
