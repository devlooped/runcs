extern alias Devlooped;
using GitCredentialManager;

namespace Devlooped;

public static class CommandContextExtensions
{
    extension(CommandContext)
    {
        public static ICommandContext Create(Devlooped::GitCredentialManager.ICredentialStore? store = default, string? @namespace = default)
            => new CommandContextAdapter(new CommandContext(), @namespace)
            {
                // We need adapting since Devlooped.CredentialManager has its own version of ICredentialStore, 
                // but since we use the GCM version, we need to adapt it here so we can preserve the functionality.
                CredentialStore = new CredentialStoreAdapter(store ?? Devlooped::GitCredentialManager.CredentialManager.Create(@namespace))
            };
    }

    class CredentialStoreAdapter(Devlooped::GitCredentialManager.ICredentialStore store) : ICredentialStore
    {
        public void AddOrUpdate(string service, string account, string secret) => store.AddOrUpdate(service, account, secret);
        public ICredential Get(string service, string account) => store.Get(service, account) is { } creds ? new CredentialAdapter(creds) : default!;
        public IList<string> GetAccounts(string service) => store.GetAccounts(service);
        public bool Remove(string service, string account) => store.Remove(service, account);
    }

    class CredentialAdapter(Devlooped::GitCredentialManager.ICredential credential) : ICredential
    {
        public string Account => credential.Account;
        public string Password => credential.Password;
    }
}