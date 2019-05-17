param (
    [switch]$xx,
    [switch]$xy,
    [String]$creds
)

if ($xx) {
    Write-Host "XX set"
}else{
    Write-Host "XX not set"
}
if ($xy) {
    Write-Host "xy set"
}else{
    Write-Host "xy not set"
}

Write-Host "Creds: $creds"

$items = Get-Item ..\*\*\ReleaseBuild\*.nupkg

if (Get-Command "nuget.exe" -ErrorAction SilentlyContinue) {
    throw  "NUGET FOUND!"
}
throw  "NUGET NOT FOUND!"

Write-Host $items