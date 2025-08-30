using System.Runtime.InteropServices;
using System.Text;
using Devlooped;
using GitCredentialManager.UI;
using Spectre.Console;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

if (args.Length == 0 || !RemoteRef.TryParse("gist.github.com/" + args[0], out var location))
{
    AnsiConsole.MarkupLine(
        $"""
        Usage:
            [grey][[dnx]][/] [lime]{ThisAssembly.Project.ToolCommandName}[/] [bold]<gistRef>[/] [grey italic][[<appArgs>...]][/]

        Arguments:
            [bold]<GIST_REF>[/]  Reference to gist file to run, with format [yellow]owner/gist[[@commit]][[:path]][/]
                        [italic][yellow]@commit[/][/] optional gist commit (default: latest)
                        [italic][yellow]:path[/][/] optional path to file in gist (default: program.cs or first .cs file)
                       
                        Examples: 
                        * kzu/0ac826dc7de666546aaedd38e5965381                 (tip commit and program.cs or first .cs file)
                        * kzu/0ac826dc7de666546aaedd38e5965381@d8079cf:run.cs  (explicit commit and file path)
                          
            [bold]<appArgs>[/]   Arguments passed to the C# program that is being run. 
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
var main = Task
    .Run(() => new RemoteRunner(location, ThisAssembly.Project.ToolCommandName)
    .RunAsync(args[1..]))
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