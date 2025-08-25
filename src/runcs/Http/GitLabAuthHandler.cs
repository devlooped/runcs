using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GitCredentialManager;
using GitLab;

namespace Devlooped.Http;

class GitLabAuthHandler(HttpMessageHandler inner) : AuthHandler(inner)
{
    ICredential? credential;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
            return await base.SendAsync(request, cancellationToken);

        var creds = await GetCredentialAsync(request.RequestUri);

        // Reissue the request
        var retry = new HttpRequestMessage(HttpMethod.Get, request.RequestUri);
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", creds.Password);

        foreach (var etag in request.Headers.IfNoneMatch)
        {
            retry.Headers.IfNoneMatch.Add(etag);
        }

        return await base.SendAsync(retry, cancellationToken);
    }

    async Task<ICredential> GetCredentialAsync(Uri uri)
    {
        if (credential != null)
            return credential;

        var input = new InputArguments(new Dictionary<string, string>
        {
            ["protocol"] = "https",
            ["host"] = "gitlab.com",
            ["path"] = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped),
        });

        var provider = new GitLabHostProvider(new CommandContext());

        credential = await provider.GetCredentialAsync(input);
        return credential;
    }
}
