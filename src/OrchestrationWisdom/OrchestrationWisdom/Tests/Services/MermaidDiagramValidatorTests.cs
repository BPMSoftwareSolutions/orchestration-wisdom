using Xunit;
using OrchestrationWisdom.Models;
using OrchestrationWisdom.Services;

namespace OrchestrationWisdom.Tests.Services;

/// <summary>
/// Tests for MermaidDiagramValidator
/// Movement 2, Beat 6: Validate Diagram Budgets
/// </summary>
public class MermaidDiagramValidatorTests
{
    [Fact]
    public async Task Validate_RejectsPattern_WhenActorCountExceeds7()
    {
        // Arrange
        var validator = new MermaidDiagramValidator();
        var asIsDiagram = @"
            sequenceDiagram
            participant A as Actor1
            participant B as Actor2
            participant C as Actor3
            participant D as Actor4
            participant E as Actor5
            participant F as Actor6
            participant G as Actor7
            participant H as Actor8
            A->>B: Message
        ";

        // Act
        var result = await validator.ValidateDiagramsAsync(asIsDiagram, "sequenceDiagram\nparticipant A as Actor");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("exceeds actor budget"));
    }

    [Fact]
    public async Task Validate_RejectsPattern_WhenNestedAltBlocks()
    {
        // Arrange
        var validator = new MermaidDiagramValidator();
        var diagram = @"
            sequenceDiagram
            participant A as Actor
            participant B as Actor2
            alt Condition 1
                A->>B: Message
                alt Condition 2
                    B->>A: Nested
                end
            end
        ";

        // Act
        var result = await validator.ValidateDiagramsAsync(diagram, "sequenceDiagram\nparticipant A as Actor");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("nested alt blocks"));
    }

    [Fact]
    public async Task Validate_PassesDiagram_WhenWithinBudget()
    {
        // Arrange
        var validator = new MermaidDiagramValidator();
        var diagram = @"
            sequenceDiagram
            participant A as Actor1
            participant B as Actor2
            participant C as Actor3
            A->>B: Message 1
            B->>C: Message 2
            C->>A: Message 3
        ";

        // Act
        var result = await validator.ValidateDiagramsAsync(diagram, diagram);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_RejectsDiagram_WhenStepsExceed18()
    {
        // Arrange
        var validator = new MermaidDiagramValidator();
        var diagram = @"
            sequenceDiagram
            participant A as Actor1
            participant B as Actor2
            A->>B: Step 1
            B->>A: Step 2
            A->>B: Step 3
            B->>A: Step 4
            A->>B: Step 5
            B->>A: Step 6
            A->>B: Step 7
            B->>A: Step 8
            A->>B: Step 9
            B->>A: Step 10
            A->>B: Step 11
            B->>A: Step 12
            A->>B: Step 13
            B->>A: Step 14
            A->>B: Step 15
            B->>A: Step 16
            A->>B: Step 17
            B->>A: Step 18
            A->>B: Step 19
        ";

        // Act
        var result = await validator.ValidateDiagramsAsync(diagram, "sequenceDiagram\nparticipant A as Actor");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("exceeds step budget"));
    }

    [Fact]
    public async Task Validate_RejectsDiagram_WhenAltBlocksExceed2()
    {
        // Arrange
        var validator = new MermaidDiagramValidator();
        var diagram = @"
            sequenceDiagram
            participant A as Actor
            participant B as Actor2
            alt Condition 1
                A->>B: Message
            end
            alt Condition 2
                B->>A: Message
            end
            alt Condition 3
                A->>B: Message
            end
        ";

        // Act
        var result = await validator.ValidateDiagramsAsync(diagram, "sequenceDiagram\nparticipant A as Actor");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("exceeds alt block budget"));
    }

    [Fact]
    public async Task Validate_ValidatesBothDiagrams()
    {
        // Arrange
        var validator = new MermaidDiagramValidator();
        var validDiagram = @"
            sequenceDiagram
            participant A as Actor1
            participant B as Actor2
            A->>B: Message
        ";
        var invalidDiagram = @"
            sequenceDiagram
            participant A as Actor1
            participant B as Actor2
            participant C as Actor3
            participant D as Actor4
            participant E as Actor5
            participant F as Actor6
            participant G as Actor7
            participant H as Actor8
            A->>B: Message
        ";

        // Act
        var result = await validator.ValidateDiagramsAsync(validDiagram, invalidDiagram);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == "OrchestratedDiagram");
    }
}
