namespace MarkdownViewer.Services
{
    /// <summary>
    /// Defines operations for loading and rendering HTML templates
    /// </summary>
    public interface ITemplateService
    {
        string LoadTemplate(string templateName);
        string RenderTemplate(string template, object data);
    }
}