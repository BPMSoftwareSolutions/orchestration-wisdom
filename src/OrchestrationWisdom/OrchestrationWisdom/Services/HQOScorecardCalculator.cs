using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Calculates and validates HQO (High-Quality Orchestration) scorecard
/// Movement 2, Beat 5: Calculate HQO Scorecard
/// </summary>
public interface IHQOScorecardCalculator
{
    Task<ValidationResult> CalculateAndValidateAsync(OrchestrationScorecard scorecard);
}

public class HQOScorecardCalculator : IHQOScorecardCalculator
{
    private const int MinDimensionScore = 3;
    private const int MaxDimensionScore = 5;
    private const int MinTotalScore = 30;
    private const int MaxTotalScore = 40;

    /// <summary>
    /// Validates HQO scorecard against publication thresholds
    /// Event: pattern.hqo.calculated
    /// Publication Requirements:
    /// - Each dimension must be ≥3/5
    /// - Total score must be ≥30/40
    /// </summary>
    public Task<ValidationResult> CalculateAndValidateAsync(OrchestrationScorecard scorecard)
    {
        var result = new ValidationResult
        {
            ValidationType = "HQO Scorecard",
            ValidatedAt = DateTime.UtcNow,
            Errors = new List<ValidationError>()
        };

        // Validate individual dimensions
        ValidateDimension(result.Errors, scorecard.Ownership, "Ownership", "Clear ownership assignment and accountability");
        ValidateDimension(result.Errors, scorecard.TimeSLA, "TimeSLA", "Time-based SLAs and response requirements");
        ValidateDimension(result.Errors, scorecard.Capacity, "Capacity", "Capacity planning and load balancing");
        ValidateDimension(result.Errors, scorecard.Visibility, "Visibility", "System visibility and observability");
        ValidateDimension(result.Errors, scorecard.CustomerLoop, "CustomerLoop", "Customer feedback and communication loop");
        ValidateDimension(result.Errors, scorecard.Escalation, "Escalation", "Escalation paths and procedures");
        ValidateDimension(result.Errors, scorecard.Handoffs, "Handoffs", "Clean handoff procedures and protocols");
        ValidateDimension(result.Errors, scorecard.Documentation, "Documentation", "Documentation quality and completeness");

        // Validate total score
        var totalScore = scorecard.TotalScore;

        if (totalScore < MinTotalScore)
        {
            result.Errors.Add(new ValidationError
            {
                Field = "TotalScore",
                Message = $"Total HQO score ({totalScore}/40) is below publication threshold ({MinTotalScore}/40)",
                Severity = "Critical",
                RemediationGuidance = $"Improve weak dimensions to reach minimum total score of {MinTotalScore}. Current score: {totalScore}. " +
                                      GetImprovementSuggestions(scorecard)
            });
        }
        else if (totalScore >= MinTotalScore && totalScore <= 32)
        {
            // Flag marginal scores for priority review
            result.Errors.Add(new ValidationError
            {
                Field = "TotalScore",
                Message = $"Total HQO score ({totalScore}/40) is marginal",
                Severity = "Minor",
                RemediationGuidance = $"Score meets minimum threshold but is close to the boundary. Consider strengthening: {GetImprovementSuggestions(scorecard)}"
            });
        }

        result.IsValid = !result.Errors.Any(e => e.Severity == "Critical");

        return Task.FromResult(result);
    }

    private void ValidateDimension(List<ValidationError> errors, int score, string dimensionName, string description)
    {
        if (score < MinDimensionScore)
        {
            errors.Add(new ValidationError
            {
                Field = $"Scorecard.{dimensionName}",
                Message = $"{dimensionName} score ({score}/5) is below minimum threshold ({MinDimensionScore}/5)",
                Severity = "Critical",
                RemediationGuidance = $"Improve {dimensionName}: {description}. Current score: {score}, minimum required: {MinDimensionScore}"
            });
        }

        if (score < 1 || score > MaxDimensionScore)
        {
            errors.Add(new ValidationError
            {
                Field = $"Scorecard.{dimensionName}",
                Message = $"{dimensionName} score ({score}) is out of valid range (1-{MaxDimensionScore})",
                Severity = "Critical",
                RemediationGuidance = $"Provide a valid score between 1 and {MaxDimensionScore} for {dimensionName}"
            });
        }
    }

    private string GetImprovementSuggestions(OrchestrationScorecard scorecard)
    {
        var weakDimensions = new List<string>();

        if (scorecard.Ownership < 4) weakDimensions.Add("Ownership");
        if (scorecard.TimeSLA < 4) weakDimensions.Add("TimeSLA");
        if (scorecard.Capacity < 4) weakDimensions.Add("Capacity");
        if (scorecard.Visibility < 4) weakDimensions.Add("Visibility");
        if (scorecard.CustomerLoop < 4) weakDimensions.Add("CustomerLoop");
        if (scorecard.Escalation < 4) weakDimensions.Add("Escalation");
        if (scorecard.Handoffs < 4) weakDimensions.Add("Handoffs");
        if (scorecard.Documentation < 4) weakDimensions.Add("Documentation");

        return weakDimensions.Any()
            ? string.Join(", ", weakDimensions)
            : "All dimensions are strong";
    }
}
