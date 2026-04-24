namespace FPGA.Models;

// ── Role Constants ────────────────────────────────────────────────────────────
public static class Roles
{
    public const string Admin   = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    // Guest = unauthenticated user (no role needed)
}

// ── Application User ──────────────────────────────────────────────────────────
public class AppUser
{
    public int    Id           { get; set; }
    public string Username     { get; set; } = "";
    public string Email        { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role         { get; set; } = Roles.Student;
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public bool   IsActive     { get; set; } = true;
}

// ── View Models ───────────────────────────────────────────────────────────────
public class LoginViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool   Remember { get; set; }
}

public class RegisterViewModel
{
    public string Username        { get; set; } = "";
    public string Email           { get; set; } = "";
    public string Password        { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}

public class UserAdminViewModel
{
    public List<AppUser> Users    { get; set; } = new();
    public AppUser?      EditUser { get; set; }
}
