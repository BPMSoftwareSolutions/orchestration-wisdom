using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrchestrationWisdom.Tools.MarkdownViewer.Services
{
    /// <summary>
    /// Validates Mermaid diagrams against budget constraints.
    /// Part of the desktop validation workflow for content creators.
    /// </summary>
    public interface IMermaidDiagramValidator
    {
        /// <summary>
        /// Validate a Mermaid diagram against budget constraints.
        /// Budget: ≤7 actors, ≤18 steps, ≤2 alt blocks, no nested alt blocks
        /// </summary>
        ValidationResult ValidateDiagram(string mermaidSyntax);

        /// <summary>
        /// Get detailed metrics about the diagram.
        /// </summary>
        DiagramMetrics AnalyzeDiagram(string mermaidSyntax);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public DiagramMetrics Metrics { get; set; }
    }

    public class DiagramMetrics
    {
        public int ParticipantCount { get; set; }
        public int StepCount { get; set; }
        public int AltBlockCount { get; set; }
        public int MaxAltBranchLength { get; set; }
        public bool HasNestedAlt { get; set; }
        public List<string> Participants { get; set; } = new List<string>();
    }

    /// <summary>
    /// Default Mermaid diagram validator.
    /// </summary>
    public class MermaidDiagramValidator : IMermaidDiagramValidator
    {
        private const int MaxParticipants = 7;
        private const int MaxSteps = 18;
        private const int MaxAltBlocks = 2;
        private const int MaxAltBranchLength = 8;

        public ValidationResult ValidateDiagram(string mermaidSyntax)
        {
            var result = new ValidationResult();
            var metrics = AnalyzeDiagram(mermaidSyntax);
            result.Metrics = metrics;

            // Check participant count
            if (metrics.ParticipantCount > MaxParticipants)
            {
                result.Errors.Add($"Too many participants: {metrics.ParticipantCount} (max {MaxParticipants})");
            }

            // Check step count
            if (metrics.StepCount > MaxSteps)
            {
                result.Errors.Add($"Too many steps: {metrics.StepCount} (max {MaxSteps})");
            }

            // Check alt block count
            if (metrics.AltBlockCount > MaxAltBlocks)
            {
                result.Errors.Add($"Too many alt blocks: {metrics.AltBlockCount} (max {MaxAltBlocks})");
            }

            // Check nested alt
            if (metrics.HasNestedAlt)
            {
                result.Errors.Add("Nested alt blocks not allowed (reduces clarity)");
            }

            // Check alt branch length
            if (metrics.MaxAltBranchLength > MaxAltBranchLength)
            {
                result.Warnings.Add($"Alt branch is long: {metrics.MaxAltBranchLength} steps (recommended max {MaxAltBranchLength})");
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        public DiagramMetrics AnalyzeDiagram(string mermaidSyntax)
        {
            var metrics = new DiagramMetrics();

            if (string.IsNullOrWhiteSpace(mermaidSyntax))
                return metrics;

            // Count participants
            var participantMatches = Regex.Matches(mermaidSyntax, @"participant\s+(\w+)", RegexOptions.IgnoreCase);
            metrics.ParticipantCount = participantMatches.Count;
            metrics.Participants = participantMatches.Cast<Match>().Select(m => m.Groups[1].Value).ToList();

            // Count arrows (steps)
            var arrowMatches = Regex.Matches(mermaidSyntax, @"(->>|--|>>|-\)-)", RegexOptions.IgnoreCase);
            metrics.StepCount = arrowMatches.Count;

            // Count alt blocks
            metrics.AltBlockCount = Regex.Matches(mermaidSyntax, @"\balt\b", RegexOptions.IgnoreCase).Count;

            // Check for nested alt (simple heuristic)
            var altRegex = new Regex(@"alt.*?else.*?end", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var altMatches = altRegex.Matches(mermaidSyntax);
            foreach (Match match in altMatches)
            {
                var altContent = match.Value;
                if (Regex.IsMatch(altContent, @"alt\s+.*alt", RegexOptions.IgnoreCase))
                {
                    metrics.HasNestedAlt = true;
                    break;
                }
            }

            // Calculate max alt branch length
            // This is a simplified calculation
            var lines = mermaidSyntax.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var inAlt = false;
            var currentBranchLength = 0;
            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, @"\balt\b", RegexOptions.IgnoreCase))
                {
                    inAlt = true;
                    currentBranchLength = 0;
                }
                else if (Regex.IsMatch(line, @"\belse\b", RegexOptions.IgnoreCase))
                {
                    metrics.MaxAltBranchLength = Math.Max(metrics.MaxAltBranchLength, currentBranchLength);
                    currentBranchLength = 0;
                }
                else if (Regex.IsMatch(line, @"\bend\b", RegexOptions.IgnoreCase))
                {
                    metrics.MaxAltBranchLength = Math.Max(metrics.MaxAltBranchLength, currentBranchLength);
                    inAlt = false;
                }
                else if (inAlt && Regex.IsMatch(line, @"(->>|--|>>|-\))", RegexOptions.IgnoreCase))
                {
                    currentBranchLength++;
                }
            }

            return metrics;
        }
    }
}
