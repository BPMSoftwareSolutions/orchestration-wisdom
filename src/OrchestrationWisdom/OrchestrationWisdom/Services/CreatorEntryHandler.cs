using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Orchestrates the complete creator entry workflow
/// Coordinates all movements from homepage to workspace
/// </summary>
public interface ICreatorEntryHandler
{
    Task<CreatorEntryResult> HandleEntryAsync(EntrySource source, string? sessionToken = null);
    Task<bool> RequiresAuthenticationAsync(string? sessionToken);
}

public class CreatorEntryHandler : ICreatorEntryHandler
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IWorkspaceService _workspaceService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IAuditLogger _auditLogger;

    public CreatorEntryHandler(
        IAuthenticationService authenticationService,
        IWorkspaceService workspaceService,
        IAnalyticsService analyticsService,
        IAuditLogger auditLogger)
    {
        _authenticationService = authenticationService;
        _workspaceService = workspaceService;
        _analyticsService = analyticsService;
        _auditLogger = auditLogger;
    }

    /// <summary>
    /// Handles creator entry workflow
    /// Movement 2, Beat 3: Handle Creator Entry Click
    /// Event: creator.entry.initiated
    /// </summary>
    public async Task<CreatorEntryResult> HandleEntryAsync(EntrySource source, string? sessionToken = null)
    {
        var startTime = DateTime.UtcNow;

        var result = new CreatorEntryResult
        {
            Source = source,
            StartedAt = startTime
        };

        try
        {
            // Check authentication status
            var isAuthenticated = await _authenticationService.CheckAuthStatusAsync(sessionToken ?? string.Empty);

            result.RequiresAuthentication = !isAuthenticated;

            if (!isAuthenticated)
            {
                Console.WriteLine($"[ENTRY] User not authenticated - redirecting to sign-in (Source: {source})");
                result.RedirectTo = "/signin";
                result.Success = false;
                return result;
            }

            // User is authenticated - proceed to workspace
            Console.WriteLine($"[ENTRY] User authenticated - loading workspace (Source: {source})");

            // Get user session to load workspace
            // In production, extract userId from sessionToken
            result.RedirectTo = "/workspace";
            result.Success = true;

            // Track analytics
            await TrackEntryCompletion(source, startTime, sessionToken);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ENTRY ERROR] {ex.Message}");
            result.Success = false;
            result.ErrorMessage = "Entry workflow failed";
            return result;
        }
    }

    public async Task<bool> RequiresAuthenticationAsync(string? sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken))
            return true;

        return !await _authenticationService.CheckAuthStatusAsync(sessionToken);
    }

    private async Task TrackEntryCompletion(EntrySource source, DateTime startTime, string? sessionToken)
    {
        var timeToWorkspace = (DateTime.UtcNow - startTime).TotalSeconds;

        await _analyticsService.TrackEventAsync(new AnalyticsEvent
        {
            EventType = "creator.entry.completed",
            UserId = sessionToken ?? "unknown",
            Timestamp = DateTime.UtcNow,
            Properties = new Dictionary<string, object>
            {
                { "source", source.ToString() },
                { "time_to_workspace_seconds", timeToWorkspace }
            }
        });
    }
}

public class CreatorEntryResult
{
    public bool Success { get; set; }
    public EntrySource Source { get; set; }
    public bool RequiresAuthentication { get; set; }
    public string RedirectTo { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
}
