# Tuần 1 – Chương 2: RESTful API

## Mục tiêu

Hoàn thành Submission API bằng ASP.NET Core Minimal API (.NET 10), kết nối SQL Server qua EF Core và triển khai các API tối thiểu của đề tài.

## Checklist nghiệm thu

- [ ] `dotnet restore` thành công.
- [ ] SQL Server container khởi động và sẵn sàng nhận kết nối.
- [ ] Tạo migration `InitialCreate`.
- [ ] Database `EduSubmitDb` được tạo/cập nhật thành công.
- [ ] Dữ liệu mẫu SV001, SV002 và ba Assignment được seed.
- [ ] `GET /api/assignments` trả về HTTP 200.
- [ ] `POST /api/assignments/{id}/submissions` trả về HTTP 201.
- [ ] Thiếu StudentCode hoặc RepositoryUrl trả về HTTP 400.
- [ ] Assignment hoặc Student không tồn tại trả về HTTP 404.
- [ ] `GET /api/submissions/{id}` trả về bài nộp.
- [ ] `GET /api/students/{studentId}/submissions` trả về lịch sử nộp bài.
- [ ] `dotnet build` thành công.
- [ ] Smoke test thành công.
- [ ] Commit source code và thư mục Migrations lên GitHub.

## Commit gợi ý

```text
feat(ch02): add SQL Server persistence with EF Core
feat(ch02): implement assignment and submission endpoints
feat(ch02): validate submission request payload
test(ch02): add API smoke tests
 docs(ch02): add Week 1 setup and API guide
```
