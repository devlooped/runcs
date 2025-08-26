using System;
using System.Net;
using System.Net.Http;

namespace Devlooped;

static class Extensions
{
    public static bool IsRedirect(this HttpStatusCode status) =>
        status is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    public static HttpRequestMessage WithTag(this HttpRequestMessage request, string? etag)
    {
        if (!string.IsNullOrEmpty(etag))
            request.Headers.IfNoneMatch.ParseAdd(etag);

        return request;
    }

    public static HttpRequestMessage Retry(this HttpResponseMessage response, Uri uri)
    {
        var retry = new HttpRequestMessage(HttpMethod.Get, uri);
        if (response.RequestMessage == null)
            return retry;

        if (response.RequestMessage.Headers.Authorization != null)
            retry.Headers.Authorization = response.RequestMessage.Headers.Authorization;

        foreach (var etag in response.RequestMessage.Headers.IfNoneMatch)
        {
            retry.Headers.IfNoneMatch.Add(etag);
        }

        return retry;
    }
}
