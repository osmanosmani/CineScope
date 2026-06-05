# Deployment Guide

This guide explains how CineScope can be prepared for Azure deployment.

## Local Configuration

Local database connection:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CineScopeDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

TMDB settings:

```json
"Tmdb": {
  "ApiKey": "",
  "BaseUrl": "https://api.themoviedb.org/3",
  "ImageBaseUrl": "https://image.tmdb.org/t/p/w500",
  "Language": "en-US"
}
```

For local development, use user secrets for the TMDB key:

```powershell
dotnet user-secrets set "Tmdb:ApiKey" "YOUR_TMDB_API_KEY"
```

## Azure App Service Configuration

In Azure App Service, do not store secrets directly in source code. Configure them in:

```text
Azure Portal > App Service > Settings > Environment variables
```

Recommended settings:

```text
ConnectionStrings__DefaultConnection = production SQL Server connection string
Tmdb__ApiKey = production TMDB API key
Tmdb__BaseUrl = https://api.themoviedb.org/3
Tmdb__ImageBaseUrl = https://image.tmdb.org/t/p/w500
Tmdb__Language = en-US
```

For Azure SQL, use the connection string from the Azure SQL Database connection strings panel.

## Database Migration Strategy

For school demo deployment, the simplest option is to run migrations manually:

```powershell
dotnet ef database update
```

For production-style deployment, migrations can be applied during a controlled deployment step or from a CI/CD pipeline.

## Publish from Visual Studio

General flow:

1. Right-click the project.
2. Select `Publish`.
3. Choose `Azure`.
4. Select or create an Azure App Service.
5. Choose `Release` configuration.
6. Publish the application.
7. Configure App Service environment variables.
8. Run migrations against the production database.
9. Open the Azure App Service URL and test login.

Microsoft reference:

- https://learn.microsoft.com/en-us/aspnet/core/tutorials/publish-to-azure-webapp-using-vs
- https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/

## GitHub Actions Deployment

A future CI/CD workflow could:

1. Restore NuGet packages.
2. Build the project.
3. Run tests when tests are added.
4. Publish the ASP.NET Core app.
5. Deploy to Azure App Service.

GitHub Actions should use repository secrets for publish credentials and API keys.

Microsoft reference:

- https://learn.microsoft.com/en-us/azure/app-service/deploy-github-actions

## SignalR Deployment Note

CineScope uses simple in-app SignalR. For one App Service instance, this is enough for a school project.

If the app is scaled out to multiple instances, consider Azure SignalR Service.

Microsoft reference:

- https://learn.microsoft.com/en-us/aspnet/core/signalr/publish-to-azure-web-app

## Pre-Deployment Checklist

- Build succeeds in Release mode.
- `appsettings.json` does not contain real secrets.
- Production connection string is configured in Azure.
- TMDB API key is configured in Azure.
- Database migrations are applied.
- Admin login works.
- Movie browsing works.
- TMDB import works.
- Dashboard loads.
- About Me page presents the project clearly.
