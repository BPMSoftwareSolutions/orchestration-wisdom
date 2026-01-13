using Xunit;
using OrchestrationWisdom.Tools.MarkdownViewer.Services;
using System.Threading.Tasks;

namespace OrchestrationWisdom.Tools.MarkdownViewer.Tests
{
    public class PatternJsonExporterTests
    {
        private readonly PatternJsonExporter _exporter = new PatternJsonExporter();

        [Fact]
        public async Task ExportAsync_WithValidMarkdown_ReturnsPatternJson()
        {
            // Arrange
            var markdown = @"
## Hook
Escalated tickets somehow become invisible

## Problem Detail
The problem is real and widespread

## As-Is
```mermaid
sequenceDiagram
  participant C as Customer
  participant L1 as L1
  C->>L1: Request
```

## Orchestrated
```mermaid
sequenceDiagram
  participant C as Customer
  participant R as Router
  C->>R: Request
```

## Decision Point
Who owns this escalation?

## Metrics
MTTR reduction

## Implementation Checklist
1. First step
2. Second step
3. Third step

## Common Pitfalls
- Pitfall 1
- Pitfall 2

## Closing Insight
It's an orchestration gap
";

            // Act
            var result = await _exporter.ExportAsync(markdown);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("invisible", result.HookMarkdown);
            Assert.NotEmpty(result.AsIsDiagramMermaid);
            Assert.NotEmpty(result.OrchestratedDiagramMermaid);
            Assert.NotEmpty(result.ImplementationChecklist);
            Assert.NotEmpty(result.CommonPitfalls);
        }

        [Fact]
        public async Task ExportAsync_ExtractsMultipleSections()
        {
            // Arrange
            var markdown = @"
## Hook
Simple hook

## Problem Detail
Problem here

## Implementation Checklist
1. Step one
2. Step two
3. Step three
4. Step four
";

            // Act
            var result = await _exporter.ExportAsync(markdown);

            // Assert
            Assert.Equal(4, result.ImplementationChecklist.Count);
            Assert.Contains("Step one", result.ImplementationChecklist);
        }
    }
}
