using Xunit;
using OrchestrationWisdom.Services;
using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Tests.Services;

public class WorkspaceServiceTests
{
    private readonly WorkspaceService _workspaceService;

    public WorkspaceServiceTests()
    {
        _workspaceService = new WorkspaceService();
    }

    [Fact]
    public async Task LoadWorkspaceAsync_ReturnsWorkspaceState()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        var workspace = await _workspaceService.LoadWorkspaceAsync(userId);

        // Assert
        Assert.NotNull(workspace);
        Assert.Equal(userId, workspace.UserId);
        Assert.True(workspace.LoadedAt <= DateTime.UtcNow);
        Assert.NotNull(workspace.RecentDrafts);
        Assert.NotNull(workspace.Preferences);
        Assert.NotNull(workspace.Stats);
    }

    [Fact]
    public async Task LoadWorkspaceAsync_ForNewUser_ShowsOnboarding()
    {
        // Arrange
        var userId = "new-user-123";

        // Act
        var workspace = await _workspaceService.LoadWorkspaceAsync(userId);

        // Assert
        Assert.True(workspace.ShowOnboarding);
        Assert.Empty(workspace.RecentDrafts);
    }

    [Fact]
    public async Task LoadWorkspaceAsync_ForDemoUser_HasDrafts()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act
        var workspace = await _workspaceService.LoadWorkspaceAsync(userId);

        // Assert
        Assert.False(workspace.ShowOnboarding);
        Assert.NotEmpty(workspace.RecentDrafts);
        Assert.True(workspace.RecentDrafts.Count > 0);
    }

    [Fact]
    public async Task GetRecentDraftsAsync_ForNewUser_ReturnsEmptyList()
    {
        // Arrange
        var userId = "new-user-123";

        // Act
        var drafts = await _workspaceService.GetRecentDraftsAsync(userId);

        // Assert
        Assert.NotNull(drafts);
        Assert.Empty(drafts);
    }

    [Fact]
    public async Task GetRecentDraftsAsync_ForDemoUser_ReturnsDrafts()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act
        var drafts = await _workspaceService.GetRecentDraftsAsync(userId);

        // Assert
        Assert.NotNull(drafts);
        Assert.NotEmpty(drafts);
        Assert.All(drafts, draft =>
        {
            Assert.NotEmpty(draft.DraftId);
            Assert.NotEmpty(draft.Title);
            Assert.NotEmpty(draft.Status);
            Assert.True(draft.CompletionPercentage >= 0 && draft.CompletionPercentage <= 100);
        });
    }

    [Fact]
    public async Task GetRecentDraftsAsync_ReturnsDraftsInDescendingOrder()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act
        var drafts = await _workspaceService.GetRecentDraftsAsync(userId);

        // Assert
        Assert.NotEmpty(drafts);
        for (int i = 0; i < drafts.Count - 1; i++)
        {
            Assert.True(drafts[i].UpdatedAt >= drafts[i + 1].UpdatedAt,
                "Drafts should be ordered by UpdatedAt descending");
        }
    }

    [Fact]
    public async Task GetRecentDraftsAsync_RespectsCountLimit()
    {
        // Arrange
        var userId = "user-demo-001";
        var limit = 1;

        // Act
        var drafts = await _workspaceService.GetRecentDraftsAsync(userId, limit);

        // Assert
        Assert.True(drafts.Count <= limit);
    }

    [Fact]
    public async Task GetWorkspaceStatsAsync_ForNewUser_ReturnsZeroStats()
    {
        // Arrange
        var userId = "new-user-123";

        // Act
        var stats = await _workspaceService.GetWorkspaceStatsAsync(userId);

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalDrafts);
        Assert.Equal(0, stats.PublishedPatterns);
        Assert.Equal(0, stats.PendingReview);
        Assert.Null(stats.LastPublished);
    }

    [Fact]
    public async Task GetWorkspaceStatsAsync_ForDemoUser_ReturnsStats()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act - must call GetRecentDraftsAsync first to initialize drafts
        await _workspaceService.GetRecentDraftsAsync(userId);
        var stats = await _workspaceService.GetWorkspaceStatsAsync(userId);

        // Assert
        Assert.NotNull(stats);
        Assert.True(stats.TotalDrafts > 0);
        Assert.True(stats.PublishedPatterns > 0);
        Assert.NotNull(stats.LastPublished);
        Assert.True(stats.LastPublished < DateTime.UtcNow);
    }

    [Fact]
    public async Task LoadWorkspaceAsync_IncludesWorkspaceStats()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act
        var workspace = await _workspaceService.LoadWorkspaceAsync(userId);

        // Assert
        Assert.NotNull(workspace.Stats);
        Assert.Equal(workspace.RecentDrafts.Count, workspace.Stats.TotalDrafts);
    }

    [Fact]
    public async Task LoadWorkspaceAsync_SetsDefaultPreferences()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        var workspace = await _workspaceService.LoadWorkspaceAsync(userId);

        // Assert
        Assert.NotNull(workspace.Preferences);
        Assert.Equal("light", workspace.Preferences.Theme);
        Assert.Equal("default", workspace.Preferences.Layout);
        Assert.True(workspace.Preferences.NotificationsEnabled);
        Assert.True(workspace.Preferences.OnboardingTooltips);
    }

    [Fact]
    public async Task GetRecentDraftsAsync_DraftHasRequiredProperties()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act
        var drafts = await _workspaceService.GetRecentDraftsAsync(userId);

        // Assert
        Assert.NotEmpty(drafts);
        var draft = drafts.First();
        Assert.NotEmpty(draft.DraftId);
        Assert.NotEmpty(draft.Title);
        Assert.NotEmpty(draft.Status);
        Assert.True(draft.CreatedAt <= draft.UpdatedAt);
        Assert.InRange(draft.CompletionPercentage, 0, 100);
    }

    [Fact]
    public async Task LoadWorkspaceAsync_MultipleCallsReturnConsistentData()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act
        var workspace1 = await _workspaceService.LoadWorkspaceAsync(userId);
        var workspace2 = await _workspaceService.LoadWorkspaceAsync(userId);

        // Assert
        Assert.Equal(workspace1.RecentDrafts.Count, workspace2.RecentDrafts.Count);
        Assert.Equal(workspace1.ShowOnboarding, workspace2.ShowOnboarding);
    }

    [Fact]
    public async Task GetRecentDraftsAsync_WithDefaultCount_ReturnsUpTo10Drafts()
    {
        // Arrange
        var userId = "user-demo-001";

        // Act
        var drafts = await _workspaceService.GetRecentDraftsAsync(userId);

        // Assert
        Assert.True(drafts.Count <= 10);
    }
}
