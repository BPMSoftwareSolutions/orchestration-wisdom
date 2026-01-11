namespace OrchestrationWisdom.Services.Markdown;

/// <summary>
/// Defines operations for converting markdown to HTML
/// Ported from MarkdownViewer WPF application
/// </summary>
public interface IMarkdownService
{
    /// <summary>
    /// Converts markdown text to HTML
    /// </summary>
    /// <param name="markdown">The markdown content to convert</param>
    /// <returns>HTML string</returns>
    string ConvertToHtml(string markdown);
}
