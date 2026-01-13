using Xunit;
using OrchestrationWisdom.Services;
using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Tests.Services;

public class AuditLoggerTests
{
    private readonly AuditLogger _auditLogger;

    public AuditLoggerTests()
    {
        _auditLogger = new AuditLogger();
    }

    [Fact]
    public async Task LogCreatorEntryAsync_CreatesAuditLog()
    {
        // Arrange
        var userId = "test-user-123";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";
        var success = true;

        // Act
        await _auditLogger.LogCreatorEntryAsync(userId, ipAddress, userAgent, success);
        var logs = await _auditLogger.GetAuditLogsAsync(userId);

        // Assert
        Assert.Single(logs);
        var log = logs.First();
        Assert.Equal(userId, log.UserId);
        Assert.Equal("creator.entry", log.EventType);
        Assert.Equal(ipAddress, log.IpAddress);
        Assert.Equal(userAgent, log.UserAgent);
        Assert.True(log.Success);
        Assert.NotEmpty(log.LogId);
        Assert.StartsWith("audit_", log.LogId);
    }

    [Fact]
    public async Task LogCreatorEntryAsync_SetsCorrectTimestamp()
    {
        // Arrange
        var userId = "test-user-123";
        var beforeLog = DateTime.UtcNow;

        // Act
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);
        var logs = await _auditLogger.GetAuditLogsAsync(userId);

        var afterLog = DateTime.UtcNow;

