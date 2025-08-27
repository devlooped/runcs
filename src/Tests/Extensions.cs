namespace Devlooped.Tests;

static class Extensions
{
    extension(HttpResponseMessage message)
    {
        public Uri OriginalUri => message.RequestMessage!.Headers.TryGetValues("X-Original-URI", out var values) &&
            values.FirstOrDefault() is { } url ? new Uri(url) : message.RequestMessage!.RequestUri!;
    }
}
