using OrchestrationWisdom.Models;
using OrchestrationWisdom.Services;

namespace OrchestrationWisdom.Services.Examples;

/// <summary>
/// Demonstrates the complete pattern publication workflow
/// This example shows all 6 movements of the pattern-publication-process sequence
/// </summary>
public class PatternPublicationExample
{
    public static async Task RunCompleteWorkflowExample()
    {
        Console.WriteLine("===== Pattern Publication Workflow - Complete Example =====\n");

        // Initialize all services
        var publicationService = new PatternPublicationService();
        var metadataExtractor = new PatternMetadataExtractor();
        var workflowOrchestrator = new PublicationWorkflowOrchestrator();
        var schemaValidator = new SchemaValidator();
        var hqoCalculator = new HQOScorecardCalculator();
        var diagramValidator = new MermaidDiagramValidator();
        var reportGenerator = new ValidationReportGenerator();
        var reviewerQueueManager = new ReviewerQueueManager();
        var notificationService = new NotificationService();

        var orchestrator = new PatternPublicationOrchestrator(
            publicationService,
            metadataExtractor,
            workflowOrchestrator,
            schemaValidator,
            hqoCalculator,
            diagramValidator,
            reportGenerator,
            reviewerQueueManager,
            notificationService
        );

        // Create a sample pattern
        var pattern = CreateSamplePattern();

        Console.WriteLine($"Pattern: {pattern.Title}");
        Console.WriteLine($"Author: sample-author@example.com\n");

        // Execute the complete workflow
        var result = await orchestrator.PublishPatternAsync(
            pattern,
            "author-123",
            "sample-author@example.com"
        );

        // Display results
        Console.WriteLine("\n===== Workflow Execution Results =====\n");
        Console.WriteLine($"Submission ID: {result.SubmissionId}");
        Console.WriteLine($"Ticket ID: {result.TicketId}");
        Console.WriteLine($"Status: {result.Status}");
        Console.WriteLine($"Started: {result.StartedAt:yyyy-MM-dd HH:mm:ss UTC}");
        Console.WriteLine($"Completed: {result.CompletedAt:yyyy-MM-dd HH:mm:ss UTC}");
        Console.WriteLine($"Duration: {(result.CompletedAt - result.StartedAt).TotalSeconds:F2} seconds\n");

        Console.WriteLine("Workflow Steps Completed:");
        foreach (var step in result.Steps)
        {
            var statusIcon = step.Status == "completed" ? "✓" :
                           step.Status == "failed" ? "✗" :
                           "⏳";

            Console.WriteLine($"{statusIcon} Movement {step.Movement}, Beat {step.Beat}: {step.Name} - {step.Status}");
            if (!string.IsNullOrEmpty(step.Details))
            {
                Console.WriteLine($"   Details: {step.Details}");
            }
        }

        if (result.ValidationReport != null)
        {
            Console.WriteLine("\n===== Validation Report =====\n");
            Console.WriteLine($"Overall Valid: {result.ValidationReport.OverallValid}");
            Console.WriteLine($"Status: {result.ValidationReport.Status}");

            if (!result.ValidationReport.OverallValid)
            {
                Console.WriteLine("\nValidation Errors:");
                DisplayValidationErrors(result.ValidationReport);
            }

            Console.WriteLine("\nNext Steps:");
            foreach (var step in result.ValidationReport.NextSteps)
            {
                Console.WriteLine($"- {step}");
            }
        }

        Console.WriteLine("\n===== Example Complete =====\n");
    }

    public static async Task RunFailedValidationExample()
    {
        Console.WriteLine("===== Pattern Publication - Failed Validation Example =====\n");

        var publicationService = new PatternPublicationService();
        var metadataExtractor = new PatternMetadataExtractor();
        var workflowOrchestrator = new PublicationWorkflowOrchestrator();
        var schemaValidator = new SchemaValidator();
        var hqoCalculator = new HQOScorecardCalculator();
        var diagramValidator = new MermaidDiagramValidator();
        var reportGenerator = new ValidationReportGenerator();
        var reviewerQueueManager = new ReviewerQueueManager();
        var notificationService = new NotificationService();

        var orchestrator = new PatternPublicationOrchestrator(
            publicationService,
            metadataExtractor,
            workflowOrchestrator,
            schemaValidator,
            hqoCalculator,
            diagramValidator,
            reportGenerator,
            reviewerQueueManager,
            notificationService
        );

        // Create a pattern with validation issues
        var pattern = CreatePatternWithValidationIssues();

        var result = await orchestrator.PublishPatternAsync(
            pattern,
            "author-456",
            "author-with-issues@example.com"
        );

        Console.WriteLine($"\nStatus: {result.Status}");
        Console.WriteLine("\nValidation failed. Author will receive detailed feedback on how to fix the issues.");
    }

