# Sharelord (lowercase l)
![](https://github.com/NocteDefensor/ShareLord/blob/main/starlord-dance.gif)

.NET Assembly to learn how to interact with Win32 APIs to create network shares, delete shares, and set share perms. It uses .NET methods to set ACE. Will eventually switch all to use Win32 APIs. Currently using PInvoke. I hope to switch to DInvoke soon when I figure out an memory access violation i'm running into. Help with DInvoke would be greatly appreciated.
---
Sharelord is a command-line tool for managing Windows shared folders and their permissions.

## Features

- Create and delete shared folders
- Set file system access control entries (ACEs)
- Set share-level permissions

## Usage

Run `Sharelord.exe --help` for detailed information on available commands and options.

Basic syntax:

```
Sharelord.exe <command> [options]
```

## Examples

Create a shared folder:
```
Sharelord.exe create C:\SharedFolder MyShare -r "Shared folder for team"
```

Set file system permissions:
```
Sharelord.exe setace C:\SharedFolder Domain\User Modify Allow
```

Set share-level permissions:
```
Sharelord.exe setshareperms MyShare Domain\User Change Allow
```

Delete a share:
```
Sharelord.exe delete MyShare
```

## Note

For a full list of commands, options, and permissions, refer to the built-in help menu by running `Sharelord.exe --help`.

## Build Instructions

This project is a .NET application targeting .NET Framework 4.8.

### Prerequisites

To build and run this project, you'll need:

- [Visual Studio Code](https://code.visualstudio.com/)
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- [C# extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

### Dependencies

The project has the following dependencies:

- System.IO.FileSystem.AccessControl (5.0.0)
- System.Security.AccessControl (6.0.1)
- System.Security.Principal.Windows (5.0.0)


### Building the Project

1. Open the project folder in Visual Studio Code.
2. Open the integrated terminal in VS Code (View > Terminal).
3. Run the following command to build the project:

   ```
   dotnet build
   ```

4. If you want to create a release build, use:

   ```
   dotnet build -c Release
   ```

### Notes

- The project is configured to use the latest C# language version.
- Unsafe blocks are allowed in the code.
- In Release configuration, debug symbols are not generated.

For more detailed information about the project structure and configuration, refer to the `.csproj` file.

## References
- https://github.com/TheWover/DInvoke
- https://github.com/rasta-mouse/DInvoke
- https://www.pinvoke.net/default.aspx/netapi32/_ContentBaseDefinition.html
- https://learn.microsoft.com/en-us/windows/win32/api/lmshare/nf-lmshare-netshareadd
- https://learn.microsoft.com/en-us/windows/win32/api/lmshare/nf-lmshare-netsharedel
- https://learn.microsoft.com/en-us/windows/win32/api/lmshare/nf-lmshare-netsharesetinfo
- https://learn.microsoft.com/en-us/windows/win32/api/lmshare/ns-lmshare-share_info_2
- https://learn.microsoft.com/en-us/windows/win32/api/lmshare/ns-lmshare-share_info_502
