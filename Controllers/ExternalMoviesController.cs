using CineScope.Data;
using CineScope.Hubs;
using CineScope.Models;
using CineScope.Services;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace CineScope.Controllers;

[Authorize(Roles = IdentitySeeder.AdminRole)]
public class ExternalMoviesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly TmdbService _tmdbService;
    private readonly IHubContext<ReviewHub> _reviewHubContext;

    public ExternalMoviesController(
        ApplicationDbContext context,
        TmdbService tmdbService,
        IHubContext<ReviewHub> reviewHubContext)
    {
        _context = context;
        _tmdbService = tmdbService;
        _reviewHubContext = reviewHubContext;
    }

    public async Task<IActionResult> Search(string? searchTerm, CancellationToken cancellationToken)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm)
            ? null
            : searchTerm.Trim();

        var viewModel = new ExternalMovieSearchViewModel
        {
            SearchTerm = normalizedSearchTerm,
            HasSearched = normalizedSearchTerm != null,
            IsApiConfigured = _tmdbService.IsConfigured,
            PageTitle = "External Movie Search",
            PageDescription = "Search TMDB by title and import selected movies into the local CineScope catalog.",
            EmptyMessage = "No TMDB movies matched this search.",
            ReturnAction = nameof(Search)
        };

        if (!_tmdbService.IsConfigured)
        {
            viewModel.ErrorMessage = "TMDB API key is missing. Add it in appsettings.json before using external import.";
            return View(viewModel);
        }

        if (normalizedSearchTerm == null)
        {
            return View(viewModel);
        }

        try
        {
            viewModel.Results = await _tmdbService.SearchMoviesAsync(normalizedSearchTerm, cancellationToken);
            await MarkAlreadyImportedAsync(viewModel.Results, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            viewModel.ErrorMessage = "Could not reach TMDB right now. Please try again later.";
        }

        return View(viewModel);
    }

    public async Task<IActionResult> Trending(CancellationToken cancellationToken)
    {
        var viewModel = new ExternalMovieSearchViewModel
        {
            HasSearched = true,
            IsApiConfigured = _tmdbService.IsConfigured,
            PageTitle = "Trending Movies",
            PageDescription = "Browse movies currently trending on TMDB and import them into CineScope.",
            EmptyMessage = "No trending movies were returned from TMDB.",
            ReturnAction = nameof(Trending)
        };

        if (!_tmdbService.IsConfigured)
        {
            viewModel.ErrorMessage = "TMDB API key is missing. Add it in appsettings.json before using trending movies.";
            return View(viewModel);
        }

        try
        {
            viewModel.Results = await _tmdbService.GetTrendingMoviesAsync(cancellationToken);
            await MarkAlreadyImportedAsync(viewModel.Results, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            viewModel.ErrorMessage = "Could not reach TMDB right now. Please try again later.";
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(
        ExternalMovieImportViewModel model,
        string? signalRConnectionId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ExternalMovieError"] = "The selected TMDB movie could not be imported.";
            return RedirectBackToExternalPage(model);
        }

        if (!_tmdbService.IsConfigured)
        {
            TempData["ExternalMovieError"] = "TMDB API key is missing. Add it in appsettings.json before importing.";
            return RedirectBackToExternalPage(model);
        }

        ExternalMovieResultViewModel? externalMovie;

        try
        {
            externalMovie = await _tmdbService.GetMovieDetailsAsync(model.ExternalId, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ExternalMovieError"] = ex.Message;
            return RedirectBackToExternalPage(model);
        }
        catch (HttpRequestException)
        {
            TempData["ExternalMovieError"] = "Could not reach TMDB right now. Please try again later.";
            return RedirectBackToExternalPage(model);
        }

        if (externalMovie == null)
        {
            TempData["ExternalMovieError"] = "TMDB did not return details for the selected movie.";
            return RedirectBackToExternalPage(model);
        }

        if (externalMovie.ReleaseYear < 1888 || externalMovie.ReleaseYear > 2100)
        {
            TempData["ExternalMovieError"] = "This TMDB movie is missing a valid release year, so it was not imported.";
            return RedirectBackToExternalPage(model);
        }

        var title = Truncate(FirstNonEmpty(externalMovie.Title, "Untitled"), 120);
        var duplicateExists = await _context.Movies
            .AsNoTracking()
            .AnyAsync(movie => movie.Title == title && movie.ReleaseYear == externalMovie.ReleaseYear, cancellationToken);

        if (duplicateExists)
        {
            TempData["ExternalMovieWarning"] = $"{title} ({externalMovie.ReleaseYear}) already exists in CineScope.";
            return RedirectBackToExternalPage(model);
        }

        var movie = new Movie
        {
            Title = title,
            Genre = Truncate(FirstNonEmpty(externalMovie.Genre, "Unknown"), 60),
            ReleaseYear = externalMovie.ReleaseYear,
            Rating = externalMovie.Rating,
            Duration = externalMovie.Duration.GetValueOrDefault(),
            PosterUrl = string.IsNullOrWhiteSpace(externalMovie.PosterUrl)
                ? null
                : Truncate(externalMovie.PosterUrl, 500),
            Description = Truncate(FirstNonEmpty(externalMovie.Description, "No description available."), 2000)
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);

        var notificationClients = string.IsNullOrWhiteSpace(signalRConnectionId)
            ? _reviewHubContext.Clients.All
            : _reviewHubContext.Clients.AllExcept(new[] { signalRConnectionId.Trim() });

        await notificationClients.SendAsync("MovieImported", new
        {
            movieId = movie.Id,
            movieTitle = movie.Title,
            releaseYear = movie.ReleaseYear,
            importedBy = User.Identity?.Name ?? "Admin"
        }, cancellationToken);

        TempData["ExternalMovieSuccess"] = $"{movie.Title} was imported into CineScope.";
        return RedirectToAction("Details", "Movies", new { id = movie.Id });
    }

    private async Task MarkAlreadyImportedAsync(List<ExternalMovieResultViewModel> results, CancellationToken cancellationToken)
    {
        var validResults = results
            .Where(movie => !string.IsNullOrWhiteSpace(movie.Title) && movie.ReleaseYear > 0)
            .ToList();

        if (!validResults.Any())
        {
            return;
        }

        var titles = validResults
            .Select(movie => Truncate(movie.Title.Trim(), 120))
            .Distinct()
            .ToList();
        var years = validResults.Select(movie => movie.ReleaseYear).Distinct().ToList();
        var localMovies = await _context.Movies
            .AsNoTracking()
            .Where(movie => titles.Contains(movie.Title) && years.Contains(movie.ReleaseYear))
            .Select(movie => new { movie.Title, movie.ReleaseYear })
            .ToListAsync(cancellationToken);

        var importedKeys = localMovies
            .Select(movie => BuildDuplicateKey(movie.Title, movie.ReleaseYear))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var result in validResults)
        {
            result.IsAlreadyImported = importedKeys.Contains(BuildDuplicateKey(Truncate(result.Title.Trim(), 120), result.ReleaseYear));
        }
    }

    private IActionResult RedirectBackToExternalPage(ExternalMovieImportViewModel model)
    {
        if (string.Equals(model.ReturnAction, nameof(Trending), StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Trending));
        }

        return RedirectToAction(nameof(Search), new { searchTerm = model.SearchTerm });
    }

    private static string BuildDuplicateKey(string title, int releaseYear)
    {
        return $"{title.Trim()}|{releaseYear}";
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }
}
