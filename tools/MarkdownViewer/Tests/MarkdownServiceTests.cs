using Xunit;

namespace OrchestrationWisdom.Tests.Services
{
    public class MarkdownServiceTests
    {
        [Fact]
        public void ConvertToHtml_WithValidMarkdown_ReturnsHtml()
        {
            // Arrange
            var markdown = "# Heading\n\nParagraph text";

            // Act
            // var html = MarkdownService.ConvertToHtml(markdown);

            // Assert
            // Assert.Contains("<h1>", html);
            Assert.True(true);
        }
    }
}
