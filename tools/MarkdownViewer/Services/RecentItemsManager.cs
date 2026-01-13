using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkdownViewer.Services
{
    /// <summary>
    /// Manages recent files and folders using JSON file storage
    /// Single Responsibility: Recent items persistence
    /// </summary>
    public class RecentItemsManager
    {
        private const int MaxRecentItems = 10;
        private readonly string settingsPath;
        private List<string> recentFiles;
        private List<string> recentFolders;

        public RecentItemsManager()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MarkdownViewer"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            settingsPath = Path.Combine(appDataPath, "recent-items.txt");
            LoadRecentItems();
        }

        public IReadOnlyList<string> RecentFiles => recentFiles.AsReadOnly();
        public IReadOnlyList<string> RecentFolders => recentFolders.AsReadOnly();

        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            // Remove if already exists (to move to top)
            recentFiles.Remove(filePath);

            // Add to top
            recentFiles.Insert(0, filePath);

            // Keep only max items
            if (recentFiles.Count > MaxRecentItems)
            {
                recentFiles = recentFiles.Take(MaxRecentItems).ToList();
            }

            SaveRecentItems();
        }

        public void AddRecentFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return;

            // Remove if already exists (to move to top)
            recentFolders.Remove(folderPath);

            // Add to top
            recentFolders.Insert(0, folderPath);

            // Keep only max items
            if (recentFolders.Count > MaxRecentItems)
            {
                recentFolders = recentFolders.Take(MaxRecentItems).ToList();
            }

            SaveRecentItems();
        }

        public void ClearRecentFiles()
        {
            recentFiles.Clear();
            SaveRecentItems();
        }

        public void ClearRecentFolders()
        {
            recentFolders.Clear();
            SaveRecentItems();
        }

        private void LoadRecentItems()
        {
            recentFiles = new List<string>();
            recentFolders = new List<string>();

            if (!File.Exists(settingsPath))
                return;

            try
            {
                var lines = File.ReadAllLines(settingsPath);
                var section = "";

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        section = line.Trim('[', ']');
                        continue;
                    }

                    if (section == "RecentFiles" && File.Exists(line))
                    {
                        recentFiles.Add(line);
                    }
                    else if (section == "RecentFolders" && Directory.Exists(line))
                    {
                        recentFolders.Add(line);
                    }
                }
            }
            catch
            {
                // If load fails, start with empty lists
                recentFiles = new List<string>();
                recentFolders = new List<string>();
            }
        }

        private void SaveRecentItems()
        {
            try
            {
                var lines = new List<string>();

                lines.Add("[RecentFiles]");
                foreach (var file in recentFiles)
                {
                    lines.Add(file);
                }

                lines.Add("");
                lines.Add("[RecentFolders]");
                foreach (var folder in recentFolders)
                {
                    lines.Add(folder);
                }

                File.WriteAllLines(settingsPath, lines);
            }
            catch
            {
                // Silently fail if save fails
            }
        }
    }
}