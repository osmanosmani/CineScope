using CineScope.Models;

namespace CineScope.ViewModels;

public class MovieDetailsViewModel
{
    public Movie Movie { get; set; } = default!;

    public ReviewFormViewModel ReviewForm { get; set; } = new();

    public List<Review> Reviews { get; set; } = new();

    public double AverageUserRating { get; set; }

    public int ReviewCount { get; set; }

    public bool IsFavorite { get; set; }

    public bool CanReview { get; set; }

    public bool CanFavorite { get; set; }
}
