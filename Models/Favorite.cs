using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CineScope.Models;

public class Favorite
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public Movie Movie { get; set; } = default!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
