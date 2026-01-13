using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MarkdownViewer
{
    /// <summary>
    /// JSON Sequence Editor Window
    /// Allows editing of musical sequence JSON files with focus on mermaid diagrams
    /// </summary>
    public partial class SequenceEditorWindow : Window
    {
        private string sequenceFilePath;
        private JObject sequenceJson;
        private bool hasUnsavedChanges = false;
        private string currentDiagramPath = "diagram";

        public SequenceEditorWindow(string filePath)
        {
            InitializeComponent();

            sequenceFilePath = filePath;
            TxtFilePath.Text = filePath;

            // Set up keyboard shortcuts
            this.KeyDown += Window_KeyDown;

            InitializeWebView();
            LoadSequenceFile();

            // Set default selection
            CmbDiagramSelector.SelectedIndex = 0;
        }

        private async void InitializeWebView()
        {
            try
            {
                await PreviewWebView.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}\n\nPlease install WebView2 Runtime.",
                    "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSequenceFile()
        {
            try
            {
                TxtStatus.Text = "Loading sequence file...";
                StatusIcon.Kind = PackIconKind.Loading;

                // Read and parse JSON
                string jsonContent = File.ReadAllText(sequenceFilePath, Encoding.UTF8);
                sequenceJson = JObject.Parse(jsonContent);

                // Update title
                string sequenceName = sequenceJson["name"]?.ToString() ?? "Untitled Sequence";
                TxtTitle.Text = $"Editing: {sequenceName}";
                Title = $"Sequence Editor - {sequenceName}";

                // Load the main diagram by default
                LoadDiagramFromPath("diagram");

                // Populate diagram selector with available diagrams
                PopulateDiagramSelector();

                TxtStatus.Text = "Sequence loaded successfully";
                StatusIcon.Kind = PackIconKind.CheckCircle;
                StatusIcon.Foreground = System.Windows.Media.Brushes.Green;

                hasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sequence file:\n\n{ex.Message}",
                    "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Error loading sequence";
                StatusIcon.Kind = PackIconKind.AlertCircle;
                StatusIcon.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void PopulateDiagramSelector()
        {
            // Clear existing items (except the default ones)
            CmbDiagramSelector.Items.Clear();

            // Add main diagram
            if (sequenceJson["diagram"] != null)
            {
                CmbDiagramSelector.Items.Add(new ComboBoxItem
                {
                    Content = "Main Sequence Diagram",
                    Tag = "diagram"
                });
            }

            // Add user story diagram
            if (sequenceJson["userStory"]?["diagram"] != null)
            {
                CmbDiagramSelector.Items.Add(new ComboBoxItem
                {
                    Content = "User Story Diagram",
                    Tag = "userStory.diagram"
                });
            }

            // Add movement diagrams
            var movements = sequenceJson["movements"] as JArray;
            if (movements != null)
            {
                for (int i = 0; i < movements.Count; i++)
                {
                    var movement = movements[i] as JObject;
                    if (movement?["diagram"] != null)
                    {
                        string movementName = movement["name"]?.ToString() ?? $"Movement {i + 1}";
                        CmbDiagramSelector.Items.Add(new ComboBoxItem
                        {
                            Content = $"Movement: {movementName}",
                            Tag = $"movements[{i}].diagram"
                        });
                    }

                    // Add movement user story diagram
                    if (movement?["userStory"]?["diagram"] != null)
                    {
                        string movementName = movement["name"]?.ToString() ?? $"Movement {i + 1}";
                        CmbDiagramSelector.Items.Add(new ComboBoxItem
                        {
                            Content = $"Movement User Story: {movementName}",
                            Tag = $"movements[{i}].userStory.diagram"
                        });
                    }

                    // Add beat diagrams
                    var beats = movement["beats"] as JArray;
                    if (beats != null)
                    {
                        for (int j = 0; j < beats.Count; j++)
                        {
                            var beat = beats[j] as JObject;
                            if (beat?["diagram"] != null)
                            {
                                string beatName = beat["name"]?.ToString() ?? $"Beat {j + 1}";
                                CmbDiagramSelector.Items.Add(new ComboBoxItem
                                {
                                    Content = $"  Beat: {beatName}",
                                    Tag = $"movements[{i}].beats[{j}].diagram"
                                });
                            }

                            // Add beat user story diagram
                            if (beat?["userStory"]?["diagram"] != null)
                            {
                                string beatName = beat["name"]?.ToString() ?? $"Beat {j + 1}";
                                CmbDiagramSelector.Items.Add(new ComboBoxItem
                                {
                                    Content = $"  Beat User Story: {beatName}",
                                    Tag = $"movements[{i}].beats[{j}].userStory.diagram"
                                });
                            }
                        }
                    }
                }
            }

            if (CmbDiagramSelector.Items.Count > 0)
            {
                CmbDiagramSelector.SelectedIndex = 0;
            }
        }

        private void LoadDiagramFromPath(string path)
        {
            try
            {
                currentDiagramPath = path;

                // Navigate the JSON path to get the diagram
                string diagramCode = GetValueFromPath(sequenceJson, path)?.ToString() ?? "";

                TxtDiagramCode.Text = diagramCode;
                TxtEditorInfo.Text = $"Editing: {GetDiagramDisplayName(path)}";

                UpdateLineCount();
                RefreshPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading diagram:\n\n{ex.Message}",
                    "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetDiagramDisplayName(string path)
        {
            if (path == "diagram") return "Main Sequence Diagram";
            if (path == "userStory.diagram") return "User Story Diagram";

            // For movement and beat diagrams
            if (path.Contains("movements"))
            {
                var parts = path.Split('.');
                var movementIndex = ExtractIndex(parts[0]);

                if (path.Contains("beats"))
                {
                    var beatIndex = ExtractIndex(parts[1]);
                    var movement = sequenceJson["movements"]?[movementIndex];
                    var beat = movement?["beats"]?[beatIndex];

                    if (path.EndsWith("userStory.diagram"))
                        return $"Beat User Story: {beat?["name"]}";
                    else
                        return $"Beat: {beat?["name"]}";
                }
                else
                {
                    var movement = sequenceJson["movements"]?[movementIndex];
                    if (path.EndsWith("userStory.diagram"))
                        return $"Movement User Story: {movement?["name"]}";
                    else
                        return $"Movement: {movement?["name"]}";
                }
            }

            return path;
        }

        private int ExtractIndex(string part)
        {
            var start = part.IndexOf('[') + 1;
            var end = part.IndexOf(']');
            if (start > 0 && end > start)
            {
                return int.Parse(part.Substring(start, end - start));
            }
            return 0;
        }

        private JToken GetValueFromPath(JObject obj, string path)
        {
            var parts = path.Split('.');
            JToken current = obj;

            foreach (var part in parts)
            {
                if (part.Contains("["))
                {
                    // Array access: movements[0]
                    var arrayName = part.Substring(0, part.IndexOf('['));
                    var index = ExtractIndex(part);
                    current = current[arrayName]?[index];
                }
                else
                {
                    current = current[part];
                }

                if (current == null)
                    break;
            }

            return current;
        }

        private void SetValueAtPath(JObject obj, string path, string value)
        {
            var parts = path.Split('.');
            JToken current = obj;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (part.Contains("["))
                {
                    var arrayName = part.Substring(0, part.IndexOf('['));
                    var index = ExtractIndex(part);
                    current = current[arrayName]?[index];
                }
                else
                {
                    current = current[part];
                }

                if (current == null)
                    throw new Exception($"Invalid path: {path}");
            }

            var lastPart = parts[parts.Length - 1];
            if (current is JObject jobj)
            {
                jobj[lastPart] = value;
            }
        }

        private void RefreshPreview()
        {
            try
            {
                string diagramCode = TxtDiagramCode.Text.Trim();

                if (string.IsNullOrWhiteSpace(diagramCode))
                {
                    PreviewMessage.Visibility = Visibility.Visible;
                    TxtPreviewMessage.Text = "Enter diagram code to see preview";
                    return;
                }

                PreviewMessage.Visibility = Visibility.Collapsed;

                // Generate HTML with Mermaid.js for diagram rendering
                string html = GenerateMermaidHtml(diagramCode);

                if (PreviewWebView.CoreWebView2 != null)
                {
                    PreviewWebView.NavigateToString(html);
                }

                TxtStatus.Text = "Preview updated";
            }
            catch (Exception ex)
            {
                PreviewMessage.Visibility = Visibility.Visible;
                TxtPreviewMessage.Text = $"Preview Error: {ex.Message}";
                TxtStatus.Text = "Preview error";
            }
        }

        private string GenerateMermaidHtml(string mermaidCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{
            margin: 20px;
            padding: 20px;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #fafafa;
        }}
        .mermaid {{
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        #error {{
            background: #ffebee;
            color: #c62828;
            padding: 16px;
            border-radius: 4px;
            border-left: 4px solid #c62828;
            font-family: 'Consolas', monospace;
            display: none;
        }}
    </style>
    <script type=""module"">
        import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.esm.min.mjs';
        mermaid.initialize({{
            startOnLoad: true,
            theme: 'default',
            securityLevel: 'loose',
            logLevel: 'error'
        }});
    </script>
</head>
<body>
    <div id=""error""></div>
    <div class=""mermaid"">
{mermaidCode}
    </div>
    <script>
        window.addEventListener('error', function(e) {{
            document.getElementById('error').style.display = 'block';
            document.getElementById('error').innerText = 'Diagram Error: ' + e.message;
        }});
    </script>
</body>
</html>";
        }

        private void SaveSequence()
        {
            try
            {
                TxtStatus.Text = "Saving sequence...";
                StatusIcon.Kind = PackIconKind.Loading;

                // Update the JSON with current diagram code
                SetValueAtPath(sequenceJson, currentDiagramPath, TxtDiagramCode.Text.Trim());

                // Write to file with proper formatting
                string jsonOutput = sequenceJson.ToString(Formatting.Indented);
                File.WriteAllText(sequenceFilePath, jsonOutput, Encoding.UTF8);

                hasUnsavedChanges = false;
                TxtStatus.Text = "Sequence saved successfully";
                StatusIcon.Kind = PackIconKind.CheckCircle;
                StatusIcon.Foreground = System.Windows.Media.Brushes.Green;

                MessageBox.Show("Sequence saved successfully!", "Save",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving sequence:\n\n{ex.Message}",
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Error saving sequence";
                StatusIcon.Kind = PackIconKind.AlertCircle;
                StatusIcon.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void UpdateLineCount()
        {
            int lineCount = TxtDiagramCode.Text.Split('\n').Length;
            TxtLineCount.Text = $"Lines: {lineCount}";
        }

        // Event Handlers
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSequence();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreview();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CmbDiagramSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbDiagramSelector.SelectedItem is ComboBoxItem item)
            {
                string path = item.Tag?.ToString();
                if (!string.IsNullOrEmpty(path))
                {
                    // Check for unsaved changes before switching
                    if (hasUnsavedChanges)
                    {
                        var result = MessageBox.Show(
                            "You have unsaved changes. Do you want to save them before switching diagrams?",
                            "Unsaved Changes",
                            MessageBoxButton.YesNoCancel,
                            MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            SaveSequence();
                        }
                        else if (result == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }

                    LoadDiagramFromPath(path);
                    hasUnsavedChanges = false;
                }
            }
        }

        private void TxtDiagramCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            hasUnsavedChanges = true;
            UpdateLineCount();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+S to save
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveSequence();
                e.Handled = true;
            }
            // F5 to refresh preview
            else if (e.Key == Key.F5)
            {
                RefreshPreview();
                e.Handled = true;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveSequence();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }
    }
}
