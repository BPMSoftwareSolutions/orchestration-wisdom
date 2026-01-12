namespace OrchestrationWisdom.Models;

/// <summary>
/// Represents a pattern submission for publication
/// </summary>
public class PatternSubmission
{
    public string Id { get; set; } = string.Empty;
    public string PatternId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public PublicationStatus Status { get; set; }
    public Pattern Pattern { get; set; } = new();
    public PatternMetadata? Metadata { get; set; }
    public PublicationTicket? Ticket { get; set; }
}

/// <summary>
/// Pattern metadata extracted during submission
/// </summary>
public class PatternMetadata
{
    public int ActorCountAsIs { get; set; }
    public int ActorCountOrchestrated { get; set; }
    public int StepCountAsIs { get; set; }
    public int StepCountOrchestrated { get; set; }
    public int AltBlocksAsIs { get; set; }
    public int AltBlocksOrchestrated { get; set; }
    public int TotalWordCount { get; set; }
    public string ComplexityLevel { get; set; } = string.Empty;
    public DateTime ExtractedAt { get; set; }
}

/// <summary>
/// Publication workflow ticket
/// </summary>
public class PublicationTicket
{
    public string TicketId { get; set; } = string.Empty;
    public string SubmissionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public TicketStatus Status { get; set; }
    public int Priority { get; set; }
    public string? AssignedReviewerId { get; set; }
    public List<TicketEvent> Events { get; set; } = new();
}

/// <summary>
/// Ticket event for audit trail
/// </summary>
public class TicketEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ActorId { get; set; }
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ValidationType { get; set; } = string.Empty;
    public List<ValidationError> Errors { get; set; } = new();
    public DateTime ValidatedAt { get; set; }
}

/// <summary>
/// Validation error detail
/// </summary>
public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? RemediationGuidance { get; set; }
}

/// <summary>
/// Complete validation report
/// </summary>
public class ValidationReport
{
    public string SubmissionId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public bool OverallValid { get; set; }
    public ValidationResult SchemaValidation { get; set; } = new();
    public ValidationResult HQOValidation { get; set; } = new();
    public ValidationResult DiagramValidation { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public List<string> NextSteps { get; set; } = new();
}

/// <summary>
/// Review decision
/// </summary>
public class ReviewDecision
{
    public string SubmissionId { get; set; } = string.Empty;
    public string ReviewerId { get; set; } = string.Empty;
    public DateTime ReviewedAt { get; set; }
    public ReviewStatus Decision { get; set; }
    public List<ReviewFeedback> Feedback { get; set; } = new();
}

/// <summary>
/// Review feedback item
/// </summary>
public class ReviewFeedback
{
    public string Section { get; set; } = string.Empty;
    public FeedbackSeverity Severity { get; set; }
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// Deployment package
/// </summary>
public class DeploymentPackage
{
    public string PackageId { get; set; } = string.Empty;
    public string SubmissionId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime BuiltAt { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public Dictionary<string, string> Artifacts { get; set; } = new();
}

/// <summary>
/// Engagement metrics
/// </summary>
public class EngagementMetrics
{
    public string PatternId { get; set; } = string.Empty;
    public DateTime MeasuredAt { get; set; }
    public int ViewCount { get; set; }
    public int UniqueVisitors { get; set; }
    public int Downloads { get; set; }
    public double BounceRate { get; set; }
    public double AverageTimeSpent { get; set; }
    public int RatingCount { get; set; }
    public double AverageRating { get; set; }
}

/// <summary>
/// User feedback
/// </summary>
public class UserFeedback
{
    public string PatternId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public FeedbackType Type { get; set; }
}

// Enums
public enum PublicationStatus
{
    SubmissionReceived,
    ValidationInProgress,
    ValidationFailed,
    ValidationPassed,
    AwaitingReview,
    InReview,
    ChangesRequested,
    Approved,
    BuildInProgress,
    BuildFailed,
    DeployingToStaging,
    StagingVerificationFailed,
    DeployingToProduction,
    Published,
    PublicationFailed,
    RolledBack
}

public enum TicketStatus
{
    Created,
    Validating,
    AwaitingReview,
    InReview,
    Approved,
    Building,
    Deploying,
    Completed,
    Failed
}

public enum ReviewStatus
{
    Approved,
    Rejected,
    RequestChanges
}

public enum FeedbackSeverity
{
    Critical,
    Major,
    Minor
}

public enum FeedbackType
{
    Rating,
    Comment,
    Issue
}
