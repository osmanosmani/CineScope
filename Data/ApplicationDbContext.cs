using CineScope.Models;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(movie => movie.Title)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(movie => movie.Genre)
                .IsRequired()
                .HasMaxLength(60);

            entity.Property(movie => movie.Rating)
                .HasPrecision(3, 1);

            entity.Property(movie => movie.PosterUrl)
                .HasMaxLength(500);

            entity.Property(movie => movie.Description)
                .IsRequired()
                .HasMaxLength(2000);
        });
    }
}
