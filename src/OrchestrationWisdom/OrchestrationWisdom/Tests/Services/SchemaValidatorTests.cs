using Xunit;
using OrchestrationWisdom.Models;
using OrchestrationWisdom.Services;

namespace OrchestrationWisdom.Tests.Services;

/// <summary>
/// Tests for SchemaValidator
/// Movement 2, Beat 4: Validate Against Schema
/// </summary>
public class SchemaValidatorTests
{
    [Fact]
    public async Task Validate_RejectsPattern_WhenRequiredFieldsMissing()
    {
        // Arrange
        var validator = new SchemaValidator();
        var pattern = new Pattern
        {
            Id = "test-1",
            Title = "Test Pattern"
            // Missing other required fields
        };

        // Act
        var result = await validator.ValidateAsync(pattern);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Field == "Hook");
        Assert.Contains(result.Errors, e => e.Field == "AsIsDiagram");
        Assert.Contains(result.Errors, e => e.Field == "OrchestratedDiagram");
    }

    [Fact]
    public async Task Validate_PassesPattern_WhenAllFieldsProvided()
    {
        // Arrange
        var validator = new SchemaValidator();
        var pattern = CreateValidPattern();

        // Act
        var result = await validator.ValidateAsync(pattern);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_RejectsPattern_WhenScorecardMissing()
    {
        // Arrange
        var validator = new SchemaValidator();
        var pattern = CreateValidPattern();
        pattern.Scorecard = new OrchestrationScorecard(); // All zeros

        // Act
        var result = await validator.ValidateAsync(pattern);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "Scorecard");
    }

    [Fact]
    public async Task Validate_RejectsPattern_WhenIndustriesMissing()
    {
        // Arrange
        var validator = new SchemaValidator();
        var pattern = CreateValidPattern();
        pattern.Industries = new List<string>();

        // Act
        var result = await validator.ValidateAsync(pattern);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "Industries");
    }

    [Fact]
    public async Task Validate_RejectsPattern_WhenBrokenSignalsMissing()
    {
        // Arrange
        var validator = new SchemaValidator();
        var pattern = CreateValidPattern();
        pattern.BrokenSignals = new List<string>();

        // Act
        var result = await validator.ValidateAsync(pattern);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "BrokenSignals");
    }

    [Fact]
    public async Task Validate_IncludesRemediationGuidance_ForErrors()
    {
        // Arrange
        var validator = new SchemaValidator();
        var pattern = new Pattern { Id = "test-1" };

        // Act
        var result = await validator.ValidateAsync(pattern);

        // Assert
        Assert.All(result.Errors, e => Assert.NotNull(e.RemediationGuidance));
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
