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
        // A video clip (video, header video_loop; step 9B): mp4 / webm, imported by Unity as a VideoClip
        Video,
        // A 3D model (model_3d, header model_turntable; step 10A): glb / gltf, imported by glTFast as a prefab (a GameObject)
        Model,
        // A 360 equirect picture (panorama_360; step 10A): the picture file types, read as a texture, but its own kind so the picker and the
        // default library keep panoramas apart from ordinary pictures
        Panorama,
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
    //
    // A value may instead be a DEFAULT key (_3.1 step 13, "default:<key>"): a pointer into the Framework's own generated
    // media library (or a wall's override of it, CardMediaLibraryLookup), never a path on disk. Check treats a non-empty
    // key as valid for any kind here -- whether the key actually HOLDS media of that kind is a lookup question, checked
    // where the library is available (the Editor drawer, CardMediaSource.Load), not here.
    public static class MediaPathRule
    {
        public const string DefaultKeyPrefix = "default:";

        // The file types each kind takes (what Resources.Load reads from the imported files)
        public static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };
        public static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg" };
        public static readonly string[] CaptionExtensions = { ".vtt" };
        // - the two containers Unity decodes on every platform the app targets (H.264 mp4 and VP8 webm)
        public static readonly string[] VideoExtensions = { ".mp4", ".webm" };
        // - what glTFast imports as a prefab (a .gltf keeps its .bin and pictures beside it; a .glb is one file)
        public static readonly string[] ModelExtensions = { ".glb", ".gltf" };
        // - an equirect 360 picture is a picture file (2:1)
        public static readonly string[] PanoramaExtensions = ImageExtensions;

        // Whether a stored value names a default-library key rather than a path
        public static bool IsDefaultKey(string path) => !string.IsNullOrWhiteSpace(path) && Normalize(path).StartsWith(DefaultKeyPrefix, System.StringComparison.Ordinal);

        // The key part of a "default:<key>" value, or null when `path` is not a default key
        public static string DefaultKeyOf(string path) => IsDefaultKey(path) ? Normalize(path).Substring(DefaultKeyPrefix.Length) : null;

        // The value to store for picking a default library entry by key
        public static string PathForDefaultKey(string key) => DefaultKeyPrefix + (key ?? "");

        // The MediaKind a loaded asset TYPE reads as (the reverse of IsAssetOfKind), used to look a default key up in a
        // CardMediaLibrary by kind when only the generic Load<T> knows the wanted type
        public static MediaKind KindOfAssetType(System.Type type)
        {
            if (type == null) return MediaKind.None;
            if (typeof(UnityEngine.Texture).IsAssignableFrom(type)) return MediaKind.Image;
            if (typeof(UnityEngine.AudioClip).IsAssignableFrom(type)) return MediaKind.Audio;
            if (typeof(UnityEngine.TextAsset).IsAssignableFrom(type)) return MediaKind.Captions;
            if (typeof(UnityEngine.Video.VideoClip).IsAssignableFrom(type)) return MediaKind.Video;
            if (typeof(UnityEngine.GameObject).IsAssignableFrom(type)) return MediaKind.Model;
            return MediaKind.None;
        }

        // Every kind a loaded asset TYPE may be read as, in the order a default key is looked up: a texture is a picture first, then a
        // panorama (both are textures: only the library entry's kind tells them apart)
        public static MediaKind[] KindsOfAssetType(System.Type type)
        {
            var kind = KindOfAssetType(type);
            return kind == MediaKind.Image ? new[] { MediaKind.Image, MediaKind.Panorama } : new[] { kind };
        }

        // The extensions a field of this kind takes (empty for None)
        public static string[] ExtensionsOf(MediaKind kind) => kind switch
        {
            MediaKind.Image => ImageExtensions,
            MediaKind.Audio => AudioExtensions,
            MediaKind.Captions => CaptionExtensions,
            MediaKind.Video => VideoExtensions,
            MediaKind.Model => ModelExtensions,
            MediaKind.Panorama => PanoramaExtensions,
            _ => System.Array.Empty<string>(),
        };

        public static MediaPathProblem Check(string path, MediaKind kind)
        {
            if (string.IsNullOrWhiteSpace(path)) return MediaPathProblem.Empty;
            string p = Normalize(path);
            if (IsDefaultKey(p)) return DefaultKeyOf(p).Length > 0 ? MediaPathProblem.None : MediaPathProblem.Empty;
            if (p.StartsWith("/") || p.Contains(":") || p.StartsWith("Assets/") || p.StartsWith("Packages/")
                || p == ".." || p.StartsWith("../") || p.Contains("/../") || p.EndsWith("/.."))
                return MediaPathProblem.OutsideFolder;
            return HasExtensionOf(p, kind) ? MediaPathProblem.None : MediaPathProblem.WrongType;
        }

        public static bool IsValid(string path, MediaKind kind) => Check(path, kind) == MediaPathProblem.None;

        // The kind a path's extension names (.mp3 = Audio, .vtt = Captions, .png = Image, .mp4 = Video, .glb = Model), or None for no or
        // another extension. A picture file names Image (a panorama is a picture file too: its field's kind, not its extension, says so)
        public static MediaKind KindOfExtension(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return MediaKind.None;
            string p = Normalize(path);
            foreach (var kind in new[] { MediaKind.Image, MediaKind.Audio, MediaKind.Captions, MediaKind.Video, MediaKind.Model })
                if (HasExtensionOf(p, kind)) return kind;
            return MediaKind.None;
        }

        // Whether a loaded Unity asset is the type a kind is read as (a texture, an AudioClip, a TextAsset, a VideoClip, a prefab); None fits
        // anything
        public static bool IsAssetOfKind(UnityEngine.Object asset, MediaKind kind) => kind switch
        {
            MediaKind.Image => asset is UnityEngine.Texture,
            MediaKind.Audio => asset is UnityEngine.AudioClip,
            MediaKind.Captions => asset is UnityEngine.TextAsset,
            MediaKind.Video => asset is UnityEngine.Video.VideoClip,
            MediaKind.Model => asset is UnityEngine.GameObject,
            MediaKind.Panorama => asset is UnityEngine.Texture,
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
