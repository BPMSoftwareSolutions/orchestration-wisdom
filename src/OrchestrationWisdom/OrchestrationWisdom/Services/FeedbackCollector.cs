using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Collects and processes user feedback on patterns
/// Movement 6, Beat 20: Collect User Feedback
/// </summary>
public interface IFeedbackCollector
{
    Task<UserFeedback> CollectAsync(string patternId, string userId, int? rating, string? comment, FeedbackType type);
    Task<List<UserFeedback>> GetFeedbackAsync(string patternId);
    Task<FeedbackSummary> GetFeedbackSummaryAsync(string patternId);
}

public class FeedbackCollector : IFeedbackCollector
{
    private readonly Dictionary<string, List<UserFeedback>> _feedback = new();
    private readonly IEngagementTracker _engagementTracker;

    public FeedbackCollector(IEngagementTracker engagementTracker)
    {
        _engagementTracker = engagementTracker;
    }

    /// <summary>
    /// Collects user feedback (ratings and comments)
    /// Event: pattern.feedback.collected
    /// </summary>
    public Task<UserFeedback> CollectAsync(string patternId, string userId, int? rating, string? comment, FeedbackType type)
    {
        ValidateFeedback(rating, comment, type);

        var feedback = new UserFeedback
        {
            PatternId = patternId,
            UserId = userId,
            SubmittedAt = DateTime.UtcNow,
            Rating = rating,
            Comment = comment,
            Type = type
        };

        if (!_feedback.ContainsKey(patternId))
        {
            _feedback[patternId] = new List<UserFeedback>();
        }

        _feedback[patternId].Add(feedback);

        // Update engagement metrics if rating provided
        if (rating.HasValue)
        {
            UpdateEngagementMetrics(patternId);
        }

        Console.WriteLine($"""
            [FEEDBACK] Collected for pattern: {patternId}
            User: {userId}
            Type: {type}
            Rating: {rating?.ToString() ?? "N/A"}
            Comment: {(string.IsNullOrEmpty(comment) ? "N/A" : "Provided")}
            Timestamp: {DateTime.UtcNow:u}
            """);

        return Task.FromResult(feedback);
    }

    public Task<List<UserFeedback>> GetFeedbackAsync(string patternId)
    {
        var feedback = _feedback.ContainsKey(patternId)
            ? _feedback[patternId]
            : new List<UserFeedback>();

        return Task.FromResult(feedback);
    }

    public async Task<FeedbackSummary> GetFeedbackSummaryAsync(string patternId)
    {
        var feedback = await GetFeedbackAsync(patternId);

        var ratings = feedback.Where(f => f.Rating.HasValue).Select(f => f.Rating!.Value).ToList();

        var summary = new FeedbackSummary
        {
            PatternId = patternId,
            TotalFeedbackCount = feedback.Count,
            RatingCount = ratings.Count,
            AverageRating = ratings.Any() ? ratings.Average() : 0.0,
            CommentCount = feedback.Count(f => !string.IsNullOrEmpty(f.Comment)),
            IssueCount = feedback.Count(f => f.Type == FeedbackType.Issue),
            RatingDistribution = CalculateRatingDistribution(ratings),
            RecentComments = feedback
                .Where(f => !string.IsNullOrEmpty(f.Comment))
                .OrderByDescending(f => f.SubmittedAt)
                .Take(5)
                .Select(f => f.Comment!)
                .ToList()
        };

        return summary;
    }

    private void ValidateFeedback(int? rating, string? comment, FeedbackType type)
    {
        if (type == FeedbackType.Rating && !rating.HasValue)
        {
            throw new ArgumentException("Rating is required for Rating feedback type");
        }

        if (rating.HasValue && (rating.Value < 1 || rating.Value > 5))
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        if (type == FeedbackType.Comment && string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("Comment is required for Comment feedback type");
        }

        if (type == FeedbackType.Issue && string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException("Issue description is required for Issue feedback type");
        }
    }

    private void UpdateEngagementMetrics(string patternId)
    {
        var feedback = _feedback[patternId];
        var ratings = feedback.Where(f => f.Rating.HasValue).Select(f => f.Rating!.Value).ToList();

        if (ratings.Any())
        {
            var metrics = _engagementTracker.GetMetricsAsync(patternId).Result;
            metrics.RatingCount = ratings.Count;
            metrics.AverageRating = ratings.Average();
        }
    }

    private Dictionary<int, int> CalculateRatingDistribution(List<int> ratings)
    {
        var distribution = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
        };

        foreach (var rating in ratings)
        {
            distribution[rating]++;
        }

        return distribution;
    }
}

public class FeedbackSummary
{
    public string PatternId { get; set; } = string.Empty;
    public int TotalFeedbackCount { get; set; }
    public int RatingCount { get; set; }
    public double AverageRating { get; set; }
    public int CommentCount { get; set; }
    public int IssueCount { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public List<string> RecentComments { get; set; } = new();
}
