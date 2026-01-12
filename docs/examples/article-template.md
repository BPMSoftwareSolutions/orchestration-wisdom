# Orchestration Wisdom Article Template

Use this template to generate consistent, high-quality orchestration patterns. Follow the structure below and include all sections.

## 1. Hook
**Purpose**: Grab attention with a specific, relatable problem statement.
**Length**: 2-3 sentences
**Example**: "Escalated tickets that somehow become invisible"

Write your hook here:

## 2. Problem Detail
**Purpose**: Explain the broken signals and why this matters.
**Length**: 3 paragraphs (150-200 words total)
**Include**: 
- What's broken (Ownership, Visibility, Time/SLA, etc.)
- Why it matters operationally
- Real-world impact (metrics when available)

Write your problem detail here:

## 3. As-Is Diagram (Broken)
**Format**: Mermaid sequence diagram
**Budget**: ≤7 actors, ≤18 steps, ≤2 alt blocks
**Purpose**: Show the current broken workflow

\`\`\`mermaid
sequenceDiagram
  participant C as Customer
  participant L1 as L1 Support
  participant L2 as L2 Expert
  participant Queue as Escalation Queue
  
  Note over C,Queue: As-Is: Ticket gets lost in escalation
  C->>L1: Submit complex issue
  L1->>L1: Unable to resolve
  L1->>Queue: Escalate without ownership
  Queue->>Queue: Queued but no owner assigned
  L2->>L2: Finds ticket after 3 days
  L2->>C: ✗ Slow response, SLA breach
\`\`\`

## 4. Orchestrated Diagram (Improved)
**Format**: Mermaid sequence diagram
**Budget**: ≤7 actors, ≤18 steps, ≤2 alt blocks
**Purpose**: Show the improved workflow with orchestration changes

\`\`\`mermaid
sequenceDiagram
  participant C as Customer
  participant L1 as L1 Support
  participant Router as Smart Router
  participant Monitor as SLA Monitor
  participant L2 as L2 Expert
  
  Note over C,L2: Orchestrated: Clear ownership & monitoring
  C->>L1: Submit complex issue
  L1->>Router: Escalate with context
  Router->>Router: Check L2 availability & capacity
  Router->>L2: Assign + notify immediately
  L2->>Monitor: Start SLA tracking (4hr response)
  Monitor->>Monitor: Watch SLA deadline
  L2->>C: ✓ Prompt response, documented
\`\`\`

## 5. Decision Point
**Purpose**: Highlight the key decision that changes outcomes.
**Format**: Single question or decision node
**Example**: "Who owns this escalation?"

Write your decision point here:

## 6. Metrics & KPIs
**Purpose**: Define how to measure success.
**Include**: 3-5 metrics with current vs. target values

Write your metrics here:

## 7. Implementation Checklist
**Purpose**: Actionable steps to implement this pattern.
**Format**: 5-8 numbered items
**Requirement**: Each item must be specific and testable

1. Write checklist items here
2. Make them specific and measurable
3. Ensure they're implementable in 1-2 weeks

## 8. Common Pitfalls
**Purpose**: Warn implementers about failure modes.
**Format**: 3-5 bullet points with brief explanation

- Pitfall 1: Description
- Pitfall 2: Description

## 9. Closing Insight
**Purpose**: End with a memorable takeaway.
**Length**: 1-2 sentences
**Example**: "The backlog isn't a people problem—it's an orchestration gap"

Write your closing insight here:

---

## HQO Scorecard Template

Score each dimension from 1-5:

- **Ownership**: Is it clear who owns each step? (1-5)
- **Time/SLA**: Are time constraints and SLA visible? (1-5)
- **Capacity**: Does the system check capacity before routing? (1-5)
- **Visibility**: Can all parties see the current state? (1-5)
- **Customer Loop**: Is the customer kept informed? (1-5)
- **Escalation**: Are escalation paths clear? (1-5)
- **Handoffs**: Are context and responsibilities clearly transferred? (1-5)
- **Documentation**: Is the pattern fully documented? (1-5)

**Total: _/40** (Publish when ≥30/40)

---

## Tips for High-Quality Patterns

✓ Use specific, measurable examples
✓ Make diagrams as simple as possible (use budget constraints)
✓ Include at least one metric that proves the pattern works
✓ Test your checklist—is it actually doable in 1-2 weeks?
✓ Be honest about when this pattern DOESN'T apply
