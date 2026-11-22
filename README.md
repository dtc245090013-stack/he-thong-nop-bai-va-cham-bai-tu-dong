# EduSubmit – Tuần 1 (Chương 2: RESTful API)

Hệ thống nộp bài và chấm bài tự động – nền tảng C# / .NET 10, ASP.NET Core Minimal API và SQL Server.

## Phạm vi hoàn thành trong Tuần 1

- ASP.NET Core **Minimal API** trên .NET 10.
- SQL Server chạy bằng Docker Compose để máy phát triển không cần cài SQL Server trực tiếp.
- Entity Framework Core 10 và Code First Migrations.
- Các API nghiệp vụ tối thiểu của đề tài:
  - `GET /api/assignments`
  - `POST /api/assignments/{id}/submissions`
  - `GET /api/submissions/{id}`
  - `GET /api/students/{studentId}/submissions`
- Kiểm tra dữ liệu đầu vào, HTTP status code, logging cơ bản.
- Có dữ liệu mẫu cho sinh viên và bài tập khi khởi động lần đầu.

> Tuần 1 chưa triển khai RabbitMQ, Worker, SignalR, GraphQL, gRPC hoặc React. Các nội dung này được thực hiện ở những tuần/chương tiếp theo theo lộ trình học phần.

## 1. Yêu cầu môi trường

- .NET 10 SDK.
- Git for Windows.
- Docker Desktop chạy với WSL 2 backend và Linux containers.
- PowerShell 7 được khuyến nghị.

## 2. Khởi động SQL Server

Tại thư mục gốc `EduSubmit`:

```powershell
Copy-Item .env.example .env
notepad .env
```

Mở `.env`, đổi `MSSQL_SA_PASSWORD` thành mật khẩu mạnh riêng của bạn. Không commit file `.env` lên GitHub.

Khởi chạy SQL Server:

```powershell
docker compose up -d
```

Kiểm tra container:

```powershell
docker compose ps
docker compose logs --tail 50 sqlserver
```

Chờ log SQL Server cho biết dịch vụ đã sẵn sàng nhận kết nối. Cổng máy Windows là `1433`.

## 3. Cấu hình connection string cho Submission API

Trong cùng cửa sổ PowerShell sẽ chạy API, nhập:

```powershell
$env:ConnectionStrings__EduSubmit = "Server=localhost,1433;Database=EduSubmitDb;User Id=sa;Password=THAY_BANG_MAT_KHAU_TRONG_FILE_ENV;TrustServerCertificate=True;Encrypt=False"
```

Thay `THAY_BANG_MAT_KHAU_TRONG_FILE_ENV` bằng đúng mật khẩu trong `.env`. Đây là biến môi trường chỉ tồn tại trong cửa sổ PowerShell hiện tại, không được lưu vào mã nguồn.

## 4. Restore package và tạo database migration

Chạy từ thư mục gốc:

```powershell
dotnet restore .\EduSubmit.sln
```

Kiểm tra Entity Framework CLI:

```powershell
dotnet ef --version
```

Nếu chưa cài `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
```

Nếu đã cài một phiên bản khác, cập nhật:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.15
```

Tạo migration đầu tiên và cập nhật database:

```powershell
dotnet ef migrations add InitialCreate --project .\src\EduSubmit.Submission.Api\EduSubmit.Submission.Api.csproj

dotnet ef database update --project .\src\EduSubmit.Submission.Api\EduSubmit.Submission.Api.csproj
```

Migration được tạo trong thư mục `src/EduSubmit.Submission.Api/Migrations`. Hãy commit thư mục này lên GitHub vì đây là lịch sử thay đổi cấu trúc database.

## 5. Build và chạy API

```powershell
dotnet build .\EduSubmit.sln
```

Có hai cách chạy API.

### Cách A – Chạy API trực tiếp bằng .NET

Giữ biến môi trường connection string đã thiết lập ở bước 3, sau đó chạy:

```powershell
dotnet run --project .\src\EduSubmit.Submission.Api\EduSubmit.Submission.Api.csproj
```

Project có `launchSettings.json` cấu hình `http://localhost:5001`.

