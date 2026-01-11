# Orchestration Wisdom - MVP Website

A Blazor Server application implementing the Orchestration Wisdom website design specification.

## Overview

This application transforms complex operational problems into clear orchestration publications using pattern-based insights, no-scroll diagrams, and implementation-ready playbooks.

## Features Implemented

### Core Pages
- **Home Page** (`/`) - Hero section with featured patterns and principles preview
- **Pattern Library** (`/patterns`) - Filterable pattern catalog with industry, signal, and maturity filters
- **Pattern Detail** (`/patterns/{slug}`) - Full pattern articles with:
  - Mermaid diagrams (As-Is and Orchestrated states)
  - Implementation checklists
  - Orchestration scorecards
  - Component breakdowns
  - Export to ARTICLE.md
  - Print checklist functionality
- **Principles** (`/principles`) - Core orchestration principles
- **Method** (`/method`) - Methodology documentation (OWS, HQO, diagram budgets)
- **About** (`/about`) - Platform information
- **Legal** (`/legal`) - Terms, disclaimers, and privacy

### Components
- **PatternCard** - Reusable pattern card with badges
- **MermaidDiagram** - Diagram rendering with copy functionality
- **Header** - Global navigation
- **Footer** - Site footer with links

### Design System
- Calm, minimal, reference-grade aesthetic
- Clean typography hierarchy
- CSS custom properties for theming
- Responsive layout
- WCAG AA accessibility support

### Technical Features
- Pattern service with filtering and sorting
- Mermaid.js integration for diagram rendering
- Client-side file download for ARTICLE.md export
- Sample pattern data (Support Ticket Escalation, Project Handoff Knowledge Loss)

## Running the Application

```bash
cd src/OrchestrationWisdom/OrchestrationWisdom
dotnet run
```

Navigate to `https://localhost:5001` (or the URL shown in console)

## Project Structure

```
OrchestrationWisdom/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── ReconnectModal.razor
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── Patterns.razor
│   │   ├── PatternDetail.razor
│   │   ├── Principles.razor
│   │   ├── Method.razor
│   │   ├── About.razor
│   │   └── Legal.razor
│   ├── Shared/
│   │   ├── Header.razor
│   │   ├── PatternCard.razor
│   │   └── MermaidDiagram.razor
│   └── App.razor
├── Models/
│   ├── Pattern.cs
│   ├── Capability.cs
│   └── FilterOptions.cs
├── Services/
│   ├── IPatternService.cs
│   └── PatternService.cs
├── wwwroot/
│   ├── app.css
│   └── mermaid-init.js
└── Program.cs
```

## Design Principles

1. **Clarity before conversion** - No popups, no dark patterns, no noisy ads
2. **One-screen diagrams** - Mermaid diagrams render without scrolling
3. **Systems over blame** - Pattern-based, not company-specific claims
4. **Composable solutions** - Solutions decompose into implementable components
5. **Trust is the moat** - Ranking cannot be pay-to-win

## Next Steps (Future Releases)

- Expert onboarding and verification workflow
- Capability catalog with provider matching
- Solution composition and checkout
- Coordination hub for engagements
- Outcome tracking and benchmarking

## Technology Stack

- .NET 10.0
- Blazor Server (SSR + Interactive Server Components)
- Mermaid.js for diagrams
- CSS Custom Properties for theming
