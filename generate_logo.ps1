Add-Type -AssemblyName System.Drawing

function Draw-Logo([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.Color]::Transparent)

    $scale = $size / 256.0

    # 1. Background Rounded Squircle with subtle gradient
    $pad = 12 * $scale
    $sqSize = $size - (2 * $pad)
    $radius = 48 * $scale

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $rect = New-Object System.Drawing.RectangleF $pad, $pad, $sqSize, $sqSize
    $path.AddArc($rect.X, $rect.Y, $radius, $radius, 180, 90)
    $path.AddArc($rect.Right - $radius, $rect.Y, $radius, $radius, 270, 90)
    $path.AddArc($rect.Right - $radius, $rect.Bottom - $radius, $radius, $radius, 0, 90)
    $path.AddArc($rect.X, $rect.Bottom - $radius, $radius, $radius, 90, 90)
    $path.CloseFigure()

    # Squircle Fill Gradient: Dark Slate Acrylic / Win11 Dark
    $bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.PointF 0, 0),
        (New-Object System.Drawing.PointF $size, $size),
        [System.Drawing.Color]::FromArgb(255, 22, 27, 34),
        [System.Drawing.Color]::FromArgb(255, 11, 15, 25)
    )
    $g.FillPath($bgBrush, $path)
    $bgBrush.Dispose()

    # Outer border: Glowing Cyan / Accent
    $borderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(200, 0, 180, 255), [Math]::Max(1.0, 2.5 * $scale))
    $g.DrawPath($borderPen, $path)
    $borderPen.Dispose()
    $path.Dispose()

    # 2. Secondary Monitor (Right Display - Monitor 2: Taskbar Visible)
    $m2X = 132 * $scale
    $m2Y = 62 * $scale
    $m2W = 98 * $scale
    $m2H = 114 * $scale

    # Monitor 2 Stand
    $stand2Pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 60, 68, 80), [Math]::Max(1.0, 6 * $scale))
    $g.DrawLine($stand2Pen, ($m2X + $m2W/2), ($m2Y + $m2H), ($m2X + $m2W/2), ($m2Y + $m2H + 20 * $scale))
    $stand2Pen.Dispose()
    
    $base2Brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 75, 85, 100))
    $g.FillRectangle($base2Brush, ($m2X + $m2W/2 - 22 * $scale), ($m2Y + $m2H + 20 * $scale), (44 * $scale), (4 * $scale))
    $base2Brush.Dispose()

    # Monitor 2 Outer Bezel
    $m2BezelBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 30, 36, 46))
    $g.FillRectangle($m2BezelBrush, $m2X, $m2Y, $m2W, $m2H)
    $m2BezelBrush.Dispose()

    $m2BorderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 70, 80, 96), [Math]::Max(1.0, 1.5 * $scale))
    $g.DrawRectangle($m2BorderPen, $m2X, $m2Y, $m2W, $m2H)
    $m2BorderPen.Dispose()

    # Monitor 2 Screen Content (Dark desktop)
    $m2InnerX = $m2X + 4 * $scale
    $m2InnerY = $m2Y + 4 * $scale
    $m2InnerW = $m2W - 8 * $scale
    $m2InnerH = $m2H - 8 * $scale
    $m2ScreenBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.PointF $m2InnerX, $m2InnerY),
        (New-Object System.Drawing.PointF ($m2InnerX + $m2InnerW), ($m2InnerY + $m2InnerH)),
        [System.Drawing.Color]::FromArgb(255, 16, 24, 40),
        [System.Drawing.Color]::FromArgb(255, 26, 38, 60)
    )
    $g.FillRectangle($m2ScreenBrush, $m2InnerX, $m2InnerY, $m2InnerW, $m2InnerH)
    $m2ScreenBrush.Dispose()

    # Monitor 2 Active Glowing Taskbar at bottom!
    $tb2H = 16 * $scale
    $tb2Y = $m2InnerY + $m2InnerH - $tb2H
    $tb2Brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.PointF $m2InnerX, $tb2Y),
        (New-Object System.Drawing.PointF ($m2InnerX + $m2InnerW), $tb2Y),
        [System.Drawing.Color]::FromArgb(255, 0, 150, 255),
        [System.Drawing.Color]::FromArgb(255, 0, 230, 255)
    )
    $g.FillRectangle($tb2Brush, $m2InnerX, $tb2Y, $m2InnerW, $tb2H)
    $tb2Brush.Dispose()

    # Little taskbar app icon dots on Monitor 2
    if ($size -ge 32) {
        $dotBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(240, 255, 255, 255))
        $dotY = $tb2Y + ($tb2H / 2) - (2 * $scale)
        $dotR = 3 * $scale
        $centerDotX = $m2InnerX + ($m2InnerW / 2)
        $g.FillEllipse($dotBrush, ($centerDotX - 10 * $scale), $dotY, $dotR, $dotR)
        $g.FillEllipse($dotBrush, ($centerDotX - 2 * $scale), $dotY, $dotR, $dotR)
        $g.FillEllipse($dotBrush, ($centerDotX + 6 * $scale), $dotY, $dotR, $dotR)
        $dotBrush.Dispose()
    }

    # 3. Primary Monitor (Left Display - Monitor 1: Taskbar HIDDEN / Full Screen!)
    $m1X = 26 * $scale
    $m1Y = 50 * $scale
    $m1W = 112 * $scale
    $m1H = 130 * $scale

    # Monitor 1 Stand
    $stand1Pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 80, 90, 105), [Math]::Max(1.0, 7 * $scale))
    $g.DrawLine($stand1Pen, ($m1X + $m1W/2), ($m1Y + $m1H), ($m1X + $m1W/2), ($m1Y + $m1H + 24 * $scale))
    $stand1Pen.Dispose()
    
    $base1Brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 100, 115, 135))
    $g.FillRectangle($base1Brush, ($m1X + $m1W/2 - 26 * $scale), ($m1Y + $m1H + 24 * $scale), (52 * $scale), (5 * $scale))
    $base1Brush.Dispose()

    # Shadow behind Primary Monitor
    $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(90, 0, 0, 0))
    $g.FillRectangle($shadowBrush, ($m1X + 4 * $scale), ($m1Y + 4 * $scale), $m1W, $m1H)
    $shadowBrush.Dispose()

    # Monitor 1 Outer Bezel (Sleek Dark Titanium)
    $m1BezelBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 38, 44, 56))
    $g.FillRectangle($m1BezelBrush, $m1X, $m1Y, $m1W, $m1H)
    $m1BezelBrush.Dispose()

    $m1BorderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 0, 160, 255), [Math]::Max(1.0, 2 * $scale))
    $g.DrawRectangle($m1BorderPen, $m1X, $m1Y, $m1W, $m1H)
    $m1BorderPen.Dispose()

    # Monitor 1 Screen (Vibrant Windows 11 Full Screen Bloom Wallpaper extending ALL THE WAY TO BOTTOM)
    $m1InnerX = $m1X + 4 * $scale
    $m1InnerY = $m1Y + 4 * $scale
    $m1InnerW = $m1W - 8 * $scale
    $m1InnerH = $m1H - 8 * $scale

    $m1ScreenBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.PointF $m1InnerX, $m1InnerY),
        (New-Object System.Drawing.PointF ($m1InnerX + $m1InnerW), ($m1InnerY + $m1InnerH)),
        [System.Drawing.Color]::FromArgb(255, 20, 65, 140),
        [System.Drawing.Color]::FromArgb(255, 85, 35, 135)
    )
    $g.FillRectangle($m1ScreenBrush, $m1InnerX, $m1InnerY, $m1InnerW, $m1InnerH)
    $m1ScreenBrush.Dispose()

    # Bloom Petal Highlights on Monitor 1
    if ($size -ge 48) {
        $bloomBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
            (New-Object System.Drawing.PointF ($m1InnerX + 20 * $scale), ($m1InnerY + 20 * $scale)),
            (New-Object System.Drawing.PointF ($m1InnerX + $m1InnerW), ($m1InnerY + $m1InnerH - 20 * $scale)),
            [System.Drawing.Color]::FromArgb(180, 0, 210, 255),
            [System.Drawing.Color]::FromArgb(140, 180, 50, 255)
        )
        $bloomPath = New-Object System.Drawing.Drawing2D.GraphicsPath
        $bloomPath.AddEllipse(($m1InnerX + 15 * $scale), ($m1InnerY + 25 * $scale), (70 * $scale), (65 * $scale))
        $g.FillPath($bloomBrush, $bloomPath)
        $bloomBrush.Dispose()
        $bloomPath.Dispose()
    }

    # "1" Badge on Monitor 1 (Top-Left)
    if ($size -ge 32) {
        $badgeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(230, 0, 120, 215))
        $g.FillEllipse($badgeBrush, ($m1InnerX + 4 * $scale), ($m1InnerY + 4 * $scale), (16 * $scale), (16 * $scale))
        $badgeBrush.Dispose()

        $fontSize = [float][Math]::Max(7.0, 9.0 * $scale)
        $font = New-Object System.Drawing.Font("Arial", $fontSize, [System.Drawing.FontStyle]::Bold)
        $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $sf = New-Object System.Drawing.StringFormat
        $sf.Alignment = [System.Drawing.StringAlignment]::Center
        $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
        $g.DrawString("1", $font, $textBrush, [float]($m1InnerX + 12 * $scale), [float]($m1InnerY + 12 * $scale), $sf)
        $font.Dispose()
        $textBrush.Dispose()
        $sf.Dispose()
    }

    # Dotted / Dashed Line at Bottom of Screen 1 indicating Hidden Taskbar!
    $dashPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(230, 0, 240, 255), [Math]::Max(1.0, 2 * $scale))
    $dashPen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash
    $dashY = $m1InnerY + $m1InnerH - (3 * $scale)
    $g.DrawLine($dashPen, ($m1InnerX + 4 * $scale), $dashY, ($m1InnerX + $m1InnerW - 4 * $scale), $dashY)
    $dashPen.Dispose()

    # Small Eye-slash / Hide indicator at bottom center of Monitor 1
    if ($size -ge 48) {
        $slashPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(240, 255, 80, 80), [Math]::Max(1.0, 2 * $scale))
        $cX = $m1InnerX + ($m1InnerW / 2)
        $cY = $dashY - (10 * $scale)
        # Eye arc
        $g.DrawArc($slashPen, ($cX - 8 * $scale), ($cY - 4 * $scale), (16 * $scale), (8 * $scale), 0, 180)
        # Slash line across eye
        $g.DrawLine($slashPen, ($cX - 7 * $scale), ($cY + 5 * $scale), ($cX + 7 * $scale), ($cY - 5 * $scale))
        $slashPen.Dispose()
    }

    $g.Dispose()
    return $bmp
}

