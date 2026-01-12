using Xunit;
using OrchestrationWisdom.Models;
using OrchestrationWisdom.Services;

namespace OrchestrationWisdom.Tests.Services;

/// <summary>
/// Tests for PatternPublicationService
/// Movement 1, Beat 1: Receive Pattern Submission
/// </summary>
public class PatternPublicationServiceTests
{
    [Fact]
    public async Task ReceiveSubmission_CapturesMetadata_WhenValidPayloadProvided()
    {
        // Arrange
        var service = new PatternPublicationService();
        var pattern = CreateValidPattern();
        var authorId = "author-123";
        var authorEmail = "author@example.com";

        // Act
        var submission = await service.ReceiveSubmissionAsync(pattern, authorId, authorEmail);

        // Assert
        Assert.NotNull(submission);
        Assert.NotEmpty(submission.Id);
        Assert.Equal(pattern.Id, submission.PatternId);
        Assert.Equal(authorId, submission.AuthorId);
        Assert.Equal(authorEmail, submission.AuthorEmail);
        Assert.Equal(PublicationStatus.SubmissionReceived, submission.Status);
        Assert.True(submission.SubmittedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task ReceiveSubmission_ThrowsException_WhenPatternIdMissing()
    {
        // Arrange
        var service = new PatternPublicationService();
        var pattern = CreateValidPattern();
        pattern.Id = string.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ReceiveSubmissionAsync(pattern, "author-123", "author@example.com"));
    }

    [Fact]
    public async Task ReceiveSubmission_ThrowsException_WhenRequiredFieldsMissing()
    {
        // Arrange
        var service = new PatternPublicationService();
        var pattern = CreateValidPattern();
        pattern.Hook = string.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ReceiveSubmissionAsync(pattern, "author-123", "author@example.com"));
    }

    [Fact]
    public async Task GetSubmission_ReturnsSubmission_WhenExists()
    {
        // Arrange
        var service = new PatternPublicationService();
        var pattern = CreateValidPattern();
        var submission = await service.ReceiveSubmissionAsync(pattern, "author-123", "author@example.com");

        // Act
        var retrieved = await service.GetSubmissionAsync(submission.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(submission.Id, retrieved.Id);
    }

    [Fact]
    public async Task GetSubmissionsByAuthor_ReturnsAuthorSubmissions()
    {
        // Arrange
        var service = new PatternPublicationService();
        var authorId = "author-123";
        var pattern1 = CreateValidPattern();
        var pattern2 = CreateValidPattern();
        pattern2.Id = "pattern-2";

        await service.ReceiveSubmissionAsync(pattern1, authorId, "author@example.com");
        await service.ReceiveSubmissionAsync(pattern2, authorId, "author@example.com");
        await service.ReceiveSubmissionAsync(CreateValidPattern(), "other-author", "other@example.com");

        // Act
        var submissions = await service.GetSubmissionsByAuthorAsync(authorId);

        // Assert
        Assert.Equal(2, submissions.Count);
        Assert.All(submissions, s => Assert.Equal(authorId, s.AuthorId));
    }

    private Pattern CreateValidPattern()
    {
        return new Pattern
        {
            Id = "pattern-1",
            Title = "Test Pattern",
            Hook = "Test hook",
            ProblemDetail = "Test problem",
            AsIsDiagram = "sequenceDiagram\nparticipant A as Actor",
            OrchestratedDiagram = "sequenceDiagram\nparticipant A as Actor",
            DecisionPoint = "Test decision",
            Metrics = "Test metrics",
            Checklist = "Test checklist",
            ClosingInsight = "Test insight",
            Scorecard = new OrchestrationScorecard
            {
                Ownership = 4,
                TimeSLA = 4,
                Capacity = 4,
                Visibility = 4,
                CustomerLoop = 3,
                Escalation = 4,
                Handoffs = 4,
                Documentation = 3
            },
            Industries = new List<string> { "Technology" },
            BrokenSignals = new List<string> { "Ownership" }
        };
    }
}
