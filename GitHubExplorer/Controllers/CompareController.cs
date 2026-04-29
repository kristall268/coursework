// Controllers/CompareController.cs
using GitHubExplorer.Services.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Controllers;

public class CompareController : Controller
{
    private readonly IGitHubService _github;

    public CompareController(IGitHubService github)
    {
        _github = github;
    }

    // GET /compare?repo1=facebook/react&repo2=vuejs/vue
    public async Task<IActionResult> Index(
        string? repo1 = null,
        string? repo2 = null)
    {
        // Форма без параметров — пустая страница
        if (string.IsNullOrWhiteSpace(repo1) || string.IsNullOrWhiteSpace(repo2))
            return View(null);

        var vm = await _github.CompareRepositoriesAsync(repo1, repo2);
        return View(vm);
    }
}