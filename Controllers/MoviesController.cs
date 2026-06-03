using CineScope.Data;
using CineScope.Models;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MoviesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        var moviesQuery = _context.Movies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            moviesQuery = moviesQuery.Where(movie => movie.Title.Contains(searchTerm));
        }

        var viewModel = new MovieIndexViewModel
        {
            SearchTerm = searchTerm,
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
            .FirstOrDefaultAsync(movie => movie.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
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
