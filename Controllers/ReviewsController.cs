using System.Security.Claims;
using CineScope.Data;
using CineScope.Hubs;
using CineScope.Models;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace CineScope.Controllers;

public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ReviewHub> _reviewHubContext;

    public ReviewsController(ApplicationDbContext context, IHubContext<ReviewHub> reviewHubContext)
    {
        _context = context;
        _reviewHubContext = reviewHubContext;
    }

    [HttpPost]
    [Authorize(Roles = IdentitySeeder.MemberRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ReviewFormViewModel model, string? signalRConnectionId)
    {
        model.Comment = model.Comment?.Trim() ?? string.Empty;

        var movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(movie => movie.Id == model.MovieId);

        if (movie == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Comment))
        {
            TempData["ReviewError"] = "Please enter a rating from 1 to 10 and a review comment.";
            return RedirectToAction("Details", "Movies", new { id = model.MovieId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var review = new Review
        {
            MovieId = model.MovieId,
            UserId = userId,
            Rating = model.Rating,
            Comment = model.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var reviewerName = User.Identity?.Name ?? "A CineScope member";

        var notificationClients = string.IsNullOrWhiteSpace(signalRConnectionId)
            ? _reviewHubContext.Clients.All
            : _reviewHubContext.Clients.AllExcept(new[] { signalRConnectionId.Trim() });

        await notificationClients.SendAsync("ReviewAdded", new
        {
            movieId = movie.Id,
            movieTitle = movie.Title,
            rating = review.Rating,
            userName = reviewerName
        });

        await notificationClients.SendAsync("MovieRated", new
        {
            movieId = movie.Id,
            movieTitle = movie.Title,
            rating = review.Rating,
            userName = reviewerName
        });

        TempData["ReviewSuccess"] = "Your review was added.";
        return RedirectToAction("Details", "Movies", new { id = model.MovieId });
    }

    [HttpPost]
    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return NotFound();
        }

        var movieId = review.MovieId;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        TempData["ReviewSuccess"] = "Review deleted.";
        return RedirectToAction("Details", "Movies", new { id = movieId });
    }
}
