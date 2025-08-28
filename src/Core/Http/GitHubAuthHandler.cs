extern alias Devlooped;
using System.Net.Http.Headers;
using Core;
using GitCredentialManager;
using GitHub;

namespace Devlooped.Http;

public class GitHubAuthHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
{
    ICredential? credential;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
            return await base.SendAsync(request, cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
            response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            var creds = await GetCredentialAsync(request.RequestUri);
            if (creds == null)
                return response;

            var retry = response.CreateRetry(request.RequestUri);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", creds.Password);

            return await base.SendAsync(retry, cancellationToken);
        }

        return response;
    }

    async Task<ICredential?> GetCredentialAsync(Uri? uri)
    {
        if (credential != null)
            return credential;

        // We can use the namespace-less version since we don't need any specific permissions besides 
        // the built-in GCM GH auth.
        var store = Devlooped::GitCredentialManager.CredentialManager.Create();

        if (uri != null &&
            uri.PathAndQuery.Split('/', StringSplitOptions.RemoveEmptyEntries) is { Length: >= 2 } parts)
        {
            // Try using GCM via API first to retrieve creds
            if (store.GetCredential($"https://github.com/{parts[0]}/{parts[1]}") is { } repo)
                return repo;
            else if (store.GetCredential($"https://github.com/{parts[0]}") is { } owner)
                return owner;
            else if (store.GetCredential("https://github.com") is { } global)
                return global;
        }

        var input = new InputArguments(new Dictionary<string, string>
        {
            ["protocol"] = "https",
            ["host"] = "github.com",
        });

        var provider = new GitHubHostProvider(CommandContext.Create(store));

        try
        {
            credential = await provider.GetCredentialAsync(input);
            store.AddOrUpdate("https://github.com", credential.Account, credential.Password);
            return credential;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
