<div align="center">
    <img src="src/DotnetCat/Resources/Icon.ico" width=175px alt="logo" />
</div>

# DotnetCat

<div align="left">
    <img src="https://img.shields.io/badge/c%23-v13-9325ff" alt="csharp-13-badge" />
    <img src="https://img.shields.io/github/license/vandavey/DotnetCat" alt="license-badge" />
    <img src="https://img.shields.io/github/contributors/vandavey/DotnetCat?color=blue" alt="contributors-badge" />
    <img src="https://img.shields.io/github/issues-pr/vandavey/DotnetCat" alt="pull-requests-badge" />
</div>

Remote command shell application written in C# targeting the
[.NET 9.0 runtime](https://dotnet.microsoft.com/download/dotnet/9.0).

***

## Overview

DotnetCat is a multithreaded console application that can be used to spawn bind and reverse
command shells, upload and download files, perform connection testing, and transmit user-defined
payloads. This application uses [Transmission Control Protocol (TCP)](https://www.ietf.org/rfc/rfc9293.html)
network sockets to perform network communications.

At its core, DotnetCat is built of unidirectional TCP [socket pipelines](src/DotnetCat/IO/Pipelines),
each responsible for asynchronously reading from or writing to a connected socket. This allows a
single socket stream to be used by multiple pipelines simultaneously without deadlock issues
occurring.

### Features

* Bind command shells
* Reverse command shells
* Remote file uploads and downloads
* Connection probing
* User-defined data transmission

***

## Basic Usage

### Linux Systems

```bash
dncat [OPTIONS] TARGET
```

### Windows Systems

```powershell
dncat.exe [OPTIONS] TARGET
```

***

## Command-Line Options

All available DotnetCat command-line arguments are listed below:

| Argument           | Type       | Description                        | Default          |
|:------------------:|:----------:|:----------------------------------:|:----------------:|
| `TARGET`           | *Required* | Host to use for the connection     | *N/A or 0.0.0.0* |
| `-p/--port PORT`   | *Optional* | Port to use for the connection     | *44444*          |
| `-e/--exec EXEC`   | *Optional* | Pipe executable I/O data (shell)   | *N/A*            |
| `-o/--output PATH` | *Optional* | Download a file from a remote host | *N/A*            |
| `-s/--send PATH`   | *Optional* | Send a local file to a remote host | *N/A*            |
| `-t, --text`       | *Optional* | Send a string to a remote host     | *False*          |
| `-l, --listen`     | *Optional* | Listen for an inbound connection   | *False*          |
| `-z, --zero-io`    | *Optional* | Determine if an endpoint is open   | *False*          |
| `-v, --verbose`    | *Optional* | Enable verbose console output      | *False*          |
| `-h/-?, --help`    | *Optional* | Display the app help menu and exit | *False*          |

> See the [Usage Examples](#usage-examples) section for more information.

***

## Installation

DotnetCat can be automatically configured and installed or
updated using the installers in the [tools](tools) directory.

It can be installed manually by building from source or using the precompiled
standalone executables in the [Zips](src/DotnetCat/bin/Zips) directory.

### Linux Systems

Download and execute the [dncat-install.sh](tools/dncat-install.sh) installer script using Bash:

```bash
curl -sLS "https://raw.githubusercontent.com/vandavey/DotnetCat/master/tools/dncat-install.sh" | bash
```

<blockquote>
    <a href="tools/dncat-install.sh">dncat-install.sh</a> only supports <em>ARM64</em> and
    <em>x64</em> architectures and is dependent on <code><a href="https://www.7-zip.org">7-Zip</a></code>
    and <code><a href="https://curl.se">curl</a></code>.
</blockquote>

### Windows Systems

Download and execute the [dncat-install.ps1](tools/dncat-install.ps1) installer script using PowerShell:

```powershell
irm -d "https://raw.githubusercontent.com/vandavey/DotnetCat/master/tools/dncat-install.ps1" | powershell -
```

> [dncat-install.ps1](tools/dncat-install.ps1) only supports *x64*
  and *x86* architectures and must be executed as an administrator.

### Manual Setup

DotnetCat can be manually installed using the following precompiled standalone executables:

* [Linux-x64](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_linux-x64.zip)
* [Linux-ARM64](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_linux-arm64.zip)
* [Windows-x64](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_win-x64.zip)
* [Windows-x86](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_win-x86.zip)

It can be built from source by publishing [DotnetCat.csproj](src/DotnetCat/DotnetCat.csproj) using
the publish profiles in the [PublishProfiles](src/DotnetCat/Properties/PublishProfiles) directory.

***

## Uninstallation

DotnetCat can be uninstalled automatically using the uninstallers in the [tools](tools) directory.

It can be uninstalled manually by deleting the install
directory and removing it from the local environment path.

### Linux Systems

Execute the [dncat-uninstall.sh](tools/dncat-uninstall.sh) uninstaller script using Bash:

```bash
source /opt/dncat/bin/dncat-uninstall.sh
```

> [dncat-uninstall.sh](tools/dncat-uninstall.sh) only supports *ARM64* and *x64* architectures.

### Windows Systems

Execute the [dncat-uninstall.ps1](tools/dncat-uninstall.ps1) uninstaller script using PowerShell:

```powershell
gc "${env:ProgramFiles}\DotnetCat\dncat-uninstall.ps1" | powershell -
```

> [dncat-uninstall.ps1](tools/dncat-uninstall.ps1) only supports *x64*
  and *x86* architectures and must be executed as an administrator.

***

## Usage Examples

### Basic Operations

Print the application help menu, then exit:

```powershell
dncat --help
```

Connect to remote endpoint `192.168.1.1:1524`:

```powershell
dncat "192.168.1.1" --port 1524
```

Listen for an inbound connection on any local Wi-Fi interface:

```powershell
dncat --listen
```

> `TARGET` defaults to `0.0.0.0` when the `-l` or `--listen` flag is specified.

Determine whether `localhost` is accepting connections on port `22`:

```powershell
dncat -z localhost -p 22
```

### Command Shells

Connect to remote endpoint `127.0.0.1:4444` to establish a bind shell:

```powershell
dncat "127.0.0.1" -p 4444
```

Listen for an inbound connection to establish a reverse `bash` shell:

```powershell
dncat -lv --exec bash
```

### Data Transfer

Transmit string payload *Hello world!* to remote endpoint `fake.addr.com:80`:

```powershell
dncat -vt "Hello world!" fake.addr.com -p 80
```

### File Transfer

Listen for inbound file data and write the contents to path `C:\TestFile.txt`:

```powershell
dncat -lvo C:\TestFile.txt
```

Transmit the contents of file `/home/user/profit.json` to remote target `Joe-Mama`:

```powershell
dncat --send /home/user/profit.json Joe-Mama
```

***

## Remarks

* This application only supports Linux and Windows operating systems.
* Please use discretion as this application is still in development.

***

## Copyright & Licensing

DotnetCat is licensed under the [MIT license](LICENSE.md) and officially
hosted in [this](https://github.com/vandavey/DotnetCat) repository.
