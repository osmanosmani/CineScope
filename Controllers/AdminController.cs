using CineScope.Data;
using CineScope.Hubs;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Controllers;

[Authorize(Roles = IdentitySeeder.AdminRole)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new AdminDashboardViewModel
        {
            TotalMovies = await _context.Movies.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            ActiveUsers = ReviewHub.CurrentActiveUsers,
            TotalReviews = await _context.Reviews.CountAsync(),
            TotalFavorites = await _context.Favorites.CountAsync(),
            TopRatedMovie = await GetTopRatedMovieAsync(),
            MostReviewedMovie = await GetMostReviewedMovieAsync(),
            LatestReviews = await GetLatestReviewsAsync(),
            RecentlyAddedMovies = await GetRecentlyAddedMoviesAsync(),
            GeneratedAt = DateTime.UtcNow
        };

        viewModel.TopGenres = await GetTopGenresAsync();

        return View(viewModel);
    }

    private async Task<AdminMovieSummaryViewModel?> GetTopRatedMovieAsync()
    {
        return await _context.Movies
            .AsNoTracking()
            .OrderByDescending(movie => movie.Rating)
            .ThenByDescending(movie => movie.ReleaseYear)
            .ThenBy(movie => movie.Title)
            .Select(movie => new AdminMovieSummaryViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseYear = movie.ReleaseYear,
                Rating = movie.Rating,
                ReviewCount = movie.Reviews.Count,
                FavoriteCount = movie.Favorites.Count
            })
            .FirstOrDefaultAsync();
    }

    private async Task<AdminMovieSummaryViewModel?> GetMostReviewedMovieAsync()
    {
        return await _context.Movies
            .AsNoTracking()
            .Where(movie => movie.Reviews.Any())
            .Select(movie => new AdminMovieSummaryViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseYear = movie.ReleaseYear,
                Rating = movie.Rating,
                ReviewCount = movie.Reviews.Count,
                FavoriteCount = movie.Favorites.Count
            })
            .OrderByDescending(movie => movie.ReviewCount)
            .ThenBy(movie => movie.Title)
            .FirstOrDefaultAsync();
    }

    private async Task<List<AdminReviewSummaryViewModel>> GetLatestReviewsAsync()
    {
        return await _context.Reviews
            .AsNoTracking()
            .OrderByDescending(review => review.CreatedAt)
            .Take(5)
            .Select(review => new AdminReviewSummaryViewModel
            {
                Id = review.Id,
                MovieId = review.MovieId,
                MovieTitle = review.Movie.Title,
                UserEmail = review.User != null && review.User.Email != null
                    ? review.User.Email
                    : "Unknown user",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<List<AdminMovieSummaryViewModel>> GetRecentlyAddedMoviesAsync()
    {
        return await _context.Movies
            .AsNoTracking()
            .OrderByDescending(movie => movie.Id)
            .Take(6)
            .Select(movie => new AdminMovieSummaryViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseYear = movie.ReleaseYear,
                Rating = movie.Rating,
                ReviewCount = movie.Reviews.Count,
                FavoriteCount = movie.Favorites.Count
            })
            .ToListAsync();
    }

    private async Task<List<AdminGenreSummaryViewModel>> GetTopGenresAsync()
    {
        var genreValues = await _context.Movies
            .AsNoTracking()
            .Select(movie => movie.Genre)
            .ToListAsync();

        var groupedGenres = genreValues
            .SelectMany(genre => genre.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(genre => !string.IsNullOrWhiteSpace(genre))
            .GroupBy(genre => genre, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AdminGenreSummaryViewModel
            {
                Genre = group.Key,
                MovieCount = group.Count()
            })
            .OrderByDescending(genre => genre.MovieCount)
            .ThenBy(genre => genre.Genre)
            .Take(6)
            .ToList();

        var topGenreCount = groupedGenres.FirstOrDefault()?.MovieCount ?? 0;

        foreach (var genre in groupedGenres)
        {
            genre.Percentage = topGenreCount == 0
                ? 0
                : (int)Math.Round(genre.MovieCount * 100.0 / topGenreCount);
        }

        return groupedGenres;
    }
}
