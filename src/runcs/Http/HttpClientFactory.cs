using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Devlooped.Http;

static class HttpClientFactory
{
    public static HttpClient Create()
        => new(
            new GitAuthHandler(
                new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression =
                    DecompressionMethods.Brotli | DecompressionMethods.GZip
                }))
        {
            Timeout = Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(15)
        };
}
