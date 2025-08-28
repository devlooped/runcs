extern alias Devlooped;
using System.Net;
using System.Net.Http.Headers;
using GitCredentialManager;
using GitCredentialManager.Authentication.OAuth;
using GitLab;

namespace Devlooped.Http;

public class GitLabAuthHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
{
    static readonly Lazy<HttpClient> http = new Lazy<HttpClient>(() => new());
    ICredential? credential;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
            return await base.SendAsync(request, cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound)
        {
            var creds = await GetCredentialAsync(request.RequestUri);
            if (creds == null)
                return response;

            var retry = new HttpRequestMessage(HttpMethod.Get, request.RequestUri);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", creds.Password);
            foreach (var etag in request.Headers.IfNoneMatch)
            {
                retry.Headers.IfNoneMatch.Add(etag);
            }

            return await base.SendAsync(retry, cancellationToken);
        }

        return response;
    }

    async Task<ICredential?> GetCredentialAsync(Uri uri)
    {
        if (credential != null)
            return credential;

        // we need to *always* use our own namespaced credential since we cannot reuse the 
        // default git auth for gitlab since it uses OAuth by default and does not include the 
        // read_api scope needed to download archives. So we need to restrict it to PAT and 
        // keep our own version instead.
        var store = Devlooped::GitCredentialManager.CredentialManager.Create(ThisAssembly.Project.ProjectName);

        if (uri.PathAndQuery.Split('/', StringSplitOptions.RemoveEmptyEntries) is { Length: >= 2 } parts)
        {
            // Try using GCM via API first to retrieve creds
            if (await GetCredentialAsync(store, $"https://gitlab.com/{parts[0]}/{parts[1]}") is { } repo)
                return repo;
            else if (await GetCredentialAsync(store, $"https://gitlab.com/{parts[0]}") is { } owner)
                return owner;
            else if (await GetCredentialAsync(store, "https://gitlab.com") is { } global)
                return global;
        }

        var input = new InputArguments(new Dictionary<string, string>
        {
            ["protocol"] = "https",
            ["host"] = "gitlab.com",
        });

        var context = CommandContext.Create(store, ThisAssembly.Project.ProjectName);
        var provider = new GitLabHostProvider(context, new ScopesGitLabAuth(new GitLabAuthentication(context)));

        try
        {
            credential = await provider.GetCredentialAsync(input);
            store.AddOrUpdate("https://gitlab.com", credential.Account, credential.Password);
            return credential;
        }
        catch (Exception)
        {
            return null;
        }
    }

    async Task<ICredential?> GetCredentialAsync(Devlooped::GitCredentialManager.ICredentialStore store, string url)
    {
        var accounts = store.GetAccounts(url);
        if (accounts.Count != 1)
            return null;

        if (store.Get(url, accounts[0]) is not { } creds)
            return null;

        if (creds.Account == "oauth2" && await IsOAuthTokenExpired(new Uri(url), creds.Password))
        {
            store.Remove(url, accounts[0]);
            return default;
        }

        return new Credential(creds.Account, creds.Password);
    }

    record Credential(string Account, string Password) : ICredential;

    static async Task<bool> IsOAuthTokenExpired(Uri baseUri, string accessToken)
    {
        var requestUri = new Uri(baseUri, "/oauth/token/info");
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        try
        {
            return (await http.Value.SendAsync(request)).StatusCode == HttpStatusCode.Unauthorized;
        }
        catch
        {
            return false;
        }
    }

    class ScopesGitLabAuth(IGitLabAuthentication authentication) : IGitLabAuthentication
    {
        public void Dispose() => authentication.Dispose();
        public Task<AuthenticationPromptResult> GetAuthenticationAsync(Uri targetUri, string userName, AuthenticationModes modes) => authentication.GetAuthenticationAsync(targetUri, userName, AuthenticationModes.Pat);
        public Task<OAuth2TokenResult> GetOAuthTokenViaBrowserAsync(Uri targetUri, IEnumerable<string> scopes) => authentication.GetOAuthTokenViaBrowserAsync(targetUri, ["api"]);
        public Task<OAuth2TokenResult> GetOAuthTokenViaRefresh(Uri targetUri, string refreshToken) => authentication.GetOAuthTokenViaRefresh(targetUri, refreshToken);
    }
}
