[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern("^\d+\.\d+\.\d+$")]
    [string]$Version,

    [ValidatePattern("^\d{4}-\d{2}-\d{2}$")]
    [string]$PublishedDate = (Get-Date).ToString("yyyy-MM-dd")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "ScreenCapture\ScreenCapture.csproj"
$releaseStatePath = Join-Path $repoRoot "store\release-state.json"
$releaseNotesPath = Join-Path $repoRoot "store\releases\$Version.md"

foreach ($requiredPath in @($projectPath, $releaseStatePath, $releaseNotesPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required file was not found: $requiredPath"
    }
}

$parsedPublishedDate = [datetime]::MinValue
if (-not [datetime]::TryParseExact(
    $PublishedDate,
    "yyyy-MM-dd",
    [System.Globalization.CultureInfo]::InvariantCulture,
    [System.Globalization.DateTimeStyles]::None,
    [ref]$parsedPublishedDate
)) {
    throw "PublishedDate is not a valid calendar date: $PublishedDate"
}

[xml]$project = Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8
$currentProjectVersion = [string]$project.Project.PropertyGroup.Version
if ($currentProjectVersion -ne $Version) {
    throw "The project version is $currentProjectVersion, but the release being completed is $Version. Run this from the released source revision."
}

$releaseState = Get-Content -LiteralPath $releaseStatePath -Raw -Encoding UTF8 | ConvertFrom-Json
$targetPackageVersion = "$Version.0"
$lastPublishedPackageVersion = [string]$releaseState.lastPublishedPackageVersion

if ([version]$targetPackageVersion -lt [version]$lastPublishedPackageVersion) {
    throw "Version $targetPackageVersion is older than the recorded Store version $lastPublishedPackageVersion."
}

if ($targetPackageVersion -eq $lastPublishedPackageVersion) {
    Write-Host "Store release $targetPackageVersion is already recorded; no changes are required."
    return
}

$releaseNotes = [System.IO.File]::ReadAllText($releaseNotesPath)
if ($releaseNotes -notmatch "(?m)^- Status: preparing\r?$") {
    throw "The release notes do not contain the expected preparation status: $releaseNotesPath"
}

if ($releaseNotes -notmatch "(?m)^- Published date: TBD\r?$") {
    throw "The release notes do not contain the expected unpublished date: $releaseNotesPath"
}

if ($PSCmdlet.ShouldProcess($releaseStatePath, "Record Store release $targetPackageVersion published on $PublishedDate")) {
    $releaseState.lastPublishedProjectVersion = $Version
    $releaseState.lastPublishedPackageVersion = $targetPackageVersion
    $releaseState.lastPublishedDate = $PublishedDate
    $releaseStateJson = $releaseState | ConvertTo-Json -Depth 10
    [System.IO.File]::WriteAllText(
        $releaseStatePath,
        "$releaseStateJson`r`n",
        [System.Text.UTF8Encoding]::new($false)
    )
}

if ($PSCmdlet.ShouldProcess($releaseNotesPath, "Mark Store release notes as published")) {
    $releaseNotes = [System.Text.RegularExpressions.Regex]::Replace(
        $releaseNotes,
        "(?m)^- Status: preparing\r?$",
        "- Status: published"
    )
    $releaseNotes = [System.Text.RegularExpressions.Regex]::Replace(
        $releaseNotes,
        "(?m)^- Published date: TBD\r?$",
        "- Published date: $PublishedDate"
    )
    [System.IO.File]::WriteAllText(
        $releaseNotesPath,
        $releaseNotes,
        [System.Text.UTF8Encoding]::new($false)
    )
}

Write-Host "Store release recorded successfully."
Write-Host "  Project: $Version"
Write-Host "  Package: $targetPackageVersion"
Write-Host "  Date:    $PublishedDate"
Write-Host "Commit the release state and release notes so the next work thread starts from this version."
