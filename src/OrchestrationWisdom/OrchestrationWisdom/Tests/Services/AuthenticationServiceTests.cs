using Xunit;
using Moq;
using OrchestrationWisdom.Services;
using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<ISessionManager> _mockSessionManager;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _mockSessionManager = new Mock<ISessionManager>();
        _authService = new AuthenticationService(_mockSessionManager.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidEmailPassword_ReturnsSuccess()
    {
        // Arrange
        var credentials = new AuthenticationCredentials
        {
            Email = "test@example.com",
            Password = "password123",
            Provider = AuthProvider.EmailPassword
        };

        var session = new UserSession
        {
            SessionId = "test-session-123",
            UserId = "test-user-id",
            Email = credentials.Email,
            Name = "test",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockSessionManager
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync(session);

        // Act
        var result = await _authService.AuthenticateAsync(credentials);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.UserId);
        Assert.NotNull(result.SessionToken);
        Assert.Equal(session.SessionId, result.SessionToken);
        Assert.NotNull(result.UserProfile);
        Assert.Equal(credentials.Email, result.UserProfile.Email);
        Assert.True(result.IsFirstTimeUser); // New user auto-registered
    }

    [Fact]
    public async Task AuthenticateAsync_WithExistingDemoUser_ReturnsSuccess()
    {
        // Arrange
        var credentials = new AuthenticationCredentials
        {
            Email = "demo@orchestrationwisdom.com",
            Password = "password123",
            Provider = AuthProvider.EmailPassword
        };

        var session = new UserSession
        {
            SessionId = "demo-session-123",
            UserId = "user-demo-001",
            Email = credentials.Email,
            Name = "Demo Creator",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockSessionManager
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync(session);

        // Act
        var result = await _authService.AuthenticateAsync(credentials);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("user-demo-001", result.UserId);
        Assert.NotNull(result.SessionToken);
        Assert.NotNull(result.UserProfile);
        Assert.Equal("Demo Creator", result.UserProfile.Name);
        Assert.False(result.IsFirstTimeUser); // Existing user
    }

    [Fact]
    public async Task AuthenticateAsync_WithOAuthProvider_ReturnsSuccess()
    {
        // Arrange
        var credentials = new AuthenticationCredentials
        {
            Email = "oauth@example.com",
            Provider = AuthProvider.Google,
            OAuthToken = "oauth-token-123"
        };

        var session = new UserSession
        {
            SessionId = "oauth-session-123",
            UserId = "oauth-user-id",
            Email = credentials.Email,
            Name = "oauth",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockSessionManager
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync(session);

        // Act
        var result = await _authService.AuthenticateAsync(credentials);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.UserId);
        Assert.NotNull(result.SessionToken);
        Assert.True(result.IsFirstTimeUser);
    }

    [Fact]
    public async Task CheckAuthStatusAsync_WithEmptyToken_ReturnsFalse()
    {
        // Arrange
        var sessionToken = string.Empty;

        // Act
        var result = await _authService.CheckAuthStatusAsync(sessionToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckAuthStatusAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var sessionToken = "valid-token-123";
        _mockSessionManager
            .Setup(x => x.ValidateSessionAsync(sessionToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.CheckAuthStatusAsync(sessionToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckAuthStatusAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var sessionToken = "invalid-token-123";
        _mockSessionManager
            .Setup(x => x.ValidateSessionAsync(sessionToken))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.CheckAuthStatusAsync(sessionToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithValidUserId_ReturnsProfile()
    {
        // Arrange - authenticate first to create a user
        var credentials = new AuthenticationCredentials
        {
            Email = "profile@example.com",
            Password = "password123",
            Provider = AuthProvider.EmailPassword
        };

        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = "user-123",
            Email = credentials.Email,
            Name = "profile",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockSessionManager
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync(session);

        var authResult = await _authService.AuthenticateAsync(credentials);

        // Act - use the actual userId returned from authentication
        var profile = await _authService.GetUserProfileAsync(authResult.UserId!);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(credentials.Email, profile.Email);
        Assert.Equal(UserRole.Creator, profile.Role);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithInvalidUserId_ReturnsNull()
    {
        // Act
        var profile = await _authService.GetUserProfileAsync("non-existent-user");

        // Assert
        Assert.Null(profile);
    }

    [Fact]
    public async Task ValidateSessionAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var sessionToken = "valid-token-123";
        _mockSessionManager
            .Setup(x => x.ValidateSessionAsync(sessionToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.ValidateSessionAsync(sessionToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AuthenticateAsync_UpdatesLastLoginTime()
    {
        // Arrange
        var credentials = new AuthenticationCredentials
        {
            Email = "demo@orchestrationwisdom.com",
            Password = "password123",
            Provider = AuthProvider.EmailPassword
        };

        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = "user-demo-001",
            Email = credentials.Email,
            Name = "Demo Creator",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockSessionManager
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync(session);

        var beforeLogin = DateTime.UtcNow;

        // Act
        var result = await _authService.AuthenticateAsync(credentials);
        var profile = await _authService.GetUserProfileAsync(result.UserId!);

        // Assert
        Assert.NotNull(profile);
        Assert.True(profile.LastLoginAt >= beforeLogin);
        Assert.True(profile.LastLoginAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AuthenticateAsync_CreatesSessionForNewUser()
    {
        // Arrange
        var credentials = new AuthenticationCredentials
        {
            Email = "newuser@example.com",
            Password = "password123",
            Provider = AuthProvider.EmailPassword
        };

        var session = new UserSession
        {
            SessionId = "new-session-123",
            UserId = "new-user-id",
            Email = credentials.Email,
            Name = "newuser",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _mockSessionManager
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync(session);

        // Act
        var result = await _authService.AuthenticateAsync(credentials);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.IsFirstTimeUser);
        _mockSessionManager.Verify(x => x.CreateSessionAsync(It.IsAny<UserProfile>()), Times.Once);
    }
}
