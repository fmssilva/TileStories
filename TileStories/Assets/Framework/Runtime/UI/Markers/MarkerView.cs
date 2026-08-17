using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Visual representation of a POI marker in AR space (uGUI World Space Canvas prefab).
    // Shows a truncated label and handles basic tap detection.
    //
    // The prefab structure expected (all children of the root MarkerView transform):
    //   - Symbol (Image)                         -> MarkerCircleGlyphView
    //   - Ring (Image)                           -> MarkerRingView
    //   - Badge (Image)                          -> MarkerCircleGlyphView
    //   - Label (TextMeshPro - World Space)
    //   - PulseEffect (optional)                 -> MarkerPulseEffect
    //   - GlowEffect (optional)                  -> MarkerGlowEffect
    //   - SunEffect (optional)                   -> MarkerSunEffect
    //   - AccentEffect (optional)                -> MarkerAccentEffect
    //
    // All effect components are optional -- a prefab without them simply never
    // animates. This keeps the base marker cheap.
    public class MarkerView : MonoBehaviour
    {
        private const string FallbackUnknownIconKey = "unknown";
        private const string FallbackUnknownBadgeCategoryKey = "unknown_damage";
        private const string FallbackUnknownStatusLevelKey = "unknown";

        private static readonly Color IconTint = new Color(0.949f, 0.925f, 0.827f);
        private static readonly Color HaloTint = new Color(0.949f, 0.925f, 0.827f, 0.2f);

        [Header("References")]
        [SerializeField] private MarkerCircleGlyphView symbol;
        [SerializeField] private MarkerRingView ring;
        [SerializeField] private MarkerCircleGlyphView badge;
        [SerializeField] private TextMeshProUGUI labelText;

        [Header("Layout")]
        [SerializeField] private MarkerLayoutProportions layout = new();

        [Header("Effects (optional)")]
        [SerializeField] private MarkerPulseEffect pulseEffect;
        [SerializeField] private MarkerGlowEffect glowEffect;
        [SerializeField] private MarkerSunEffect sunEffect;
        [SerializeField] private MarkerAccentEffect accentEffect;
        [SerializeField] private MarkerEffectFlags effectFlags = MarkerEffectFlags.None;

                [Header("External assets")]
        [SerializeField] private SpriteKeyLibrary shapeLibrary;
        [SerializeField] private SpriteKeyLibrary iconLibrary;

        // Shared with MarkerRevealEffect -- auto-resolved in EnsureMarkerWiring.
        // SetVisible toggles this same CanvasGroup so LOD and reveal cannot fight
        // over alpha values.
        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;

        // Runtime state
        private POIAnchor _anchor;
        private Vector3 _baseLocalPosition;
        private bool _hasBasePosition;
        private MarkerOutlineMode _outlineMode = MarkerOutlineMode.Gold;
        private bool _useBadge;
        private MarkerShape _shape;
        private MarkerShape _badgeShape = MarkerShape.Circle;
        private SpriteKeyLibrary _runtimeIconLibraryOverride;
        private EffectDefaults _effectDefaults;
        private bool _applyCategoryVisuals = true;
        private bool _applyShapeVisuals = true;
        private bool _enableStatusVisuals = true;
        private Vector2 _baseLabelSize;
        private bool _hasBaseLabelSize;
        private HierarchyStyle _hierarchyStyle;
        private bool _hasHierarchy;
        private bool _baseLabelWordWrapping;
        private TextOverflowModes _baseLabelOverflowMode;
        private TextAlignmentOptions _baseLabelAlignment;
        private Vector2 _baseLabelAnchorMin;
        private Vector2 _baseLabelAnchorMax;
        private Vector2 _baseLabelPivot;

        // Expose the POI id for deterministic sorting in overlap resolution
        public string PoiId { get; private set; }

        // Expose the hierarchy level key for priority-based LOD count cap sorting.
        // Same source as the _hierarchyStyle resolution in ApplyVisuals.
        public string HierarchyLevelKey => _anchor?.Data?.hierarchy_level_key ?? string.Empty;

        private void OnValidate()
        {
            EnsureMarkerWiring(allowCreate: false);
            ApplyLayout();
        }

        // Initialise is called once by WallSession after spawn.
        // The style/shape parameters come from the wall config (parsed by
        // MarkerVisualsParser) and are passed in here rather than read
        // directly, so the marker can be instantiated in the Editor without
        // a config present.
        public void Initialise(POIAnchor anchor, MarkerStyle style, MarkerShape shape)
        {
            Initialise(anchor, style, shape, MarkerEffectFlags.None);
        }

        public void Initialise(POIAnchor anchor, MarkerStyle style, MarkerShape shape, MarkerEffectFlags effects)
        {
            MarkerVisualsParser.DeriveOutlineAndBadgeFromLegacyStyle(style.ToString() switch
            {
                nameof(MarkerStyle.Badge) => "badge",
                nameof(MarkerStyle.OutlineSameHue) => "outline_same_hue",
                _ => "outline_gold",
            }, out var outlineMode, out var useBadge);

            Initialise(anchor, outlineMode, useBadge, shape, effects);
        }

        public void Initialise(POIAnchor anchor, MarkerOutlineMode outlineMode, bool useBadge, MarkerShape shape, MarkerEffectFlags effects)
        {
            Initialise(anchor, outlineMode, useBadge, shape, effects, true, true, true, null, MarkerShape.Circle);
        }

        public void Initialise(
            POIAnchor anchor,
            MarkerOutlineMode outlineMode,
            bool useBadge,
            MarkerShape shape,
            MarkerEffectFlags effects,
            bool applyCategoryVisuals,
            bool applyShapeVisuals,
            bool enableStatusVisuals)
        {
            Initialise(anchor, outlineMode, useBadge, shape, effects, applyCategoryVisuals, applyShapeVisuals, enableStatusVisuals, null, MarkerShape.Circle);
        }

        public void Initialise(
            POIAnchor anchor,
            MarkerOutlineMode outlineMode,
            bool useBadge,
            MarkerShape shape,
            MarkerEffectFlags effects,
            bool applyCategoryVisuals,
            bool applyShapeVisuals,
            bool enableStatusVisuals,
            SpriteKeyLibrary iconLibraryOverride,
            MarkerShape badgeShape = MarkerShape.Circle,
            EffectDefaults effectDefaults = null)
        {
            EnsureMarkerWiring(allowCreate: true);
            _anchor = anchor;
            _outlineMode = outlineMode;
            _useBadge = useBadge;
            _shape = shape;
            _badgeShape = badgeShape;
            _runtimeIconLibraryOverride = iconLibraryOverride;
            _effectDefaults = effectDefaults;
            _applyCategoryVisuals = applyCategoryVisuals;
            _applyShapeVisuals = applyShapeVisuals;
            _enableStatusVisuals = enableStatusVisuals;
            effectFlags = effects;
            PoiId = anchor?.Data?.id ?? string.Empty;

            ApplyVisuals();
            ApplyLabelState();

            var reveal = GetComponent<MarkerRevealEffect>();
                        reveal?.Play(_hierarchyStyle.RevealDelaySeconds, _hierarchyStyle.RevealDurationSeconds);
        }

        private void ApplyVisuals()
        {
            if (_anchor?.Data == null) return;

            var poi = _anchor.Data;

            // Resolve hierarchy level to get size/label/effects/reveal-delay.
            // Falls back to MarkerHierarchyResolver.Fallback when no hierarchy_level_key
            // is set or the resolver has not been configured -- matches the empty-state
            // behavior of CategoryPalette/StatusRamp.
            _hasHierarchy = MarkerHierarchyResolver.TryResolveByKey(poi.hierarchy_level_key, out _hierarchyStyle);
            if (!_hasHierarchy) _hierarchyStyle = MarkerHierarchyResolver.Fallback;

            // Apply hierarchy-driven size (cm -> metres conversion at this one call site).
            // Cannot assign through ?. operator to RectTransform -- check null first.
            if (symbol != null)
                symbol.RectTransform.sizeDelta = Vector2.one * (_hierarchyStyle.SizeCm / 100f);

            bool hasConfiguredCategory = CategoryPalette.TryResolveConfigured(poi.category, out var categoryColor, out var iconKey);
            var activeIconLibrary = _runtimeIconLibraryOverride != null ? _runtimeIconLibraryOverride : iconLibrary;

            // Determine status states
            bool isUnknown = poi.has_status && poi.status_unknown;
            bool knownStatus = poi.has_status && !isUnknown;

            // Symbol: always present, coloured by category, shaped by marker_shape.
            // A POI may override just the icon via custom_symbol_key -- category fill
            // colour, ring, and badge are unaffected.
            Sprite shapeSprite = shapeLibrary?.Get(ShapeKey(_shape));
            string resolvedIconKey = (poi.has_custom_symbol && !string.IsNullOrWhiteSpace(poi.custom_symbol_key))
                ? poi.custom_symbol_key
                : iconKey;
            Sprite iconSprite = ResolveIconWithFallback(activeIconLibrary, resolvedIconKey);

            // OutlineSameHue drains the FILL toward black as status worsens; the
            // other two styles keep the fill as a pure, constant category colour.
            // Skipped when status_unknown -- there's no known percentage to drain
            // toward, so the fill stays the plain category colour and the universal
            // "?" badge (ApplyStatus below) carries the whole signal instead.
            Color fill = (_outlineMode == MarkerOutlineMode.SameHue && knownStatus)
                ? StatusRamp.ShadeTowardBlack(categoryColor, poi.status_pct)
                : categoryColor;

            float iconOpacity = (_outlineMode == MarkerOutlineMode.SameHue && knownStatus)
                ? Mathf.Lerp(1f, 0.28f, Mathf.Clamp01(poi.status_pct / 100f))
                : 1f;

            // Background shape "none" (section 20.1): hide just the symbol's
            // backdrop while keeping the icon readable. Otherwise draw the shape
            // backdrop as usual.
            if (_shape == MarkerShape.None)
            {
                symbol?.SetBackgroundVisible(false);
                if (_applyCategoryVisuals && hasConfiguredCategory)
                    symbol?.SetIcon(iconSprite, IconTint, iconOpacity);
            }
            else if ((_applyCategoryVisuals && hasConfiguredCategory) || _applyShapeVisuals)
            {
                symbol?.SetBackgroundVisible(true);
                if (_applyShapeVisuals && hasConfiguredCategory && shapeSprite != null)
                    symbol?.SetBackground(shapeSprite, fill);

                if (_applyCategoryVisuals && hasConfiguredCategory)
                    symbol?.SetIcon(iconSprite, IconTint, iconOpacity);
            }

            // Ring: status-enabled non-badge visuals. Unknown can also render a
            // ring by resolving status_level_key (or the semantic fallback key
            // "unknown") from StatusRamp's configured levels.
            bool canRenderSameHue = _outlineMode != MarkerOutlineMode.SameHue || hasConfiguredCategory;
            bool canRenderRingByStyle = _enableStatusVisuals && poi.has_status && _outlineMode != MarkerOutlineMode.None && canRenderSameHue;
            StatusLevel unknownRingLevel = StatusRamp.UnknownFallbackLevel;
            bool hasUnknownRingLevel = isUnknown && TryResolveUnknownStatusLevel(poi, out unknownRingLevel);
            bool showRing = canRenderRingByStyle && (knownStatus || hasUnknownRingLevel);
            if (showRing)
            {
                var level = knownStatus ? StatusRamp.Resolve(poi.status_pct) : unknownRingLevel;
                bool shadeWithCategoryHue = _outlineMode == MarkerOutlineMode.SameHue && knownStatus;
                if (shadeWithCategoryHue)
                {
                    Color ringColor = ShadeRingTowardBlack(categoryColor, poi.status_pct);
                    ring?.Apply(level, ringColor);
                }
                else
                {
                    ring?.Apply(level);
                }
            }
            else
            {
                ring?.Hide();
            }

            // Rotate the status ring when the hierarchy level opts in.
            // Only meaningful when the ring is actually visible -- gated on showRing.
            // rotate_contour is now a hierarchy-level property, not a per-POI field.
            ring?.SetRotating(showRing && _hierarchyStyle.RotateContour);

            // Push the active icon library into the ring view so custom line
            // styles resolve from the same wall library (section 20.3).
            ring?.SetLineStyleLibrary(activeIconLibrary);

            // Badge: ordinary status badge for MarkerStyle.Badge, OR the universal "?"
            // badge for status_unknown regardless of style. The badge's background
            // shape comes from badge_shape (section 20.2), independent of marker_shape.
            Sprite badgeShapeSprite = _badgeShape == MarkerShape.None ? null : shapeLibrary?.Get(ShapeKey(_badgeShape));
            bool badgeHasBackground = _badgeShape != MarkerShape.None;
            if (_enableStatusVisuals && isUnknown)
            {
                // Unknown status can be author-driven through badge_category. If the
                // selected key is missing, fallback to unknown_damage, then to the
                // general unknown icon key.
                var unknownBadgeDef = ResolveUnknownBadgeDefinition(poi);
                Sprite unknownIcon = ResolveIconWithFallback(activeIconLibrary, unknownBadgeDef.IconKey);
                badge?.SetBackgroundVisible(badgeHasBackground);
                badge?.SetBackground(badgeShapeSprite, unknownBadgeDef.Color);
                badge?.SetIcon(unknownIcon, IconTint, 1f);
                badge?.SetVisible(true);
            }
            else if (_enableStatusVisuals && _useBadge && !string.IsNullOrWhiteSpace(poi.badge_category) && BadgeCategoryPalette.TryResolve(poi.badge_category, out var badgeDef))
            {
                Sprite badgeIcon = ResolveIconWithFallback(activeIconLibrary, badgeDef.IconKey);
                badge?.SetBackgroundVisible(badgeHasBackground);
                badge?.SetBackground(badgeShapeSprite, badgeDef.Color);
                badge?.SetIcon(badgeIcon, IconTint, 1f);
                badge?.SetVisible(true);
            }
            else if (_enableStatusVisuals && _useBadge && knownStatus)
            {
                StatusLevel level = StatusRamp.Resolve(poi.status_pct);
                badge?.SetBackgroundVisible(badgeHasBackground);
                badge?.SetBackground(badgeShapeSprite, level.RingColor);
                badge?.SetIcon(iconSprite, IconTint, 1f);
                badge?.SetVisible(true);
            }
            else
            {
                badge?.SetVisible(false);
            }

            // Label: set text, then apply layout
            if (labelText != null)
            {
                string name = poi.name;
                if (name.Length > 29)
                    name = name.Substring(0, 26) + "...";
                labelText.text = name;
            }

            // Apply layout after all elements are configured
            ApplyLayout();
        }

        private static Sprite ResolveIconWithFallback(SpriteKeyLibrary activeIconLibrary, string preferredKey)
        {
            if (activeIconLibrary == null)
                return null;

            if (!string.IsNullOrWhiteSpace(preferredKey))
            {
                Sprite preferred = activeIconLibrary.Get(preferredKey);
                if (preferred != null)
                    return preferred;
            }

            return activeIconLibrary.Get(FallbackUnknownIconKey);
        }

        private static BadgeCategoryPalette.BadgeDefinition ResolveUnknownBadgeDefinition(POIData poi)
        {
            if (poi != null &&
                !string.IsNullOrWhiteSpace(poi.badge_category) &&
                BadgeCategoryPalette.TryResolve(poi.badge_category, out var selectedUnknown))
            {
                return selectedUnknown;
            }

            if (BadgeCategoryPalette.TryResolve(FallbackUnknownBadgeCategoryKey, out var fallbackUnknown))
                return fallbackUnknown;

            return new BadgeCategoryPalette.BadgeDefinition(StatusRamp.UnknownColor, FallbackUnknownIconKey);
        }

        private static bool TryResolveUnknownStatusLevel(POIData poi, out StatusLevel level)
        {
            level = default;

            if (poi != null &&
                !string.IsNullOrWhiteSpace(poi.status_level_key) &&
                StatusRamp.TryResolveByKey(poi.status_level_key, out level))
            {
                return true;
            }

            if (StatusRamp.TryResolveByKey(FallbackUnknownStatusLevelKey, out level))
                return true;

            level = StatusRamp.UnknownFallbackLevel;
            return true;
        }

        private void ApplyLayout()
        {
            MarkerLayout.Apply(
                symbol?.RectTransform,
                ring?.RectTransform,
                badge?.RectTransform,
                labelText != null ? (RectTransform)labelText.transform : null,
                layout);
        }

        private void ApplyLabelState()
        {
            if (_anchor?.Data == null) return;

            // Label visibility now comes from the hierarchy level, not is_hero.
            // _hierarchyStyle is resolved once in ApplyVisuals and shared here.
            bool showLabel = _hierarchyStyle.ShowLabel;

            if (labelText != null && !_hasBaseLabelSize)
            {
                _baseLabelSize = ((RectTransform)labelText.transform).sizeDelta;
                _baseLabelWordWrapping = labelText.enableWordWrapping;
                _baseLabelOverflowMode = labelText.overflowMode;
                _baseLabelAlignment = labelText.alignment;
                var baseRect = (RectTransform)labelText.transform;
                _baseLabelAnchorMin = baseRect.anchorMin;
                _baseLabelAnchorMax = baseRect.anchorMax;
                _baseLabelPivot = baseRect.pivot;
                _hasBaseLabelSize = true;
            }

            // Label: hierarchy level with showLabel=true shows a persistent label,
            // others hide it. Otherwise identical label sizing/pivot logic as before.
            if (labelText != null)
            {
                labelText.gameObject.SetActive(showLabel);

                var labelRect = (RectTransform)labelText.transform;
                if (showLabel)
                {
                    labelText.enableWordWrapping = false;
                    labelText.overflowMode = TextOverflowModes.Overflow;
                    labelText.alignment = TextAlignmentOptions.Center;
                    labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                    labelRect.pivot = new Vector2(0.5f, 0.5f);
                    labelRect.sizeDelta = new Vector2(Mathf.Max(_baseLabelSize.x, 0.42f), _baseLabelSize.y);
                }
                else if (_hasBaseLabelSize)
                {
                    labelText.enableWordWrapping = _baseLabelWordWrapping;
                    labelText.overflowMode = _baseLabelOverflowMode;
                    labelText.alignment = _baseLabelAlignment;
                    labelRect.anchorMin = _baseLabelAnchorMin;
                    labelRect.anchorMax = _baseLabelAnchorMax;
                    labelRect.pivot = _baseLabelPivot;
                    labelRect.sizeDelta = _baseLabelSize;
                }
            }

            // Effects are now driven by the hierarchy level when one is resolved.
            // When no hierarchy level is set (hasHierarchy == false, e.g. the gallery
            // testing arbitrary flag combinations), fall back to the effectFlags
            // parameter as before. This is the one subtle fallback part of the refactor.
            MarkerEffectFlags activeFlags = _hasHierarchy ? _hierarchyStyle.EffectFlags : effectFlags;

            bool pulseActive = HasEffect(activeFlags, MarkerEffectFlags.Pulse);
            bool sunContoursActive = HasEffect(activeFlags, MarkerEffectFlags.SunContours);
            bool sunCirclesActive = HasEffect(activeFlags, MarkerEffectFlags.SunCircles);
            bool sunActive = sunContoursActive || sunCirclesActive;

            if (sunActive)
            {
                var sunStyle = sunContoursActive
                    ? MarkerSunEffect.SunVisualStyle.Contours
                    : MarkerSunEffect.SunVisualStyle.FilledCircles;
                sunEffect?.SetVisualStyle(sunStyle);
            }

            pulseEffect?.SetActive(pulseActive);
            glowEffect?.SetActive(false);
            sunEffect?.SetActive(sunActive);

            // The three single-accent styles are mutually exclusive in this
            // implementation (one MarkerAccentEffect instance, reconfigured per
            // marker) -- priority order below (RingPulse > SimpleSun > Beacon) is
            // arbitrary but deterministic. Stack freely with Pulse and Sun*; don't
            // expect two of these three at once on the same marker (section 19.2).
            bool ringPulseActive = HasEffect(activeFlags, MarkerEffectFlags.RingPulse);
            bool simpleSunActive = HasEffect(activeFlags, MarkerEffectFlags.SimpleSun);
            bool beaconActive = HasEffect(activeFlags, MarkerEffectFlags.Beacon);

            if (ringPulseActive)
                accentEffect?.Configure(symbol.RectTransform, MarkerAccentEffect.AccentShape.Contour, MarkerAccentEffect.AccentMotion.Breathe);
            else if (simpleSunActive)
                accentEffect?.Configure(symbol.RectTransform, MarkerAccentEffect.AccentShape.FilledCircle, MarkerAccentEffect.AccentMotion.Breathe);
            else if (beaconActive)
                accentEffect?.Configure(symbol.RectTransform, MarkerAccentEffect.AccentShape.Contour, MarkerAccentEffect.AccentMotion.Beacon);

            // Apply defaults after Configure so the configured shape/motion is set,
            // but defaults are applied before the first Update tick animates.
            accentEffect?.ApplyDefaults(_effectDefaults?.accent);

            accentEffect?.SetActive(ringPulseActive || simpleSunActive || beaconActive);
        }

        private static bool HasEffect(MarkerEffectFlags mask, MarkerEffectFlags effect)
        {
            return (mask & effect) != 0;
        }

        // OutlineSameHue ring stays in the category hue family while darkening with severity.
        private static Color ShadeRingTowardBlack(Color categoryColor, float pct)
        {
            float t = Mathf.Clamp01(pct / 100f);
            Color shaded = Color.Lerp(categoryColor, Color.black, 0.62f * t);
            shaded.a = 1f;
            return shaded;
        }

        // Explicit shape key mapping to avoid enum.ToString() producing "roundedsquare"
        // instead of "rounded_square" (and similar for other multi-word shapes).
        private static string ShapeKey(MarkerShape shape) => shape switch
        {
            MarkerShape.Circle => "circle",
            MarkerShape.RoundedSquare => "rounded_square",
            MarkerShape.Hexagon => "hexagon",
            MarkerShape.Diamond => "diamond",
            MarkerShape.Star => "star",
            // "none" -> null: no background sprite is looked up; ApplyVisuals tests
            // this before drawing a backdrop behind the symbol/badge glyph.
            MarkerShape.None => null,
            _ => "circle",
        };

        // Nudge this marker up by overlapOffsetAmount * offsetIndex so it doesn't sit
        // on top of another marker that resolved to a similar screen position.
        // Idempotent: sets an absolute offset from the stored base position,
        // never adds to the current position.
        public void ApplyOverlapOffset(float offsetIndex)
        {
            if (_anchor == null) return;

            // Capture the base position on first call (spawn-time position)
            if (!_hasBasePosition)
            {
                _baseLocalPosition = _anchor.transform.localPosition;
                _hasBasePosition = true;
            }

            // Set position to base + offset, never add to current position
            Vector3 newPos = _baseLocalPosition;
            newPos.y += offsetIndex * 0.15f;
            _anchor.transform.localPosition = newPos;
        }

                private void EnsureMarkerWiring(bool allowCreate)
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (symbol != null)
            {
                if (allowCreate) symbol.EnsureIconReference();
            }

            if (badge != null)
            {
                if (allowCreate) badge.EnsureIconReference();
            }

            if (symbol == null)
                return;

            if (pulseEffect == null && allowCreate)
                pulseEffect = GetComponent<MarkerPulseEffect>() ?? gameObject.AddComponent<MarkerPulseEffect>();
            pulseEffect?.ApplyDefaults(_effectDefaults?.pulse);
            pulseEffect?.Configure(symbol.RectTransform);

            var haloImage = EnsureHaloImage(allowCreate);
            if (glowEffect == null && allowCreate)
                glowEffect = GetComponent<MarkerGlowEffect>() ?? gameObject.AddComponent<MarkerGlowEffect>();
            if (haloImage != null)
                glowEffect?.Configure(haloImage);

            if (sunEffect == null && allowCreate)
                sunEffect = GetComponent<MarkerSunEffect>() ?? gameObject.AddComponent<MarkerSunEffect>();
            sunEffect?.ApplyDefaults(_effectDefaults?.sun);
            sunEffect?.Configure(symbol.RectTransform);

            if (accentEffect == null && allowCreate)
                accentEffect = GetComponent<MarkerAccentEffect>() ?? gameObject.AddComponent<MarkerAccentEffect>();
            accentEffect?.ApplyDefaults(_effectDefaults?.accent);
        }

        private Image EnsureHaloImage(bool allowCreate)
        {
            var existing = transform.Find("Halo");
            Image haloImage;
            if (existing != null)
            {
                haloImage = existing.GetComponent<Image>() ?? existing.gameObject.AddComponent<Image>();
            }
            else if (!allowCreate)
            {
                return null;
            }
            else
            {
                var haloObject = new GameObject("Halo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var haloTransform = (RectTransform)haloObject.transform;
                haloTransform.SetParent(transform, false);
                haloTransform.SetSiblingIndex(0);
                haloTransform.anchorMin = new Vector2(0.5f, 0.5f);
                haloTransform.anchorMax = new Vector2(0.5f, 0.5f);
                haloTransform.sizeDelta = new Vector2(0.16f, 0.16f);
                haloImage = haloObject.GetComponent<Image>();
            }

            var symbolBackground = symbol.GetComponent<Image>();
            if (symbolBackground != null && haloImage.sprite == null)
                haloImage.sprite = symbolBackground.sprite;

                        haloImage.color = HaloTint;
            haloImage.raycastTarget = false;
            haloImage.enabled = false;
            return haloImage;
        }

        private const float ALPHA_FULL = 1f;
        private const float ALPHA_HIDDEN = 0f;
        // Alpha at/below which a marker is treated as hidden (raycasts off).
        private const float DIM_THRESHOLD = 0.001f;

        // Selection-dim level (spec section 11) that persists across LOD ticks.
        // When non-1, LODController's per-tick SetVisible(true, ...) targets this
        // dimmed alpha instead of full, so selection-highlight and LOD layer their
        // alpha writes through one seam instead of fighting over CanvasGroup.alpha
        // (the "single source of truth" the MarkerView header documents). Default
        // 1.0 => existing callers (LOD, reveal) see identical behaviour until a
        // highlight is active, so there is no regression when the feature is off.
        private float _highlightAlpha = ALPHA_FULL;

        // Read-only access to this marker's hierarchy reveal duration, so
        // SelectionHighlightController can time its highlight fade to the same
        // curve MarkerRevealEffect uses for this marker's own spawn-in.
        public float RevealDurationSeconds => _hasHierarchy
            ? _hierarchyStyle.RevealDurationSeconds
            : MarkerHierarchyResolver.Fallback.RevealDurationSeconds;

        // Instantly toggle marker visibility via the shared CanvasGroup.
        // In Edit Mode, sets alpha immediately (coroutines do not tick there).
        public void SetVisible(bool visible)
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) return;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            float targetAlpha = visible ? _highlightAlpha : ALPHA_HIDDEN;
            _canvasGroup.alpha = targetAlpha;
            bool interactive = targetAlpha > DIM_THRESHOLD;
            _canvasGroup.interactable = interactive;
            _canvasGroup.blocksRaycasts = interactive;
        }

        // Fade marker visibility over fadeDuration, using the shared CanvasGroup.
        // In Edit Mode, falls back to instant (coroutines do not tick there).
                public void SetVisible(bool visible, float fadeDuration)
        {
            // Route the bool API through the float overload, mapping visible to this
            // marker's current highlight level. This is what lets LODController's
            // visible-call retarget to a dimmed alpha (spec section 11) instead of
            // snapping back to full -- LOD and selection-highlight share one seam.
            SetVisible(visible ? _highlightAlpha : ALPHA_HIDDEN, fadeDuration);
        }

        // Fade (or instantly set, at fadeDuration <= 0) to an explicit target alpha.
        // Partial alpha implements the selection dim (spec section 11): non-selected
        // markers sit at a dim level while remaining tappable; the selected marker
        // targets full. Reuses the same CanvasGroup + coroutine as the bool overload
        // so LOD and selection-highlight compose through one alpha seam.
        public void SetVisible(float targetAlpha, float fadeDuration)
        {
            if (!Application.isPlaying || fadeDuration <= 0f)
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) return;

                if (_fadeCoroutine != null)
                {
                    StopCoroutine(_fadeCoroutine);
                    _fadeCoroutine = null;
                }

                _highlightAlpha = targetAlpha > DIM_THRESHOLD ? Mathf.Clamp01(targetAlpha) : ALPHA_HIDDEN;
                _canvasGroup.alpha = targetAlpha;
                bool interactive = targetAlpha > DIM_THRESHOLD;
                _canvasGroup.interactable = interactive;
                _canvasGroup.blocksRaycasts = interactive;
                return;
            }

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) return;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, fadeDuration));
        }

        // Crossfade CanvasGroup alpha from current to target; flip interaction
        // flags at the endpoints so raycasts resume only above the hidden threshold.
        // Stamps _highlightAlpha so a concurrent LOD tick (which calls
        // SetVisible(true, ...) -> SetVisible(_highlightAlpha, ...)) targets the same
        // dimmed level instead of snapping back to full -- the seam that lets
        // selection-dimming and LOD coexist on one shared CanvasGroup.
        private IEnumerator FadeCoroutine(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            _highlightAlpha = targetAlpha > DIM_THRESHOLD ? Mathf.Clamp01(targetAlpha) : ALPHA_HIDDEN;
            bool willBeInteractive = targetAlpha > DIM_THRESHOLD;
            if (willBeInteractive)
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

            if (!willBeInteractive)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
