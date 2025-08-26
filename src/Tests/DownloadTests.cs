using System.Net;

namespace Devlooped.Tests;

public class DownloadTests
{
    [Theory]
    [InlineData("github.com/kzu/runcs")]
    [InlineData("github.com/kzu/runcs@v0.1.0")]
    [InlineData("github.com/kzu/runcs@211de7614553152d848ef53dd9587d1a52c76582")]
    [InlineData("github.com/kzu/runcs@211de7614")]
    [InlineData("github.com/kzu/runcs@211de761455")]
    public async Task DownloadPrivateUnchanged(string value)
    {
        Assert.True(FileRef.TryParse(value, out var location));

        var provider = Assert.IsType<GitHubDownloadProvider>(DownloadProvider.Create(location));

        var contents = await provider.GetAsync(location);

        Assert.True(contents.IsSuccessStatusCode);

        var etag = contents.Headers.ETag?.ToString();
        location = location with
        {
            ETag = etag,
            ResolvedUri = new Uri(contents.RequestMessage!.RequestUri!.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped))
        };

        var refresh = await provider.GetAsync(location);

        Assert.Equal(HttpStatusCode.NotModified, refresh.StatusCode);
    }
}
