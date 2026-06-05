using System.ComponentModel.DataAnnotations;

namespace CineScope.ViewModels;

public class ExternalMovieImportViewModel
{
    [Required]
    public int ExternalId { get; set; }

    public string ReturnAction { get; set; } = "Search";

    public string? SearchTerm { get; set; }
}
