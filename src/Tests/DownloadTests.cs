using System.Net;

namespace Devlooped.Tests;

public class DownloadTests
{
    [LocalTheory]
    // Requires being authenticated with private kzu's GH repo
    [InlineData("github.com/kzu/runcs")]
    [InlineData("github.com/kzu/runcs@v0.1.0")]
    [InlineData("github.com/kzu/runcs@dev")]
    [InlineData("github.com/kzu/runcs@211de7614553152d848ef53dd9587d1a52c76582")]
    [InlineData("github.com/kzu/runcs@211de7614")]
    [InlineData("github.com/kzu/runcs@211de761455")]
    // Requires running the CLI app once against this private repo and saving a PAT
    [InlineData("gitlab.com/kzu/runcs")]
    [InlineData("gitlab.com/kzu/runcs@v0.1.0")]
    [InlineData("gitlab.com/kzu/runcs@dev")]
    [InlineData("gitlab.com/kzu/runcs@533ecac61d4cf62dac0c72567e73753acd235ac2")]
    [InlineData("gitlab.com/kzu/runcs@533ecac61")]
    [InlineData("gitlab.com/kzu/runcs@533ecac61d4")]
    // Also private auth
    //[InlineData("bitbucket.org/kzu/runcs")]
    //[InlineData("bitbucket.org/kzu/runcs@v0.1.0")]
    //[InlineData("bitbucket.org/kzu/runcs@dev")]
    public async Task DownloadPrivateUnchanged(string value)
    {
        Assert.True(RemoteRef.TryParse(value, out var location));

        var provider = DownloadProvider.Create(location);
        var contents = await provider.GetAsync(location);

        Assert.True(contents.IsSuccessStatusCode);

        var etag = contents.Headers.ETag?.ToString();

        location = location with
        {
            ETag = etag,
            ResolvedUri = contents.OriginalUri
        };

        var refresh = await provider.GetAsync(location);

        Assert.Equal(HttpStatusCode.NotModified, refresh.StatusCode);
    }
}
