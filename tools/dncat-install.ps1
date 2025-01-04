<#
.SYNOPSIS
    DotnetCat installer script for x64 and x86 Windows systems.
.DESCRIPTION
    DotnetCat remote command shell application
    installer script for x64 and x86 Windows systems.
.LINK
    Application repository: https://github.com/vandavey/DotnetCat
#>
using namespace System.Runtime.InteropServices
using namespace System.Security.Principal

[CmdletBinding()]
param ()

$DefaultErrorPreference = $ErrorActionPreference
$DefaultProgressPreference = $ProgressPreference

# Reset the global preference variables.
function Reset-Preferences {
    $ErrorActionPreference = $DefaultErrorPreference
    $ProgressPreference = $DefaultProgressPreference
}

# Write an error message to stderr and exit.
function Show-Error {
    $Symbol = "[x]"
    Reset-Preferences

    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $Symbol = "`e[91m${Symbol}`e[0m"
    }
    [Console]::Error.WriteLine("${Symbol} ${args}`n")
    exit 1
}

# Write a status message to stdout.
function Show-Status {
    $Symbol = "[*]"

    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $Symbol = "`e[96m${Symbol}`e[0m"
    }
    Write-Output "${Symbol} ${args}"
}

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Require Windows operating system
if (-not [RuntimeInformation]::IsOSPlatform([OSPlatform]::Windows)) {
    Show-Error "Windows operating system required"
}

$RepoRoot = "https://raw.githubusercontent.com/vandavey/DotnetCat/master"
$UserPrincipal = [WindowsPrincipal]::new([WindowsIdentity]::GetCurrent())

# Require elevated shell privileges
if (-not $UserPrincipal.IsInRole([WindowsBuiltInRole]::Administrator)) {
    Show-Error "Administrator shell privileges required"
}

# Validate CPU architecture and set variables
if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") {
    $AppDir = "${env:ProgramFiles}\DotnetCat"
    $ZipUrl = "${RepoRoot}/src/DotnetCat/bin/Zips/DotnetCat_win-x64.zip"
}
elseif ($env:PROCESSOR_ARCHITECTURE -eq "x86") {
    $AppDir = "${env:ProgramFiles(x86)}\DotnetCat"
    $ZipUrl = "${RepoRoot}/src/DotnetCat/bin/Zips/DotnetCat_win-x86.zip"
}
else {
    Show-Error "Unsupported processor architecture: '${env:PROCESSOR_ARCHITECTURE}'"
}

# Remove existing installation
if (Test-Path $AppDir) {
    Show-Status "Removing existing installation from '${AppDir}'..."
    Remove-Item $AppDir -Recurse -Force
}

Show-Status "Creating install directory '${AppDir}'..."
New-Item $AppDir -ItemType Directory -Force > $null

$ZipPath = "${AppDir}\dncat.zip"
Show-Status "Downloading temporary zip file to '${ZipPath}'..."

# Download application zip file
try {
    Invoke-WebRequest $ZipUrl -DisableKeepAlive -OutFile $ZipPath
}
catch {
    $ErrorMsg = $Error[0].ErrorDetails.Message
    Show-Error "Failed to download '$(Split-Path $ZipUrl -Leaf)' (${ErrorMsg})"
}

Show-Status "Installing application files to '${AppDir}'..."
Expand-Archive $ZipPath $AppDir -Force > $null

Show-Status "Deleting temporary zip file '${ZipPath}'..."
Remove-Item $ZipPath -Force

$EnvTarget = [EnvironmentVariableTarget]::Machine
$EnvPath = [Environment]::GetEnvironmentVariable("Path", $EnvTarget)

# Add application directory to environment path
if (-not $EnvPath.Contains($AppDir)) {
    if ($EnvPath -and -not $EnvPath.EndsWith(";")) {
        $EnvPath += ";"
    }

    $EnvPath += "${AppDir};"
    [Environment]::SetEnvironmentVariable("Path", $EnvPath, $EnvTarget)

    if ($?) {
        Show-Status "Added '${AppDir}' to environment path"
    }
    else {
        Show-Error "Failed to add '${AppDir}' to environment path"
    }
}

Reset-Preferences
Show-Status "DotnetCat was successfully installed, please restart your shell`n"
