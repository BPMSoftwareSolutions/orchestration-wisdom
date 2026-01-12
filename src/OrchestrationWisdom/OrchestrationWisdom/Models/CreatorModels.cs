namespace OrchestrationWisdom.Models;

/// <summary>
/// Represents a user session for authenticated creators
/// </summary>
public class UserSession
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool OnboardingComplete { get; set; }
    public bool WorkspaceEnabled { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// User authentication credentials
/// </summary>
public class AuthenticationCredentials
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public AuthProvider Provider { get; set; }
    public string? OAuthToken { get; set; }
}

/// <summary>
/// Result of authentication attempt
/// </summary>
public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string? SessionToken { get; set; }
    public string? ErrorMessage { get; set; }
    public UserProfile? UserProfile { get; set; }
    public bool IsFirstTimeUser { get; set; }
}

/// <summary>
/// User profile information
/// </summary>
public class UserProfile
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public WorkspacePreferences Preferences { get; set; } = new();
}

/// <summary>
/// Creator workspace preferences
/// </summary>
public class WorkspacePreferences
{
    public string Theme { get; set; } = "light";
    public string Layout { get; set; } = "default";
    public bool NotificationsEnabled { get; set; } = true;
    public bool OnboardingTooltips { get; set; } = true;
}

/// <summary>
/// Creator workspace state
/// </summary>
public class WorkspaceState
{
    public string UserId { get; set; } = string.Empty;
    public DateTime LoadedAt { get; set; }
    public List<PatternDraft> RecentDrafts { get; set; } = new();
    public WorkspacePreferences Preferences { get; set; } = new();
    public bool ShowOnboarding { get; set; }
    public WorkspaceStats Stats { get; set; } = new();
}

/// <summary>
/// Pattern draft in workspace
/// </summary>
public class PatternDraft
{
    public string DraftId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CompletionPercentage { get; set; }
}

/// <summary>
/// Workspace statistics
/// </summary>
public class WorkspaceStats
{
    public int TotalDrafts { get; set; }
    public int PublishedPatterns { get; set; }
    public int PendingReview { get; set; }
    public DateTime? LastPublished { get; set; }
}

/// <summary>
/// Creator entry event for analytics
/// </summary>
public class CreatorEntryEvent
{
    public string EventId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public EntrySource Source { get; set; }
    public AuthProvider AuthMethod { get; set; }
    public double TimeToWorkspaceSeconds { get; set; }
    public bool IsFirstVisit { get; set; }
    public string? ReferrerUrl { get; set; }
    public string DeviceType { get; set; } = string.Empty;
}

/// <summary>
/// Analytics tracking data
/// </summary>
public class AnalyticsEvent
{
    public string EventType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
}

/// <summary>
/// Audit log entry
/// </summary>
public class AuditLogEntry
{
    public string LogId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Dictionary<string, string> Context { get; set; } = new();
    public bool Success { get; set; }
}

// Enums
public enum UserRole
{
    Creator,
    Reviewer,
    Admin
}

public enum AuthProvider
{
    EmailPassword,
    Google,
    GitHub,
    Microsoft
}

public enum EntrySource
{
    HomePageCTA,
    NavigationLink,
    DirectURL,
    EmailLink
}
