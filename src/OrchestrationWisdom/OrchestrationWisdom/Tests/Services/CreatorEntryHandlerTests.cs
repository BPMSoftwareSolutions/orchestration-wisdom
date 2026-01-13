using Xunit;
using Moq;
using OrchestrationWisdom.Services;
using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Tests.Services;

public class CreatorEntryHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IWorkspaceService> _mockWorkspaceService;
    private readonly Mock<IAnalyticsService> _mockAnalyticsService;
    private readonly Mock<IAuditLogger> _mockAuditLogger;
    private readonly CreatorEntryHandler _entryHandler;

    public CreatorEntryHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockWorkspaceService = new Mock<IWorkspaceService>();
        _mockAnalyticsService = new Mock<IAnalyticsService>();
        _mockAuditLogger = new Mock<IAuditLogger>();

        _entryHandler = new CreatorEntryHandler(
            _mockAuthService.Object,
            _mockWorkspaceService.Object,
            _mockAnalyticsService.Object,
            _mockAuditLogger.Object
        );
    }

    [Fact]
    public async Task HandleEntryAsync_WithoutAuthentication_ReturnsSignInRedirect()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA);

        // Assert
        Assert.False(result.Success);
        Assert.True(result.RequiresAuthentication);
        Assert.Equal("/signin", result.RedirectTo);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task HandleEntryAsync_WithAuthentication_ReturnsWorkspaceRedirect()
    {
        // Arrange
        var sessionToken = "valid-session-token";
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(sessionToken))
            .ReturnsAsync(true);

        _mockAnalyticsService
            .Setup(x => x.TrackEventAsync(It.IsAny<AnalyticsEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA, sessionToken);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.RequiresAuthentication);
        Assert.Equal("/workspace", result.RedirectTo);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task HandleEntryAsync_WithEmptySessionToken_ReturnsSignInRedirect()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(string.Empty))
            .ReturnsAsync(false);

        // Act
        var result = await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA, string.Empty);

        // Assert
        Assert.False(result.Success);
        Assert.True(result.RequiresAuthentication);
        Assert.Equal("/signin", result.RedirectTo);
    }

    [Fact]
    public async Task HandleEntryAsync_WithNullSessionToken_ReturnsSignInRedirect()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(string.Empty))
            .ReturnsAsync(false);

        // Act
        var result = await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA, null);

        // Assert
        Assert.False(result.Success);
        Assert.True(result.RequiresAuthentication);
        Assert.Equal("/signin", result.RedirectTo);
    }

    [Fact]
    public async Task HandleEntryAsync_TracksAnalyticsOnSuccess()
    {
        // Arrange
        var sessionToken = "valid-session-token";
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(sessionToken))
            .ReturnsAsync(true);

        _mockAnalyticsService
            .Setup(x => x.TrackEventAsync(It.IsAny<AnalyticsEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA, sessionToken);

        // Assert
        _mockAnalyticsService.Verify(
            x => x.TrackEventAsync(It.Is<AnalyticsEvent>(e =>
                e.EventType == "creator.entry.completed" &&
                e.Properties.ContainsKey("source") &&
                e.Properties.ContainsKey("time_to_workspace_seconds")
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleEntryAsync_DoesNotTrackAnalyticsOnFailure()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA);

        // Assert
        _mockAnalyticsService.Verify(
            x => x.TrackEventAsync(It.IsAny<AnalyticsEvent>()),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleEntryAsync_SetsCorrectEntrySource()
    {
        // Arrange
        var sources = new[]
        {
            EntrySource.HomePageCTA,
            EntrySource.NavigationLink,
            EntrySource.DirectURL,
            EntrySource.EmailLink
        };

        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act & Assert
        foreach (var source in sources)
        {
            var result = await _entryHandler.HandleEntryAsync(source);
            Assert.Equal(source, result.Source);
        }
    }

    [Fact]
    public async Task HandleEntryAsync_SetsStartedAtTimestamp()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA);

        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.True(result.StartedAt >= beforeCall);
        Assert.True(result.StartedAt <= afterCall);
    }

    [Fact]
    public async Task RequiresAuthenticationAsync_WithEmptyToken_ReturnsTrue()
    {
        // Act
        var requiresAuth = await _entryHandler.RequiresAuthenticationAsync(string.Empty);

        // Assert
        Assert.True(requiresAuth);
    }

    [Fact]
    public async Task RequiresAuthenticationAsync_WithNullToken_ReturnsTrue()
    {
        // Act
        var requiresAuth = await _entryHandler.RequiresAuthenticationAsync(null);

        // Assert
        Assert.True(requiresAuth);
    }

    [Fact]
    public async Task RequiresAuthenticationAsync_WithValidToken_ReturnsFalse()
    {
        // Arrange
        var sessionToken = "valid-session-token";
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(sessionToken))
            .ReturnsAsync(true);

        // Act
        var requiresAuth = await _entryHandler.RequiresAuthenticationAsync(sessionToken);

        // Assert
        Assert.False(requiresAuth);
    }

    [Fact]
    public async Task RequiresAuthenticationAsync_WithInvalidToken_ReturnsTrue()
    {
        // Arrange
        var sessionToken = "invalid-session-token";
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(sessionToken))
            .ReturnsAsync(false);

        // Act
        var requiresAuth = await _entryHandler.RequiresAuthenticationAsync(sessionToken);

        // Assert
        Assert.True(requiresAuth);
    }

    [Fact]
    public async Task HandleEntryAsync_WithAuthenticationServiceError_ReturnsError()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Auth service error"));

        // Act
        var result = await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Entry workflow failed", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleEntryAsync_TracksSourceInAnalytics()
    {
        // Arrange
        var sessionToken = "valid-session-token";
        var source = EntrySource.NavigationLink;

        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(sessionToken))
            .ReturnsAsync(true);

        _mockAnalyticsService
            .Setup(x => x.TrackEventAsync(It.IsAny<AnalyticsEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        await _entryHandler.HandleEntryAsync(source, sessionToken);

        // Assert
        _mockAnalyticsService.Verify(
            x => x.TrackEventAsync(It.Is<AnalyticsEvent>(e =>
                e.Properties.ContainsKey("source") &&
                e.Properties["source"].ToString() == source.ToString()
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleEntryAsync_TracksTimeToWorkspace()
    {
        // Arrange
        var sessionToken = "valid-session-token";
        _mockAuthService
            .Setup(x => x.CheckAuthStatusAsync(sessionToken))
            .ReturnsAsync(true);

        _mockAnalyticsService
            .Setup(x => x.TrackEventAsync(It.IsAny<AnalyticsEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        await _entryHandler.HandleEntryAsync(EntrySource.HomePageCTA, sessionToken);

        // Assert
        _mockAnalyticsService.Verify(
            x => x.TrackEventAsync(It.Is<AnalyticsEvent>(e =>
                e.Properties.ContainsKey("time_to_workspace_seconds") &&
                e.Properties["time_to_workspace_seconds"] is double
            )),
            Times.Once
        );
    }
}
