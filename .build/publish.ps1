$items = Get-Item ..\*\*\ReleaseBuild\*.nupkg

if (Get-Command "nuget.exe" -ErrorAction SilentlyContinue) {
    throw  "NUGET FOUND!"
}
throw  "NUGET NOT FOUND!"

Write-Host $items