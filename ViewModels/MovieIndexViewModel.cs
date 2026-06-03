using CineScope.Models;

namespace CineScope.ViewModels;

public class MovieIndexViewModel
{
    public string? SearchTerm { get; set; }

    public List<Movie> Movies { get; set; } = new();
}
