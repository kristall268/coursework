using GitHubExplorer.Models.ViewModels;
using GitHubExplorer.Services.Cache;
using Octokit;
using System.Text.RegularExpressions;

namespace GitHubExplorer.Services.GitHub;

public class GitHubService : IGitHubService
{
    private readonly GitHubClient _client;
    private readonly ICacheService _cache;
    private readonly ILogger<GitHubService> _logger;

    private static readonly Dictionary<string, string> LanguageColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C#"] = "#178600",
        ["JavaScript"] = "#f1e05a",
        ["TypeScript"] = "#3178c6",
        ["Python"] = "#3572A5",
        ["Go"] = "#00ADD8",
        ["Rust"] = "#dea584",
        ["Java"] = "#b07219",
        ["Kotlin"] = "#A97BFF",
        ["Swift"] = "#F05138",
        ["C"] = "#555555",
        ["C++"] = "#f34b7d",
        ["Ruby"] = "#701516",
        ["PHP"] = "#4F5D95",
        ["Dart"] = "#00B4AB",
        ["Scala"] = "#c22d40",
        ["Shell"] = "#89e051",
        ["HTML"] = "#e34c26",
        ["CSS"] = "#563d7c",
        ["Vue"] = "#41b883",
        ["Elixir"] = "#6e4a7e",
    };

    public GitHubService(GitHubClient client, ICacheService cache, ILogger<GitHubService> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ExploreViewModel> SearchRepositoriesAsync(ExploreFilterModel filter)
    {
        var cacheKey = filter.ToCacheKey();
        var cached = await _cache.GetAsync<ExploreViewModel>(cacheKey);
        if (cached is not null) return cached;

        try
        {
            var request = BuildSearchRequest(filter);
            var result  = await _client.Search.SearchRepo(request);

            var vm = new ExploreViewModel
            {
                Filter     = filter,
                TotalCount = Math.Min(result.TotalCount, 1000),
                Repositories = result.Items.Select(MapToViewModel).ToList()
            };

            await _cache.SetAsync(cacheKey, vm, TimeSpan.FromMinutes(5));
            return vm;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching repositories with filter {Filter}", filter.ToCacheKey());
            return new ExploreViewModel { Filter = filter };
        }
    }

    public async Task<RepoDetailViewModel?> GetRepositoryAsync(string owner, string name)
    {
        var cacheKey = $"repo:{owner}:{name}";
        var cached   = await _cache.GetAsync<RepoDetailViewModel>(cacheKey);
        if (cached is not null) return cached;

        try
        {
            var repo = await _client.Repository.Get(owner, name);
            var vm   = new RepoDetailViewModel { Repo = MapToViewModel(repo) };

            // README — декодируем Markdown и конвертируем в базовый HTML
            try
            {
                var readme = await _client.Repository.Content.GetReadme(owner, name);
                vm.ReadmeHtml = ConvertMarkdownToHtml(readme.Content);
            }
            catch { /* no readme */ }

            // Contributors (top 10)
            try
            {
                var contributors = await _client.Repository.GetAllContributors(owner, name);
                vm.Contributors = contributors.Take(10).Select(c => new ContributorViewModel
                {
                    Login         = c.Login,
                    AvatarUrl     = c.AvatarUrl,
                    HtmlUrl       = c.HtmlUrl,
                    Contributions = c.Contributions
                }).ToList();
            }
            catch { /* ignore */ }

            // Language breakdown
            try
            {
                var langs = await _client.Repository.GetAllLanguages(owner, name);
                vm.Languages = new LanguageBreakdown
                {
                    Languages = langs.ToDictionary(l => l.Name, l => l.NumberOfBytes)
                };
            }
            catch { /* ignore */ }

            await _cache.SetAsync(cacheKey, vm, TimeSpan.FromMinutes(15));
            return vm;
        }
        catch (NotFoundException)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching repo {Owner}/{Name}", owner, name);
            return null;
        }
    }

    public async Task<UserProfileViewModel?> GetUserAsync(string login)
    {
        var cacheKey = $"user:{login}";
        var cached   = await _cache.GetAsync<UserProfileViewModel>(cacheKey);
        if (cached is not null) return cached;

        try
        {
            var user  = await _client.User.Get(login);
            var repos = await _client.Repository.GetAllForUser(login,
                new ApiOptions { PageSize = 30, PageCount = 1 });

            var vm = new UserProfileViewModel
            {
                Login       = user.Login,
                Name        = user.Name,
                AvatarUrl   = user.AvatarUrl,
                Bio         = user.Bio,
                Company     = user.Company,
                Location    = user.Location,
                Blog        = user.Blog,
                HtmlUrl     = user.HtmlUrl,
                PublicRepos = user.PublicRepos,
                Followers   = user.Followers,
                Following   = user.Following,
                CreatedAt   = user.CreatedAt.UtcDateTime,
                Repositories = repos.Select(MapToViewModel).ToList()
            };

            await _cache.SetAsync(cacheKey, vm, TimeSpan.FromMinutes(20));
            return vm;
        }
        catch (NotFoundException) { return null; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {Login}", login);
            return null;
        }
    }

    public async Task<TrendingViewModel> GetTrendingAsync(string language, string since)
    {
        var cacheKey = $"trending:{language}:{since}";
        var cached   = await _cache.GetAsync<TrendingViewModel>(cacheKey);
        if (cached is not null) return cached;

        try
        {
            var dateFilter = since switch
            {
                "weekly"  => DateTime.UtcNow.AddDays(-7),
                "monthly" => DateTime.UtcNow.AddDays(-30),
                _         => DateTime.UtcNow.AddDays(-1)
            };

            var request = new SearchRepositoriesRequest
            {
                SortField = RepoSearchSort.Stars,
                Order     = SortDirection.Descending,
                Created   = new DateRange(dateFilter, SearchQualifierOperator.GreaterThan),
                PerPage   = 25
            };

            if (!string.IsNullOrWhiteSpace(language) && language != "all")
                request.Language = ParseLanguage(language);

            var result = await _client.Search.SearchRepo(request);

            var vm = new TrendingViewModel
            {
                Language   = language,
                Since      = since,
                Repositories = result.Items.Select(r =>
                {
                    var repo = MapToViewModel(r);
                    repo.IsTrending = true;
                    return repo;
                }).ToList(),
                AvailableLanguages = GetAvailableLanguages()
            };

            await _cache.SetAsync(cacheKey, vm, TimeSpan.FromMinutes(30));
            return vm;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching trending repos");
            return new TrendingViewModel { Language = language, Since = since };
        }
    }

    public async Task<CompareViewModel?> CompareRepositoriesAsync(string repo1, string repo2)
    {
        var vm = new CompareViewModel { Repo1Input = repo1, Repo2Input = repo2 };

        var results = await Task.WhenAll(
            FetchSingleRepo(repo1),
            FetchSingleRepo(repo2)
        );

        vm.Repo1 = results[0];
        vm.Repo2 = results[1];
        return vm;
    }

    // ── Private helpers ───────────────────────────────────────────

    private async Task<RepositoryViewModel?> FetchSingleRepo(string fullName)
    {
        var parts = fullName.Split('/');
        if (parts.Length != 2) return null;
        var detail = await GetRepositoryAsync(parts[0], parts[1]);
        return detail?.Repo;
    }

    private SearchRepositoriesRequest BuildSearchRequest(ExploreFilterModel filter)
    {
        var query = string.IsNullOrWhiteSpace(filter.Query) ? "stars:>100" : filter.Query;

        var request = new SearchRepositoriesRequest(query)
        {
            SortField = filter.Sort switch
            {
                "forks"   => RepoSearchSort.Forks,
                "updated" => RepoSearchSort.Updated,
                _         => RepoSearchSort.Stars
            },
            Order   = filter.Order == "asc" ? SortDirection.Ascending : SortDirection.Descending,
            Page    = filter.Page,
            PerPage = filter.PageSize
        };

        if (!string.IsNullOrWhiteSpace(filter.Language))
            request.Language = ParseLanguage(filter.Language);

        if (!string.IsNullOrWhiteSpace(filter.Topic))
            request.Topic = filter.Topic;

        if (filter.MinStars.HasValue)
            request.Stars = new Octokit.Range(filter.MinStars.Value, int.MaxValue);

        return request;
    }

    private static RepositoryViewModel MapToViewModel(Repository repo) => new()
    {
        Id          = repo.Id,
        Owner       = repo.Owner.Login,
        Name        = repo.Name,
        Description = repo.Description,
        Language    = repo.Language,
        LanguageColor = repo.Language is not null && LanguageColors.TryGetValue(repo.Language, out var color)
            ? color : "#8b949e",
        Stars      = repo.StargazersCount,
        Forks      = repo.ForksCount,
        OpenIssues = repo.OpenIssuesCount,
        Watchers   = repo.WatchersCount,
        License    = repo.License?.SpdxId,
        HtmlUrl    = repo.HtmlUrl,
        OwnerAvatarUrl = repo.Owner.AvatarUrl,
        OwnerHtmlUrl   = repo.Owner.HtmlUrl,
        Topics    = repo.Topics?.ToList() ?? new(),
        IsPrivate = repo.Private,
        IsFork    = repo.Fork,
        UpdatedAt = repo.UpdatedAt.UtcDateTime,
        PushedAt  = repo.PushedAt?.UtcDateTime,
        CreatedAt = repo.CreatedAt.UtcDateTime
    };

    private static Language? ParseLanguage(string lang) => lang.ToLower() switch
    {
        "c#" or "csharp"          => Octokit.Language.CSharp,
        "javascript" or "js"      => Octokit.Language.JavaScript,
        "typescript" or "ts"      => Octokit.Language.TypeScript,
        "python"                  => Octokit.Language.Python,
        "go"                      => Octokit.Language.Go,
        "rust"                    => Octokit.Language.Rust,
        "java"                    => Octokit.Language.Java,
        "kotlin"                  => Octokit.Language.Kotlin,
        "swift"                   => Octokit.Language.Swift,
        "ruby"                    => Octokit.Language.Ruby,
        "php"                     => Octokit.Language.Php,
        "c"                       => Octokit.Language.C,
        "c++" or "cpp"            => Octokit.Language.CPlusPlus,
        "shell"                   => Octokit.Language.Shell,
        "dart"                    => Octokit.Language.Dart,
        _                         => null
    };

    private static List<string> GetAvailableLanguages() =>
    [
        "all", "C#", "JavaScript", "TypeScript", "Python", "Go",
        "Rust", "Java", "Kotlin", "Swift", "Ruby", "PHP", "C", "C++", "Shell", "Dart"
    ];

    /// <summary>
    /// Converts basic Markdown to HTML for README display.
    /// Handles headings, bold, italic, code blocks, inline code, links, images, lists.
    /// No external dependencies needed.
    /// </summary>
    private static string ConvertMarkdownToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;

        var lines   = markdown.Replace("\r\n", "\n").Split('\n');
        var html    = new System.Text.StringBuilder();
        bool inCode = false;
        bool inList = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine;

            // ── Fenced code block ──────────────────────────────
            if (line.TrimStart().StartsWith("```"))
            {
                if (!inCode)
                {
                    if (inList) { html.Append("</ul>"); inList = false; }
                    var lang = line.TrimStart().TrimStart('`').Trim();
                    html.Append($"<pre class=\"readme-code\"><code{(lang.Length > 0 ? $" class=\"language-{System.Web.HttpUtility.HtmlEncode(lang)}\"" : "")}>");
                    inCode = true;
                }
                else
                {
                    html.Append("</code></pre>");
                    inCode = false;
                }
                continue;
            }

            if (inCode)
            {
                html.AppendLine(System.Web.HttpUtility.HtmlEncode(line));
                continue;
            }

            // ── Empty line → paragraph break ──────────────────
            if (string.IsNullOrWhiteSpace(line))
            {
                if (inList) { html.Append("</ul>"); inList = false; }
                html.Append("<br/>");
                continue;
            }

            // ── Headings ───────────────────────────────────────
            if (line.StartsWith("######")) { AppendHeading(html, line[6..], 6, inList, ref inList); continue; }
            if (line.StartsWith("#####"))  { AppendHeading(html, line[5..], 5, inList, ref inList); continue; }
            if (line.StartsWith("####"))   { AppendHeading(html, line[4..], 4, inList, ref inList); continue; }
            if (line.StartsWith("###"))    { AppendHeading(html, line[3..], 3, inList, ref inList); continue; }
            if (line.StartsWith("##"))     { AppendHeading(html, line[2..], 2, inList, ref inList); continue; }
            if (line.StartsWith("# "))     { AppendHeading(html, line[2..], 1, inList, ref inList); continue; }

            // ── Horizontal rule ────────────────────────────────
            if (line.TrimStart().StartsWith("---") || line.TrimStart().StartsWith("***"))
            {
                if (inList) { html.Append("</ul>"); inList = false; }
                html.Append("<hr class=\"readme-hr\"/>");
                continue;
            }

            // ── Unordered list ─────────────────────────────────
            if (line.TrimStart().StartsWith("- ") || line.TrimStart().StartsWith("* "))
            {
                if (!inList) { html.Append("<ul class=\"readme-list\">"); inList = true; }
                var item = line.TrimStart().Substring(2);
                html.Append($"<li>{InlineMarkdown(item)}</li>");
                continue;
            }

            // ── Ordered list ───────────────────────────────────
            if (Regex.IsMatch(line.TrimStart(), @"^\d+\. "))
            {
                if (inList) { html.Append("</ul>"); inList = false; }
                if (!html.ToString().EndsWith("</ol>") && !html.ToString().EndsWith("<ol class=\"readme-list\">"))
                    html.Append("<ol class=\"readme-list\">");
                var item = Regex.Replace(line.TrimStart(), @"^\d+\. ", "");
                html.Append($"<li>{InlineMarkdown(item)}</li>");
                continue;
            }

            // ── Blockquote ─────────────────────────────────────
            if (line.TrimStart().StartsWith("> "))
            {
                if (inList) { html.Append("</ul>"); inList = false; }
                var quote = line.TrimStart().Substring(2);
                html.Append($"<blockquote class=\"readme-quote\">{InlineMarkdown(quote)}</blockquote>");
                continue;
            }

            // ── Regular paragraph ──────────────────────────────
            if (inList) { html.Append("</ul>"); inList = false; }
            html.Append($"<p class=\"readme-p\">{InlineMarkdown(line)}</p>");
        }

        if (inList)  html.Append("</ul>");
        if (inCode)  html.Append("</code></pre>");

        return html.ToString();
    }

    private static void AppendHeading(
        System.Text.StringBuilder html,
        string text, int level,
        bool inList, ref bool inListRef)
    {
        if (inList) { html.Append("</ul>"); inListRef = false; }
        html.Append($"<h{level} class=\"readme-h{level}\">{InlineMarkdown(text.Trim())}</h{level}>");
    }

    /// <summary>Processes inline Markdown: bold, italic, code, links, images.</summary>
    private static string InlineMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Escape HTML first (except we'll reinsert tags)
        text = System.Web.HttpUtility.HtmlEncode(text);

        // Images: ![alt](url) — decode HTML entities in URL first
        text = Regex.Replace(text,
            @"!\[([^\]]*)\]\(([^)]+)\)",
            m => $"<img src=\"{System.Web.HttpUtility.HtmlDecode(m.Groups[2].Value)}\" alt=\"{m.Groups[1].Value}\" class=\"readme-img\" loading=\"lazy\" />");

        // Links: [text](url)
        text = Regex.Replace(text,
            @"\[([^\]]+)\]\(([^)]+)\)",
            m => $"<a href=\"{m.Groups[2].Value}\" target=\"_blank\" rel=\"noopener\" class=\"readme-link\">{m.Groups[1].Value}</a>");

        // Inline code: `code`
        text = Regex.Replace(text,
            @"`([^`]+)`",
            m => $"<code class=\"readme-inline-code\">{m.Groups[1].Value}</code>");

        // Bold: **text** or __text__
        text = Regex.Replace(text,
            @"\*\*([^*]+)\*\*|__([^_]+)__",
            m => $"<strong>{m.Groups[1].Value}{m.Groups[2].Value}</strong>");

        // Italic: *text* or _text_
        text = Regex.Replace(text,
            @"\*([^*]+)\*|_([^_]+)_",
            m => $"<em>{m.Groups[1].Value}{m.Groups[2].Value}</em>");

        // Strikethrough: ~~text~~
        text = Regex.Replace(text,
            @"~~([^~]+)~~",
            m => $"<del>{m.Groups[1].Value}</del>");

        return text;
    }
}
