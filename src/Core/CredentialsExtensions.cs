extern alias Devlooped;
using GitCredentialManager;

namespace Core;

public static class CredentialsExtensions
{
    public static ICredential? GetCredential(this Devlooped::GitCredentialManager.ICredentialStore store, string url)
    {
        var accounts = store.GetAccounts(url);
        if (accounts.Count == 1)
        {
            var creds = store.Get(url, accounts[0]);
            if (creds != null)
                return new Credential(accounts[0], creds.Password);
        }
        return default;
    }

    record Credential(string Account, string Password) : ICredential;
}
