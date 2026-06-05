namespace CineScope.ViewModels;

public class ExternalMovieSearchViewModel
{
    public string? SearchTerm { get; set; }

    public string PageTitle { get; set; } = "External Movie Search";

    public string PageDescription { get; set; } = "Search TMDB and import movies into the local CineScope catalog.";

    public string EmptyMessage { get; set; } = "No external movies found.";

    public string ReturnAction { get; set; } = "Search";

    public bool HasSearched { get; set; }

    public bool IsApiConfigured { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public List<ExternalMovieResultViewModel> Results { get; set; } = new();
}
