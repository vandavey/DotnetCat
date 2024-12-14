<#
.SYNOPSIS
    DotnetCat uninstaller script for x64 and x86 Windows systems.
.DESCRIPTION
    DotnetCat remote command-shell application uninstaller script
    for x64 and x86 Windows systems.
.LINK
    Application repository: https://github.com/vandavey/DotnetCat
#>
using namespace System.Runtime.InteropServices
using namespace System.Security.Principal

[CmdletBinding()]
param ()

# Write an error message to stderr and exit.
function Show-Error {
    $Symbol = "[x]"

    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $Symbol = "`e[91m${Symbol}`e[0m"
    }
    [Console]::Error.WriteLine("${Symbol} ${args}`n")
    exit 1
}

# Write a status message to stdout.
function Show-Status {
    $Symbol = "[*]"

    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $Symbol = "`e[96m${Symbol}`e[0m"
    }
    Write-Output "${Symbol} ${args}"
}

# Require Windows operating system
if (-not [RuntimeInformation]::IsOSPlatform([OSPlatform]::Windows)) {
    Show-Error "Windows operating system required"
}

$UserPrincipal = [WindowsPrincipal]::new([WindowsIdentity]::GetCurrent())

# Require elevated shell privileges
if (-not $UserPrincipal.IsInRole([WindowsBuiltInRole]::Administrator)) {
    Show-Error "Administrator shell privileges required"
}

# Validate CPU architecture and set variables
if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") {
    $AppDir = "${env:ProgramFiles}\DotnetCat"
}
elseif ($env:PROCESSOR_ARCHITECTURE -eq "x86") {
    $AppDir = "${env:ProgramFiles(x86)}\DotnetCat"
}
else {
    Show-Error "Unsupported processor architecture: '${env:PROCESSOR_ARCHITECTURE}'"
}

# Remove all application files
if (Test-Path $AppDir) {
    Show-Status "Removing application files from '${AppDir}'..."
    Remove-Item $AppDir -Force -Recurse
}
else {
    Show-Status "No application files to remove from '${AppDir}'"
}

$EnvTarget = [EnvironmentVariableTarget]::Machine
$EnvPath = [Environment]::GetEnvironmentVariable("Path", $EnvTarget)

# Remove application directory from environment path
if ($EnvPath.Contains($AppDir)) {
    if ($EnvPath -eq $AppDir -or $EnvPath -eq "${AppDir};") {
        $EnvPath = [string]::Empty
    }
    elseif ($EnvPath.StartsWith($AppDir)) {
        $EnvPath = $EnvPath.Replace("${AppDir};", $null)
    }
    else {
        $EnvPath = $EnvPath.Replace(";${AppDir}", $null)
    }

    [Environment]::SetEnvironmentVariable("Path", $EnvPath, $EnvTarget)

    if ($?) {
        Show-Status "Removed '${AppDir}' from environment path"
    }
    else {
        Show-Error "Failed to remove '${AppDir}' from environment path"
    }
}

Show-Status "DotnetCat was successfully uninstalled"
