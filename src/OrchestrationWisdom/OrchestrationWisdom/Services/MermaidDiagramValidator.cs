using OrchestrationWisdom.Models;
using System.Text.RegularExpressions;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Validates Mermaid diagrams against budget constraints
/// Movement 2, Beat 6: Validate Diagram Budgets
/// </summary>
public interface IMermaidDiagramValidator
{
    Task<ValidationResult> ValidateDiagramsAsync(string asIsDiagram, string orchestratedDiagram);
}

public class MermaidDiagramValidator : IMermaidDiagramValidator
{
    private const int MaxActors = 7;
    private const int MaxSteps = 18;
    private const int MaxAltBlocks = 2;

    /// <summary>
    /// Validates diagrams against budget constraints
    /// Event: pattern.diagrams.validated
    /// Budget Constraints:
    /// - ≤7 actors per diagram
    /// - ≤18 steps per diagram
    /// - ≤2 alt blocks per diagram
    /// - No nested alt blocks
    /// </summary>
    public Task<ValidationResult> ValidateDiagramsAsync(string asIsDiagram, string orchestratedDiagram)
    {
        var result = new ValidationResult
        {
            ValidationType = "Diagram Budget",
            ValidatedAt = DateTime.UtcNow,
            Errors = new List<ValidationError>()
        };

        // Validate As-Is diagram
        ValidateDiagram(result.Errors, asIsDiagram, "AsIsDiagram", "As-Is Diagram");

        // Validate Orchestrated diagram
        ValidateDiagram(result.Errors, orchestratedDiagram, "OrchestratedDiagram", "Orchestrated Diagram");

        result.IsValid = !result.Errors.Any();

        return Task.FromResult(result);
    }

    private void ValidateDiagram(List<ValidationError> errors, string diagram, string fieldName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(diagram))
        {
            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{displayName} is missing or empty",
                Severity = "Critical",
                RemediationGuidance = $"Provide a valid Mermaid sequence diagram for {displayName}"
            });
            return;
        }

        // Count actors
        var actorCount = CountActors(diagram);
        if (actorCount > MaxActors)
        {
            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{displayName} exceeds actor budget ({actorCount}/{MaxActors})",
                Severity = "Critical",
                RemediationGuidance = $"Reduce the number of actors to {MaxActors} or fewer. Consider grouping related actors or simplifying the workflow. Current count: {actorCount}"
            });
        }

        // Count steps
        var stepCount = CountSteps(diagram);
        if (stepCount > MaxSteps)
        {
            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{displayName} exceeds step budget ({stepCount}/{MaxSteps})",
                Severity = "Critical",
                RemediationGuidance = $"Reduce the number of steps to {MaxSteps} or fewer. Consider breaking into multiple patterns or removing unnecessary steps. Current count: {stepCount}"
            });
        }

        // Count alt blocks
        var altBlockCount = CountAltBlocks(diagram);
        if (altBlockCount > MaxAltBlocks)
        {
            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{displayName} exceeds alt block budget ({altBlockCount}/{MaxAltBlocks})",
                Severity = "Critical",
                RemediationGuidance = $"Reduce the number of alt blocks to {MaxAltBlocks} or fewer. Current count: {altBlockCount}"
            });
        }

        // Check for nested alt blocks
        if (HasNestedAltBlocks(diagram))
        {
            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{displayName} contains nested alt blocks",
                Severity = "Critical",
                RemediationGuidance = "Diagrams cannot contain nested alt blocks. Simplify the diagram to remove nesting or split into separate patterns"
            });
        }
    }

    private int CountActors(string diagram)
    {
        var participantPattern = @"participant\s+\w+\s+as";
        var matches = Regex.Matches(diagram, participantPattern);
        return matches.Count;
    }

    private int CountSteps(string diagram)
    {
        // Count various arrow types in Mermaid sequence diagrams
        var stepPattern = @"(->>|->>\\+|-->>|->)";
        var matches = Regex.Matches(diagram, stepPattern);
        return matches.Count;
    }

    private int CountAltBlocks(string diagram)
    {
        // Count alt keyword occurrences (each represents an alt block start)
        var altPattern = @"\balt\b";
        var matches = Regex.Matches(diagram, altPattern, RegexOptions.IgnoreCase);
        return matches.Count;
    }

    private bool HasNestedAltBlocks(string diagram)
    {
        // Simple check for nested alt blocks by tracking depth
        var lines = diagram.Split('\n');
        var altDepth = 0;
        var maxDepth = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim().ToLower();

            if (trimmedLine.StartsWith("alt "))
            {
                altDepth++;
                maxDepth = Math.Max(maxDepth, altDepth);
            }
            else if (trimmedLine == "end")
            {
                if (altDepth > 0)
                    altDepth--;
            }
        }

        // If max depth > 1, we have nesting
        return maxDepth > 1;
    }
}
