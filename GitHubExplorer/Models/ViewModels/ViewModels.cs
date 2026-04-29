namespace GitHubExplorer.Models.ViewModels;

// ── /explore ────────────────────────────────────────────────────
public class ExploreViewModel
{
    public List<RepositoryViewModel> Repositories { get; set; } = new();
    public ExploreFilterModel Filter { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Filter.PageSize);
    public bool HasPreviousPage => Filter.Page > 1;
    public bool HasNextPage => Filter.Page < TotalPages;
}

// ── /repo/{owner}/{name} ────────────────────────────────────────
public class RepoDetailViewModel
{
    public RepositoryViewModel Repo { get; set; } = new();
    public string? ReadmeHtml { get; set; }
    public List<ContributorViewModel> Contributors { get; set; } = new();
    public List<RepositoryViewModel> SimilarRepos { get; set; } = new();
    public LanguageBreakdown Languages { get; set; } = new();
}

public class ContributorViewModel
{
    public string Login { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? HtmlUrl { get; set; }
    public int Contributions { get; set; }
}

public class LanguageBreakdown
{
    public Dictionary<string, long> Languages { get; set; } = new();
    public long Total => Languages.Values.Sum();

    public double GetPercent(string language) =>
        Total == 0 ? 0 : Math.Round(Languages[language] / (double)Total * 100, 1);
}

// ── /user/{login} ───────────────────────────────────────────────
public class UserProfileViewModel
{
    public string Login { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Blog { get; set; }
    public string? HtmlUrl { get; set; }
    public int PublicRepos { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RepositoryViewModel> Repositories { get; set; } = new();
}

// ── /trending ───────────────────────────────────────────────────
public class TrendingViewModel
{
    public List<RepositoryViewModel> Repositories { get; set; } = new();
    public string Language { get; set; } = string.Empty;
    public string Since { get; set; } = "daily";
    public List<string> AvailableLanguages { get; set; } = new();
}

// ── /compare ────────────────────────────────────────────────────
public class CompareViewModel
{
    public RepositoryViewModel? Repo1 { get; set; }
    public RepositoryViewModel? Repo2 { get; set; }
    public string? Repo1Input { get; set; }
    public string? Repo2Input { get; set; }
}