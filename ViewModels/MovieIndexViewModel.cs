using CineScope.Models;

namespace CineScope.ViewModels;

public class MovieIndexViewModel
{
    private const int DefaultPageSize = 8;

    public string? SearchTerm { get; set; }

    public string? SelectedGenre { get; set; }

    public int? SelectedReleaseYear { get; set; }

    public List<string> Genres { get; set; } = new();

    public List<int> ReleaseYears { get; set; } = new();

    public List<Movie> Movies { get; set; } = new();

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = DefaultPageSize;

    public int TotalMovies { get; set; }

    public int TotalPages =>
        TotalMovies == 0
            ? 1
            : (int)Math.Ceiling(TotalMovies / (double)PageSize);

    public int FirstItemNumber =>
        TotalMovies == 0
            ? 0
            : ((PageNumber - 1) * PageSize) + 1;

    public int LastItemNumber =>
        TotalMovies == 0
            ? 0
            : Math.Min(PageNumber * PageSize, TotalMovies);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public bool ShouldShowPagination => TotalPages > 1;

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        !string.IsNullOrWhiteSpace(SelectedGenre) ||
        SelectedReleaseYear.HasValue;
}
