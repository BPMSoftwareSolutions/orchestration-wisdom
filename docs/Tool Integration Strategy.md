# Tool Integration Strategy
## Integrating WealthBuilder Tools into Orchestration Wisdom

### Overview

You have two powerful authoring and rendering tools that can significantly enhance the Orchestration Wisdom platform:

1. **MarkdownViewer** - A professional WPF application with Markdig + Mermaid support
2. **Generate-SequenceDocumentation.ps1** - PowerShell script for JSON → Markdown/HTML conversion

These tools can be integrated to create a complete content authoring and publishing pipeline for orchestration patterns.

---

## Your Existing Tools

### 1. MarkdownViewer (WPF Application)

**Key Capabilities:**
- Markdig-based markdown → HTML conversion with advanced extensions
- **Native Mermaid support** via custom `MermaidCodeBlockRenderer`
- Dark/Light theme support
- Table of Contents auto-generation
- HTML export functionality
- Template-based rendering system
- Professional SOLID architecture

**Technology Stack:**
- .NET Framework 4.8 / WPF
- Markdig (markdown processing)
- Material Design UI
- WebView2 rendering
- Service-based architecture (IMarkdownService, ITemplateService, IStyleService)

### 2. Generate-SequenceDocumentation.ps1

**Key Capabilities:**
- Converts JSON sequence files → Markdown
- **Built-in Markdown → HTML converter** (custom PowerShell implementation)
- Generates index pages with domain grouping
- Handles Mermaid diagrams in markdown
- Dark/Light theme CSS support
- Auto-generates Table of Contents
- Creates beat-by-beat sequence documentation

**Technology:**
- PowerShell
- Custom regex-based markdown parser
- HTML template generation with embedded CSS

---

## Integration Options

### Option 1: Port MarkdownService to .NET Core for Blazor

**What:** Extract the MarkdownService from your WPF app and create a .NET Core version for Orchestration Wisdom

**Benefits:**
- ✅ Reuse proven Markdig + Mermaid pipeline
- ✅ Professional, tested codebase
- ✅ SOLID architecture already in place
- ✅ Same rendering quality as desktop app
- ✅ Extensible template system

**Implementation:**

```csharp
// Create in Orchestration Wisdom project
namespace OrchestrationWisdom.Services;

public interface IMarkdownService
{
    string ConvertToHtml(string markdown);
}

public class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();
    }

    public string ConvertToHtml(string markdown)
    {
        var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        _pipeline.Setup(renderer);

        // Use custom Mermaid renderer
        renderer.ObjectRenderers.Replace<CodeBlockRenderer>(
            new MermaidCodeBlockRenderer()
        );

        var document = Markdig.Markdown.Parse(markdown ?? string.Empty, _pipeline);
        renderer.Render(document);
        writer.Flush();

        return writer.ToString();
    }
}

// Custom renderer for Mermaid diagrams
internal class MermaidCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        var fencedCodeBlock = obj as FencedCodeBlock;
        var info = fencedCodeBlock?.Info;

        if (info != null && info.ToLowerInvariant() == "mermaid")
        {
            renderer.Write("<pre class=\"mermaid\">");
            renderer.WriteLeafRawLines(obj, true, false);
            renderer.WriteLine("</pre>");
        }
        else
        {
            // Default code block rendering
            renderer.Write("<pre><code");
            if (!string.IsNullOrEmpty(info))
            {
                renderer.Write($" class=\"language-{info}\"");
            }
            renderer.Write(">");
            renderer.WriteLeafRawLines(obj, true, true);
            renderer.Write("</code></pre>");
            renderer.WriteLine();
        }
    }
}
```

**Blazor Component Usage:**

```razor
@inject IMarkdownService MarkdownService

<div class="markdown-content">
    @((MarkupString)MarkdownService.ConvertToHtml(markdownText))
</div>
```

**Package Required:**
```xml
<PackageReference Include="Markdig" Version="0.40.0" />
```

---

