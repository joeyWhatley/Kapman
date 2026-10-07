# Rebuilds the mockup page and screenshots it in the states named on the command line (hash states joined by "+").
param([string[]]$states = @("port"))
$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
& "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" "$dir\build.py" 2>$null | Select-Object -Last 1
$chrome = "C:\Program Files\Google\Chrome\Application\chrome.exe"
$wrap = "$dir\preview.html"
"<!doctype html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><style>body{margin:0}</style></head><body>" + (Get-Content "$dir\kapman_table.html" -Raw -Encoding UTF8) + "</body></html>" | Set-Content $wrap -Encoding UTF8
foreach ($s in $states) {
  $shot = "$dir\v2_$($s -replace '\+','_').png"
  $url = "file:///" + ($wrap -replace '\\','/') + "#" + $s
  $p = Start-Process $chrome -ArgumentList "--headless=new","--disable-gpu","--hide-scrollbars","--window-size=1320,960","--virtual-time-budget=5000","--screenshot=$shot",$url -Wait -PassThru
  "$s -> exit $($p.ExitCode), $(Test-Path $shot)"
}
