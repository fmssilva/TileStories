using System.IO;
using UnityEditor;
using UnityEditor.Media;
using UnityEngine;

namespace TileStories.Editor
{
    // Generates the Framework's own default media library (_3.1 step 13): a small set of pictures, two short audio
    // clips (one with captions) and one short video, made ONLY by this script -- no third-party material, so no
    // licence question. Then (re)builds CardMediaLibrary.asset (Resources/TileStories/, so it resolves at runtime
    // exactly like a wall's own override does) pointing at the generated, imported files. Every picture is a plain
    // geometric pattern: abstract, app-agnostic, not tied to any one wall's real content (_5.1's "guide content
    // must stay app-agnostic" rule applies to shipped Framework defaults too). Run GenerateAll from the Editor
    // console or a script; the generated files and the asset are checked in, CC0.
    public static class CardMediaLibraryGenerator
    {
        public const string RootFolder = "Assets/Framework/Runtime/Resources/TileStories";
        public const string PicturesFolder = RootFolder + "/CardMedia/pictures";
        public const string AudioFolder = RootFolder + "/CardMedia/audio";
        public const string VideoFolder = RootFolder + "/CardMedia/video";
        public const string LibraryPath = RootFolder + "/CardMediaLibrary.asset";

        private static readonly Color32 Blue = new(31, 63, 143, 255);
        private static readonly Color32 Yellow = new(217, 169, 58, 255);
        private static readonly Color32 Ochre = new(180, 120, 42, 255);
        private static readonly Color32 White = new(242, 238, 227, 255);
        private static readonly Color32 Grout = new(200, 194, 180, 255);
        private static readonly Color32 Paper = new(232, 226, 210, 255);
        private static readonly Color32 Street = new(252, 250, 244, 255);
        private static readonly Color32 Ink = new(40, 36, 30, 255);

        [MenuItem("TileStories/Dev/Regenerate Default Media Library", priority = 2000)]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(PicturesFolder);
            Directory.CreateDirectory(AudioFolder);
            Directory.CreateDirectory(VideoFolder);

            WritePng(Path.Combine(PicturesFolder, "azulejo_blue.png"), 384, 384, Tile(384, 384, 48, Blue, Yellow, White));
            WritePng(Path.Combine(PicturesFolder, "azulejo_ochre.png"), 384, 384, Tile(384, 384, 48, Ochre, Blue, White));
            WritePng(Path.Combine(PicturesFolder, "azulejo_detail.png"), 512, 512, Tile(512, 512, 256, Blue, Yellow, White));
            WritePng(Path.Combine(PicturesFolder, "panel_plain.png"), 512, 320, Panel(512, 320));
            WritePng(Path.Combine(PicturesFolder, "poster_plain.png"), 320, 480, Poster(320, 480));
            WritePng(Path.Combine(PicturesFolder, "map_plan.png"), 512, 320, MapPlan(512, 320));
            WriteReadme();

            WriteWav(Path.Combine(AudioFolder, "chime.wav"), Chime());
            var (ambient, cues) = Ambient();
            WriteWav(Path.Combine(AudioFolder, "ambient.wav"), ambient);
            WriteVtt(Path.Combine(AudioFolder, "ambient.vtt"), cues);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            WriteVideo(Path.Combine(VideoFolder, "tile_pattern.mp4"));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            BuildLibrary();
            Debug.Log("[CardMediaLibraryGenerator] regenerated the default media library at " + LibraryPath);
        }

        // ---------------- pictures ----------------

        private delegate Color32 Pixel(int x, int y);

        private static Pixel Tile(int w, int h, int tile, Color32 ink, Color32 accent, Color32 glaze) => (x, y) =>
        {
            if (x % tile == 0 || y % tile == 0) return Grout;
            float u = (x % tile) / (float)tile, v = (y % tile) / (float)tile;
            float du = Mathf.Abs(u - 0.5f), dv = Mathf.Abs(v - 0.5f);
            if (du + dv < 0.08f) return accent;
            if (Mathf.Abs(du + dv - 0.30f) < 0.05f) return ink;
            foreach (var (cx, cy) in new[] { (0, 0), (1, 0), (0, 1), (1, 1) })
                if (Mathf.Abs(Mathf.Sqrt((u - cx) * (u - cx) + (v - cy) * (v - cy)) - 0.32f) < 0.05f) return ink;
            return glaze;
        };

        // A plain bordered rectangle: a generic surface for any "panel" media use (posters, plaques, boards)
        private static Pixel Panel(int w, int h) => (x, y) =>
        {
            const int border = 10;
            if (x < border || y < border || x >= w - border || y >= h - border) return Ink;
            return White;
        };

        // A portrait "poster": a plain field with a framed inner rectangle and a decorative bar near the bottom
        private static Pixel Poster(int w, int h) => (x, y) =>
        {
            const int margin = 24;
            bool onFrame = x == margin || y == margin || x == w - margin - 1 || y == h - margin - 1;
            if (onFrame) return Ink;
            if (x < margin || y < margin || x >= w - margin || y >= h - margin) return Ochre;
            if (y > h - margin - 56 && y < h - margin - 40) return Blue;
            return White;
        };

        // An abstract street grid over a paper field, no real place -- a generic "you are somewhere on this plan" look
        private static Pixel MapPlan(int w, int h) => (x, y) =>
        {
            if (x % 64 < 6 || y % 48 < 5 || Mathf.Abs((x - y * 1.3f) % 160f) < 8f) return Street;
            return Paper;
        };

