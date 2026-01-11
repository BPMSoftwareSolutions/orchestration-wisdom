namespace OrchestrationWisdom.Models;

public class Capability
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> OrchestrationComponents { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public string TimeToValueRange { get; set; } = string.Empty;
    public string ScaleFit { get; set; } = string.Empty;
    public List<string> DoesNotSolve { get; set; } = new();
}
