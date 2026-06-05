# Deployment Guide

This guide explains how to deploy CineScope to Microsoft Azure using Azure App Service and Azure SQL Database.

## Recommended Azure Architecture

```text
User Browser -> Azure App Service -> Azure SQL Database
                      |
                      -> TMDB API
```

Azure deployment target used for the school demo:

- Resource Group: `rg-cinescope-school`
- Azure SQL Server: `sql-cinescope-osman`
- Azure SQL Database: `CineScopeDb`
- Azure App Service Plan: `asp-cinescope-school`
- Azure App Service: `app-cinescope-osman`

If an Azure resource name is already taken, add a short suffix such as your initials or the year.

## Project Configuration Check

CineScope is already prepared for Azure-style configuration:

- `Program.cs` registers `ApplicationDbContext`.
- EF Core uses SQL Server.
- The connection string is read by name:

```csharp
builder.Configuration.GetConnectionString("DefaultConnection")
```

- `appsettings.json` contains only the local development connection string.
- Azure production secrets should be configured in Azure App Service, not committed to GitHub.

This means the Azure connection string must be named:

```text
DefaultConnection
```

## Local Configuration

The local database connection remains in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CineScopeDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

TMDB settings are also in `appsettings.json`, but the API key should stay empty in source control:

```json
"Tmdb": {
  "ApiKey": "",
  "BaseUrl": "https://api.themoviedb.org/3",
  "ImageBaseUrl": "https://image.tmdb.org/t/p/w500",
  "Language": "en-US"
}
```

For local development, store the TMDB key with user secrets:

```powershell
dotnet user-secrets set "Tmdb:ApiKey" "YOUR_TMDB_API_KEY"
```

Run this from:

```text
C:\Users\OSI\source\repos\Lectures\CineScope
```

## Azure SQL Database Setup

In the Azure Portal:

1. Search for `Resource groups`.
2. Create a resource group named `rg-cinescope-school`.
3. Search for `SQL databases`.
4. Create a new database named `CineScopeDb`.
5. Create a new SQL Server named `sql-cinescope-osman`.
6. Choose the closest available region, for example `Sweden Central`, `North Europe`, or `West Europe`.
7. Create a SQL admin username, for example `cinescopeadmin`.
8. Create a strong password and save it locally outside the project.
9. Choose a low-cost database option suitable for a school project.
10. Create the database.

Firewall:

1. Open the SQL Server resource `sql-cinescope-osman`.
2. Go to `Security > Networking`.
3. Add your current client IP so your computer can run migrations.
4. Enable `Allow Azure services and resources to access this server` if the App Service needs public Azure access to the database.
5. Save the firewall settings.

Connection string:

1. Open `CineScopeDb`.
2. Go to `Settings > Connection strings`.
3. Choose the ADO.NET connection string.
4. Replace `{your_username}` and `{your_password}` with the SQL admin credentials.
5. Do not commit this connection string to GitHub.

Example format:

```text
Server=tcp:sql-cinescope-osman.database.windows.net,1433;Initial Catalog=CineScopeDb;Persist Security Info=False;User ID=<sql-admin-user>;Password=<sql-admin-password>;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Azure App Service Setup

In the Azure Portal:

1. Search for `App Services`.
2. Create a new Web App.
3. Resource Group: `rg-cinescope-school`.
4. Name: `app-cinescope-osman`.
5. Publish: `Code`.
6. Runtime stack: `.NET`.
7. Operating system: `Windows`.
8. Region: same region as the database if possible.
9. App Service Plan: create `asp-cinescope-school`.
10. Pricing plan: choose a low-cost student/demo option. Use the portal pricing screen to confirm current cost.
11. Create the Web App.

Runtime note:

CineScope currently targets `net10.0`. If Azure App Service does not show a compatible .NET runtime in your selected region/plan, either choose a supported .NET 10 runtime option or retarget the project to .NET 8 LTS before publishing.

## Configure App Service Environment Variables

Open the Web App:

```text
App Service > app-cinescope-osman > Settings > Environment variables
```

Add connection string:

```text
Tab: Connection strings
Name: DefaultConnection
Value: Azure SQL connection string
Type: SQLAzure
```

The name must be exactly `DefaultConnection`, because `Program.cs` uses:

```csharp
builder.Configuration.GetConnectionString("DefaultConnection")
```

Add application settings:

```text
Tab: App settings
Name: Tmdb__ApiKey
Value: YOUR_TMDB_API_KEY

Name: Tmdb__BaseUrl
Value: https://api.themoviedb.org/3

Name: Tmdb__ImageBaseUrl
Value: https://image.tmdb.org/t/p/w500

