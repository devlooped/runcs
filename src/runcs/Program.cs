using System.Runtime.InteropServices;
using System.Text;
using Devlooped;
using GitCredentialManager.UI;
using Spectre.Console;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

if (args.Length == 0 || !RemoteRef.TryParse(args[0], out var location))
{
    AnsiConsole.MarkupLine($"Usage: [grey][[dnx]][/] [lime]{ThisAssembly.Project.ToolCommandName}[/] [italic]REPO_REF[/] [grey][[args]][/]");
    AnsiConsole.MarkupLine("""
            [bold]REPO_REF[/]  Reference to remote file to run, with format [yellow][[host/]]owner/repo[[@ref]][[:path]][/]
                      [italic][yellow]host[/][/] optional host name (default: github.com)
                      [italic][yellow]@ref[/][/] optional branch, tag, or commit (default: default branch)
                      [italic][yellow]:path[/][/] optional path to file in repo (default: program.cs at repo root)

                      Examples: 
                      * kzu/sandbox@v1.0.0:run.cs           (implied host github.com, explicit tag and file path)
                      * gitlab.com/kzu/sandbox@main:run.cs  (all explicit parts)
                      * bitbucket.org/kzu/sandbox           (implied ref as default branch and path as program.cs)
                      * kzu/sandbox                         (implied host github.com, ref and path defaults)

            [bold]args[/]      Arguments to pass to the C# program
        """);
    return;
}

// Create the dispatcher on the main thread. This is required
// for some platform UI services such as macOS that mandates
// all controls are created/accessed on the initial thread
// created by the process (the process entry thread).
Dispatcher.Initialize();

// Run AppMain in a new thread and keep the main thread free
// to process the dispatcher's job queue.
var main = Task.Run(() => Main(location, args[1..]));

// Process the dispatcher job queue (aka: message pump, run-loop, etc...)
// We must ensure to run this on the same thread that it was created on
// (the main thread) so we cannot use any async/await calls between
// Dispatcher.Initialize and Run.
Dispatcher.MainThread.Run();

// Dispatcher was shutdown
Environment.Exit(await main);

static async Task<int> Main(RemoteRef location, string[] args)
{
    var provider = DownloadProvider.Create(location);
    var contents = await provider.GetAsync(location);

    AnsiConsole.MarkupLine($":check_box_with_check:  {location} :backhand_index_pointing_right: {contents.StatusCode}");

    Dispatcher.MainThread.Shutdown();
    return 0;
}