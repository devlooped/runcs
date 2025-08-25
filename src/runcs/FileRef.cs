using System;
using System.Text.RegularExpressions;

namespace Devlooped;

public partial class FileRef(string ownerRepo, string? branchOrTag, string? filePath)
{
    public static FileRef Parse(string value)
    {
        var match = ParseExp().Match(value);
        if (!match.Success)
            throw new ArgumentException($"Invalid file reference '{value}'. Expected format: 'owner/repo[@ref][:path]'");

        var ownerRepo = match.Groups["repo"].Value;
        var branchOrTag = match.Groups["ref"].Value;
        var filePath = match.Groups["path"].Value;

        return new FileRef(ownerRepo, string.IsNullOrEmpty(branchOrTag) ? null : branchOrTag, string.IsNullOrEmpty(filePath) ? null : filePath);
    }

    public string OwnerRepo => ownerRepo;
    public string? BranchOrTag => branchOrTag;
    public string? FilePath => filePath;

    [GeneratedRegex(@"^(?<repo>(?:[A-Za-z0-9](?:-?[A-Za-z0-9]){0,38})/[A-Za-z0-9._-]{1,100})(?:@(?<ref>[^:\s]+))?(?::(?<path>.+))?$")]
    private static partial Regex ParseExp();
}
