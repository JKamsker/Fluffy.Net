param (
    [String]$creds
)
#change
# Write-Host "Creds:: $creds";
# Write-Host "Credstype:: $($creds.GetType())"
# return;


if ((Get-Command "nuget.exe" -ErrorAction SilentlyContinue) -eq $null) {
    throw  "NUGET NOT FOUND!"
}

$items = Get-Item ..\*\*\ReleaseBuild\*.nupkg
foreach ($item in $items) {
   Write-Host "Publishing $($item.Name)"
   try {
        nuget push $($item.FullName) -Source "https://api.nuget.org/v3/index.json" -ApiKey "$creds" -Verbosity detailed
   }
   catch {
       Write-Host "Version already published"
   }
 
}


Write-Host "Publishing finished"
$LASTEXITCODE = 0;
