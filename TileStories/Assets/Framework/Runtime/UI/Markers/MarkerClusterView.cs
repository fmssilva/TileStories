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
        private const float MinSizePx = 48f;   // >= 44px WCAG 2.5.5 tap target, headroom for the label
        private const float MaxSizePx = 112f;  // cap so very large clusters do not fill the screen
        private const float SizePerMember = 9f;
        private const string DefaultClusterMode = "pie_and_count";

        // CVD-safe neutral accent fallback (single hue, no red<->green encoding).
        private static readonly Color FallbackAccent = new Color(0.46f, 0.43f, 0.40f);

        [Header("References")]
        [SerializeField] private RectTransform pieContainer;
        [SerializeField] private TextMeshProUGUI countLabel;
        [SerializeField] private Image backgroundImage;

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
        }

        // Build the cluster visual from a group of member markers. Called on first
        // creation and on every reconcile cycle.
        public void Initialize(List<MarkerView> members, SpriteKeyLibrary iconLibrary, LodSettings settings)
        {
            EnsureWiring();
            _members = members ?? new List<MarkerView>();
            BuildCategoryCounts();
            BuildPie(iconLibrary, settings);
            UpdateCountLabel();
            ScaleByMemberCount();
        }

        // Refresh members + visuals when reusing a pooled view (member set drifted).
        public void Refresh(List<MarkerView> members, SpriteKeyLibrary iconLibrary, LodSettings settings)
        {
            _members = members ?? new List<MarkerView>();
            ClearSlices();
            BuildCategoryCounts();
            BuildPie(iconLibrary, settings);
            UpdateCountLabel();
            ScaleByMemberCount();
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
                slice.rectTransform.localRotation = Quaternion.Euler(0f, 0f, cumulative * 360f);
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

        private void ScaleByMemberCount()
        {
            int total = _members?.Count ?? 0;
            float size = Mathf.Clamp(MinSizePx + total * SizePerMember, MinSizePx, MaxSizePx);
            var rt = transform as RectTransform;
            if (rt != null) rt.sizeDelta = Vector2.one * size;
            if (pieContainer != null) pieContainer.sizeDelta = Vector2.one * size;
            if (countLabel != null) countLabel.fontSize = Mathf.Lerp(14f, 32f, (size - MinSizePx) / (MaxSizePx - MinSizePx));
            if (backgroundImage != null) backgroundImage.rectTransform.sizeDelta = Vector2.one * size;
        }

        private Color ResolveAccentColor()
        {
            // Decision 6: derive the single hue from the wall palette via the dominant
            // category, so cluster colour is wall-owned data, not hardcoded.
            if (_categoryCounts.Count > 0)
            {
                string topCategory = null;
                int topCount = -1;
                foreach (var kvp in _categoryCounts)
                {
                    if (kvp.Value > topCount) { topCount = kvp.Value; topCategory = kvp.Key; }
                }
                if (topCategory != null && topCategory != "uncategorized" &&
                    CategoryPalette.TryResolveConfigured(topCategory, out var color, out _))
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