        // Assert
        var log = logs.First();
        Assert.True(log.Timestamp >= beforeLog);
        Assert.True(log.Timestamp <= afterLog);
    }

    [Fact]
    public async Task LogCreatorEntryAsync_AddsWorkspaceAccessContext()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);
        var logs = await _auditLogger.GetAuditLogsAsync(userId);

        // Assert
        var log = logs.First();
        Assert.NotNull(log.Context);
        Assert.True(log.Context.ContainsKey("action"));
        Assert.Equal("workspace_access", log.Context["action"]);
    }

    [Fact]
    public async Task LogCreatorEntryAsync_CanLogFailedAttempts()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", false);
        var logs = await _auditLogger.GetAuditLogsAsync(userId);

        // Assert
        var log = logs.First();
        Assert.False(log.Success);
    }

    [Fact]
    public async Task LogAuthenticationAttemptAsync_CreatesAuditLog()
    {
        // Arrange
        var email = "test@example.com";
        var provider = AuthProvider.EmailPassword;
        var success = true;
        var ipAddress = "192.168.1.1";

        // Act
        await _auditLogger.LogAuthenticationAttemptAsync(email, provider, success, ipAddress);
        var logs = await _auditLogger.GetAuditLogsAsync(email);

        // Assert
        Assert.Single(logs);
        var log = logs.First();
        Assert.Equal(email, log.UserId); // Uses email as identifier
        Assert.Equal("authentication.attempt", log.EventType);
        Assert.Equal(ipAddress, log.IpAddress);
        Assert.True(log.Success);
    }

    [Fact]
    public async Task LogAuthenticationAttemptAsync_WithNullIpAddress_UsesUnknown()
    {
        // Arrange
        var email = "test@example.com";
        var provider = AuthProvider.EmailPassword;

        // Act
        await _auditLogger.LogAuthenticationAttemptAsync(email, provider, true, null);
        var logs = await _auditLogger.GetAuditLogsAsync(email);

        // Assert
        var log = logs.First();
        Assert.Equal("unknown", log.IpAddress);
    }

    [Fact]
    public async Task LogAuthenticationAttemptAsync_StoresProviderInContext()
    {
        // Arrange
        var email = "test@example.com";
        var provider = AuthProvider.Google;

        // Act
        await _auditLogger.LogAuthenticationAttemptAsync(email, provider, true);
        var logs = await _auditLogger.GetAuditLogsAsync(email);

        // Assert
        var log = logs.First();
        Assert.True(log.Context.ContainsKey("provider"));
        Assert.Equal(provider.ToString(), log.Context["provider"]);
    }

    [Fact]
    public async Task LogAuthenticationAttemptAsync_StoresEmailInContext()
    {
        // Arrange
        var email = "test@example.com";
        var provider = AuthProvider.EmailPassword;

        // Act
        await _auditLogger.LogAuthenticationAttemptAsync(email, provider, true);
        var logs = await _auditLogger.GetAuditLogsAsync(email);

        // Assert
        var log = logs.First();
        Assert.True(log.Context.ContainsKey("email"));
        Assert.Equal(email, log.Context["email"]);
    }

    [Fact]
    public async Task LogAuthenticationAttemptAsync_CanLogFailedAttempts()
    {
        // Arrange
        var email = "test@example.com";
        var provider = AuthProvider.EmailPassword;

        // Act
        await _auditLogger.LogAuthenticationAttemptAsync(email, provider, false);
        var logs = await _auditLogger.GetAuditLogsAsync(email);

        // Assert
        var log = logs.First();
        Assert.False(log.Success);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithNoLogs_ReturnsEmptyList()
    {
        // Act
        var logs = await _auditLogger.GetAuditLogsAsync("non-existent-user");

        // Assert
        Assert.NotNull(logs);
        Assert.Empty(logs);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ReturnsLogsInDescendingOrder()
    {
        // Arrange
        var userId = "test-user-123";
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);
        await Task.Delay(10); // Small delay to ensure different timestamps
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);

        // Act
        var logs = await _auditLogger.GetAuditLogsAsync(userId);

        // Assert
        Assert.Equal(2, logs.Count);
        Assert.True(logs[0].Timestamp >= logs[1].Timestamp,
            "Logs should be ordered by timestamp descending");
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithStartDate_FiltersOldLogs()
    {
        // Arrange
        var userId = "test-user-123";
        var cutoffDate = DateTime.UtcNow;

        // Log entry before cutoff (won't be included in test due to timing)
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);

        await Task.Delay(10);

        // Log entry after cutoff
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);

        // Act
        var logs = await _auditLogger.GetAuditLogsAsync(userId, cutoffDate);

        // Assert
        Assert.NotEmpty(logs);
        Assert.All(logs, log => Assert.True(log.Timestamp >= cutoffDate));
    }

    [Fact]
    public async Task GetAuditLogsAsync_OnlyReturnsLogsForSpecificUser()
    {
        // Arrange
        var userId1 = "user-1";
        var userId2 = "user-2";

        await _auditLogger.LogCreatorEntryAsync(userId1, "192.168.1.1", "Mozilla/5.0", true);
        await _auditLogger.LogCreatorEntryAsync(userId2, "192.168.1.2", "Mozilla/5.0", true);
        await _auditLogger.LogCreatorEntryAsync(userId1, "192.168.1.1", "Mozilla/5.0", true);

        // Act
        var logs = await _auditLogger.GetAuditLogsAsync(userId1);

        // Assert
        Assert.Equal(2, logs.Count);
        Assert.All(logs, log => Assert.Equal(userId1, log.UserId));
    }

    [Fact]
    public async Task LogId_IsUnique()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);
        var logs = await _auditLogger.GetAuditLogsAsync(userId);

        // Assert
        Assert.Equal(2, logs.Count);
        Assert.NotEqual(logs[0].LogId, logs[1].LogId);
    }

    [Fact]
    public async Task LogAuthenticationAttemptAsync_WithDifferentProviders()
    {
        // Arrange
        var email = "test@example.com";
        var providers = new[] {
            AuthProvider.EmailPassword,
            AuthProvider.Google,
            AuthProvider.GitHub,
            AuthProvider.Microsoft
        };

        // Act
        foreach (var provider in providers)
        {
            await _auditLogger.LogAuthenticationAttemptAsync(email, provider, true);
        }
        var logs = await _auditLogger.GetAuditLogsAsync(email);

        // Assert
        Assert.Equal(providers.Length, logs.Count);
        foreach (var provider in providers)
        {
            Assert.Contains(logs, log => log.Context["provider"] == provider.ToString());
        }
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithNullStartDate_ReturnsAllLogs()
    {
        // Arrange
        var userId = "test-user-123";
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);
        await _auditLogger.LogCreatorEntryAsync(userId, "192.168.1.1", "Mozilla/5.0", true);

        // Act
        var logs = await _auditLogger.GetAuditLogsAsync(userId, null);

        // Assert
        Assert.Equal(2, logs.Count);
    }
}
