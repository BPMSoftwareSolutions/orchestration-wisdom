using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

/// <summary>
/// Service for receiving and processing pattern submissions
/// Movement 1, Beat 1: Receive Pattern Submission
/// </summary>
public interface IPatternPublicationService
{
    Task<PatternSubmission> ReceiveSubmissionAsync(Pattern pattern, string authorId, string authorEmail);
    Task<PatternSubmission?> GetSubmissionAsync(string submissionId);
    Task<List<PatternSubmission>> GetSubmissionsByAuthorAsync(string authorId);
}

public class PatternPublicationService : IPatternPublicationService
{
    private readonly List<PatternSubmission> _submissions = new();

    /// <summary>
    /// Receives a pattern submission and captures metadata
    /// Event: pattern.submission.received
    /// </summary>
    public Task<PatternSubmission> ReceiveSubmissionAsync(Pattern pattern, string authorId, string authorEmail)
    {
        ValidateSubmission(pattern);

        var submission = new PatternSubmission
        {
            Id = GenerateSubmissionId(),
            PatternId = pattern.Id,
            AuthorId = authorId,
            AuthorEmail = authorEmail,
            SubmittedAt = DateTime.UtcNow,
            Status = PublicationStatus.SubmissionReceived,
            Pattern = pattern
        };

        _submissions.Add(submission);

        // Audit log entry
        LogSubmissionReceived(submission);

        return Task.FromResult(submission);
    }

    public Task<PatternSubmission?> GetSubmissionAsync(string submissionId)
    {
        var submission = _submissions.FirstOrDefault(s => s.Id == submissionId);
        return Task.FromResult(submission);
    }

    public Task<List<PatternSubmission>> GetSubmissionsByAuthorAsync(string authorId)
    {
        var submissions = _submissions.Where(s => s.AuthorId == authorId).ToList();
        return Task.FromResult(submissions);
    }

    private void ValidateSubmission(Pattern pattern)
    {
        if (string.IsNullOrEmpty(pattern.Id))
            throw new ArgumentException("Pattern ID is required");

        if (string.IsNullOrEmpty(pattern.Title))
            throw new ArgumentException("Pattern title is required");

        if (string.IsNullOrEmpty(pattern.Hook))
            throw new ArgumentException("Pattern hook is required");

        if (string.IsNullOrEmpty(pattern.AsIsDiagram))
            throw new ArgumentException("As-Is diagram is required");

        if (string.IsNullOrEmpty(pattern.OrchestratedDiagram))
            throw new ArgumentException("Orchestrated diagram is required");
    }

    private string GenerateSubmissionId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var sequence = _submissions.Count + 1;
        return $"SUB-{timestamp}-{sequence:D4}";
    }

    private void LogSubmissionReceived(PatternSubmission submission)
    {
        // In production, this would log to a proper logging system
        Console.WriteLine($"[AUDIT] Submission received: {submission.Id} by {submission.AuthorEmail} at {submission.SubmittedAt:u}");
    }
}
