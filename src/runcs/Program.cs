using System.CommandLine;
using System.Runtime.InteropServices;
using System.Text;
using Devlooped;
using DotNetConfig;
using GitCredentialManager.UI;
using Spectre.Console;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

// Default PublishAot=false since it severely limits the code that can run (i.e. no reflection, STJ even)
var aot = false;
if (args.Any(x => x == "--aot"))
{
    aot = true;
    args = [.. args.Where(x => x != "--aot")];
}

var config = Config.Build(Config.GlobalLocation);
if (args.Length > 0 && config.GetString("runcs", args[0]) is string aliased)
    args = [aliased, .. args[1..]];

// Set alias and remove from args if present
var option = new Option<string?>("--alias");
var parsed = new RootCommand() { Options = { option } }.Parse(args);
var alias = parsed.GetValue(option);
if (alias != null)
    args = [.. parsed.UnmatchedTokens];

if (args.Length == 0 || !RemoteRef.TryParse(args[0], out var location))
{
    AnsiConsole.MarkupLine(
        $"""
        Usage:
            [grey][[dnx]][/] [lime]{ThisAssembly.Project.ToolCommandName}[/] [grey][[--aot]][/] [grey][[--alias ALIAS]][/] [bold]<repoRef>[/] [grey italic][[<appArgs>...]][/]

        Arguments:
            [bold]<REPO_REF>[/]  Reference to remote file to run, with format [yellow][[host/]]owner/repo[[@ref]][[:path]][/]
                        [italic][yellow]host[/][/] optional host name ([grey][[gist.]][/]github.com|gitlab.com|dev.azure.com, default: github.com)
                        [italic][yellow]@ref[/][/] optional branch, tag, or commit (default: default branch)
                        [italic][yellow]:path[/][/] optional path to file in repo (default: program.cs at repo root)
                  
                        Examples: 
                        * kzu/sandbox@v1.0.0:run.cs           (implied host github.com, explicit tag and file path)
                        * gitlab.com/kzu/sandbox@main:run.cs  (all explicit parts)
                        * kzu/sandbox                         (implied host github.com, ref and path defaults)
                  
                        Can be an alias previously set with --alias.

            [bold]<appArgs>[/]   Arguments passed to the C# program that is being run. 

        Options:
            [bold]--aot[/]         (optional) Enable dotnet AOT defaults for run file.cs. Defaults to false.
            [bold]--alias[/] ALIAS (optional) Assign an alias on first usage which can be used instead of the full ref.
        """);
    return;
}

if (alias != null)
    config = config.SetString("runcs", alias, location.ToString());

// Create the dispatcher on the main thread. This is required
// for some platform UI services such as macOS that mandates
// all controls are created/accessed on the initial thread
// created by the process (the process entry thread).
Dispatcher.Initialize();

// Run AppMain in a new thread and keep the main thread free
// to process the dispatcher's job queue.
var main = Task
    .Run(() => new RemoteRunner(location, ThisAssembly.Project.ToolCommandName, config)
    .RunAsync(args[1..], aot))
    .ContinueWith(t =>
    {
        Dispatcher.MainThread.Shutdown();
        return t.Result;
    });

// Process the dispatcher job queue (aka: message pump, run-loop, etc...)
// We must ensure to run this on the same thread that it was created on
// (the main thread) so we cannot use any async/await calls between
// Dispatcher.Initialize and Run.
Dispatcher.MainThread.Run();

// Dispatcher was shutdown
Environment.Exit(await main);
