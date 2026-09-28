namespace TileStories
{
    // Which links a card opens (_3.1 Tier 2 group B, today_map's Directions), pure: only a complete web address -- http or
    // https with a host. Anything else (no scheme, "javascript:", "file:", a bare word) is never handed to the device:
    // the card hides its button and the Editor warns under the row.
    public static class WebLinkRule
    {
        // Whether the card opens this link
        public static bool IsOpenable(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (!System.Uri.TryCreate(url.Trim(), System.UriKind.Absolute, out var uri)) return false;
            return (uri.Scheme == System.Uri.UriSchemeHttps || uri.Scheme == System.Uri.UriSchemeHttp) && uri.Host.Length > 0;
        }

        // The link as the card opens it (trimmed), or "" when it would not open it
        public static string Openable(string url) => IsOpenable(url) ? url.Trim() : "";
    }
}
