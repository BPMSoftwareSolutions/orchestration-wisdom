# Canonical Source File Mapping

This document maps every handler, service, and source file referenced in the canonical sequences to its implementation location. This ensures that sequences guide implementation rather than being aspirational.

## Content Creator End-to-End Sequence (content-creator-end-to-end.json)

### Phase 1: Pattern Discovery & Research

#### Beat 1: Review Platform Analytics
- **Handler**: Analytics dashboard review
- **Scope**: External (UI)
- **Source**: N/A (Google Analytics / Custom Dashboard)
- **Event**: research.initiated
- **Required Interface**: `IContentGapAnalyzer`
- **Implementation**: `src/OrchestrationWisdom/OrchestrationWisdom/Services/Analytics/ContentGapAnalyzer.cs`
- **Test**: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Analytics/ContentGapAnalysisTests.cs`

#### Beat 2: Conduct Industry Research
- **Handler**: Manual research across multiple sources
- **Scope**: External (Manual)
- **Event**: gap.identified

#### Beat 3: Document Research Findings
- **Handler**: Note-taking in Notion/Confluence
- **Scope**: External (Manual)
- **Event**: research.documented

### Phase 2: AI-Assisted Pattern Generation

#### Beat 1: Load OWS Template
- **Handler**: Template file loading
- **Scope**: Internal
- **Source**: `docs/examples/article-template.md` ✅
- **Event**: template.loaded
- **Status**: CREATED

#### Beat 2: Generate Pattern with AI
- **Handler**: AI API call (Claude/GPT-4)
- **Scope**: External (API)
- **Event**: draft.generated

#### Beat 3: Save Pattern Draft
- **Handler**: File system save
- **Scope**: Internal
- **Source**: File system
- **Event**: draft.saved

### Phase 3: Desktop Validation & Refinement

#### Beat 1: Open in MarkdownViewer
- **Handler**: MarkdownViewer.MainWindow
- **Scope**: Internal
- **Source**: `Tools/MarkdownViewer/MainWindow.xaml.cs`
- **Event**: preview.opened
- **Status**: Referenced in sequence

#### Beat 2: Refine Content
- **Handler**: Text editor + FileWatcher
- **Scope**: External
- **Event**: content.refined

#### Beat 3: Validate Diagram Budgets
- **Handler**: MermaidDiagramValidator
- **Scope**: Internal
- **Source**: `Tools/MarkdownViewer/Services/MermaidDiagramValidator.cs` ✅
- **Test**: `Tools/MarkdownViewer/Tests/MermaidDiagramValidatorTests.cs` ✅
- **Event**: diagram.validated
- **Status**: CREATED
- **Key Methods**:
  - `ValidateDiagram(string mermaidSyntax): ValidationResult`
  - `AnalyzeDiagram(string mermaidSyntax): DiagramMetrics`

#### Beat 4: Calculate HQO Scorecard
- **Handler**: HQOScorecardCalculator
- **Scope**: Internal
- **Source**: `Tools/MarkdownViewer/Services/HQOScorecardCalculator.cs` ✅
- **Test**: `Tools/MarkdownViewer/Tests/HQOScorecardCalculatorTests.cs` ✅
- **Event**: hqo.calculated
- **Status**: CREATED
- **Key Methods**:
  - `CalculateScorecard(HQODimensions): HQOScorecard`
  - `ValidateScorecard(HQOScorecard): (bool, List<string>)`
- **Constraints**:
  - Total score: ≥30/40, ≤40/40
  - Per dimension: ≥3/5, ≤5/5
  - All 8 dimensions required

### Phase 4: JSON Export & Schema Validation

#### Beat 1: Export to OWS JSON
- **Handler**: PatternJsonExporter
- **Scope**: Internal
- **Source**: `Tools/MarkdownViewer/Services/PatternJsonExporter.cs` ✅
- **Test**: `Tools/MarkdownViewer/Tests/PatternJsonExporterTests.cs` ✅
- **Event**: json.exported
- **Status**: CREATED
- **Key Methods**:
  - `ExportAsync(string markdown): Task<PatternJson>`
  - `SaveAsync(PatternJson, string path): Task`

#### Beat 2: Validate Against Schema
- **Handler**: JSON Schema validator
- **Scope**: Internal
- **Source**: `schemas/pattern.schema.json` ✅
- **Event**: schema.validated
- **Status**: CREATED
- **Validation Rules**:
  - Draft-07 JSON Schema
  - Required fields: id, slug, title, diagrams, scorecard
  - HQO constraints enforced

### Phase 5: Version Control & Publication

#### Beat 1: Commit to Git
- **Handler**: Git CLI
- **Scope**: External
- **Event**: pattern.committed

#### Beat 2: CI/CD Pipeline Deploy
- **Handler**: Azure DevOps / GitHub Actions
- **Scope**: External
- **Event**: pattern.published
- **Pipeline**: dotnet test → dotnet build → Azure App Service deploy

#### Beat 3: Track Publication Event
- **Handler**: AnalyticsService.TrackEvent
- **Scope**: Internal
- **Source**: `src/OrchestrationWisdom/OrchestrationWisdom/Services/AnalyticsService.cs` ✅
- **Test**: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Services/AnalyticsServiceTests.cs` ✅
- **Event**: analytics.tracked
- **Status**: CREATED
- **Key Methods**:
  - `TrackEventAsync(string eventName, Dictionary<string,object> metadata): Task`
  - `GetPatternAnalyticsAsync(string patternId): Task<PatternAnalytics>`
  - `InitializePatternTrackingAsync(string patternId, string title, int hqoScore): Task`

