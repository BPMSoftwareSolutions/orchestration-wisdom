using OrchestrationWisdom.Models;
using System.Text.RegularExpressions;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Extracts metadata from pattern submissions
/// Movement 1, Beat 2: Extract Pattern Metadata
/// </summary>
public interface IPatternMetadataExtractor
{
    Task<PatternMetadata> ExtractAsync(Pattern pattern);
}

public class PatternMetadataExtractor : IPatternMetadataExtractor
{
    /// <summary>
    /// Extracts metadata including diagram analysis and complexity assessment
    /// Event: pattern.metadata.extracted
    /// </summary>
    public Task<PatternMetadata> ExtractAsync(Pattern pattern)
    {
        var metadata = new PatternMetadata
        {
            ExtractedAt = DateTime.UtcNow,
            ActorCountAsIs = CountActors(pattern.AsIsDiagram),
            ActorCountOrchestrated = CountActors(pattern.OrchestratedDiagram),
            StepCountAsIs = CountSteps(pattern.AsIsDiagram),
            StepCountOrchestrated = CountSteps(pattern.OrchestratedDiagram),
            AltBlocksAsIs = CountAltBlocks(pattern.AsIsDiagram),
            AltBlocksOrchestrated = CountAltBlocks(pattern.OrchestratedDiagram),
            TotalWordCount = CalculateWordCount(pattern),
            ComplexityLevel = DetermineComplexity(pattern)
        };

        return Task.FromResult(metadata);
    }

    private int CountActors(string mermaidDiagram)
    {
        if (string.IsNullOrWhiteSpace(mermaidDiagram))
            return 0;

        // Match participant declarations in mermaid diagrams
        var participantPattern = @"participant\s+\w+\s+as";
        var matches = Regex.Matches(mermaidDiagram, participantPattern);
        return matches.Count;
    }

    private int CountSteps(string mermaidDiagram)
    {
        if (string.IsNullOrWhiteSpace(mermaidDiagram))
            return 0;

        // Count arrow operations (->>, ->>+, -->>)
        var stepPattern = @"(->>|->>\\+|-->>|->)";
        var matches = Regex.Matches(mermaidDiagram, stepPattern);
        return matches.Count;
    }

    private int CountAltBlocks(string mermaidDiagram)
    {
        if (string.IsNullOrWhiteSpace(mermaidDiagram))
            return 0;

        // Count alt and else keywords
        var altPattern = @"\b(alt|else)\b";
        var matches = Regex.Matches(mermaidDiagram, altPattern, RegexOptions.IgnoreCase);

        // alt blocks come in pairs or groups, count unique alt declarations
        var altCount = Regex.Matches(mermaidDiagram, @"\balt\b", RegexOptions.IgnoreCase).Count;
        return altCount;
    }

    private int CalculateWordCount(Pattern pattern)
    {
        var allText = string.Join(" ", new[]
        {
            pattern.Hook,
            pattern.ProblemDetail,
            pattern.OrchestrationShift,
            pattern.DecisionPoint,
            pattern.Metrics,
            pattern.Checklist,
            pattern.ClosingInsight
        });

        var words = allText.Split(new[] { ' ', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        return words.Length;
    }

    private string DetermineComplexity(Pattern pattern)
    {
        // Simple heuristic based on scorecard and content
        var totalScore = pattern.Scorecard.TotalScore;
        var componentCount = pattern.Components.Count;

        if (totalScore >= 35 && componentCount >= 5)
            return "Advanced";
        else if (totalScore >= 28 && componentCount >= 3)
            return "Intermediate";
        else
            return "Basic";
    }
}
