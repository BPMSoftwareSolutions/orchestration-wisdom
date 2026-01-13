using Xunit;
using OrchestrationWisdom.Tools.MarkdownViewer.Services;

namespace OrchestrationWisdom.Tools.MarkdownViewer.Tests
{
    public class HQOScorecardCalculatorTests
    {
        private readonly HQOScorecardCalculator _calculator = new HQOScorecardCalculator();

        [Fact]
        public void CalculateScorecard_ValidDimensions_ReturnsCorrectTotal()
        {
            // Arrange
            var dimensions = new HQODimensions
            {
                Ownership = 5,
                TimeSLA = 4,
                Capacity = 4,
                Visibility = 4,
                CustomerLoop = 3,
                Escalation = 5,
                Handoffs = 4,
                Documentation = 3
            };

            // Act
            var scorecard = _calculator.CalculateScorecard(dimensions);

            // Assert
            Assert.Equal(35, scorecard.Total);
            Assert.True(scorecard.MeetsThreshold);
        }

        [Fact]
        public void CalculateScorecard_BelowMinimumTotal_NotValid()
        {
            // Arrange
            var dimensions = new HQODimensions
            {
                Ownership = 3,
                TimeSLA = 3,
                Capacity = 3,
                Visibility = 3,
                CustomerLoop = 3,
                Escalation = 3,
                Handoffs = 3,
                Documentation = 1  // Below minimum
            };

            // Act
            var scorecard = _calculator.CalculateScorecard(dimensions);

            // Assert
            Assert.Equal(26, scorecard.Total);
            Assert.False(scorecard.MeetsThreshold);
        }

        [Fact]
        public void ValidateScorecard_DimensionBelowMinimum_ReturnsError()
        {
            // Arrange
            var scorecard = new HQOScorecard
            {
                Dimensions = new HQODimensions
                {
                    Ownership = 5,
                    TimeSLA = 4,
                    Capacity = 4,
                    Visibility = 4,
                    CustomerLoop = 2,  // Below minimum of 3
                    Escalation = 5,
                    Handoffs = 4,
                    Documentation = 3
                },
                Total = 31
            };

            // Act
            var (isValid, issues) = _calculator.ValidateScorecard(scorecard);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(issues);
        }

        [Fact]
        public void ValidateScorecard_MeetsThreshold_ReturnsValid()
        {
            // Arrange
            var scorecard = new HQOScorecard
            {
                Dimensions = new HQODimensions
                {
                    Ownership = 5,
                    TimeSLA = 4,
                    Capacity = 4,
                    Visibility = 4,
                    CustomerLoop = 3,
                    Escalation = 5,
                    Handoffs = 4,
                    Documentation = 3
                },
                Total = 35
            };

            // Act
            var (isValid, issues) = _calculator.ValidateScorecard(scorecard);

            // Assert
            Assert.True(isValid);
            Assert.Empty(issues);
        }
    }
}
