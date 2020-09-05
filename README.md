
<p align="center">
    <img src="DotnetCat/Resources/Icon.ico" width=175 alt="logo">
</p>

# DotnetCat

Remote command shell application written in C#, using the [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet-core/current/runtime).

## Arguments

| Argument         | Type       | Description                        | Default Value       |
|:----------------:|:----------:|:----------------------------------:|:-------------------:|
|`TARGET`          | *Required* | Target IPv4 address or host name   | *N/A*               |
|`-h/-?, --help`   | *Optional* | Display the application help menu  | *False*             |
|`-v, --verbose`   | *Optional* | Enable verbose console output      | *False*             |
|`-l, --listen`    | *Optional* | Listen for an incoming connection  | *False*             |
|`-r, --recurse`   | *Optional* | Send directory files recursively   | *False*             |
|`-p/--port PORT`  | *Optional* | Primary loca/remote port number    | *4444*              |
|`-e/--exec EXEC`  | *Optional* | Command shell executable file path | *PowerShell / Bash* |
|`-o/--output PATH`| *Optional* | Send file data to remote host      | *N/A*               |
|`-s/--send PATH`  | *Optional* | Receive file data from remote host | *N/A*               |

## Basic Usage

* Linux Systems
  
    ```bat
    dncat [OPTIONS] TARGET
    ```

* Windows Systems

    ```bat
    dncat.exe [OPTIONS] TARGET
    ```

## Copyright & Licensing

The DotnetCat application source code is available on https://github.com/vandavey/DotnetCat and licensed under the [MIT license](DotnetCat/Resources/LICENSE.md).