    private static Pattern CreateSamplePattern()
    {
        return new Pattern
        {
            Id = "emergency-escalation-workflow",
            Slug = "emergency-escalation-workflow",
            Title = "Emergency Escalation Workflow",
            Problem = "Critical incidents get lost in multiple communication channels with unclear ownership",
            OrchestrationShift = "Centralize emergency routing with explicit on-call assignment and automated escalation",
            Hook = "When a system crashes at 3 AM, the last thing you need is confusion about who to call.",
            ProblemDetail = @"Organizations struggle with emergency escalation when critical issues arise outside business hours.
                Incidents get reported through multiple channels (email, Slack, phone), and responders are unclear about
                who has primary ownership. This leads to delayed response times and potentially catastrophic outcomes.",
            AsIsDiagram = @"sequenceDiagram
                participant User as 👤 End User
                participant Support as 📞 Support Hotline
                participant Email as 📧 Email System
                participant Slack as 💬 Slack
                participant Engineer as 👨‍💻 Engineer

                User->>Support: Reports critical issue
                Support->>Email: Forwards to distribution list
                Support->>Slack: Posts in #incidents
                Note over Engineer: Checks email hours later
                Note over Engineer: Misses Slack notification
                User->>Support: Calls again (still broken)",
            OrchestratedDiagram = @"sequenceDiagram
                participant User as 👤 End User
                participant PagerDuty as 🚨 PagerDuty
                participant OnCall as 👨‍💻 On-Call Engineer
                participant Backup as 👩‍💻 Backup Engineer
                participant Incident as 📋 Incident System

                User->>PagerDuty: Reports critical issue
                PagerDuty->>OnCall: Pages primary on-call
                PagerDuty->>Incident: Creates incident ticket
                OnCall->>User: Acknowledges within 5 min
                alt Primary unavailable
                    PagerDuty->>Backup: Auto-escalates to backup
                end
                OnCall->>Incident: Updates status
                OnCall->>User: Resolves issue",
            DecisionPoint = @"The critical decision is: **who is currently on-call and accountable for this incident?**
                Without clear assignment and automated routing, emergencies become everyone's problem—which means nobody's problem.",
            Metrics = @"**Key Metrics:**
                - Mean Time to Acknowledge (MTTA): <5 minutes
                - Mean Time to Resolve (MTTR): <30 minutes for critical
                - Escalation success rate: >95%
                - False positive rate: <10%",
            Checklist = @"- [ ] Implement PagerDuty or similar on-call management system
                - [ ] Define clear escalation tiers with backup assignments
                - [ ] Configure automated routing based on severity
                - [ ] Set up acknowledgment requirements (5-minute SLA)
                - [ ] Create incident response playbooks
                - [ ] Schedule regular on-call rotation reviews
                - [ ] Monitor and alert on escalation metrics",
            Scorecard = new OrchestrationScorecard
            {
                Ownership = 5,
                TimeSLA = 5,
                Capacity = 4,
                Visibility = 5,
                CustomerLoop = 4,
                Escalation = 5,
                Handoffs = 4,
                Documentation = 4
            },
            ClosingInsight = @"Emergency escalation isn't about having heroes available 24/7—it's about having a system
                that ensures the right person is notified, acknowledges, and responds within defined SLAs.",
            Industries = new List<string> { "Technology", "Healthcare", "Finance" },
            BrokenSignals = new List<string> { "Ownership", "TimeSLA", "Escalation" },
            MaturityLevel = "Advanced",
            Components = new List<Component>
            {
                new Component
                {
                    Id = "1",
                    Name = "On-Call Management System",
                    Description = "PagerDuty or similar tool for on-call scheduling and routing"
                },
                new Component
                {
                    Id = "2",
                    Name = "Incident Response Playbooks",
                    Description = "Step-by-step procedures for common emergency scenarios"
                },
                new Component
                {
                    Id = "3",
                    Name = "Escalation Policy",
                    Description = "Automated escalation rules based on acknowledgment timeouts"
                }
            }
        };
    }

    private static Pattern CreatePatternWithValidationIssues()
    {
        return new Pattern
        {
            Id = "invalid-pattern",
            Title = "Pattern with Issues",
            Hook = "This pattern has validation issues",
            // Missing required fields: ProblemDetail, AsIsDiagram, OrchestratedDiagram, etc.
            Scorecard = new OrchestrationScorecard
            {
                Ownership = 2, // Below threshold of 3
                TimeSLA = 2,
                Capacity = 2,
                Visibility = 2,
                CustomerLoop = 2,
                Escalation = 2,
                Handoffs = 2,
                Documentation = 2
            },
            Industries = new List<string>(),
            BrokenSignals = new List<string>()
        };
    }

    private static void DisplayValidationErrors(ValidationReport report)
    {
        if (report.SchemaValidation.Errors.Any())
        {
            Console.WriteLine("\nSchema Validation Errors:");
            foreach (var error in report.SchemaValidation.Errors)
            {
                Console.WriteLine($"  [{error.Severity}] {error.Field}: {error.Message}");
            }
        }

        if (report.HQOValidation.Errors.Any())
        {
            Console.WriteLine("\nHQO Scorecard Errors:");
            foreach (var error in report.HQOValidation.Errors)
            {
                Console.WriteLine($"  [{error.Severity}] {error.Field}: {error.Message}");
            }
        }

        if (report.DiagramValidation.Errors.Any())
        {
            Console.WriteLine("\nDiagram Validation Errors:");
            foreach (var error in report.DiagramValidation.Errors)
            {
                Console.WriteLine($"  [{error.Severity}] {error.Field}: {error.Message}");
            }
        }
    }
}
