using System.IO.Compression;
using System.Net;

namespace Devlooped;

public static class HttpExtensions
{
    public static async Task ExtractToAsync(this HttpResponseMessage content, RemoteRef location)
    {
        // read content as zip and extract to temp path
        if (Directory.Exists(location.TempPath))
            Directory.Delete(location.TempPath, true);

        location.EnsureTempPath();

        // Extract files while skipping the top-level directory and preserving structure from that point onwards
        // This matches the behavior of github/gitlab archive downloads.
        using var archive = new ZipArchive(await content.Content.ReadAsStreamAsync(), ZipArchiveMode.Read);

        // Find the common top-level directory prefix to skip
        var topLevelDir = FindTopLevelDirectory(archive);

        foreach (var entry in archive.Entries)
        {
            // Skip directories
            if (string.IsNullOrEmpty(entry.Name))
                continue;

            // Skip if the entry doesn't start with the top-level directory
            if (!entry.FullName.StartsWith(topLevelDir))
                continue;

            // Remove the top-level directory from the path to get the relative path
            var relativePath = entry.FullName[topLevelDir.Length..].TrimStart('/');

            // Skip if this results in an empty path (shouldn't happen with files)
            if (string.IsNullOrEmpty(relativePath))
                continue;

            // Convert forward slashes to platform-specific directory separators
            var destinationPath = Path.Combine(location.TempPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directoryPath))
                Directory.CreateUserDirectory(directoryPath);

            entry.ExtractToFile(destinationPath);
        }
    }

    static string FindTopLevelDirectory(ZipArchive archive)
    {
        // Find all unique top-level directory names
        var topLevelDirs = new HashSet<string>();

        foreach (var entry in archive.Entries)
        {
            if (!string.IsNullOrEmpty(entry.FullName))
            {
                var firstSlash = entry.FullName.IndexOf('/');
                if (firstSlash > 0)
                {
                    var topDir = entry.FullName.Substring(0, firstSlash + 1);
                    topLevelDirs.Add(topDir);
                }
            }
        }

        // If there's exactly one top-level directory, use it as the prefix to skip
        // Otherwise, don't skip any directory (preserve original behavior)
        return topLevelDirs.Count == 1 ? topLevelDirs.First() : "";
    }

    public static bool IsRedirect(this HttpStatusCode status) =>
        status is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    public static HttpRequestMessage WithTag(this HttpRequestMessage request, string? etag)
    {
        if (!string.IsNullOrEmpty(etag))
            request.Headers.IfNoneMatch.ParseAdd(etag);

        return request;
    }

    public static HttpRequestMessage CreateRetry(this HttpResponseMessage response, Uri uri)
    {
        var retry = new HttpRequestMessage(HttpMethod.Get, uri);
        if (response.RequestMessage == null)
            return retry;

        if (response.RequestMessage.Headers.Authorization != null)
            retry.Headers.Authorization = response.RequestMessage.Headers.Authorization;

        foreach (var etag in response.RequestMessage.Headers.IfNoneMatch)
        {
            retry.Headers.IfNoneMatch.Add(etag);
        }

        return retry;
    }
}
