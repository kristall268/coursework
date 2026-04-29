// Controllers/RepoController.cs
using GitHubExplorer.Services.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Controllers;

public class RepoController : Controller
{
    private readonly IGitHubService _github;

    public RepoController(IGitHubService github)
    {
        _github = github;
    }

    // GET /repo/{owner}/{name}
    public async Task<IActionResult> Detail(string owner, string name)
    {
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(name))
            return BadRequest();

        var vm = await _github.GetRepositoryAsync(owner, name);

        if (vm is null)
            return NotFound();

        return View(vm);
    }
}