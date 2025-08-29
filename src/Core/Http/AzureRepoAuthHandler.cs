extern alias Devlooped;
using System.Text;
using GitCredentialManager;
using Microsoft.AzureRepos;

namespace Devlooped.Http;

public class AzureRepoAuthHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
{
    ICredential? credential;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
            return await base.SendAsync(request, cancellationToken);

        var creds = await GetCredentialAsync(request.RequestUri);
        if (creds == null)
            return await base.SendAsync(request, cancellationToken);

        var retry = new HttpRequestMessage(HttpMethod.Get, request.RequestUri);
        retry.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($":{creds.Password}")));
        foreach (var etag in request.Headers.IfNoneMatch)
        {
            retry.Headers.IfNoneMatch.Add(etag);
        }

        return await base.SendAsync(retry, cancellationToken);
    }

    async Task<ICredential?> GetCredentialAsync(Uri? uri)
    {
        if (credential != null || uri == null ||
            // We need at least the org
            uri.PathAndQuery.Split('/', StringSplitOptions.RemoveEmptyEntries) is not { Length: >= 1 } parts)
            return credential;

        // We can use the namespace-less version since we don't need any specific permissions besides 
        // the built-in GCM GH auth.
        var store = Devlooped::GitCredentialManager.CredentialManager.Create();

        // Try using GCM via API first to retrieve creds
        if (parts.Length >= 2 && store.GetCredential($"https://dev.azure.com/{parts[0]}/{parts[1]}") is { } project)
            return project;
        else if (store.GetCredential($"https://dev.azure.com/{parts[0]}") is { } owner)
            return owner;

        var input = new InputArguments(new Dictionary<string, string>
        {
            ["protocol"] = "https",
            ["host"] = "dev.azure.com",
            ["path"] = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped),
        });

        var provider = new AzureReposHostProvider(CommandContext.Create());

        try
        {
            credential = await provider.GetCredentialAsync(input);
            store.AddOrUpdate($"https://dev.azure.com/{parts[0]}", credential.Account, credential.Password);
        }
        catch (Exception)
        {
            credential = null;
        }

        return credential;
    }
}
