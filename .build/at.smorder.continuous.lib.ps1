#Requires -Version 5.0
# https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
# $projectPath = Get-Item -Path "C:\Users\Jonas Kamsker\source\repos\server\At.Smorder.Client.Api\At.Smorder.Client.Api.csproj";

# Using Builder:
#       $builder = [Builder]::new();
#       $builder.IsVerbose = $true;
#       $builder.FindMsBuild();
#       $builder.Build($projectPath);
#       $publishFile = $builder.Pack($projectPath.Directory, @("ConnectionStrings.config", "\.bin"));

#Using Deployer
#       $deployer = [AzWebDepoy]::new($false, $true); #Bypass integrity check, Verbosity
#       $loggedIn = $deployer.IsLoggedIn();
#       $app = $deployer.FindWebApp("smorderdev-jonas");
#       $deployer.PublishZip($publishFile, $app.Id); #OPTIONAL: [string]deploymentSlot
function Get-File {
    param (
        [string]$path
    )

    try {
        return [System.IO.FileInfo]::new($path);
    }
    catch {
        return $null;    
    }
   
}

class Builder {
    
    Builder() {
      
    }

    [bool]$IsVerbose = $false;
    [bool]$IsVeryVerbose = $false;

    [System.IO.DirectoryInfo[]] GetCandidates() {
        $candidateFoldersRaw = [System.IO.Directory]::GetDirectories("$($(Get-Location).Path)\..\", "At.Smorder*") | 
        Select-Object { [System.IO.DirectoryInfo]::new($_) } | 
        Select-Object -ExpandProperty *;

        $candidateFolderNames = $candidateFoldersRaw | 
        Select-Object { $_.FullName } | 
        Select-Object -ExpandProperty ' $_.FullName ';
        
        $candidates = $candidateFolderNames | 
        Select-Object { [System.IO.DirectoryInfo]::new($_) } | 
        Select-Object -ExpandProperty *;
        # [System.IO.Directory]::GetDirectories("$($(Get-Location).Path)\..\", "At.Smorder*") | Select-Object { [System.IO.DirectoryInfo]::new($_) } | Select-Object -ExpandProperty *
        return $candidates;
        # return Get-ChildItem -Path  '..\At.Smorder.*' -Directory;
        # [System.IO.Directory]::EnumerateDirectories("..\", "At.Smorder*")
    }

    [System.IO.FileInfo] GetProjectByCandidateOrPath([string]$candidateOrPath) {
        $fileInfo = Get-File -path $candidateOrPath;
        if ($fileInfo -ne $null -and $fileInfo.Exists) {
            return $fileInfo;
        }

        $candidate = $this.GetCandidates() | Where-Object { $_.Name -eq $candidateOrPath };
        if ($candidate -eq $null ) {
            throw "Candidate '$candidateOrPath' not found";
        }

        $projects = $candidate.GetFiles("*.csproj");
        if ($projects.GetType().IsArray) {
            if ($projects.Count -ge 2) {
                throw "Multiple projects for candidate '$candidateOrPath' detected, cannot proceed";
            }
            return $projects[0];
        }
        return $projects;
    }

    [string] GetProgramFolder() {
        # $msbuild = x;
        return [Environment]::GetEnvironmentVariable($(IF ([Environment]::Is64BitOperatingSystem) { "ProgramFiles(x86)" } ELSE { "ProgramFiles" }));
    }

    [System.Management.Automation.ApplicationInfo]FindMsBuild() {
        $msBuild = Get-Command "msbuild.exe" -ErrorAction SilentlyContinue;
        if ($msBuild) { 
            return $msBuild;
        }

        $programFolder = $this.GetProgramFolder();
        $msbuildPath = "$programFolder\MSBuild\*\Bin\msbuild.exe";
        $vsMsbuildPath = "$programFolder\Microsoft Visual Studio\*\*\MSBuild\*\Bin\msbuild.exe";

        $paths = (Get-ChildItem @($msbuildPath, $vsMsbuildPath) -ErrorAction SilentlyContinue) | 
        select -expand FullName | 
        select { Get-Command $_ } | 
        sort { $_.'Get-Command $_ '.Version }
      
        if ($paths.Count -ge 1) {
            return $paths[0].' Get-Command $_ ';
        }
  
        throw 'No MsBuild Found';
        exit;
    }

    Build([System.IO.FileInfo]$projectFile) {
        if ($null -eq $projectFile) {
            throw "Cannot build Project: No project file given";
        }
  
        $ps = new-object System.Diagnostics.Process
        $ps.StartInfo.Filename = $this.FindMsBuild().Path
        $ps.StartInfo.Arguments = "`"$($projectFile.FullName)`" /t:Build /p:Configuration=Release"
        $ps.StartInfo.RedirectStandardOutput = $True
        $ps.StartInfo.UseShellExecute = $false
        $ps.StartInfo.CreateNoWindow = $true;
  
        $ps.start()
   
        if ($this.IsVerbose) {
            while ($ps.HasExited -eq $false) {
                Write-Host $ps.StandardOutput.ReadLine();
            }
        }
   
        $ps.WaitForExit()
    }

    [System.IO.FileInfo] Pack([System.IO.DirectoryInfo]$folder, [string[]]$exclude = @("")) {

        $targetFile = Get-File -Path "$($(Get-Location).Path)\$($folder.Name).zip"; #  [System.IO.FileInfo]::new();

        if ($targetFile.Exists) {
            $targetFile.Delete();
        }
      
        $fsItems = $folder.EnumerateFileSystemInfos() | 
        where { -not $_.Name.StartsWith(".") -and -not $_.Name.EndsWith(".tmp") } | 
        where { $_.Name -notin $Exclude } | 
        select -expand FullName
      
        if ($this.IsVeryVerbose) {
            Write-Host "vv selected"
            Compress-Archive -Path $fsItems -DestinationPath $targetFile.FullName -Update -CompressionLevel Fastest -Verbose

        }
        else {
            Write-Host "No vv"
            Compress-Archive -Path $fsItems -DestinationPath $targetFile.FullName -Update -CompressionLevel Fastest 
        }
  
        return $targetFile;
    }
}

class WebAppFinderArgs {
    WebAppFinderArgs([string]$name) {
        $this.Name = $name;
    }

    WebAppFinderArgs([string]$name, [string]$subscriptionId) {
        $this.Name = $name;
        $this.SubscriptionId = $subscriptionId;
    }

    [string] $Name;
    [string] $SubscriptionId;
}

class DependencyManager {
    DependencyManager() {    
    }
    # [System.Net.WebClient]::new().DownloadFile("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", "$(Get-Location)\nuget.exe")
    # [System.Net.WebClient]::new().DownloadFile("https://azurecliprod.blob.core.windows.net/msi/azure-cli-2.0.64.msi", "$(Get-Location)\azure-cli.msi")
    [bool]NugetInstalled() {
        return (Get-Command "nuget" -ErrorAction SilentlyContinue) -ne $null;
    }

    [bool]AzureCliInstalled() {
        return (Get-Command "az" -ErrorAction SilentlyContinue) -ne $null;
        # if ($null -eq $(Find-Module -Name az -RequiredVersion '2.0.0' -ErrorAction SilentlyContinue)) {
        #     return $false;
        # }
        # return $true;
    }

    InstallNuget() {
        Install-PackageProvider -Scope CurrentUser -Force -Name NuGet -MinimumVersion 2.8.5.201  
    }

    InstallAzureCli() {
        Install-Module -Scope CurrentUser -Force -Name Az -AllowClobber -RequiredVersion '2.0.0'
    }

}

class AzWebDepoy {
    hidden [bool] $dependenciesChecked = $false;
    [bool]$IsVerbose = $false;
    [PSCustomObject[]] $accountInfoCache = $null;
    $webAppListCache = $null;
    [DependencyManager] $DependencyManager;

    AzWebDepoy() {
        $this.Init($false);
    }

    AzWebDepoy([bool]$skipDependencyCheck, [bool]$isVerbose) {
        $this.IsVerbose = $isVerbose;
        $this.Init($skipDependencyCheck);
    }

    hidden Init([bool]$skipDependencyCheck) {
        $this.webAppListCache = New-Object "System.Collections.Generic.Dictionary``2[System.Guid,PSCustomObject[]]"
        $this.DependencyManager = [DependencyManager]::new();
        if ($skipDependencyCheck -eq $false) {
            $this.CheckPublishDependencies();
        }
    }

    hidden CheckPublishDependencies() {
        if ($this.dependenciesChecked) {
            if ($this.IsVerbose) {
                Write-Host "Skipping dependency checking";
            }
            return;
        }

        if ($this.IsVerbose) {
            Write-Host "Checking publishing dependencies"
            Write-Host "Checking Nuget ..."         
        }

        # Only if we want to install az 
        # if ($this.DependencyManager.NugetInstalled() -eq $false) {
        #     throw "Nuget not found (required minimum version: 2.8.5.201)"
        # }
  
        if ($this.IsVerbose) {
            Write-Host "Checking Microsoft Azure PowerShell ..."         
        }
  
        if ($this.DependencyManager.AzureCliInstalled() -eq $false) {
            throw "AZ cmdlet not found (Microsoft Azure PowerShell required version: 2.0.0)"
        }
        # return $true;
        $this.dependenciesChecked = $true;
    }

    [bool] IsLoggedIn() {
        try {
            return $this.ShowAccountInfo().Count -ne 0;
        }
        catch {
            return $false;
        }
    }

    [PSCustomObject[]] ShowAccountInfo() {
        if ($this.accountInfoCache -eq $null) {
            $accInfo = az account list | ConvertFrom-Json;
            if ($accInfo -eq $null -or ($accInfo.Count -eq 0)) {
                throw "Not logged in!"
            }
            $this.accountInfoCache = $accInfo;
        }
        return $this.accountInfoCache;
    }

    [PSCustomObject[]]GetWebAppBySubscriptionId([System.Guid]$SubscriptionId) {
        if ($this.IsLoggedIn() -eq $false) {
            throw "Not logged in!"
        }
      
        $data = $null;
        if ($this.webAppListCache.ContainsKey($SubscriptionId) -eq $false) {
            $data = (az webapp list --subscription $SubscriptionId --query '[*].{Id: id, Name: name, DefaultHostName: defaultHostName}' | ConvertFrom-Json );
            $this.webAppListCache[$SubscriptionId] = $data;
        }
        else {
            $data = $this.webAppListCache[$SubscriptionId];
        }
        return $data;
    }
    # WebAppFinderArgs
    [PSCustomObject]FindWebApp([string]$name) {
        $finderArgs = [WebAppFinderArgs]::new();
        $finderArgs.Name = $name;
        return $this.FindWebApp($finderArgs);
    }

    [PSCustomObject]FindWebApp([WebAppFinderArgs]$finderArgs) {
        if ($this.IsLoggedIn() -eq $false) {
            throw "Not logged in!"
        }

        if ($finderArgs -eq $null) {
            throw "finderArgs cannot be null"
        }
        elseif ($finderArgs.Name -eq $null -or $finderArgs.Name -eq "") {
            throw "A name is required"
        }

        $accountInfos = $this.ShowAccountInfo();
        if ($finderArgs.SubscriptionId -ne $null -and $finderArgs.SubscriptionId -ne "") {
            $accountInfos = $accountInfos | Where-Object { $_.Id -eq $finderArgs.SubscriptionId }
            $accountInfos = , $accountInfos;
        }

        if ($this.IsVerbose) {
            # $count = $(IF( $accountInfos.GetType() == [PSObject[]])  );
            Write-Host "Querying $($accountInfos.Count) subscriptions";
        }
        $result = $null;
        Foreach ($info in $accountInfos) {
            if ($this.IsVerbose) {
                Write-Host "Querying $($info.Id) - $($info.name)";
            }

            $webApp = $this.GetWebAppBySubscriptionId($info.id) | Where-Object { $_.Name -eq $finderArgs.Name };
            if ($webApp -eq $null) {
                continue;
            }
            if ($webApp.GetType() -eq [System.Management.Automation.PSCustomObject]) {
                return $webApp;
            }
            elseif ($webApp.GetType() -eq [System.Object[]]) {
                throw "Ambigious result: multiple web apps with the same name found!"
            }
        }
        throw "No webapp found!";
    }

    PublishZip([System.IO.FileInfo]$archive, [string]$webAppId) {
        $this.PublishZip($archive, $webAppId, $null);
    }

    PublishZip([System.IO.FileInfo]$archive, [string]$webAppId, [string]$deploymentSlot) {
        if ($this.IsLoggedIn() -eq $false) {
            throw "Not logged in!"
        }

        $command = "az webapp deployment source config-zip --id $webAppId --src '$($archive.FullName)'";

        if ($deploymentSlot -ne $null -and $deploymentSlot -ne "") {
            $command = "$command --slot $deploymentSlot";
        }

        if ($this.IsVerbose) {
            Write-Host "Starting deployment ...";
            $command = "$command --verbose";
        }

        Invoke-Expression $command
    }
}


class ColoredWriter {
    [string[]] $AnimatedText;
    [string[]] $NormalText;
    
    [string[]] $Colors;
    [int] $colorPtr;

    [int]$mode;

    ColoredWriter() {
        $this.Colors = @("Red", "Yellow", "Green", "Cyan", "Magenta", "DarkRed") # "Blue",
    }

    hidden CharacterWritten() {
      
        $this.currentPosition.X += 1; 
        if ($this.mode -eq 1) {
            $this.IncrementColorPtr();
        }
    }

    hidden LineWritten() {
        $this.currentPosition.Y++;
        $this.currentPosition.X = 0;
        if ($this.mode -eq 2) {
            $this.IncrementColorPtr();
        }
    }

    hidden [int] IncrementColorPtr() {
        if ($this.colorPtr -ge $this.Colors.Count - 1) {
            $this.colorPtr = 0;
        }
        
        return $this.colorPtr++;
    }

    hidden [string] GetColor() {
        if ($this.mode -eq 0) {
            return "White";
        }
        return $this.Colors[$this.colorPtr];
    }

    [object]$originalPosition;
    [object]$currentPosition;
     

    
    Write() {
        $this.originalPosition = $Script:host.UI.RawUI.CursorPosition;
        if ($this.originalPosition.X -ne 0) {
            Write-Host
        }
        $this.currentPosition = $Script:host.UI.RawUI.CursorPosition;
        foreach ($line in $this.AnimatedText) {
            for ($i = 0; $i -lt $line.Length; $i++) {
                $character = $line[$i];
                $Script:host.UI.RawUI.CursorPosition = $this.currentPosition;
                Write-Host $character -NoNewline -ForegroundColor $($this.GetColor());
                $this.CharacterWritten();
            }
          
            $this.LineWritten();
        }
        Write-Host
        if ($this.NormalText -ne $null -and $this.NormalText.Count -ne 0) {
            foreach ($line in $this.NormalText) {
                Write-Host $line;
            }
        }
        $Script:host.UI.RawUI.CursorPosition = $this.originalPosition;
    }
}

function Write-Copyright {
    Write-Host "  _________                        .___ "
    Write-Host " /   _____/ _____   ___________  __| _/___________  "
    Write-Host " \_____  \ /     \ /  _ \_  __ \/ __ |/ __ \_  __ \"
    Write-Host " /        \  Y Y  (  <_> )  | \/ /_/ \  ___/|  | \/ "
    Write-Host "/_______  /__|_|  /\____/|__|  \____ |\___  >__|"
    Write-Host "        \/      \/                  \/    \/"
    Write-Host "           Smart Order - Smart Deployment"
    Write-Host '        Smorder GmbH 2019 All Rights Reserved'
}

function Animated-Text {
    Clear-Host
    [console]::TreatControlCAsInput = $true
    $writer = [ColoredWriter]::new();
    $writer.mode = 0
    $writer.AnimatedText =  
    @(
        "  _________                        .___ "
        " /   _____/ _____   ___________  __| _/___________  "
        " \_____  \ /     \ /  _ \_  __ \/ __ |/ __ \_  __ \"
        " /        \  Y Y  (  <_> )  | \/ /_/ \  ___/|  | \/ "
        "/_______  /__|_|  /\____/|__|  \____ |\___  >__|"
        "        \/      \/                  \/    \/"
    );
    $writer.NormalText = @("           Smart Order - Smart Deployment");
    
    $i = 0;
    while ($true) {
       
        $writer.Write();
        $i++;
        if ($i -ge 20) {
            $writer.mode = $(IF ($writer.mode -eq 1) { 2 } ELSE { 1 })
            $i = 0
        }
        Start-Sleep -Milliseconds 10
        if ($Host.UI.RawUI.KeyAvailable -and (3 -eq [int]$Host.UI.RawUI.ReadKey("AllowCtrlC,IncludeKeyUp,NoEcho").Character)) {
            break;
        }
    }
    $Script:host.UI.RawUI.CursorPosition = $writer.currentPosition;
    [console]::TreatControlCAsInput = $false
    Write-Host
}

