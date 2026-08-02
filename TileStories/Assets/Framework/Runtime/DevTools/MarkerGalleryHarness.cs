using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace TileStories
{
    // Play Mode only, lives in MarkerGalleryScene.unity (never added to Build
    // Settings). Spawns every MarkerGalleryDefinitions.Entries through the exact
    // same POI_Marker prefab and MarkerView.Initialise call WallSession uses --
    // no config.json, no WallConfigLoader, no tracking.
    public class MarkerGalleryHarness : MonoBehaviour
    {
        [SerializeField] private GameObject poiMarkerPrefab;
        [SerializeField] private TextMeshProUGUI groupLabelPrefab; // optional, world-space TMP text
        [SerializeField] private float columnSpacing = 0.25f;
        [SerializeField] private float heroColumnSpacing = 0.62f;
        [SerializeField] private float rowSpacing = 0.45f;
        [SerializeField] private bool enableDeepDiagnostics = true;
        [SerializeField] private bool forceDiagnosticsAllEntries = true;
        [SerializeField] private bool diagnosticsOnlyFocusedEntry = false;
        [SerializeField] private string diagnosticsFocusLabel = "Single circle, civic, no status";

        // Spawn all configured marker variants and emit a compact summary for each.
        private void Start()
        {
            CategoryPalette.Configure(MarkerGalleryDefinitions.Overrides);

            Debug.Log($"[MarkerGalleryDiag] mode enabled={enableDeepDiagnostics} forceAll={forceDiagnosticsAllEntries} focusedOnly={diagnosticsOnlyFocusedEntry} focusLabel='{diagnosticsFocusLabel}' entries={MarkerGalleryDefinitions.Entries.Count}");

            string currentGroup = null;
            int row = -1;
            int col = 0;

            foreach (var entry in MarkerGalleryDefinitions.Entries)
            {
                if (entry.Group != currentGroup)
                {
                    currentGroup = entry.Group;
                    row++;
                    col = 0;
                    if (groupLabelPrefab != null)
                    {
                        var groupText = Instantiate(groupLabelPrefab, transform);
                        groupText.text = currentGroup;
                        groupText.transform.localPosition = new Vector3(-0.3f, -row * rowSpacing, 0f);
                    }
                }

                float spacing = ResolveColumnSpacing(entry.Group);
                SpawnEntry(entry, row, col, spacing);
                col++;
            }

            Debug.Log($"[MarkerGallery] Spawned {MarkerGalleryDefinitions.Entries.Count} entries across {row + 1} groups.");
        }

        // Instantiate one entry, initialize the real marker path, and log diagnostics.
        private void SpawnEntry(MarkerGalleryEntry entry, int row, int col, float spacing)
        {
            var go = Instantiate(poiMarkerPrefab, transform);
            go.transform.localPosition = new Vector3(col * spacing, -row * rowSpacing, 0f);
            go.name = $"Gallery_{entry.Group}_{entry.Label}";

            var poiData = new POIData
            {
                id = go.name,
                name = entry.Label,
                category = entry.Category,
                status_pct = entry.StatusPct,
                has_status = entry.HasStatus,
                status_unknown = entry.StatusUnknown,
                is_hero = entry.IsHero,
                rotate_contour = entry.RotateContour,
                has_captured_position = true,
            };

            var anchor = go.AddComponent<POIAnchor>();
            anchor.Initialise(poiData);

            var markerView = go.GetComponentInChildren<MarkerView>();
            markerView.Initialise(anchor, entry.Style, entry.Shape, entry.EffectFlags);

            LogResult(entry, go);
            if (ShouldRunDeepDiagnostics(entry))
                LogDeepDiagnostics(entry, go);
        }

        private float ResolveColumnSpacing(string group)
        {
            if (!string.IsNullOrEmpty(group) && group.StartsWith("Hero Effects"))
                return heroColumnSpacing;

            return columnSpacing;
        }

        // Structured, greppable, MCP-queryable -- one line per marker, every
        // field an agent needs to verify correctness without seeing anything.
        private void LogResult(MarkerGalleryEntry entry, GameObject go)
        {
            var images = go.GetComponentsInChildren<UnityEngine.UI.Image>(includeInactive: true);
            var parts = new List<string>();
            foreach (var img in images)
            {
                parts.Add($"{img.gameObject.name}(active={img.gameObject.activeInHierarchy}," +
                          $"enabled={img.enabled},sprite={(img.sprite != null ? img.sprite.name : "NULL")}," +
                          $"color=#{ColorUtility.ToHtmlStringRGBA(img.color)})");
            }
            Debug.Log($"[MarkerGallery] {entry.Group} | {entry.Label} || " + string.Join(" ", parts));
        }

        // Decide whether this entry should emit the full deep diagnostic block.
        private bool ShouldRunDeepDiagnostics(MarkerGalleryEntry entry)
        {
            if (!enableDeepDiagnostics)
                return false;

            if (forceDiagnosticsAllEntries)
                return true;

            if (!diagnosticsOnlyFocusedEntry)
                return true;

            if (string.IsNullOrEmpty(diagnosticsFocusLabel))
                return false;

            return entry.Label == diagnosticsFocusLabel;
        }

        // Emit deep diagnostics for each renderable child and assert core invariants.
        private void LogDeepDiagnostics(MarkerGalleryEntry entry, GameObject go)
        {
            var marker = go.GetComponentInChildren<MarkerView>(includeInactive: true);
            Debug.Assert(marker != null, "[MarkerGalleryDiag] MarkerView missing after spawn.");

            var symbol = go.transform.Find("Symbol")?.GetComponent<Image>();
            var symbolIcon = go.transform.Find("Symbol/Icon")?.GetComponent<Image>();
            var ring = go.transform.Find("Ring")?.GetComponent<Image>();
            var badge = go.transform.Find("Badge")?.GetComponent<Image>();
            var badgeIcon = go.transform.Find("Badge/Icon")?.GetComponent<Image>();
            var halo = go.transform.Find("Halo")?.GetComponent<Image>();
            var sunInner = go.transform.Find("SunInner")?.GetComponent<Image>();
            var sunMiddle = go.transform.Find("SunMiddle")?.GetComponent<Image>();
            var sunOuter = go.transform.Find("SunOuter")?.GetComponent<Image>();

            var pulseFx = go.GetComponent<MarkerPulseEffect>();
            var glowFx = go.GetComponent<MarkerGlowEffect>();
            var sunFx = go.GetComponent<MarkerSunEffect>();

            LogImageDiagnostics(entry, "Symbol", symbol);
            LogImageDiagnostics(entry, "Symbol/Icon", symbolIcon);
            LogImageDiagnostics(entry, "Ring", ring);
            LogImageDiagnostics(entry, "Badge", badge);
            LogImageDiagnostics(entry, "Badge/Icon", badgeIcon);
            LogImageDiagnostics(entry, "Halo", halo);
            LogImageDiagnostics(entry, "SunInner", sunInner);
            LogImageDiagnostics(entry, "SunMiddle", sunMiddle);
            LogImageDiagnostics(entry, "SunOuter", sunOuter);

            bool isUnknown = entry.HasStatus && entry.StatusUnknown;
            bool expectRing = entry.HasStatus && !isUnknown && entry.Style != MarkerStyle.Badge;
            bool expectBadge = entry.HasStatus && (isUnknown || entry.Style == MarkerStyle.Badge);

            bool hasSymbolSprite = symbol != null && symbol.enabled && symbol.sprite != null;
            bool ringActive = ring != null && ring.enabled;
            bool badgeActive = badge != null && badge.gameObject.activeSelf;
            bool heroPulseExpected = HasEffect(entry.EffectFlags, MarkerEffectFlags.Pulse);
            bool heroSunExpected = HasEffect(entry.EffectFlags, MarkerEffectFlags.SunContours) || HasEffect(entry.EffectFlags, MarkerEffectFlags.SunCircles);
            bool sunVisible = sunFx != null && sunFx.IsActive;

            Debug.Assert(hasSymbolSprite,
                $"[MarkerGalleryDiag][FAIL] {entry.Label}: Symbol should have a non-null sprite.");
            Debug.Assert(ringActive == expectRing,
                $"[MarkerGalleryDiag][FAIL] {entry.Label}: Ring enabled={ringActive} expected={expectRing}.");
            Debug.Assert(badgeActive == expectBadge,
                $"[MarkerGalleryDiag][FAIL] {entry.Label}: Badge active={badgeActive} expected={expectBadge}.");

            Debug.Log(
                $"[MarkerGalleryDiag][FX] {entry.Label} | pulse={(pulseFx != null ? pulseFx.IsActive : false)} " +
                $"glow={(glowFx != null ? glowFx.IsActive : false)} sun={(sunFx != null ? sunFx.IsActive : false)} " +
                $"sunStyle={(sunFx != null ? sunFx.CurrentStyle.ToString() : "n/a")} " +
                $"sunExpected={heroSunExpected} pulseExpected={heroPulseExpected} " +
                $"sunInner={(sunInner != null && sunInner.enabled)} sunMiddle={(sunMiddle != null && sunMiddle.enabled)} sunOuter={(sunOuter != null && sunOuter.enabled)}");

            if (heroSunExpected)
            {
                AssertSunLayerLooksCircular(entry, "SunInner", sunInner);
                AssertSunLayerLooksCircular(entry, "SunMiddle", sunMiddle);
                AssertSunLayerLooksCircular(entry, "SunOuter", sunOuter);
            }

            Debug.Log(
                $"[MarkerGalleryDiag][SUMMARY] {entry.Group} | {entry.Label} | style={entry.Style} shape={entry.Shape} " +
                $"hasStatus={entry.HasStatus} unknown={entry.StatusUnknown} hero={entry.IsHero} " +
                $"symbolOk={hasSymbolSprite} ringOk={ringActive == expectRing} badgeOk={badgeActive == expectBadge}");
        }

        // Log one line of diagnostic state for a single Image component.
        private void LogImageDiagnostics(MarkerGalleryEntry entry, string key, Image image)
        {
            if (image == null)
            {
                Debug.LogWarning($"[MarkerGalleryDiag][MISSING] {entry.Label} | {key} image component is null.");
                return;
            }

            var rect = image.rectTransform;
            string spriteName = image.sprite != null ? image.sprite.name : "NULL";
            string color = "#" + ColorUtility.ToHtmlStringRGBA(image.color);
            string active = image.gameObject.activeSelf + "/" + image.gameObject.activeInHierarchy;

            string alphaCoverage = "n/a";
            bool looksLikeOpaqueSquare = false;
            if (image.sprite != null && TryGetSpriteAlphaCoverage(image.sprite, out float coverage, out string spritePath))
            {
                alphaCoverage = coverage.ToString("P1") + " @ " + spritePath;
                looksLikeOpaqueSquare = coverage > 0.95f;
            }

            string textureInfo = "n/a";
            string screenInfo = "n/a";
            string samplingInfo = "n/a";
            bool undersampled = false;
            if (image.sprite != null)
            {
                int texW = image.sprite.texture != null ? image.sprite.texture.width : 0;
                int texH = image.sprite.texture != null ? image.sprite.texture.height : 0;
                textureInfo = texW > 0 && texH > 0 ? texW + "x" + texH : "unknown";

                if (TryGetScreenSizePixels(image.rectTransform, out float screenW, out float screenH))
                {
                    screenInfo = screenW.ToString("F1") + "x" + screenH.ToString("F1") + "px";
                    if (texW > 0 && texH > 0 && screenW > 0.01f && screenH > 0.01f)
                    {
                        float sx = texW / screenW;
                        float sy = texH / screenH;
                        float minScale = Mathf.Min(sx, sy);
                        samplingInfo = "tex/screen=" + sx.ToString("F2") + "," + sy.ToString("F2");
                        undersampled = minScale < 1f;
                    }
                }
            }

            Debug.Log(
                $"[MarkerGalleryDiag][IMAGE] {entry.Label} | {key} | active={active} enabled={image.enabled} " +
                $"sprite={spriteName} color={color} rect=({rect.sizeDelta.x:F4},{rect.sizeDelta.y:F4}) " +
                $"anchored=({rect.anchoredPosition.x:F4},{rect.anchoredPosition.y:F4}) alphaCoverage={alphaCoverage} " +
                $"tex={textureInfo} screen={screenInfo} {samplingInfo}");

            bool isIconKey = key.EndsWith("/Icon") || key == "Icon";

            if (looksLikeOpaqueSquare && isIconKey)
            {
                Debug.LogWarning(
                    $"[MarkerGalleryDiag][SUSPECT] {entry.Label} | {key} uses a near-fully-opaque sprite. " +
                    "If this is an icon, this often renders as a flat square overlay.");
            }

            if (looksLikeOpaqueSquare && !isIconKey)
            {
                Debug.LogWarning(
                    $"[MarkerGalleryDiag][SUSPECT] {entry.Label} | {key} uses a near-fully-opaque sprite. " +
                    "Verify this component is intentionally filled and not masking other marker layers.");
            }

            if (undersampled)
            {
                Debug.LogWarning(
                    $"[MarkerGalleryDiag][SUSPECT] {entry.Label} | {key} texture is smaller than on-screen size. " +
                    "This commonly appears blurry; use a larger source texture.");
            }
        }

        private static bool HasEffect(MarkerEffectFlags mask, MarkerEffectFlags effect)
        {
            return (mask & effect) != 0;
        }

        private static void AssertSunLayerLooksCircular(MarkerGalleryEntry entry, string key, Image image)
        {
            Debug.Assert(image != null,
                $"[MarkerGalleryDiag][FAIL] {entry.Label}: {key} image missing for expected sun effect.");
            if (image == null)
                return;

            Debug.Assert(image.sprite != null,
                $"[MarkerGalleryDiag][FAIL] {entry.Label}: {key} sprite is NULL (this renders as a square UI quad).");
            if (image.sprite == null || image.sprite.texture == null)
                return;

            var texture = image.sprite.texture;
            float tl = texture.GetPixel(0, 0).a;
            float tr = texture.GetPixel(texture.width - 1, 0).a;
            float bl = texture.GetPixel(0, texture.height - 1).a;
            float br = texture.GetPixel(texture.width - 1, texture.height - 1).a;

            Debug.Log($"[MarkerGalleryDiag][SUNCHECK] {entry.Label} | {key} | cornerAlpha tl={tl:F3} tr={tr:F3} bl={bl:F3} br={br:F3}");

            bool cornersTransparent = tl < 0.05f && tr < 0.05f && bl < 0.05f && br < 0.05f;
            Debug.Assert(cornersTransparent,
                $"[MarkerGalleryDiag][FAIL] {entry.Label}: {key} sprite corners are not transparent (looks square, not circular).");
        }

        // Estimate how large this UI element appears in screen pixels in world-space canvas mode.
        private static bool TryGetScreenSizePixels(RectTransform rectTransform, out float width, out float height)
        {
            width = 0f;
            height = 0f;

            if (rectTransform == null || Camera.main == null)
                return false;

            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Vector3 bl = Camera.main.WorldToScreenPoint(corners[0]);
            Vector3 tr = Camera.main.WorldToScreenPoint(corners[2]);
            width = Mathf.Abs(tr.x - bl.x);
            height = Mathf.Abs(tr.y - bl.y);
            return true;
        }

#if UNITY_EDITOR
        // Compute alpha coverage from the PNG bytes on disk (independent of texture import read/write flags).
        private static bool TryGetSpriteAlphaCoverage(Sprite sprite, out float coverage, out string assetPath)
        {
            coverage = 0f;
            assetPath = "";

            if (sprite == null)
                return false;

            assetPath = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".png"))
                return false;

            string fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
                return false;

            byte[] pngBytes = File.ReadAllBytes(fullPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            bool ok = tex.LoadImage(pngBytes, markNonReadable: false);
            if (!ok)
            {
                Object.Destroy(tex);
                return false;
            }

            Color32[] pixels = tex.GetPixels32();
            int opaque = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0)
                    opaque++;
            }

            coverage = pixels.Length > 0 ? opaque / (float)pixels.Length : 0f;
            Object.Destroy(tex);
            return true;
        }
#else
        private static bool TryGetSpriteAlphaCoverage(Sprite sprite, out float coverage, out string assetPath)
        {
            coverage = 0f;
            assetPath = "";
            return false;
        }
#endif
    }
}