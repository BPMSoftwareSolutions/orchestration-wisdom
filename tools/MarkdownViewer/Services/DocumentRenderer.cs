using System.Collections.Generic;

namespace MarkdownViewer.Services
{
    /// <summary>
    /// Renders complete HTML documents from markdown
    /// Dependency Inversion: Depends on abstractions (interfaces) not concretions
    /// Open/Closed: Open for extension (new themes), closed for modification
    /// </summary>
    public class DocumentRenderer : IDocumentRenderer
    {
        private readonly IMarkdownService markdownService;
        private readonly ITemplateService templateService;
        private readonly IStyleService styleService;

        public DocumentRenderer(
            IMarkdownService markdownService,
            ITemplateService templateService,
            IStyleService styleService)
        {
            this.markdownService = markdownService;
            this.templateService = templateService;
            this.styleService = styleService;
        }

        public string RenderDocument(string markdown, string themeName)
        {
            // Convert markdown to HTML
            string htmlContent = markdownService.ConvertToHtml(markdown);

            // Load appropriate style
            string styleFileName = $"{themeName}.css";
            string styles = styleService.LoadStyle(styleFileName);

            // Determine Mermaid theme based on CSS theme
            string mermaidTheme = themeName == "dark-theme" ? "dark" : "default";

            // Load template
            string template = templateService.LoadTemplate("document.html");

            // Render final document with all placeholders
            var data = new
            {
                STYLES = styles,
                CONTENT = htmlContent,
                MERMAID_THEME = mermaidTheme
            };

            return templateService.RenderTemplate(template, data);
        }
    }
}