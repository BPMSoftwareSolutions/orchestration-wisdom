# Orchestration Wisdom — Website Design Spec v1.0

## Purpose

Build a calm, authoritative, frictionless web experience that turns complex operational problems into **clear orchestration publications** and enables **composable B2B services commerce**:

* Companies **buy outcomes**
* Experts **sell capabilities**
* The platform uses **clarity + orchestration structure** to align them

The site must feel like a **reference system**, not a content farm.

---

## Product Principles

1. **Clarity before conversion**
   No popups, no dark patterns, no noisy ads.
2. **One-screen diagrams**
   Articles must render diagrams without scrolling where possible.
3. **Systems over blame**
   No company-specific internal claims; pattern-based, illustrative language.
4. **Composable solutions**
   Solutions decompose into components that map to capabilities.
5. **Trust is the moat**
   Ranking and matching cannot be pay-to-win.

---

## Primary Personas

### 1) Company Buyer (Operator / Leader)

* Needs: diagnose issue fast, understand levers, reduce risk, choose help
* Success: leaves with a plan (self-serve or assembled support)

### 2) Domain Expert / MSP / Consultant

* Needs: be discovered where they fit, show proof, avoid bad leads
* Success: matched to problems they can solve with minimal sales overhead

### 3) Researcher / Learner

* Needs: pattern library, principles, method
* Success: uses the platform as a reference

---

## Core User Journeys

### Journey A: Company → Self-Serve Value

1. Land on homepage → “Browse Patterns”
2. Filter patterns → open article
3. Read “As-Is vs Orchestrated” diagram + checklist
4. Export/share `ARTICLE.md` or print checklist
5. Optional: “Assess Fit” (quick signals) → recommended next steps

### Journey B: Company → Composed Solution Purchase

1. Article → “Build a Solution” (component list derived from the publication)
2. Choose providers per component (or “single provider” option)
3. Checkout (lightweight engagement request + scope confirmation)
4. Coordination hub created (shared orchestration spec + milestones)

### Journey C: Expert → Profile → Auto-Link to Publications

1. Create profile → define capabilities (structured)
2. Verification (capability test + rubric)
3. System auto-links expert to relevant publications/components
4. Expert receives qualified opportunities mapped to components

---

## Information Architecture

### Public Site

* `/` Home
* `/patterns` Pattern Library (filterable)
* `/patterns/{slug}` Pattern Article (publication page)
* `/principles` Orchestration Principles
* `/method` Methodology (OWS + HQO + diagram budgets)
* `/capabilities` Capability Catalog (public browsing)
* `/experts` Expert Directory (optional public view; primarily driven by article pages)
* `/about` About
* `/work-with-us` (optional; minimal, calm)
* `/legal` Terms, Privacy, Disclaimers

### Platform (Authenticated)

* Company

  * `/workspace`
  * `/assessments`
  * `/solutions/{id}` (solution builder + provider selection)
  * `/orders/{id}`
* Expert

  * `/profile`
  * `/capabilities`
  * `/verification`
  * `/opportunities`
* Admin

  * `/admin/publications`
  * `/admin/pattern-taxonomy`
  * `/admin/capability-tests`
  * `/admin/providers`
  * `/admin/reviews`

---

## Page-Level Design Specs

## 1) Home Page (`/`)

### Goals

* Instantly communicate category and credibility
* Route users into pattern discovery without friction

### Layout (above-the-fold)

* Headline: “Operational problems are orchestration problems.”
* Subheadline: “No-scroll diagrams + implementation-ready playbooks.”
* Primary CTA: “Browse Patterns”
* Secondary CTA: “How it Works”

### Below fold

* 3–6 featured patterns (not trending, not clickbait)
* “How value is realized” (1 short diagram thumbnail optional)
* Principles preview (3 bullets max)

### Prohibited

* Popups
* Newsletter gates
* Auto-playing media

---

## 2) Pattern Library (`/patterns`)

### Goals

* Fast discovery
* High signal filtering

### Filters

* Industry
* Problem Type (Backlog, Escalation, Capacity, SLA Failure, Handoffs)
* Broken Signals (Ownership, Time/SLA, Capacity, Visibility)
* Maturity Level (Early / Mid / Advanced)

### Pattern Cards

* Title
* 1-line “Problem”
* 1-line “Orchestration Shift”
* Badges: signals / industries
* “View Diagram” icon (no full thumbnails required)

### Sorting

* Most referenced
* Newest
* Highest clarity score (internal metric)

---

## 3) Pattern Article (`/patterns/{slug}`)

### Goals

* Deliver clarity and usable artifacts
* Offer optional next-step: self-serve or composed help

### Required Structure (mirrors ARTICLE.md)

* Hook
* Problem in one minute
* As-Is diagram (Mermaid)
* Orchestrated diagram (Mermaid)
* Decision point that matters
* Metrics & SLAs
* Checklist
* Orchestration scorecard
* Closing insight

### Functional Features

* Mermaid render with copy button (“Copy Mermaid”)
* Export options:

  * Download `ARTICLE.md`
  * Print checklist
  * Copy link to section anchors
* “Build a Solution” panel (see below)
* “Get help implementing” (routes to solution builder)

### “Build a Solution” Panel

Generated from article metadata:

* Components list (e.g., Ownership model, SLA triggers, Escalation routing, Customer comms loop)
* Each component links to:

  * explanation
  * verified providers
  * expected timeline
  * prerequisites

### Ads Policy on Article Pages

* **No third-party display ads**
* Allowed:

  * one small “Verified providers available for these components” module
  * one “Platform supports itself” disclosure line if referrals exist

---

## 4) Capability Catalog (`/capabilities`)

### Goals

