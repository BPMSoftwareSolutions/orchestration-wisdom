using Xunit;
using OrchestrationWisdom.Models;
using OrchestrationWisdom.Services;

namespace OrchestrationWisdom.Tests.Services;

/// <summary>
/// Tests for ReviewDecisionProcessor
/// Movement 3, Beat 10: Review Decision & Feedback
/// </summary>
public class ReviewDecisionProcessorTests
{
    [Fact]
    public async Task ProcessDecision_RoutesToApprovalQueue_WhenApprovedDecision()
    {
        // Arrange
        var processor = new ReviewDecisionProcessor();
        var submission = CreateSubmissionWithTicket();
        var decision = new ReviewDecision
        {
            SubmissionId = submission.Id,
            ReviewerId = "reviewer-1",
            ReviewedAt = DateTime.UtcNow,
            Decision = ReviewStatus.Approved,
            Feedback = new List<ReviewFeedback>()
        };

        // Act
        await processor.ProcessDecisionAsync(decision, submission);

        // Assert
        Assert.Equal(PublicationStatus.Approved, submission.Status);
        Assert.Equal(TicketStatus.Approved, submission.Ticket!.Status);
        Assert.Contains(submission.Ticket.Events, e => e.EventType == "review_approved");
    }

    [Fact]
    public async Task ProcessDecision_ReturnsToAuthor_WhenRejected()
    {
        // Arrange
        var processor = new ReviewDecisionProcessor();
        var submission = CreateSubmissionWithTicket();
        var decision = new ReviewDecision
        {
            SubmissionId = submission.Id,
            ReviewerId = "reviewer-1",
            ReviewedAt = DateTime.UtcNow,
            Decision = ReviewStatus.Rejected,
            Feedback = new List<ReviewFeedback>
            {
                new ReviewFeedback
                {
                    Section = "Problem",
                    Severity = FeedbackSeverity.Critical,
                    Comment = "Problem statement is unclear"
                }
            }
        };

        // Act
        await processor.ProcessDecisionAsync(decision, submission);

        // Assert
        Assert.Equal(PublicationStatus.ValidationFailed, submission.Status);
        Assert.Equal(TicketStatus.Failed, submission.Ticket!.Status);
        Assert.Contains(submission.Ticket.Events, e => e.EventType == "review_rejected");
    }

    [Fact]
    public async Task ProcessDecision_QueuesForReReview_WhenChangesRequested()
    {
        // Arrange
        var processor = new ReviewDecisionProcessor();
        var submission = CreateSubmissionWithTicket();
        var decision = new ReviewDecision
        {
            SubmissionId = submission.Id,
            ReviewerId = "reviewer-1",
            ReviewedAt = DateTime.UtcNow,
            Decision = ReviewStatus.RequestChanges,
            Feedback = new List<ReviewFeedback>
            {
                new ReviewFeedback
                {
                    Section = "Metrics",
                    Severity = FeedbackSeverity.Major,
                    Comment = "Add more specific metrics"
                }
            }
        };

        // Act
        await processor.ProcessDecisionAsync(decision, submission);

        // Assert
        Assert.Equal(PublicationStatus.ChangesRequested, submission.Status);
        Assert.Equal(TicketStatus.AwaitingReview, submission.Ticket!.Status);
        Assert.Contains(submission.Ticket.Events, e => e.EventType == "changes_requested");
    }

    [Fact]
    public async Task ProcessDecision_RecordsReviewerInEvents()
    {
        // Arrange
        var processor = new ReviewDecisionProcessor();
        var submission = CreateSubmissionWithTicket();
        var reviewerId = "reviewer-123";
        var decision = new ReviewDecision
        {
            SubmissionId = submission.Id,
            ReviewerId = reviewerId,
            ReviewedAt = DateTime.UtcNow,
            Decision = ReviewStatus.Approved,
            Feedback = new List<ReviewFeedback>()
        };

        // Act
        await processor.ProcessDecisionAsync(decision, submission);

        // Assert
        Assert.Contains(submission.Ticket!.Events, e => e.ActorId == reviewerId);
    }

    private PatternSubmission CreateSubmissionWithTicket()
    {
        var pattern = new Pattern
        {
            Id = "pattern-1",
            Title = "Test Pattern",
            Hook = "Test hook",
            ProblemDetail = "Test problem",
            AsIsDiagram = "sequenceDiagram\nparticipant A as Actor",
            OrchestratedDiagram = "sequenceDiagram\nparticipant A as Actor",
            DecisionPoint = "Test decision",
            Metrics = "Test metrics",
            Checklist = "Test checklist",
            ClosingInsight = "Test insight",
            Scorecard = new OrchestrationScorecard
            {
                Ownership = 4,
                TimeSLA = 4,
                Capacity = 4,
                Visibility = 4,
                CustomerLoop = 3,
                Escalation = 4,
                Handoffs = 4,
                Documentation = 3
            },
            Industries = new List<string> { "Technology" },
            BrokenSignals = new List<string> { "Ownership" }
        };

        return new PatternSubmission
        {
            Id = "SUB-001",
            PatternId = pattern.Id,
            AuthorId = "author-1",
            AuthorEmail = "author@example.com",
            SubmittedAt = DateTime.UtcNow,
            Status = PublicationStatus.InReview,
            Pattern = pattern,
            Ticket = new PublicationTicket
            {
                TicketId = "PUB-2026-001",
                SubmissionId = "SUB-001",
                CreatedAt = DateTime.UtcNow,
                Status = TicketStatus.InReview,
                Priority = 1,
                Events = new List<TicketEvent>()
            }
        };
    }
}
