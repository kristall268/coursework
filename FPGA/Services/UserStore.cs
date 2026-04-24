using System.Security.Cryptography;
using System.Text;
using FPGA.Models;

namespace FPGA.Services;

public class UserStore
{
    private readonly List<AppUser> _users = new();
    private int _nextId = 1;

    public UserStore()
    {
        // Seed users for each role
        SeedUser("admin",   "admin@fpga.dev",   "Admin123!",   Roles.Admin);
        SeedUser("teacher", "teacher@fpga.dev", "Teacher123!", Roles.Teacher);
        SeedUser("student", "student@fpga.dev", "Student123!", Roles.Student);
    }

    private void SeedUser(string username, string email, string password, string role)
    {
        _users.Add(new AppUser
        {
            Id           = _nextId++,
            Username     = username,
            Email        = email,
            PasswordHash = Hash(password),
            Role         = role,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        });
    }

    // ── Queries ───────────────────────────────────────────────────────────────
    public AppUser? FindByUsername(string username) =>
        _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public AppUser? FindByEmail(string email) =>
        _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public AppUser? FindById(int id) =>
        _users.FirstOrDefault(u => u.Id == id);

    public List<AppUser> GetAll() => _users.ToList();

    public AppUser? Validate(string username, string password)
    {
        var user = FindByUsername(username);
        if (user is null || !user.IsActive) return null;
        return user.PasswordHash == Hash(password) ? user : null;
    }

    // ── Mutations ─────────────────────────────────────────────────────────────
    public (bool ok, string error) Register(RegisterViewModel vm)
    {
        if (FindByUsername(vm.Username) is not null)
            return (false, "Имя пользователя уже занято.");
        if (FindByEmail(vm.Email) is not null)
            return (false, "Email уже используется.");
        if (vm.Password != vm.ConfirmPassword)
            return (false, "Пароли не совпадают.");
        if (vm.Password.Length < 6)
            return (false, "Пароль должен быть не менее 6 символов.");

        _users.Add(new AppUser
        {
            Id           = _nextId++,
            Username     = vm.Username,
            Email        = vm.Email,
            PasswordHash = Hash(vm.Password),
            Role         = Roles.Student,   // new registrations = student
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        });
        return (true, "");
    }

    public bool UpdateRole(int userId, string newRole)
    {
        var user = FindById(userId);
        if (user is null) return false;
        user.Role = newRole;
        return true;
    }

    public bool ToggleActive(int userId)
    {
        var user = FindById(userId);
        if (user is null) return false;
        user.IsActive = !user.IsActive;
        return true;
    }

    public bool Delete(int userId)
    {
        var user = FindById(userId);
        if (user is null) return false;
        _users.Remove(user);
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
