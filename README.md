# Pty.Net (Fork)

Pty.Net is a cross platform, .NET library providing idiomatic bindings for `forkpty()`.

This is a modified fork of [microsoft/vs-pty.net](https://github.com/microsoft/vs-pty.net) with the following changes:

## Changes from upstream

- **Removed enterprise build dependencies** - Stripped out MicroBuild, Nerdbank.GitVersioning, StyleCop, and Azure DevOps feed references
- **Removed bundled ConPTY DLLs** - Windows now uses the built-in `kernel32.dll` ConPTY APIs (requires Windows 10 1809+)
- **Simplified project files** - Cleaned up `Directory.Build.props` and `Pty.Net.csproj`
- **Removed nuget.config** - No longer requires authenticated Azure DevOps feeds

## Platform Support

| Platform | Implementation |
|----------|---------------|
| Windows 10 1809+ | `kernel32.dll` (ConPTY) |
| Linux | `libc.so.6` + `libutil.so.1` (forkpty) |
| macOS | `libSystem.dylib` (forkpty) |

## Requirements

- .NET Standard 2.0 compatible runtime
- Windows 10 version 1809 or later (for Windows support)

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