### Cách B – Chạy API trong Docker

Dừng tiến trình API chạy trực tiếp trên máy (nếu đang chạy) để giải phóng cổng `5001`. Sau khi đã tạo migration (bước 4) và đã tạo file `.env` (bước 2), chạy:

```powershell
docker compose up -d --build
```

Compose sẽ chạy SQL Server và Submission API. Trong network nội bộ, API kết nối tới SQL Server bằng service name `sqlserver`; trên máy Windows, API được công bố tại cổng `5001`.

Kiểm tra:

```powershell
docker compose ps
docker compose logs -f submission-api
```

Kết quả mong đợi khi API sẵn sàng:

```text
Now listening on: http://localhost:5001
Application started.
```

Trong môi trường Development, OpenAPI JSON có tại:

- `http://localhost:5001/openapi/v1.json`

## 6. API và ví dụ kiểm thử

### 6.1. Lấy danh sách bài tập

```http
GET http://localhost:5001/api/assignments
```

Trả về `200 OK` và danh sách bài tập mẫu.

### 6.2. Nộp bài

```http
POST http://localhost:5001/api/assignments/1/submissions
Content-Type: application/json

{
  "studentCode": "SV001",
  "repositoryUrl": "https://github.com/example/student-assignment"
}
```

Trả về `201 Created` cùng thông tin bài nộp. `studentCode` phải tồn tại trong bảng Students; dữ liệu mẫu gồm `SV001` và `SV002`.

### 6.3. Tra cứu bài nộp

```http
GET http://localhost:5001/api/submissions/1
```

Trả về `200 OK` nếu tìm thấy, hoặc `404 Not Found` nếu không tồn tại.

### 6.4. Tra cứu bài nộp theo sinh viên

```http
GET http://localhost:5001/api/students/1/submissions
```

`studentId` là khóa số nguyên trong bảng Students. Có thể lấy ID qua phần `studentId` trong phản hồi của API nộp bài.

### 6.5. Health endpoint

```http
GET http://localhost:5001/health
```

Endpoint này xác nhận tiến trình API đang phản hồi; chưa phải readiness check cho SQL Server.

## 7. Chạy smoke test bằng PowerShell

Khi API đang chạy, mở cửa sổ PowerShell khác tại thư mục gốc và thực hiện:

```powershell
.\tests\SmokeTest.ps1
```

Hoặc mở `tests/requests.http` trong VS Code với REST Client để gửi từng request.

## 8. Dừng các container

```powershell
docker compose down
```

Dữ liệu database nằm trong named volume `edusubmit_sqlserver_data`, vì vậy lệnh trên không xóa dữ liệu.

> Không dùng `docker compose down -v` trừ khi bạn chủ ý xóa toàn bộ dữ liệu SQL Server phát triển.

## 9. Ghi chú thiết kế

- API dùng Minimal API (`MapGet`, `MapPost`), không dùng Controller.
- `Submission` có trạng thái ban đầu `Submitted`; việc chấm điểm tự động sẽ được bổ sung cùng RabbitMQ và Grading Worker ở Chương 6.
- Điểm (`Score`) và nhận xét (`Feedback`) có thể null trước khi chấm.
- Dữ liệu mẫu chỉ được thêm khi bảng tương ứng chưa có dữ liệu.
- Mỗi lần ứng dụng chạy ở Development, EF Core áp dụng các migration đã có và kiểm tra dữ liệu mẫu.
- Không lưu mật khẩu thật trong `appsettings.json`, source code hoặc GitHub.

## 10. Commit Tuần 1 lên GitHub

Sau khi đã build, migration và smoke test thành công, kiểm tra `git status` để chắc chắn file `.env` không xuất hiện trong danh sách. Sau đó:

```powershell
git add .
git status
git commit -m "feat(ch02): complete Week 1 REST API and SQL Server"
git push -u origin main
```

Nếu repository chưa có remote `origin`, thêm remote bằng URL repository của bạn trước khi push:

```powershell
git remote add origin https://github.com/OWNER/REPOSITORY.git
```
