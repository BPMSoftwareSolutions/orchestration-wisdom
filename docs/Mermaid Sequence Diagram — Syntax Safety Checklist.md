# Mermaid Sequence Diagram — Syntax Safety Checklist

This checklist is a **portability + readability** guardrail for Mermaid sequence diagrams used across repos, docs sites, and tooling pipelines.  
It focuses on: **(1) render safety**, **(2) semantic clarity**, and **(3) consistent UX-orchestration storytelling**.

---

## 0) Guiding Principle: Movements & Beats (Semantic Safety)

### Movement Definition
A **Movement** begins with a **user action** and ends when the user has a **closed UX loop** (they can observe the system state and know the next action).

✅ A Movement is “complete” only when the user’s uncertainty is resolved (even if the state is “in progress”).

### Beat Definition
A **Beat** is a **user-meaningful orchestration unit** — a cohesive chunk of interaction that produces a recognizable outcome in the UX story.

✅ Beats should not be split into “micro-steps” unless those steps create independent decision points or distinct UX outcomes.

---

## 1) NEW Key Principle: Closure Beat Span Rule (Span-by-Impact)

### ✅ Closure Beat Span Rule
If a beat **closes the UX loop** for a movement (i.e., it resolves the user’s uncertainty and establishes observable truth), then its **Beat note should span all participants impacted by that closure**, even if the beat’s internal messages touch them indirectly.

**Why:** It produces cleaner diagrams and matches how humans reason about end-to-end outcomes.

#### When to use a full-span beat note
Use `Note over <first>,<last>` for a beat when:
- It is the **final beat** of the movement, AND
- It includes **status confirmation + state update + UX observation**, AND
- Its outcome affects multiple participants across the boundary graph

**Example:**  
Beat: “RMS confirms receipt (queue status update + UX closure)”  
Even if the messages hop through MobileServer + Queue + MobileClient, the outcome impacts Officer ↔ RMS end-to-end.

#### When NOT to use a full-span beat note
Do **not** full-span a beat note when:
- The beat is a local/internal step with no user-facing resolution
- The beat affects only a small subset of participants
- The beat is not a closure/UX resolution unit

---

## 2) Diagram Header & Structure (Syntax Safety)

- ✅ Start with `sequenceDiagram`
- ✅ Use `autonumber` only if you want stable step references
- ✅ Declare all `actor` / `participant` entries before the first message
- ✅ Ensure participant IDs are unique within the diagram

---

## 3) Participants & Naming (Portability)

- ✅ Keep participant IDs simple: `MobileServer`, `Queue`, `RMS`
- ✅ Labels may include emojis, but beware of environments that render them poorly
  - ✅ Emojis are typically safe syntactically
  - ⚠️ Emojis may cause layout issues in some renderers

---

## 4) Notes (Movement & Beat Notes)

### Movement Notes
- ✅ Every movement must include a movement note:
  - `Note over <first>,<last>: Movement N — ...`
- ✅ Movement notes must span the **entire participant list**

### Beat Notes
- ✅ Every beat must include a beat note
- ✅ Beat notes generally span **only the participants involved in that beat**
- ✅ **Exception:** apply the **Closure Beat Span Rule** for UX-closure beats
- ✅ Avoid sub-beat labels like `5.1`, `8A`, etc. unless absolutely required

---

## 5) Control Blocks (alt/opt/loop Safety)

- ✅ `alt` blocks must always end with `end`
- ✅ Use `else` only inside `alt`
- ✅ Avoid deeply nested control blocks unless necessary
- ✅ If nesting, keep indentation consistent and verify `end` count

---

## 6) Message Arrows & Valid Syntax

- ✅ Use supported arrows:
  - `->>` request/call
  - `-->>` return/response
  - `->` / `-->` are okay but less expressive
- ✅ Ensure messages reference valid participants only

---

## 7) Text Safety (Avoid Renderer Edge Cases)

### Prefer “Safe Mode” payload formatting
Some markdown pipelines and doc generators handle braces oddly.

- ✅ Prefer parentheses for payload summaries:
  - `POST /confirm (state=Merged, reportId, version=Vn)`
- ⚠️ Avoid curly braces if you’ve seen rendering issues:
  - `{ state="Merged" }` (usually fine in Mermaid, but can be fragile in certain pipelines)

### Quotes
- ✅ Prefer straight quotes `"..."` over smart quotes `“...”`
- ✅ Keep text short; long labels wrap differently across renderers

---

## 8) Closed UX Loop Requirement (Movement Completeness)

✅ Every movement diagram must end with a **closed UX loop**:
- a **status observation** (typically a GET) and/or UI message
- the user sees a definitive state:
  - `DraftSaved`, `DraftReceived`, `Merged`, `Rejected`, `Returned`, `Accepted`, `Validating`, etc.
- the user knows the **next action**
  - continue drafting, fix errors, resubmit, await review, done

**Rule of thumb:**  
If the movement ends and the user still asks “what’s happening?” → the UX loop is not closed.

---

## 9) Common Failure Modes (Quick Scan)

- ❌ Note references a participant not declared
- ❌ Missing `end` for an `alt`
- ❌ Beat notes too granular (micro-step explosion)
- ❌ No closed UX loop at the end of a movement
- ❌ Closure beat note spans too narrowly (diagram looks fragmented)
  - ✅ Fix by applying the **Closure Beat Span Rule**

---

## 10) Pre-Commit Checklist (Recommended)

Before committing a diagram:
1. ✅ Renders successfully in your target environment(s)
2. ✅ Movement note spans full participant list
3. ✅ Beats are user-meaningful (not micro-steps)
4. ✅ Closure beat spans full participant set (when applicable)
5. ✅ Movement ends with closed UX loop
6. ✅ No syntax hazards (missing end, unknown participant, weird braces)

