using CineScope.Models;

namespace CineScope.ViewModels;

public class FavoriteMoviesViewModel
{
    public List<Favorite> Favorites { get; set; } = new();
}
