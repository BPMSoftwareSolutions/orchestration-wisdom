namespace MarkdownViewer.Services
{
    /// <summary>
    /// Defines operations for converting markdown to HTML
    /// </summary>
    public interface IMarkdownService
    {
        string ConvertToHtml(string markdown);
    }
}