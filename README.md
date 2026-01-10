# orchestration-wisdom

**High-quality orchestration, distilled and distributed with clarity.**

`orchestration-wisdom` is a framework, schema, and content engine for turning messy, real-world operational problems into **clear, concise orchestration diagrams and articles**—designed to *fit on one screen*, tell the truth about system behavior, and show what a better design looks like.

This repository exists to help humans and AI **spread operational wisdom**, not opinions.

---

## What This Repo Is

This repo provides:

- A **JSON schema (OWS – Orchestration Wisdom Schema)**  
  that enforces high-quality orchestration thinking
- A **repeatable process** for modeling:
  - the *current state* (as-is)
  - the *orchestrated state* (to-be)
  - the *decision points* (`alt`) that actually matter
- Deterministic generation of:
  - Mermaid sequence diagrams (no scroll, no clutter)
  - Publish-ready articles
  - Implementation checklists
- Built-in **quality gates** so weak orchestration never ships

This is not a diagramming toy.  
It is **process intelligence encoded**.

---

## The Core Idea

Most operational problems are not caused by bad people or bad tools.

They are caused by:

- unclear ownership
- invisible time (no SLA clocks)
- unmanaged capacity constraints
- escalation as an afterthought
- customers left out of the loop

**Orchestration wisdom** makes these failures visible—and shows how small, strategic design choices transform outcomes.

---

## What Makes This Different

### 1. Orchestration-First (Not Flowcharts)
Every diagram must explicitly model:
- ownership
- routing
- SLAs
- escalation
- customer visibility
- capacity reality
- observability (events + state)

If it doesn’t orchestrate, it doesn’t pass.

---

### 2. Contrast Through `alt`
Every meaningful diagram includes `alt` blocks that show:

- what *actually* happens today
- what *could* happen with intentional orchestration

No hypothetical fluff.  
Just system behavior.

---

### 3. No-Scroll Visuals (Hard Constraint)
Diagrams are intentionally bounded:
- ≤ 7 actors
- ≤ ~18 steps total
- ≤ 2 `alt` blocks
- short messages
- minimal notes

If it doesn’t fit on one screen, it gets redesigned.

---

### 4. AI-Friendly, Human-Readable
JSON is the single source of truth.

From one schema instance, AI can generate:
- Mermaid diagrams
- Articles
- Checklists
- Rubric scorecards

Humans can still read and reason about every piece.

---

## Repository Structure (Recommended)

```text
orchestration-wisdom/
├── schema/
│   ├── orchestration-wisdom.schema.json      # OWS v1.x (single source of truth)
│   └── examples/
│       ├── warranty-escalation.json
│       └── capacity-backlog.json
│
├── generators/
│   ├── mermaid/                              # Deterministic Mermaid renderer
│   ├── article/                              # Article generation templates
│   └── checklist/                            # Implementation checklist generator
│
├── quality/
│   ├── orchestration-rubric.md               # HQO rubric (human readable)
│   └── diagram-budget.md                     # Visual size constraints
│
├── docs/
│   ├── patterns/                             # Published pattern articles
│   └── playbooks/
│
└── README.md
