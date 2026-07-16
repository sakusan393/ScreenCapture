[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$PackageIdentityName = "393.393ScreenCapture",

    [ValidateNotNullOrEmpty()]
    [string]$Publisher = "CN=6F9032AC-F4A1-4304-8FB0-9E12219A5335",

    [ValidateNotNullOrEmpty()]
    [string]$PackageDisplayName = "393 ScreenCapture",

    [ValidateNotNullOrEmpty()]
    [string]$PublisherDisplayName = "393",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("win-x64")]
    [string]$RuntimeIdentifier = "win-x64",

    [string]$OutputDirectory,

    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "ScreenCapture\ScreenCapture.csproj"
$manifestTemplatePath = Join-Path $repoRoot "store\Package.appxmanifest"
$iconSourcePath = Join-Path $repoRoot "ScreenCapture\icon.png"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot "artifacts\store"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot $OutputDirectory
}

$repoRootFullPath = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd('\') + '\'
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
if (-not $OutputDirectory.StartsWith($repoRootFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDirectory must stay inside the repository: $repoRoot"
}

foreach ($requiredPath in @($projectPath, $manifestTemplatePath, $iconSourcePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required file was not found: $requiredPath"
    }
}

if ($PackageIdentityName -match "PACKAGE_IDENTITY_NAME|REPLACE") {
    throw "Use the exact Package/Identity/Name value from Partner Center."
}

if ($Publisher -match "PACKAGE_PUBLISHER|REPLACE") {
    throw "Use the exact Package/Identity/Publisher value from Partner Center."
}

[xml]$project = Get-Content -LiteralPath $projectPath -Raw
$projectVersion = [string]$project.Project.PropertyGroup.Version
if ($projectVersion -notmatch "^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)$") {
    throw "The project Version must use MAJOR.MINOR.BUILD format: $projectVersion"
}

$packageVersion = "$($Matches.major).$($Matches.minor).$($Matches.build).0"
$packageBaseName = "ScreenCapture_$($packageVersion)_x64"
$stagingRoot = Join-Path $OutputDirectory "staging"
$publishDirectory = Join-Path $stagingRoot "publish"
$layoutDirectory = Join-Path $stagingRoot "layout"
$assetsDirectory = Join-Path $layoutDirectory "Assets"
$symbolsDirectory = Join-Path $stagingRoot "symbols"
$msixPath = Join-Path $OutputDirectory "$packageBaseName.msix"
$appxSymPath = Join-Path $OutputDirectory "$packageBaseName.appxsym"
$msixUploadPath = Join-Path $OutputDirectory "$packageBaseName.msixupload"
$checksumPath = "$msixUploadPath.sha256"

if (Test-Path -LiteralPath $stagingRoot) {
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $publishDirectory, $layoutDirectory, $assetsDirectory, $symbolsDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$dotnetArguments = @(
    "publish",
    $projectPath,
    "--configuration", $Configuration,
    "--runtime", $RuntimeIdentifier,
    "--self-contained", "true",
    "--output", $publishDirectory,
    "-p:Version=$projectVersion",
    "-p:PublishSingleFile=false",
    "-p:IncludeNativeLibrariesForSelfExtract=false",
    "-p:DebugType=portable",
    "-p:DebugSymbols=true"
)

if ($NoRestore) {
    $dotnetArguments += "--no-restore"
}

& dotnet @dotnetArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$publishedExe = Join-Path $publishDirectory "ScreenCapture.exe"
if (-not (Test-Path -LiteralPath $publishedExe -PathType Leaf)) {
    throw "Published executable was not found: $publishedExe"
}

$pdbFiles = @(Get-ChildItem -LiteralPath $publishDirectory -Filter "*.pdb" -File)
foreach ($pdbFile in $pdbFiles) {
    Move-Item -LiteralPath $pdbFile.FullName -Destination $symbolsDirectory
}

Copy-Item -Path (Join-Path $publishDirectory "*") -Destination $layoutDirectory -Recurse -Force

[xml]$manifest = Get-Content -LiteralPath $manifestTemplatePath -Raw
$identity = $manifest.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']")
$identity.SetAttribute("Name", $PackageIdentityName)
$identity.SetAttribute("Publisher", $Publisher)
$identity.SetAttribute("Version", $packageVersion)

$publisherDisplayNameNode = $manifest.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Properties']/*[local-name()='PublisherDisplayName']")
$publisherDisplayNameNode.InnerText = $PublisherDisplayName

$packageDisplayNameNode = $manifest.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Properties']/*[local-name()='DisplayName']")
$packageDisplayNameNode.InnerText = $PackageDisplayName

$applicationDisplayNameNode = $manifest.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='VisualElements']")
$applicationDisplayNameNode.SetAttribute("DisplayName", $PackageDisplayName)

$manifestOutputPath = Join-Path $layoutDirectory "AppxManifest.xml"
$xmlSettings = [System.Xml.XmlWriterSettings]::new()
$xmlSettings.Encoding = [System.Text.UTF8Encoding]::new($false)
$xmlSettings.Indent = $true
$xmlWriter = [System.Xml.XmlWriter]::Create($manifestOutputPath, $xmlSettings)
try {
    $manifest.Save($xmlWriter)
}
finally {
    $xmlWriter.Dispose()
}

Add-Type -AssemblyName System.Drawing

function New-SquarePngAsset {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationPath,

        [Parameter(Mandatory = $true)]
        [int]$Size
    )

    $sourceImage = [System.Drawing.Image]::FromFile($SourcePath)
    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.DrawImage($sourceImage, 0, 0, $Size, $Size)
        $bitmap.Save($DestinationPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
        $sourceImage.Dispose()
    }
}

New-SquarePngAsset -SourcePath $iconSourcePath -DestinationPath (Join-Path $assetsDirectory "StoreLogo.png") -Size 50
New-SquarePngAsset -SourcePath $iconSourcePath -DestinationPath (Join-Path $assetsDirectory "Square44x44Logo.png") -Size 44
New-SquarePngAsset -SourcePath $iconSourcePath -DestinationPath (Join-Path $assetsDirectory "Square150x150Logo.png") -Size 150

$windowsKitsBin = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"
$makeAppx = Get-ChildItem -LiteralPath $windowsKitsBin -Filter "makeappx.exe" -Recurse -File |
    Where-Object {
        $_.DirectoryName -match "\\x64$" -and
        $_.Directory.Parent.Name -match "^\d+\.\d+\.\d+\.\d+$"
    } |
    Sort-Object { [version]$_.Directory.Parent.Name } -Descending |
    Select-Object -First 1

if ($null -eq $makeAppx) {
    throw "MakeAppx.exe was not found. Install the Windows 10 or Windows 11 SDK."
}

foreach ($outputPath in @($msixPath, $appxSymPath, $msixUploadPath, $checksumPath)) {
    if (Test-Path -LiteralPath $outputPath) {
        Remove-Item -LiteralPath $outputPath -Force
    }
}

& $makeAppx.FullName pack /d $layoutDirectory /p $msixPath /o
if ($LASTEXITCODE -ne 0) {
    throw "MakeAppx.exe failed with exit code $LASTEXITCODE."
}

if ($pdbFiles.Count -gt 0) {
    $symbolsZipPath = Join-Path $stagingRoot "symbols.zip"
    Compress-Archive -Path (Join-Path $symbolsDirectory "*") -DestinationPath $symbolsZipPath -CompressionLevel Optimal
    Move-Item -LiteralPath $symbolsZipPath -Destination $appxSymPath
}

$uploadContents = @($msixPath)
if (Test-Path -LiteralPath $appxSymPath -PathType Leaf) {
    $uploadContents += $appxSymPath
}

$uploadZipPath = Join-Path $stagingRoot "store-upload.zip"
Compress-Archive -LiteralPath $uploadContents -DestinationPath $uploadZipPath -CompressionLevel NoCompression
Move-Item -LiteralPath $uploadZipPath -Destination $msixUploadPath

$hash = (Get-FileHash -LiteralPath $msixUploadPath -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash *$([System.IO.Path]::GetFileName($msixUploadPath))" | Set-Content -LiteralPath $checksumPath -Encoding ascii

Write-Host "Store package created successfully."
Write-Host "  MSIX:       $msixPath"
Write-Host "  MSIXUPLOAD: $msixUploadPath"
Write-Host "  SHA-256:    $hash"
