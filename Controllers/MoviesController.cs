using CineScope.Data;
using CineScope.Models;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CineScope.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MoviesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Discover(string? searchTerm, string? genre)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm)
            ? null
            : searchTerm.Trim();
        var selectedGenre = string.IsNullOrWhiteSpace(genre)
            ? null
            : genre.Trim();

        var allMovies = await _context.Movies
            .AsNoTracking()
            .OrderByDescending(movie => movie.Rating)
            .ThenByDescending(movie => movie.ReleaseYear)
            .ToListAsync();

        var filteredMovies = allMovies.AsEnumerable();

        if (normalizedSearchTerm != null)
        {
            filteredMovies = filteredMovies.Where(movie =>
                movie.Title.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (selectedGenre != null)
        {
            filteredMovies = filteredMovies.Where(movie =>
                string.Equals(movie.Genre, selectedGenre, StringComparison.OrdinalIgnoreCase));
        }

        var movies = filteredMovies.ToList();

        var viewModel = new MovieDiscoverViewModel
        {
            SearchTerm = normalizedSearchTerm,
            SelectedGenre = selectedGenre,
            SpotlightMovie = movies.FirstOrDefault(),
            TopRatedMovies = movies.Take(8).ToList(),
            RecentlyReleasedMovies = movies
                .OrderByDescending(movie => movie.ReleaseYear)
                .ThenBy(movie => movie.Title)
                .Take(6)
                .ToList(),
            Movies = movies,
            Genres = allMovies
                .Select(movie => movie.Genre)
                .Distinct()
                .OrderBy(genre => genre)
                .ToList(),
            TotalMovies = allMovies.Count
        };

        return View(viewModel);
    }

    public IActionResult About()
    {
        return RedirectToAction(nameof(AboutMe));
    }

    public IActionResult AboutMe()
    {
        return View();
    }

    public async Task<IActionResult> Index(string? searchTerm, string? genre, int? releaseYear)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm)
            ? null
            : searchTerm.Trim();
        var selectedGenre = string.IsNullOrWhiteSpace(genre)
            ? null
            : genre.Trim();

        var genres = await _context.Movies
            .AsNoTracking()
            .Select(movie => movie.Genre)
            .Distinct()
            .OrderBy(movieGenre => movieGenre)
            .ToListAsync();

        var releaseYears = await _context.Movies
            .AsNoTracking()
            .Select(movie => movie.ReleaseYear)
            .Distinct()
            .OrderByDescending(year => year)
            .ToListAsync();

        var moviesQuery = _context.Movies.AsNoTracking();

        if (normalizedSearchTerm != null)
        {
            moviesQuery = moviesQuery.Where(movie => movie.Title.Contains(normalizedSearchTerm));
        }

        if (selectedGenre != null)
        {
            moviesQuery = moviesQuery.Where(movie => movie.Genre == selectedGenre);
        }

        if (releaseYear.HasValue)
        {
            moviesQuery = moviesQuery.Where(movie => movie.ReleaseYear == releaseYear.Value);
        }

        var viewModel = new MovieIndexViewModel
        {
            SearchTerm = normalizedSearchTerm,
            SelectedGenre = selectedGenre,
            SelectedReleaseYear = releaseYear,
            Genres = genres,
            ReleaseYears = releaseYears,
            Movies = await moviesQuery
                .OrderBy(movie => movie.Title)
                .ToListAsync()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .AsNoTracking()
            .Include(movie => movie.Reviews)
                .ThenInclude(review => review.User)
            .FirstOrDefaultAsync(movie => movie.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isMember = User.IsInRole(IdentitySeeder.MemberRole);

        var viewModel = new MovieDetailsViewModel
        {
            Movie = movie,
            Reviews = movie.Reviews
                .OrderByDescending(review => review.CreatedAt)
                .ToList(),
            AverageUserRating = movie.Reviews.Any()
                ? movie.Reviews.Average(review => review.Rating)
                : 0,
            ReviewCount = movie.Reviews.Count,
            IsFavorite = isMember && userId != null && await _context.Favorites
                .AsNoTracking()
                .AnyAsync(favorite => favorite.MovieId == movie.Id && favorite.UserId == userId),
            CanReview = isMember,
            CanFavorite = isMember,
            ReviewForm = new ReviewFormViewModel
            {
                MovieId = movie.Id
            }
        };

        return View(viewModel);
    }

    [Authorize(Roles = IdentitySeeder.AdminRole)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Genre,ReleaseYear,Rating,Duration,PosterUrl,Description")] Movie movie)
    {
        if (!ModelState.IsValid)
        {
            return View(movie);
        }

        _context.Add(movie);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentitySeeder.AdminRole)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    [HttpPost]
    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,ReleaseYear,Rating,Duration,PosterUrl,Description")] Movie movie)
    {
        if (id != movie.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(movie);
        }

        try
        {
            _context.Update(movie);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MovieExists(movie.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentitySeeder.AdminRole)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(movie => movie.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie != null)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool MovieExists(int id)
    {
        return _context.Movies.Any(movie => movie.Id == id);
    }
}
