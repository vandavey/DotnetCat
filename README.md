<div align="center">
    <img src="src/DotnetCat/Resources/Icon.ico" width=175px alt="logo">
</div>

# DotnetCat

<div>
    <a href="https://learn.microsoft.com/en-us/dotnet/csharp">
        <img src="https://img.shields.io/badge/c%23-v12-9325ff" alt="csharp-12">
    </a>
    <a href="LICENSE.md">
        <img src="https://img.shields.io/github/license/vandavey/DotnetCat" alt="license">
    </a>
    <a href="https://github.com/vandavey/DotnetCat/graphs/contributors">
        <img src="https://img.shields.io/github/contributors/vandavey/DotnetCat?color=blue" alt="contributors">
    </a>
    <a href="https://github.com/vandavey/DotnetCat/pulls">
        <img src="https://img.shields.io/github/issues-pr/vandavey/DotnetCat" alt="pull-requests">
    </a>
</div>

Remote command shell application written in C#,
targeting the [.NET 8 runtime](https://dotnet.microsoft.com/download/dotnet/8.0).

***

## Overview

DotnetCat is a multithreaded console application that can be used to spawn bind and reverse
command shells, upload and download files, perform connection testing, and transmit user-defined
payloads. This application uses [Transmission Control Protocol (TCP)](https://www.ietf.org/rfc/rfc9293.html)
network sockets to perform network communications.

At its core, DotnetCat is built of unidirectional TCP [socket pipelines](src/DotnetCat/IO/Pipelines),
each responsible for asynchronously reading from or writing to a connected socket. This allows a
single socket stream to be used by multiple pipelines simultaneously without thread lock issues
occurring.

### Features

* Bind command shells
* Reverse command shells
* Remote file uploads and downloads
* Connection probing
* User-defined data transmission

***

## Basic Usage

* Linux Systems

    ```bash
    dncat [OPTIONS] TARGET
    ```

* Windows Systems

    ```powershell
    dncat.exe [OPTIONS] TARGET
    ```

***

## Command-Line Arguments

All available DotnetCat arguments are listed in the following table:

| Argument           | Type       | Description                        | Default |
|:------------------:|:----------:|:----------------------------------:|:-------:|
| `TARGET`           | *Required* | Host to use for the connection     | *N/A*   |
| `-p/--port PORT`   | *Optional* | Port to use for the connection     | *44444* |
| `-e/--exec EXEC`   | *Optional* | Pipe executable I/O data (shell)   | *N/A*   |
| `-o/--output PATH` | *Optional* | Download a file from a remote host | *N/A*   |
| `-s/--send PATH`   | *Optional* | Send a local file to a remote host | *N/A*   |
| `-t, --text`       | *Optional* | Send a string to a remote host     | *False* |
| `-l, --listen`     | *Optional* | Listen for an inbound connection   | *False* |
| `-z, --zero-io`    | *Optional* | Determine if an endpoint is open   | *False* |
| `-v, --verbose`    | *Optional* | Enable verbose console output      | *False* |
| `-d, --debug`      | *Optional* | Enable verbose error output        | *False* |
| `-h/-?, --help`    | *Optional* | Display the app help menu and exit | *False* |

> See the [usage examples](#usage-examples) section for more information.

***

## Installation

### Installers (Recommended)

The [tools](tools) directory contains Linux and Windows installer and uninstaller scripts.

#### Linux Systems

The [dncat-install.sh](tools/dncat-install.sh) installer can be used to
install DotnetCat on Linux systems.

The following code snippet can be executed in Bash to download and run the 
[dncat-install.sh](tools/dncat-install.sh) DotnetCat installer:

```bash
curl -sLS "https://raw.githubusercontent.com/vandavey/DotnetCat/master/tools/dncat-install.sh" | bash
```

DotnetCat can be uninstalled using the [dncat-uninstall.sh](tools/dncat-uninstall.sh) uninstaller.

<blockquote>
    <a href="tools/dncat-install.sh">dncat-install.sh</a> and
    <a href="tools/dncat-uninstall.sh">dncat-uninstall.sh</a> only support ARM64 and x64
    Linux systems and require <code><a href="https://curl.se">curl</a></code> and
    <code><a href="https://infozip.sourceforge.net/UnZip.html">unzip</a></code> to be installed.
</blockquote>

#### Windows Systems

The [dncat-install.ps1](tools/dncat-install.ps1) installer can be used to
install DotnetCat on Windows systems.

The following code snippet can be executed in PowerShell to download and run the 
[dncat-install.ps1](tools/dncat-install.ps1) DotnetCat installer:

```powershell
$Uri = "https://raw.githubusercontent.com/vandavey/DotnetCat/master/tools/dncat-install.ps1"
Write-Output (Invoke-WebRequest $Uri -DisableKeepAlive).Content | powershell.exe -
```

DotnetCat can be uninstalled using the [dncat-uninstall.ps1](tools/dncat-uninstall.ps1) uninstaller.

> [dncat-install.ps1](tools/dncat-install.ps1) and [dncat-uninstall.ps1](tools/dncat-uninstall.ps1)
  only support x64 and x86 Windows systems and must be executed as an administrator.

### Manual Setup

The entire DotnetCat source code repository can be downloaded
[here](https://github.com/vandavey/DotnetCat/archive/master.zip).

To download a precompiled standalone executable, select one of the options below:

* [Linux-x64](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_linux-x64.zip)
* [Linux-ARM64](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_linux-arm64.zip)
* [Windows-x64](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_win-x64.zip)
* [Windows-x86](https://raw.githubusercontent.com/vandavey/DotnetCat/master/src/DotnetCat/bin/Zips/DotnetCat_win-x86.zip)

***

## Usage Examples

### Basic Operations

* Display the application help menu, then exit:

    ```powershell
    dncat --help
    ```

* Connect to remote endpoint `192.168.1.1:1524`:

    ```powershell
    dncat "192.168.1.1" --port 1524
    ```

* Listen for an inbound connection on any local Wi-Fi interface:

    ```powershell
    dncat --listen
    ```

    > When the `-l` or `--listen` flag is specified, `TARGET` defaults to `0.0.0.0`.

* Determine if `localhost` is accepting connections on port `22`:

    ```powershell
    dncat -z localhost -p 22
    ```

### Command Shells

* Connect to remote endpoint `127.0.0.1:4444` to establish a bind shell:

    ```powershell
    dncat "127.0.0.1" -p 4444
    ```

* Listen for an inbound connection to establish a reverse `bash` shell:

    ```powershell
    dncat -lv --exec bash
    ```

### Data Transfer

* Transmit string payload *Hello world!* to remote endpoint `fake.addr.com:80`:

    ```powershell
    dncat -dt "Hello world!" fake.addr.com -p 80
    ```

#### File Transfer

* Listen for inbound file data and write the contents to path `C:\TestFile.txt`:

    ```powershell
    dncat -lvo C:\TestFile.txt
    ```

* Transmit the contents of file `/home/user/profit.json` to remote target `Joe-Mama`:

    ```powershell
    dncat --send /home/user/profit.json Joe-Mama
    ```

***

## Remarks

* This application is designed to be used as a multi-functional command-line
  networking tool, and should only be used on your own systems.

* Please use discretion, as this application is still in development.

* In no event shall the authors or copyright holders of this software be liable for
  any claim, damages or other liability arising from, out of or in connection with
  the software or the use or other dealings in the software.

    > For more information, click [here](LICENSE.md) to view the
      software's [MIT license](LICENSE.md).

***

## Copyright & Licensing

The DotnetCat application source code is available [here](https://github.com/vandavey/DotnetCat)
and licensed under the [MIT license](LICENSE.md).
