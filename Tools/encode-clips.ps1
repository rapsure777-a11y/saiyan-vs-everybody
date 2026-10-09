# Encodes the frame sequences written by VisualReportTests (Logs/frames/<clip>/fNNNN.png) into MP4 files in AI_HANDOFF/SCREENSHOTS.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File Tools\encode-clips.ps1 -Tag M1_2026-10-09
param([string]$Tag = "M1_2026-10-09")
$root = Split-Path $PSScriptRoot -Parent
$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
if (-not $ffmpeg) { $ffmpeg = Get-ChildItem "$env:LOCALAPPDATA\Microsoft\WinGet\Packages" -Recurse -Filter ffmpeg.exe -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName }
if (-not $ffmpeg) { "ffmpeg not found"; exit 1 }
$out = Join-Path $root "AI_HANDOFF\SCREENSHOTS"
New-Item -ItemType Directory -Force $out | Out-Null
foreach ($d in Get-ChildItem (Join-Path $root "Logs\frames") -Directory) {
    $mp4 = Join-Path $out ($Tag + "_video_" + $d.Name + ".mp4")
    & $ffmpeg -y -loglevel error -framerate 30 -i (Join-Path $d.FullName "f%04d.png") -c:v libx264 -pix_fmt yuv420p -crf 23 -movflags +faststart $mp4
    "{0}: {1:N1} MB" -f $mp4, ((Get-Item $mp4).Length / 1MB)
}
