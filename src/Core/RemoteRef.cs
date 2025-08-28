using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Devlooped;

public partial record RemoteRef(string Owner, string Repo, string? Ref, string? Path, string? Host)
{
    public override string ToString() => $"{(Host == null ? "" : Host + "/")}{Owner}/{Repo}{(Ref == null ? "" : "@" + Ref)}{(Path == null ? "" : ":" + Path)}";

    public static bool TryParse(string value, [NotNullWhen(true)] out RemoteRef? remote)
    {
        if (string.IsNullOrEmpty(value) || ParseExp().Match(value) is not { Success: true } match)
        {
            remote = null;
            return false;
        }

        var host = match.Groups["host"].Value;
        var owner = match.Groups["owner"].Value;
        var repo = match.Groups["repo"].Value;
        var reference = match.Groups["ref"].Value;
        var filePath = match.Groups["path"].Value;

        remote = new RemoteRef(owner, repo,
            string.IsNullOrEmpty(reference) ? null : reference,
            string.IsNullOrEmpty(filePath) ? null : filePath,
            string.IsNullOrEmpty(host) ? null : host);

        return true;
    }

    public string? ETag { get; init; }
    public Uri? ResolvedUri { get; init; }

    [GeneratedRegex(@"^(?:(?<host>[A-Za-z0-9.-]+\.[A-Za-z]{2,})/)?(?<owner>[A-Za-z0-9](?:-?[A-Za-z0-9]){0,38})/(?<repo>[A-Za-z0-9._-]{1,100})(?:@(?<ref>[^:\s]+))?(?::(?<path>.+))?$")]
    private static partial Regex ParseExp();
}
