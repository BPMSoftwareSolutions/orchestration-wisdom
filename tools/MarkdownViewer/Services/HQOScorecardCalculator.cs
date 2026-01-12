using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchestrationWisdom.Tools.MarkdownViewer.Services
{
    /// <summary>
    /// Calculates the High Quality Orchestration (HQO) scorecard for patterns.
    /// Ensures patterns meet minimum quality standards before publication.
    /// </summary>
    public interface IHQOScorecardCalculator
    {
        /// <summary>
        /// Calculate HQO scorecard from pattern dimensions.
        /// Minimum threshold: Total ≥30/40, no dimension <3
        /// </summary>
        HQOScorecard CalculateScorecard(HQODimensions dimensions);

        /// <summary>
        /// Validate that a scorecard meets publication requirements.
        /// </summary>
        (bool IsValid, List<string> Issues) ValidateScorecard(HQOScorecard scorecard);
    }

    public class HQODimensions
    {
        /// <summary>Is it clear who owns each step? (1-5)</summary>
        public int Ownership { get; set; }

        /// <summary>Are time constraints and SLA visible? (1-5)</summary>
        public int TimeSLA { get; set; }

        /// <summary>Does the system check capacity before routing? (1-5)</summary>
        public int Capacity { get; set; }

        /// <summary>Can all parties see the current state? (1-5)</summary>
        public int Visibility { get; set; }

        /// <summary>Is the customer kept informed? (1-5)</summary>
        public int CustomerLoop { get; set; }

        /// <summary>Are escalation paths clear? (1-5)</summary>
        public int Escalation { get; set; }

        /// <summary>Are context and responsibilities clearly transferred? (1-5)</summary>
        public int Handoffs { get; set; }

        /// <summary>Is the pattern fully documented? (1-5)</summary>
        public int Documentation { get; set; }
    }

    public class HQOScorecard
    {
        public HQODimensions Dimensions { get; set; }
        public int Total { get; set; }
        public bool MeetsThreshold { get; set; }
        public DateTime CalculatedAt { get; set; }

        public Dictionary<string, int> ToDictionary()
        {
            return new Dictionary<string, int>
            {
                { "Ownership", Dimensions.Ownership },
                { "TimeSLA", Dimensions.TimeSLA },
                { "Capacity", Dimensions.Capacity },
                { "Visibility", Dimensions.Visibility },
                { "CustomerLoop", Dimensions.CustomerLoop },
                { "Escalation", Dimensions.Escalation },
                { "Handoffs", Dimensions.Handoffs },
                { "Documentation", Dimensions.Documentation }
            };
        }
    }

    /// <summary>
    /// Default HQO scorecard calculator.
    /// </summary>
    public class HQOScorecardCalculator : IHQOScorecardCalculator
    {
        private const int MinimumTotal = 30;
        private const int MinimumPerDimension = 3;
        private const int MaxPerDimension = 5;
        private const int MaxTotal = 40;

        public HQOScorecard CalculateScorecard(HQODimensions dimensions)
        {
            var total = dimensions.Ownership
                + dimensions.TimeSLA
                + dimensions.Capacity
                + dimensions.Visibility
                + dimensions.CustomerLoop
                + dimensions.Escalation
                + dimensions.Handoffs
                + dimensions.Documentation;

            var (isValid, _) = ValidateScorecard(new HQOScorecard
            {
                Dimensions = dimensions,
                Total = total,
                CalculatedAt = DateTime.UtcNow
            });

            return new HQOScorecard
            {
                Dimensions = dimensions,
                Total = total,
                MeetsThreshold = isValid,
                CalculatedAt = DateTime.UtcNow
            };
        }

        public (bool IsValid, List<string> Issues) ValidateScorecard(HQOScorecard scorecard)
        {
            var issues = new List<string>();

            // Check total
            if (scorecard.Total < MinimumTotal)
            {
                issues.Add($"Total score {scorecard.Total} is below minimum {MinimumTotal}");
            }

            if (scorecard.Total > MaxTotal)
            {
                issues.Add($"Total score {scorecard.Total} exceeds maximum {MaxTotal}");
            }

            // Check individual dimensions
            var dims = scorecard.Dimensions;
            var dimensionNames = new[]
            {
                ("Ownership", dims.Ownership),
                ("TimeSLA", dims.TimeSLA),
                ("Capacity", dims.Capacity),
                ("Visibility", dims.Visibility),
                ("CustomerLoop", dims.CustomerLoop),
                ("Escalation", dims.Escalation),
                ("Handoffs", dims.Handoffs),
                ("Documentation", dims.Documentation)
            };

            foreach (var (name, value) in dimensionNames)
            {
                if (value < MinimumPerDimension)
                {
                    issues.Add($"{name} score {value} is below minimum {MinimumPerDimension}");
                }

                if (value > MaxPerDimension)
                {
                    issues.Add($"{name} score {value} exceeds maximum {MaxPerDimension}");
                }
            }

            return (issues.Count == 0, issues);
        }
    }
}
