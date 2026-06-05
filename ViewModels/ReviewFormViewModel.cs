using System.ComponentModel.DataAnnotations;

namespace CineScope.ViewModels;

public class ReviewFormViewModel
{
    public int MovieId { get; set; }

    [Range(1, 10)]
    public int Rating { get; set; } = 8;

    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
