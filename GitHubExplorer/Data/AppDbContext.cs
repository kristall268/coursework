using GitHubExplorer.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace GitHubExplorer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<FavoriteRepo> FavoriteRepos => Set<FavoriteRepo>();
    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AppUser
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.GitHubLogin).IsUnique();
            e.HasIndex(u => u.GitHubId).IsUnique();
            e.Property(u => u.GitHubLogin).HasMaxLength(100).IsRequired();
            e.Property(u => u.AccessToken).HasMaxLength(500);
            e.Property(u => u.AvatarUrl).HasMaxLength(500);
            e.Property(u => u.Email).HasMaxLength(300);
            e.Property(u => u.DisplayName).HasMaxLength(200);
        });

        // FavoriteRepo
        modelBuilder.Entity<FavoriteRepo>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Owner).HasMaxLength(100).IsRequired();
            e.Property(f => f.Name).HasMaxLength(200).IsRequired();
            e.Property(f => f.Description).HasMaxLength(1000);
            e.Property(f => f.Language).HasMaxLength(100);
            e.Property(f => f.HtmlUrl).HasMaxLength(500);
            e.Property(f => f.AvatarUrl).HasMaxLength(500);

            // Один пользователь не может дважды добавить одно репо
            e.HasIndex(f => new { f.AppUserId, f.Owner, f.Name }).IsUnique();

            e.HasOne(f => f.AppUser)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(f => f.Collection)
                .WithMany(c => c.Repos)
                .HasForeignKey(f => f.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Collection
        modelBuilder.Entity<Collection>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Description).HasMaxLength(1000);

            e.HasOne(c => c.AppUser)
                .WithMany(u => u.Collections)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}