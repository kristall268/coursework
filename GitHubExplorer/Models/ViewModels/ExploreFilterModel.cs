namespace GitHubExplorer.Models.ViewModels;

public class ExploreFilterModel
{
    public string? Query { get; set; }
    public string? Language { get; set; }
    public string? Topic { get; set; }
    public string? License { get; set; }
    public int? MinStars { get; set; }
    public int? MaxStars { get; set; }
    public string Sort { get; set; } = "stars";
    public string Order { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;

    /// <summary>Builds a cache key from the current filter state</summary>
    public string ToCacheKey()
    {
        return $"search:{Query}:{Language}:{Topic}:{License}:{MinStars}:{MaxStars}:{Sort}:{Order}:{Page}";
    }
}