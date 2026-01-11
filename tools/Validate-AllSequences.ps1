<#
.SYNOPSIS
    Validates all sequence JSON files in the sequences folder

.DESCRIPTION
    Quick validation script to check if sequences are well-formed JSON and have required fields
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Sequence Validation Report ===" -ForegroundColor Cyan
Write-Host ""

$sequencesPath = "C:\source\repos\bpm\internal\orchestration-wisdom\sequences"
$sequenceFiles = Get-ChildItem -Path $sequencesPath -Filter "*.json"

$allValid = $true

foreach ($file in $sequenceFiles) {
    Write-Host "Validating: $($file.Name)" -ForegroundColor Yellow

    try {
        $sequence = Get-Content $file.FullName -Raw | ConvertFrom-Json

        # Check required fields from schema
        $requiredFields = @('domainId', 'id', 'name', 'movements', 'userStory', 'diagram')
        $missing = @()

        foreach ($field in $requiredFields) {
            if (-not $sequence.PSObject.Properties[$field]) {
                $missing += $field
            }
        }

        if ($missing.Count -gt 0) {
            Write-Host "  [ERROR] Missing required fields: $($missing -join ', ')" -ForegroundColor Red
            $allValid = $false
        } else {
            Write-Host "  [OK] JSON is well-formed" -ForegroundColor Green
            Write-Host "  [OK] All required fields present" -ForegroundColor Green
            Write-Host "  Domain: $($sequence.domainId)" -ForegroundColor Gray
            Write-Host "  ID: $($sequence.id)" -ForegroundColor Gray
            Write-Host "  Status: $($sequence.status)" -ForegroundColor Gray
            Write-Host "  Movements: $($sequence.movements.Count)" -ForegroundColor Gray

            $totalBeats = 0
            foreach ($movement in $sequence.movements) {
                $totalBeats += $movement.beats.Count
            }
            Write-Host "  Total Beats: $totalBeats" -ForegroundColor Gray
        }

    } catch {
        Write-Host "  [ERROR] Failed to parse JSON: $_" -ForegroundColor Red
        $allValid = $false
    }

    Write-Host ""
}

if ($allValid) {
    Write-Host "=== All Sequences Valid! ===" -ForegroundColor Green
    exit 0
} else {
    Write-Host "=== Validation Failed ===" -ForegroundColor Red
    exit 1
}
