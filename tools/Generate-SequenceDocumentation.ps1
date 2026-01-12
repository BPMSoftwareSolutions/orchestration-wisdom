<#
.SYNOPSIS
    Generate detailed sequence documentation from canonical JSON files

.DESCRIPTION
    Reads sequence JSON files and generates comprehensive markdown documentation
    with movements, beats, handlers, tests, and acceptance criteria.

    This ensures all sequence documentation is auto-generated consistently.

.PARAMETER SequenceFile
    Path to the sequence JSON file to generate documentation for

.PARAMETER OutputFolder
    Output folder for generated markdown (default: reports/sequences/)

.PARAMETER GenerateHtml
    Switch to generate HTML files in addition to markdown files

.EXAMPLE
    .\Tools\Generate-SequenceDocumentation.ps1 -SequenceFile schemas/sequences/dispatch-call-assignment.sequence.json

.EXAMPLE
    .\Tools\Generate-SequenceDocumentation.ps1 -GenerateHtml

.EXAMPLE
    .\Tools\Generate-SequenceDocumentation.ps1 -SequenceFile schemas/sequences/dispatch-call-assignment.sequence.json -GenerateHtml
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$SequenceFile,

    [Parameter(Mandatory=$false)]
    [string]$OutputFolder = "reports/sequences",

    [Parameter(Mandatory=$false)]
    [switch]$GenerateHtml = $false
)

$ErrorActionPreference = "Stop"
$RepoRoot = $PSScriptRoot | Split-Path -Parent

# Ensure output folder exists
$outputPath = Join-Path $RepoRoot $OutputFolder
if (-not (Test-Path $outputPath)) {
    New-Item -Path $outputPath -ItemType Directory -Force | Out-Null
}

