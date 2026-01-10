Got it. The clean way to guarantee “no scrolling” is to make **diagram size a first-class constraint** in the schema *and* enforce it with a **quality gate** (your agent must revise until it passes).

Below is a practical, enforceable approach you can drop into OWS v1.1.

---

## 1) Add a Diagram Budget to the Schema

Create a `diagramBudget` object that sets hard limits for:

* max participants
* max steps (per diagram and per branch)
* max `alt` blocks
* max message length
* max note length
* optional “page fit” target (approx.)

### Schema Fragment: `diagramBudget`

```json
{
  "diagramBudget": {
    "type": "object",
    "additionalProperties": false,
    "required": [
      "maxActors",
      "maxTotalSteps",
      "maxStepsPerBranch",
      "maxAltBlocks",
      "maxMessageChars",
      "maxNoteChars",
      "maxNotes"
    ],
    "properties": {
      "maxActors": { "type": "integer", "minimum": 3, "maximum": 9, "default": 7 },
      "maxTotalSteps": { "type": "integer", "minimum": 8, "maximum": 40, "default": 18 },
      "maxStepsPerBranch": { "type": "integer", "minimum": 3, "maximum": 20, "default": 8 },
      "maxAltBlocks": { "type": "integer", "minimum": 1, "maximum": 4, "default": 2 },
      "maxNotes": { "type": "integer", "minimum": 0, "maximum": 8, "default": 3 },
      "maxMessageChars": { "type": "integer", "minimum": 20, "maximum": 90, "default": 56 },
      "maxNoteChars": { "type": "integer", "minimum": 20, "maximum": 140, "default": 90 },
      "targetViewport": {
        "type": "string",
        "enum": ["single_screen", "single_slide", "no_scroll"],
        "default": "no_scroll",
        "description": "Intent for how compact the diagram must be."
      },
      "layoutGuidance": {
        "type": "array",
        "items": {
          "type": "string",
          "enum": [
            "prefer_short_actor_labels",
            "prefer_short_messages",
            "avoid_long_notes",
            "avoid_nested_alt",
            "avoid_more_than_one_return_chain",
            "limit_internal_steps",
            "use_two_phase_structure"
          ]
        },
        "default": ["prefer_short_messages", "avoid_nested_alt", "use_two_phase_structure"]
      }
    }
  }
}
```

**Why these numbers?**
In practice, Mermaid sequence diagrams start feeling “scroll-y” past ~18–22 steps, especially with notes and long messages. Keeping branches to ~6–8 steps makes `alt` readable.

---

## 2) Enforce It With a “Diagram Size Scorecard”

Have the agent compute a scorecard and revise if any hard limit is exceeded.

### Required Output Block (agent must return)

```json
{
  "diagramSizeScorecard": {
    "type": "object",
    "additionalProperties": false,
    "required": [
      "actorCount",
      "totalSteps",
      "altBlockCount",
      "maxStepsInAnyBranch",
      "noteCount",
      "maxMessageCharsObserved",
      "maxNoteCharsObserved",
      "passesBudget"
    ],
    "properties": {
      "actorCount": { "type": "integer" },
      "totalSteps": { "type": "integer" },
      "altBlockCount": { "type": "integer" },
      "maxStepsInAnyBranch": { "type": "integer" },
      "noteCount": { "type": "integer" },
      "maxMessageCharsObserved": { "type": "integer" },
      "maxNoteCharsObserved": { "type": "integer" },
      "passesBudget": { "type": "boolean" },
      "violations": { "type": "array", "items": { "type": "string" } }
    }
  }
}
```

This turns “concise” into something measurable.

---

## 3) Add a “Compression Strategy” So the Agent Knows *How* to Shrink

When it violates the budget, the agent needs a deterministic way to compress.

Add `compressionRules` (ordered priority):

```json
{
  "compressionRules": {
    "type": "array",
    "minItems": 5,
    "items": {
      "type": "string",
      "enum": [
        "merge_adjacent_steps_same_actor_pair",
        "replace_two_steps_with_one_state_change",
        "convert_detail_to_note",
        "remove_nonessential_returns",
        "shorten_messages_to_verbs",
        "collapse_system_internals_into_single_internal_step",
        "reduce_alt_branches_to_two",
        "reduce_actors_by_merging_support_roles",
        "extract_secondary_flow_into_followup_diagram"
      ]
    },
    "default": [
      "shorten_messages_to_verbs",
      "merge_adjacent_steps_same_actor_pair",
      "collapse_system_internals_into_single_internal_step",
      "reduce_alt_branches_to_two",
      "extract_secondary_flow_into_followup_diagram"
    ]
  }
}
```

The last rule is key: **if it can’t fit, it becomes a second diagram**.

---

## 4) Best Practice: Use a Two-Diagram Structure Instead of One Giant One

To keep “no scroll,” standardize the article format to:

### Diagram A — “As-Is vs Orchestrated (Core Loop)”

* 5–7 actors
* 12–18 steps total
* 1 alt decision point

### Diagram B — “Deep Dive: One Decision Point”

* only the tricky branch (e.g., capacity unavailable)
* 10–16 steps
* 1 alt (optional)

This avoids the “all-in-one spaghetti” trap while keeping each visual clean.

---

## 5) Recommended Default Budgets (What I’d Start With)

For your exact use case (warranty escalation):

* `maxActors`: **7**
* `maxTotalSteps`: **18**
* `maxAltBlocks`: **2**
* `maxStepsPerBranch`: **8**
* `maxMessageChars`: **56**
* `maxNoteChars`: **90**
* `maxNotes`: **3**
* `avoid_nested_alt`: **true** (enforced via layoutGuidance)

---

## 6) Add One More Constraint: “Single-Screen Readability Rules”

These are human factors that matter:

* Actor labels: **≤ 18 chars** (emoji + short name)
* Messages: start with a **verb** (“Create case”, “Start SLA”, “Escalate”)
* Notes: only for **movement/beat titles** or a single key insight
* No nested `alt`
* No more than **one** `return` chain per branch (returns can clutter)

You can enforce label length too if you want—same pattern as `maxMessageChars`.

---

## If you want, I’ll produce the full OWS v1.1 schema

Including:

* `diagramBudget`
* `compressionRules`
* `diagramSizeScorecard` required output
* step fields for `emits/updates`
* HQO rubric + pass/fail gate

Just say: **“Generate the full OWS v1.1 schema with diagramBudget”** and I’ll output the complete JSON Schema in one block.
