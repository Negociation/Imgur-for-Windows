# Re-download Imgur preset avatars from tools/imgur-avatars-manifest.csv into Imgur.Uwp/Assets/Avatars
$root = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path (Join-Path $PSScriptRoot "imgur-avatars-manifest.csv"))) {
  $root = $PSScriptRoot
  if (-not (Test-Path (Join-Path $root "tools\imgur-avatars-manifest.csv"))) {
    throw "manifest not found"
  }
}
$manifest = Join-Path $PSScriptRoot "imgur-avatars-manifest.csv"
$destDir = Join-Path (Split-Path $PSScriptRoot -Parent) "Imgur.Uwp\Assets\Avatars"
if (-not (Test-Path $destDir)) {
  $destDir = Join-Path $PSScriptRoot "..\Imgur.Uwp\Assets\Avatars"
}
New-Item -ItemType Directory -Force -Path $destDir | Out-Null
$rows = Import-Csv $manifest
$wc = New-Object System.Net.WebClient
foreach ($row in $rows) {
  $dest = Join-Path $destDir "$($row.Slug).png"
  if (Test-Path $dest) { continue }
  $url = "https://i.imgur.com/$($row.Hash)_d.png?maxwidth=155&fidelity=grand"
  Write-Host "GET $url"
  $wc.DownloadFile($url, $dest)
}
Write-Host "Done. Files in $destDir"
