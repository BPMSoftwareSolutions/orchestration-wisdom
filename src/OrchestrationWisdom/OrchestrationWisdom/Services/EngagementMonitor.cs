using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Monitors pattern engagement and detects anomalies
/// Movement 6, Beat 19: Monitor Initial Engagement
/// </summary>
public interface IEngagementMonitor
{
    Task MonitorAsync(string patternId);
    Task<bool> CheckEngagementHealthAsync(string patternId, int hoursSincePublication);
}

public class EngagementMonitor : IEngagementMonitor
{
    private readonly IEngagementTracker _engagementTracker;
    private readonly Dictionary<string, double> _categoryAverages = new();

    public EngagementMonitor(IEngagementTracker engagementTracker)
    {
        _engagementTracker = engagementTracker;
        InitializeCategoryAverages();
    }

    /// <summary>
    /// Monitors pattern engagement and alerts on anomalies
    /// Event: pattern.engagement.monitored
    /// </summary>
    public async Task MonitorAsync(string patternId)
    {
        var metrics = await _engagementTracker.GetMetricsAsync(patternId);

        Console.WriteLine($"""
            [MONITOR] Engagement check for pattern: {patternId}
            Timestamp: {DateTime.UtcNow:u}
            Views: {metrics.ViewCount}
            Unique Visitors: {metrics.UniqueVisitors}
            Downloads: {metrics.Downloads}
            Bounce Rate: {metrics.BounceRate:F2}%
            Average Time Spent: {metrics.AverageTimeSpent:F2}s
            Rating: {metrics.AverageRating:F2}/5 ({metrics.RatingCount} ratings)
            """);

        // Check for anomalies
        CheckForAnomalies(patternId, metrics);
    }

    public async Task<bool> CheckEngagementHealthAsync(string patternId, int hoursSincePublication)
    {
        var metrics = await _engagementTracker.GetMetricsAsync(patternId);

        // Alert if zero views after 24 hours
        if (hoursSincePublication >= 24 && metrics.ViewCount == 0)
        {
            SendZeroViewsAlert(patternId);
            return false;
        }

        // Check if views are significantly below category average
        var categoryAverage = GetCategoryAverage("Technology"); // Would be dynamic
        var viewsPerHour = (double)metrics.ViewCount / hoursSincePublication;

        if (viewsPerHour < categoryAverage * 0.3) // Less than 30% of average
        {
            SendLowEngagementAlert(patternId, viewsPerHour, categoryAverage);
            return false;
        }

        return true;
    }

    private void CheckForAnomalies(string patternId, EngagementMetrics metrics)
    {
        // Check for unusually high bounce rate
        if (metrics.BounceRate > 70.0)
        {
            Console.WriteLine($"""
                [ALERT] High bounce rate detected
                Pattern: {patternId}
                Bounce Rate: {metrics.BounceRate:F2}%
                Threshold: 70%
                Action: Pattern may need content improvements
                """);
        }

        // Check for unusually low time spent
        if (metrics.ViewCount > 10 && metrics.AverageTimeSpent < 30.0)
        {
            Console.WriteLine($"""
                [ALERT] Low average time spent detected
                Pattern: {patternId}
                Average Time: {metrics.AverageTimeSpent:F2}s
                Action: Users may not find content engaging
                """);
        }

        // Check for poor ratings
        if (metrics.RatingCount >= 5 && metrics.AverageRating < 3.0)
        {
            Console.WriteLine($"""
                [ALERT] Low average rating detected
                Pattern: {patternId}
                Average Rating: {metrics.AverageRating:F2}/5
                Rating Count: {metrics.RatingCount}
                Action: Review pattern quality and user feedback
                """);
        }
    }

    private void SendZeroViewsAlert(string patternId)
    {
        Console.WriteLine($"""
            [ALERT] Zero views after 24 hours
            Pattern: {patternId}
            Published: 24+ hours ago
            Views: 0
            Recommendation: Check if pattern is properly indexed and discoverable
            """);
    }

    private void SendLowEngagementAlert(string patternId, double viewsPerHour, double categoryAverage)
    {
        Console.WriteLine($"""
            [ALERT] Low engagement detected
            Pattern: {patternId}
            Views per hour: {viewsPerHour:F2}
            Category average: {categoryAverage:F2}
            Percentage of average: {(viewsPerHour / categoryAverage * 100):F2}%
            Recommendation: Review pattern title, tags, and discoverability
            """);
    }

    private void InitializeCategoryAverages()
    {
        // In production, calculate from historical data
        _categoryAverages["Technology"] = 5.0; // 5 views per hour
        _categoryAverages["Healthcare"] = 3.0;
        _categoryAverages["Finance"] = 4.0;
        _categoryAverages["Manufacturing"] = 2.0;
    }

    private double GetCategoryAverage(string category)
    {
        return _categoryAverages.ContainsKey(category)
            ? _categoryAverages[category]
            : 3.0; // Default
    }
}
