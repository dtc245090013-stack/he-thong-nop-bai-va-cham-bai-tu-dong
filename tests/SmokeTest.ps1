$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5001"

Write-Host "[1/4] Checking API health..." -ForegroundColor Cyan
$health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
if ($health.status -ne "Healthy") {
    throw "API health check failed."
}

Write-Host "[2/4] Loading assignments..." -ForegroundColor Cyan
$assignments = @(Invoke-RestMethod -Uri "$baseUrl/api/assignments" -Method Get)
if ($assignments.Count -lt 1) {
    throw "No seeded assignments were returned."
}
$assignmentId = $assignments[0].id
Write-Host "Found $($assignments.Count) assignment(s)."

Write-Host "[3/4] Creating a submission..." -ForegroundColor Cyan
$body = @{
    studentCode = "SV001"
    repositoryUrl = "https://github.com/example/student-assignment"
} | ConvertTo-Json

$created = Invoke-RestMethod `
    -Uri "$baseUrl/api/assignments/$assignmentId/submissions" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

if (-not $created.id) {
    throw "Submission response did not contain an ID."
}
Write-Host "Created submission ID: $($created.id)"

Write-Host "[4/4] Reading the submission and student history..." -ForegroundColor Cyan
$submission = Invoke-RestMethod -Uri "$baseUrl/api/submissions/$($created.id)" -Method Get
$studentSubmissions = @(Invoke-RestMethod -Uri "$baseUrl/api/students/$($created.studentId)/submissions" -Method Get)

if ($submission.status -ne "Submitted") {
    throw "Unexpected submission status: $($submission.status)"
}
if ($studentSubmissions.Count -lt 1) {
    throw "Student submission history is empty."
}

Write-Host "All smoke tests passed." -ForegroundColor Green
Write-Host "Submission ID: $($submission.id); Student ID: $($created.studentId); Status: $($submission.status)"
