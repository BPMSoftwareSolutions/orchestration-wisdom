using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace OrchestrationWisdom.Services.Markdown;

/// <summary>
/// Service for converting markdown to HTML
/// Ported from MarkdownViewer WPF application
///
/// Single Responsibility: Markdown conversion
/// Supports Mermaid diagrams via custom code block rendering
/// </summary>
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
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        // Convert markdown to HTML with custom rendering for mermaid blocks
        var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);

        // Setup pipeline first (this initializes all extensions)
        _pipeline.Setup(renderer);

        // Then override code block renderer to handle mermaid
        renderer.ObjectRenderers.Replace<CodeBlockRenderer>(new MermaidCodeBlockRenderer());

        var document = Markdig.Markdown.Parse(markdown, _pipeline);
        renderer.Render(document);
        writer.Flush();

        return writer.ToString();
    }
}

/// <summary>
/// Custom renderer that converts ```mermaid code blocks to pre class="mermaid"
/// so Mermaid.js can automatically render them as diagrams
///
/// This matches the rendering approach from the MarkdownViewer WPF application
/// </summary>
internal class MermaidCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        var fencedCodeBlock = obj as FencedCodeBlock;
        var info = fencedCodeBlock?.Info;

        if (info != null && info.ToLowerInvariant() == "mermaid")
        {
            // Render as <pre class="mermaid"> for Mermaid.js to process
            renderer.Write("<pre class=\"mermaid\">");
            renderer.WriteLeafRawLines(obj, true, false);
            renderer.WriteLine("</pre>");
        }
        else
        {
            // Use default code block rendering for non-mermaid blocks
            renderer.Write("<pre><code");

            if (!string.IsNullOrEmpty(info))
            {
                renderer.Write(" class=\"language-");
                renderer.Write(info);
                renderer.Write("\"");
            }

            renderer.Write(">");
            renderer.WriteLeafRawLines(obj, true, true);
            renderer.Write("</code></pre>");
            renderer.WriteLine();
        }
    }
}
