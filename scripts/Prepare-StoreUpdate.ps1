[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern("^\d+\.\d+\.\d+$")]
    [string]$Version
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "ScreenCapture\ScreenCapture.csproj"
$releaseStatePath = Join-Path $repoRoot "store\release-state.json"
$releaseNotesDirectory = Join-Path $repoRoot "store\releases"
$releaseNotesPath = Join-Path $releaseNotesDirectory "$Version.md"

foreach ($requiredPath in @($projectPath, $releaseStatePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required file was not found: $requiredPath"
    }
}

$releaseState = Get-Content -LiteralPath $releaseStatePath -Raw -Encoding UTF8 | ConvertFrom-Json
[xml]$project = Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8
$currentProjectVersion = [string]$project.Project.PropertyGroup.Version

if ($currentProjectVersion -notmatch "^\d+\.\d+\.\d+$") {
    throw "The current project Version must use MAJOR.MINOR.PATCH format: $currentProjectVersion"
}

$targetPackageVersion = "$Version.0"
$lastPublishedProjectVersion = [string]$releaseState.lastPublishedProjectVersion
$lastPublishedPackageVersion = [string]$releaseState.lastPublishedPackageVersion

if ([version]$targetPackageVersion -le [version]$lastPublishedPackageVersion) {
    throw "Version $targetPackageVersion must be newer than the published Store version $lastPublishedPackageVersion."
}

if ([version]$Version -lt [version]$currentProjectVersion) {
    throw "Version $Version must not be older than the current project version $currentProjectVersion."
}

if ($currentProjectVersion -ne $Version -and $PSCmdlet.ShouldProcess($projectPath, "Change project Version from $currentProjectVersion to $Version")) {
    $projectText = [System.IO.File]::ReadAllText($projectPath)
    $versionPattern = "(<Version>)[^<]+(</Version>)"
    $versionMatches = [System.Text.RegularExpressions.Regex]::Matches($projectText, $versionPattern)
    if ($versionMatches.Count -ne 1) {
        throw "Expected exactly one <Version> element in $projectPath, but found $($versionMatches.Count)."
    }

    $updatedProjectText = [System.Text.RegularExpressions.Regex]::Replace(
        $projectText,
        $versionPattern,
        { param($match) $match.Groups[1].Value + $Version + $match.Groups[2].Value }
    )
    [System.IO.File]::WriteAllText(
        $projectPath,
        $updatedProjectText,
        [System.Text.UTF8Encoding]::new($false)
    )
}

if (-not (Test-Path -LiteralPath $releaseNotesPath) -and $PSCmdlet.ShouldProcess($releaseNotesPath, "Create Store release notes template")) {
    New-Item -ItemType Directory -Path $releaseNotesDirectory -Force | Out-Null
    $releaseNotes = @(
        "# 393 ScreenCapture $Version",
        "",
        "- Status: preparing",
        "- Store package version: $targetPackageVersion",
        "- Based on published version: $lastPublishedProjectVersion ($lastPublishedPackageVersion)",
        "- Published date: TBD",
        "",
        "## Changes",
        "",
        "- TODO: Describe the user-visible fixes and features.",
        "",
        "## Automated verification",
        "",
        "- [ ] dotnet restore ScreenCapture.sln",
        "- [ ] dotnet build ScreenCapture.sln --configuration Debug",
        "- [ ] dotnet publish ScreenCapture/ScreenCapture.csproj --configuration Release",
        "- [ ] .\scripts\Build-StoreMsix.ps1",
        "- [ ] git diff --check",
        "",
        "## Manual verification",
        "",
        "- [ ] Verified the changed behavior on Windows.",
        "- [ ] Verified startup, capture, copy, save, notification area, and hotkey.",
        "- [ ] Verified the Store MSIX version, identity, and architecture.",
        "",
        "## Partner Center What's new",
        "",
        "TODO: Add a short customer-facing update summary."
    ) -join [Environment]::NewLine
    [System.IO.File]::WriteAllText(
        $releaseNotesPath,
        $releaseNotes,
        [System.Text.UTF8Encoding]::new($false)
    )
}

Write-Host "Store update preparation validated."
Write-Host "  Published: $lastPublishedPackageVersion"
Write-Host "  Project:   $Version"
Write-Host "  Package:   $targetPackageVersion"
Write-Host "  Notes:     $releaseNotesPath"
Write-Host "Next: implement and verify the change, complete the release notes, then run scripts\Build-StoreMsix.ps1."
