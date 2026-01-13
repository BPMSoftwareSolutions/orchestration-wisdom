using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using MarkdownViewer.Services;

namespace MarkdownViewer
{
    /// <summary>
    /// Main Window - Presentation Layer
    /// Single Responsibility: UI interaction and coordination
    /// Depends on service abstractions (Dependency Inversion Principle)
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDocumentRenderer documentRenderer;
        private readonly RecentItemsManager recentItemsManager;
        private string currentFolderPath;
        private List<MarkdownFile> allFiles;
        private MarkdownFile currentFile;
        private string currentTheme = "light-theme";

        public MainWindow()
        {
            InitializeComponent();

            // Dependency Injection (manual for simplicity, could use IoC container)
            var markdownService = new MarkdownService();
            var templateService = new TemplateService();
            var styleService = new StyleService();
            documentRenderer = new DocumentRenderer(markdownService, templateService, styleService);
            recentItemsManager = new RecentItemsManager();

            InitializeWebView();
            LoadDefaultFolder();
            UpdateRecentMenus();
        }

        private async void InitializeWebView()
        {
            try
            {
                await MarkdownWebView.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}\n\nPlease install WebView2 Runtime.",
                    "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDefaultFolder()
        {
            string defaultPath = Directory.GetCurrentDirectory();
            if (Directory.Exists(defaultPath))
            {
                LoadFolder(defaultPath);
            }
        }

        private void LoadFolder(string folderPath)
        {
            try
            {
                currentFolderPath = folderPath;
                TxtCurrentPath.Text = folderPath;
                TxtStatus.Text = "Loading files...";

                // Add to recent folders
                recentItemsManager.AddRecentFolder(folderPath);
                UpdateRecentMenus();

                allFiles = new List<MarkdownFile>();
                var mdFiles = Directory.GetFiles(folderPath, "*.md", SearchOption.AllDirectories);
                var jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);

                foreach (var file in mdFiles.Concat(jsonFiles))
                {
                    var fileInfo = new FileInfo(file);
                    allFiles.Add(new MarkdownFile
                    {
                        FullPath = file,
                        Name = fileInfo.Name,
                        RelativePath = file.Replace(folderPath, "").TrimStart('\\', '/'),
                        Size = fileInfo.Length,
                        Modified = fileInfo.LastWriteTime
                    });
                }

                allFiles = allFiles.OrderBy(f => f.RelativePath).ToList();
                UpdateFileList(allFiles);

                TxtStatus.Text = $"Loaded {allFiles.Count} markdown files";
                TxtFileCount.Text = $"{allFiles.Count} files";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Error loading folder";
            }
        }

        private void UpdateFileList(List<MarkdownFile> files)
        {
            FileListView.ItemsSource = new ObservableCollection<MarkdownFile>(files);
        }

        private async void LoadMarkdownFile(MarkdownFile file)
        {
            try
            {
                currentFile = file;
                WelcomePanel.Visibility = Visibility.Collapsed;
                LoadingProgress.Visibility = Visibility.Visible;

                TxtFileName.Text = file.Name;
                TxtFileInfo.Text = $"{file.SizeText} • Modified {file.ModifiedText}";
                TxtStatus.Text = $"Loading {file.Name}...";

                // Add to recent files
                recentItemsManager.AddRecentFile(file.FullPath);
                UpdateRecentMenus();

                // Read markdown content
                string markdown = File.ReadAllText(file.FullPath, Encoding.UTF8);

                // Use document renderer service (SOLID principles)
                string html = documentRenderer.RenderDocument(markdown, currentTheme);

                // Load in WebView2
                if (MarkdownWebView.CoreWebView2 != null)
                {
                    MarkdownWebView.NavigateToString(html);
                }

                // Generate TOC
                GenerateTableOfContents(markdown);

                LoadingProgress.Visibility = Visibility.Collapsed;
                TxtStatus.Text = $"Viewing: {file.Name}";
            }
            catch (Exception ex)
            {
                LoadingProgress.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Error loading file";
            }
        }

