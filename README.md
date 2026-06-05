# CineScope - Modern Movie Discovery Platform

CineScope is a full-stack ASP.NET Core MVC portfolio project that demonstrates practical web application development with a real database, authentication, role-based authorization, external API integration, admin analytics, and Azure deployment.

The application is built as a modern movie discovery platform where users can browse movies, search and filter the catalog, review movies, manage favorites, and where Admin users can manage local movie data and import movies from TMDB.

## Project Links

- Live Azure app: [CineScope on Azure](https://app-cinescope-osman-hma2adbye5bjgxcy.swedencentral-01.azurewebsites.net)
- GitHub repository: [github.com/osmanosmani/CineScope](https://github.com/osmanosmani/CineScope)

## Portfolio Summary

This project shows that I can build and deploy a complete ASP.NET Core MVC application from database design to production hosting.

Key skills demonstrated:

- Building MVC applications with controllers, models, Razor views, and view models
- Designing relational data with Entity Framework Core and SQL Server
- Implementing ASP.NET Core Identity with roles and authorization rules
- Creating CRUD workflows with validation and clean UI
- Integrating an external REST API through a service layer
- Using Bootstrap 5 and custom CSS for a responsive dark UI
- Deploying an ASP.NET Core application to Azure App Service with Azure SQL Database
- Writing project documentation for setup, architecture, roles, API flow, and deployment

## Tech Stack

- ASP.NET Core MVC
- .NET 10.0
- SQL Server LocalDB / SQL Server
- Entity Framework Core
- Razor Views
- Bootstrap 5
- ASP.NET Core Identity
- Role-based authorization
- TMDB external movie API
- SignalR for simple real-time notifications
- Azure App Service and Azure SQL Database for live deployment

## Features

- Modern dark movie discovery homepage
- Movie CRUD management
- Search movies by title
- Filter movies by genre and release year
- Movie details page with poster, metadata, reviews, ratings, and favorites
- Member reviews and 1-10 ratings
- Member favorites page
- Admin-only movie create, edit, delete
- Admin-only TMDB movie search and trending import
- Duplicate import prevention by movie title and release year
- Admin analytics dashboard
- Latest reviews, active users, top genres, top-rated movie, most reviewed movie, recent movies
- Simple SignalR active users indicator and toast notifications

## Technical Highlights

- `ApplicationDbContext` uses Entity Framework Core and ASP.NET Core Identity tables in one database context.
- Movie management is protected so only Admin users can create, edit, and delete movies.
- Guest users can browse, search, filter, and view details without logging in.
- Member users can add reviews, give ratings, and manage favorites.
- TMDB integration is separated into a service layer, so external API logic is not mixed into controllers.
- Duplicate movie imports are prevented by checking title and release year before saving.
- The live version uses Azure App Service for hosting and Azure SQL Database for production data.
- Secrets such as connection strings and API keys are configured through Azure environment variables.

## Architecture Overview

```text
Browser
  -> ASP.NET Core MVC Controllers
  -> ViewModels and Razor Views
  -> Services, including TMDB API service
  -> Entity Framework Core
  -> SQL Server / Azure SQL Database
```

Live deployment:

```text
User Browser -> Azure App Service -> Azure SQL Database
```

## User Roles

### Guest

- Browse the homepage
- Search and filter movies
- View movie details
- Cannot add reviews, ratings, favorites, or manage movies

### Member

- All Guest permissions
- Add reviews and ratings
- Add/remove favorites
- View personal favorites page

### Admin

- All catalog browsing features
- Create, edit, and delete movies
- Import movies from TMDB
- Delete inappropriate reviews
- View admin dashboard analytics

## Default Admin Login

```text
Email: admin@cinescope.com
Password: Admin123!
```

The Admin user and roles are created automatically by `IdentitySeeder` when the app starts.

Security note: this default Admin account is included only for school demo/testing. For a real production app, change the password and keep all secrets in user secrets or Azure environment variables.

## Run Locally

Prerequisites:

- .NET 10 SDK installed
- SQL Server LocalDB or SQL Server
- EF Core tools installed

Commands:

```powershell
dotnet tool install --global dotnet-ef
dotnet restore
dotnet build
dotnet ef database update
dotnet run --urls http://localhost:5270
```

Open:

```text
http://localhost:5270
```

## Database Migrations

Existing migrations:

- `InitialCreate`
- `AddIdentityRoles`
- `AddReviewsFavorites`

Apply migrations:

```powershell
dotnet ef database update
```

Create a new migration after model changes:

```powershell
dotnet ef migrations add MigrationName
dotnet ef database update
```

## TMDB API Integration

CineScope uses TMDB for:

- Searching external movies by title
- Viewing trending movies
- Importing selected movies into the local database

The API settings are stored in `appsettings.json`:

```json
"Tmdb": {
  "ApiKey": "",
  "BaseUrl": "https://api.themoviedb.org/3",
  "ImageBaseUrl": "https://image.tmdb.org/t/p/w500",
  "Language": "en-US"
}
```

For local development, store the API key using user secrets instead of committing it:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Tmdb:ApiKey" "YOUR_TMDB_API_KEY"
```

Do not commit real API keys to GitHub.

## Screenshots

The screenshots below show the main CineScope pages for a portfolio/demo walkthrough.

### Homepage

![Homepage](Docs/screenshots/homepage.png)

### Movie Details

![Movie Details](Docs/screenshots/movie-details.png)

### Admin Dashboard

![Admin Dashboard](Docs/screenshots/admin-dashboard.png)

### TMDB Import

![TMDB Import](Docs/screenshots/tmdb-import.png)

### Favorites

![Favorites](Docs/screenshots/favorites.png)

### Movie Management

![Movie Management](Docs/screenshots/movie-management.png)

### Login

![Login](Docs/screenshots/login.png)

### About Me

![About Me](Docs/screenshots/about-me.png)

## Project Structure

```text
CineScope/
  Controllers/
  Data/
  Docs/
  Hubs/
  Migrations/
  Models/
  Services/
  ViewModels/
  Views/
  wwwroot/
```

## Important Pages

- `/` - Discover homepage
- `/Movies` - Movie management/listing
- `/Movies/Details/{id}` - Movie details
- `/Favorites` - Member favorites
- `/Movies/AboutMe` - About Me and project overview
- `/ExternalMovies/Search` - Admin TMDB search
- `/ExternalMovies/Trending` - Admin trending movies
- `/Admin` - Admin dashboard

## Deployment Notes

Azure resources used:

- Resource Group: `rg-cinescope-school`
- Azure SQL Server: `sql-cinescope-osman`
- Azure SQL Database: `CineScopeDb`
- Azure App Service Plan: `asp-cinescope-school`
- Azure App Service: `app-cinescope-osman`

Deployment checklist:

1. Create Azure SQL Database.
2. Create Azure App Service.
3. Add `DefaultConnection` in App Service connection strings.
4. Add `Tmdb__ApiKey` in App Service app settings.
5. Apply EF Core migrations to Azure SQL.
6. Publish from Visual Studio.
7. Test the live URL.

See `Docs/Deployment-Guide.md` for details.

Secrets are not stored in the repository. The live app reads `DefaultConnection` and `Tmdb__ApiKey` from Azure App Service environment variables.

## Future Improvements

- Better review moderation tools
- User profile pages
- Pagination for movies and reviews
- More advanced analytics charts
- Azure Key Vault for secrets
- Automated CI/CD pipeline
- Docker container deployment
- Unit and integration tests
