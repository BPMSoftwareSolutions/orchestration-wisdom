using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Sends notifications for publication events
/// Movement 5, Beat 18: Send Publication Notification
/// </summary>
public interface INotificationService
{
    Task SendPublicationNotificationAsync(PatternSubmission submission);
    Task SendValidationFailureNotificationAsync(PatternSubmission submission, ValidationReport report);
}

public class NotificationService : INotificationService
{
    /// <summary>
    /// Sends publication success notification
    /// Event: pattern.publication.notification.sent
    /// </summary>
    public Task SendPublicationNotificationAsync(PatternSubmission submission)
    {
        var patternUrl = $"/patterns/{submission.Pattern.Slug}";

        Console.WriteLine($"""
            [EMAIL] To: {submission.AuthorEmail}
            Cc: editorial-team@example.com
            Subject: 🎉 Your Pattern is Live - {submission.Pattern.Title}

            Dear Author,

            Congratulations! Your pattern "{submission.Pattern.Title}" has been successfully published to the Orchestration Wisdom platform.

            Pattern Details:
            - Title: {submission.Pattern.Title}
            - Published: {submission.Pattern.PublishedDate:yyyy-MM-dd HH:mm UTC}
            - URL: https://orchestrationwisdom.com{patternUrl}
            - Industries: {string.Join(", ", submission.Pattern.Industries)}
            - HQO Score: {submission.Pattern.Scorecard.TotalScore}/40

            Initial Metrics:
            - Pattern is indexed and searchable
            - Appears in {submission.Pattern.Industries.Count} industry categories
            - Related patterns have been linked
            - Analytics tracking is active

            What's Next:
            - We'll send you daily engagement reports for the first week
            - User feedback will be forwarded to you automatically
            - You can update your pattern anytime in the author dashboard

            Track your pattern's performance:
            https://orchestrationwisdom.com/author/dashboard

            Thank you for contributing to the Orchestration Wisdom community!

            The Orchestration Wisdom Team
            """);

        // Also send to Slack/Teams if configured
        SendSlackNotification(submission);

        // Log to publication changelog
        LogPublicationToChangelog(submission);

        return Task.CompletedTask;
    }

    public Task SendValidationFailureNotificationAsync(PatternSubmission submission, ValidationReport report)
    {
        var markdownReport = new ValidationReportGenerator().GenerateMarkdownReport(report);

        Console.WriteLine($"""
            [EMAIL] To: {submission.AuthorEmail}
            Subject: Pattern Validation Results - {submission.Pattern.Title}

            Dear Author,

            Your pattern "{submission.Pattern.Title}" (Submission ID: {submission.Id}) has completed validation.

            {markdownReport}

            If you have questions about the validation results, please reply to this email.

            Best regards,
            Orchestration Wisdom Platform
            """);

        return Task.CompletedTask;
    }

    private void SendSlackNotification(PatternSubmission submission)
    {
        Console.WriteLine($"""
            [SLACK] #pattern-publications channel
            🎉 New Pattern Published!

            *{submission.Pattern.Title}*
            By: {submission.AuthorEmail}
            Industries: {string.Join(", ", submission.Pattern.Industries)}
            HQO Score: {submission.Pattern.Scorecard.TotalScore}/40

            <https://orchestrationwisdom.com/patterns/{submission.Pattern.Slug}|View Pattern>
            """);
    }

    private void LogPublicationToChangelog(PatternSubmission submission)
    {
        Console.WriteLine($"""
            [CHANGELOG] Pattern Publication Logged
            Date: {DateTime.UtcNow:yyyy-MM-dd}
            Pattern: {submission.Pattern.Title}
            Submission ID: {submission.Id}
            Ticket ID: {submission.Ticket?.TicketId}
            Author: {submission.AuthorEmail}
            """);
    }
}
