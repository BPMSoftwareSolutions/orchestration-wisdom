using OrchestrationWisdom.Models;
using System.Security.Cryptography;
using System.Text;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Handles user authentication and session creation
/// Movement 3: Authentication Flow
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(AuthenticationCredentials credentials);
    Task<bool> CheckAuthStatusAsync(string sessionToken);
    Task<UserProfile?> GetUserProfileAsync(string userId);
    Task<bool> ValidateSessionAsync(string sessionToken);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly Dictionary<string, UserProfile> _users = new();
    private readonly Dictionary<string, string> _sessionTokens = new(); // sessionToken -> userId
    private readonly ISessionManager _sessionManager;

    public AuthenticationService(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        InitializeSampleUsers();
    }

    /// <summary>
    /// Authenticates user with provided credentials
    /// Movement 3, Beat 6: Process Authentication
    /// Event: auth.completed
    /// </summary>
    public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationCredentials credentials)
    {
        try
        {
            // Validate credentials based on provider
            var (isValid, userId, isFirstTime) = await ValidateCredentials(credentials);

            if (!isValid || userId == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            // Get user profile
            var userProfile = await GetUserProfileAsync(userId);

            if (userProfile == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User profile not found"
                };
            }

            // Create session
            var session = await _sessionManager.CreateSessionAsync(userProfile);

            // Update last login
            userProfile.LastLoginAt = DateTime.UtcNow;

            return new AuthenticationResult
            {
                Success = true,
                UserId = userId,
                SessionToken = session.SessionId,
                UserProfile = userProfile,
                IsFirstTimeUser = isFirstTime
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH ERROR] Authentication failed: {ex.Message}");
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Authentication service error"
            };
        }
    }

    /// <summary>
    /// Checks if user has valid authentication
    /// Movement 2, Beat 4: Check Authentication Status
    /// Event: auth.status.checked
    /// </summary>
    public async Task<bool> CheckAuthStatusAsync(string sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken))
            return false;

        return await _sessionManager.ValidateSessionAsync(sessionToken);
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        await Task.CompletedTask;
        return _users.ContainsKey(userId) ? _users[userId] : null;
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        return await CheckAuthStatusAsync(sessionToken);
    }

    private async Task<(bool isValid, string? userId, bool isFirstTime)> ValidateCredentials(AuthenticationCredentials credentials)
    {
        await Task.CompletedTask;

        if (credentials.Provider == AuthProvider.EmailPassword)
        {
            // Simple email/password validation (in production, use proper password hashing)
            var user = _users.Values.FirstOrDefault(u => u.Email.Equals(credentials.Email, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                // For demo purposes, accept any password
                // In production: verify hashed password
                return (true, user.UserId, false);
            }

            // Create new user if doesn't exist (auto-registration for demo)
            var newUserId = $"user-{Guid.NewGuid():N}";
            var newUser = new UserProfile
            {
                UserId = newUserId,
                Email = credentials.Email,
                Name = credentials.Email.Split('@')[0],
                Role = UserRole.Creator,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                Preferences = new WorkspacePreferences()
            };

            _users[newUserId] = newUser;
            return (true, newUserId, true);
        }
        else
        {
            // OAuth validation (in production, validate OAuth token with provider)
            // For demo, create user from OAuth token
            var userId = $"oauth-{Guid.NewGuid():N}";
            var user = new UserProfile
            {
                UserId = userId,
                Email = credentials.Email,
                Name = credentials.Email.Split('@')[0],
                Role = UserRole.Creator,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                Preferences = new WorkspacePreferences()
            };

            _users[userId] = user;
            return (true, userId, true);
        }
    }

    private void InitializeSampleUsers()
    {
        // Create sample users for demo
        var sampleUser = new UserProfile
        {
            UserId = "user-demo-001",
            Email = "demo@orchestrationwisdom.com",
            Name = "Demo Creator",
            Role = UserRole.Creator,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastLoginAt = DateTime.UtcNow,
            Preferences = new WorkspacePreferences
            {
                Theme = "light",
                OnboardingTooltips = false // Existing user
            }
        };

        _users[sampleUser.UserId] = sampleUser;
    }
}
