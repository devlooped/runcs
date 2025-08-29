using System.Net;

namespace Devlooped.Http;

/// <summary>A handler that redirects traffic preserving auth/etag headers to originating domain or subdomains.</summary>
public sealed class RedirectingHttpHandler(HttpMessageHandler innerHandler, params string[] followHosts) : DelegatingHandler(innerHandler)
{
    /// <summary>Maximum number of redirects to follow. Default is 10.</summary>
    public int MaxRedirects { get; set; } = 10;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ThrowIfNull(request);

        var currentRequest = request;
        var originalUri = request.RequestUri ?? throw new InvalidOperationException("RequestUri is required.");
        var originalHost = string.Join('.', NormalizeHost(originalUri.Host).Split('.')[^2..]);
        var redirectCount = 0;

        while (true)
        {
            var response = await base.SendAsync(currentRequest, cancellationToken).ConfigureAwait(false);

            if (!response.StatusCode.IsRedirect() || response.Headers.Location is null)
                return response;

            var location = response.Headers.Location;
            var nextUri = location.IsAbsoluteUri ? location : new Uri(currentRequest.RequestUri!, location);

            var nextHost = NormalizeHost(nextUri.Host);
            // Never redirect to a different domain (security)
            if (!IsWithinSubdomain(originalHost, nextHost) &&
                !followHosts.Any(host => IsWithinSubdomain(host, nextHost)))
                return response;

            // Limit to prevent loops
            if (++redirectCount > MaxRedirects)
            {
                response.Dispose();
                throw new HttpRequestException($"Too many redirects (>{MaxRedirects}).");
            }

            // Decide next method and content per RFC / .NET behavior
            var nextMethod = currentRequest.Method;
            HttpContent? nextContent = null;

            switch (response.StatusCode)
            {
                case HttpStatusCode.SeeOther:            // 303 -> force GET, drop body
                    nextMethod = HttpMethod.Get;
                    break;

                case HttpStatusCode.Moved:               // 301
                case HttpStatusCode.Found:               // 302
                    if (currentRequest.Method == HttpMethod.Post)
                        nextMethod = HttpMethod.Get;
                    else if (currentRequest.Content != null)
                        nextContent = await CloneHttpContentAsync(currentRequest.Content).ConfigureAwait(false);
                    break;

                case HttpStatusCode.TemporaryRedirect:   // 307
                case HttpStatusCode.PermanentRedirect:   // 308
                    if (currentRequest.Content != null)
                        nextContent = await CloneHttpContentAsync(currentRequest.Content).ConfigureAwait(false);
                    break;
            }

            // Build next request
            var next = new HttpRequestMessage(nextMethod, nextUri)
            {
                Version = currentRequest.Version,
                VersionPolicy = currentRequest.VersionPolicy,
                Content = nextContent
            };

            // Copy headers (preserve conditional ones like If-None-Match)
            CopyHeaders(currentRequest, next);

            next.Headers.TryAddWithoutValidation("X-Original-URI", originalUri.AbsoluteUri);

            // We're not going to read this 3xx body; free the socket.
            response.Dispose();

            // Prepare next hop
            currentRequest = next;
        }
    }

    static void CopyHeaders(HttpRequestMessage from, HttpRequestMessage to)
    {
        foreach (var h in from.Headers)
        {
            if (string.Equals(h.Key, "Host", StringComparison.OrdinalIgnoreCase))
                continue;
            to.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        if (from.Content != null && to.Content != null)
        {
            foreach (var h in from.Content.Headers)
                to.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }
    }

    static async Task<HttpContent> CloneHttpContentAsync(HttpContent original)
    {
        var bytes = await original.ReadAsByteArrayAsync().ConfigureAwait(false);
        var clone = new ByteArrayContent(bytes);

        foreach (var h in original.Headers)
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);

        return clone;
    }

    static string NormalizeHost(string host) => host.TrimEnd('.'); // normalize trailing dot

    /// <summary>
    /// Returns true if candidateHost equals baseHost OR is a subdomain of baseHost
    /// (i.e., ends with "." + baseHost). Case-insensitive.
    /// Examples:
    /// base: example.com -> ok: example.com, a.example.com, b.a.example.com
    /// base: api.example.com -> ok: api.example.com, v2.api.example.com
    /// </summary>
    static bool IsWithinSubdomain(string baseHost, string candidateHost)
    {
        if (candidateHost.Equals(baseHost, StringComparison.OrdinalIgnoreCase))
            return true;

        // require a dot boundary to avoid "notexample.com" false positives
        return candidateHost.EndsWith("." + baseHost, StringComparison.OrdinalIgnoreCase);
    }
}
