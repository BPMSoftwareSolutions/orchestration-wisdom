namespace OrchestrationWisdom.Models;

public class FilterOptions
{
    public List<string> Industries { get; set; } = new();
    public List<string> ProblemTypes { get; set; } = new();
    public List<string> BrokenSignals { get; set; } = new();
    public List<string> MaturityLevels { get; set; } = new();
    public string SortBy { get; set; } = "referenced";
}

public static class FilterConstants
{
    public static readonly List<string> Industries = new()
    {
        "Technology",
        "Healthcare",
        "Finance",
        "Retail",
        "Manufacturing",
        "Education",
        "Professional Services"
    };

    public static readonly List<string> ProblemTypes = new()
    {
        "Backlog",
        "Escalation",
        "Capacity",
        "SLA Failure",
        "Handoffs"
    };

    public static readonly List<string> BrokenSignals = new()
    {
        "Ownership",
        "Time/SLA",
        "Capacity",
        "Visibility"
    };

    public static readonly List<string> MaturityLevels = new()
    {
        "Early",
        "Mid",
        "Advanced"
    };

    public static readonly List<string> SortOptions = new()
    {
        "referenced",
        "newest",
        "clarity"
    };
}