### Option 2: Use PowerShell Script as Content Generation Pipeline

**What:** Use `Generate-SequenceDocumentation.ps1` as a pre-build step to generate HTML content

**Benefits:**
- ✅ Already proven and working
- ✅ No additional .NET dependencies
- ✅ Generates static HTML files
- ✅ Works with your existing JSON workflow
- ✅ Can run as CI/CD pipeline step

**Implementation:**

1. **Create OWS JSON Schema** (similar to your sequence JSON):

```json
{
  "id": "support-ticket-escalation",
  "name": "Support Ticket Escalation Backlog",
  "version": "1.0",
  "domain": "customer-support",
  "problem": "Tickets escalate but get stuck in limbo",
  "orchestrationShift": "Define explicit ownership per tier",
  "hook": "Every support team knows the pain...",
  "asIsDiagram": "sequenceDiagram\n...",
  "orchestratedDiagram": "sequenceDiagram\n...",
  "scorecard": {
    "ownership": 5,
    "timeSLA": 4,
    ...
  },
  "components": [...]
}
```

2. **Modify PowerShell Script** for OWS format:

```powershell
# New function: Generate-OrchestrationPattern.ps1
function Generate-PatternMarkdown {
    param($PatternFilePath, $OutputPath)

    $pattern = Get-Content $PatternFilePath -Raw | ConvertFrom-Json

    $md = @"
# $($pattern.name)

$($pattern.hook)

## Problem in One Minute

$($pattern.problemDetail)

## As-Is Diagram

``````mermaid
$($pattern.asIsDiagram)
``````

## Orchestrated Diagram

``````mermaid
$($pattern.orchestratedDiagram)
``````

## Components

"@

    foreach ($component in $pattern.components) {
        $md += "`n- **$($component.name)**: $($component.description)"
    }

    # Convert to HTML using existing Convert-MarkdownToHtml function
    $htmlContent = Convert-MarkdownToHtml -Markdown $md -Title $pattern.name

    # Save HTML
    $htmlFile = Join-Path $OutputPath "$($pattern.id).html"
    [System.IO.File]::WriteAllText($htmlFile, $htmlContent, [System.Text.UTF8Encoding]::new($false))
}
```

3. **Pre-build Integration:**

```xml
<!-- OrchestrationWisdom.csproj -->
<Target Name="GeneratePatternHTML" BeforeTargets="Build">
  <Exec Command="pwsh -File $(SolutionDir)../../../Tools/Generate-OrchestrationPatterns.ps1" />
</Target>
```

---

### Option 3: Hybrid Approach (Recommended)

**What:** Combine both approaches for maximum flexibility

**Architecture:**

```
Pattern Authoring Flow:
┌─────────────────────────────────────────────────────────────┐
│ 1. AI-Generated Markdown (ChatGPT/Claude)                   │
│    - Uses ARTICLE.md template                               │
│    - Includes Mermaid diagrams                              │
│    - Follows OWS structure                                  │
└─────────────┬───────────────────────────────────────────────┘
              │
              v
┌─────────────────────────────────────────────────────────────┐
│ 2. MarkdownViewer (WPF Desktop Tool)                        │
│    - Preview rendered output                                │
│    - Edit and refine content                                │
│    - Validate Mermaid syntax                                │
│    - Export to HTML for testing                             │
└─────────────┬───────────────────────────────────────────────┘
              │
              v
┌─────────────────────────────────────────────────────────────┐
│ 3. Convert to OWS JSON                                      │
│    - Structured pattern data                                │
│    - Stored in /patterns folder                             │
│    - Versioned in git                                       │
└─────────────┬───────────────────────────────────────────────┘
              │
              v
