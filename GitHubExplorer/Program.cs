using GitHubExplorer.Data;
using GitHubExplorer.Services.Cache;
using GitHubExplorer.Services.GitHub;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Octokit;
using Serilog;

// ── Serilog ──────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gitexplorer-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ── MVC ──────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── PostgreSQL + EF Core ─────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Memory Cache ─────────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ── Octokit GitHub Client (singleton) ────────────────────────────
builder.Services.AddSingleton(_ =>
{
    var token = builder.Configuration["GitHub:ServerToken"]
                ?? throw new InvalidOperationException("GitHub:ServerToken is not configured.");

    return new GitHubClient(new ProductHeaderValue("GitHubExplorer"))
    {
        Credentials = new Credentials(token)
    };
});

// ── App Services ─────────────────────────────────────────────────
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<ICacheService, CacheService>();

// ── GitHub OAuth ─────────────────────────────────────────────────
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "GitHub";
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/account/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["GitHub:ClientId"]
                           ?? throw new InvalidOperationException("GitHub:ClientId is not configured.");
        options.ClientSecret = builder.Configuration["GitHub:ClientSecret"]
                               ?? throw new InvalidOperationException("GitHub:ClientSecret is not configured.");
        options.CallbackPath = "/account/callback";
        options.Scope.Add("read:user");
        options.Scope.Add("user:email");
        options.SaveTokens = true;
    });

// ── Build ─────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ── Middleware pipeline ───────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

// ── Routes ────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "repo",
    pattern: "repo/{owner}/{name}",
    defaults: new { controller = "Repo", action = "Detail" });

app.MapControllerRoute(
    name: "user",
    pattern: "user/{login}",
    defaults: new { controller = "User", action = "Profile" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();