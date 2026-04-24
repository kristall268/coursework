using Microsoft.AspNetCore.Mvc;
using FPGA.Models;

namespace FPGA.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var model = new HomeViewModel
        {
            LearningSteps = new List<LearningStep>
            {
                new() { Icon = "⚡", Title = "Electronics Basics",   Description = "Fundamental concepts of electronic circuits",   Order = 1 },
                new() { Icon = "</>", Title = "Digital Logic",        Description = "Boolean algebra and logic gates",               Order = 2 },
                new() { Icon = "</>", Title = "HDL Programming",      Description = "Verilog, VHDL, and SystemVerilog",              Order = 3 },
                new() { Icon = "⚙",  Title = "FPGA Design",          Description = "Implementation and synthesis",                  Order = 4 },
                new() { Icon = "↗",  Title = "FPGA Projects",        Description = "Real-world applications",                       Order = 5 }
            },
            FeaturedComponents = new List<ComponentItem>
            {
                new() { Title = "Adder",       Description = "Binary addition circuit",   Slug = "adder" },
                new() { Title = "Subtractor",  Description = "Binary subtraction logic",  Slug = "subtractor" },
                new() { Title = "Multiplexer", Description = "Data selector circuit",     Slug = "multiplexer" }
            }
        };
        return View(model);
    }
}
