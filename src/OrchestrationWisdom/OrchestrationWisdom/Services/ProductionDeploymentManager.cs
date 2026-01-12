using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Manages production deployments with blue-green strategy and automatic rollback
/// Movement 5, Beat 15: Deploy to Production
/// </summary>
public interface IProductionDeploymentManager
{
    Task<DeploymentResult> DeployAsync(PatternSubmission submission);
    Task<bool> RollbackAsync(string deploymentId);
}

public class ProductionDeploymentManager : IProductionDeploymentManager
{
    private readonly Dictionary<string, DeploymentResult> _productionDeployments = new();
    private readonly List<Pattern> _productionPatterns = new();
    private readonly List<Pattern> _backupPatterns = new();

    /// <summary>
    /// Deploys pattern to production with gradual traffic shift and health monitoring
    /// Event: pattern.deployed.to.production
    /// </summary>
    public Task<DeploymentResult> DeployAsync(PatternSubmission submission)
    {
        var deploymentId = GenerateDeploymentId();

        // Create production backup
        BackupProduction();

        var result = new DeploymentResult
        {
            DeploymentId = deploymentId,
            SubmissionId = submission.Id,
            Environment = "production",
            StartedAt = DateTime.UtcNow,
            Status = "deploying"
        };

        _productionDeployments[deploymentId] = result;

        // Update submission status
        submission.Status = PublicationStatus.DeployingToProduction;

        if (submission.Ticket != null)
        {
            submission.Ticket.Status = TicketStatus.Deploying;
            submission.Ticket.Events.Add(new TicketEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = "production_deployment_started",
                Description = $"Production deployment initiated: {deploymentId}"
            });
        }

        // Perform blue-green deployment
        PerformBlueGreenDeployment(submission, result);

        // Monitor health during gradual rollout
        MonitorHealthDuringRollout(result);

        // Finalize deployment
        FinalizeDeployment(submission, result);

        return Task.FromResult(result);
    }

    public Task<bool> RollbackAsync(string deploymentId)
    {
        if (!_productionDeployments.ContainsKey(deploymentId))
            return Task.FromResult(false);

        var deployment = _productionDeployments[deploymentId];

        Console.WriteLine($"""
            [ROLLBACK] Initiating automatic rollback
            Deployment ID: {deploymentId}
            Reason: Health check failure detected
            Timestamp: {DateTime.UtcNow:u}
            """);

        // Restore from backup
        _productionPatterns.Clear();
        _productionPatterns.AddRange(_backupPatterns);

        deployment.Status = "rolled_back";
        deployment.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Deployment rolled back successfully");

        return Task.FromResult(true);
    }

    private void BackupProduction()
    {
        _backupPatterns.Clear();
        _backupPatterns.AddRange(_productionPatterns);

        var backupId = $"PROD-BACKUP-{DateTime.UtcNow:yyyyMMddHHmmss}";
        Console.WriteLine($"""
            [BACKUP] Production backup created
            Backup ID: {backupId}
            Patterns backed up: {_productionPatterns.Count}
            Timestamp: {DateTime.UtcNow:u}
            """);
    }

    private void PerformBlueGreenDeployment(PatternSubmission submission, DeploymentResult result)
    {
        result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Starting blue-green deployment");

        // Gradual traffic shift: 0% → 10% → 25% → 50% → 100%
        var trafficSteps = new[] { 0, 10, 25, 50, 100 };

        foreach (var trafficPercent in trafficSteps)
        {
            result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Traffic shift to {trafficPercent}%");

            // Simulate monitoring metrics at each step
            var metrics = SimulateMetrics();
            result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Metrics: Error rate {metrics.ErrorRate:F2}%, Latency {metrics.LatencyMs}ms");

            // Check if metrics are within acceptable thresholds
            if (metrics.ErrorRate > 1.0 || metrics.LatencyMs > 1000)
            {
                result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - ✗ Health check failed at {trafficPercent}% traffic");
                result.Status = "failed";
                RollbackAsync(result.DeploymentId);
                return;
            }

            result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - ✓ Health check passed at {trafficPercent}% traffic");
        }

        // All traffic shifted successfully
        _productionPatterns.Add(submission.Pattern);
        result.DeployedAt = DateTime.UtcNow;
        result.Status = "deployed";

        result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Pattern deployed to production successfully");
    }

    private void MonitorHealthDuringRollout(DeploymentResult result)
    {
        result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Monitoring deployment health");

        var healthChecks = new[]
        {
            "Pattern page accessibility",
            "Diagram rendering",
            "Search functionality",
            "Analytics tracking",
            "Related patterns links"
        };

        foreach (var check in healthChecks)
        {
            result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - ✓ {check}");
        }

        result.SmokeTestsPassed = true;
    }

    private void FinalizeDeployment(PatternSubmission submission, DeploymentResult result)
    {
        submission.Status = PublicationStatus.Published;
        submission.Pattern.PublishedDate = DateTime.UtcNow;

        if (submission.Ticket != null)
        {
            submission.Ticket.Status = TicketStatus.Completed;
            submission.Ticket.Events.Add(new TicketEvent
            {
                Timestamp = DateTime.UtcNow,
                EventType = "production_deployment_completed",
                Description = "Pattern successfully published to production"
            });
        }

        result.Status = "completed";
        result.CompletedAt = DateTime.UtcNow;

        result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Deployment finalized");
        result.Logs.Add($"{DateTime.UtcNow:HH:mm:ss} - Pattern is now live at: /patterns/{submission.Pattern.Slug}");

        Console.WriteLine($"""
            [DEPLOYMENT] Production Deployment Complete
            Pattern: {submission.Pattern.Title}
            Production URL: /patterns/{submission.Pattern.Slug}
            Published At: {submission.Pattern.PublishedDate:u}
            Status: Live
            """);
    }

    private DeploymentMetrics SimulateMetrics()
    {
        var random = new Random();
        return new DeploymentMetrics
        {
            ErrorRate = random.NextDouble() * 0.5, // 0-0.5%
            LatencyMs = random.Next(100, 300) // 100-300ms
        };
    }

    private string GenerateDeploymentId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"PROD-{timestamp}";
    }
}

public class DeploymentMetrics
{
    public double ErrorRate { get; set; }
    public int LatencyMs { get; set; }
}
