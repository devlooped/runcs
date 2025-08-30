using System.Diagnostics;
using System.Net;
using DotNetConfig;
using Spectre.Console;

namespace Devlooped;

public class RemoteRunner(RemoteRef location, string toolName)
{
    public async Task<int> RunAsync(string[] args)
    {
        var config = Config.Build(Config.GlobalLocation);
        var etag = config.GetString(toolName, location.ToString(), "etag");
        if (etag != null && Directory.Exists(location.TempPath))
        {
            if (etag.StartsWith("W/\"", StringComparison.OrdinalIgnoreCase) && !etag.EndsWith('"'))
                etag += '"';

            location = location with { ETag = etag };
        }
        if (config.TryGetString(toolName, location.ToString(), "uri", out var url) &&
            Uri.TryCreate(url, UriKind.Absolute, out var uri))
            location = location with { ResolvedUri = uri };

        if (DotnetMuxer.Path is null)
        {
            AnsiConsole.MarkupLine($":cross_mark:  Unable to locate the .NET SDK.");
            return 1;
        }

        var provider = DownloadProvider.Create(location);
        var contents = await provider.GetAsync(location);
        var updated = false;
        // We consider a not modified as successful too
        var success = contents.IsSuccessStatusCode || contents.StatusCode == HttpStatusCode.NotModified;

        if (!success)
        {
            AnsiConsole.MarkupLine($":cross_mark: Reference [yellow]{location}[/] not found.");
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
                config = config.SetString(toolName, location.ToString(), "etag", newEtag);

            if (contents.Headers.TryGetValues("X-Original-URI", out var urls) && urls.Any())
                config = config.SetString(toolName, location.ToString(), "uri", urls.First());
            else
                config = config.SetString(toolName, location.ToString(), "uri", contents.RequestMessage!.RequestUri!.AbsoluteUri);

            updated = true;
        }

        var program = Path.Combine(location.TempPath, location.Path ?? "program.cs");
        if (!File.Exists(program))
        {
            if (location.Path is not null)
            {
                AnsiConsole.MarkupLine($":cross_mark:  File reference not found in {location}.");
                return 1;
            }

            var first = Directory.EnumerateFiles(location.TempPath, "*.cs", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (first is null)
            {
                AnsiConsole.MarkupLine($":cross_mark:  No .cs files found in {location}.");
                return 1;
            }
            program = first;
        }

        if (updated)
        {
            // Clean since otherwise we sometimes get stale build outputs? :/
            Process.Start(DotnetMuxer.Path.FullName, ["clean", "-v:q", program]).WaitForExit();
        }

#if DEBUG
        AnsiConsole.MarkupLine($":rocket: {DotnetMuxer.Path.FullName} run -v:q {program} {string.Join(' ', args)}");
#endif

        var start = new ProcessStartInfo(DotnetMuxer.Path.FullName, ["run", "-v:q", program, .. args]);
        var process = Process.Start(start);
        process?.WaitForExit();

        return process?.ExitCode ?? 1;
    }
}
