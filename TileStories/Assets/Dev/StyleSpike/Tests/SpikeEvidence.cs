using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TileStories.Tests;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0): the spike's captures and pixel reads. Every PNG goes to TestEvidence/StyleSpike/ (40-testing 4.2.4).
    public static class SpikeEvidence
    {
        public const string Domain = "StyleSpike";

        // Grab the finished frame (the caller destroys the texture). Pixels are display-encoded sRGB, what the eye gets.
        public static IEnumerator Grab(System.Action<Texture2D> got)
        {
            yield return new WaitForEndOfFrame();
            got(ScreenCapture.CaptureScreenshotAsTexture());
        }

        // Save a texture as TestEvidence/StyleSpike/<name>.png and return its path
        public static string Save(Texture2D tex, string name)
        {
            string path = TestEvidence.PathFor(Domain, name + ".png");
            File.WriteAllBytes(path, tex.EncodeToPNG());
            return path;
        }

        // Grab the frame and save it in one go
        public static IEnumerator Capture(string name)
        {
            Texture2D tex = null;
            yield return Grab(t => tex = t);
            try { Save(tex, name); }
            finally { Object.Destroy(tex); }
        }

        // The screen pixel (texture coordinates, y up) under a panel point (y down)
        public static Vector2Int PanelToPixel(VisualElement inPanel, Vector2 panelPoint, Texture2D tex)
        {
            var origin = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.zero);
            float unit = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.right).x - origin.x;
            var screen = (panelPoint - origin) / unit;
            return new Vector2Int(Mathf.RoundToInt(screen.x), tex.height - 1 - Mathf.RoundToInt(screen.y));
        }

        // The pixel colour under a panel point
        public static Color PixelAt(VisualElement inPanel, Vector2 panelPoint, Texture2D tex)
        {
            var p = PanelToPixel(inPanel, panelPoint, tex);
            return tex.GetPixel(p.x, p.y);
        }

        // Paste several equally sized frames side by side into one picture (the vision pass reads one image, not six)
        public static Texture2D SideBySide(IReadOnlyList<Texture2D> frames, int gap = 8)
        {
            Assert.IsTrue(frames.Count > 0, "nothing to paste");
            int w = frames[0].width, h = frames[0].height;
            var sheet = new Texture2D(w * frames.Count + gap * (frames.Count - 1), h, TextureFormat.RGBA32, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++) fill[i] = new Color32(128, 128, 128, 255);
            sheet.SetPixels32(fill);
            for (int i = 0; i < frames.Count; i++)
                sheet.SetPixels(i * (w + gap), 0, w, h, frames[i].GetPixels());
            sheet.Apply();
            return sheet;
        }

        // Cut the part of a frame under a panel rect (a frame sequence keeps only the moving thing)
        public static Texture2D Crop(VisualElement inPanel, Rect panelRect, Texture2D tex)
        {
            var a = PanelToPixel(inPanel, new Vector2(panelRect.xMin, panelRect.yMax), tex);   // bottom-left in texture space
            int w = Mathf.RoundToInt(panelRect.width), h = Mathf.RoundToInt(panelRect.height);
            int x = Mathf.Clamp(a.x, 0, tex.width - w), y = Mathf.Clamp(a.y, 0, tex.height - h);
            var cut = new Texture2D(w, h, TextureFormat.RGBA32, false);
            cut.SetPixels(tex.GetPixels(x, y, w, h));
            cut.Apply();
            return cut;
        }

        // A smaller copy (bilinear) so a long frame sequence fits one readable picture
        public static Texture2D Scaled(Texture2D tex, float factor)
        {
            int w = Mathf.Max(1, Mathf.RoundToInt(tex.width * factor)), h = Mathf.Max(1, Mathf.RoundToInt(tex.height * factor));
            var small = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    small.SetPixel(x, y, tex.GetPixelBilinear((x + 0.5f) / w, (y + 0.5f) / h));
            small.Apply();
            return small;
        }

        // A one-line measurement for the run log and the R8 table, prefixed so it is easy to find in the Editor log
        public static void Note(string spike, string line) => Debug.Log($"[StyleSpike] {spike}: {line}");
    }
}