        private static void WritePng(string path, int w, int h, Pixel pixel)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            var pixels = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[y * w + x] = pixel(x, y);
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void WriteReadme()
        {
            string readme = "Framework default media library (_3.1 step 13)\r\n"
                + "Every file in CardMedia/ here is generated by CardMediaLibraryGenerator.cs (Editor/DevTools) --\r\n"
                + "no third-party material, no licence question. Released CC0. Rerun 'TileStories/Dev/Regenerate\r\n"
                + "Default Media Library' from the Editor menu to rebuild all of it deterministically.\r\n";
            File.WriteAllText(Path.Combine(RootFolder, "CardMedia", "README.txt"), readme);
        }

        // ---------------- audio ----------------

        private const int SampleRate = 8000;

        private static float[] Chime()
        {
            const float seconds = 3f;
            var samples = new float[(int)(seconds * SampleRate)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)SampleRate;
                float beat = Mathf.Exp(-2.5f * t);
                samples[i] = 0.25f * beat * (Mathf.Sin(2 * Mathf.PI * 523f * t) + 0.5f * Mathf.Sin(2 * Mathf.PI * 784f * t));
            }
            return samples;
        }

        private static (float[] Samples, (float Start, float End, string Text)[] Cues) Ambient()
        {
            const float seconds = 10f;
            var samples = new float[(int)(seconds * SampleRate)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)SampleRate;
                samples[i] = 0.08f * (Mathf.Sin(2 * Mathf.PI * 110f * t) + 0.5f * Mathf.Sin(2 * Mathf.PI * 165f * t + 0.3f));
            }
            var cues = new[]
            {
                (0f, 5f, "Ambient tone, first half."),
                (5f, 10f, "Ambient tone, second half."),
            };
            return (samples, cues);
        }

        private static void WriteWav(string path, float[] samples)
        {
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            int byteRate = SampleRate * 2;
            int dataSize = samples.Length * 2;
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);           // PCM
            writer.Write((short)1);           // mono
            writer.Write(SampleRate);
            writer.Write(byteRate);
            writer.Write((short)2);           // block align
            writer.Write((short)16);          // bits per sample
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);
            foreach (float s in samples)
                writer.Write((short)(Mathf.Clamp(s, -1f, 1f) * 32767));
        }

        private static void WriteVtt(string path, (float Start, float End, string Text)[] cues)
        {
            var lines = new System.Text.StringBuilder("WEBVTT\r\n\r\n");
            for (int i = 0; i < cues.Length; i++)
            {
                lines.Append(i + 1).Append("\r\n");
                lines.Append(Stamp(cues[i].Start)).Append(" --> ").Append(Stamp(cues[i].End)).Append("\r\n");
                lines.Append(cues[i].Text).Append("\r\n\r\n");
            }
            File.WriteAllText(path, lines.ToString());
        }

        private static string Stamp(float seconds)
        {
            int whole = (int)seconds;
            int ms = (int)Mathf.Round((seconds - whole) * 1000f);
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}", whole / 3600, (whole / 60) % 60, whole % 60, ms);
        }

        // ---------------- video ----------------

        private static void WriteVideo(string path)
        {
            const int width = 192, height = 108, fps = 10;
            const float seconds = 4f;
            var attributes = new VideoTrackAttributes
            {
                frameRate = new MediaRational(fps),
                width = width,
                height = height,
                includeAlpha = false,
                bitRateMode = VideoBitrateMode.Low,
            };
            var frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            int frames = Mathf.CeilToInt(seconds * fps);
            using var encoder = new MediaEncoder(path, attributes);
            for (int f = 0; f < frames; f++)
            {
                int shift = f * 6; // the tile pattern slides sideways each frame -- "tile pattern motion"
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        pixels[y * width + x] = Tile(width, height, 36, Blue, Yellow, White)((x + shift) % width, y);
                frame.SetPixels32(pixels);
                frame.Apply();
                encoder.AddFrame(frame);
            }
            UnityEngine.Object.DestroyImmediate(frame);
        }

        // ---------------- the library asset ----------------

        private static void BuildLibrary()
        {
            var library = AssetDatabase.LoadAssetAtPath<CardMediaLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<CardMediaLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            void SetPicture(string key, string file, string note) =>
                library.Set(key, MediaKind.Image, AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(PicturesFolder, file)), note);
            void SetAudio(string key, string file, string note) =>
                library.Set(key, MediaKind.Audio, AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(AudioFolder, file)), note);
            void SetCaptions(string key, string file, string note) =>
                library.Set(key, MediaKind.Captions, AssetDatabase.LoadAssetAtPath<TextAsset>(Path.Combine(AudioFolder, file)), note);
            void SetVideo(string key, string file, string note) =>
                library.Set(key, MediaKind.Video, AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(Path.Combine(VideoFolder, file)), note);

            SetPicture("azulejo_blue", "azulejo_blue.png", "Blue-on-white tile pattern, generated");
            SetPicture("azulejo_ochre", "azulejo_ochre.png", "Ochre-on-blue tile pattern, generated");
            SetPicture("azulejo_detail", "azulejo_detail.png", "One tile motif, large -- a zoom_image / detail default");
            SetPicture("panel_plain", "panel_plain.png", "A plain bordered panel -- a generic surface placeholder");
            SetPicture("poster_plain", "poster_plain.png", "A plain portrait poster frame");
            SetPicture("map_plan", "map_plan.png", "An abstract street grid -- a generic today_map / wall_locator default");
            SetAudio("chime", "chime.wav", "A short two-note chime, 3 s, no captions (too short to caption)");
            SetAudio("ambient", "ambient.wav", "A 10 s ambient tone, with captions");
            SetCaptions("ambient", "ambient.vtt", "Two cues describing the ambient tone's halves");
            SetVideo("tile_pattern", "tile_pattern.mp4", "A sliding azulejo tile pattern, ~4 s, silent, low bitrate");

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
        }
    }
}
