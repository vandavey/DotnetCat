<p align="center">
    <img src="DotnetCat/Resources/Icon.ico" width=175 alt="logo">
</p>

# DotnetCat

Remote command shell application written in C#,
targeting the [.NET 5 runtime](https://dotnet.microsoft.com/download/dotnet/5.0).

## Basic Usage

* Linux Systems

    ```bash
    dncat [OPTIONS] TARGET
    ```

* Windows Systems

    ```powershell
    dncat.exe [OPTIONS] TARGET
    ```

## Available Arguments

All available DotnetCat arguments are listed in the following table:

| Argument           | Type       | Description                    | Default |
|:------------------:|:----------:|:------------------------------:|:-------:|
| `TARGET`           | *Required* | Host to use for the connection | *N/A*   |
| `-p/--port PORT`   | *Optional* | Port to use for the connection | *4444*  |
| `-e/--exec EXEC`   | *Optional* | Local executable file path     | *N/A*   |
| `-o/--output PATH` | *Optional* | Download file from remote host | *N/A*   |
| `-s/--send PATH`   | *Optional* | Send local file to remote host | *N/A*   |
| `-t, --text`       | *Optional* | Send a string to remote host   | *False* |
| `-l, --listen`     | *Optional* | Listen for incoming connection | *False* |
| `-v, --verbose`    | *Optional* | Enable verbose console output  | *False* |
| `-d, --debug`      | *Optional* | Enable verbose error output    | *False* |
| `-h/-?, --help`    | *Optional* | Display help menu and exit     | *False* |

***

## Download Options

### Standalone Executable

To download a prebuilt, standalone executable, select one of the options below:

* [Windows-x64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Win-x64.zip)
* [Windows-x86](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Win-x86.zip)
* [Linux-x64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Linux-x64.zip)
* [Linux-arm64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Linux-arm64.zip)

### Full Repository

The entire DotnetCat source code repository can be downloaded
[here](https://github.com/vandavey/DotnetCat/archive/master.zip).

***

## Remarks

* Please use discretion, as this application is still in development.

***

## Copyright & Licensing

The DotnetCat application source code is available [here](https://github.com/vandavey/DotnetCat)
and licensed under the [MIT license](LICENSE.md).
