using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Aggregate cluster marker that replaces a group of individual POI markers when
    // density_response_mode is "cluster" or "hybrid" (spec §6.1). A cluster has no
    // single POI identity -- it carries the member list, a centroid world position, and
    // per-category counts. Keeping this a separate type (not a repurposed MarkerView)
    // avoids overloading MarkerView with a dual real-POI / cluster-aggregate job.
    //
    // Prefab structure on POI_Cluster.prefab (all children under the root GO, which
    // also carries the World-Space Canvas + CanvasGroup):
    //   - PieContainer (RectTransform)   <- runtime slice Images spawned here
    //   - CountLabel   (TextMeshProUGUI) <- "+N"
    //   - (optional) BackgroundImage (Image)
    //
    // Mirrors MarkerView's soft-transition fade exactly (same CanvasGroup smoothstep
    // coroutine) so clusters and individual markers fade in/out with identical timing.
    public class MarkerClusterView : MonoBehaviour
    {
        // A cluster lives on a WORLD-space canvas, like every marker: sizes are metres, not pixels.
        // Its diameter follows its largest member's symbol (x cluster_size_ratio), growing a little
        // with member count (GrowthPerMember, at most MaxGrowth) so "+12" reads bigger than "+2".
        private const float GrowthPerMember = 0.05f;
        private const float MaxGrowth = 0.5f;
        private const float MinDiameterMetres = 0.02f;   // never collapse to nothing (e.g. members without a symbol)
        private const float CountLabelFontRatio = 0.32f; // "+N" text height as a share of the diameter
        public const float MinSizeRatio = 1f;             // the Editor's Cluster size slider uses these limits
        public const float MaxSizeRatio = 4f;
        private const string DefaultClusterMode = "pie_and_count";

        // CVD-safe neutral accent fallback (single hue, no red<->green encoding).
        private static readonly Color FallbackAccent = new Color(0.46f, 0.43f, 0.40f);

        // The dark disc behind "+N": a donut hole over the pie, or the whole cluster in the other modes,
        // so the white count reads on any category colour.
        private static readonly Color DiscColor = new Color(0.12f, 0.12f, 0.14f, 0.92f);
        private const float PieHoleRatio = 0.58f;        // hole diameter as a share of the cluster
        private const float DominantIconRatio = 0.5f;    // dominant category icon, above the count

        [Header("References")]
        [SerializeField] private RectTransform pieContainer;
        [SerializeField] private TextMeshProUGUI countLabel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image dominantIcon;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;

        // Runtime state
        private List<MarkerView> _members;
        private HashSet<string> _memberPoiIds;
        private Vector3 _centroid;
        private readonly Dictionary<string, int> _categoryCounts = new();
        private readonly List<GameObject> _slices = new();

        // Exposed for LODController cluster lifecycle (reconciliation by member overlap).
        public IReadOnlyCollection<string> MemberPoiIds => _memberPoiIds;
        public Vector3 CentroidWorldPos => _centroid;
        public IReadOnlyDictionary<string, int> CategoryCounts => _categoryCounts;

        // Exposed for Phase A gallery rendering tests (spec §4.4).
        public Image DominantIcon => dominantIcon;

        private void Awake()
        {
            EnsureWiring();
        }

        // Lazy-resolve Canvas/CanvasGroup (mirrors MarkerView.EnsureMarkerWiring).
        private void EnsureWiring()
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            // World-Space Canvas needs an explicit event camera to raycast against.
            if (_canvas != null && _canvas.renderMode == RenderMode.WorldSpace && _canvas.worldCamera == null)
                _canvas.worldCamera = Camera.main;

            // Draw order (later siblings on top): pie, dark disc, dominant icon, then "+N" last --
            // whatever order the prefab lists them in (it once drew an untextured white background
            // over the pie and the count).
            if (pieContainer != null) pieContainer.SetAsLastSibling();
            if (backgroundImage != null)
            {
                backgroundImage.transform.SetAsLastSibling();
                backgroundImage.sprite = MarkerCircleSpriteFactory.GetFilled(1f);
                backgroundImage.color = DiscColor;
                backgroundImage.raycastTarget = false;
            }
            if (dominantIcon != null) dominantIcon.transform.SetAsLastSibling();
            if (countLabel != null)
            {
                countLabel.transform.SetAsLastSibling();
                countLabel.color = Color.white;
                countLabel.raycastTarget = false;
            }
        }

        // Build the cluster visual from a group of member markers. Called on first
        // creation and on every reconcile cycle.
        public void Initialize(List<MarkerView> members, SpriteKeyLibrary iconLibrary, LodSettings settings)
        {
            EnsureWiring();
            _members = members ?? new List<MarkerView>();
            BuildCategoryCounts();
            BuildPie(iconLibrary, settings);
            BuildDominantIcon(iconLibrary, settings);
            UpdateCountLabel();
            ApplySize(settings);
        }

        // Refresh members + visuals when reusing a pooled view (member set drifted).
        public void Refresh(List<MarkerView> members, SpriteKeyLibrary iconLibrary, LodSettings settings)
        {
            _members = members ?? new List<MarkerView>();
            ClearSlices();
            BuildCategoryCounts();
            BuildPie(iconLibrary, settings);
            BuildDominantIcon(iconLibrary, settings);
            UpdateCountLabel();
            ApplySize(settings);
        }

        // Current diameter in metres (world units of the cluster's canvas)
        public float DiameterMetres { get; private set; }

        // The cluster diameter rule, pure: largest member symbol x size ratio x a small count growth
        public static float ComputeDiameterMetres(float largestMemberDiameter, float sizeRatio, int memberCount)
        {
            float ratio = Mathf.Clamp(sizeRatio, MinSizeRatio, MaxSizeRatio);
            float growth = 1f + Mathf.Min(MaxGrowth, GrowthPerMember * Mathf.Max(0, memberCount - 2));
            return Mathf.Max(MinDiameterMetres, largestMemberDiameter * ratio * growth);
        }

        // Reposition the aggregate in AR space. centroid = world-space mean of members
        // (spec §6.1). Parented under the same root as individual markers so it lives
        // in the calibrated AR space.
        public void PositionAt(Vector3 worldPosition, Transform parent)
        {
            _centroid = worldPosition;
            transform.SetParent(parent, true);
            transform.position = worldPosition;
        }

        // --- rendering helpers (pure data, no lifecycle side-effects) ---

        private void BuildCategoryCounts()
        {
            _categoryCounts.Clear();
            _memberPoiIds = new HashSet<string>();
            if (_members == null) return;

            foreach (var m in _members)
            {
                if (m == null) continue;
                _memberPoiIds.Add(m.PoiId);
                string category = m.GetComponentInParent<POIAnchor>()?.Data?.category ?? string.Empty;
                string key = string.IsNullOrEmpty(category) ? "uncategorized" : category;
                _categoryCounts[key] = _categoryCounts.GetValueOrDefault(key) + 1;
            }
        }

        // pie_and_count (default): stacked Image.Filled + Radial360 slices, one per
        // category, sized by proportion, single-hue sequential ramp (Decision 6).
        // count_only: no pie, label only. dominant_category: icon + label.
        // Single source of truth for "which category wins" -- shared by the accent
        // colour (ResolveAccentColor, Decision 6) and the dominant_category icon so
        // the dominant member count is computed in exactly one place.
        private string GetDominantCategory()
        {
            string topCategory = null;
            int topCount = -1;
            foreach (var kvp in _categoryCounts)
            {
                if (kvp.Key == "uncategorized") continue;
                if (kvp.Value > topCount) { topCount = kvp.Value; topCategory = kvp.Key; }
            }
            return topCategory;
        }

        // Renders the single category icon used ONLY in "dominant_category" mode.
        // Inactive (hidden) in every other mode -- BuildPie handles pie/count_only and
        // early-returns for dominant_category, so this is its visual counterpart.
        private void BuildDominantIcon(SpriteKeyLibrary iconLibrary, LodSettings settings)
        {
            if (dominantIcon == null) return;
            bool isActive = (settings?.cluster_icon_mode ?? DefaultClusterMode) == "dominant_category";
            dominantIcon.gameObject.SetActive(isActive);
            if (!isActive) return;
            string category = GetDominantCategory();
            if (string.IsNullOrEmpty(category)) return;
            string iconKey = CategoryPalette.ResolveIconKey(category);
            dominantIcon.sprite = iconLibrary != null ? iconLibrary.Get(iconKey) : null;
        }

        private void BuildPie(SpriteKeyLibrary iconLibrary, LodSettings settings)
        {
            ClearSlices();
            string mode = settings?.cluster_icon_mode ?? DefaultClusterMode;
            int total = _members?.Count ?? 0;
            if (total <= 0) return;

            if (mode == "count_only") return; // label-only -- no pie geometry
            if (mode == "dominant_category") return; // single icon + label (simplified; no pie)

            Color accent = ResolveAccentColor();
            float hue = 0f, sat = 0f, val = 0f;
            Color.RGBToHSV(accent, out hue, out sat, out val);

            // Deterministic order (count desc, then key) so slices do not reshuffle
            // between reconcile cycles for the same group.
            var entries = new List<KeyValuePair<string, int>>(_categoryCounts);
            entries.Sort((a, b) =>
            {
                int c = b.Value.CompareTo(a.Value);
                return c != 0 ? c : string.CompareOrdinal(a.Key, b.Key);
            });

            float cumulative = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                var slice = CreateSlice();
                slice.type = Image.Type.Filled;
                slice.fillMethod = Image.FillMethod.Radial360;
                slice.fillOrigin = 0; // Bottom (3 o'clock); RectTransform rotation advances subsequent slices
                slice.fillClockwise = true;
                slice.fillAmount = (float)entries[i].Value / total;
                // - the slice fills CLOCKWISE, so the next one starts rotated clockwise (negative z in
                //   Unity) by everything before it; a positive angle left a wedge-shaped gap
                slice.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -cumulative * 360f);
                float t = entries.Count > 1 ? (float)i / (entries.Count - 1) : 1f;
                slice.color = Color.HSVToRGB(hue, sat, Mathf.Lerp(0.45f, 1f, t));
                cumulative += slice.fillAmount;
            }
        }

        private Image CreateSlice()
        {
            // Base circle sprite comes from the shared factory (caching + domain-reload
            // safety already solved there); Image.Filled carves the radial slice out of it.
            var go = new GameObject("cluster_slice", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(pieContainer, false);
            // - stretch over the pie container (a new RectTransform defaults to 100x100 units = 100 m here)
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = MarkerCircleSpriteFactory.GetFilled(1f);
            _slices.Add(go);
            return img;
        }

        private void ClearSlices()
        {
            foreach (var s in _slices)
                if (s != null) Destroy(s);
            _slices.Clear();
        }

        private void UpdateCountLabel()
        {
            if (countLabel == null) return;
            int total = _members?.Count ?? 0;
            countLabel.text = "+" + total;
        }

        // Size every part of the aggregate from the largest member's symbol (ComputeDiameterMetres)
        private void ApplySize(LodSettings settings)
        {
            float largest = 0f;
            if (_members != null)
                foreach (var m in _members)
                    if (m != null && m.SymbolDiameterMetres > largest) largest = m.SymbolDiameterMetres;

            float ratio = settings != null ? settings.cluster_size_ratio : new LodSettings().cluster_size_ratio;
            float size = ComputeDiameterMetres(largest, ratio, _members?.Count ?? 0);
            DiameterMetres = size;

            string mode = settings?.cluster_icon_mode ?? DefaultClusterMode;
            bool pie = mode != "count_only" && mode != "dominant_category";
            bool dominant = mode == "dominant_category";

            var rt = transform as RectTransform;
            if (rt != null) rt.sizeDelta = Vector2.one * size;
            if (pieContainer != null) pieContainer.sizeDelta = Vector2.one * size;
            // - pie: the dark disc is the donut hole; otherwise it IS the cluster
            if (backgroundImage != null) backgroundImage.rectTransform.sizeDelta = Vector2.one * (pie ? size * PieHoleRatio : size);
            if (dominantIcon != null)
            {
                dominantIcon.rectTransform.sizeDelta = Vector2.one * (size * DominantIconRatio);
                dominantIcon.rectTransform.anchoredPosition = new Vector2(0f, size * 0.14f);
            }
            if (countLabel != null)
            {
                var labelRect = countLabel.rectTransform;
                labelRect.sizeDelta = new Vector2(size, dominant ? size * 0.36f : size);
                labelRect.anchoredPosition = new Vector2(0f, dominant ? -size * 0.24f : 0f);
                countLabel.alignment = TextAlignmentOptions.Center;
                countLabel.enableWordWrapping = false;
                countLabel.fontSize = size * (dominant ? CountLabelFontRatio * 0.7f : CountLabelFontRatio);
            }
        }

        private Color ResolveAccentColor()
        {
            // Decision 6: derive the single hue from the wall palette via the dominant
            // category, so cluster colour is wall-owned data, not hardcoded.
            if (_categoryCounts.Count > 0)
            {
                string topCategory = GetDominantCategory();
                if (topCategory != null && CategoryPalette.TryResolveConfigured(topCategory, out var color, out _))
                    return color;
            }
            return FallbackAccent;
        }


        // --- visibility (shared CanvasGroup soft-transition, identical to MarkerView) ---

        // Instantly toggle cluster visibility via the shared CanvasGroup.
        public void SetVisible(bool visible)
        {
            EnsureWiring();
            if (_canvasGroup == null) return;
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        // Fade cluster visibility over fadeDuration (spec §4 step 6 / §7).
        public void SetVisible(bool visible, float fadeDuration)
        {
            if (!Application.isPlaying || fadeDuration <= 0f)
            {
                SetVisible(visible);
                return;
            }
            EnsureWiring();
            if (_canvasGroup == null) return;
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCoroutine(visible, fadeDuration));
        }

        // Crossfade CanvasGroup alpha from current to target; flip interaction flags at
        // the endpoints so raycasts resume only when fully visible. Identical shape to
        // MarkerView.FadeCoroutine so the two fades are indistinguishable to the visitor.
        private IEnumerator FadeCoroutine(bool fadeIn, float duration)
        {
            float startAlpha = _canvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;
            float elapsed = 0f;

            if (fadeIn)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smooth = t * t * (3f - 2f * t); // smoothstep
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smooth);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _fadeCoroutine = null;

            if (!fadeIn)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
