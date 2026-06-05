using CineScope.Models;

namespace CineScope.ViewModels;

public class MovieDiscoverViewModel
{
    public string? SearchTerm { get; set; }

    public string? SelectedGenre { get; set; }

    public Movie? SpotlightMovie { get; set; }

    public List<Movie> TopRatedMovies { get; set; } = new();

    public List<Movie> RecentlyReleasedMovies { get; set; } = new();

    public List<Movie> Movies { get; set; } = new();

    public List<string> Genres { get; set; } = new();

    public int TotalMovies { get; set; }

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(SelectedGenre);
}
