using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Orchestrates the publication workflow and manages tickets
/// Movement 1, Beat 3: Create Publication Ticket
/// </summary>
public interface IPublicationWorkflowOrchestrator
{
    Task<PublicationTicket> CreateTicketAsync(PatternSubmission submission);
    Task<PublicationTicket?> GetTicketAsync(string ticketId);
    Task UpdateTicketStatusAsync(string ticketId, TicketStatus status, string? description = null);
}

public class PublicationWorkflowOrchestrator : IPublicationWorkflowOrchestrator
{
    private readonly List<PublicationTicket> _tickets = new();
    private readonly Dictionary<string, int> _dailyCounters = new();

    /// <summary>
    /// Creates a publication ticket and routes to validation queue
    /// Event: publication.ticket.created
    /// </summary>
    public Task<PublicationTicket> CreateTicketAsync(PatternSubmission submission)
    {
        var ticket = new PublicationTicket
        {
            TicketId = GenerateTicketId(),
            SubmissionId = submission.Id,
            CreatedAt = DateTime.UtcNow,
            Status = TicketStatus.Created,
            Priority = CalculatePriority(submission)
        };

        ticket.Events.Add(new TicketEvent
        {
            Timestamp = DateTime.UtcNow,
            EventType = "ticket_created",
            Description = "Publication ticket created and added to validation queue"
        });

        _tickets.Add(ticket);

        // In production, send acknowledgment email
        SendAcknowledgmentEmail(submission, ticket);

        return Task.FromResult(ticket);
    }

    public Task<PublicationTicket?> GetTicketAsync(string ticketId)
    {
        var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
        return Task.FromResult(ticket);
    }

    public Task UpdateTicketStatusAsync(string ticketId, TicketStatus status, string? description = null)
    {
        var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
        if (ticket == null)
            throw new ArgumentException($"Ticket not found: {ticketId}");

        ticket.Status = status;
        ticket.Events.Add(new TicketEvent
        {
            Timestamp = DateTime.UtcNow,
            EventType = $"status_changed_to_{status}",
            Description = description ?? $"Status changed to {status}"
        });

        return Task.CompletedTask;
    }

    private string GenerateTicketId()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (!_dailyCounters.ContainsKey(today))
        {
            _dailyCounters[today] = 0;
        }

        _dailyCounters[today]++;
        var year = DateTime.UtcNow.Year;
        var sequence = _dailyCounters[today];

        return $"PUB-{year}-{sequence:D3}";
    }

    private int CalculatePriority(PatternSubmission submission)
    {
        // Priority calculation based on pattern topic demand
        // Higher priority for patterns in high-demand industries
        var pattern = submission.Pattern;

        if (pattern.Industries.Contains("Technology") ||
            pattern.Industries.Contains("Healthcare"))
        {
            return 1; // High priority
        }

        if (pattern.Industries.Contains("Finance") ||
            pattern.Industries.Contains("Professional Services"))
        {
            return 2; // Medium priority
        }

        return 3; // Standard priority
    }

    private void SendAcknowledgmentEmail(PatternSubmission submission, PublicationTicket ticket)
    {
        // In production, integrate with email service
        var estimatedReviewTime = "3-5 business days";

        Console.WriteLine($"""
            [EMAIL] To: {submission.AuthorEmail}
            Subject: Pattern Submission Received - {ticket.TicketId}

            Dear Author,

            Thank you for submitting your pattern "{submission.Pattern.Title}".

            Your submission has been received and assigned ticket number: {ticket.TicketId}

            Next Steps:
            1. Automated validation will run within 1 hour
            2. If validation passes, your pattern will be queued for editorial review
            3. Estimated review time: {estimatedReviewTime}

            You will receive email notifications at each stage of the publication process.

            Thank you,
            Orchestration Wisdom Platform
            """);
    }
}
