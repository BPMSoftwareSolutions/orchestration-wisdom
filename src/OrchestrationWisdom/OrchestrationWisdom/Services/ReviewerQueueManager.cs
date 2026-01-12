using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Manages reviewer queue and pattern routing
/// Movement 3, Beat 8: Route to Reviewer Queue
/// </summary>
public interface IReviewerQueueManager
{
    Task<string> RoutePatternAsync(PatternSubmission submission);
    Task<List<PatternSubmission>> GetReviewerQueueAsync(string reviewerId);
    Task<Dictionary<string, int>> GetQueueLoadAsync();
}

public class ReviewerQueueManager : IReviewerQueueManager
{
    private readonly Dictionary<string, Reviewer> _reviewers = new();
    private readonly Dictionary<string, List<string>> _reviewerQueues = new();

    public ReviewerQueueManager()
    {
        InitializeReviewers();
    }

    /// <summary>
    /// Routes pattern to appropriate reviewer based on expertise and workload
    /// Event: pattern.routed.to.reviewer
    /// </summary>
    public Task<string> RoutePatternAsync(PatternSubmission submission)
    {
        // Find reviewers with matching expertise
        var matchingReviewers = FindMatchingReviewers(submission.Pattern.Industries);

        if (!matchingReviewers.Any())
        {
            // No specialist match, use general reviewers
            matchingReviewers = _reviewers.Values.Where(r => r.IsGeneralReviewer).ToList();
        }

        // Select reviewer with lightest queue load
        var selectedReviewer = matchingReviewers
            .OrderBy(r => GetReviewerQueueSize(r.Id))
            .First();

        // Add to reviewer's queue
        if (!_reviewerQueues.ContainsKey(selectedReviewer.Id))
        {
            _reviewerQueues[selectedReviewer.Id] = new List<string>();
        }

        _reviewerQueues[selectedReviewer.Id].Add(submission.Id);

        // Update submission
        submission.Status = PublicationStatus.AwaitingReview;
        if (submission.Ticket != null)
        {
            submission.Ticket.AssignedReviewerId = selectedReviewer.Id;
        }

        // Send notification to reviewer
        SendReviewerNotification(selectedReviewer, submission);

        return Task.FromResult(selectedReviewer.Id);
    }

    public Task<List<PatternSubmission>> GetReviewerQueueAsync(string reviewerId)
    {
        // In production, fetch from database
        var queueIds = _reviewerQueues.ContainsKey(reviewerId)
            ? _reviewerQueues[reviewerId]
            : new List<string>();

        // Would map to actual submissions
        return Task.FromResult(new List<PatternSubmission>());
    }

    public Task<Dictionary<string, int>> GetQueueLoadAsync()
    {
        var loads = _reviewerQueues.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count
        );

        return Task.FromResult(loads);
    }

    private List<Reviewer> FindMatchingReviewers(List<string> industries)
    {
        return _reviewers.Values
            .Where(r => r.Expertise.Any(e => industries.Contains(e)))
            .ToList();
    }

    private int GetReviewerQueueSize(string reviewerId)
    {
        return _reviewerQueues.ContainsKey(reviewerId)
            ? _reviewerQueues[reviewerId].Count
            : 0;
    }

    private void SendReviewerNotification(Reviewer reviewer, PatternSubmission submission)
    {
        var slaDeadline = DateTime.UtcNow.AddDays(3);

        Console.WriteLine($"""
            [EMAIL] To: {reviewer.Email}
            Subject: New Pattern for Review - {submission.Pattern.Title}

            Dear {reviewer.Name},

            A new pattern has been assigned to you for review:

            Pattern: {submission.Pattern.Title}
            Submission ID: {submission.Id}
            Author: {submission.AuthorEmail}
            Industries: {string.Join(", ", submission.Pattern.Industries)}

            SLA Deadline: {slaDeadline:yyyy-MM-dd}

            Please review the pattern in the Content Review Dashboard.

            Thank you,
            Orchestration Wisdom Platform
            """);
    }

    private void InitializeReviewers()
    {
        _reviewers["REV001"] = new Reviewer
        {
            Id = "REV001",
            Name = "Sarah Chen",
            Email = "sarah.chen@example.com",
            Expertise = new List<string> { "Technology", "Healthcare" },
            IsGeneralReviewer = false
        };

        _reviewers["REV002"] = new Reviewer
        {
            Id = "REV002",
            Name = "Michael Torres",
            Email = "michael.torres@example.com",
            Expertise = new List<string> { "Finance", "Professional Services" },
            IsGeneralReviewer = false
        };

        _reviewers["REV003"] = new Reviewer
        {
            Id = "REV003",
            Name = "Emily Johnson",
            Email = "emily.johnson@example.com",
            Expertise = new List<string> { "Manufacturing", "Retail", "E-commerce" },
            IsGeneralReviewer = false
        };

        _reviewers["REV999"] = new Reviewer
        {
            Id = "REV999",
            Name = "General Review Team",
            Email = "review-team@example.com",
            Expertise = new List<string>(),
            IsGeneralReviewer = true
        };
    }
}

public class Reviewer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Expertise { get; set; } = new();
    public bool IsGeneralReviewer { get; set; }
}
