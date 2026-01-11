using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

public class PatternService : IPatternService
{
    private readonly List<Pattern> _patterns;

    public PatternService()
    {
        _patterns = LoadSamplePatterns();
    }

    public Task<List<Pattern>> GetAllPatternsAsync()
    {
        return Task.FromResult(_patterns);
    }

    public Task<Pattern?> GetPatternBySlugAsync(string slug)
    {
        var pattern = _patterns.FirstOrDefault(p => p.Slug == slug);
        return Task.FromResult(pattern);
    }

    public Task<List<Pattern>> GetFeaturedPatternsAsync(int count = 6)
    {
        var featured = _patterns
            .OrderByDescending(p => p.ClarityScore)
            .Take(count)
            .ToList();
        return Task.FromResult(featured);
    }

    public Task<List<Pattern>> FilterPatternsAsync(FilterOptions filter)
    {
        var filtered = _patterns.AsEnumerable();

        if (filter.Industries.Any())
        {
            filtered = filtered.Where(p => p.Industries.Any(i => filter.Industries.Contains(i)));
        }

        if (filter.BrokenSignals.Any())
        {
            filtered = filtered.Where(p => p.BrokenSignals.Any(s => filter.BrokenSignals.Contains(s)));
        }

        if (filter.MaturityLevels.Any())
        {
            filtered = filtered.Where(p => filter.MaturityLevels.Contains(p.MaturityLevel));
        }

        filtered = filter.SortBy switch
        {
            "newest" => filtered.OrderByDescending(p => p.PublishedDate),
            "clarity" => filtered.OrderByDescending(p => p.ClarityScore),
            _ => filtered.OrderByDescending(p => p.ReferenceCount)
        };

        return Task.FromResult(filtered.ToList());
    }

