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
    public static DownloadProvider Create(RemoteRef location) => location.Host?.ToLowerInvariant() switch
    {
        "gitlab.com" => new GitLabDownloadProvider(),
        //"bitbucket.org" => new BitbucketDownloadProvider(),
        _ => new GitHubDownloadProvider(),
    };

    public abstract Task<HttpResponseMessage> GetAsync(RemoteRef location);
}

public class GitHubDownloadProvider : DownloadProvider
{
    static readonly HttpClient http = new(new GitHubAuthHandler(
        new RedirectingHttpHandler(
            new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip
            })))
    {
        Timeout = Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(15)
    };

    public override async Task<HttpResponseMessage> GetAsync(RemoteRef location)
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

    static Uri GetBranchUri(RemoteRef location)
    {
        var url = $"https://github.com/{location.Owner}/{location.Repo}/archive";

        if (!string.IsNullOrEmpty(location.Ref))
            url += "/refs/heads/" + location.Ref;
        else
            url += "/refs/heads/main"; // TODO: get default branch for repo

        url += ".zip";

        return new Uri(url);
    }

    static Uri GetTagUri(RemoteRef location)
        => new($"https://github.com/{location.Owner}/{location.Repo}/archive/refs/tags/{location.Ref}.zip");

    static Uri GetShaUri(RemoteRef location)
        => new($"https://github.com/{location.Owner}/{location.Repo}/archive/{location.Ref}.zip");
}

public class GitLabDownloadProvider : DownloadProvider
{
    static readonly HttpClient http = new(new GitLabAuthHandler(
        new RedirectingHttpHandler(
            new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip,
            })))
    {
        Timeout = Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(15)
    };

    public override async Task<HttpResponseMessage> GetAsync(RemoteRef location)
    {
        if (location.ResolvedUri != null)
        {
            return await http.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, location.ResolvedUri).WithTag(location.ETag),
                HttpCompletionOption.ResponseHeadersRead);
        }

        var url = $"https://gitlab.com/api/v4/projects/{Uri.EscapeDataString(location.Owner + "/" + location.Repo)}/repository/archive.zip?sha={location.Ref ?? "main"}";
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)).WithTag(location.ETag);
        var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        return response;
    }
}

public class BitbucketDownloadProvider : DownloadProvider
{
    public override Task<HttpResponseMessage> GetAsync(RemoteRef address)
        => throw new NotImplementedException();
}