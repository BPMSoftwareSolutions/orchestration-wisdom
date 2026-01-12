using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Logs audit events for security and compliance
/// Movement 5, Beat 13: Log Entry Event for Audit
/// </summary>
public interface IAuditLogger
{
    Task LogCreatorEntryAsync(string userId, string ipAddress, string userAgent, bool success);
    Task LogAuthenticationAttemptAsync(string email, AuthProvider provider, bool success, string? ipAddress = null);
    Task<List<AuditLogEntry>> GetAuditLogsAsync(string userId, DateTime? startDate = null);
}

public class AuditLogger : IAuditLogger
{
    private readonly List<AuditLogEntry> _auditLogs = new();

    /// <summary>
    /// Logs creator entry event
    /// Event: audit.creator.entry
    /// </summary>
    public Task LogCreatorEntryAsync(string userId, string ipAddress, string userAgent, bool success)
    {
        var logEntry = new AuditLogEntry
        {
            LogId = $"audit_{Guid.NewGuid():N}",
            UserId = userId,
            EventType = "creator.entry",
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            Context = new Dictionary<string, string>
            {
                { "action", "workspace_access" }
            }
        };

        _auditLogs.Add(logEntry);

        Console.WriteLine($"""
            [AUDIT] Creator Entry Logged
            Log ID: {logEntry.LogId}
            User: {userId}
            IP: {ipAddress}
            Success: {success}
            Timestamp: {logEntry.Timestamp:yyyy-MM-dd HH:mm:ss UTC}
            """);

        return Task.CompletedTask;
    }

    public Task LogAuthenticationAttemptAsync(string email, AuthProvider provider, bool success, string? ipAddress = null)
    {
        var logEntry = new AuditLogEntry
        {
            LogId = $"audit_{Guid.NewGuid():N}",
            UserId = email, // Use email as identifier for failed attempts
            EventType = "authentication.attempt",
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress ?? "unknown",
            UserAgent = "unknown",
            Success = success,
            Context = new Dictionary<string, string>
            {
                { "provider", provider.ToString() },
                { "email", email }
            }
        };

        _auditLogs.Add(logEntry);

        Console.WriteLine($"""
            [AUDIT] Authentication Attempt Logged
            Email: {email}
            Provider: {provider}
            Success: {success}
            IP: {ipAddress ?? "unknown"}
            """);

        return Task.CompletedTask;
    }

    public Task<List<AuditLogEntry>> GetAuditLogsAsync(string userId, DateTime? startDate = null)
    {
        var logs = _auditLogs
            .Where(l => l.UserId == userId)
            .Where(l => !startDate.HasValue || l.Timestamp >= startDate.Value)
            .OrderByDescending(l => l.Timestamp)
            .ToList();

        return Task.FromResult(logs);
    }
}
