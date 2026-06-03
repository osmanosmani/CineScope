using CineScope.Models;

namespace CineScope.ViewModels;

public class MovieIndexViewModel
{
    public string? SearchTerm { get; set; }

    public string? SelectedGenre { get; set; }

    public int? SelectedReleaseYear { get; set; }

    public List<string> Genres { get; set; } = new();

    public List<int> ReleaseYears { get; set; } = new();

    public List<Movie> Movies { get; set; } = new();

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(SelectedGenre) ||
        SelectedReleaseYear.HasValue;
}
