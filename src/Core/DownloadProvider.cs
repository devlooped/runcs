extern alias Devlooped;
using System.Diagnostics;
using System.Net;
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

public class GitHubDownloadProvider(bool gist = false) : DownloadProvider
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

        var subdomain = gist ? "gist." : "";
        var request = new HttpRequestMessage(HttpMethod.Get,
            // Direct archive link works for branch, tag, sha
            new Uri($"https://{subdomain}github.com/{location.Owner}/{location.Repo}/archive/{(location.Ref ?? "main")}.zip"))
            .WithTag(location.ETag);

        return await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }
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

/*
public class BitbucketDownloadProvider : DownloadProvider
{
    ICredential? credential;

    public override async Task<HttpResponseMessage> GetAsync(RemoteRef location)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        await Task.CompletedTask;

        var repoUrl = $"https://bitbucket.org/{location.Owner}/{location.Repo}.git";
        var creds = await GetCredentialAsync(new Uri(repoUrl));
        if (creds == null)
            return response;

        var options = new CloneOptions
        {
            Checkout = true,
            BranchName = location.Ref ?? "main",
        };

        //options.FetchOptions.Depth = 1;
        options.FetchOptions.CredentialsProvider = (_url, _user, _types) =>
            new UsernamePasswordCredentials { Username = creds.Account, Password = creds.Password };

        var repoDir = DownloadManager.GetTempSubpath("bitbucket", location.Owner, location.Repo, location.Ref ?? "main");
        if (Directory.Exists(Path.Combine(repoDir, ".git")))
        {
            // There's an existing clone, delete everything.
            Directory.Delete(repoDir, true);
        }

        var workdir = Repository.Clone(repoUrl, repoDir, options);

        // 2) Export files (no .git)
        ExportHead(workdir, "export");
        Directory.Delete(Path.Combine(workdir, ".git"), true); // optional cleanup

        return response;
    }

    static void ExportHead(string repoPath, string exportDir)
    {
        using var repo = new Repository(repoPath);
        var commit = repo.Head.Tip;
        Directory.CreateDirectory(exportDir);

        void WriteTree(Tree tree, string root)
        {
            foreach (var entry in tree)
            {
                var dest = Path.Combine(root, entry.Path.Replace('/', Path.DirectorySeparatorChar));
                switch (entry.TargetType)
                {
                    case TreeEntryTargetType.Blob:
                        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                        var blob = (Blob)entry.Target;
                        using (var src = blob.GetContentStream())
                        using (var dst = File.Create(dest))
                            src.CopyTo(dst);
                        break;
                    case TreeEntryTargetType.Tree:
                        Directory.CreateDirectory(dest);
                        WriteTree((Tree)entry.Target, root);
                        break;
                }
            }
        }

        WriteTree(commit.Tree, exportDir);
    }

    async Task<ICredential?> GetCredentialAsync(Uri uri)
    {
        if (credential != null)
            return credential;

        // We can use the namespace-less version since we don't need any specific permissions besides 
        // the built-in GCM GH auth.
        var store = Devlooped::GitCredentialManager.CredentialManager.Create();
        if (uri.PathAndQuery.Split('/', StringSplitOptions.RemoveEmptyEntries) is { Length: >= 2 } parts)
        {
            // Try using GCM via API first to retrieve creds
            if (store.GetCredential($"https://bitbucket.org/{parts[0]}/{parts[1]}") is { } repo)
                return repo;
            else if (store.GetCredential($"https://bitbucket.org/{parts[0]}") is { } owner)
                return owner;
            else if (store.GetCredential("https://bitbucket.org") is { } global)
                return global;
        }

        var input = new InputArguments(new Dictionary<string, string>
        {
            ["protocol"] = "https",
            ["host"] = "bitbucket.org",
            ["path"] = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped),
        });

        var provider = new BitbucketHostProvider(CommandContext.Create(store));

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

}
*/