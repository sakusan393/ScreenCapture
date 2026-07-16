[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$sourcePath = Join-Path $PSScriptRoot "icon.png"
$destinationPath = Join-Path $PSScriptRoot "app.ico"

if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "Icon source was not found: $sourcePath"
}

$magick = Get-Command magick -ErrorAction SilentlyContinue
if ($null -eq $magick) {
    throw "ImageMagick was not found. Install ImageMagick and make 'magick' available on PATH."
}

& $magick.Source $sourcePath `
    -background none `
    -define "icon:auto-resize=256,128,64,48,40,32,24,20,16" `
    $destinationPath

if ($LASTEXITCODE -ne 0) {
    throw "ImageMagick failed with exit code $LASTEXITCODE."
}

Write-Host "Multi-resolution icon created: $destinationPath"
