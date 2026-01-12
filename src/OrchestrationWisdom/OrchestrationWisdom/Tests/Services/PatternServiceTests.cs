using Xunit;
using OrchestrationWisdom.Services;
using System.Threading.Tasks;

namespace OrchestrationWisdom.Tests.Services
{
    public class PatternServiceTests
    {
        private readonly IPatternService _patternService;

        public PatternServiceTests()
        {
            _patternService = new PatternService();
        }

        [Fact]
        public async Task GetAllPatternsAsync_ReturnsPatterns()
        {
            // Act
            var patterns = await _patternService.GetAllPatternsAsync();

            // Assert
            Assert.NotNull(patterns);
        }

        [Fact]
        public async Task GetPatternBySlugAsync_ReturnsSinglePattern()
        {
            // Act
            var pattern = await _patternService.GetPatternBySlugAsync("test-pattern");

            // Assert
            Assert.NotNull(pattern);
        }
    }
}
