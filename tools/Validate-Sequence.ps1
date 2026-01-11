<#
.SYNOPSIS
    Validates a canonical sequence JSON file against the musical-sequence schema.

.DESCRIPTION
    This script validates sequence files to ensure they conform to the canonical
    reference schema. It checks for required fields, proper structure, and
    provides detailed validation feedback.

.PARAMETER SequencePath
    Path to the sequence JSON file to validate.

.EXAMPLE
    .\Tools\Validate-Sequence.ps1 -SequencePath schemas/sequences/authentication-oauth2-code-flow.sequence.json

.NOTES
    Author: NWPS Mobile Team
    Version: 1.0.0
    Last Updated: 2025-12-09
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SequencePath
)

$ErrorActionPreference = "Stop"

Write-Host "=== Canonical Sequence Validator ===" -ForegroundColor Cyan
Write-Host ""

# Check if sequence file exists
if (-not (Test-Path $SequencePath)) {
    Write-Host "ERROR: Sequence file not found: $SequencePath" -ForegroundColor Red
    exit 1
}

Write-Host "Validating: $SequencePath" -ForegroundColor Yellow

try {
    # Load sequence JSON
    $sequence = Get-Content $SequencePath -Raw | ConvertFrom-Json
    Write-Host "[OK] JSON is well-formed" -ForegroundColor Green

    # Validate required top-level fields
    $requiredFields = @('domainId', 'id', 'name', 'movements', 'userStory')
    $missingFields = @()

    foreach ($field in $requiredFields) {
        if (-not $sequence.PSObject.Properties[$field]) {
            $missingFields += $field
        }
    }

    if ($missingFields.Count -gt 0) {
        Write-Host "[ERROR] Missing required fields: $($missingFields -join ', ')" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] All required top-level fields present" -ForegroundColor Green

    # Validate movements
    if ($sequence.movements.Count -eq 0) {
        Write-Host "[ERROR] Sequence must have at least one movement" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Sequence has $($sequence.movements.Count) movement(s)" -ForegroundColor Green

    # Validate each movement
    $movementNum = 1
    $totalBeats = 0
    foreach ($movement in $sequence.movements) {
        Write-Host "  Movement $movementNum`: $($movement.name)" -ForegroundColor Cyan

        # Check required movement fields
        if (-not $movement.name) {
            Write-Host "  [ERROR] Movement $movementNum is missing 'name'" -ForegroundColor Red
            exit 1
        }

        if (-not $movement.beats) {
            Write-Host "  [ERROR] Movement $movementNum is missing 'beats' array" -ForegroundColor Red
            exit 1
        }

        if ($movement.beats.Count -eq 0) {
            Write-Host "  [ERROR] Movement $movementNum has no beats" -ForegroundColor Red
            exit 1
        }

        Write-Host "    [OK] $($movement.beats.Count) beat(s)" -ForegroundColor Green

        # Validate each beat
        $beatNum = 1
        foreach ($beat in $movement.beats) {
            $totalBeats++

            # Check required beat fields
            $requiredBeatFields = @('event', 'userStory', 'acceptanceCriteria', 'testFile')
            $missingBeatFields = @()

            foreach ($field in $requiredBeatFields) {
                if (-not $beat.PSObject.Properties[$field]) {
                    $missingBeatFields += $field
                }
            }

            if ($missingBeatFields.Count -gt 0) {
                Write-Host "    [ERROR] Beat $beatNum is missing: $($missingBeatFields -join ', ')" -ForegroundColor Red
                exit 1
            }

            # Validate user story structure
            if (-not $beat.userStory.persona -or -not $beat.userStory.goal -or -not $beat.userStory.benefit) {
                Write-Host "    [ERROR] Beat $beatNum has incomplete userStory (needs persona, goal, benefit)" -ForegroundColor Red
                exit 1
            }

            # Validate acceptance criteria structure
            if ($beat.acceptanceCriteria.Count -eq 0) {
                Write-Host "    [ERROR] Beat $beatNum has no acceptance criteria" -ForegroundColor Red
                exit 1
            }

            foreach ($criteria in $beat.acceptanceCriteria) {
                if (-not $criteria.given -or -not $criteria.when -or -not $criteria.then) {
                    Write-Host "    [ERROR] Beat $beatNum has incomplete acceptance criteria (needs given, when, then)" -ForegroundColor Red
                    exit 1
                }
            }

            # Check if test file exists
            $testFilePath = Join-Path (Split-Path $SequencePath -Parent | Split-Path -Parent) $beat.testFile
            if (-not (Test-Path $testFilePath)) {
                Write-Host "    [WARN] Beat $beatNum`: Test file not found: $($beat.testFile)" -ForegroundColor Yellow
            }

            $beatNum++
        }

        $movementNum++
    }

    Write-Host ""
    Write-Host "=== Validation Summary ===" -ForegroundColor Cyan
    Write-Host "Sequence ID: $($sequence.id)" -ForegroundColor White
    Write-Host "Name: $($sequence.name)" -ForegroundColor White
    Write-Host "Domain: $($sequence.domainId)" -ForegroundColor White
    Write-Host "Movements: $($sequence.movements.Count)" -ForegroundColor White
    Write-Host "Total Beats: $totalBeats" -ForegroundColor White
    Write-Host "Status: $($sequence.status)" -ForegroundColor White
    Write-Host ""
    Write-Host "[SUCCESS] Sequence is valid!" -ForegroundColor Green

    exit 0
}
catch {
    Write-Host ""
    Write-Host "[ERROR] Validation failed: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
}