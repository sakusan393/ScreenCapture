# シンプルなカメラアイコンを作成するスクリプト
Add-Type -AssemblyName System.Drawing

$sizes = @(256, 128, 64, 48, 32, 16)
$icons = @()

foreach ($size in $sizes) {
    $bitmap = New-Object System.Drawing.Bitmap $size, $size
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)

    # スケール
    $scale = $size / 256.0

    # カメラの本体
    $bodyRect = [System.Drawing.Rectangle]::new(
        [int](50 * $scale), [int](80 * $scale),
        [int](156 * $scale), [int](120 * $scale)
    )
    $blueBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 33, 150, 243))
    $graphics.FillRectangle($blueBrush, $bodyRect)

    # レンズ
    $lensRect = [System.Drawing.Rectangle]::new(
        [int](88 * $scale), [int](110 * $scale),
        [int](80 * $scale), [int](80 * $scale)
    )
    $whiteBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
    $graphics.FillEllipse($whiteBrush, $lensRect)

    # レンズ中心
    $centerRect = [System.Drawing.Rectangle]::new(
        [int](108 * $scale), [int](130 * $scale),
        [int](40 * $scale), [int](40 * $scale)
    )
    $graphics.FillEllipse($blueBrush, $centerRect)

    # シャッターボタン
    $shutterRect = [System.Drawing.Rectangle]::new(
        [int](180 * $scale), [int](50 * $scale),
        [int](30 * $scale), [int](20 * $scale)
    )
    $redBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 244, 67, 54))
    $graphics.FillEllipse($redBrush, $shutterRect)

    # ファインダー
    $viewfinderRect = [System.Drawing.Rectangle]::new(
        [int](80 * $scale), [int](50 * $scale),
        [int](40 * $scale), [int](20 * $scale)
    )
    $graphics.FillRectangle($whiteBrush, $viewfinderRect)

    $icons += $bitmap
    
    $graphics.Dispose()
    $blueBrush.Dispose()
    $whiteBrush.Dispose()
    $redBrush.Dispose()
}

# .icoファイルとして保存
$iconPath = Join-Path $PSScriptRoot "app.ico"
$icon256 = $icons[0]
$icon256.Save($iconPath, [System.Drawing.Imaging.ImageFormat]::Icon)

Write-Host "Icon created at: $iconPath"

# クリーンアップ
foreach ($icon in $icons) {
    $icon.Dispose()
}
