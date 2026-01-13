using Xunit;
using OrchestrationWisdom.Tools.MarkdownViewer.Services;
using System.Collections.Generic;

namespace OrchestrationWisdom.Tools.MarkdownViewer.Tests
{
    public class MermaidDiagramValidatorTests
    {
        private readonly MermaidDiagramValidator _validator = new MermaidDiagramValidator();

        [Fact]
        public void ValidateDiagram_WithinBudget_ReturnsValid()
        {
            // Arrange
            var diagram = @"sequenceDiagram
                participant C as Customer
                participant L1 as L1
                participant L2 as L2
                C->>L1: Request
                L1->>L2: Escalate
                L2->>C: Response";

            // Act
            var result = _validator.ValidateDiagram(diagram);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateDiagram_TooManyParticipants_ReturnError()
        {
            // Arrange
            var diagram = @"sequenceDiagram
                participant A as A
                participant B as B
                participant C as C
                participant D as D
                participant E as E
                participant F as F
                participant G as G
                participant H as H
                A->>B: Test";

            // Act
            var result = _validator.ValidateDiagram(diagram);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void AnalyzeDiagram_CountsParticipants()
        {
            // Arrange
            var diagram = @"sequenceDiagram
                participant Creator
                participant AI
                participant Desktop
                Creator->>AI: Generate";

            // Act
            var metrics = _validator.AnalyzeDiagram(diagram);

            // Assert
            Assert.Equal(3, metrics.ParticipantCount);
        }

        [Fact]
        public void AnalyzeDiagram_CountsSteps()
        {
            // Arrange
            var diagram = @"sequenceDiagram
                participant A
                participant B
                A->>B: Step 1
                B->>A: Step 2
                A->>B: Step 3";

            // Act
            var metrics = _validator.AnalyzeDiagram(diagram);

            // Assert
            Assert.Equal(3, metrics.StepCount);
        }
    }
}
