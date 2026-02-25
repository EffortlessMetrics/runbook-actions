# Build + pack the Runbook plugin for Windows.
#
# Prerequisites:
#   - .NET 8 SDK
#   - logiplugintool (from Logi Actions SDK)
#
# Usage: .\tools\pack.ps1

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$src  = Join-Path $root 'src' 'Runbook.LogiPlugin'
$win  = Join-Path $root 'win'

Write-Host '--- dotnet publish ---'
dotnet publish $src `
    --configuration Release `
    --output $win `
    --self-contained false

Write-Host '--- logiplugintool pack ---'
$pluginTool = Get-Command logiplugintool -ErrorAction SilentlyContinue
if ($pluginTool) {
    logiplugintool pack $root
    Write-Host '--- logiplugintool verify ---'
    $lplug4 = Get-ChildItem $root -Filter '*.lplug4' | Select-Object -First 1
    if ($lplug4) {
        logiplugintool verify $lplug4.FullName
    }
} else {
    Write-Warning 'logiplugintool not found in PATH. Skipping pack/verify.'
    Write-Warning 'Install the Logi Actions SDK to enable packaging.'
}

Write-Host 'Done.'
