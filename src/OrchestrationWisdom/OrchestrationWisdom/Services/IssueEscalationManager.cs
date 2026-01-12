using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Manages escalation of critical issues with published patterns
/// Movement 6, Beat 21: Escalate Critical Issues
/// </summary>
public interface IIssueEscalationManager
{
    Task EscalateAsync(string patternId, string issueDescription, string reportedBy);
    Task<List<EscalatedIssue>> GetEscalatedIssuesAsync(string patternId);
}

public class IssueEscalationManager : IIssueEscalationManager
{
    private readonly Dictionary<string, List<EscalatedIssue>> _escalatedIssues = new();
    private readonly Dictionary<string, IssueTracker> _issueTrackers = new();

    /// <summary>
    /// Escalates critical issues when multiple reports or high severity detected
    /// Event: pattern.issue.escalated
    /// </summary>
    public Task EscalateAsync(string patternId, string issueDescription, string reportedBy)
    {
        // Track the issue
        if (!_issueTrackers.ContainsKey(patternId))
        {
            _issueTrackers[patternId] = new IssueTracker();
        }

        var tracker = _issueTrackers[patternId];
        tracker.IssueReports.Add(new IssueReport
        {
            ReportedAt = DateTime.UtcNow,
            ReportedBy = reportedBy,
            Description = issueDescription
        });

        // Check if escalation is needed
        var shouldEscalate = ShouldEscalate(tracker, issueDescription);

        if (shouldEscalate)
        {
            var issue = CreateEscalatedIssue(patternId, issueDescription, tracker.IssueReports.Count);

            if (!_escalatedIssues.ContainsKey(patternId))
            {
                _escalatedIssues[patternId] = new List<EscalatedIssue>();
            }

            _escalatedIssues[patternId].Add(issue);

            // Notify support team and author
            NotifyEscalation(issue);

            Console.WriteLine($"""
                [ESCALATION] Critical issue escalated
                Pattern: {patternId}
                Issue ID: {issue.IssueId}
                Report Count: {tracker.IssueReports.Count}
                Severity: {issue.Severity}
                """);
        }

        return Task.CompletedTask;
    }

    public Task<List<EscalatedIssue>> GetEscalatedIssuesAsync(string patternId)
    {
        var issues = _escalatedIssues.ContainsKey(patternId)
            ? _escalatedIssues[patternId]
            : new List<EscalatedIssue>();

        return Task.FromResult(issues);
    }

    private bool ShouldEscalate(IssueTracker tracker, string issueDescription)
    {
        // Escalate if 3+ reports within 1 hour
        var recentReports = tracker.IssueReports
            .Where(r => r.ReportedAt > DateTime.UtcNow.AddHours(-1))
            .ToList();

        if (recentReports.Count >= 3)
        {
            Console.WriteLine($"[ESCALATION] Threshold met: {recentReports.Count} reports in last hour");
            return true;
        }

        // Escalate if issue contains critical keywords
        var criticalKeywords = new[] { "data error", "broken", "incorrect", "harmful", "misleading" };
        var isCritical = criticalKeywords.Any(keyword =>
            issueDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (isCritical)
        {
            Console.WriteLine($"[ESCALATION] Critical keyword detected in issue description");
            return true;
        }

        // Escalate if error rate spike (would check actual metrics in production)
        if (tracker.ErrorRateSpike)
        {
            Console.WriteLine($"[ESCALATION] Error rate spike detected (>100 errors/hour)");
            return true;
        }

        return false;
    }

    private EscalatedIssue CreateEscalatedIssue(string patternId, string description, int reportCount)
    {
        var severity = DetermineSeverity(description, reportCount);

        return new EscalatedIssue
        {
            IssueId = GenerateIssueId(),
            PatternId = patternId,
            Description = description,
            Severity = severity,
            ReportCount = reportCount,
            EscalatedAt = DateTime.UtcNow,
            Status = "open",
            RequiresRollback = severity == "critical"
        };
    }

    private string DetermineSeverity(string description, int reportCount)
    {
        var criticalKeywords = new[] { "data error", "harmful", "misleading", "security" };
        var hasCriticalKeyword = criticalKeywords.Any(keyword =>
            description.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (hasCriticalKeyword || reportCount >= 5)
        {
            return "critical";
        }

        if (reportCount >= 3)
        {
            return "high";
        }

        return "medium";
    }

    private void NotifyEscalation(EscalatedIssue issue)
    {
        Console.WriteLine($"""
            [EMAIL] URGENT - Critical Issue Escalated
            To: support-team@example.com, pattern-author@example.com

            Issue ID: {issue.IssueId}
            Pattern: {issue.PatternId}
            Severity: {issue.Severity}
            Report Count: {issue.ReportCount}
            Escalated At: {issue.EscalatedAt:u}

            Description:
            {issue.Description}

            {(issue.RequiresRollback ? "⚠️ IMMEDIATE ACTION REQUIRED: This issue may require pattern rollback" : "")}

            Action Required:
            1. Review issue details immediately
            2. Assess impact on users
            3. Determine if rollback is necessary
            4. Update issue status in dashboard

            Issue Dashboard: /admin/issues/{issue.IssueId}
            """);
    }

    private string GenerateIssueId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(100, 999);
        return $"ISSUE-{timestamp}-{random}";
    }
}

public class IssueTracker
{
    public List<IssueReport> IssueReports { get; set; } = new();
    public bool ErrorRateSpike { get; set; }
}

public class IssueReport
{
    public DateTime ReportedAt { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class EscalatedIssue
{
    public string IssueId { get; set; } = string.Empty;
    public string PatternId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public DateTime EscalatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool RequiresRollback { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
