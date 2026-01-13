using Xunit;
using OrchestrationWisdom.Services;
using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Tests.Services;

public class SessionManagerTests
{
    private readonly SessionManager _sessionManager;

    public SessionManagerTests()
    {
        _sessionManager = new SessionManager();
    }

    [Fact]
    public async Task CreateSessionAsync_ReturnsValidSession()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            UserId = "test-user-123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences()
        };

        // Act
        var session = await _sessionManager.CreateSessionAsync(userProfile);

        // Assert
        Assert.NotNull(session);
        Assert.NotEmpty(session.SessionId);
        Assert.Equal(userProfile.UserId, session.UserId);
        Assert.Equal(userProfile.Email, session.Email);
        Assert.Equal(userProfile.Name, session.Name);
        Assert.Equal(userProfile.Role, session.Role);
        Assert.True(session.ExpiresAt > DateTime.UtcNow);
        Assert.True(session.WorkspaceEnabled);
    }

    [Fact]
    public async Task CreateSessionAsync_SetsExpirationTo24Hours()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            UserId = "test-user-123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences()
        };

        // Act
        var session = await _sessionManager.CreateSessionAsync(userProfile);

        // Assert
        var expectedExpiration = session.CreatedAt.AddHours(24);
        var timeDifference = Math.Abs((session.ExpiresAt - expectedExpiration).TotalSeconds);
        Assert.True(timeDifference < 1); // Within 1 second
    }

    [Fact]
    public async Task CreateSessionAsync_SetsOnboardingCompleteBasedOnPreferences()
    {
        // Arrange - new user with onboarding enabled
        var newUser = new UserProfile
        {
            UserId = "new-user-123",
            Email = "new@example.com",
            Name = "New User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences { OnboardingTooltips = true }
        };

        // Act
        var newSession = await _sessionManager.CreateSessionAsync(newUser);

        // Assert
        Assert.False(newSession.OnboardingComplete);

        // Arrange - existing user with onboarding disabled
        var existingUser = new UserProfile
        {
            UserId = "existing-user-123",
            Email = "existing@example.com",
            Name = "Existing User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences { OnboardingTooltips = false }
        };

        // Act
        var existingSession = await _sessionManager.CreateSessionAsync(existingUser);

        // Assert
        Assert.True(existingSession.OnboardingComplete);
    }

    [Fact]
    public async Task GetSessionAsync_WithValidToken_ReturnsSession()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            UserId = "test-user-123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences()
        };

        var createdSession = await _sessionManager.CreateSessionAsync(userProfile);

        // Act
        var retrievedSession = await _sessionManager.GetSessionAsync(createdSession.SessionId);

        // Assert
        Assert.NotNull(retrievedSession);
        Assert.Equal(createdSession.SessionId, retrievedSession.SessionId);
        Assert.Equal(createdSession.UserId, retrievedSession.UserId);
        Assert.Equal(createdSession.Email, retrievedSession.Email);
    }

    [Fact]
    public async Task GetSessionAsync_WithEmptyToken_ReturnsNull()
    {
        // Act
        var session = await _sessionManager.GetSessionAsync(string.Empty);

        // Assert
        Assert.Null(session);
    }

    [Fact]
    public async Task GetSessionAsync_WithInvalidToken_ReturnsNull()
    {
        // Act
        var session = await _sessionManager.GetSessionAsync("non-existent-token");

        // Assert
        Assert.Null(session);
    }

    [Fact]
    public async Task ValidateSessionAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            UserId = "test-user-123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences()
        };

        var session = await _sessionManager.CreateSessionAsync(userProfile);

        // Act
        var isValid = await _sessionManager.ValidateSessionAsync(session.SessionId);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateSessionAsync_WithInvalidToken_ReturnsFalse()
    {
        // Act
        var isValid = await _sessionManager.ValidateSessionAsync("invalid-token");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task InvalidateSessionAsync_RemovesSession()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            UserId = "test-user-123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences()
        };

        var session = await _sessionManager.CreateSessionAsync(userProfile);

        // Act
        await _sessionManager.InvalidateSessionAsync(session.SessionId);
        var retrievedSession = await _sessionManager.GetSessionAsync(session.SessionId);

        // Assert
        Assert.Null(retrievedSession);
    }

    [Fact]
    public async Task InvalidateSessionAsync_WithNonExistentToken_DoesNotThrow()
    {
        // Act & Assert - should not throw
        await _sessionManager.InvalidateSessionAsync("non-existent-token");
    }

    [Fact]
    public async Task CreateSessionAsync_GeneratesUniqueSessionIds()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            UserId = "test-user-123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences()
        };

        // Act
        var session1 = await _sessionManager.CreateSessionAsync(userProfile);
        var session2 = await _sessionManager.CreateSessionAsync(userProfile);

        // Assert
        Assert.NotEqual(session1.SessionId, session2.SessionId);
    }

    [Fact]
    public async Task SessionId_HasCorrectFormat()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            UserId = "test-user-123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences()
        };

        // Act
        var session = await _sessionManager.CreateSessionAsync(userProfile);

        // Assert
        Assert.StartsWith("sess_", session.SessionId);
        Assert.Contains("_", session.SessionId.Substring(5)); // Should have another underscore after guid
    }
}
