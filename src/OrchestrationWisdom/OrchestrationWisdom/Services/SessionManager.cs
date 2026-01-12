using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Manages user sessions and session lifecycle
/// Movement 3, Beat 7: Create User Session
/// </summary>
public interface ISessionManager
{
    Task<UserSession> CreateSessionAsync(UserProfile userProfile);
    Task<UserSession?> GetSessionAsync(string sessionToken);
    Task<bool> ValidateSessionAsync(string sessionToken);
    Task InvalidateSessionAsync(string sessionToken);
}

public class SessionManager : ISessionManager
{
    private readonly Dictionary<string, UserSession> _sessions = new();
    private const int SessionExpirationHours = 24;

    /// <summary>
    /// Creates a new user session
    /// Event: session.created
    /// </summary>
    public Task<UserSession> CreateSessionAsync(UserProfile userProfile)
    {
        var sessionId = GenerateSessionToken();

        var session = new UserSession
        {
            SessionId = sessionId,
            UserId = userProfile.UserId,
            Email = userProfile.Email,
            Name = userProfile.Name,
            Role = userProfile.Role,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(SessionExpirationHours),
            OnboardingComplete = !userProfile.Preferences.OnboardingTooltips,
            WorkspaceEnabled = true
        };

        _sessions[sessionId] = session;

        Console.WriteLine($"""
            [SESSION] Session created
            Session ID: {sessionId}
            User: {userProfile.Email}
            Expires: {session.ExpiresAt:yyyy-MM-dd HH:mm:ss UTC}
            """);

        return Task.FromResult(session);
    }

    public Task<UserSession?> GetSessionAsync(string sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken))
            return Task.FromResult<UserSession?>(null);

        if (!_sessions.ContainsKey(sessionToken))
            return Task.FromResult<UserSession?>(null);

        var session = _sessions[sessionToken];

        // Check if session is expired
        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _sessions.Remove(sessionToken);
            return Task.FromResult<UserSession?>(null);
        }

        return Task.FromResult<UserSession?>(session);
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        var session = await GetSessionAsync(sessionToken);
        return session != null;
    }

    public Task InvalidateSessionAsync(string sessionToken)
    {
        if (_sessions.ContainsKey(sessionToken))
        {
            _sessions.Remove(sessionToken);
            Console.WriteLine($"[SESSION] Session invalidated: {sessionToken}");
        }

        return Task.CompletedTask;
    }

    private string GenerateSessionToken()
    {
        return $"sess_{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}
