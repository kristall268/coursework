// Controllers/TrendingController.cs
using GitHubExplorer.Services.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Controllers;

public class TrendingController : Controller
{
    private readonly IGitHubService _github;

    public TrendingController(IGitHubService github)
    {
        _github = github;
    }

    // GET /trending?language=all&since=daily
    public async Task<IActionResult> Index(
        string language = "all",
        string since = "daily")
    {
        var vm = await _github.GetTrendingAsync(language, since);
        return View(vm);
    }
}