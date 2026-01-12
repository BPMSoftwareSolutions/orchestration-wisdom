using Xunit;
using OrchestrationWisdom.Models;
using OrchestrationWisdom.Services;

namespace OrchestrationWisdom.Tests.Services;

/// <summary>
/// Tests for HQOScorecardCalculator
/// Movement 2, Beat 5: Calculate HQO Scorecard
/// </summary>
public class HQOScorecardCalculatorTests
{
    [Fact]
    public async Task Calculate_RejectsScorecard_WhenDimensionBelow3()
    {
        // Arrange
        var calculator = new HQOScorecardCalculator();
        var scorecard = new OrchestrationScorecard
        {
            Ownership = 2, // Below threshold
            TimeSLA = 4,
            Capacity = 4,
            Visibility = 4,
            CustomerLoop = 4,
            Escalation = 4,
            Handoffs = 4,
            Documentation = 4
        };

        // Act
        var result = await calculator.CalculateAndValidateAsync(scorecard);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field.Contains("Ownership") && e.Severity == "Critical");
    }

    [Fact]
    public async Task Calculate_RejectsScorecard_WhenTotalBelow30()
    {
        // Arrange
        var calculator = new HQOScorecardCalculator();
        var scorecard = new OrchestrationScorecard
        {
            Ownership = 3,
            TimeSLA = 3,
            Capacity = 3,
            Visibility = 3,
            CustomerLoop = 3,
            Escalation = 3,
            Handoffs = 3,
            Documentation = 3
        }; // Total = 24, below threshold of 30

        // Act
        var result = await calculator.CalculateAndValidateAsync(scorecard);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "TotalScore" && e.Severity == "Critical");
    }

    [Fact]
    public async Task Calculate_PassesScorecard_WhenAllThresholdsMet()
    {
        // Arrange
        var calculator = new HQOScorecardCalculator();
        var scorecard = new OrchestrationScorecard
        {
            Ownership = 5,
            TimeSLA = 4,
            Capacity = 4,
            Visibility = 4,
            CustomerLoop = 3,
            Escalation = 5,
            Handoffs = 4,
            Documentation = 3
        }; // Total = 32, all dimensions ≥3

        // Act
        var result = await calculator.CalculateAndValidateAsync(scorecard);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Calculate_FlagsMarginalScore_WhenTotal30To32()
    {
        // Arrange
        var calculator = new HQOScorecardCalculator();
        var scorecard = new OrchestrationScorecard
        {
            Ownership = 4,
            TimeSLA = 4,
            Capacity = 4,
            Visibility = 4,
            CustomerLoop = 3,
            Escalation = 4,
            Handoffs = 4,
            Documentation = 3
        }; // Total = 30, marginal

        // Act
        var result = await calculator.CalculateAndValidateAsync(scorecard);

        // Assert
        Assert.True(result.IsValid); // Should pass
        Assert.Contains(result.Errors, e => e.Field == "TotalScore" && e.Severity == "Minor");
    }

    [Fact]
    public async Task Calculate_ProvidesImprovementSuggestions()
    {
        // Arrange
        var calculator = new HQOScorecardCalculator();
        var scorecard = new OrchestrationScorecard
        {
            Ownership = 2,
            TimeSLA = 3,
            Capacity = 3,
            Visibility = 3,
            CustomerLoop = 3,
            Escalation = 3,
            Handoffs = 3,
            Documentation = 3
        };

        // Act
        var result = await calculator.CalculateAndValidateAsync(scorecard);

        // Assert
        Assert.All(result.Errors.Where(e => e.Severity == "Critical"),
            e => Assert.NotNull(e.RemediationGuidance));
    }

    [Fact]
    public async Task Calculate_RejectsInvalidScoreRange()
    {
        // Arrange
        var calculator = new HQOScorecardCalculator();
        var scorecard = new OrchestrationScorecard
        {
            Ownership = 6, // Out of range
            TimeSLA = 4,
            Capacity = 4,
            Visibility = 4,
            CustomerLoop = 3,
            Escalation = 4,
            Handoffs = 4,
            Documentation = 3
        };

        // Act
        var result = await calculator.CalculateAndValidateAsync(scorecard);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field.Contains("Ownership") && e.Message.Contains("out of valid range"));
    }
}
