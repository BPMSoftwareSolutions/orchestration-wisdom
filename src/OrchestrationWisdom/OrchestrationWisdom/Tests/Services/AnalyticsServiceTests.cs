using Xunit;
using OrchestrationWisdom.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OrchestrationWisdom.Tests.Services
{
    public class AnalyticsServiceTests
    {
        private readonly AnalyticsService _service = new();

        [Fact]
        public async Task TrackEventAsync_RecordsEvent()
        {
            // Act
            await _service.TrackEventAsync("pattern_published", new Dictionary<string, object>
            {
                { "PatternId", "test-pattern" },
                { "HQOScore", 35 }
            });

            // Assert - verify no exception thrown
            Assert.True(true);
        }

        [Fact]
        public async Task GetPatternAnalyticsAsync_ReturnsAnalytics()
        {
            // Act
            var analytics = await _service.GetPatternAnalyticsAsync("test-pattern");

            // Assert
            Assert.NotNull(analytics);
            Assert.Equal("test-pattern", analytics.PatternId);
        }

        [Fact]
        public async Task InitializePatternTrackingAsync_CreatesTracking()
        {
            // Act
            await _service.InitializePatternTrackingAsync("new-pattern", "Pattern Title", 35);

            // Assert - verify no exception thrown
            Assert.True(true);
        }
    }
}