        private void GenerateTableOfContents(string markdown)
        {
            var tocItems = new List<TocItem>();
            var lines = markdown.Split('\n');
            var stack = new Stack<TocItem>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("#"))
                {
                    int level = 0;
                    while (level < trimmed.Length && trimmed[level] == '#')
                        level++;

                    if (level > 0 && level <= 6)
                    {
                        string title = trimmed.Substring(level).Trim();
                        var item = new TocItem { Title = title, Level = level };

                        while (stack.Count > 0 && stack.Peek().Level >= level)
                            stack.Pop();

                        if (stack.Count > 0)
                        {
                            stack.Peek().Children.Add(item);
                        }
                        else
                        {
                            tocItems.Add(item);
                        }

                        stack.Push(item);
                    }
                }
            }

            TocTreeView.ItemsSource = tocItems;
        }

        // Event Handlers
        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            // Use Windows folder browser dialog
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a folder containing markdown files (you can paste a path in the dialog)";
                dialog.ShowNewFolderButton = false;

                // Set initial directory to current folder if available
                if (!string.IsNullOrEmpty(currentFolderPath))
                {
                    dialog.SelectedPath = currentFolderPath;
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadFolder(dialog.SelectedPath);
                }
            }
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Markdown File",
                Filter = "Markdown Files (*.md)|*.md|All Files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true,
                Multiselect = false
            };

            // Set initial directory
            if (!string.IsNullOrEmpty(currentFolderPath))
            {
                dialog.InitialDirectory = currentFolderPath;
            }
            else if (currentFile != null)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(currentFile.FullPath);
            }

            if (dialog.ShowDialog() == true)
            {
                // Load the selected file
                var fileInfo = new FileInfo(dialog.FileName);
                var mdFile = new MarkdownFile
                {
                    FullPath = dialog.FileName,
                    Name = fileInfo.Name,
                    RelativePath = fileInfo.Name,
                    Size = fileInfo.Length,
                    Modified = fileInfo.LastWriteTime
                };

                LoadMarkdownFile(mdFile);

                // Update current folder to the file's directory
                currentFolderPath = Path.GetDirectoryName(dialog.FileName);
                TxtCurrentPath.Text = currentFolderPath;
                TxtStatus.Text = $"Opened file from: {currentFolderPath}";
            }
        }

        private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileListView.SelectedItem is MarkdownFile file)
            {
                // Check if it's a JSON file
                if (file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    // Prompt to open in sequence editor
                    var result = MessageBox.Show(
                        $"Would you like to open '{file.Name}' in the Sequence Editor?",
                        "Open JSON Sequence",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var editorWindow = new SequenceEditorWindow(file.FullPath);
                            editorWindow.Owner = this;
                            editorWindow.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error opening sequence editor:\n\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    LoadMarkdownFile(file);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allFiles == null) return;

            string query = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(query))
            {
                UpdateFileList(allFiles);
            }
            else
            {
                var filtered = allFiles.Where(f =>
                    f.Name.ToLower().Contains(query) ||
                    f.RelativePath.ToLower().Contains(query)
                ).ToList();
                UpdateFileList(filtered);
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = string.Empty;
        }

        private void BtnToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between light and dark theme
            currentTheme = currentTheme == "light-theme" ? "dark-theme" : "light-theme";

            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(currentTheme == "dark-theme" ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);

            // Update button label
            TxtThemeLabel.Text = currentTheme == "dark-theme" ? "LIGHT MODE" : "DARK MODE";

            if (currentFile != null)
            {
                LoadMarkdownFile(currentFile);
            }

            TxtStatus.Text = currentTheme == "dark-theme" ? "Dark theme enabled" : "Light theme enabled";
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (currentFile != null)
            {
                LoadMarkdownFile(currentFile);
                TxtStatus.Text = "File refreshed";
            }
            else if (!string.IsNullOrEmpty(currentFolderPath))
            {
                LoadFolder(currentFolderPath);
            }
        }

        private void BtnToggleToc_Click(object sender, RoutedEventArgs e)
        {
            TocPanel.Visibility = TocPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void TocTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TocItem item)
            {
                TxtStatus.Text = $"Navigated to: {item.Title}";
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (currentFile == null)
            {
                MessageBox.Show("No file loaded to export", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "HTML Files (*.html)|*.html",
                FileName = Path.GetFileNameWithoutExtension(currentFile.Name) + ".html"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string markdown = File.ReadAllText(currentFile.FullPath, Encoding.UTF8);
                    string html = documentRenderer.RenderDocument(markdown, currentTheme);
                    File.WriteAllText(saveDialog.FileName, html, Encoding.UTF8);
                    TxtStatus.Text = $"Exported to {Path.GetFileName(saveDialog.FileName)}";
                    MessageBox.Show("File exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (MarkdownWebView.CoreWebView2 != null)
            {
                MarkdownWebView.CoreWebView2.ExecuteScriptAsync("window.print();");
                TxtStatus.Text = "Print dialog opened";
            }
        }

        private void BtnEditSequence_Click(object sender, RoutedEventArgs e)
        {
            MenuEditSequence_Click(sender, e);
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            string helpMessage = @"Markdown Viewer - Professional Edition

FEATURES:
• Material Design UI with light/dark themes
• Real-time file browser with search
• Table of Contents navigation
• Export to HTML
• Print support

NAVIGATION:
• Open Folder - Browse for markdown files
• Search - Filter files by name or path
• File List - Click to view

ACTIONS:
• Toggle Theme - Switch between light/dark
• Refresh - Reload current file
• Toggle TOC - Show/hide table of contents
• Export - Save as HTML
• Print - Print current document

KEYBOARD SHORTCUTS:
• F5 - Refresh
• Ctrl+O - Open folder
• Ctrl+F - Search

ARCHITECTURE:
This application follows SOLID principles:
• Single Responsibility Principle
• Open/Closed Principle
• Dependency Inversion Principle
• Separation of Concerns

Built with:
• WPF & Material Design
• WebView2 for rendering
• Markdig for markdown processing
• Data-driven templates & styles";

            MessageBox.Show(helpMessage, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Menu Handlers
        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            BtnOpenFile_Click(sender, e);
        }

        private void MenuOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            BtnOpenFolder_Click(sender, e);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuEditSequence_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Sequence JSON File to Edit",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true
            };

            // Set initial directory
            if (!string.IsNullOrEmpty(currentFolderPath))
            {
                dialog.InitialDirectory = currentFolderPath;
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    TxtStatus.Text = "Opening sequence editor...";

                    // Open the sequence editor window
                    var editorWindow = new SequenceEditorWindow(dialog.FileName);
                    editorWindow.Owner = this;
                    editorWindow.ShowDialog();

                    TxtStatus.Text = "Sequence editor closed";
                }
                catch (Exception ex)
                {
                    TxtStatus.Text = "Error opening sequence editor";
                    MessageBox.Show($"Error opening sequence editor:\n\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuConvertSequence_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Sequence JSON File",
                Filter = "Sequence JSON Files (*.sequence.json)|*.sequence.json|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    TxtStatus.Text = "Converting sequence to markdown...";

                    // Find repository root by searching for the PowerShell script
                    var repoRoot = FindRepositoryRoot(dialog.FileName);
                    if (repoRoot == null)
                    {
                        MessageBox.Show("Could not find repository root. Please ensure the PowerShell script exists at Tools/Generate-SequenceDocumentation.ps1",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Use service for conversion (Dependency Inversion Principle)
                    var converter = new PowerShellSequenceConverter(repoRoot);
                    var markdownFile = converter.ConvertSequenceToMarkdown(dialog.FileName);

                    // Load the generated markdown file
                    var fileInfo = new FileInfo(markdownFile);
                    var mdFile = new MarkdownFile
                    {
                        FullPath = markdownFile,
                        Name = fileInfo.Name,
                        RelativePath = fileInfo.Name,
                        Size = fileInfo.Length,
                        Modified = fileInfo.LastWriteTime
                    };

                    LoadMarkdownFile(mdFile);
                    TxtStatus.Text = $"Sequence converted successfully: {fileInfo.Name}";

                    MessageBox.Show($"Sequence documentation generated successfully!\n\nFile: {markdownFile}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    TxtStatus.Text = "Error converting sequence";
                    MessageBox.Show($"Error converting sequence:\n\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string FindRepositoryRoot(string sequenceFilePath)
        {
            // Start from the sequence file's directory and walk up looking for the script
            var currentDir = Path.GetDirectoryName(sequenceFilePath);

            while (!string.IsNullOrEmpty(currentDir))
            {
                var scriptPath = Path.Combine(currentDir, "Tools", "Generate-SequenceDocumentation.ps1");
                if (File.Exists(scriptPath))
                {
                    return currentDir;
                }

                // Move up one directory
                var parentDir = Directory.GetParent(currentDir);
                if (parentDir == null)
                    break;

                currentDir = parentDir.FullName;
            }

            return null;
        }

        private void UpdateRecentMenus()
        {
            // Update Recent Files submenu
            MenuRecentFiles.Items.Clear();

            if (recentItemsManager.RecentFiles.Count == 0)
            {
                var noFilesItem = new MenuItem { Header = "(No recent files)", IsEnabled = false };
                MenuRecentFiles.Items.Add(noFilesItem);
            }
            else
            {
                foreach (var filePath in recentItemsManager.RecentFiles)
                {
                    var menuItem = new MenuItem
                    {
                        Header = Path.GetFileName(filePath),
                        ToolTip = filePath,
                        Tag = filePath
                    };
                    menuItem.Click += RecentFile_Click;
                    MenuRecentFiles.Items.Add(menuItem);
                }

                MenuRecentFiles.Items.Add(new Separator());

                var clearItem = new MenuItem { Header = "Clear Recent Files" };
                clearItem.Click += (s, e) =>
                {
                    recentItemsManager.ClearRecentFiles();
                    UpdateRecentMenus();
                };
                MenuRecentFiles.Items.Add(clearItem);
            }

            // Update Recent Folders submenu
            MenuRecentFolders.Items.Clear();

            if (recentItemsManager.RecentFolders.Count == 0)
            {
                var noFoldersItem = new MenuItem { Header = "(No recent folders)", IsEnabled = false };
                MenuRecentFolders.Items.Add(noFoldersItem);
            }
            else
            {
                foreach (var folderPath in recentItemsManager.RecentFolders)
                {
                    var menuItem = new MenuItem
                    {
                        Header = Path.GetFileName(folderPath),
                        ToolTip = folderPath,
                        Tag = folderPath
                    };
                    menuItem.Click += RecentFolder_Click;
                    MenuRecentFolders.Items.Add(menuItem);
                }

                MenuRecentFolders.Items.Add(new Separator());

                var clearItem = new MenuItem { Header = "Clear Recent Folders" };
                clearItem.Click += (s, e) =>
                {
                    recentItemsManager.ClearRecentFolders();
                    UpdateRecentMenus();
                };
                MenuRecentFolders.Items.Add(clearItem);
            }
        }

        private void RecentFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string filePath)
            {
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    var mdFile = new MarkdownFile
                    {
                        FullPath = filePath,
                        Name = fileInfo.Name,
                        RelativePath = fileInfo.Name,
                        Size = fileInfo.Length,
                        Modified = fileInfo.LastWriteTime
                    };

                    LoadMarkdownFile(mdFile);
                    currentFolderPath = Path.GetDirectoryName(filePath);
                    TxtCurrentPath.Text = currentFolderPath;
                }
                else
                {
                    MessageBox.Show($"File not found: {filePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UpdateRecentMenus();
                }
            }
        }

        private void RecentFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string folderPath)
            {
                if (Directory.Exists(folderPath))
                {
                    LoadFolder(folderPath);
                }
                else
                {
                    MessageBox.Show($"Folder not found: {folderPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UpdateRecentMenus();
                }
            }
        }
    }

    // Data Models (separate from business logic)
    public class MarkdownFile
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }

        public string SizeText => Size < 1024 ? $"{Size} B" :
                                 Size < 1024 * 1024 ? $"{Size / 1024.0:F1} KB" :
                                 $"{Size / (1024.0 * 1024.0):F1} MB";

        public string ModifiedText => Modified.ToString("yyyy-MM-dd HH:mm");
    }

    public class TocItem
    {
        public string Title { get; set; }
        public int Level { get; set; }
        public ObservableCollection<TocItem> Children { get; set; } = new ObservableCollection<TocItem>();
    }
}