using Devlooped;

namespace Devlooped.Tests;

public class FileRefTests
{
    [Theory]
    [InlineData("owner/repo", "owner/repo", null, null)]
    [InlineData("user-name/repo-name", "user-name/repo-name", null, null)]
    [InlineData("user123/repo456", "user123/repo456", null, null)]
    [InlineData("a/b", "a/b", null, null)]
    [InlineData("github/copilot", "github/copilot", null, null)]
    [InlineData("microsoft/vscode", "microsoft/vscode", null, null)]
    public void Parse_OwnerRepoOnly_SetsOwnerRepoAndNullsOthers(string input, string expectedOwnerRepo, string? expectedBranch, string? expectedPath)
    {
        var result = FileRef.Parse(input);
        
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.BranchOrTag);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("owner/repo@main", "owner/repo", "main", null)]
    [InlineData("owner/repo@develop", "owner/repo", "develop", null)]
    [InlineData("owner/repo@v1.0.0", "owner/repo", "v1.0.0", null)]
    [InlineData("owner/repo@feature-branch", "owner/repo", "feature-branch", null)]
    [InlineData("owner/repo@release_1.0", "owner/repo", "release_1.0", null)]
    [InlineData("owner/repo@tag.with.dots", "owner/repo", "tag.with.dots", null)]
    [InlineData("owner/repo@release/8.0", "owner/repo", "release/8.0", null)] // Now supported!
    [InlineData("owner/repo@feature/new-api", "owner/repo", "feature/new-api", null)]
    [InlineData("owner/repo@hotfix/urgent-fix", "owner/repo", "hotfix/urgent-fix", null)]
    [InlineData("user-name/repo-name@branch_name", "user-name/repo-name", "branch_name", null)]
    [InlineData("owner/repo@123", "owner/repo", "123", null)]
    [InlineData("owner/repo@v2", "owner/repo", "v2", null)]
    public void Parse_OwnerRepoWithBranch_SetsOwnerRepoAndBranch(string input, string expectedOwnerRepo, string expectedBranch, string? expectedPath)
    {
        var result = FileRef.Parse(input);
        
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.BranchOrTag);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("owner/repo:file.txt", "owner/repo", null, "file.txt")]
    [InlineData("owner/repo:path/to/file.cs", "owner/repo", null, "path/to/file.cs")]
    [InlineData("owner/repo:src/Program.cs", "owner/repo", null, "src/Program.cs")]
    [InlineData("owner/repo:docs/readme.md", "owner/repo", null, "docs/readme.md")]
    [InlineData("owner/repo:file with spaces.txt", "owner/repo", null, "file with spaces.txt")]
    [InlineData("owner/repo:path/with spaces/file.cs", "owner/repo", null, "path/with spaces/file.cs")]
    [InlineData("owner/repo:file-with-dashes.txt", "owner/repo", null, "file-with-dashes.txt")]
    [InlineData("owner/repo:file_with_underscores.txt", "owner/repo", null, "file_with_underscores.txt")]
    [InlineData("owner/repo:path/to/deep/nested/file.json", "owner/repo", null, "path/to/deep/nested/file.json")]
    [InlineData("owner/repo: file starting with space.txt", "owner/repo", null, " file starting with space.txt")]
    public void Parse_OwnerRepoWithPath_SetsOwnerRepoAndPath(string input, string expectedOwnerRepo, string? expectedBranch, string expectedPath)
    {
        var result = FileRef.Parse(input);
        
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.BranchOrTag);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("owner/repo@main:file.txt", "owner/repo", "main", "file.txt")]
    [InlineData("owner/repo@develop:src/Program.cs", "owner/repo", "develop", "src/Program.cs")]
    [InlineData("owner/repo@v1.0.0:docs/readme.md", "owner/repo", "v1.0.0", "docs/readme.md")]
    [InlineData("owner/repo@feature-branch:path/to/file.cs", "owner/repo", "feature-branch", "path/to/file.cs")]
    [InlineData("owner/repo@release/8.0:src/Framework/file.cs", "owner/repo", "release/8.0", "src/Framework/file.cs")]
    [InlineData("owner/repo@feature/new-api:docs/api.md", "owner/repo", "feature/new-api", "docs/api.md")]
    [InlineData("user-name/repo-name@branch_name:file with spaces.txt", "user-name/repo-name", "branch_name", "file with spaces.txt")]
    [InlineData("owner/repo@v2.1.0:src/deep/nested/structure/file.cs", "owner/repo", "v2.1.0", "src/deep/nested/structure/file.cs")]
    public void Parse_OwnerRepoWithBranchAndPath_SetsAllProperties(string input, string expectedOwnerRepo, string expectedBranch, string expectedPath)
    {
        var result = FileRef.Parse(input);
        
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.BranchOrTag);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("owner")]
    [InlineData("/repo")]
    [InlineData("owner/")]
    [InlineData("owner//repo")]
    [InlineData("owner repo/test")] // Space in owner name
    [InlineData("owner/repo@")]
    [InlineData("owner/repo:")]  // Empty path not allowed
    [InlineData("owner/repo@:")]
    [InlineData("@branch")]
    [InlineData(":path")]
    [InlineData("@branch:path")]
    [InlineData("invalid")]
    [InlineData("owner/repo@branch with spaces")] // Space in branch name
    [InlineData("owner with spaces/repo")] // Space in owner name
    [InlineData("owner/repo-with-very-long-name-that-exceeds-the-hundred-character-limit-for-repository-names-in-github-which-should-fail")] // Repo name too long
    [InlineData("owner/repo@feature/awesome:")] // Empty path after colon not allowed
    public void Parse_InvalidFormats_ThrowsArgumentException(string input)
    {
        var exception = Assert.Throws<ArgumentException>(() => FileRef.Parse(input));
        Assert.Contains("Invalid file reference", exception.Message);
        Assert.Contains("Expected format: 'owner/repo[@ref][:path]'", exception.Message);
        Assert.Contains(input, exception.Message);
    }

    [Fact]
    public void Parse_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FileRef.Parse(null!));
    }

    [Theory]
    [InlineData("github/copilot", "github", "copilot")]
    [InlineData("microsoft/vscode", "microsoft", "vscode")]
    [InlineData("dotnet/core", "dotnet", "core")]
    [InlineData("octocat/Hello-World", "octocat", "Hello-World")]
    public void Parse_RealWorldExamples_WorksCorrectly(string input, string expectedOwner, string expectedRepo)
    {
        var result = FileRef.Parse(input);
        
        Assert.Equal($"{expectedOwner}/{expectedRepo}", result.OwnerRepo);
        Assert.Null(result.BranchOrTag);
        Assert.Null(result.FilePath);
    }

    [Theory]
    [InlineData("microsoft/vscode@main:src/vs/workbench/workbench.main.ts")]
    [InlineData("dotnet/aspnetcore@release/8.0:src/Framework/AspNetCoreAnalyzers/src/Analyzers/Infrastructure/VirtualChars/VirtualCharSequence.cs")]
    [InlineData("octocat/Hello-World@master:README")]
    [InlineData("owner/repo@feature/awesome-feature:very/deep/path/structure/with/many/segments/file.extension")]
    [InlineData("facebook/react@v18.2.0:packages/react-dom/src/client/ReactDOMRoot.js")]
    public void Parse_ComplexRealWorldExamples_WorksCorrectly(string input)
    {
        // These should not throw and should produce valid FileRef objects
        var result = FileRef.Parse(input);
        
        Assert.NotNull(result.OwnerRepo);
        Assert.Contains("/", result.OwnerRepo);
        Assert.NotNull(result.BranchOrTag);
        Assert.NotNull(result.FilePath);
    }

    [Theory]
    [InlineData("a/b")] // Minimum valid case
    [InlineData("owner123/repo456@very-long-branch-name-with-lots-of-characters:very/deep/path/structure/with/many/segments/file.extension")]
    [InlineData("user/project@feature/multi-level/branch/name:path/to/file.txt")]
    public void Parse_EdgeCaseLengths_WorksCorrectly(string input)
    {
        var result = FileRef.Parse(input);
        Assert.NotNull(result);
        Assert.NotEmpty(result.OwnerRepo);
    }

    [Theory]
    [InlineData("OWNER/REPO", "OWNER/REPO", null, null)]
    [InlineData("Owner/Repo@Branch", "Owner/Repo", "Branch", null)]
    [InlineData("owner/repo@MAIN:PATH/FILE.TXT", "owner/repo", "MAIN", "PATH/FILE.TXT")]
    [InlineData("GitHub/Copilot@Release/2024:src/main.cs", "GitHub/Copilot", "Release/2024", "src/main.cs")]
    public void Parse_CaseSensitive_PreservesCase(string input, string expectedOwnerRepo, string? expectedBranch, string? expectedPath)
    {
        var result = FileRef.Parse(input);
        
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.BranchOrTag);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("org/repo@123456:file.txt", "org/repo", "123456", "file.txt")]
    [InlineData("company/project@dev_branch:src/main.cs", "company/project", "dev_branch", "src/main.cs")]
    [InlineData("team/app@feature.new:docs/api.md", "team/app", "feature.new", "docs/api.md")]
    [InlineData("owner/repo@release/v1.0/hotfix:config/settings.json", "owner/repo", "release/v1.0/hotfix", "config/settings.json")]
    [InlineData("dev/tool@branch-with-unicode-??:??/??.txt", "dev/tool", "branch-with-unicode-??", "??/??.txt")]
    [InlineData("owner/repo@branch@tag:file.txt", "owner/repo", "branch@tag", "file.txt")] // @ in branch name is allowed
    [InlineData("owner/repo::path.txt", "owner/repo", null, ":path.txt")] // Double colon creates null ref and path starting with colon
    [InlineData("owner/repo@branch:with:colons.txt", "owner/repo", "branch", "with:colons.txt")] // Colons in path are allowed
    public void Parse_SpecialCharacterCombinations_WorksCorrectly(string input, string expectedOwnerRepo, string? expectedBranch, string expectedPath)
    {
        var result = FileRef.Parse(input);
        
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.BranchOrTag);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("owner/repo@feature/awesome: ", "owner/repo", "feature/awesome", " ")]
    [InlineData("owner/repo@main:	", "owner/repo", "main", "	")] // Tab character
    [InlineData("owner/repo@develop:x", "owner/repo", "develop", "x")] // Single character
    public void Parse_EdgeCaseMinimalPaths_WorksCorrectly(string input, string expectedOwnerRepo, string expectedBranch, string expectedPath)
    {
        var result = FileRef.Parse(input);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.BranchOrTag);
        Assert.Equal(expectedPath, result.FilePath);
    }
}