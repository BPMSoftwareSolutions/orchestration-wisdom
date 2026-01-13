using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MarkdownViewer.Services
{
    /// <summary>
    /// Converts sequence JSON files to markdown documentation
    /// Single Responsibility: Sequence to markdown conversion
    /// </summary>
    public class SequenceConverter
    {
        public string ConvertSequenceToMarkdown(string sequenceFilePath)
        {
            if (!File.Exists(sequenceFilePath))
            {
                throw new FileNotFoundException($"Sequence file not found: {sequenceFilePath}");
            }

            var json = File.ReadAllText(sequenceFilePath);
            var sequence = JObject.Parse(json);

            var markdown = new StringBuilder();

            // Title
            var sequenceName = sequence["sequence_name"]?.ToString() ?? Path.GetFileNameWithoutExtension(sequenceFilePath);
            markdown.AppendLine($"# {sequenceName}");
            markdown.AppendLine();

            // Metadata
            var domain = sequence["domain"]?.ToString();
            var version = sequence["version"]?.ToString();
            var description = sequence["description"]?.ToString();

            if (!string.IsNullOrEmpty(domain) || !string.IsNullOrEmpty(version))
            {
                markdown.AppendLine("## Metadata");
                markdown.AppendLine();
                if (!string.IsNullOrEmpty(domain))
                    markdown.AppendLine($"- **Domain**: {domain}");
                if (!string.IsNullOrEmpty(version))
                    markdown.AppendLine($"- **Version**: {version}");
                markdown.AppendLine();
            }

            // Description
            if (!string.IsNullOrEmpty(description))
            {
                markdown.AppendLine("## Description");
                markdown.AppendLine();
                markdown.AppendLine(description);
                markdown.AppendLine();
            }

            // Movements
            var movements = sequence["movements"] as JArray;
            if (movements != null && movements.Count > 0)
            {
                markdown.AppendLine("## Movements");
                markdown.AppendLine();
                foreach (var movement in movements)
                {
                    var movementName = movement["movement_name"]?.ToString();
                    var movementDesc = movement["description"]?.ToString();

                    markdown.AppendLine($"### {movementName}");
                    markdown.AppendLine();
                    if (!string.IsNullOrEmpty(movementDesc))
                    {
                        markdown.AppendLine(movementDesc);
                        markdown.AppendLine();
                    }

                    // Beats
                    var beats = movement["beats"] as JArray;
                    if (beats != null && beats.Count > 0)
                    {
                        markdown.AppendLine("#### Beats");
                        markdown.AppendLine();
                        for (int i = 0; i < beats.Count; i++)
                        {
                            var beat = beats[i];
                            var beatNumber = i + 1;
                            var beatName = beat["beat_name"]?.ToString();
                            var beatDesc = beat["description"]?.ToString();
                            var actor = beat["actor"]?.ToString();

                            markdown.AppendLine($"**Beat {beatNumber}: {beatName}**");
                            if (!string.IsNullOrEmpty(actor))
                                markdown.AppendLine($"- **Actor**: {actor}");
                            if (!string.IsNullOrEmpty(beatDesc))
                                markdown.AppendLine($"- **Description**: {beatDesc}");

                            // Handlers
                            var handlers = beat["handlers"] as JArray;
                            if (handlers != null && handlers.Count > 0)
                            {
                                markdown.AppendLine("- **Handlers**:");
                                foreach (var handler in handlers)
                                {
                                    var handlerName = handler["handler_name"]?.ToString();
                                    var handlerType = handler["handler_type"]?.ToString();
                                    markdown.AppendLine($"  - `{handlerName}` ({handlerType})");
                                }
                            }

                            // Events
                            var events = beat["events"] as JArray;
                            if (events != null && events.Count > 0)
                            {
                                markdown.AppendLine("- **Events**:");
                                foreach (var evt in events)
                                {
                                    var eventName = evt["event_name"]?.ToString();
                                    var eventType = evt["event_type"]?.ToString();
                                    markdown.AppendLine($"  - `{eventName}` ({eventType})");
                                }
                            }

                            // Data Access
                            var dataAccess = beat["data_access"] as JArray;
                            if (dataAccess != null && dataAccess.Count > 0)
                            {
                                markdown.AppendLine("- **Data Access**:");
                                foreach (var da in dataAccess)
                                {
                                    var operation = da["operation"]?.ToString();
                                    var entity = da["entity"]?.ToString();
                                    markdown.AppendLine($"  - {operation}: `{entity}`");
                                }
                            }

                            markdown.AppendLine();
                        }
                    }
                }
            }

            // External Systems
            var externalSystems = sequence["external_systems"] as JArray;
            if (externalSystems != null && externalSystems.Count > 0)
            {
                markdown.AppendLine("## External Systems");
                markdown.AppendLine();
                foreach (var system in externalSystems)
                {
                    var systemName = system["system_name"]?.ToString();
                    var systemType = system["system_type"]?.ToString();
                    var protocol = system["protocol"]?.ToString();

                    markdown.AppendLine($"### {systemName}");
                    if (!string.IsNullOrEmpty(systemType))
                        markdown.AppendLine($"- **Type**: {systemType}");
                    if (!string.IsNullOrEmpty(protocol))
                        markdown.AppendLine($"- **Protocol**: {protocol}");
                    markdown.AppendLine();
                }
            }

            // Acceptance Criteria
            var acceptanceCriteria = sequence["acceptance_criteria"] as JArray;
            if (acceptanceCriteria != null && acceptanceCriteria.Count > 0)
            {
                markdown.AppendLine("## Acceptance Criteria");
                markdown.AppendLine();
                foreach (var criterion in acceptanceCriteria)
                {
                    var given = criterion["given"]?.ToString();
                    var when = criterion["when"]?.ToString();
                    var then = criterion["then"]?.ToString();

                    markdown.AppendLine($"**Scenario**: {given}");
                    markdown.AppendLine($"- **Given**: {given}");
                    markdown.AppendLine($"- **When**: {when}");
                    markdown.AppendLine($"- **Then**: {then}");
                    markdown.AppendLine();
                }
            }

            // Test Coverage
            var testCoverage = sequence["test_coverage"];
            if (testCoverage != null)
            {
                markdown.AppendLine("## Test Coverage");
                markdown.AppendLine();
                var unitTests = testCoverage["unit_tests"]?.ToString();
                var integrationTests = testCoverage["integration_tests"]?.ToString();
                var e2eTests = testCoverage["e2e_tests"]?.ToString();

                if (!string.IsNullOrEmpty(unitTests))
                    markdown.AppendLine($"- **Unit Tests**: {unitTests}%");
                if (!string.IsNullOrEmpty(integrationTests))
                    markdown.AppendLine($"- **Integration Tests**: {integrationTests}%");
                if (!string.IsNullOrEmpty(e2eTests))
                    markdown.AppendLine($"- **End-to-End Tests**: {e2eTests}%");
                markdown.AppendLine();
            }

            // Drift Metrics
            var driftMetrics = sequence["drift_metrics"];
            if (driftMetrics != null)
            {
                markdown.AppendLine("## Drift Metrics");
                markdown.AppendLine();
                var driftScore = driftMetrics["drift_score"]?.ToString();
                var lastChecked = driftMetrics["last_checked"]?.ToString();

                if (!string.IsNullOrEmpty(driftScore))
                    markdown.AppendLine($"- **Drift Score**: {driftScore}");
                if (!string.IsNullOrEmpty(lastChecked))
                    markdown.AppendLine($"- **Last Checked**: {lastChecked}");
                markdown.AppendLine();
            }

            // Footer
            markdown.AppendLine("---");
            markdown.AppendLine();
            markdown.AppendLine($"*Generated from: `{Path.GetFileName(sequenceFilePath)}`*");
            markdown.AppendLine();
            markdown.AppendLine($"*Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");

            return markdown.ToString();
        }

        public void ConvertAndSave(string sequenceFilePath, string outputPath)
        {
            var markdown = ConvertSequenceToMarkdown(sequenceFilePath);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(outputPath, markdown, Encoding.UTF8);
        }
    }
}