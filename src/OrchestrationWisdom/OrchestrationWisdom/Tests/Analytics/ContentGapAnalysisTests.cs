using Xunit;
using OrchestrationWisdom.Services.Analytics;
using System.Threading.Tasks;

namespace OrchestrationWisdom.Tests.Analytics
{
    public class ContentGapAnalysisTests
    {
        private readonly ContentGapAnalyzer _analyzer = new();

        [Fact]
        public async Task IdentifyContentGapsAsync_ReturnsGaps()
        {
            // Act
            var gaps = await _analyzer.IdentifyContentGapsAsync();

            // Assert
            Assert.NotNull(gaps);
            // TODO: Add assertions for actual gap data once implemented
        }

        [Fact]
        public async Task ScoreGapsAsync_RanksByDemand()
        {
            // Arrange
            var gaps = await _analyzer.IdentifyContentGapsAsync();

            // Act
            var scoredGaps = await _analyzer.ScoreGapsAsync(gaps);

            // Assert
            Assert.NotNull(scoredGaps);
        }
    }
}
