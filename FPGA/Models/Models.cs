namespace FPGA.Models;

public class LearningStep
{
    public string Icon        { get; set; } = "";
    public string Title       { get; set; } = "";
    public string Description { get; set; } = "";
    public int    Order        { get; set; }
}

public class ComponentItem
{
    public string Title       { get; set; } = "";
    public string Description { get; set; } = "";
    public string Slug        { get; set; } = "";
    public string? VerilogCode { get; set; }
    public string? VhdlCode    { get; set; }
}

public class FpgaProject
{
    public string   Title      { get; set; } = "";
    public string   Difficulty { get; set; } = "";
    public string[] Tags       { get; set; } = Array.Empty<string>();
    public string   Slug       { get; set; } = "";
}

public class HdlModule
{
    public string Title       { get; set; } = "";
    public string Language    { get; set; } = "";
    public string Difficulty  { get; set; } = "";
    public string Description { get; set; } = "";
}

public class PracticeProblem
{
    public string Title      { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Slug       { get; set; } = "";
}

// ── View Models ──────────────────────────────────────────────────────────────

public class HomeViewModel
{
    public List<LearningStep>  LearningSteps      { get; set; } = new();
    public List<ComponentItem> FeaturedComponents { get; set; } = new();
}

public class ProjectsViewModel
{
    public List<FpgaProject> Projects   { get; set; } = new();
    public List<HdlModule>   HdlModules { get; set; } = new();
}
