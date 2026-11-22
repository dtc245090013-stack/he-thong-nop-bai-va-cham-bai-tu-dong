# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/EduSubmit.Submission.Api/EduSubmit.Submission.Api.csproj", "src/EduSubmit.Submission.Api/"]
RUN dotnet restore "src/EduSubmit.Submission.Api/EduSubmit.Submission.Api.csproj"

COPY . .
WORKDIR /src/src/EduSubmit.Submission.Api
RUN dotnet publish "EduSubmit.Submission.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "EduSubmit.Submission.Api.dll"]
