# Re-prints kapman_paper_prototype.html to kapman_paper_prototype.pdf using Microsoft Edge (headless).
# Usage: right-click > Run with PowerShell, or:  powershell -ExecutionPolicy Bypass -File build_pdf.ps1
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$html = Join-Path $here "kapman_paper_prototype.html"
$pdf  = Join-Path $here "kapman_paper_prototype.pdf"
$edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$url  = ([System.Uri]$html).AbsoluteUri

& $edge --headless --disable-gpu --no-pdf-header-footer --print-to-pdf-no-header `
    --virtual-time-budget=5000 "--print-to-pdf=$pdf" $url | Out-Null
Start-Sleep -Milliseconds 500
if (Test-Path $pdf) { Write-Host "Wrote $pdf" } else { Write-Host "PDF not created" }
