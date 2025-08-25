using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Devlooped;

public partial class FileRef(string ownerRepo, string? branchOrTag, string? filePath, string? host)
{
    public static bool TryParse(string value, [NotNullWhen(true)] out FileRef? fileRef)
    {
        if (string.IsNullOrEmpty(value) || ParseExp().Match(value) is not { Success: true } match)
        {
            fileRef = null;
            return false;
        }

        var host = match.Groups["host"].Value;
        var ownerRepo = match.Groups["repo"].Value;
        var branchOrTag = match.Groups["ref"].Value;
        var filePath = match.Groups["path"].Value;

        fileRef = new FileRef(ownerRepo,
            string.IsNullOrEmpty(branchOrTag) ? null : branchOrTag,
            string.IsNullOrEmpty(filePath) ? null : filePath,
            string.IsNullOrEmpty(host) ? null : host);

        return true;
    }

    public string? Host => host;
    public string OwnerRepo => ownerRepo;
    public string? BranchOrTag => branchOrTag;
    public string? FilePath => filePath;

    [GeneratedRegex(@"^^(?:(?<host>[A-Za-z0-9.-]+\.[A-Za-z]{2,})/)?(?<repo>(?:[A-Za-z0-9](?:-?[A-Za-z0-9]){0,38})/[A-Za-z0-9._-]{1,100})(?:@(?<ref>[^:\s]+))?(?::(?<path>.+))?$")]
    private static partial Regex ParseExp();
}
