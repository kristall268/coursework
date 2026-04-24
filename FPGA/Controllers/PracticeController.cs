using Microsoft.AspNetCore.Mvc;
using FPGA.Models;

namespace FPGA.Controllers;

public class PracticeController : Controller
{
    private static readonly List<PracticeProblem> _problems = new()
    {
        new() { Title = "Design a 4:1 Multiplexer",          Difficulty = "Beginner",     Slug = "mux-4to1" },
        new() { Title = "Implement Full Adder",              Difficulty = "Beginner",     Slug = "full-adder" },
        new() { Title = "Create 8-Bit Register",             Difficulty = "Intermediate", Slug = "8bit-register" },
        new() { Title = "Design ALU (Arithmetic Logic Unit)",Difficulty = "Advanced",     Slug = "alu-design" },
    };

    public IActionResult Index()
    {
        ViewData["Title"] = "Practice Problems";
        return View(_problems);
    }

    public IActionResult Solve(string slug)
    {
        var problem = _problems.FirstOrDefault(p => p.Slug == slug);
        if (problem is null) return NotFound();
        ViewData["Title"] = $"Solve: {problem.Title}";
        return View(problem);
    }
}
