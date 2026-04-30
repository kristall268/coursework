using GitHubExplorer.Models.ViewModels;

namespace GitHubExplorer.Models.ViewModels;

public class HomeViewModel
{
    /// <summary>Trending repos shown on the main page (top 6)</summary>
    public List<RepositoryViewModel> TrendingRepos { get; set; } = new();

    /// <summary>Shown in sidebar — fetched from GitHub Topics API</summary>
    public List<TopicViewModel> TrendingTopics { get; set; } = new();

    /// <summary>Whether the GitHub API call succeeded</summary>
    public bool ApiAvailable { get; set; } = true;
}

public class TopicViewModel
{
    public string Tag { get; set; } = string.Empty;
    public string Count { get; set; } = string.Empty;
    public string Delta { get; set; } = string.Empty;
}
