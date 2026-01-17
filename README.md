# Pty.Net (Fork)

Pty.Net is a cross platform, .NET library providing idiomatic bindings for `forkpty()`.

This is a modified fork of [microsoft/vs-pty.net](https://github.com/microsoft/vs-pty.net) with the following changes:

## Changes from upstream

- **Removed enterprise build dependencies** - Stripped out MicroBuild, Nerdbank.GitVersioning, StyleCop, and Azure DevOps feed references
- **Removed bundled ConPTY DLLs** - Windows now uses the built-in `kernel32.dll` ConPTY APIs (requires Windows 10 1809+)
- **Simplified project files** - Cleaned up `Directory.Build.props` and `Pty.Net.csproj`
- **Removed nuget.config** - No longer requires authenticated Azure DevOps feeds
- **Updated to .NET 10** - Targets `net10.0` with modern C# features and implicit usings

## Platform Support

| Platform | Implementation |
|----------|---------------|
| Windows 10 1809+ | `kernel32.dll` (ConPTY) |
| Linux | `libc.so.6` + `libutil.so.1` (forkpty) |
| macOS | `libSystem.dylib` (forkpty) |

## Requirements

- .NET 10.0 or later
- Windows 10 version 1809 or later (for Windows support)

## Usage

### EzTerminal - Simple Wrapper

`EzTerminal` provides a clean abstraction over the PTY library with hooks for input/output handling.

#### Basic Example

```csharp
using var cts = new CancellationTokenSource();

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

using var terminal = new EzTerminal(
    userOutputHandler: input =>
    {
        // Called when user types something (before it's sent to terminal)
        // Use this to log keystrokes, filter input, etc.
    },
    terminalOutputHandler: output =>
    {
        // Called when terminal produces output
        // Display it, parse it, log it - whatever you need
        Console.Write(output);
    },
    cancellationToken: cts.Token,
    onExit: () =>
    {
        Console.WriteLine("\n[Session ended]");
    }
);

// Spawns a shell (pwsh on Windows, bash on Linux/macOS)
// then immediately runs: dotnet --version
await terminal.Run("dotnet", cts.Token, "--version");
```

#### Constructor Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `userOutputHandler` | `Action<string>` | Receives user input before it's sent to the terminal |
| `terminalOutputHandler` | `Action<string>` | Receives all output from the terminal |
| `cancellationToken` | `CancellationToken` | Token to cancel the terminal session |
| `onExit` | `Action?` | Optional callback when the session ends |

#### Fluent Configuration

```csharp
// Set environment variables
await terminal
    .WithEnvironment("NODE_ENV", "development")
    .WithEnvironment("DEBUG", "true")
    .Run("npm", cts.Token, "start");

// Or pass a dictionary
var env = new Dictionary<string, string>
{
    ["API_KEY"] = "secret",
    ["LOG_LEVEL"] = "verbose"
};

await terminal
    .WithEnvironment(env)
    .WithWorkingDirectory(@"C:\Projects\MyApp")
    .Run("dotnet", cts.Token, "build");
```

#### API Reference

| Method | Returns | Description |
|--------|---------|-------------|
| `WithEnvironment(string key, string value)` | `IEzTerminal` | Add a single environment variable |
| `WithEnvironment(IReadOnlyDictionary<string, string> env)` | `IEzTerminal` | Add multiple environment variables |
| `WithWorkingDirectory(string path)` | `IEzTerminal` | Set the working directory for the shell |
| `Run(string command, CancellationToken ct, params string[] args)` | `Task` | Spawn shell and execute command |

### Direct Pty.Net Usage

For more control, use the underlying `Pty.Net` library directly:

```csharp
using Pty.Net;

var options = new PtyOptions
{
    Name = "MyTerminal",
    Cols = 120,
    Rows = 30,
    Cwd = Environment.CurrentDirectory,
    App = "pwsh.exe",  // or "bash" on Linux/macOS
    CommandLine = Array.Empty<string>(),
    Environment = new Dictionary<string, string>()
};

using var pty = await PtyProvider.SpawnAsync(options, CancellationToken.None);

// Write to terminal
var bytes = System.Text.Encoding.UTF8.GetBytes("echo hello\r");
await pty.WriterStream.WriteAsync(bytes);

// Read from terminal
var buffer = new byte[4096];
var bytesRead = await pty.ReaderStream.ReadAsync(buffer);
var output = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
```

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
