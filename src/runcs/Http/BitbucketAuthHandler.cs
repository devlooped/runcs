extern alias Devlooped;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Bitbucket;
using GitCredentialManager;
using GitCredentialManager.Authentication.OAuth;

namespace Devlooped.Http;

class BitbucketAuthHandler(HttpMessageHandler inner) : AuthHandler(inner)
{
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

            var builder = new UriBuilder(request.RequestUri)
            {
                UserName = Uri.EscapeDataString(creds.Account),
                Password = Uri.EscapeDataString(creds.Password)
            };
            var retry = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
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

        // We can use the namespace-less version since we don't need any specific permissions besides 
        // the built-in GCM GH auth.
        var store = Devlooped::GitCredentialManager.CredentialManager.Create(ThisAssembly.Project.ToolCommandName);
        if (store.GetCredential("https://bitbucket.org") is { } global)
            return global;

        var input = new InputArguments(new Dictionary<string, string>
        {
            ["protocol"] = "https",
            ["host"] = "bitbucket.org",
        });

        var context = CommandContext.Create(store, ThisAssembly.Project.ToolCommandName);
        var provider = new BitbucketHostProvider(context,
            new ScopesBitbucketAuth(new BitbucketAuthentication(context)),
            new BitbucketRestApiRegistry(context));

        try
        {
            credential = await provider.GetCredentialAsync(input);
            store.AddOrUpdate("https://bitbucket.org", credential.Account, credential.Password);
            return credential;
        }
        catch (Exception)
        {
            return null;
        }
    }

    class ScopesBitbucketAuth(IBitbucketAuthentication authentication) : IBitbucketAuthentication
    {
        public Task<OAuth2TokenResult> CreateOAuthCredentialsAsync(InputArguments input) => authentication.CreateOAuthCredentialsAsync(input);
        public void Dispose() => authentication.Dispose();
        public Task<CredentialsPromptResult> GetCredentialsAsync(Uri targetUri, string userName, AuthenticationModes modes) => authentication.GetCredentialsAsync(targetUri, userName, AuthenticationModes.Basic);
        public string GetRefreshTokenServiceName(InputArguments input) => authentication.GetRefreshTokenServiceName(input);
        public Task<OAuth2TokenResult> RefreshOAuthCredentialsAsync(InputArguments input, string refreshToken) => authentication.RefreshOAuthCredentialsAsync(input, refreshToken);
    }
}