$repoRoot = "C:\Projeler\you-can-hide-the-taskbar-on-your-primary-monitor-in-windows-11"
$assetsDir = Join-Path $repoRoot "src\YCHTTOPYMIW11.App\Assets"
if (!(Test-Path $assetsDir)) {
    New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null
}

$png256Path = Join-Path $assetsDir "app.png"
$bmp256 = Draw-Logo 256
$bmp256.Save($png256Path, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "Saved 256x256 PNG to $png256Path"

# Sizes for multi-resolution ICO: 256, 128, 64, 48, 32, 16
$sizes = @(256, 128, 64, 48, 32, 16)
$imagesData = @()

foreach ($s in $sizes) {
    $bmp = Draw-Logo $s
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $bytes = $ms.ToArray()
    $ms.Dispose()

    $imagesData += ,@($s, $bytes)
}

# Construct ICO file
$icoMs = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter $icoMs

$bw.Write([uint16]0) # Reserved
$bw.Write([uint16]1) # Icon type
$bw.Write([uint16]$imagesData.Count) # Count

# Header offset: 6 bytes + (16 * count)
$offset = 6 + (16 * $imagesData.Count)

foreach ($item in $imagesData) {
    $s = $item[0]
    $data = $item[1]
    
    $wByte = if ($s -ge 256) { [byte]0 } else { [byte]$s }
    $hByte = if ($s -ge 256) { [byte]0 } else { [byte]$s }

    $bw.Write($wByte)
    $bw.Write($hByte)
    $bw.Write([byte]0) # Palette colors
    $bw.Write([byte]0) # Reserved
    $bw.Write([uint16]1) # Color planes
    $bw.Write([uint16]32) # Bits per pixel
    $bw.Write([uint32]$data.Length) # Image size
    $bw.Write([uint32]$offset) # Image offset

    $offset += $data.Length
}

foreach ($item in $imagesData) {
    $data = $item[1]
    $bw.Write($data)
}

$bw.Flush()
$icoBytes = $icoMs.ToArray()
$bw.Dispose()
$icoMs.Dispose()

$icoPath = Join-Path $assetsDir "app.ico"
[System.IO.File]::WriteAllBytes($icoPath, $icoBytes)
Write-Host "Saved multi-resolution ICO to $icoPath ($($icoBytes.Length) bytes)"

# Verify loading icon
$testIcon = New-Object System.Drawing.Icon $icoPath
Write-Host "Verified ICO: width=$($testIcon.Width), height=$($testIcon.Height)"
$testIcon.Dispose()
