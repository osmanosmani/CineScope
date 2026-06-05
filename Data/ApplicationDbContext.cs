using CineScope.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineScope.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

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

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(review => review.UserId)
                .IsRequired();

            entity.Property(review => review.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(review => review.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(review => review.Movie)
                .WithMany(movie => movie.Reviews)
                .HasForeignKey(review => review.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(review => review.User)
                .WithMany()
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.Property(favorite => favorite.UserId)
                .IsRequired();

            entity.Property(favorite => favorite.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(favorite => new { favorite.MovieId, favorite.UserId })
                .IsUnique();

            entity.HasOne(favorite => favorite.Movie)
                .WithMany(movie => movie.Favorites)
                .HasForeignKey(favorite => favorite.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(favorite => favorite.User)
                .WithMany()
                .HasForeignKey(favorite => favorite.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
