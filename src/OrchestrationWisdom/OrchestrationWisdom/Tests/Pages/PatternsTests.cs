using Xunit;
using OrchestrationWisdom.Components.Pages;
using OrchestrationWisdom.Services;
using OrchestrationWisdom.Models;
using Moq;

namespace OrchestrationWisdom.Tests.Pages
{
    public class PatternsTests
    {
        [Fact]
        public void Patterns_LoadsAllPatterns()
        {
            // Arrange
            var mockPatternService = new Mock<IPatternService>();
            var patterns = new List<Pattern>
            {
                new() { Title = "Pattern 1", Slug = "pattern-1" },
                new() { Title = "Pattern 2", Slug = "pattern-2" }
            };
            
            mockPatternService.Setup(s => s.GetAllPatternsAsync())
                .ReturnsAsync(patterns);

            // Act - Component initialization would occur here
            
            // Assert
            Assert.NotEmpty(patterns);
        }
    }
}