┌─────────────────────────────────────────────────────────────┐
│ 4. Blazor PatternService (Runtime)                          │
│    - Load JSON patterns                                     │
│    - Use MarkdownService for rich text sections            │
│    - Render Mermaid diagrams client-side                    │
│    - Serve via web UI                                       │
└─────────────────────────────────────────────────────────────┘
```

**Benefits:**
- ✅ AI can generate initial content in markdown
- ✅ MarkdownViewer provides WYSIWYG preview
- ✅ JSON storage enables filtering/search
- ✅ Blazor app dynamically renders from JSON
- ✅ Same rendering pipeline (Markdig) everywhere

---

## Recommended Implementation Plan

### Phase 1: Extract and Port MarkdownService
1. Create `OrchestrationWisdom.Services.Markdown` namespace
2. Port `IMarkdownService`, `MarkdownService`, `MermaidCodeBlockRenderer`
3. Add Markdig NuGet package
4. Create unit tests
5. Update `PatternDetail.razor` to use MarkdownService for rich text sections

### Phase 2: Enhance Pattern Model
1. Add `RichTextSections` to Pattern model for sections that need markdown processing:
   ```csharp
   public class Pattern
   {
       // ... existing properties ...

       // Rich text sections (markdown)
       public string ProblemDetailMarkdown { get; set; }
       public string DecisionPointMarkdown { get; set; }
       public string MetricsMarkdown { get; set; }
       public string ChecklistMarkdown { get; set; }
       public string ClosingInsightMarkdown { get; set; }
   }
   ```

2. Update `PatternService` to process markdown on load

### Phase 3: Create Pattern Authoring Workflow
1. Define ARTICLE.md template matching OWS structure
2. Create PowerShell script `Convert-ArticleToJSON.ps1`:
   - Parses ARTICLE.md
   - Extracts Mermaid diagrams
   - Extracts metadata
   - Generates OWS JSON
3. Create `Import-Pattern.ps1` to import patterns into Blazor app

### Phase 4: MarkdownViewer Integration
1. Update MarkdownViewer to recognize OWS pattern templates
2. Add "Export to OWS JSON" feature
3. Add validation against OWS schema
4. Add Orchestration Scorecard calculator

---

## Detailed Example: End-to-End Flow

### Step 1: AI Generates Pattern (Markdown)

```markdown
# Support Ticket Escalation Backlog

Every support team knows the pain: escalated tickets that somehow become invisible.

## Problem in One Minute

When a support ticket gets escalated, it often falls into a gray zone where no one
feels responsible. The escalation queue grows, SLAs slip, and customers suffer.

## As-Is Diagram

``````mermaid
sequenceDiagram
    participant C as Customer
    participant L1 as L1 Support
    participant L2 as L2 Queue

    C->>L1: Reports issue
    L1->>L2: Escalates ticket
    Note over L2: Ticket sits in queue
``````

## Orchestrated Diagram

``````mermaid
sequenceDiagram
    participant C as Customer
    participant L1 as L1 Support
    participant Router as Smart Router
    participant L2 as L2 Owner

    C->>L1: Reports issue
    L1->>Router: Escalates with context
    Router->>L2: Routes to owner by capacity
``````

## Components

- **Escalation Routing** - Capacity-aware ticket routing
- **Ownership Assignment** - Clear owner identification
- **SLA Monitoring** - Automated tracking
```

### Step 2: Open in MarkdownViewer

- Developer opens in MarkdownViewer
- Sees rendered preview with Mermaid diagrams
- Makes edits, validates formatting
- Checks diagram constraints (≤7 actors, ≤18 steps)

### Step 3: Convert to JSON

```powershell
.\Convert-ArticleToJSON.ps1 -ArticleFile "support-ticket-escalation.md" -OutputFile "patterns/support-ticket-escalation.pattern.json"
```

Generates:

```json
{
  "id": "support-ticket-escalation",
  "slug": "support-ticket-escalation-backlog",
  "title": "Support Ticket Escalation Backlog",
  "hook": "Every support team knows the pain...",
  "asIsDiagram": "sequenceDiagram\n    participant C...",
  "orchestratedDiagram": "sequenceDiagram\n    participant C...",
  "components": [
    {
      "id": "escalation-routing",
      "name": "Escalation Routing",
      "description": "Capacity-aware ticket routing"
    }
  ]
}
```

### Step 4: Blazor App Loads and Renders

```csharp
// PatternService loads JSON
var pattern = await LoadPatternAsync("support-ticket-escalation");