    private List<Pattern> LoadSamplePatterns()
    {
        return new List<Pattern>
        {
            new Pattern
            {
                Id = "1",
                Slug = "support-ticket-escalation-backlog",
                Title = "Support Ticket Escalation Backlog",
                Problem = "Tickets escalate but get stuck in a limbo queue with unclear ownership",
                OrchestrationShift = "Define explicit ownership per escalation tier + auto-route by capacity",
                Hook = "Every support team knows the pain: escalated tickets that somehow become invisible.",
                ProblemDetail = @"When a support ticket gets escalated, it often falls into a gray zone where no one feels responsible.
The escalation queue grows, SLAs slip, and customers suffer. The issue isn't effort or skill—it's missing orchestration structure.",
                AsIsDiagram = @"sequenceDiagram
    participant C as Customer
    participant L1 as L1 Support
    participant L2 as L2 Queue
    participant L3 as L3 Engineers

    C->>L1: Reports issue
    L1->>L2: Escalates ticket
    Note over L2: Ticket sits in queue
    Note over L2: No clear owner
    L2-->>C: Auto-response only
    Note over C: Days pass, no update
    L3->>L2: Checks queue randomly
    L3->>C: Finally responds",
                OrchestratedDiagram = @"sequenceDiagram
    participant C as Customer
    participant L1 as L1 Support
    participant Router as Smart Router
    participant L2 as L2 Owner
    participant Monitor as SLA Monitor

    C->>L1: Reports issue
    L1->>Router: Escalates with context
    Router->>L2: Routes to owner by capacity + skill
    L2->>C: Acknowledges within SLA
    Monitor->>L2: Checks SLA status
    L2->>C: Provides resolution
    Monitor->>Router: Updates capacity model",
                DecisionPoint = @"The critical decision is: **who owns this escalation?**
Without clear ownership assignment and capacity-aware routing, escalations become nobody's problem.",
                Metrics = @"**Key Metrics:**
- Mean Time to First Response on escalated tickets
- Escalation queue depth
- % of escalations resolved within SLA
- Re-escalation rate",
                Checklist = @"- [ ] Define escalation tiers with explicit ownership
- [ ] Implement capacity-aware routing
- [ ] Set up SLA monitoring per tier
- [ ] Create escalation acknowledgment automation
- [ ] Establish re-escalation prevention process
- [ ] Build customer communication loop",
                Scorecard = new OrchestrationScorecard
                {
                    Ownership = 5,
                    TimeSLA = 4,
                    Capacity = 4,
                    Visibility = 4,
                    CustomerLoop = 3,
                    Escalation = 5,
                    Handoffs = 4,
                    Documentation = 3
                },
                ClosingInsight = "The backlog isn't a people problem—it's an orchestration gap. Fix ownership + routing, and the queue clears itself.",
                Industries = new List<string> { "Technology", "Healthcare", "Professional Services" },
                BrokenSignals = new List<string> { "Ownership", "Capacity", "Visibility" },
                MaturityLevel = "Mid",
                Components = new List<Component>
                {
                    new Component { Id = "1", Name = "Escalation Routing", Description = "Capacity-aware ticket routing system" },
                    new Component { Id = "2", Name = "Ownership Assignment", Description = "Clear owner identification per tier" },
                    new Component { Id = "3", Name = "SLA Monitoring", Description = "Automated SLA tracking and alerts" },
                    new Component { Id = "4", Name = "Customer Communication", Description = "Proactive update mechanism" }
                },
                PublishedDate = DateTime.Now.AddDays(-10),
                ClarityScore = 35,
                ReferenceCount = 142
            },
            new Pattern
            {
                Id = "2",
                Slug = "project-handoff-knowledge-loss",
                Title = "Project Handoff Knowledge Loss",
                Problem = "Teams complete projects but critical context evaporates during handoff to operations",
                OrchestrationShift = "Embed operational playbook creation into project completion criteria",
                Hook = "The project is done. The team celebrates. Six months later, nobody knows how it works.",
                ProblemDetail = @"Project teams build systems with deep contextual knowledge, then hand them off to operations with a brief transition meeting.
When issues arise, the operational team lacks the context to debug effectively. The original team has moved on.",
                AsIsDiagram = @"sequenceDiagram
    participant PT as Project Team
    participant Mgmt as Management
    participant Ops as Operations Team

    PT->>Mgmt: Project complete
    Mgmt->>Ops: Handoff meeting scheduled
    PT->>Ops: 1-hour knowledge transfer
    Note over PT: Team disbanded
    Note over Ops: Incomplete documentation
    Ops->>Ops: Struggles with production issues
    Ops->>Mgmt: Escalates for help
    Note over Mgmt: Original team unavailable",
                OrchestratedDiagram = @"sequenceDiagram
    participant PT as Project Team
    participant Playbook as Operational Playbook
    participant Ops as Operations Team
    participant Monitor as Health Monitor

    PT->>Playbook: Creates runbook during build
    PT->>Playbook: Documents decision rationale
    PT->>Ops: Structured handoff with playbook
    Ops->>Playbook: Reviews and validates
    Ops->>PT: Asks clarifying questions
    PT->>Playbook: Updates based on feedback
    Monitor->>Ops: Detects issue
    Ops->>Playbook: Follows troubleshooting guide
    Ops->>Monitor: Resolves using playbook",
                DecisionPoint = @"The critical decision is: **is the operational playbook a project deliverable?**
If it's optional, knowledge loss is guaranteed.",
                Metrics = @"**Key Metrics:**
- Mean Time to Resolution for post-handoff incidents
- Number of escalations back to project team
- Playbook completeness score
- Operations team confidence rating",
                Checklist = @"- [ ] Make operational playbook a required deliverable
- [ ] Include architecture decisions and rationale
- [ ] Document troubleshooting procedures
- [ ] Create runbook for common operations
- [ ] Conduct structured handoff with validation
- [ ] Schedule 30/60/90 day check-ins",
                Scorecard = new OrchestrationScorecard
                {
                    Ownership = 4,
                    TimeSLA = 3,
                    Capacity = 3,
                    Visibility = 5,
                    CustomerLoop = 2,
                    Escalation = 4,
                    Handoffs = 5,
                    Documentation = 5
                },
                ClosingInsight = "Knowledge transfer isn't a meeting—it's a structured artifact. Build the playbook as you build the system.",
                Industries = new List<string> { "Technology", "Finance", "Manufacturing" },
                BrokenSignals = new List<string> { "Visibility", "Handoffs" },
                MaturityLevel = "Advanced",
                Components = new List<Component>
                {
                    new Component { Id = "5", Name = "Playbook Framework", Description = "Standardized operational documentation template" },
                    new Component { Id = "6", Name = "Handoff Protocol", Description = "Structured knowledge transfer process" },
                    new Component { Id = "7", Name = "Decision Log", Description = "Architecture and design rationale tracking" }
                },
                PublishedDate = DateTime.Now.AddDays(-5),
                ClarityScore = 38,
                ReferenceCount = 98
            }
        };
    }
}
