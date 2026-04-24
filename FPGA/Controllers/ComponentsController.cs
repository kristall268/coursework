using Microsoft.AspNetCore.Mvc;
using FPGA.Models;

namespace FPGA.Controllers;

public class ComponentsController : Controller
{
    private static readonly List<ComponentItem> _components = new()
    {
        new() { Title = "Adder",           Description = "Binary addition circuit",                    Slug = "adder" },
        new() { Title = "Subtractor",      Description = "Binary subtraction logic",                   Slug = "subtractor" },
        new() { Title = "Multiplexer",     Description = "Data selector circuit",                      Slug = "multiplexer" },
        new() { Title = "Decoder",         Description = "Binary to one-hot decoder",                  Slug = "decoder" },
        new() { Title = "Encoder",         Description = "Priority encoder logic",                      Slug = "encoder" },
        new() { Title = "Flip-Flop (D)",   Description = "Edge-triggered D flip-flop",                 Slug = "dff" },
        new() { Title = "Register",        Description = "N-bit parallel load register",               Slug = "register" },
        new() { Title = "Counter",         Description = "Synchronous up/down counter",                Slug = "counter" },
        new() { Title = "ALU",             Description = "Arithmetic logic unit",                      Slug = "alu" },
    };

    public IActionResult Index()
    {
        ViewData["Title"] = "Digital Components Library";
        return View(_components);
    }

    public IActionResult Details(string slug)
    {
        var component = _components.FirstOrDefault(c => c.Slug == slug);
        if (component is null) return NotFound();
        ViewData["Title"] = component.Title;
        return View(component);
    }
}
