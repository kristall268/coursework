namespace GitHubExplorer.Models.ViewModels;

public class RepositoryViewModel
{
    public long Id { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName => $"{Owner}/{Name}";
    public string? Description { get; set; }
    public string? Language { get; set; }
    public string? LanguageColor { get; set; }
    public int Stars { get; set; }
    public int Forks { get; set; }
    public int OpenIssues { get; set; }
    public int Watchers { get; set; }
    public string? License { get; set; }
    public string? HtmlUrl { get; set; }
    public string? OwnerAvatarUrl { get; set; }
    public string? OwnerHtmlUrl { get; set; }
    public List<string> Topics { get; set; } = new();
    public bool IsPrivate { get; set; }
    public bool IsFork { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PushedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsTrending { get; set; }

    public string UpdatedAgo
    {
        get
        {
            var date = PushedAt ?? UpdatedAt;
            if (date is null) return "unknown";
            var diff = DateTime.UtcNow - date.Value;
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 30) return $"{(int)diff.TotalDays}d ago";
            return date.Value.ToString("MMM d, yyyy");
        }
    }

    public string StarsFormatted => Stars >= 1000
        ? $"{Stars / 1000.0:F1}k"
        : Stars.ToString();
}