* Let companies browse capabilities directly
* Establish language consistency (capabilities as “SKUs”)

### Capability Detail Includes

* What it implements (mapped orchestration components)
* Prerequisites
* Time-to-value range
* Scale fit
* What it does **not** solve
* Providers who offer it (verified first)

---

## 5) Expert Profile & Verification (Authenticated)

### Expert Profile Required Fields

* Capability list (structured)
* Industries
* Delivery modes (design, implementation, enablement)
* Integration constraints
* Evidence: case studies (anonymized), artifacts, references
* Availability windows

### Verification

* Capability test submissions (bounded diagram + checklist)
* HQO rubric scoring
* Badge levels:

  * Verified (passed rubric)
  * Proven (multiple successful outcomes)
  * Specialist (high fit in a narrow pattern set)

---

## 6) Solution Builder + Checkout (Authenticated)

### Solution Builder

* Input: pattern article + assessment answers
* Output:

  * components
  * recommended approach: self-serve / hybrid / external
  * provider selection per component

### Checkout / Engagement

* Not a shopping cart for hours
* It’s an **engagement request** with:

  * selected components
  * scope boundaries
  * target SLAs / outcomes
  * coordination expectations
  * disclosure of referral/fees

### Coordination Hub (post-checkout)

* Shared orchestration spec
* Milestones aligned to components
* Change control (scope drift)
* Outcome tracking

---

## Visual Design System

### Tone

* Calm, minimal, “reference-grade”
* Strong typography hierarchy, generous spacing

### Typography

* Highly readable sans-serif
* Code blocks styled cleanly
* Mermaid diagrams centered, ample padding

### Color

* 1 primary accent color (buttons, highlights)
* Neutral grays for UI
* No attention-grabbing reds/yellows except warnings

### Components

* PatternCard
* FilterBar
* MermaidPanel
* ComponentList
* ProviderBadge
* CapabilitySKU Card
* Checklist block (checkbox UI)
* Scorecard table

---

## Content & Formatting Standards

### Mermaid Constraints (UI-enforced where possible)

* ≤ 7 actors
* ≤ 18 steps total
* ≤ 2 alt blocks
* ≤ 8 steps per alt branch
* No nested alt
* Concise messages (verb-led)
* Notes limited and short

### Article Quality Gates

* HQO score ≥ 30/40
* No dimension < 3
* Includes ownership, SLA, escalation, capacity, customer loop

---

## Ads & Monetization Spec

### Phase 1 (MVP): No Ads

* Monetize later via verification + referrals + premium tooling
* Trust first

### Phase 2: Ethical Monetization (Allowed)

1. **Verification fees** (providers pay for evaluation, not ranking)
2. **Referral fee** on successful engagements (full disclosure)
3. **Premium tooling** (templates, audits, orchestration kits)

### Prohibited Monetization

* Pay-to-rank
* Third-party display ad networks
* Popups or interstitials
* “Sponsored pattern” content

### Disclosure Standard

All monetization must have a simple line on relevant pages:

* “Some providers support this platform through verification fees or referrals. Ranking is based on fit + proof, not payment.”

---

## SEO, Sharing, and Distribution

### SEO

* Static, crawlable pages for patterns
* Clean URLs, rich metadata
* Schema.org markup:

  * Article
  * Breadcrumbs
  * FAQ (optional)

### Sharing

* One-click copy link to diagram sections
* Social preview cards (title + 1-line insight)

---

## Performance and Accessibility

### Performance Targets

* Lighthouse ≥ 90 (Performance/Accessibility/SEO)
* Fast initial render (SSR/SSG)
* Mermaid lazy-load with placeholder skeleton

### Accessibility

* WCAG AA
* Keyboard navigable filters
* High contrast ratios
* Mermaid diagrams accompanied by short text summary (“Diagram Summary”) for screen readers

---

## Trust, Safety, and Legal

### Required Disclaimers

* Pattern-based modeling; not claims about specific internal operations
* No legal advice, no guarantees
* Provider work is independent; platform facilitates alignment

### Data Handling

* Minimal PII
* Clear privacy policy
* Secure auth for expert/company accounts
* Audit logs for admin actions

---

## Analytics & Measurement

### Core Metrics

* Time-to-pattern (seconds from landing to first pattern view)
* Pattern completion rate (scroll depth + section completion)
* Checklist copy/download rate
* “Build a Solution” conversion rate
* Match acceptance rate (company → provider)
* Outcome reporting adoption (post-engagement)

### Event Tracking (examples)

* `pattern_viewed`
* `mermaid_copied`
* `article_md_downloaded`
* `component_viewed`
* `provider_profile_opened`
* `solution_builder_started`
* `checkout_submitted`

---

## MVP Scope (Recommended)

### MVP Must Include

* Home + Pattern Library + Pattern Article pages
* Mermaid rendering
* `ARTICLE.md` download per pattern
* Pattern taxonomy + filters
* Minimal “Build a Solution” component list (even if providers are not live yet)

### MVP Should Not Include

* Full marketplace checkout
* Ratings/reviews
* Complex provider ranking algorithms
* Community features

Start with clarity + artifacts. Add commerce once trust exists.

---

## v2 / v3 Roadmap

### v2

* Expert onboarding + capability profiles
* Verification workflow + badges
* Provider linking into publications

### v3

* Solution composition + checkout
* Coordination hub
* Outcome benchmarking by pattern type

---

## Implementation Notes (Tech-Agnostic)

* Prefer static generation for publications
* Store patterns as:

  * `ARTICLE.md` (canonical human-readable)
  * OWS JSON (canonical machine-readable)
* Render Mermaid client-side with consistent styling constraints
* Separate “platform” features behind auth; keep the publication layer fast and public

