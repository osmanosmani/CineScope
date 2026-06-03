# CineScope

CineScope is a school mini project built with ASP.NET Core MVC.

## Level 1: Foundation

This version includes a working Movie Management CRUD system:

- List all movies
- Search movies by title
- View movie details
- Add a movie
- Edit a movie
- Delete a movie

## Tech Stack

- ASP.NET Core MVC
- SQL Server LocalDB
- Entity Framework Core
- Razor Views
- Bootstrap 5

## Project Structure

- `Controllers` - MVC controllers
- `Models` - entity models
- `Data` - EF Core database context
- `ViewModels` - page-specific view models
- `Views` - Razor views
- `wwwroot` - static files such as CSS, JavaScript, images, and libraries

## Run Locally

```powershell
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

Open the app at:

```text
http://localhost:5270/Movies
```

## Current Status

Level 1 is complete. Authentication, reviews, favorites, external movie API integration, admin dashboard, SignalR, Docker, and Azure deployment are planned for later levels.