Name: Tmdb__Language
Value: en-US
```

Optional:

```text
Name: ASPNETCORE_ENVIRONMENT
Value: Production
```

Save the settings and restart the App Service.

## Entity Framework Core Migrations

Current migrations:

- `InitialCreate`
- `AddIdentityRoles`
- `AddReviewsFavorites`

If migrations did not exist, the commands would be:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

For CineScope, migrations already exist. To apply them to Azure SQL, run this from the project folder:

```powershell
dotnet ef database update --connection "Server=tcp:sql-cinescope-osman.database.windows.net,1433;Initial Catalog=CineScopeDb;Persist Security Info=False;User ID=<sql-admin-user>;Password=<sql-admin-password>;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

Important:

- Do not paste the real password into documentation or GitHub.
- Your current IP must be allowed in Azure SQL firewall.
- Run migrations before the final production test. CineScope seeds Admin and Member roles on startup, so the Identity tables must exist.

## Publish From Visual Studio

In Visual Studio:

1. Open the CineScope solution.
2. Right-click the CineScope project.
3. Select `Publish`.
4. Choose `Azure`.
5. Choose `Azure App Service (Windows)`.
6. Select existing App Service: `app-cinescope-osman`.
7. Use `Release` configuration.
8. Publish.
9. Open the live URL.
10. Test the app.

Do not put the Azure SQL connection string into `appsettings.json`. Keep it in App Service Environment variables.

## Alternative Publish From CLI

Run from the project folder:

```powershell
dotnet restore
dotnet build -c Release
dotnet publish -c Release -o .\publish
```

The published output will be in:

```text
C:\Users\OSI\source\repos\Lectures\CineScope\publish
```

For manual zip deployment, create a zip from the contents of the `publish` folder and upload it to Azure App Service using Visual Studio, Azure Portal deployment tools, or Azure CLI.

Example Azure CLI flow after Azure CLI login:

```powershell
az login
az webapp deploy --resource-group rg-cinescope-school --name app-cinescope-osman --src-path .\publish.zip --type zip
```

Use Visual Studio publish first unless CLI deployment is required.

## After Deployment Testing

Test these pages and workflows:

1. Homepage: `/`
2. Movies page: `/Movies`
3. Movie details: `/Movies/Details/{id}`
4. Register a Member account.
5. Login and logout.
6. Admin login:

```text
Email: admin@cinescope.com
Password: Admin123!
```

7. Admin creates, edits, and deletes a movie.
8. Member adds a review and rating.
9. Member adds and removes a favorite.
10. Admin opens `/ExternalMovies/Search` and tests TMDB search/import.
11. Admin opens `/Admin` and checks dashboard statistics.
12. Check that Bootstrap styling and images load correctly.

## Common Azure Errors And Fixes

### Database connection failed

Check:

- App Service connection string exists.
- Name is exactly `DefaultConnection`.
- Type is `SQLAzure`.
- Azure SQL username/password are correct.
- Firewall allows Azure services or the App Service outbound connection.

### Login works locally but not on Azure

Most likely cause:

- Identity tables were not created in Azure SQL.

Fix:

```powershell
dotnet ef database update --connection "<Azure SQL connection string>"
```

### Migration not applied

Check Azure SQL tables. Identity tables should include:

- `AspNetUsers`
- `AspNetRoles`
- `AspNetUserRoles`

Project tables should include:

- `Movies`
- `Reviews`
- `Favorites`

### App Service 500 error

Check:

- App Service logs.
- Connection string.
- Missing migrations.
- Wrong runtime version.
- TMDB setting if opening external movie pages.

### Missing connection string

Make sure the connection string is under:

```text
App Service > Settings > Environment variables > Connection strings
```

Not only under regular App settings.

### Firewall issue with Azure SQL

If migrations fail from your computer, add your current IP to SQL Server firewall.

If the deployed app cannot connect, enable Azure service access or configure a more secure private networking setup later.

### Wrong .NET runtime

CineScope targets `net10.0`. In Azure:

1. Open App Service.
2. Go to `Development Tools > Advanced Tools`.
3. Open Kudu.
4. Run:

```powershell
dotnet --info
```

If .NET 10 is unavailable, retarget CineScope to .NET 8 LTS or publish self-contained.

### Static files or CSS not loading

Check:

- `app.UseStaticFiles()` exists in `Program.cs`.
- `wwwroot` files were included in publish.
- Browser cache is cleared.
- App Service deployment contains `wwwroot/css/site.css`.

## Final School Submission

Include:

- Live Azure URL.
- GitHub repository link.
- README with screenshots.
- Short Azure architecture explanation.
- Demo video.
- Documentation files from the `Docs` folder.

Short architecture text:

```text
User Browser -> Azure App Service -> Azure SQL Database
```

TMDB import flow:

```text
Admin -> CineScope App Service -> TMDB API -> Local Azure SQL Movies table
```

## References

- https://learn.microsoft.com/en-us/azure/app-service/tutorial-dotnetcore-sqldb-app
- https://learn.microsoft.com/en-us/azure/app-service/configure-common
- https://learn.microsoft.com/en-us/aspnet/core/tutorials/publish-to-azure-webapp-using-vs
- https://learn.microsoft.com/en-us/azure/azure-sql/database/firewall-configure
- https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/
