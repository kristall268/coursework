// Controllers/UserController.cs
using GitHubExplorer.Services.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace GitHubExplorer.Controllers;

public class UserController : Controller
{
    private readonly IGitHubService _github;

    public UserController(IGitHubService github)
    {
        _github = github;
    }

    // GET /user/{login}
    public async Task<IActionResult> Profile(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest();

        var vm = await _github.GetUserAsync(login);

        if (vm is null)
            return NotFound();

        return View(vm);
    }
}