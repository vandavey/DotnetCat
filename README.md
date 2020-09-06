
<p align="center">
    <img src="DotnetCat/Resources/Icon.ico" width=175 alt="logo">
</p>

# DotnetCat

Remote command shell application written in C#, targeting the [.NET Core 3.1 runtime](https://dotnet.microsoft.com/download/dotnet-core/current/runtime).

## Basic Usage

* Linux Systems
  
    ```bash
    dncat [OPTIONS] TARGET
    ```

* Windows Systems

    ```bat
    dncat.exe [OPTIONS] TARGET
    ```

## Available Arguments

| Argument         | Type       | Description                        | Default Value       |
|:----------------:|:----------:|:----------------------------------:|:-------------------:|
|`TARGET`          | *Required* | Target IPv4 address or host name   | *N/A*               |
|`-h/-?, --help`   | *Optional* | Display the application help menu  | *False*             |
|`-v, --verbose`   | *Optional* | Enable verbose console output      | *False*             |
|`-l, --listen`    | *Optional* | Listen for an incoming connection  | *False*             |
|`-r, --recurse`   | *Optional* | Send entire directory recursively  | *False*             |
|`-p/--port PORT`  | *Optional* | Primary local/remote port number   | *4444*              |
|`-e/--exec EXEC`  | *Optional* | Command shell executable file path | *PowerShell / Bash* |
|`-o/--output PATH`| *Optional* | Send file data to remote host      | *N/A*               |
|`-s/--send PATH`  | *Optional* | Receive file data from remote host | *N/A*               |

## Downloads

### All-In-One Executables

To download an all-in-one executable, select one of the options below:

* [Windows-x64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Win-x64.zip)
* [Windows-x86](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Win-x86.zip)
* [Linux-x64](https://github.com/vandavey/DotnetCat/raw/master/DotnetCat/bin/Zips/DotnetCat_Linux-x64.zip)

### Full Repository

The entire DotnetCat repository can be downloaded [here](https://github.com/vandavey/DotnetCat/archive/master.zip), or by clicking the green download button at the top of the page.

## Licensing

The DotnetCat application source code is available [here](https://github.com/vandavey/DotnetCat) and licensed under the [MIT license](DotnetCat/Resources/LICENSE.md).
