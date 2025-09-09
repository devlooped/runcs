using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Devlooped;

public partial record RemoteRef(string Owner, string Repo, string? Ref, string? Path, string? Host)
{
    // When an optional third segment ("project") is provided (owner/repo/project),
    // we treat the originally parsed <repo> as the Azure DevOps project and the
    // third segment as the actual repository name.
    public string? Project { get; init; }

    public override string ToString() =>
        $"{(Host == null ? "" : Host + "/")}{Owner}/{(Project == null ? "" : Project + "/")}{Repo}{(Ref == null ? "" : "@" + Ref)}{(Path == null ? "" : ":" + Path)}";

    public static bool TryParse(string value, [NotNullWhen(true)] out RemoteRef? remote)
    {
        Match GetMatch(string input)
        {
            // Try Azure DevOps first since it is more specific
            var match = ParseAzureDevOpsExp().Match(input);

            if (match.Success)
            {
                return match;
            }

            return ParseExp().Match(input);
        }

        if (string.IsNullOrEmpty(value)
            || GetMatch(value) is not { Success: true } match)
        {
            remote = null;
            return false;
        }

        var host = match.Groups["host"].Value;
        var owner = match.Groups["owner"].Value;
        var repo = match.Groups["repo"].Value;
        var project = match.Groups["project"].Value;
        var reference = match.Groups["ref"].Value;
        var filePath = match.Groups["path"].Value;

        if (string.IsNullOrEmpty(project))
            project = null;

        // If project is provided, host is required, since GH does not support projects
        if (project != null && string.IsNullOrEmpty(host))
        {
            remote = null;
            return false;
        }

        remote = new RemoteRef(owner, repo,
            string.IsNullOrEmpty(reference) ? null : reference,
            string.IsNullOrEmpty(filePath) ? null : filePath,
            string.IsNullOrEmpty(host) ? null : host)
        {
            Project = project
        };

        return true;
    }

    public string? ETag { get; init; }
    public Uri? ResolvedUri { get; init; }

    [GeneratedRegex(@"^(?:(?<host>[A-Za-z0-9.-]+\.[A-Za-z]{2,})/)?(?<owner>[A-Za-z0-9](?:-?[A-Za-z0-9]){0,38})/(?<repo>[A-Za-z0-9._-]{1,100})(?:/(?<project>[A-Za-z0-9._-]{1,100}))?(?:@(?<ref>[^:\s]+))?(?::(?<path>.+))?$")]
    private static partial Regex ParseExp();

    [GeneratedRegex(@"^(?:(?<host>dev.azure.com)/)(?<owner>[A-Za-z0-9](?:-?[A-Za-z0-9]){0,38})/(?<project>[%A-Za-z0-9._-]{1,100})(?:/(?<repo>[%A-Za-z0-9._-]{1,100}))?(?:@(?<ref>[^:\s]+))?(?::(?<path>.+))?$")]
    private static partial Regex ParseAzureDevOpsExp();
}
