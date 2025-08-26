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
    public void TryParse_OwnerRepoOnly_SetsOwnerRepoAndNullsOthers(string input, string expectedOwnerRepo, string? expectedBranch, string? expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
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
    public void TryParse_OwnerRepoWithBranch_SetsOwnerRepoAndBranch(string input, string expectedOwnerRepo, string expectedBranch, string? expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
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
    public void TryParse_OwnerRepoWithPath_SetsOwnerRepoAndPath(string input, string expectedOwnerRepo, string? expectedBranch, string expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
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
    public void TryParse_OwnerRepoWithBranchAndPath_SetsAllProperties(string input, string expectedOwnerRepo, string expectedBranch, string expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
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
    public void TryParse_InvalidFormats_ReturnsFalse(string input)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse()
    {
        var success = FileRef.TryParse(null!, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("github/copilot", "github", "copilot")]
    [InlineData("microsoft/vscode", "microsoft", "vscode")]
    [InlineData("dotnet/core", "dotnet", "core")]
    [InlineData("octocat/Hello-World", "octocat", "Hello-World")]
    public void TryParse_RealWorldExamples_WorksCorrectly(string input, string expectedOwner, string expectedRepo)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal($"{expectedOwner}/{expectedRepo}", result.OwnerRepo);
        Assert.Null(result.Ref);
        Assert.Null(result.FilePath);
    }

    [Theory]
    [InlineData("microsoft/vscode@main:src/vs/workbench/workbench.main.ts")]
    [InlineData("dotnet/aspnetcore@release/8.0:src/Framework/AspNetCoreAnalyzers/src/Analyzers/Infrastructure/VirtualChars/VirtualCharSequence.cs")]
    [InlineData("octocat/Hello-World@master:README")]
    [InlineData("owner/repo@feature/awesome-feature:very/deep/path/structure/with/many/segments/file.extension")]
    [InlineData("facebook/react@v18.2.0:packages/react-dom/src/client/ReactDOMRoot.js")]
    public void TryParse_ComplexRealWorldExamples_WorksCorrectly(string input)
    {
        // These should not throw and should produce valid FileRef objects
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.NotNull(result.OwnerRepo);
        Assert.Contains("/", result.OwnerRepo);
        Assert.NotNull(result.Ref);
        Assert.NotNull(result.FilePath);
    }

    [Theory]
    [InlineData("a/b")] // Minimum valid case
    [InlineData("owner123/repo456@very-long-branch-name-with-lots-of-characters:very/deep/path/structure/with/many/segments/file.extension")]
    [InlineData("user/project@feature/multi-level/branch/name:path/to/file.txt")]
    public void TryParse_EdgeCaseLengths_WorksCorrectly(string input)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.NotEmpty(result.OwnerRepo);
    }

    [Theory]
    [InlineData("OWNER/REPO", "OWNER/REPO", null, null)]
    [InlineData("Owner/Repo@Branch", "Owner/Repo", "Branch", null)]
    [InlineData("owner/repo@MAIN:PATH/FILE.TXT", "owner/repo", "MAIN", "PATH/FILE.TXT")]
    [InlineData("GitHub/Copilot@Release/2024:src/main.cs", "GitHub/Copilot", "Release/2024", "src/main.cs")]
    public void TryParse_CaseSensitive_PreservesCase(string input, string expectedOwnerRepo, string? expectedBranch, string? expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
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
    public void TryParse_SpecialCharacterCombinations_WorksCorrectly(string input, string expectedOwnerRepo, string? expectedBranch, string expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("owner/repo@feature/awesome: ", "owner/repo", "feature/awesome", " ")]
    [InlineData("owner/repo@main:	", "owner/repo", "main", "	")] // Tab character
    [InlineData("owner/repo@develop:x", "owner/repo", "develop", "x")] // Single character
    public void TryParse_EdgeCaseMinimalPaths_WorksCorrectly(string input, string expectedOwnerRepo, string expectedBranch, string expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("github.com/owner/repo", "github.com", "owner/repo", null, null)]
    [InlineData("gitlab.com/user/project", "gitlab.com", "user/project", null, null)]
    [InlineData("bitbucket.org/team/repository", "bitbucket.org", "team/repository", null, null)]
    [InlineData("codeberg.org/dev/tool", "codeberg.org", "dev/tool", null, null)]
    [InlineData("git.example.com/company/app", "git.example.com", "company/app", null, null)]
    [InlineData("source.internal.org/internal/project", "source.internal.org", "internal/project", null, null)]
    [InlineData("dev.azure.com/org/project", "dev.azure.com", "org/project", null, null)]
    [InlineData("git.sr.ht/user/repo", "git.sr.ht", "user/repo", null, null)]
    public void TryParse_WithHost_SetsHostAndOwnerRepo(string input, string expectedHost, string expectedOwnerRepo, string? expectedBranch, string? expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedHost, result.Host);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("github.com/owner/repo@main", "github.com", "owner/repo", "main", null)]
    [InlineData("gitlab.com/user/project@develop", "gitlab.com", "user/project", "develop", null)]
    [InlineData("bitbucket.org/team/repository@feature/awesome", "bitbucket.org", "team/repository", "feature/awesome", null)]
    [InlineData("codeberg.org/dev/tool@v1.0.0", "codeberg.org", "dev/tool", "v1.0.0", null)]
    [InlineData("git.example.com/company/app@release/2024", "git.example.com", "company/app", "release/2024", null)]
    [InlineData("source.internal.org/internal/project@hotfix/urgent", "source.internal.org", "internal/project", "hotfix/urgent", null)]
    [InlineData("dev.azure.com/org/project@refs/heads/main", "dev.azure.com", "org/project", "refs/heads/main", null)]
    [InlineData("git.sr.ht/user/repo@master", "git.sr.ht", "user/repo", "master", null)]
    public void TryParse_WithHostAndBranch_SetsHostOwnerRepoAndBranch(string input, string expectedHost, string expectedOwnerRepo, string expectedBranch, string? expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedHost, result.Host);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("github.com/owner/repo:src/main.cs", "github.com", "owner/repo", null, "src/main.cs")]
    [InlineData("gitlab.com/user/project:docs/README.md", "gitlab.com", "user/project", null, "docs/README.md")]
    [InlineData("bitbucket.org/team/repository:config/settings.json", "bitbucket.org", "team/repository", null, "config/settings.json")]
    [InlineData("codeberg.org/dev/tool:lib/utils.py", "codeberg.org", "dev/tool", null, "lib/utils.py")]
    [InlineData("git.example.com/company/app:frontend/index.html", "git.example.com", "company/app", null, "frontend/index.html")]
    [InlineData("source.internal.org/internal/project:scripts/deploy.sh", "source.internal.org", "internal/project", null, "scripts/deploy.sh")]
    [InlineData("dev.azure.com/org/project:azure-pipelines.yml", "dev.azure.com", "org/project", null, "azure-pipelines.yml")]
    [InlineData("git.sr.ht/user/repo:Makefile", "git.sr.ht", "user/repo", null, "Makefile")]
    public void TryParse_WithHostAndPath_SetsHostOwnerRepoAndPath(string input, string expectedHost, string expectedOwnerRepo, string? expectedBranch, string expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedHost, result.Host);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("github.com/owner/repo@main:src/main.cs", "github.com", "owner/repo", "main", "src/main.cs")]
    [InlineData("gitlab.com/user/project@develop:docs/README.md", "gitlab.com", "user/project", "develop", "docs/README.md")]
    [InlineData("bitbucket.org/team/repository@feature/new-api:config/settings.json", "bitbucket.org", "team/repository", "feature/new-api", "config/settings.json")]
    [InlineData("codeberg.org/dev/tool@v2.1.0:lib/utils.py", "codeberg.org", "dev/tool", "v2.1.0", "lib/utils.py")]
    [InlineData("git.example.com/company/app@release/2024:frontend/index.html", "git.example.com", "company/app", "release/2024", "frontend/index.html")]
    [InlineData("source.internal.org/internal/project@hotfix/critical:scripts/deploy.sh", "source.internal.org", "internal/project", "hotfix/critical", "scripts/deploy.sh")]
    [InlineData("dev.azure.com/org/project@refs/heads/feature:azure-pipelines.yml", "dev.azure.com", "org/project", "refs/heads/feature", "azure-pipelines.yml")]
    [InlineData("git.sr.ht/user/repo@experimental:Makefile", "git.sr.ht", "user/repo", "experimental", "Makefile")]
    public void TryParse_WithHostBranchAndPath_SetsAllProperties(string input, string expectedHost, string expectedOwnerRepo, string expectedBranch, string expectedPath)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedHost, result.Host);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Equal(expectedBranch, result.Ref);
        Assert.Equal(expectedPath, result.FilePath);
    }

    [Theory]
    [InlineData("my-git-server.co.uk/owner/repo", "my-git-server.co.uk", "owner/repo")]
    [InlineData("git-hosting.example.org/user/project", "git-hosting.example.org", "user/project")]
    [InlineData("source.company-name.io/team/app", "source.company-name.io", "team/app")]
    [InlineData("git.my-domain.net/dev/tool", "git.my-domain.net", "dev/tool")]
    [InlineData("code.enterprise.com/internal/service", "code.enterprise.com", "internal/service")]
    [InlineData("vcs.startup.tech/product/frontend", "vcs.startup.tech", "product/frontend")]
    [InlineData("repos.university.edu/research/project", "repos.university.edu", "research/project")]
    [InlineData("git123.hosting456.biz/client/website", "git123.hosting456.biz", "client/website")]
    public void TryParse_WithCustomHostDomains_WorksCorrectly(string input, string expectedHost, string expectedOwnerRepo)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedHost, result.Host);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
        Assert.Null(result.Ref);
        Assert.Null(result.FilePath);
    }

    [Theory]
    [InlineData("localhost/owner/repo")] // localhost without TLD
    [InlineData("git/owner/repo")] // No TLD
    [InlineData("192.168.1.1/owner/repo")] // IP address
    [InlineData("git.a/owner/repo")] // TLD too short (needs 2+ chars)
    [InlineData("example.com./owner/repo")] // Invalid hostname (ends with dot)
    [InlineData("git host.com/owner/repo")] // Space in hostname
    [InlineData("git_host.com/owner/repo")] // Underscore in hostname (not allowed in regex)
    public void TryParse_WithInvalidHosts_ReturnsFalse(string input)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("GITHUB.COM/owner/repo", "GITHUB.COM", "owner/repo")]
    [InlineData("GitLab.Com/user/project", "GitLab.Com", "user/project")]
    [InlineData("BitBucket.Org/team/app", "BitBucket.Org", "team/app")]
    [InlineData("GIT.EXAMPLE.COM/company/service", "GIT.EXAMPLE.COM", "company/service")]
    public void TryParse_WithUppercaseHosts_PreservesCase(string input, string expectedHost, string expectedOwnerRepo)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedHost, result.Host);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
    }

    [Theory]
    [InlineData("github.com/microsoft/vscode@main:src/vs/workbench/workbench.main.ts")]
    [InlineData("gitlab.com/gitlab-org/gitlab@master:app/assets/javascripts/main.js")]
    [InlineData("bitbucket.org/atlassian/stash@develop:stash-web/src/main/webapp/static/feature.js")]
    [InlineData("codeberg.org/forgejo/forgejo@forgejo:web/src/js/features/repo-diff.js")]
    [InlineData("git.sr.ht/sircmpwn/aerc@master:widgets/msgviewer.go")]
    public void TryParse_RealWorldHostExamplesWithAllComponents_WorksCorrectly(string input)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.NotNull(result.Host);
        Assert.NotNull(result.OwnerRepo);
        Assert.NotNull(result.Ref);
        Assert.NotNull(result.FilePath);
        Assert.Contains(".", result.Host); // Should have a TLD
        Assert.Contains("/", result.OwnerRepo); // Should have owner/repo format
    }

    [Fact]
    public void TryParse_WithoutHost_HostIsNull()
    {
        var success = FileRef.TryParse("owner/repo@main:file.txt", out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Null(result.Host);
        Assert.Equal("owner/repo", result.OwnerRepo);
        Assert.Equal("main", result.Ref);
        Assert.Equal("file.txt", result.FilePath);
    }

    [Theory]
    [InlineData("git-.example.com/owner/repo", "git-.example.com", "owner/repo")] // Dash at start is allowed in regex
    [InlineData("git..example.com/owner/repo", "git..example.com", "owner/repo")] // Double dots are allowed in regex
    [InlineData("a.bc/owner/repo", "a.bc", "owner/repo")] // Minimum 2-char TLD
    [InlineData("git-.example.com/owner/repo@main:file.txt", "git-.example.com", "owner/repo")] // Dash with branch and path
    [InlineData("git..example.com/owner/repo@develop", "git..example.com", "owner/repo")] // Double dots with branch
    public void TryParse_WithEdgeCaseValidHosts_WorksCorrectly(string input, string expectedHost, string expectedOwnerRepo)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(expectedHost, result.Host);
        Assert.Equal(expectedOwnerRepo, result.OwnerRepo);
    }

    [Theory]
    [InlineData("github.com/owner/repo@special-chars!@#$%:file.txt")] // Special chars in branch name
    [InlineData("example.com/owner/repo:path/with spaces and unicode ??.txt")] // Unicode in path
    public void TryParse_WithSpecialCharacters_WorksCorrectly(string input)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.NotNull(result.Host);
        Assert.NotNull(result.OwnerRepo);
    }

    [Theory]
    [InlineData("github.com/~invalid/repo")] // Tilde not allowed in owner name
    [InlineData("gitlab.com/user~/repo")] // Tilde not allowed in owner name
    [InlineData("bitbucket.org/user@invalid/repo")] // @ not allowed in owner name (outside branch context)
    [InlineData("example.com/owner/repo$invalid")] // $ not allowed in repo name
    [InlineData("example.com/owner/repo with space")] // Space not allowed in repo name
    public void TryParse_WithInvalidOwnerRepoCharacters_ReturnsFalse(string input)
    {
        var success = FileRef.TryParse(input, out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}