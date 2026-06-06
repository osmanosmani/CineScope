using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CineScope.Models;

public class Movie
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Genre { get; set; } = string.Empty;

    [NotMapped]
    public IReadOnlyList<string> GenreList =>
        Genre.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(genre => !string.IsNullOrWhiteSpace(genre))
            .ToList();

    [NotMapped]
    public string PrimaryGenre => GenreList.FirstOrDefault() ?? "Uncategorized";

    [Display(Name = "Release Year")]
    [Range(1888, 2100)]
    public int ReleaseYear { get; set; }

    [Range(0, 10)]
    public decimal Rating { get; set; }

    [Display(Name = "Duration (minutes)")]
    [Range(0, 600)]
    public int Duration { get; set; }

    [NotMapped]
    public string DurationDisplay
    {
        get
        {
            if (Duration <= 0)
            {
                return "Unknown";
            }

            var hours = Duration / 60;
            var minutes = Duration % 60;

            if (hours == 0)
            {
                return $"{minutes}m";
            }

            return minutes == 0
                ? $"{hours}h"
                : $"{hours}h {minutes}m";
        }
    }

    [Display(Name = "Poster URL")]
    [Url]
    [StringLength(500)]
    public string? PosterUrl { get; set; }

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
