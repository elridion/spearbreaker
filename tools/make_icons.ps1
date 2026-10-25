# Builds the item icon (assets/icon.png, embedded in the DLL and used for the trinket and its status
# effect) and the mod icon (package/icon.png, 256 x 256, Thunderstore/Hexium) from the game-icons.net
# "shield-impact" icon by Delapouite (CC BY 3.0): a spear punching through a cracked shield. The item
# icon is bronze; the mod icon is blue-grey tin like the charm's model.
#
# The archive is the same one Heraldry uses; it is downloaded when missing.
#
# Usage (from the repo root, Windows PowerShell): powershell -File tools/make_icons.ps1

param(
    [string]$Archive = 'assets/source/game-icons.net.png.zip',
    [string]$Icon = 'delapouite/shield-impact'
)

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)

if (-not (Test-Path $Archive)) {
    New-Item -ItemType Directory -Force (Split-Path $Archive) | Out-Null
    Invoke-WebRequest 'https://game-icons.net/archives/png/zip/ffffff/transparent/game-icons.net.png.zip' -UseBasicParsing -OutFile $Archive
}

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System; using System.Drawing; using System.Drawing.Drawing2D; using System.Drawing.Imaging;
public static class IconPaint {
    // Draws the white glyph as metal: a vertical gradient from top to bottom through the glyph's
    // alpha, over a dark outline (edge) grown from the same alpha, so it reads on dark backgrounds.
    public static Bitmap Glyph(Bitmap src, int size, double fill, int outline, Color top, Color bottom, Color edge) {
        var dst = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        int inner = (int)Math.Round(size * fill);
        int off = (size - inner) / 2;
        var mask = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(mask)) {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(src, new Rectangle(off, off, inner, inner));
        }
        var a = new byte[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                a[y * size + x] = mask.GetPixel(x, y).A;
        mask.Dispose();

        for (int y = 0; y < size; y++) {
            double t = (double)y / (size - 1);
            for (int x = 0; x < size; x++) {
                int ring = 0;
                for (int dy = -outline; dy <= outline; dy++)
                    for (int dx = -outline; dx <= outline; dx++) {
                        if (dx * dx + dy * dy > outline * outline) continue;
                        int sx = x + dx, sy = y + dy;
                        if (sx < 0 || sy < 0 || sx >= size || sy >= size) continue;
                        ring = Math.Max(ring, a[sy * size + sx]);
                    }
                int ga = a[y * size + x];
                double k = ga / 255.0;
                int r = (int)Math.Round(edge.R * (1 - k) + (top.R * (1 - t) + bottom.R * t) * k);
                int gg = (int)Math.Round(edge.G * (1 - k) + (top.G * (1 - t) + bottom.G * t) * k);
                int b = (int)Math.Round(edge.B * (1 - k) + (top.B * (1 - t) + bottom.B * t) * k);
                dst.SetPixel(x, y, Color.FromArgb(Math.Max(ring, ga), r, gg, b));
            }
        }
        return dst;
    }

    // The mod icon: the glyph on a dark slate round plate with a tin rim, matching the charm's
    // blue-grey tin. Heraldry and Warden use the same plate in their own colours.
    public static Bitmap ModIcon(Bitmap glyph, int size) {
        var dst = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(dst)) {
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using (var path = new GraphicsPath()) {
                path.AddEllipse(6, 6, size - 12, size - 12);
                using (var plate = new PathGradientBrush(path) { CenterColor = Color.FromArgb(255, 54, 62, 72), SurroundColors = new[] { Color.FromArgb(255, 20, 24, 30) } })
                    g.FillPath(plate, path);
                using (var rim = new Pen(Color.FromArgb(255, 138, 156, 174), 7f))
                    g.DrawEllipse(rim, 9.5f, 9.5f, size - 19, size - 19);
            }
            g.DrawImage(glyph, new Rectangle(0, 0, size, size));
        }
        return dst;
    }
}
'@

$zip = [IO.Compression.ZipFile]::OpenRead((Resolve-Path $Archive))
try {
    $entry = $zip.GetEntry("icons/ffffff/transparent/1x1/$Icon.png")
    if (-not $entry) { throw "Icon '$Icon' not in the archive" }
    $stream = $entry.Open()
    try { $src = New-Object System.Drawing.Bitmap $stream } finally { $stream.Dispose() }
} finally {
    $zip.Dispose()
}

$item = [IconPaint]::Glyph($src, 128, 0.84, 4,
    [System.Drawing.Color]::FromArgb(244, 200, 120), [System.Drawing.Color]::FromArgb(160, 96, 40), [System.Drawing.Color]::FromArgb(34, 22, 12))
$item.Save((Join-Path (Get-Location) 'assets/icon.png'), [System.Drawing.Imaging.ImageFormat]::Png)

$glyph = [IconPaint]::Glyph($src, 256, 0.62, 6,
    [System.Drawing.Color]::FromArgb(214, 226, 236), [System.Drawing.Color]::FromArgb(112, 132, 152), [System.Drawing.Color]::FromArgb(16, 20, 26))
$mod = [IconPaint]::ModIcon($glyph, 256)
$mod.Save((Join-Path (Get-Location) 'package/icon.png'), [System.Drawing.Imaging.ImageFormat]::Png)

$item.Dispose(); $glyph.Dispose(); $mod.Dispose(); $src.Dispose()
Write-Host "Wrote assets/icon.png and package/icon.png from $Icon"
