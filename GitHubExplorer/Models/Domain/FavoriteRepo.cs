namespace GitHubExplorer.Models.Domain;

public class FavoriteRepo
{
    public int Id { get; set; }

    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;

    /// <summary>Repository owner login, e.g. "facebook"</summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>Repository name, e.g. "react"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Full name: "facebook/react"</summary>
    public string FullName => $"{Owner}/{Name}";

    public string? Description { get; set; }
    public string? Language { get; set; }
    public int Stars { get; set; }
    public string? HtmlUrl { get; set; }
    public string? AvatarUrl { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Optional: which collection this belongs to
    public int? CollectionId { get; set; }
    public Collection? Collection { get; set; }
}