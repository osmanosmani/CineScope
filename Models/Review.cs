using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CineScope.Models;

public class Review
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public Movie Movie { get; set; } = default!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }

    [Range(1, 10)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
