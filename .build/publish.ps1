param (
    [String]$creds
)

if ((Get-Command "nuget.exe" -ErrorAction SilentlyContinue) -eq $null) {
    throw  "NUGET NOT FOUND!"
}

$items = Get-Item ..\*\*\ReleaseBuild\*.nupkg
foreach ($item in $items) {
   Write-Host "Publishing $($item.Name)"
   nuget push $($item.FullName) -Source "https://api.nuget.org/v3/index.json" -ApiKey $creds -Verbosity detailed
}


Write-Host "Publishing finished"