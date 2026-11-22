using EduSubmit.Submission.Api.Data;
using EduSubmit.Submission.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI mô tả các endpoint để công cụ kiểm thử có thể đọc được hợp đồng API.
builder.Services.AddOpenApi();

// Connection string được lấy từ cấu hình, ưu tiên biến môi trường
// ConnectionStrings__EduSubmit. Không ghi mật khẩu thật vào source code.
var connectionString = builder.Configuration.GetConnectionString("EduSubmit");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing connection string 'EduSubmit'. Set environment variable ConnectionStrings__EduSubmit.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Liveness endpoint: xác nhận tiến trình API đang hoạt động.
app.MapGet("/health", () => Results.Ok(new
{
    service = "EduSubmit.Submission.Api",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapAssignmentEndpoints();
app.MapSubmissionEndpoints();

// Chỉ tự áp dụng migration và seed dữ liệu khi chạy Development.
// Migration đầu tiên phải được tạo bằng `dotnet ef migrations add InitialCreate`.
if (app.Environment.IsDevelopment())
{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("EduSubmit.DatabaseStartup");
    await InitializeDatabaseAsync(app.Services, startupLogger);
}

app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider services, ILogger logger)
{
    const int maxAttempts = 10;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            await DbSeeder.SeedAsync(db);
            logger.LogInformation("Database migration and sample data initialization completed.");
            return;
        }
        catch (Exception exception)
        {
            if (attempt == maxAttempts)
            {
                throw;
            }

            logger.LogWarning(
                exception,
                "Database initialization attempt {Attempt}/{MaxAttempts} failed. Retrying in 3 seconds.",
                attempt,
                maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

// Giữ Program public để có thể dùng WebApplicationFactory trong các bài kiểm thử sau này.
public partial class Program { }
