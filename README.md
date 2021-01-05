<p align="center">
<img src="DotnetCat/Resources/Icon.ico" width=175 alt="logo">
    </p>

# DotnetCat

Remote command shell application written in C#,
targeting the [.NET Core 3.1 runtime](https://dotnet.microsoft.com/download/dotnet-core/current/runtime).

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

| Argument           | Type       | Description                | Default |
|:------------------:|:----------:|:--------------------------:|:-------:|
| `TARGET`           | *Required* | Target address or host     | *N/A*   |
| `-p/--port PORT`   | *Optional* | Primary local/remote port  | *4444*  |
| `-e/--exec EXEC`   | *Optional* | Command shell executable   | *N/A*   |
| `-o/--output PATH` | *Optional* | Receive a remote file      | *N/A*   |
| `-s/--send PATH`   | *Optional* | Send local file/directory  | *N/A*   |
| `-l, --listen`     | *Optional* | Listen for connection      | *False* |
| `-v, --verbose`    | *Optional* | Enable verbose output      | *False* |
| `-r, --recurse`    | *Optional* | Send directory recursively | *False* |
| `-h/-?, --help`    | *Optional* | Display the help menu      | *False* |

> Note: The `-r/--recurse` option is still in development and should be avoided
  in the meantime

***

## Download Options

### Standalone Executable

To download a prebuilt, standalone executable, select one of the options below:

* [Windows-x64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Win-x64.zip)
* [Windows-x86](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Win-x86.zip)
* [Linux-x64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Linux-x64.zip)
* [Linux-arm64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Linux-arm64.zip)

### Full Repository

The entire DotnetCat source code repository can be downloaded [here](https://github.com/vandavey/DotnetCat/archive/master.zip).

***

## Remarks

This application is still in development, please use caution.

***

## Copyright & Licensing

The DotnetCat application source code is available [here](https://github.com/vandavey/DotnetCat)
and licensed under the [MIT license](LICENSE.md).
