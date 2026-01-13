namespace MarkdownViewer.Services
{
    /// <summary>
    /// Defines operations for loading CSS styles
    /// </summary>
    public interface IStyleService
    {
        string LoadStyle(string styleName);
    }
}