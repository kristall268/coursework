using Microsoft.AspNetCore.Mvc;
using FPGA.Models;

namespace FPGA.Controllers;

public class ProjectsController : Controller
{
    private static readonly List<FpgaProject> _projects = new()
    {
        new() { Title = "UART",          Difficulty = "Intermediate", Tags = new[] { "Communication", "Serial"    }, Slug = "uart" },
        new() { Title = "VGA Controller",Difficulty = "Advanced",     Tags = new[] { "Display",       "Graphics"  }, Slug = "vga" },
        new() { Title = "LED Blinker",   Difficulty = "Beginner",     Tags = new[] { "I/O",           "Simple"    }, Slug = "led-blinker" },
        new() { Title = "Simple CPU",    Difficulty = "Advanced",     Tags = new[] { "Processor",     "Complex"   }, Slug = "simple-cpu" },
        new() { Title = "SPI Master",    Difficulty = "Intermediate", Tags = new[] { "Communication", "Serial"    }, Slug = "spi" },
        new() { Title = "PWM Generator", Difficulty = "Beginner",     Tags = new[] { "Signal",        "Output"    }, Slug = "pwm" },
    };

    private static readonly List<HdlModule> _modules = new()
    {
        new() { Title = "4-Bit Counter Module",        Language = "Verilog",       Difficulty = "Intermediate", Description = "A synchronous 4-bit counter with reset and enable signals" },
        new() { Title = "Full Adder",                  Language = "VHDL",          Difficulty = "Beginner",     Description = "Combinational full adder with carry in/out" },
        new() { Title = "FSM Traffic Light",           Language = "SystemVerilog", Difficulty = "Intermediate", Description = "Finite state machine for traffic light controller" },
        new() { Title = "FIFO Buffer",                 Language = "Verilog",       Difficulty = "Advanced",     Description = "Synchronous FIFO buffer with full/empty flags" },
        new() { Title = "Parameterized Shift Register",Language = "Verilog",       Difficulty = "Beginner",     Description = "Configurable-width shift register with load" },
    };

    public IActionResult Index()
    {
        ViewData["Title"] = "Projects & HDL Explorer";
        var vm = new ProjectsViewModel
        {
            Projects = _projects,
            HdlModules = _modules
        };
        return View(vm);
    }

    public IActionResult Details(string slug)
    {
        var project = _projects.FirstOrDefault(p => p.Slug == slug);
        if (project is null) return NotFound();
        ViewData["Title"] = project.Title;
        return View(project);
    }
}