// MarkdownService processes rich text
var hookHtml = _markdownService.ConvertToHtml(pattern.Hook);
var problemHtml = _markdownService.ConvertToHtml(pattern.ProblemDetailMarkdown);

// Blazor renders with Mermaid.js for diagrams
```

---

## Benefits of This Integration

### For Content Authors
- ✅ Use AI to generate initial patterns quickly
- ✅ Preview in professional desktop app (MarkdownViewer)
- ✅ WYSIWYG editing experience
- ✅ Mermaid diagrams render correctly before publishing
- ✅ Export to HTML for offline sharing

### For Platform Development
- ✅ Structured JSON storage enables search/filter
- ✅ Consistent rendering pipeline (Markdig everywhere)
- ✅ Version control friendly (JSON diffs)
- ✅ Type-safe C# models
- ✅ Automated validation

### For Platform Users
- ✅ Fast, interactive web experience
- ✅ Same rendering quality as desktop app
- ✅ Filterable, searchable content
- ✅ Download ARTICLE.md for offline use
- ✅ Print-friendly output

---

## Next Steps

### Immediate Actions
1. **Add Markdig to OrchestrationWisdom**
   ```bash
   cd src/OrchestrationWisdom/OrchestrationWisdom
   dotnet add package Markdig --version 0.40.0
   ```

2. **Create MarkdownService**
   - Port from MarkdownViewer
   - Add unit tests
   - Register in DI container

3. **Test Integration**
   - Update PatternDetail.razor to use MarkdownService
   - Verify Mermaid diagrams still render
   - Check styling consistency

### Future Enhancements
- Pattern authoring UI in Blazor (inspired by SequenceEditorWindow.xaml)
- Real-time markdown preview component
- Pattern validation against HQO rubric
- AI integration for pattern generation within app
- Git-based pattern storage with versioning

---

## Code Samples

### Register MarkdownService in Blazor

```csharp
// Program.cs
builder.Services.AddSingleton<IMarkdownService, MarkdownService>();
```

### Use in Razor Component

```razor
@page "/patterns/{slug}"
@inject IMarkdownService MarkdownService

<div class="pattern-section">
    <h2>Problem</h2>
    @((MarkupString)MarkdownService.ConvertToHtml(pattern.ProblemDetailMarkdown))
</div>

<div class="pattern-section">
    <h2>Decision Point</h2>
    @((MarkupString)MarkdownService.ConvertToHtml(pattern.DecisionPointMarkdown))
</div>
```

### Enhanced Pattern Model

```csharp
public class Pattern
{
    // Existing properties
    public string Id { get; set; }
    public string Title { get; set; }

    // Rich text (markdown) properties
    public string HookMarkdown { get; set; }
    public string ProblemDetailMarkdown { get; set; }
    public string DecisionPointMarkdown { get; set; }

    // Mermaid diagrams (raw markdown code block)
    public string AsIsDiagram { get; set; }
    public string OrchestratedDiagram { get; set; }

    // Structured data
    public OrchestrationScorecard Scorecard { get; set; }
    public List<Component> Components { get; set; }
}
```

---

## Summary

Your existing tools provide a **production-ready foundation** for building a professional content authoring and publishing pipeline:

1. **MarkdownViewer** = Desktop authoring/preview tool
2. **PowerShell Scripts** = Conversion/generation pipeline
3. **Orchestration Wisdom** = Web publishing platform

By integrating these with a hybrid approach, you get:
- AI-assisted content creation
- Professional desktop preview
- Structured JSON storage
- Dynamic web rendering
- Consistent output quality

**Recommendation:** Start with Phase 1 (port MarkdownService) and test integration. This gives immediate value with minimal risk.
