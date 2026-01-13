using System;
using System.IO;
using MarkdownViewer.Services;
using NUnit.Framework;

namespace MarkdownViewer.Tests
{
    [TestFixture]
    public class PowerShellSequenceConverterTests
    {
        private string testRepoRoot;
        private string testSequenceFile;
        private const string TestSequenceFileName = "test-sequence.json";

        [SetUp]
        public void SetUp()
        {
            // Find the actual repository root by searching for the PowerShell script
            testRepoRoot = FindRepositoryRoot();
            Assert.IsNotNull(testRepoRoot, "Could not find repository root with PowerShell script");

            // Use an actual sequence file from the sequences directory
            testSequenceFile = Path.Combine(testRepoRoot, "sequences", "wealth-builder-data-intake.json");
            Assert.IsTrue(File.Exists(testSequenceFile), $"Test sequence file not found: {testSequenceFile}");
        }

        [TearDown]
        public void TearDown()
        {
            // Optionally clean up generated test files
            // For now, we'll leave them to verify output
        }

        [Test]
        public void Constructor_WithValidRepositoryRoot_Succeeds()
        {
            // Act
            var converter = new PowerShellSequenceConverter(testRepoRoot);

            // Assert
            Assert.IsNotNull(converter);
        }

        [Test]
        public void Constructor_WithNullRepositoryRoot_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PowerShellSequenceConverter(null));
        }

        [Test]
        public void Constructor_WithInvalidRepositoryRoot_ThrowsFileNotFoundException()
        {
            // Arrange
            var invalidPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => new PowerShellSequenceConverter(invalidPath));
            Assert.That(ex.Message, Does.Contain("PowerShell script not found"));
        }

        [Test]
        public void ConvertSequenceToMarkdown_WithValidSequenceFile_GeneratesMarkdownFile()
        {
            // Arrange
            var converter = new PowerShellSequenceConverter(testRepoRoot);

            // Act
            var resultPath = converter.ConvertSequenceToMarkdown(testSequenceFile);

            // Assert
            Assert.IsNotNull(resultPath);
            Assert.IsTrue(File.Exists(resultPath), $"Generated markdown file not found: {resultPath}");
            Assert.That(resultPath, Does.EndWith(".md"));

            // Verify file has content
            var content = File.ReadAllText(resultPath);
            Assert.IsNotEmpty(content);
            Assert.That(content, Does.Contain("Stock Financial Data Ingestion")); // Title from the sequence
        }

        [Test]
        public void ConvertSequenceToMarkdown_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var converter = new PowerShellSequenceConverter(testRepoRoot);
            var nonExistentFile = Path.Combine(testRepoRoot, "nonexistent.json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => converter.ConvertSequenceToMarkdown(nonExistentFile));
        }

        [Test]
        public void ConvertSequenceToMarkdown_GeneratesHtmlFile()
        {
            // Arrange
            var converter = new PowerShellSequenceConverter(testRepoRoot);

            // Act
            var markdownPath = converter.ConvertSequenceToMarkdown(testSequenceFile);
            var htmlPath = Path.ChangeExtension(markdownPath, ".html");

            // Assert
            Assert.IsTrue(File.Exists(htmlPath), $"Generated HTML file not found: {htmlPath}");

            // Verify HTML has content
            var content = File.ReadAllText(htmlPath);
            Assert.IsNotEmpty(content);
            Assert.That(content, Does.Contain("<!DOCTYPE html>") | Does.Contain("<html"));
        }

        [Test]
        public void ConvertSequenceToMarkdown_WithMultipleConversions_Succeeds()
        {
            // Arrange
            var converter = new PowerShellSequenceConverter(testRepoRoot);

            // Act - Convert the same file twice
            var resultPath1 = converter.ConvertSequenceToMarkdown(testSequenceFile);
            var resultPath2 = converter.ConvertSequenceToMarkdown(testSequenceFile);

            // Assert - Both should succeed and produce the same path
            Assert.AreEqual(resultPath1, resultPath2);
            Assert.IsTrue(File.Exists(resultPath1));
            Assert.IsTrue(File.Exists(resultPath2));
        }

        [Test]
        public void ConvertSequenceToMarkdown_VerifyOutputStructure()
        {
            // Arrange
            var converter = new PowerShellSequenceConverter(testRepoRoot);

            // Act
            var resultPath = converter.ConvertSequenceToMarkdown(testSequenceFile);

            // Assert
            var content = File.ReadAllText(resultPath);

            // Verify expected sections are present
            Assert.That(content, Does.Contain("# "), "Should have header");
            Assert.That(content, Does.Contain("## "), "Should have sections");
            Assert.That(content, Does.Contain("Movement"), "Should contain movement information");
            Assert.That(content, Does.Contain("Beat"), "Should contain beat information");
        }

        /// <summary>
        /// Helper method to find repository root by searching for the PowerShell script
        /// </summary>
        private string FindRepositoryRoot()
        {
            // Start from the current test assembly location and walk up
            var currentDir = Path.GetDirectoryName(typeof(PowerShellSequenceConverterTests).Assembly.Location);

            while (!string.IsNullOrEmpty(currentDir))
            {
                var scriptPath = Path.Combine(currentDir, "Tools", "Generate-SequenceDocumentation.ps1");
                if (File.Exists(scriptPath))
                {
                    return currentDir;
                }

                var parentDir = Directory.GetParent(currentDir);
                if (parentDir == null)
                    break;

                currentDir = parentDir.FullName;
            }

            return null;
        }
    }
}
