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

        // Runtime state
        private POIAnchor _anchor;
        private Vector3 _baseLocalPosition;
        private bool _hasBasePosition;
        private MarkerOutlineMode _outlineMode = MarkerOutlineMode.Gold;
        private bool _useBadge;
        private MarkerShape _shape;
        private MarkerShape _badgeShape = MarkerShape.Circle;
        private SpriteKeyLibrary _runtimeIconLibraryOverride;
        private bool _applyCategoryVisuals = true;
        private bool _applyShapeVisuals = true;
        private bool _enableStatusVisuals = true;
        private Vector2 _baseLabelSize;
        private bool _hasBaseLabelSize;
        private bool _baseLabelWordWrapping;
        private TextOverflowModes _baseLabelOverflowMode;
        private TextAlignmentOptions _baseLabelAlignment;
        private Vector2 _baseLabelAnchorMin;
        private Vector2 _baseLabelAnchorMax;
        private Vector2 _baseLabelPivot;

        // Expose the POI id for deterministic sorting in overlap resolution
        public string PoiId { get; private set; }

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
            MarkerShape badgeShape = MarkerShape.Circle)
        {
            EnsureMarkerWiring(allowCreate: true);
            _anchor = anchor;
            _outlineMode = outlineMode;
            _useBadge = useBadge;
            _shape = shape;
            _badgeShape = badgeShape;
            _runtimeIconLibraryOverride = iconLibraryOverride;
            _applyCategoryVisuals = applyCategoryVisuals;
            _applyShapeVisuals = applyShapeVisuals;
            _enableStatusVisuals = enableStatusVisuals;
            effectFlags = effects;
            PoiId = anchor?.Data?.id ?? string.Empty;

            ApplyVisuals();
            ApplyHeroState();
        }

         private void ApplyVisuals()
         {
             if (_anchor?.Data == null) return;

             var poi = _anchor.Data;
             bool hasConfiguredCategory = CategoryPalette.TryResolveConfigured(poi.category, out var categoryColor, out var iconKey);
             var activeIconLibrary = _runtimeIconLibraryOverride != null ? _runtimeIconLibraryOverride : iconLibrary;

             // Determine status states
             bool isUnknown = poi.has_status && poi.status_unknown;
             bool knownStatus = poi.has_status && !isUnknown;

             // Symbol: always present, coloured by category, shaped by marker_shape.
             // A hero POI may override just the icon via hero_icon_key (section 21) --
             // category fill colour, ring, and badge are unaffected.
             Sprite shapeSprite = shapeLibrary?.Get(ShapeKey(_shape));
             string resolvedIconKey = (poi.is_hero && !string.IsNullOrWhiteSpace(poi.hero_icon_key))
                 ? poi.hero_icon_key
                 : iconKey;
             Sprite iconSprite = resolvedIconKey != null ? activeIconLibrary?.Get(resolvedIconKey) : null;

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

             // Ring: only for OutlineGold and OutlineSameHue styles, and only for a known
             // status value -- unknown suppresses the ring entirely, the "?" badge below
             // is the sole indicator in that case, across all three styles.
             bool canRenderSameHue = _outlineMode != MarkerOutlineMode.SameHue || hasConfiguredCategory;
             bool showRing = _enableStatusVisuals && knownStatus && _outlineMode != MarkerOutlineMode.None && canRenderSameHue;
             if (showRing)
             {
                 var level = StatusRamp.Resolve(poi.status_pct);
                 if (_outlineMode == MarkerOutlineMode.SameHue)
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

             // Rotate the status ring when the POI opts in (rotate_contour). Only
             // meaningful when the ring is actually visible -- gated on showRing.
             ring?.SetRotating(showRing && poi.rotate_contour);

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
                 // Same treatment regardless of markerStyle: a neutral-grey "?"
                 // badge, deliberately outside the gold->rust status family
                 // (StatusRamp.UnknownColor), so "we don't know" never reads as
                 // an extra point on the destruction scale.
                 Sprite questionIcon = activeIconLibrary?.Get("unknown");
                 badge?.SetBackgroundVisible(badgeHasBackground);
                 badge?.SetBackground(badgeShapeSprite, StatusRamp.UnknownColor);
                 badge?.SetIcon(questionIcon, IconTint, 1f);
                 badge?.SetVisible(true);
             }
             else if (_enableStatusVisuals && _useBadge && !string.IsNullOrWhiteSpace(poi.badge_category) && BadgeCategoryPalette.TryResolve(poi.badge_category, out var badgeDef))
             {
                 Sprite badgeIcon = !string.IsNullOrWhiteSpace(badgeDef.IconKey) ? activeIconLibrary?.Get(badgeDef.IconKey) : null;
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

        private void ApplyLayout()
        {
            MarkerLayout.Apply(
                symbol?.RectTransform,
                ring?.RectTransform,
                badge?.RectTransform,
                labelText != null ? (RectTransform)labelText.transform : null,
                layout);
        }

        private void ApplyHeroState()
        {
            if (_anchor?.Data == null) return;

            bool isHero = _anchor.Data.is_hero;

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

            // Label: hero POIs show persistent label, others hide it
            if (labelText != null)
            {
                labelText.gameObject.SetActive(isHero);

                var labelRect = (RectTransform)labelText.transform;
                if (isHero)
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

            // Effects are modular and independent of is_hero -- any POI can opt into
            // any effect via effect_mode in config.json. is_hero only controls the
            // persistent label above.
            bool pulseActive = HasEffect(effectFlags, MarkerEffectFlags.Pulse);
            bool sunContoursActive = HasEffect(effectFlags, MarkerEffectFlags.SunContours);
            bool sunCirclesActive = HasEffect(effectFlags, MarkerEffectFlags.SunCircles);
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
            bool ringPulseActive = HasEffect(effectFlags, MarkerEffectFlags.RingPulse);
            bool simpleSunActive = HasEffect(effectFlags, MarkerEffectFlags.SimpleSun);
            bool beaconActive = HasEffect(effectFlags, MarkerEffectFlags.Beacon);

            if (ringPulseActive)
                accentEffect?.Configure(symbol.RectTransform, MarkerAccentEffect.AccentShape.Contour, MarkerAccentEffect.AccentMotion.Breathe);
            else if (simpleSunActive)
                accentEffect?.Configure(symbol.RectTransform, MarkerAccentEffect.AccentShape.FilledCircle, MarkerAccentEffect.AccentMotion.Breathe);
            else if (beaconActive)
                accentEffect?.Configure(symbol.RectTransform, MarkerAccentEffect.AccentShape.Contour, MarkerAccentEffect.AccentMotion.Beacon);

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
            pulseEffect?.Configure(symbol.RectTransform);

            var haloImage = EnsureHaloImage(allowCreate);
            if (glowEffect == null && allowCreate)
                glowEffect = GetComponent<MarkerGlowEffect>() ?? gameObject.AddComponent<MarkerGlowEffect>();
            if (haloImage != null)
                glowEffect?.Configure(haloImage);

            if (sunEffect == null && allowCreate)
                sunEffect = GetComponent<MarkerSunEffect>() ?? gameObject.AddComponent<MarkerSunEffect>();
            sunEffect?.Configure(symbol.RectTransform);

            if (accentEffect == null && allowCreate)
                accentEffect = GetComponent<MarkerAccentEffect>() ?? gameObject.AddComponent<MarkerAccentEffect>();

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

    }
}
