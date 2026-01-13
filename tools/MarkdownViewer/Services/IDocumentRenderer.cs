namespace MarkdownViewer.Services
{
    /// <summary>
    /// Defines operations for rendering complete HTML documents from markdown
    /// </summary>
    public interface IDocumentRenderer
    {
        string RenderDocument(string markdown, string themeName);
    }
}