using System;
using System.Diagnostics;
using System.IO;

namespace MarkdownViewer.Services
{
    /// <summary>
    /// Converts sequence JSON files to markdown documentation using PowerShell script
    /// Single Responsibility: Sequence to markdown conversion via PowerShell
    /// </summary>
    public class PowerShellSequenceConverter
    {
        private readonly string repositoryRoot;
        private readonly string scriptPath;

        public PowerShellSequenceConverter(string repositoryRoot)
        {
            this.repositoryRoot = repositoryRoot ?? throw new ArgumentNullException(nameof(repositoryRoot));
            this.scriptPath = Path.Combine(repositoryRoot, "Tools", "Generate-SequenceDocumentation.ps1");

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"PowerShell script not found: {scriptPath}");
            }
        }

        /// <summary>
        /// Converts a sequence JSON file to markdown using PowerShell script
        /// </summary>
        /// <param name="sequenceFilePath">Absolute path to the sequence JSON file</param>
        /// <returns>Absolute path to the generated markdown file</returns>
        public string ConvertSequenceToMarkdown(string sequenceFilePath)
        {
            if (!File.Exists(sequenceFilePath))
            {
                throw new FileNotFoundException($"Sequence file not found: {sequenceFilePath}");
            }

            // Build relative path from repo root to sequence file
            var relativePath = GetRelativePath(repositoryRoot, sequenceFilePath).Replace("\\", "/");

            // Execute PowerShell script
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -SequenceFile \"{relativePath}\" -GenerateHtml",
                WorkingDirectory = repositoryRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            string output;
            string error;
            int exitCode;

            using (var process = Process.Start(processInfo))
            {
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    $"PowerShell script failed with exit code {exitCode}.\n\nError: {error}\n\nOutput: {output}");
            }

            // Determine output file path
            var sequenceFileName = GetSequenceFileName(sequenceFilePath);
            var outputFolder = Path.Combine(repositoryRoot, "reports", "sequences");
            var markdownFile = Path.Combine(outputFolder, $"{sequenceFileName}.md");

            if (!File.Exists(markdownFile))
            {
                throw new FileNotFoundException(
                    $"Expected output file not found after conversion.\n\nExpected: {markdownFile}\n\nOutput: {output}");
            }

            return markdownFile;
        }

        private string GetSequenceFileName(string sequenceFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(sequenceFilePath);

            // Remove .sequence extension if present (e.g., "file.sequence.json" -> "file")
            if (fileName.EndsWith(".sequence", StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }

            return fileName;
        }

        /// <summary>
        /// Gets the relative path from one directory to another (compatible with .NET Framework 4.8)
        /// </summary>
        private string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException(nameof(fromPath));
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException(nameof(toPath));

            var fromUri = new Uri(Path.GetFullPath(fromPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            var toUri = new Uri(Path.GetFullPath(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath; // Cannot make relative path
            }

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }
    }
}
