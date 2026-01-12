using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Generates comprehensive validation reports
/// Movement 2, Beat 7: Generate Validation Report
/// </summary>
public interface IValidationReportGenerator
{
    Task<ValidationReport> GenerateAsync(PatternSubmission submission, ValidationResult schema, ValidationResult hqo, ValidationResult diagram);
    string GenerateMarkdownReport(ValidationReport report);
}

public class ValidationReportGenerator : IValidationReportGenerator
{
    /// <summary>
    /// Generates validation report combining all validation checks
    /// Event: pattern.validation.report.generated
    /// </summary>
    public Task<ValidationReport> GenerateAsync(PatternSubmission submission, ValidationResult schema, ValidationResult hqo, ValidationResult diagram)
    {
        var allValid = schema.IsValid && hqo.IsValid && diagram.IsValid;

        var report = new ValidationReport
        {
            SubmissionId = submission.Id,
            GeneratedAt = DateTime.UtcNow,
            OverallValid = allValid,
            SchemaValidation = schema,
            HQOValidation = hqo,
            DiagramValidation = diagram,
            Status = DetermineStatus(allValid, hqo),
            NextSteps = GenerateNextSteps(allValid, schema, hqo, diagram)
        };

        return Task.FromResult(report);
    }

    /// <summary>
    /// Generates markdown-formatted report for author notifications
    /// </summary>
    public string GenerateMarkdownReport(ValidationReport report)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"# Validation Report - {report.SubmissionId}");
        sb.AppendLine($"**Generated:** {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine();
        sb.AppendLine($"**Overall Status:** {(report.OverallValid ? "✓ PASSED" : "✗ FAILED")}");
        sb.AppendLine();

        // Schema Validation Section
        sb.AppendLine("## Schema Validation");
        AppendValidationSection(sb, report.SchemaValidation);

        // HQO Scorecard Section
        sb.AppendLine("## HQO Scorecard Validation");
        AppendValidationSection(sb, report.HQOValidation);

        // Diagram Validation Section
        sb.AppendLine("## Diagram Budget Validation");
        AppendValidationSection(sb, report.DiagramValidation);

        // Next Steps
        if (report.NextSteps.Any())
        {
            sb.AppendLine("## Next Steps");
            foreach (var step in report.NextSteps)
            {
                sb.AppendLine($"- {step}");
            }
        }

        return sb.ToString();
    }

    private void AppendValidationSection(System.Text.StringBuilder sb, ValidationResult validation)
    {
        sb.AppendLine($"**Status:** {(validation.IsValid ? "✓ Passed" : "✗ Failed")}");
        sb.AppendLine();

        if (validation.Errors.Any())
        {
            sb.AppendLine("### Issues Found");
            sb.AppendLine();

            var criticalErrors = validation.Errors.Where(e => e.Severity == "Critical").ToList();
            var majorErrors = validation.Errors.Where(e => e.Severity == "Major").ToList();
            var minorErrors = validation.Errors.Where(e => e.Severity == "Minor").ToList();

            if (criticalErrors.Any())
            {
                sb.AppendLine("#### Critical Issues");
                foreach (var error in criticalErrors)
                {
                    sb.AppendLine($"- **{error.Field}**: {error.Message}");
                    if (!string.IsNullOrEmpty(error.RemediationGuidance))
                    {
                        sb.AppendLine($"  - *Remediation:* {error.RemediationGuidance}");
                    }
                }
                sb.AppendLine();
            }

            if (majorErrors.Any())
            {
                sb.AppendLine("#### Major Issues");
                foreach (var error in majorErrors)
                {
                    sb.AppendLine($"- **{error.Field}**: {error.Message}");
                    if (!string.IsNullOrEmpty(error.RemediationGuidance))
                    {
                        sb.AppendLine($"  - *Remediation:* {error.RemediationGuidance}");
                    }
                }
                sb.AppendLine();
            }

            if (minorErrors.Any())
            {
                sb.AppendLine("#### Minor Issues / Warnings");
                foreach (var error in minorErrors)
                {
                    sb.AppendLine($"- **{error.Field}**: {error.Message}");
                    if (!string.IsNullOrEmpty(error.RemediationGuidance))
                    {
                        sb.AppendLine($"  - *Remediation:* {error.RemediationGuidance}");
                    }
                }
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("*No issues found*");
            sb.AppendLine();
        }
    }

    private string DetermineStatus(bool allValid, ValidationResult hqo)
    {
        if (allValid)
        {
            // Check if HQO score is marginal (30-32)
            var hasMarginalWarning = hqo.Errors.Any(e => e.Severity == "Minor" && e.Field == "TotalScore");
            return hasMarginalWarning ? "ready_for_review_priority" : "ready_for_review";
        }

        return "validation_failed";
    }

    private List<string> GenerateNextSteps(bool allValid, ValidationResult schema, ValidationResult hqo, ValidationResult diagram)
    {
        var steps = new List<string>();

        if (allValid)
        {
            steps.Add("Pattern has passed all validation checks");
            steps.Add("Pattern will be routed to reviewer queue");
            steps.Add("Estimated review time: 3-5 business days");
            steps.Add("You will receive email notification when review begins");
        }
        else
        {
            steps.Add("Pattern has validation failures that must be addressed");

            if (!schema.IsValid)
            {
                steps.Add("Fix schema validation errors (missing required fields or incorrect data types)");
            }

            if (!hqo.IsValid)
            {
                steps.Add("Improve HQO scorecard dimensions to meet publication thresholds");
            }

            if (!diagram.IsValid)
            {
                steps.Add("Adjust diagrams to meet budget constraints (actors, steps, alt blocks)");
            }

            steps.Add("Resubmit pattern after addressing all issues");
        }

        return steps;
    }
}