function Convert-MarkdownToHtml {
    param([string]$Markdown, [string]$Title)

    # Process line by line for better control
    $lines = $Markdown -split "`r?`n"
    $htmlLines = @()
    $inList = $false
    $inPreBlock = $false
    $inToc = $false
    $preBlockLines = @()

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        $trimmedLine = $line.Trim()

        # Detect start of ASCII art / preformatted blocks
        # Look for patterns like: lines with lots of spaces, forward slashes, backslashes, underscores, or box-drawing
        $isBlockquote = $trimmedLine -match '^>'
        $isAsciiArtLine = ($line -match '^\s{8,}[/\\]' -or  # Leading spaces + slashes (pyramid lines)
                          $line -match '[/\\]{2,}' -or       # Multiple slashes/backslashes
                          $line -match '_{5,}' -or           # Multiple underscores
                          $line -match '\|.*\|' -or          # Pipe characters (box drawing)
                          $line -match '^\s+\d+%') -and      # Percentage indicators (like "90% Unit")
                          -not $isBlockquote                 # Exclude blockquote lines

        # If we detect ASCII art and we're not in a pre block, start one
        if ($isAsciiArtLine -and -not $inPreBlock -and -not $inList) {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }
            $inPreBlock = $true
            $preBlockLines = @($line)
            continue
        }

        # If we're in a pre block, collect lines until we hit a clear end marker
        if ($inPreBlock) {
            # Check if we're at the end of the pre block
            # End if: next line is a header, or we have 2+ consecutive empty lines, or a clear text paragraph
            $isEndOfPreBlock = $false

            if ($trimmedLine -match '^#{1,4} ' -or $trimmedLine -match '^\*\*') {
                # Header or bold text indicates end of ASCII art
                $isEndOfPreBlock = $true
            } elseif ($trimmedLine -eq '' -and $i + 1 -lt $lines.Count) {
                # Check if next line is also empty or is regular text
                $nextLine = $lines[$i + 1].Trim()
                $nextLineIsAscii = $lines[$i + 1] -match '^\s{8,}[/\\]' -or $lines[$i + 1] -match '[/\\]{2,}' -or $lines[$i + 1] -match '_{5,}' -or $lines[$i + 1] -match '\|.*\|' -or $lines[$i + 1] -match '^\s+\d+%'
                if ($nextLine -eq '' -or ($nextLine -ne '' -and -not $nextLineIsAscii)) {
                    $isEndOfPreBlock = $true
                }
            }

            if ($isEndOfPreBlock) {
                # End the pre block
                $htmlLines += '<pre>'
                # Convert special Unicode characters to HTML entities for proper display
                $preContent = ($preBlockLines -join "`n")
                $preContent = $preContent -replace ([char]0x2190), '&larr;'  # Left arrow
                $preContent = $preContent -replace ([char]0x2192), '&rarr;'  # Right arrow
                $htmlLines += $preContent
                $htmlLines += '</pre>'
                $inPreBlock = $false
                $preBlockLines = @()
                # Fall through to process current line normally
            } else {
                # Continue collecting pre block lines
                $preBlockLines += $line
                continue
            }
        }

        # Check if this is a list item
        if ($trimmedLine -match '^- (.+)$') {
            # Start list if not already in one
            if (-not $inList) {
                $htmlLines += '<ul>'
                $inList = $true
            }

            # Extract list item content and apply inline formatting
            $content = $matches[1]
            $content = $content -replace '\*\*(.+?)\*\*', '<strong>$1</strong>'
            $content = $content -replace '`([^`]+)`', '<code>$1</code>'
            $content = $content -replace '\[([^\]]+)\]\(([^\)]+)\)', '<a href="$2">$1</a>'

            $htmlLines += "  <li>$content</li>"

            # Check if next line is not a list item - close list
            if ($i + 1 -lt $lines.Count) {
                $nextLine = $lines[$i + 1].Trim()
                if (-not ($nextLine -match '^- ') -and $nextLine -ne '') {
                    $htmlLines += '</ul>'
                    $inList = $false
                }
            }
            else {
                # Last line - close list
                $htmlLines += '</ul>'
                $inList = $false
            }
        }
        # Headers
        elseif ($trimmedLine -match '^#### (.+)$') {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }
            if ($inToc) { $htmlLines += '</nav>'; $inToc = $false }
            $htmlLines += "<h4>$($matches[1])</h4>"
        }
        elseif ($trimmedLine -match '^### (.+)$') {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }
            if ($inToc) { $htmlLines += '</nav>'; $inToc = $false }
            $htmlLines += "<h3>$($matches[1])</h3>"
        }
        elseif ($trimmedLine -match '^## (.+)$') {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }
            if ($inToc) { $htmlLines += '</nav>'; $inToc = $false }

            $headerText = $matches[1]
            $htmlLines += "<h2>$headerText</h2>"

            # Check if this is the Table of Contents header
            if ($headerText -eq "Table of Contents") {
                $inToc = $true
                $htmlLines += '<nav>'
            }
        }
        elseif ($trimmedLine -match '^# (.+)$') {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }
            if ($inToc) { $htmlLines += '</nav>'; $inToc = $false }
            $htmlLines += "<h1>$($matches[1])</h1>"
        }
        # Blockquotes
        elseif ($trimmedLine -match '^> (.+)$') {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }
            $content = $matches[1]
            $content = $content -replace '\*\*(.+?)\*\*', '<strong>$1</strong>'
            $content = $content -replace '\[([^\]]+)\]\(([^\)]+)\)', '<a href="$2">$1</a>'
            $htmlLines += "<blockquote>$content</blockquote>"
        }
        # Horizontal rules
        elseif ($trimmedLine -eq '---') {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }
            $htmlLines += '<hr>'
        }
        # Empty lines
        elseif ($trimmedLine -eq '') {
            if ($inList) {
                $htmlLines += '</ul>'
                $inList = $false
            }
            $htmlLines += ''
        }
        # Regular paragraphs
        else {
            if ($inList) { $htmlLines += '</ul>'; $inList = $false }

            # Apply inline formatting
            $content = $line
            $content = $content -replace '\*\*(.+?)\*\*', '<strong>$1</strong>'
            $content = $content -replace '`([^`]+)`', '<code>$1</code>'
            $content = $content -replace '\[([^\]]+)\]\(([^\)]+)\)', '<a href="$2">$1</a>'
            $content = $content -replace '_([^_]+)_', '<em>$1</em>'

            $htmlLines += "<p>$content</p>"
        }
    }

    # Close any open list
    if ($inList) {
        $htmlLines += '</ul>'
    }

    # Close any open pre block
    if ($inPreBlock -and $preBlockLines.Count -gt 0) {
        $htmlLines += '<pre>'
        # Convert special Unicode characters to HTML entities for proper display
        $preContent = ($preBlockLines -join "`n")
        $preContent = $preContent -replace ([char]0x2190), '&larr;'  # Left arrow
        $preContent = $preContent -replace ([char]0x2192), '&rarr;'  # Right arrow
        $htmlLines += $preContent
        $htmlLines += '</pre>'
    }

    $html = $htmlLines -join "`n"

    # Create full HTML document
    $htmlDoc = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>$Title</title>
    <style>
        /* Light mode (default) */
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            background-color: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        h1 {
            color: #2c3e50;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }
        h2 {
            color: #34495e;
            border-bottom: 2px solid #95a5a6;
            padding-bottom: 8px;
            margin-top: 30px;
            margin-bottom: 15px;
        }
        h3 {
            color: #16a085;
            margin-top: 25px;
            margin-bottom: 12px;
        }
        h4 {
            color: #e67e22;
            margin-top: 20px;
            margin-bottom: 10px;
        }
        blockquote {
            background-color: #ecf0f1;
            border-left: 4px solid #3498db;
            padding: 12px 20px;
            margin: 15px 0;
            border-radius: 4px;
        }
        code {
            background-color: #f8f9fa;
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Consolas', 'Monaco', monospace;
            font-size: 0.9em;
            color: #e74c3c;
        }
        pre {
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            overflow-x: auto;
            font-family: 'Consolas', 'Monaco', monospace;
            line-height: 1.2;
            color: #2c3e50;
        }
        ul {
            margin: 10px 0;
            padding-left: 30px;
        }
        li {
            margin: 8px 0;
        }
        hr {
            border: none;
            border-top: 2px solid #ecf0f1;
            margin: 30px 0;
        }
        a {
            color: #3498db;
            text-decoration: none;
        }
        a:hover {
            text-decoration: underline;
        }
        strong {
            color: #2c3e50;
            font-weight: 600;
        }
        .beat-section {
            background-color: #fafafa;
            padding: 15px;
            margin: 15px 0;
            border-left: 3px solid #e67e22;
            border-radius: 4px;
        }
        .metadata {
            background-color: #f0f8ff;
            padding: 15px;
            border-radius: 4px;
            margin-top: 30px;
        }
        nav {
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 4px;
            border-left: 4px solid #3498db;
            margin: 20px 0;
        }
        nav ul {
            list-style: none;
            padding-left: 0;
        }
        nav ul ul {
            padding-left: 20px;
        }
        nav ul ul ul {
            padding-left: 20px;
        }
        nav li {
            margin: 5px 0;
        }
        nav a {
            text-decoration: none;
            color: #3498db;
            transition: color 0.2s;
        }
        nav a:hover {
            color: #2980b9;
            text-decoration: underline;
        }

        /* Dark mode */
        @media (prefers-color-scheme: dark) {
            body {
                color: #e4e4e7;
                background-color: #18181b;
            }
            .container {
                background-color: #27272a;
                box-shadow: 0 2px 4px rgba(0,0,0,0.3);
            }
            h1 {
                color: #fafafa;
                border-bottom-color: #60a5fa;
            }
            h2 {
                color: #e4e4e7;
                border-bottom-color: #52525b;
            }
            h3 {
                color: #5eead4;
            }
            h4 {
                color: #fb923c;
            }
            blockquote {
                background-color: #3f3f46;
                border-left-color: #60a5fa;
                color: #e4e4e7;
            }
            code {
                background-color: #3f3f46;
                color: #fca5a5;
            }
            pre {
                background-color: #3f3f46;
                color: #e4e4e7;
            }
            hr {
                border-top-color: #3f3f46;
            }
            a {
                color: #60a5fa;
            }
            a:hover {
                color: #93c5fd;
            }
            strong {
                color: #fafafa;
            }
            .beat-section {
                background-color: #3f3f46;
                border-left-color: #fb923c;
            }
            .metadata {
                background-color: #1e3a5f;
            }
            nav {
                background-color: #3f3f46;
                border-left-color: #60a5fa;
            }
            nav a {
                color: #60a5fa;
            }
            nav a:hover {
                color: #93c5fd;
            }
            p {
                color: #d4d4d8;
            }
        }
    </style>
</head>
<body>
    <div class="container">
$html
    </div>
</body>
</html>
"@

    return $htmlDoc
}

