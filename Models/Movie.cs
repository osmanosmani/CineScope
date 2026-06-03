using System.ComponentModel.DataAnnotations;

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

    [Display(Name = "Release Year")]
    [Range(1888, 2100)]
    public int ReleaseYear { get; set; }

    [Range(0, 10)]
    public decimal Rating { get; set; }

    [Display(Name = "Duration (minutes)")]
    [Range(1, 600)]
    public int Duration { get; set; }

    [Display(Name = "Poster URL")]
    [Url]
    [StringLength(500)]
    public string? PosterUrl { get; set; }

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
}
