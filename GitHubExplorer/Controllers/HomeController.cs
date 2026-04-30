using GitHubExplorer.Models.ViewModels;
using GitHubExplorer.Services.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Controllers;

public class HomeController : Controller
{
    private readonly IGitHubService _github;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IGitHubService github, ILogger<HomeController> logger)
    {
        _github = github;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            // Static trending topics (sidebar) — GitHub doesn't expose topic counts via REST API
            TrendingTopics =
            [
                new() { Tag = "machine-learning", Count = "48.2k", Delta = "+2.3k" },
                new() { Tag = "react",            Count = "32.1k", Delta = "+890"  },
                new() { Tag = "python",           Count = "28.7k", Delta = "+1.1k" },
                new() { Tag = "docker",           Count = "21.3k", Delta = "+430"  },
                new() { Tag = "api",              Count = "19.8k", Delta = "+670"  },
                new() { Tag = "cli",              Count = "14.5k", Delta = "+210"  },
            ]
        };

        try
        {
            // Fetch real trending repos — top 6 starred repos updated in last 24h
            var trending = await _github.GetTrendingAsync("all", "daily");
            vm.TrendingRepos = trending.Repositories.Take(6).ToList();
            vm.ApiAvailable  = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load trending repos for home page, using fallback");
            vm.ApiAvailable = false;

            // Fallback: basic search for popular repos
            try
            {
                var fallback = await _github.SearchRepositoriesAsync(new ExploreFilterModel
                {
                    Query    = "stars:>50000",
                    Sort     = "stars",
                    PageSize = 6
                });
                vm.TrendingRepos = fallback.Repositories;
            }
            catch
            {
                // If both fail, home page renders without repos (error banner shown)
            }
        }

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    public IActionResult Privacy() => View();
}
