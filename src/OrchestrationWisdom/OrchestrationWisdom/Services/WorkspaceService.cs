using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Manages creator workspace state and initialization
/// Movement 4: Workspace Navigation
/// </summary>
public interface IWorkspaceService
{
    Task<WorkspaceState> LoadWorkspaceAsync(string userId);
    Task<List<PatternDraft>> GetRecentDraftsAsync(string userId, int count = 10);
    Task<WorkspaceStats> GetWorkspaceStatsAsync(string userId);
}

public class WorkspaceService : IWorkspaceService
{
    private readonly Dictionary<string, List<PatternDraft>> _userDrafts = new();

    /// <summary>
    /// Loads complete workspace state for user
    /// Movement 4, Beat 9: Load Creator Workspace View
    /// Event: workspace.loaded
    /// </summary>
    public async Task<WorkspaceState> LoadWorkspaceAsync(string userId)
    {
        var recentDrafts = await GetRecentDraftsAsync(userId);
        var stats = await GetWorkspaceStatsAsync(userId);

        var workspaceState = new WorkspaceState
        {
            UserId = userId,
            LoadedAt = DateTime.UtcNow,
            RecentDrafts = recentDrafts,
            Preferences = new WorkspacePreferences(),
            ShowOnboarding = recentDrafts.Count == 0, // Show onboarding for new users
            Stats = stats
        };

        Console.WriteLine($"""
            [WORKSPACE] Workspace loaded for user: {userId}
            Recent Drafts: {recentDrafts.Count}
            Published Patterns: {stats.PublishedPatterns}
            Show Onboarding: {workspaceState.ShowOnboarding}
            """);

        return workspaceState;
    }

    /// <summary>
    /// Gets recent draft patterns for user
    /// Movement 4, Beat 10: Initialize Workspace State
    /// </summary>
    public Task<List<PatternDraft>> GetRecentDraftsAsync(string userId, int count = 10)
    {
        if (!_userDrafts.ContainsKey(userId))
        {
            // Create sample drafts for demo user
            if (userId == "user-demo-001")
            {
                _userDrafts[userId] = new List<PatternDraft>
                {
                    new PatternDraft
                    {
                        DraftId = "draft-001",
                        Title = "Emergency Escalation Workflow",
                        Status = "In Progress",
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        UpdatedAt = DateTime.UtcNow.AddHours(-2),
                        CompletionPercentage = 65
                    },
                    new PatternDraft
                    {
                        DraftId = "draft-002",
                        Title = "Incident Response Pattern",
                        Status = "Draft",
                        CreatedAt = DateTime.UtcNow.AddDays(-7),
                        UpdatedAt = DateTime.UtcNow.AddDays(-5),
                        CompletionPercentage = 30
                    }
                };
            }
            else
            {
                _userDrafts[userId] = new List<PatternDraft>();
            }
        }

        var drafts = _userDrafts[userId]
            .OrderByDescending(d => d.UpdatedAt)
            .Take(count)
            .ToList();

        return Task.FromResult(drafts);
    }

    public Task<WorkspaceStats> GetWorkspaceStatsAsync(string userId)
    {
        var drafts = _userDrafts.ContainsKey(userId) ? _userDrafts[userId] : new List<PatternDraft>();

        var stats = new WorkspaceStats
        {
            TotalDrafts = drafts.Count,
            PublishedPatterns = userId == "user-demo-001" ? 2 : 0,
            PendingReview = 0,
            LastPublished = userId == "user-demo-001" ? DateTime.UtcNow.AddDays(-10) : null
        };

        return Task.FromResult(stats);
    }
}
