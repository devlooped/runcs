using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Devlooped.Http;

namespace Devlooped;

public abstract class DownloadProvider
{
    public static DownloadProvider Create(FileRef location) => location.Host?.ToLowerInvariant() switch
    {
        "gitlab.com" => new GitLabDownloadProvider(),
        "bitbucket.org" => new BitbucketDownloadProvider(),
        _ => new GitHubDownloadProvider(),
    };

    public abstract Task<HttpResponseMessage> GetAsync(FileRef location);
}

public class GitHubDownloadProvider : DownloadProvider
{
    static readonly HttpClient http = new(new GitHubAuthHandler(
        new RedirectingHttpHandler(
            new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression =
                DecompressionMethods.Brotli | DecompressionMethods.GZip
            })))
    {
        Timeout = Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(15)
    };

    public override async Task<HttpResponseMessage> GetAsync(FileRef location)
    {
        if (location.ResolvedUri != null)
        {
            return await http.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, location.ResolvedUri).WithTag(location.ETag),
                HttpCompletionOption.ResponseHeadersRead);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, GetBranchUri(location)).WithTag(location.ETag);
        var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        // Cascading attempt for branch, tag, sha
        if (response.StatusCode == HttpStatusCode.NotFound && !string.IsNullOrEmpty(location.Ref))
        {
            response = await http.SendAsync(response.Retry(GetTagUri(location)), HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response = await http.SendAsync(response.Retry(GetShaUri(location)), HttpCompletionOption.ResponseHeadersRead);
            }
        }

        return response;
    }

    static Uri GetBranchUri(FileRef location)
    {
        var url = "https://github.com/" + location.OwnerRepo.TrimStart('/').TrimEnd('/');
        url += "/archive";

        if (!string.IsNullOrEmpty(location.Ref))
            url += "/refs/heads/" + location.Ref;
        else
            url += "/refs/heads/main"; // TODO: get default branch for repo

        url += ".zip";

        return new Uri(url);
    }

    static Uri GetTagUri(FileRef location)
    {
        var url = "https://github.com/" + location.OwnerRepo.TrimStart('/').TrimEnd('/');
        url += "/archive/refs/tags/" + location.Ref + ".zip";
        return new Uri(url);
    }

    static Uri GetShaUri(FileRef location)
    {
        var url = "https://github.com/" + location.OwnerRepo.TrimStart('/').TrimEnd('/');
        url += "/archive/" + location.Ref + ".zip";
        return new Uri(url);
    }
}

public class GitLabDownloadProvider : DownloadProvider
{
    public override Task<HttpResponseMessage> GetAsync(FileRef address)
        => throw new NotImplementedException();
}

public class BitbucketDownloadProvider : DownloadProvider
{
    public override Task<HttpResponseMessage> GetAsync(FileRef address)
        => throw new NotImplementedException();
}