function Generate-SequenceMarkdown {
    param($SequenceFilePath, $OutputPath, [switch]$GenerateHtml)

    if (-not (Test-Path $SequenceFilePath)) {
        Write-Host "Sequence file not found: $SequenceFilePath" -ForegroundColor Red
        return
    }

    $sequence = Get-Content $SequenceFilePath -Raw -Encoding utf8 | ConvertFrom-Json
    $sequenceFileName = Split-Path $SequenceFilePath -Leaf
    $sequenceId = [System.IO.Path]::GetFileNameWithoutExtension($sequenceFileName) -replace '\.sequence$', ''
    $outputFile = Join-Path $OutputPath "$sequenceId.md"

    Write-Host "Generating: $outputFile" -ForegroundColor Cyan

    # Build markdown content
    $md = @"
# $($sequence.name)

> **Domain**: $($sequence.domainId) | **Status**: $($sequence.status) | **Auto-generated from**: [$sequenceFileName](../../$($SequenceFilePath.Replace($RepoRoot + '\', '').Replace('\', '/')))

"@

    # Build Table of Contents
    $md += "`n## Table of Contents`n"
    $md += "`n- [Overview](#overview)"
    $md += "`n- [User Story](#user-story)"
    $md += "`n- [Business Value](#business-value)"
    $md += "`n- [Governance](#governance)"
    $md += "`n- [Workflow Movements](#workflow-movements)"

    # Add movement entries to TOC
    $movementCounter = 0
    foreach ($movement in $sequence.movements) {
        $movementCounter++
        $movementAnchor = "movement-$($movement.number)-$($movement.name.ToLower() -replace '\s+', '-' -replace '[^a-z0-9-]', '')"
        $md += "`n  - [Movement $($movement.number): $($movement.name)](#$movementAnchor)"

        # Add beat entries under each movement
        foreach ($beat in $movement.beats) {
            $beatAnchor = "beat-$($beat.beat)-$($beat.name.ToLower() -replace '\s+', '-' -replace '[^a-z0-9-]', '')"
            $md += "`n    - [Beat $($beat.beat): $($beat.name)](#$beatAnchor)"
        }
    }

    $md += "`n- [Metadata](#metadata)"
    $md += "`n"

    # Add Overview section
    $md += @"

## Overview

$($sequence.description)

**Purpose**: $($sequence.purpose)

**Trigger**: $($sequence.trigger)

"@

    # Add overview diagram if it exists
    if ($sequence.overviewDiagram) {
        $md += "`n### Overview Diagram`n"
        $md += "`n``````mermaid`n$($sequence.overviewDiagram)`n```````n"
    }

    $md += "`n## User Story`n"
    $md += "`n**As a** $($sequence.userStory.persona),"
    $md += "`n**I want to** $($sequence.userStory.goal),"
    $md += "`n**So that** $($sequence.userStory.benefit).`n"

    # Add sequence-level user story diagram if it exists (different from overview)
    if ($sequence.userStory.diagram) {
        $md += "`n### User Story Diagram`n"
        $md += "`n``````mermaid`n$($sequence.userStory.diagram)`n```````n"
    }

    $md += @"

## Business Value

$($sequence.businessValue)

## Governance

### Policies
"@

    # Add policies
    foreach ($policy in $sequence.governance.policies) {
        $md += "`n- $policy"
    }

    # Add metrics
    $md += "`n`n### Metrics"
    foreach ($metric in $sequence.governance.metrics) {
        $md += "`n- $metric"
    }

    $md += "`n`n## Workflow Movements`n"

    # Process each movement
    $beatCounter = 0
    foreach ($movement in $sequence.movements) {
        $md += "`n### Movement $($movement.number): $($movement.name)`n"
        $md += "`n$($movement.description)`n"
        $md += "`n**Tempo**: $($movement.tempo) | **Status**: $($movement.status)`n"

        if ($movement.userStory) {
            $md += "`n`n#### User Story`n"
            $md += "`n**As a** $($movement.userStory.persona),"
            $md += "`n**I want to** $($movement.userStory.goal),"
            $md += "`n**So that** $($movement.userStory.benefit).`n"

            # Add movement-level diagram if it exists
            if ($movement.userStory.diagram) {
                $md += "`n``````mermaid`n$($movement.userStory.diagram)`n```````n"
            }
        }

        $md += "`n**Beats**: $($movement.beats.Count)`n"

        # Process each beat
        foreach ($beat in $movement.beats) {
            $beatCounter++
            $beatNumber = $beat.beat  # Use actual beat number from JSON
            $beatName = $beat.name
            $handlerName = $beat.handler.name
            $eventName = $beat.event
            $md += "`n#### Beat ${beatNumber}: $beatName"
            $md += "`n- **Handler**: ``$handlerName``"

            # Include source path or external system info
            if ($beat.handler.externalSystem) {
                # External system integration
                $vendor = $beat.handler.externalSystem.vendor
                $system = $beat.handler.externalSystem.system
                $type = $beat.handler.externalSystem.type
                $md += "`n- **External System**: $vendor - $system ($type)"
                if ($beat.handler.externalSystem.description) {
                    $md += "`n- **Integration**: $($beat.handler.externalSystem.description)"
                }
            } elseif ($beat.handler.scope -eq "external") {
                # Legacy external marker
                $md += "`n- **External System**: External Integration"
            } elseif ($beat.handler.sourcePath) {
                # Internal handler with source path
                $relativeSourcePath = $beat.handler.sourcePath.Replace('\', '/')
                $md += "`n- **Source**: [$relativeSourcePath](../../$relativeSourcePath)"
            }

            $md += "`n- **Event**: $eventName"

            if ($beat.testFile) {
                $relativeTestPath = $beat.testFile.Replace('\', '/')
                $testCase = if ($beat.testCase) { $beat.testCase } else { "Movement$($movement.number)_Beat$($beat.beat)_" }
                $md += "`n- **Test**: [$relativeTestPath](../../$relativeTestPath) - ``$testCase``"
            }

            $md += "`n"

            # Beat-level user story (added to beat-level documentation)
            if ($beat.userStory) {
                $md += "`n**User Story**:`n"
                $md += "`n- **Persona**: $($beat.userStory.persona)"
                $md += "`n- **Goal**: $($beat.userStory.goal)"
                $md += "`n- **Benefit**: $($beat.userStory.benefit)`n"

                # Add beat-level user story diagram if it exists
                if ($beat.userStory.diagram) {
                    $md += "`n**User Story Diagram**:`n"
                    $md += "`n``````mermaid`n$($beat.userStory.diagram)`n```````n"
                }
            }

            # Beat-level diagram (check both direct and userStory locations)
            $beatDiagram = $null
            if ($beat.diagram) {
                $beatDiagram = $beat.diagram
            } elseif ($beat.userStory -and $beat.userStory.diagram) {
                $beatDiagram = $beat.userStory.diagram
            }

            if ($beatDiagram) {
                $md += "`n**Visual Diagram**:`n"
                $md += "`n``````mermaid`n$beatDiagram`n```````n"
            }

            # Acceptance Criteria
            if ($beat.acceptanceCriteria -and $beat.acceptanceCriteria.Count -gt 0) {
                $md += "`n**Acceptance Criteria**:"
                foreach ($ac in $beat.acceptanceCriteria) {
                    # Combine multiple items with semicolons for compact display
                    if ($ac.given -and $ac.given.Count -gt 0) {
                        $givenText = $ac.given -join "; "
                        $md += "`n- **Given**: $givenText"
                    }
                    if ($ac.when -and $ac.when.Count -gt 0) {
                        $whenText = $ac.when -join "; "
                        $md += "`n- **When**: $whenText"
                    }
                    if ($ac.then -and $ac.then.Count -gt 0) {
                        $thenText = $ac.then -join "; "
                        $md += "`n- **Then**: $thenText"
                    }
                }
                $md += "`n"
            }

            # Notes
            if ($beat.notes -and $beat.notes.Count -gt 0) {
                $md += "`n**Notes**:"
                foreach ($note in $beat.notes) {
                    $md += "`n- $note"
                }
                $md += "`n"
            }
        }

        $md += "`n---`n"
    }

    # Metadata footer
    $md += @"

## Metadata

- **Version**: $($sequence.metadata.version)
- **Author**: $($sequence.metadata.author)
- **Created**: $($sequence.metadata.created)
- **Tags**: $($sequence.metadata.tags -join ", ")

---

_This documentation was auto-generated from the canonical sequence definition._
_**Canonical Reference**: [$sequenceFileName](../../$($SequenceFilePath.Replace($RepoRoot + '\', '').Replace('\', '/')))_

"@

    # Write markdown to file using UTF-8 without BOM
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($outputFile, $md, $utf8NoBom)
    Write-Host "  Generated: $outputFile" -ForegroundColor Green

    # Generate HTML if requested
    if ($GenerateHtml) {
        $htmlOutputFile = Join-Path $OutputPath "$sequenceId.html"
        $htmlContent = Convert-MarkdownToHtml -Markdown $md -Title $sequence.name
        [System.IO.File]::WriteAllText($htmlOutputFile, $htmlContent, $utf8NoBom)
        Write-Host "  Generated: $htmlOutputFile" -ForegroundColor Green
    }
}

# Main execution
if ($SequenceFile) {
    # Generate single sequence
    # Handle both absolute and relative paths
    if ([System.IO.Path]::IsPathRooted($SequenceFile)) {
        $fullPath = $SequenceFile
    } else {
        $fullPath = Join-Path $RepoRoot $SequenceFile
    }
    Generate-SequenceMarkdown -SequenceFilePath $fullPath -OutputPath $outputPath -GenerateHtml:$GenerateHtml
} else {
    # Generate all sequences from domain registry
    $registryPath = Join-Path $RepoRoot "schemas\domain-registry.json"
    if (Test-Path $registryPath) {
        $registry = Get-Content $registryPath -Raw -Encoding utf8 | ConvertFrom-Json

        Write-Host ""
        Write-Host "====================================="
        Write-Host "  Sequence Documentation Generator"
        Write-Host "====================================="
        Write-Host ""
        if ($GenerateHtml) {
            Write-Host "  HTML Generation: Enabled" -ForegroundColor Yellow
            Write-Host ""
        }

        $generatedFiles = @()
        foreach ($domain in $registry.domains) {
            foreach ($seq in $domain.sequences) {
                $sequencePath = Join-Path $RepoRoot $seq.sequenceFile
                Generate-SequenceMarkdown -SequenceFilePath $sequencePath -OutputPath $outputPath -GenerateHtml:$GenerateHtml

                if ($GenerateHtml) {
                    $sequenceId = [System.IO.Path]::GetFileNameWithoutExtension($seq.sequenceFile) -replace '\.sequence$', ''
                    $generatedFiles += @{
                        title = $seq.name
                        domain = $domain.name
                        file = "$sequenceId.html"
                    }
                }
            }
        }

        # Generate index.html if HTML generation is enabled
        if ($GenerateHtml -and $generatedFiles.Count -gt 0) {
            Write-Host ""
            Write-Host "Generating index.html..." -ForegroundColor Cyan

            $indexContent = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sequence Documentation - NWPS Enterprise Mobile</title>
    <style>
        /* Light mode (default) */
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            background-color: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        h1 {
            color: #2c3e50;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }
        h2 {
            color: #34495e;
            border-bottom: 2px solid #95a5a6;
            padding-bottom: 8px;
            margin-top: 30px;
            margin-bottom: 15px;
        }
        h3 {
            color: #16a085;
            margin-top: 20px;
            margin-bottom: 12px;
        }
        .sequence-list {
            list-style: none;
            padding: 0;
        }
        .sequence-item {
            background-color: #f8f9fa;
            margin: 10px 0;
            padding: 15px 20px;
            border-radius: 5px;
            border-left: 4px solid #3498db;
            transition: all 0.2s;
        }
        .sequence-item:hover {
            background-color: #e9ecef;
            transform: translateX(5px);
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        }
        .sequence-link {
            font-size: 1.1em;
            color: #2c3e50;
            text-decoration: none;
            font-weight: 600;
        }
        .sequence-link:hover {
            color: #3498db;
        }
        .domain-badge {
            display: inline-block;
            background-color: #3498db;
            color: white;
            padding: 2px 8px;
            border-radius: 3px;
            font-size: 0.8em;
            margin-left: 10px;
        }
        .footer {
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #ecf0f1;
            text-align: center;
            color: #7f8c8d;
            font-size: 0.9em;
        }

        /* Dark mode */
        @media (prefers-color-scheme: dark) {
            body {
                color: #e4e4e7;
                background-color: #18181b;
            }
            .container {
                background-color: #27272a;
                box-shadow: 0 2px 4px rgba(0,0,0,0.3);
            }
            h1 {
                color: #fafafa;
                border-bottom-color: #60a5fa;
            }
            h2 {
                color: #e4e4e7;
                border-bottom-color: #52525b;
            }
            h3 {
                color: #5eead4;
            }
            p {
                color: #d4d4d8;
            }
            .sequence-item {
                background-color: #3f3f46;
                border-left-color: #60a5fa;
            }
            .sequence-item:hover {
                background-color: #52525b;
                box-shadow: 0 2px 5px rgba(0,0,0,0.3);
            }
            .sequence-link {
                color: #e4e4e7;
            }
            .sequence-link:hover {
                color: #60a5fa;
            }
            .domain-badge {
                background-color: #1e40af;
                color: #bfdbfe;
            }
            .footer {
                border-top-color: #3f3f46;
                color: #a1a1aa;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>Sequence Documentation</h1>
        <p>Auto-generated documentation from canonical sequence definitions for the NWPS Enterprise Mobile application suite.</p>

        <h2>Available Sequences</h2>
        <ul class="sequence-list">
"@

            # Group by domain
            $byDomain = $generatedFiles | Group-Object -Property domain

            foreach ($domainGroup in $byDomain) {
                $indexContent += "            <h3>$($domainGroup.Name)</h3>`n"
                foreach ($file in $domainGroup.Group) {
                    $indexContent += "            <li class=`"sequence-item`">`n"
                    $indexContent += "                <a href=`"$($file.file)`" class=`"sequence-link`">$($file.title)</a>`n"
                    $indexContent += "                <span class=`"domain-badge`">$($file.domain)</span>`n"
                    $indexContent += "            </li>`n"
                }
            }

            $indexContent += @"
        </ul>

        <div class="footer">
            <p><em>This documentation was auto-generated from canonical sequence definitions.</em></p>
            <p><strong>Generated:</strong> $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>
        </div>
    </div>
</body>
</html>
"@

            $indexFile = Join-Path $outputPath "index.html"
            $utf8NoBomIndex = New-Object System.Text.UTF8Encoding $false
            [System.IO.File]::WriteAllText($indexFile, $indexContent, $utf8NoBomIndex)
            Write-Host "  Generated: $indexFile" -ForegroundColor Green
        }

        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "  Generation Complete!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host "Domain registry not found: $registryPath" -ForegroundColor Red
    }
}