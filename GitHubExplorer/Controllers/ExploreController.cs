// Controllers/ExploreController.cs
using GitHubExplorer.Models.ViewModels;
using GitHubExplorer.Services.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Controllers;

public class ExploreController : Controller
{
    private readonly IGitHubService _github;

    public ExploreController(IGitHubService github)
    {
        _github = github;
    }

    // GET /explore
    public async Task<IActionResult> Index([FromQuery] ExploreFilterModel filter)
    {
        var vm = await _github.SearchRepositoriesAsync(filter);
        
        // AJAX-запрос (смена фильтра без перезагрузки)
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_RepoList", vm);

        return View(vm);
    }
}