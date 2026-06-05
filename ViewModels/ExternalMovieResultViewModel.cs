namespace CineScope.ViewModels;

public class ExternalMovieResultViewModel
{
    public int ExternalId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Genre { get; set; } = "Unknown";

    public int ReleaseYear { get; set; }

    public decimal Rating { get; set; }

    public int? Duration { get; set; }

    public string? PosterUrl { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsAlreadyImported { get; set; }
}
