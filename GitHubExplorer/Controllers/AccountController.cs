// Controllers/AccountController.cs
using GitHubExplorer.Data;
using GitHubExplorer.Models.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GitHubExplorer.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    // GET /account/login
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Collections");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // GET /account/signin — редирект на GitHub OAuth
    public IActionResult SignIn(string? returnUrl = null)
    {
        var redirectUrl = Url.Action("Callback", "Account",
            new { returnUrl }, Request.Scheme);

        return Challenge(
            new AuthenticationProperties { RedirectUri = redirectUrl },
            "GitHub");
    }

    // GET /account/callback — GitHub перенаправляет сюда после OAuth
    public async Task<IActionResult> Callback(string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return RedirectToAction("Login");

        // Читаем claims из GitHub
        var claims      = result.Principal!.Claims.ToList();
        var gitHubLogin = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
        var gitHubId    = long.TryParse(
            claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id : 0;
        var avatarUrl   = claims.FirstOrDefault(c => c.Type == "urn:github:avatar")?.Value;
        var email       = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var accessToken = result.Properties?.GetTokenValue("access_token");

        // Создаём или обновляем пользователя в БД
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.GitHubId == gitHubId);

        if (user is null)
        {
            user = new AppUser
            {
                GitHubLogin = gitHubLogin,
                GitHubId    = gitHubId,
                AvatarUrl   = avatarUrl,
                Email       = email,
                AccessToken = accessToken,
                CreatedAt   = DateTime.UtcNow,
            };
            _db.Users.Add(user);
        }
        else
        {
            user.GitHubLogin = gitHubLogin;
            user.AvatarUrl   = avatarUrl;
            user.Email       = email;
            user.AccessToken = accessToken;
            user.LastLoginAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var localUrl = returnUrl is not null && Url.IsLocalUrl(returnUrl)
            ? returnUrl : "/";
        return LocalRedirect(localUrl);
    }

    // POST /account/logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // GET /account/collections
    [Authorize]
    public async Task<IActionResult> Collections()
    {
        var gitHubId = long.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        var user = await _db.Users
            .Include(u => u.Favorites)
            .Include(u => u.Collections)
                .ThenInclude(c => c.Repos)
            .FirstOrDefaultAsync(u => u.GitHubId == gitHubId);

        if (user is null)
            return RedirectToAction("Login");

        return View(user);
    }

    // POST /account/favorite — добавить/убрать из избранного
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(
        string owner, string name,
        string? description, string? language,
        int stars, string? htmlUrl, string? avatarUrl)
    {
        var gitHubId = long.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.GitHubId == gitHubId);

        if (user is null)
            return Unauthorized();

        var existing = await _db.FavoriteRepos
            .FirstOrDefaultAsync(f =>
                f.AppUserId == user.Id &&
                f.Owner == owner &&
                f.Name == name);

        bool isFavorited;

        if (existing is not null)
        {
            _db.FavoriteRepos.Remove(existing);
            isFavorited = false;
        }
        else
        {
            _db.FavoriteRepos.Add(new FavoriteRepo
            {
                AppUserId   = user.Id,
                Owner       = owner,
                Name        = name,
                Description = description,
                Language    = language,
                Stars       = stars,
                HtmlUrl     = htmlUrl,
                AvatarUrl   = avatarUrl,
            });
            isFavorited = true;
        }

        await _db.SaveChangesAsync();

        // Если AJAX — возвращаем JSON
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { isFavorited });

        return RedirectToAction("Collections");
    }
}