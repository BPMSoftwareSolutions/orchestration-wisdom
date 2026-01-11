namespace OrchestrationWisdom.Models;

public class Pattern
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string OrchestrationShift { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string ProblemDetail { get; set; } = string.Empty;
    public string AsIsDiagram { get; set; } = string.Empty;
    public string OrchestratedDiagram { get; set; } = string.Empty;
    public string DecisionPoint { get; set; } = string.Empty;
    public string Metrics { get; set; } = string.Empty;
    public string Checklist { get; set; } = string.Empty;
    public OrchestrationScorecard Scorecard { get; set; } = new();
    public string ClosingInsight { get; set; } = string.Empty;
    public List<string> Industries { get; set; } = new();
    public List<string> BrokenSignals { get; set; } = new();
    public string MaturityLevel { get; set; } = string.Empty;
    public List<Component> Components { get; set; } = new();
    public DateTime PublishedDate { get; set; }
    public int ClarityScore { get; set; }
    public int ReferenceCount { get; set; }
}

public class OrchestrationScorecard
{
    public int Ownership { get; set; }
    public int TimeSLA { get; set; }
    public int Capacity { get; set; }
    public int Visibility { get; set; }
    public int CustomerLoop { get; set; }
    public int Escalation { get; set; }
    public int Handoffs { get; set; }
    public int Documentation { get; set; }

    public int TotalScore => Ownership + TimeSLA + Capacity + Visibility +
                            CustomerLoop + Escalation + Handoffs + Documentation;
}

public class Component
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> Prerequisites { get; set; } = new();
    public string ExpectedTimeline { get; set; } = string.Empty;
}
