using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MarkdownViewer.Services
{
    /// <summary>
    /// Service for loading and rendering HTML templates
    /// Single Responsibility: Template loading and variable substitution
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private readonly string templatesPath;
        private readonly Dictionary<string, string> templateCache;

        public TemplateService()
        {
            templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            templateCache = new Dictionary<string, string>();
        }

        public string LoadTemplate(string templateName)
        {
            if (templateCache.ContainsKey(templateName))
            {
                return templateCache[templateName];
            }

            string templateFile = Path.Combine(templatesPath, templateName);
            if (!File.Exists(templateFile))
            {
                throw new FileNotFoundException($"Template not found: {templateName}");
            }

            string template = File.ReadAllText(templateFile);
            templateCache[templateName] = template;
            return template;
        }

        public string RenderTemplate(string template, object data)
        {
            if (data == null)
            {
                return template;
            }

            string rendered = template;

            // Simple placeholder replacement: {{KEY}}
            var properties = data.GetType().GetProperties();
            foreach (var prop in properties)
            {
                string placeholder = $"{{{{{prop.Name}}}}}";
                string value = prop.GetValue(data)?.ToString() ?? string.Empty;
                rendered = rendered.Replace(placeholder, value);
            }

            return rendered;
        }
    }
}