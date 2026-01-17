namespace Pty.Net;

public interface IEzTerminal : IDisposable
{
    IEzTerminal WithEnvironment(string key, string value);
    IEzTerminal WithEnvironment(IReadOnlyDictionary<string, string> env);
    IEzTerminal WithWorkingDirectory(string path);
    Task Run(string command, CancellationToken cancellationToken, params string[] args);
}

public class EzTerminal : IEzTerminal
{
    private readonly Action<string> _userOutputHandler;
    private readonly Action<string> _terminalOutputHandler;
    private readonly CancellationToken _cancellationToken;
    private readonly Action? _onExit;
    private readonly Dictionary<string, string> _environment = new();
    private string _workingDirectory = Environment.CurrentDirectory;
    private IPtyConnection? _pty;
    private Task? _readTask;
    private Task? _writeTask;

    public EzTerminal(
        Action<string> userOutputHandler,
        Action<string> terminalOutputHandler,
        CancellationToken cancellationToken,
        Action? onExit = null)
    {
        _userOutputHandler = userOutputHandler;
        _terminalOutputHandler = terminalOutputHandler;
        _cancellationToken = cancellationToken;
        _onExit = onExit;
    }

    public IEzTerminal WithEnvironment(string key, string value)
    {
        _environment[key] = value;
        return this;
    }

    public IEzTerminal WithEnvironment(IReadOnlyDictionary<string, string> env)
    {
        foreach (var kvp in env)
            _environment[kvp.Key] = kvp.Value;
        return this;
    }

    public IEzTerminal WithWorkingDirectory(string path)
    {
        _workingDirectory = path;
        return this;
    }

    public async Task Run(string command, CancellationToken cancellationToken, params string[] args)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken);
        var token = linkedCts.Token;

        var shell = OperatingSystem.IsWindows() ? "pwsh.exe" : "bash";

        var options = new PtyOptions
        {
            Name = "EzTerminal",
            Cols = 120,
            Rows = 30,
            Cwd = _workingDirectory,
            App = shell,
            CommandLine = [],
            Environment = _environment
        };

        _pty = await PtyProvider.SpawnAsync(options, token);

        // Read output from terminal
        _readTask = Task.Run(async () =>
        {
            var buffer = new byte[4096];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var bytesRead = await _pty.ReaderStream.ReadAsync(buffer, token);
                    if (bytesRead == 0) break;
                    var text = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _terminalOutputHandler(text);
                }
            }
            catch (OperationCanceledException) { }
            catch { }
            finally
            {
                _onExit?.Invoke();
            }
        }, token);

        // Read input from user
        _writeTask = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var key = Console.ReadKey(intercept: true);
                    var input = key.KeyChar.ToString();
                    _userOutputHandler(input);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                    await _pty.WriterStream.WriteAsync(bytes, token);
                    await _pty.WriterStream.FlushAsync(token);
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }, token);

        // Send the command with args
        var fullCommand = args.Length > 0
            ? $"{command} {string.Join(" ", args)}\r"
            : $"{command}\r";

        var cmdBytes = System.Text.Encoding.UTF8.GetBytes(fullCommand);
        await _pty.WriterStream.WriteAsync(cmdBytes, token);
        await _pty.WriterStream.FlushAsync(token);

        // Wait until cancelled or process exits
        await Task.WhenAny(_readTask, _writeTask);
    }

    public void Dispose()
    {
        _pty?.Dispose();
    }
}
