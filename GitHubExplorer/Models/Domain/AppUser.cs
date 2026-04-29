namespace GitHubExplorer.Models.Domain;

public class AppUser
{
    public int Id { get; set; }

    /// <summary>GitHub login (username), e.g. "octocat"</summary>
    public string GitHubLogin { get; set; } = string.Empty;

    /// <summary>GitHub numeric user ID</summary>
    public long GitHubId { get; set; }

    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }

    /// <summary>OAuth access token — used for authenticated API calls</summary>
    public string? AccessToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<FavoriteRepo> Favorites { get; set; } = new List<FavoriteRepo>();
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
}