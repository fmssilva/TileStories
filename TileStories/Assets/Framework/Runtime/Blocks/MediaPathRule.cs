namespace TileStories
{
    // What media an Asset field holds (_3.1 section 5.3). A kind names it on the field (BlockFieldDefinition.Media), so the
    // Editor filters its object field and the rule below checks the file type. Kinds add values with their tier.
    public enum MediaKind
    {
        None,
        Image,
        // A sound clip (audio_guide, step 9A): mp3 / wav / ogg, imported by Unity as an AudioClip
        Audio,
        // A WebVTT captions file (.vtt), imported by the framework's VttTextImporter as a TextAsset
        Captions,
    }

    // What is wrong with a stored media path, if anything (MediaPathRule.Check)
    public enum MediaPathProblem
    {
        None,
        // Nothing stored
        Empty,
        // Not a path inside the wall's media folder: rooted, an "Assets/..." project path, or climbing out with ".."
        OutsideFolder,
        // A file of another kind than the field takes (an .mp3 in an image field)
        WrongType,
    }

    // The ONE rule for an Asset field's value (_3.1 step 7): a path RELATIVE to the wall's media folder
    // (card_settings.media_resources_path, inside a Resources folder), with a file type the field's MediaKind takes. The
    // Editor stores whatever the developer picked -- a file outside the folder keeps its project path -- so this rule, not
    // the drawer, decides: the Editor warns with it and BlockStackBuilder gives it as a "Not shown" reason
    // (SkipReason.InvalidMedia; an Items row with such an image is incomplete). Pure, so every case is a unit test.
    public static class MediaPathRule
    {
        // The file types each kind takes (what Resources.Load reads from the imported files)
        public static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };
        public static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg" };
        public static readonly string[] CaptionExtensions = { ".vtt" };

        // The extensions a field of this kind takes (empty for None)
        public static string[] ExtensionsOf(MediaKind kind) => kind switch
        {
            MediaKind.Image => ImageExtensions,
            MediaKind.Audio => AudioExtensions,
            MediaKind.Captions => CaptionExtensions,
            _ => System.Array.Empty<string>(),
        };

        public static MediaPathProblem Check(string path, MediaKind kind)
        {
            if (string.IsNullOrWhiteSpace(path)) return MediaPathProblem.Empty;
            string p = Normalize(path);
            if (p.StartsWith("/") || p.Contains(":") || p.StartsWith("Assets/") || p.StartsWith("Packages/")
                || p == ".." || p.StartsWith("../") || p.Contains("/../") || p.EndsWith("/.."))
                return MediaPathProblem.OutsideFolder;
            return HasExtensionOf(p, kind) ? MediaPathProblem.None : MediaPathProblem.WrongType;
        }

        public static bool IsValid(string path, MediaKind kind) => Check(path, kind) == MediaPathProblem.None;

        // The kind a path's extension names (.mp3 = Audio, .vtt = Captions, .png = Image), or None for no or another extension
        public static MediaKind KindOfExtension(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return MediaKind.None;
            string p = Normalize(path);
            foreach (var kind in new[] { MediaKind.Image, MediaKind.Audio, MediaKind.Captions })
                if (HasExtensionOf(p, kind)) return kind;
            return MediaKind.None;
        }

        // Whether a loaded Unity asset is the type a kind is read as (a texture, an AudioClip, a TextAsset); None fits anything
        public static bool IsAssetOfKind(UnityEngine.Object asset, MediaKind kind) => kind switch
        {
            MediaKind.Image => asset is UnityEngine.Texture,
            MediaKind.Audio => asset is UnityEngine.AudioClip,
            MediaKind.Captions => asset is UnityEngine.TextAsset,
            _ => true,
        };

        // The path to store for a project file (`assetPath` "Assets/.../Resources/<mediaFolder>/castle/hero.png"): the part
        // after the media folder ("castle/hero.png") when the file lies inside it, else the project path unchanged -- stored
        // as picked, so Check reports it as OutsideFolder instead of the Editor silently refusing the pick
        public static string StoredPathFor(string assetPath, string mediaFolder)
        {
            if (string.IsNullOrWhiteSpace(assetPath)) return "";
            string asset = Normalize(assetPath);
            string folder = Normalize(mediaFolder ?? "").Trim('/');
            string marker = folder.Length == 0 ? "/Resources/" : "/Resources/" + folder + "/";
            int at = asset.IndexOf(marker, System.StringComparison.Ordinal);
            return at >= 0 ? asset.Substring(at + marker.Length) : asset;
        }

        private static bool HasExtensionOf(string path, MediaKind kind)
        {
            string lower = path.ToLowerInvariant();
            foreach (string ext in ExtensionsOf(kind))
                if (lower.EndsWith(ext)) return true;
            return false;
        }

        private static string Normalize(string path) => path.Trim().Replace('\\', '/');
    }
}
