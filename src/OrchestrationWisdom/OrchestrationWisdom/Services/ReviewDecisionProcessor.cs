using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Processes review decisions and routes patterns accordingly
/// Movement 3, Beat 10: Review Decision & Feedback
/// </summary>
public interface IReviewDecisionProcessor
{
    Task ProcessDecisionAsync(ReviewDecision decision, PatternSubmission submission);
}

public class ReviewDecisionProcessor : IReviewDecisionProcessor
{
    /// <summary>
    /// Processes reviewer decision and updates submission status
    /// Event: pattern.review.decision.recorded
    /// </summary>
    public Task ProcessDecisionAsync(ReviewDecision decision, PatternSubmission submission)
    {
        switch (decision.Decision)
        {
            case ReviewStatus.Approved:
                ProcessApproval(decision, submission);
                break;

            case ReviewStatus.Rejected:
                ProcessRejection(decision, submission);
                break;

            case ReviewStatus.RequestChanges:
                ProcessChangeRequest(decision, submission);
                break;

            default:
                throw new ArgumentException($"Unknown review status: {decision.Decision}");
        }

        // Log decision for audit trail
        LogReviewDecision(decision, submission);

        return Task.CompletedTask;
    }

    private void ProcessApproval(ReviewDecision decision, PatternSubmission submission)
    {
        submission.Status = PublicationStatus.Approved;

        if (submission.Ticket != null)
        {
            submission.Ticket.Status = TicketStatus.Approved;
            submission.Ticket.Events.Add(new TicketEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = "review_approved",
                Description = "Pattern approved by reviewer and moved to approval queue for deployment",
                ActorId = decision.ReviewerId
            });
        }

        // Send approval notification to author
        SendApprovalNotification(submission, decision);
    }

    private void ProcessRejection(ReviewDecision decision, PatternSubmission submission)
    {
        submission.Status = PublicationStatus.ValidationFailed;

        if (submission.Ticket != null)
        {
            submission.Ticket.Status = TicketStatus.Failed;
            submission.Ticket.Events.Add(new TicketEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = "review_rejected",
                Description = "Pattern rejected by reviewer - critical issues found",
                ActorId = decision.ReviewerId
            });
        }

        // Send rejection notification with feedback to author
        SendRejectionNotification(submission, decision);
    }

    private void ProcessChangeRequest(ReviewDecision decision, PatternSubmission submission)
    {
        submission.Status = PublicationStatus.ChangesRequested;

        if (submission.Ticket != null)
        {
            submission.Ticket.Status = TicketStatus.AwaitingReview;
            submission.Ticket.Events.Add(new TicketEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = "changes_requested",
                Description = "Reviewer requested changes before approval",
                ActorId = decision.ReviewerId
            });
        }

        // Send change request notification to author
        SendChangeRequestNotification(submission, decision);
    }

    private void SendApprovalNotification(PatternSubmission submission, ReviewDecision decision)
    {
        var feedbackSummary = decision.Feedback.Any()
            ? "\n\nReviewer Comments:\n" + string.Join("\n", decision.Feedback.Select(f => $"- [{f.Section}] {f.Comment}"))
            : "";

        Console.WriteLine($"""
            [EMAIL] To: {submission.AuthorEmail}
            Subject: Pattern Approved - {submission.Pattern.Title}

            Dear Author,

            Great news! Your pattern "{submission.Pattern.Title}" has been approved for publication.

            Reviewer: {decision.ReviewerId}
            Review Date: {decision.ReviewedAt:yyyy-MM-dd}
            {feedbackSummary}

            Next Steps:
            1. Pattern will be added to CI/CD pipeline
            2. Integration tests will run automatically
            3. Upon successful build, pattern will be deployed to staging
            4. After staging verification, pattern will be published to production

            Estimated time to publication: 1-2 hours

            You will receive email notification when your pattern is live.

            Congratulations!
            Orchestration Wisdom Platform
            """);
    }

    private void SendRejectionNotification(PatternSubmission submission, ReviewDecision decision)
    {
        var criticalIssues = decision.Feedback
            .Where(f => f.Severity == FeedbackSeverity.Critical)
            .ToList();

        var issuesText = string.Join("\n", criticalIssues.Select(f =>
            $"- [{f.Section}] {f.Comment}"));

        Console.WriteLine($"""
            [EMAIL] To: {submission.AuthorEmail}
            Subject: Pattern Review - Issues Found - {submission.Pattern.Title}

            Dear Author,

            Thank you for submitting "{submission.Pattern.Title}". Our review has identified critical issues that prevent publication:

            Critical Issues:
            {issuesText}

            What to do next:
            1. Review the feedback above
            2. Address all critical issues
            3. Resubmit your pattern for another review

            If you have questions about the feedback, please reply to this email.

            Thank you,
            Orchestration Wisdom Platform
            """);
    }

    private void SendChangeRequestNotification(PatternSubmission submission, ReviewDecision decision)
    {
        var feedbackBySection = decision.Feedback
            .GroupBy(f => f.Section)
            .Select(g => $"\n{g.Key}:\n" + string.Join("\n", g.Select(f => $"  - [{f.Severity}] {f.Comment}")))
            .ToList();

        var feedbackText = string.Join("\n", feedbackBySection);

        Console.WriteLine($"""
            [EMAIL] To: {submission.AuthorEmail}
            Subject: Pattern Review - Changes Requested - {submission.Pattern.Title}

            Dear Author,

            Your pattern "{submission.Pattern.Title}" has been reviewed. Before we can approve it for publication, we need you to address the following feedback:
            {feedbackText}

            What to do next:
            1. Review the feedback above
            2. Update your pattern to address the comments
            3. Resubmit your pattern - it will be prioritized for re-review

            Your pattern is close to publication. Thank you for your contribution!

            Orchestration Wisdom Platform
            """);
    }

    private void LogReviewDecision(ReviewDecision decision, PatternSubmission submission)
    {
        Console.WriteLine($"""
            [AUDIT] Review Decision Recorded
            Submission: {submission.Id}
            Pattern: {submission.Pattern.Title}
            Reviewer: {decision.ReviewerId}
            Decision: {decision.Decision}
            Timestamp: {decision.ReviewedAt:u}
            Feedback Count: {decision.Feedback.Count}
            """);
    }
}
