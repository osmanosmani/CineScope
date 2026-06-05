# API Architecture

CineScope integrates with TMDB to search, view trending movies, and import selected movies into the local SQL Server database.

## External API Service

Service file:

```text
Services/TmdbService.cs
```

The service is responsible for:

- Reading TMDB settings from configuration.
- Calling TMDB endpoints with `HttpClient`.
- Searching movies by title.
- Loading trending movies.
- Loading full movie details before import.
- Mapping TMDB response data into CineScope view models.

Settings:

```json
"Tmdb": {
  "ApiKey": "",
  "BaseUrl": "https://api.themoviedb.org/3",
  "ImageBaseUrl": "https://image.tmdb.org/t/p/w500",
  "Language": "en-US"
}
```

The API key should be stored with user secrets locally:

```powershell
dotnet user-secrets set "Tmdb:ApiKey" "YOUR_TMDB_API_KEY"
```

## Controller

Controller file:

```text
Controllers/ExternalMoviesController.cs
```

This controller is restricted to Admin users:

```csharp
[Authorize(Roles = IdentitySeeder.AdminRole)]
```

Actions:

- `Search` - searches TMDB by title.
- `Trending` - loads trending TMDB movies.
- `Import` - imports one selected TMDB movie into the local database.

## ViewModels

ViewModel files:

- `ExternalMovieSearchViewModel`
- `ExternalMovieResultViewModel`
- `ExternalMovieImportViewModel`

They keep external API data separate from the local `Movie` entity.

## Import Flow

1. Admin opens `/ExternalMovies/Search` or `/ExternalMovies/Trending`.
2. The controller asks `TmdbService` for external movies.
3. Results are displayed with poster, title, genre, year, rating, and description.
4. Admin clicks `Import Movie`.
5. The form posts only the external TMDB movie id.
6. The controller loads full movie details from TMDB again.
7. The controller validates the data.
8. The controller checks for duplicates.
9. If no duplicate exists, a local `Movie` record is created.
10. The Admin is redirected to the local movie details page.

## Duplicate Prevention

Before import, CineScope checks if a movie with the same title and release year already exists:

```text
Title + ReleaseYear
```

If a match exists, the movie is not imported and the Admin sees a friendly warning.

## Why the Service Layer Matters

The service layer keeps HTTP/API logic outside the MVC controller. This makes the controller easier to read and keeps the project structure beginner-friendly:

- Controller: handles request flow and authorization.
- Service: handles TMDB communication.
- ViewModel: handles page data.
- Entity Model: handles local database data.
