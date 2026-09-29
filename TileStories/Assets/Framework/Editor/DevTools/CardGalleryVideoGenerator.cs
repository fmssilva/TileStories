using System.IO;
using UnityEditor;
using UnityEditor.Media;
using UnityEngine;

namespace TileStories.Editor
{
    // Writes the Phase A card gallery's video clips (_3.1 step 9B): Unity cannot make a VideoClip in memory the way the gallery makes its
    // pictures and silent audio (CardGalleryMedia), so the gallery reads these small files instead. Generated here with Unity's own
    // MediaEncoder (no external tool): 160x90, 10 fps, no sound, a dark frame with a light bar that crosses it once per second, so a real
    // player shows motion. Run GenerateAll once (execute it from a script or the Editor console); the files are checked in.
    public static class CardGalleryVideoGenerator
    {
        public const string Folder = CardGalleryMedia.VideoFolder;
        private const int Width = 160;
        private const int Height = 90;
        private const int Fps = 10;

        // Write every clip CardGalleryDefinitions names, then import them
        public static void GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            foreach (var pair in CardGalleryDefinitions.Videos)
                Write(Path.Combine(Folder, pair.Value.File), pair.Value.Seconds);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void Write(string path, float seconds)
        {
            var attributes = new VideoTrackAttributes
            {
                frameRate = new MediaRational(Fps),
                width = Width,
                height = Height,
                includeAlpha = false,
                bitRateMode = VideoBitrateMode.Low,
            };
            var frame = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            var pixels = new Color32[Width * Height];
            var ground = new Color32(40, 44, 52, 255);
            var bar = new Color32(200, 200, 200, 255);
            int frames = Mathf.CeilToInt(seconds * Fps);
            using (var encoder = new MediaEncoder(path, attributes))
            {
                for (int f = 0; f < frames; f++)
                {
                    int barX = (f % Fps) * Width / Fps;
                    for (int y = 0; y < Height; y++)
                        for (int x = 0; x < Width; x++)
                            pixels[y * Width + x] = x >= barX && x < barX + Width / Fps ? bar : ground;
                    frame.SetPixels32(pixels);
                    frame.Apply();
                    encoder.AddFrame(frame);
                }
            }
            Object.DestroyImmediate(frame);
        }
    }
}
