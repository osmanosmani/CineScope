namespace CineScope.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalMovies { get; set; }

    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int TotalReviews { get; set; }

    public int TotalFavorites { get; set; }

    public AdminMovieSummaryViewModel? TopRatedMovie { get; set; }

    public AdminMovieSummaryViewModel? MostReviewedMovie { get; set; }

    public List<AdminReviewSummaryViewModel> LatestReviews { get; set; } = new();

    public List<AdminGenreSummaryViewModel> TopGenres { get; set; } = new();

    public List<AdminMovieSummaryViewModel> RecentlyAddedMovies { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class AdminMovieSummaryViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public decimal Rating { get; set; }

    public int ReviewCount { get; set; }

    public int FavoriteCount { get; set; }
}

public class AdminReviewSummaryViewModel
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public string MovieTitle { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class AdminGenreSummaryViewModel
{
    public string Genre { get; set; } = string.Empty;

    public int MovieCount { get; set; }

    public int Percentage { get; set; }
}
