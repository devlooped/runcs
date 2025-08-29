using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Devlooped;
using DotNetConfig;
using GitCredentialManager.UI;
using Spectre.Console;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

if (args.Length == 0 || !RemoteRef.TryParse(args[0], out var location))
{
    AnsiConsole.MarkupLine($"Usage: [grey][[dnx]][/] [lime]{ThisAssembly.Project.ToolCommandName}[/] [italic]GIST_REF[/] [grey][[args]][/]");
    AnsiConsole.MarkupLine("""
            [bold]GIST_REF[/]  Reference to gist file to run, with format [yellow]owner/gist[[@commit]][[:path]][/]
                      [italic][yellow]@commit[/][/] optional gist commit (default: default branch)
                      [italic][yellow]:path[/][/] optional path to file in gist (default: program.cs or first .cs file)

                      Examples: 
                      * kzu/0ac826dc7de666546aaedd38e5965381                 (tip commit and program.cs or first .cs file)
                      * kzu/0ac826dc7de666546aaedd38e5965381@d8079cf:run.cs  (explicit commit and file path)

            [bold]args[/]      Arguments to pass to the C# gist program
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
    var config = Config.Build(Config.GlobalLocation);
    var etag = config.GetString(ThisAssembly.Project.ToolCommandName, $"{location.Owner}/{location.Repo}", location.Ref ?? "main");
    if (etag != null && Directory.Exists(location.TempPath))
    {
        if (etag.StartsWith("W/\"", StringComparison.OrdinalIgnoreCase) && !etag.EndsWith('"'))
            etag += '"';

        location = location with { ETag = etag };
    }

    if (DotnetMuxer.Path is null)
    {
        AnsiConsole.MarkupLine($":cross_mark:  Unable to locate the .NET SDK.");
        Dispatcher.MainThread.Shutdown();
        return 1;
    }

    var provider = new GitHubDownloadProvider(gist: true);
    var contents = await provider.GetAsync(location);
    var updated = false;

    if (!contents.IsSuccessStatusCode)
    {
        AnsiConsole.MarkupLine($":cross_mark: Reference [yellow]{location}[/] not found.");
        Dispatcher.MainThread.Shutdown();
        return 1;
    }

    if (contents.StatusCode != HttpStatusCode.NotModified)
    {
#if DEBUG
        await AnsiConsole.Status().StartAsync($":open_file_folder: {location} :backhand_index_pointing_right: {location.TempPath}", async ctx =>
        {
            await contents.ExtractToAsync(location);
        });
#else
        await contents.ExtractToAsync(location);
#endif

        if (contents.Headers.ETag?.ToString() is { } newEtag)
            config.SetString(ThisAssembly.Project.ToolCommandName, $"{location.Owner}/{location.Repo}", location.Ref ?? "main", newEtag);

        updated = true;
    }

    var program = Path.Combine(location.TempPath, location.Path ?? "program.cs");
    if (!File.Exists(program))
    {
        if (location.Path is not null)
        {
            AnsiConsole.MarkupLine($":cross_mark:  File reference not found in gist {location}.");
            Dispatcher.MainThread.Shutdown();
            return 1;
        }

        var first = Directory.EnumerateFiles(location.TempPath, "*.cs", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (first is null)
        {
            AnsiConsole.MarkupLine($":cross_mark:  No .cs files found in gist {location}.");
            Dispatcher.MainThread.Shutdown();
            return 1;
        }
        program = first;
    }

    if (updated)
    {
        // Clean since it otherwise we get stale build outputs :/
        Process.Start(DotnetMuxer.Path.FullName, ["clean", "-v:q", program]).WaitForExit();
    }

#if DEBUG
    AnsiConsole.MarkupLine($":rocket: {DotnetMuxer.Path.FullName} run -v:q {program} {string.Join(' ', args)}");
#endif

    var start = new ProcessStartInfo(DotnetMuxer.Path.FullName, ["run", "-v:q", program, .. args]);
    var process = Process.Start(start);
    process?.WaitForExit();

    Dispatcher.MainThread.Shutdown();

    return process?.ExitCode ?? 1;
}