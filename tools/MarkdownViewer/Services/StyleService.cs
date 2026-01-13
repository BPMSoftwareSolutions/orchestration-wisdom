using System;
using System.Collections.Generic;
using System.IO;

namespace MarkdownViewer.Services
{
    /// <summary>
    /// Service for loading CSS styles
    /// Single Responsibility: Style management
    /// </summary>
    public class StyleService : IStyleService
    {
        private readonly string stylesPath;
        private readonly Dictionary<string, string> styleCache;

        public StyleService()
        {
            stylesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Styles");
            styleCache = new Dictionary<string, string>();
        }

        public string LoadStyle(string styleName)
        {
            if (styleCache.ContainsKey(styleName))
            {
                return styleCache[styleName];
            }

            string styleFile = Path.Combine(stylesPath, styleName);
            if (!File.Exists(styleFile))
            {
                throw new FileNotFoundException($"Style not found: {styleName}");
            }

            string style = File.ReadAllText(styleFile);
            styleCache[styleName] = style;
            return style;
        }
    }
}