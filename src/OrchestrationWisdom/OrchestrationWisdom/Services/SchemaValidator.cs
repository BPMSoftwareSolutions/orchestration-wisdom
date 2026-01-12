using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Validates patterns against schema requirements
/// Movement 2, Beat 4: Validate Against Schema
/// </summary>
public interface ISchemaValidator
{
    Task<ValidationResult> ValidateAsync(Pattern pattern);
}

public class SchemaValidator : ISchemaValidator
{
    /// <summary>
    /// Validates pattern against schema, checking all required fields
    /// Event: pattern.schema.validated
    /// </summary>
    public Task<ValidationResult> ValidateAsync(Pattern pattern)
    {
        var result = new ValidationResult
        {
            ValidationType = "Schema",
            ValidatedAt = DateTime.UtcNow,
            Errors = new List<ValidationError>()
        };

        // Validate required fields
        ValidateRequiredField(result.Errors, pattern.Id, "Id", "Pattern ID");
        ValidateRequiredField(result.Errors, pattern.Title, "Title", "Pattern title");
        ValidateRequiredField(result.Errors, pattern.Hook, "Hook", "Pattern hook (hookMarkdown)");
        ValidateRequiredField(result.Errors, pattern.ProblemDetail, "ProblemDetail", "Problem detail");
        ValidateRequiredField(result.Errors, pattern.AsIsDiagram, "AsIsDiagram", "As-Is diagram (asIsDiagramMermaid)");
        ValidateRequiredField(result.Errors, pattern.OrchestratedDiagram, "OrchestratedDiagram", "Orchestrated diagram (orchestratedDiagramMermaid)");
        ValidateRequiredField(result.Errors, pattern.DecisionPoint, "DecisionPoint", "Decision point");
        ValidateRequiredField(result.Errors, pattern.Metrics, "Metrics", "Metrics section");
        ValidateRequiredField(result.Errors, pattern.Checklist, "Checklist", "Implementation checklist");
        ValidateRequiredField(result.Errors, pattern.ClosingInsight, "ClosingInsight", "Closing insight");

        // Validate scorecard exists and is not default
        if (pattern.Scorecard == null || pattern.Scorecard.TotalScore == 0)
        {
            result.Errors.Add(new ValidationError
            {
                Field = "Scorecard",
                Message = "Pattern scorecard is required with valid scores",
                Severity = "Critical",
                RemediationGuidance = "Provide HQO scorecard with all 8 dimensions scored (Ownership, TimeSLA, Capacity, Visibility, CustomerLoop, Escalation, Handoffs, Documentation)"
            });
        }

        // Validate collections
        if (pattern.Industries == null || !pattern.Industries.Any())
        {
            result.Errors.Add(new ValidationError
            {
                Field = "Industries",
                Message = "At least one industry must be specified",
                Severity = "Major",
                RemediationGuidance = "Add relevant industries for this pattern (e.g., Technology, Healthcare, Finance)"
            });
        }

        if (pattern.BrokenSignals == null || !pattern.BrokenSignals.Any())
        {
            result.Errors.Add(new ValidationError
            {
                Field = "BrokenSignals",
                Message = "At least one broken signal must be specified",
                Severity = "Major",
                RemediationGuidance = "Identify the orchestration signals that are broken (e.g., Ownership, Visibility, Handoffs)"
            });
        }

        result.IsValid = !result.Errors.Any();

        return Task.FromResult(result);
    }

    private void ValidateRequiredField(List<ValidationError> errors, string? value, string fieldName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new ValidationError
            {
                Field = fieldName,
                Message = $"{displayName} is required but was not provided",
                Severity = "Critical",
                RemediationGuidance = $"Provide a valid value for {displayName}"
            });
        }
    }
}
