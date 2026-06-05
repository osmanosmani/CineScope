using System.Security.Claims;
using CineScope.Data;
using CineScope.Models;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Controllers;

[Authorize(Roles = IdentitySeeder.MemberRole)]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _context;

    public FavoritesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var viewModel = new FavoriteMoviesViewModel
        {
            Favorites = await _context.Favorites
                .AsNoTracking()
                .Include(favorite => favorite.Movie)
                .Where(favorite => favorite.UserId == userId)
                .OrderByDescending(favorite => favorite.CreatedAt)
                .ToListAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int movieId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        if (!await _context.Movies.AnyAsync(movie => movie.Id == movieId))
        {
            return NotFound();
        }

        var alreadyFavorite = await _context.Favorites
            .AnyAsync(favorite => favorite.MovieId == movieId && favorite.UserId == userId);

        if (!alreadyFavorite)
        {
            _context.Favorites.Add(new Favorite
            {
                MovieId = movieId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Movies", new { id = movieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int movieId, string? returnUrl = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(favorite => favorite.MovieId == movieId && favorite.UserId == userId);

        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Movies", new { id = movieId });
    }
}