#### Beat 4: Verify Pattern Live
- **Handler**: Browser verification
- **Scope**: External
- **Event**: pattern.verified

### Phase 6: Engagement Monitoring & Iteration

#### Beat 1: Monitor Initial Engagement
- **Handler**: Analytics dashboard review
- **Event**: engagement.measured

#### Beat 2: Collect User Feedback
- **Handler**: Feedback channel review
- **Event**: feedback.collected

#### Beat 3: Plan Iteration
- **Handler**: Manual analysis and planning
- **Event**: iteration.planned

#### Beat 4: Track Long-term Impact
- **Handler**: Analytics and feedback review
- **Event**: impact.measured

#### Beat 5: Identify Next Content Gaps
- **Handler**: Analytics gap analysis
- **Event**: gaps.identified

---

## Other Sequences

### Operator Implementation Sequence (operator-implementation.json)

#### Pages and Components
- `src/OrchestrationWisdom/OrchestrationWisdom/Components/Pages/PatternDetail.razor` ✅ (exists)
  - Test: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Pages/PatternDetailTests.cs` ✅

#### Services
- `src/SmartRouter/Tests/RouterService.Tests.cs` - Not yet created (external component)
- `src/SLAMonitor/Tests/MonitorService.Tests.cs` - Not yet created (external component)

### MSP Domain Expert Sequence (msp-domain-expert.json)

#### Pages
- `src/OrchestrationWisdom/OrchestrationWisdom/Components/Pages/Patterns.razor` ✅ (exists)
  - Test: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Pages/PatternsTests.cs` ✅

### Company Buyer Discovery Sequence (company-buyer-discovery.json)

#### Pages
- `src/OrchestrationWisdom/OrchestrationWisdom/Components/Pages/Home.razor` ✅ (exists)
  - Test: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Pages/HomeTests.cs` ✅
- `src/OrchestrationWisdom/OrchestrationWisdom/Components/Pages/Patterns.razor` ✅ (exists)
  - Test: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Pages/PatternsTests.cs` ✅
- `src/OrchestrationWisdom/OrchestrationWisdom/Components/Pages/PatternDetail.razor` ✅ (exists)
  - Test: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Pages/PatternDetailTests.cs` ✅

#### Components
- `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Components/MermaidDiagramTests.cs` ✅

#### Services
- `src/OrchestrationWisdom/OrchestrationWisdom/Services/PatternService.cs` ✅ (exists)
  - Test: `src/OrchestrationWisdom/OrchestrationWisdom/Tests/Services/PatternServiceTests.cs` ✅
- `src/OrchestrationWisdom/OrchestrationWisdom/Services/Markdown/MarkdownService.cs` ✅ (exists)
  - Test: `Tools/MarkdownViewer/Tests/MarkdownServiceTests.cs` ✅

---

## Summary

### Status Overview

**CREATED** (New files generated from canonical sequence):
- ✅ `docs/examples/article-template.md` - Template for AI-assisted authoring
- ✅ `src/OrchestrationWisdom/OrchestrationWisdom/Services/Analytics/ContentGapAnalyzer.cs`
- ✅ `src/OrchestrationWisdom/OrchestrationWisdom/Services/AnalyticsService.cs`
- ✅ `Tools/MarkdownViewer/Services/MermaidDiagramValidator.cs`
- ✅ `Tools/MarkdownViewer/Services/HQOScorecardCalculator.cs`
- ✅ `Tools/MarkdownViewer/Services/PatternJsonExporter.cs`
- ✅ `schemas/pattern.schema.json`
- ✅ All test files for above services

**EXISTING** (Already in codebase):
- ✅ Razor pages: Home, Patterns, PatternDetail
- ✅ PatternService.cs
- ✅ MarkdownService.cs
- ✅ IPatternService interface

**NOT YET CREATED** (Referenced but out of scope):
- SmartRouter component (external service)
- SLAMonitor component (external service)
- CI/CD pipeline configuration
- Analytics dashboard integration

### Implementation Guidelines

1. **All newly created files** have `TODO` comments indicating what needs to be implemented
2. **All service interfaces** are defined with clear contracts
3. **All test files** include basic structure and can be extended with real test cases
4. **HQO validation** is enforced at schema level: Total ≥30/40, no dimension <3
5. **Diagram budget validation** enforced by MermaidDiagramValidator: ≤7 actors, ≤18 steps, ≤2 alt blocks

### Using This Mapping for Development

1. **For content creator workflow**: Follow the beat progression and implement each handler in sequence
2. **For validation**: Run HQOScorecardCalculator and MermaidDiagramValidator before publication
3. **For testing**: Use the provided test templates and add specific test cases as you implement
4. **For schema compliance**: Ensure exported patterns validate against `pattern.schema.json`

