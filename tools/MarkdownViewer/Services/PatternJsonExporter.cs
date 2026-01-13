using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrchestrationWisdom.Tools.MarkdownViewer.Services
{
    /// <summary>
    /// Exports Markdown patterns to OWS JSON format.
    /// Part of the content creator workflow for publishing patterns.
    /// </summary>
    public interface IPatternJsonExporter
    {
        /// <summary>
        /// Convert a Markdown pattern to OWS JSON format.
        /// </summary>
        Task<PatternJson> ExportAsync(string markdownContent);

        /// <summary>
        /// Save exported pattern to JSON file.
        /// </summary>
        Task SaveAsync(PatternJson pattern, string outputPath);
    }

    /// <summary>
    /// Pattern in OWS JSON format.
    /// </summary>
    public class PatternJson
    {
        [JsonPropertyName("$schema")]
        public string Schema { get; set; } = "../schemas/pattern.schema.json";

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = "orchestration";

        [JsonPropertyName("hookMarkdown")]
        public string HookMarkdown { get; set; }

        [JsonPropertyName("problemDetailMarkdown")]
        public string ProblemDetailMarkdown { get; set; }

        [JsonPropertyName("asIsDiagramMermaid")]
        public string AsIsDiagramMermaid { get; set; }

        [JsonPropertyName("orchestratedDiagramMermaid")]
        public string OrchestratedDiagramMermaid { get; set; }

        [JsonPropertyName("decisionPointMarkdown")]
        public string DecisionPointMarkdown { get; set; }

        [JsonPropertyName("metricsMarkdown")]
        public string MetricsMarkdown { get; set; }

        [JsonPropertyName("implementationChecklist")]
        public List<string> ImplementationChecklist { get; set; } = new List<string>();

        [JsonPropertyName("commonPitfalls")]
        public List<string> CommonPitfalls { get; set; } = new List<string>();

        [JsonPropertyName("closingInsightMarkdown")]
        public string ClosingInsightMarkdown { get; set; }

        [JsonPropertyName("scorecard")]
        public Dictionary<string, int> Scorecard { get; set; }

        [JsonPropertyName("components")]
        public List<string> Components { get; set; } = new List<string>();

        [JsonPropertyName("industries")]
        public List<string> Industries { get; set; } = new List<string>();

        [JsonPropertyName("brokenSignals")]
        public List<string> BrokenSignals { get; set; } = new List<string>();

        [JsonPropertyName("clarityScore")]
        public double ClarityScore { get; set; }

        [JsonPropertyName("referenceCount")]
        public int ReferenceCount { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Default pattern JSON exporter.
    /// </summary>
    public class PatternJsonExporter : IPatternJsonExporter
    {
        public async Task<PatternJson> ExportAsync(string markdownContent)
        {
            // TODO: Implement markdown parsing
            // Should:
            // 1. Extract front matter (if present) for metadata
            // 2. Split by heading levels (##) to identify sections
            // 3. Extract Mermaid diagram blocks
            // 4. Parse checklist items
            // 5. Extract HQO scorecard values
            
            var pattern = new PatternJson();

            // Parse sections
            var hook = ExtractSection(markdownContent, "Hook");
            var problem = ExtractSection(markdownContent, "Problem");
            var asIs = ExtractMermaidDiagram(markdownContent, 0);
            var orchestrated = ExtractMermaidDiagram(markdownContent, 1);
            var decision = ExtractSection(markdownContent, "Decision");
            var metrics = ExtractSection(markdownContent, "Metrics");
            var checklist = ExtractChecklist(markdownContent);
            var pitfalls = ExtractBulletList(markdownContent, "Common Pitfalls");
            var insight = ExtractSection(markdownContent, "Closing");

            pattern.HookMarkdown = hook;
            pattern.ProblemDetailMarkdown = problem;
            pattern.AsIsDiagramMermaid = asIs;
            pattern.OrchestratedDiagramMermaid = orchestrated;
            pattern.DecisionPointMarkdown = decision;
            pattern.MetricsMarkdown = metrics;
            pattern.ImplementationChecklist = checklist;
            pattern.CommonPitfalls = pitfalls;
            pattern.ClosingInsightMarkdown = insight;

            return await Task.FromResult(pattern);
        }

        public async Task SaveAsync(PatternJson pattern, string outputPath)
        {
            // TODO: Implement JSON serialization and file save
            // Should:
            // 1. Serialize pattern to JSON
            // 2. Format with proper indentation
            // 3. Write to file
            // 4. Verify schema validity
            
            await Task.CompletedTask;
        }

        private string ExtractSection(string markdown, string sectionName)
        {
            // Extract content between ## SectionName and next ##
            var pattern = $@"##\s+{sectionName}.*?(?=##|$)";
            var match = Regex.Match(markdown, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                var content = match.Value;
                // Remove the heading line
                content = Regex.Replace(content, @"##\s+\w+\s*\n", "");
                return content.Trim();
            }
            return string.Empty;
        }

        private string ExtractMermaidDiagram(string markdown, int diagramIndex)
        {
            // Extract Mermaid code blocks
            var matches = Regex.Matches(markdown, @"```mermaid\s+([\s\S]*?)```", RegexOptions.IgnoreCase);
            if (matches.Count > diagramIndex)
            {
                return matches[diagramIndex].Groups[1].Value.Trim();
            }
            return string.Empty;
        }

        private List<string> ExtractChecklist(string markdown)
        {
            var checklist = new List<string>();
            var pattern = @"^\s*\d+\.\s+(.+)$";
            var matches = Regex.Matches(markdown, pattern, RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                checklist.Add(match.Groups[1].Value.Trim());
            }
            return checklist;
        }

        private List<string> ExtractBulletList(string markdown, string sectionName)
        {
            var bullets = new List<string>();
            var section = ExtractSection(markdown, sectionName);
            var pattern = @"^\s*[-•]\s+(.+)$";
            var matches = Regex.Matches(section, pattern, RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                bullets.Add(match.Groups[1].Value.Trim());
            }
            return bullets;
        }
    }
}
