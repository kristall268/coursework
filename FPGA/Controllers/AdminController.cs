using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FPGA.Models;
using FPGA.Services;

namespace FPGA.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly UserStore _users;

    public AdminController(UserStore users) => _users = users;

    // ── USER LIST ─────────────────────────────────────────────────────────────
    public IActionResult Index()
    {
        ViewData["Title"] = "Управление пользователями";
        var vm = new UserAdminViewModel { Users = _users.GetAll() };
        return View(vm);
    }

    // ── CHANGE ROLE ───────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetRole(int userId, string role)
    {
        var allowed = new[] { Roles.Admin, Roles.Teacher, Roles.Student };
        if (!allowed.Contains(role))
        {
            TempData["Error"] = "Недопустимая роль.";
            return RedirectToAction(nameof(Index));
        }

        _users.UpdateRole(userId, role);
        TempData["Success"] = "Роль обновлена.";
        return RedirectToAction(nameof(Index));
    }

    // ── TOGGLE ACTIVE ─────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int userId)
    {
        _users.ToggleActive(userId);
        TempData["Success"] = "Статус пользователя изменён.";
        return RedirectToAction(nameof(Index));
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int userId)
    {
        // Prevent self-deletion
        var selfId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (userId == selfId)
        {
            TempData["Error"] = "Нельзя удалить собственный аккаунт.";
            return RedirectToAction(nameof(Index));
        }

        _users.Delete(userId);
        TempData["Success"] = "Пользователь удалён.";
        return RedirectToAction(nameof(Index));
    }
}
