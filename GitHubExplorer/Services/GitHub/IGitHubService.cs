using GitHubExplorer.Models.ViewModels;

namespace GitHubExplorer.Services.GitHub;

public interface IGitHubService
{
    Task<ExploreViewModel> SearchRepositoriesAsync(ExploreFilterModel filter);
    Task<RepoDetailViewModel?> GetRepositoryAsync(string owner, string name);
    Task<UserProfileViewModel?> GetUserAsync(string login);
    Task<TrendingViewModel> GetTrendingAsync(string language, string since);
    Task<CompareViewModel?> CompareRepositoriesAsync(string repo1, string repo2);
}