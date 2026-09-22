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
    //   - RippleEffect (optional)                -> MarkerRippleEffect
    //   - HaloEffect (optional)                  -> MarkerHaloEffect
    //
    // All effect components are optional -- a prefab without them simply never
    // animates. This keeps the base marker cheap.
    public class MarkerView : MonoBehaviour
    {
        private static readonly Color IconTint = new Color(0.949f, 0.925f, 0.827f);

        [Header("References")]
        [SerializeField] private MarkerCircleGlyphView symbol;
        [SerializeField] private MarkerRingView ring;
        [SerializeField] private MarkerCircleGlyphView badge;
        [SerializeField] private TextMeshProUGUI labelText;

        [Header("Layout")]
        [SerializeField] private MarkerLayoutProportions layout = new();

        [Header("Effects (optional)")]
        [SerializeField] private MarkerPulseEffect pulseEffect;
        [SerializeField] private MarkerRippleEffect rippleEffect;
        [SerializeField] private MarkerHaloEffect haloEffect;
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

        // Marker-root displacement (Block 4: marker/both path). Captures the root's
        // world position at baseline and offsets it camera-relatively (not via
        // label anchoredPosition). _baseLocalPosition/_hasBasePosition above are
        // intentionally NOT reused here -- their localPosition semantics would be
        // ambiguous for a root-position delta that must stay screen-aligned.
        private Vector3 _baseWorldPosition;
        private bool _hasWorldBasePosition;
        private bool _hasMarkerOffset;
        private MarkerVisualSettings _settings = MarkerVisualSettings.Default();
        private EffectDefaults _effectDefaults;
        private Color _resolvedCategoryColor;

        private Vector2 _baseLabelSize;
        private bool _hasBaseLabelSize;
        private HierarchyStyle _hierarchyStyle;
        private bool _hasHierarchy;
        private HierarchyStyle? _styleOverride;

        // Single source of truth for the effect-flag fallback rule (spec _2_3 section 9):
        // hierarchy level's flags when resolved, otherwise the serialized inspector flags,
        // then filtered by the wall's switches (master + each effect's own "enabled";
        // no effect_defaults = everything on). Consumed by both ApplyEffects (rendering) and
        // GetVisualRadiusWorld (displacement bounds, Domain 2.5) -- keep them in lockstep
        // by construction.
        private MarkerEffectFlags ActiveEffectFlags
        {
            get
            {
                var requested = _hasHierarchy ? _hierarchyStyle.EffectFlags : effectFlags;
                return _effectDefaults == null ? requested : _effectDefaults.FilterEnabled(requested);
            }
        }

        private bool _baseLabelWordWrapping;
        private TextOverflowModes _baseLabelOverflowMode;
        private TextAlignmentOptions _baseLabelAlignment;
        private Vector2 _baseLabelAnchorMin;
        private Vector2 _baseLabelAnchorMax;
                                private Vector2 _baseLabelPivot;

        // Label-only displacement (§4): absolute base anchoredPosition captured from
        // ApplyLabelState on first layout; ApplyLabelOffset/ClearLabelOffset offset from
        // this base, never add (idempotent). Distinct from the root _baseLocalPosition/_hasBasePosition
        // pair above, which displaces the whole marker root (Block 4 marker/both path).
        private Vector2 _baseLabelAnchoredPosition;
        private bool _hasLabelPosition;
        private bool _hasLabelOffset;

        // Hide/show just this marker's label text only (does not affect marker offset or
        // whole-marker visibility, which belongs to LODController per _2.5 Section 7).
        // Used by MarkerOverlapResolver's 2.5-h hide-fallback: when displacement hits
        // the configured max and labels still crowd each other, the resolver hides
        // the lower-priority label rather than rendering a clamped overlap.
        public void SetLabelVisible(bool visible)
        {
            if (labelText != null)
                labelText.enabled = visible;
        }


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

        // Initialise is called once per spawn (WallSession, the POI Editor rig, the galleries). The
        // wall-level look arrives as one MarkerVisualSettings (resolved by MarkerVisualSettings.Resolve),
        // so the marker can be built in the Editor without a config present.
        // `effects` is only the fallback for a marker whose POI names no hierarchy level.
        public void Initialise(
            POIAnchor anchor,
            MarkerVisualSettings settings,
            MarkerEffectFlags effects = MarkerEffectFlags.None,
            EffectDefaults effectDefaults = null,
            HierarchyStyle? styleOverride = null)
        {
            EnsureMarkerWiring(allowCreate: true);
            _anchor = anchor;
            _effectDefaults = effectDefaults;
            _styleOverride = styleOverride;
            effectFlags = effects;
            PoiId = anchor?.Data?.id ?? string.Empty;
            UseSettings(settings);

            ApplyVisuals();
            ApplyLabelState();

            var reveal = GetComponent<MarkerRevealEffect>();
            reveal?.Play(_hierarchyStyle.RevealDelaySeconds, _hierarchyStyle.RevealDurationSeconds);
        }

        // Swap in new wall-level settings and re-apply ONLY the visuals: no reveal restart, no label
        // re-capture. Used when the marker config changes while the wall is running (live Play Mode).
        public void ReapplyVisuals(MarkerVisualSettings settings)
        {
            if (_anchor?.Data == null) return;
            UseSettings(settings);
            ApplyVisuals();
            ApplyLabelState();
            ApplyEffects();
        }

        // Keep the settings and the parts that depend on them (layout ratios, ring spin) in step
        private void UseSettings(MarkerVisualSettings settings)
        {
            _settings = settings ?? MarkerVisualSettings.Default();
            layout.ringSizeRatio = _settings.RingSizeRatio;
            layout.badgeSizeRatio = _settings.BadgeSizeRatio;
            layout.badgeDirection = _settings.BadgeDirection;
            ring?.SetSpinSpeed(_settings.ContourSpinDegPerSecond);
        }

        // Look up this marker's hierarchy style (a style override replaces the lookup entirely)
        private void ResolveHierarchyStyle(POIData poi)
        {
            if (_styleOverride.HasValue)
            {
                _hierarchyStyle = _styleOverride.Value;
                _hasHierarchy = true;
                return;
            }

            _hasHierarchy = MarkerHierarchyResolver.TryResolveByKey(poi.hierarchy_level_key, out _hierarchyStyle);
            if (!_hasHierarchy) _hierarchyStyle = MarkerHierarchyResolver.Fallback;
        }

        // Swap in new effect settings and re-apply ONLY the effects (no visual rebuild, no reveal
        // restart). Used when the effects config changes while the wall is running.
        public void ReapplyEffects(EffectDefaults effectDefaults)
        {
            if (_anchor?.Data == null) return;
            _effectDefaults = effectDefaults;
            ResolveHierarchyStyle(_anchor.Data);
            ApplyEffects();
        }

        // Apply the resolved visual state (MarkerVisualResolver decides, this only draws it)
        private void ApplyVisuals()
        {
            if (_anchor?.Data == null) return;

            var poi = _anchor.Data;

            // Resolve hierarchy level to get size/label/effects/reveal-delay (a style override, used by
            // the effects preview grid, replaces the lookup entirely)
            ResolveHierarchyStyle(poi);
            if (symbol != null)
                symbol.RectTransform.sizeDelta = Vector2.one * (_hierarchyStyle.SizeCm / 100f);

            MarkerVisualState state = MarkerVisualResolver.Resolve(poi, _settings);
            CategoryPalette.TryResolveConfigured(poi.category, out var categoryColor, out _);
            _resolvedCategoryColor = categoryColor;
            var activeIconLibrary = _settings.WallIconLibrary != null ? _settings.WallIconLibrary : iconLibrary;
            Sprite iconSprite = ResolveIconWithFallback(activeIconLibrary, state.IconKey);

            // Symbol: backdrop shape (or none) and the icon
            if (state.Background == SymbolBackgroundState.Hidden)
                symbol?.SetBackgroundVisible(false);
            else if (state.Background == SymbolBackgroundState.Visible)
            {
                symbol?.SetBackgroundVisible(true);
                Sprite shapeSprite = shapeLibrary?.Get(ShapeKey(_settings.Shape));
                if (state.DrawShapeFill && shapeSprite != null)
                    symbol?.SetBackground(shapeSprite, state.SymbolFill);
            }
            if (state.ShowIcon)
                symbol?.SetIcon(iconSprite, IconTint, state.IconOpacity);

            // Ring (status outline) and its optional spin
            if (state.ShowRing)
            {
                if (state.RingUsesCategoryHue) ring?.Apply(state.RingLevel, state.RingHueColor);
                else ring?.Apply(state.RingLevel);
            }
            else
                ring?.Hide();
            ring?.SetRotating(state.ShowRing && _hierarchyStyle.RotateContour);
            ring?.SetLineStyleLibrary(activeIconLibrary);

            // Badge: its own background shape (badge_shape), independent of the symbol's
            bool badgeHasBackground = _settings.BadgeShape != MarkerShape.None;
            if (state.Badge == BadgeSource.Hidden)
                badge?.SetVisible(false);
            else
            {
                Sprite badgeShapeSprite = badgeHasBackground ? shapeLibrary?.Get(ShapeKey(_settings.BadgeShape)) : null;
                Sprite badgeIcon = state.BadgeIconKey != null
                    ? ResolveIconWithFallback(activeIconLibrary, state.BadgeIconKey)
                    : iconSprite;
                badge?.SetBackgroundVisible(badgeHasBackground);
                badge?.SetBackground(badgeShapeSprite, state.BadgeColor);
                badge?.SetIcon(badgeIcon, IconTint, 1f);
                badge?.SetVisible(true);
            }

            // Label text, then layout after every element is configured
            if (labelText != null)
            {
                string name = poi.name;
                if (name.Length > 29)
                    name = name.Substring(0, 26) + "...";
                labelText.text = name;
            }
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

            return activeIconLibrary.Get(MarkerVisualResolver.FallbackUnknownIconKey);
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

            // Capture the layout-time base anchoredPosition for label-only displacement (§4).
            // MarkerLayout.Apply has run by now, so this is the post-layout anchor point.
            EnsureLabelBaseCaptured();

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

            ApplyEffects();
        }

        // Switch each effect on or off for this marker and hand it ITS OWN parameter block from
        // the wall's effect_defaults. Which effects are requested comes from the hierarchy level
        // (or the effectFlags parameter when no level resolved, e.g. the galleries), already
        // filtered by the wall's switches in ActiveEffectFlags.
        private void ApplyEffects()
        {
            MarkerEffectFlags flags = ActiveEffectFlags;

            pulseEffect?.ApplyDefaults(_effectDefaults?.pulse);
            pulseEffect?.SetActive(HasEffect(flags, MarkerEffectFlags.Pulse));

            // Ripple rings and discs are alternatives (one MarkerRippleEffect per marker); rings
            // win if both are somehow requested. Defaults go in before SetActive so the first
            // Update tick already animates with them.
            bool rings = HasEffect(flags, MarkerEffectFlags.RippleRings);
            bool discs = HasEffect(flags, MarkerEffectFlags.RippleDiscs);
            if (rippleEffect != null)
            {
                if (rings)
                {
                    rippleEffect.SetStyle(MarkerRippleEffect.RippleStyle.Rings);
                    rippleEffect.ApplyDefaults(_effectDefaults?.ripple_rings);
                }
                else if (discs)
                {
                    rippleEffect.SetStyle(MarkerRippleEffect.RippleStyle.Discs);
                    rippleEffect.ApplyDefaults(_effectDefaults?.ripple_discs);
                }
                rippleEffect.SetActive(rings || discs);
            }

            // The three halo variants are alternatives too (one MarkerHaloEffect per marker),
            // priority HaloRing > HaloDisc > Beacon. They stack freely with Pulse and Ripple.
            bool haloRing = HasEffect(flags, MarkerEffectFlags.HaloRing);
            bool haloDisc = HasEffect(flags, MarkerEffectFlags.HaloDisc);
            bool beacon = HasEffect(flags, MarkerEffectFlags.Beacon);
            if (haloEffect != null)
            {
                if (haloRing)
                {
                    haloEffect.Configure(symbol.RectTransform, MarkerHaloEffect.HaloVariant.Ring);
                    haloEffect.ApplyDefaults(_effectDefaults?.halo_ring);
                }
                else if (haloDisc)
                {
                    haloEffect.Configure(symbol.RectTransform, MarkerHaloEffect.HaloVariant.Disc);
                    haloEffect.ApplyDefaults(_effectDefaults?.halo_disc);
                }
                else if (beacon)
                {
                    haloEffect.Configure(symbol.RectTransform, MarkerHaloEffect.HaloVariant.Beacon);
                    haloEffect.ApplyDefaults(_effectDefaults?.beacon);
                }
                haloEffect.SetActive(haloRing || haloDisc || beacon);
            }
        }

        private static bool HasEffect(MarkerEffectFlags mask, MarkerEffectFlags effect)
        {
            return (mask & effect) != 0;
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

        // Shift this marker's label by screenOffsetPx (screen-space pixels) from its
        // layout-time base position (§4). Pure, frame-stable conversion: only when the
        // root has zero roll relative to the screen (e.g. vertical_alignment_mode ==
        // "screen_up") do the root's local X/Y axes equal screen X/Y by construction. In
        // every other case the root can be rolled relative to the screen, so the
        // world-space offset is rotated by -rootRollDeg before becoming a local-space
        // delta (_2.1_Marker_Orientation.md Block 5) -- this reduces to the original
        // zero-roll behaviour exactly when the root is screen-aligned, since rootRollDeg
        // is 0 there. Converting screenOffsetPx to a local-space delta via
        // distance+FOV (MarkerLayout.ScreenPixelsToWorld) and writing it as an
        // anchoredPosition offset needs no camera-basis reprojection, and critically, no
        // dependency on the root's rotation at call time. A world<->screen<->world round-trip
        // (an earlier revision of this method) bakes the root's CURRENT rotation into the
        // result via InverseTransformPoint; since MarkerBillboard re-rotates every frame while
        // this method is only called once per (slower) LOD evaluation cycle, that rotation
        // goes stale by the next frame and the label visibly drifts. anchoredPosition has no
        // such dependency -- Unity re-renders it correctly every frame regardless of how many
        // times the parent has rotated since this was last computed. Idempotent: always
        // computed fresh from the same base, never accumulated. label_only only (§4); the
        // marker/both root-offset path is Block 4.
        public void ApplyLabelOffset(Camera cam, Vector2 screenOffsetPx)
        {
            if (_anchor == null || labelText == null) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            EnsureLabelBaseCaptured();
            RectTransform rt = (RectTransform)labelText.transform;

            // Depth along the camera's forward axis -- NOT Vector3.Distance (full Euclidean
            // distance to the point). Camera.WorldToScreenPoint's screen-space scaling (and
            // the FOV-based world-units-per-pixel formula in MarkerLayout.ScreenPixelsToWorld)
            // is derived from perspective depth, which only equals Euclidean distance for a
            // marker exactly on the optical axis (screen center). Off-center markers (the
            // common case) have Euclidean distance > depth, which previously introduced a
            // small but consistent pixel-conversion error growing with distance from center.
            float depthM = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);
            Vector2 worldOffset = MarkerLayout.ScreenPixelsToWorld(screenOffsetPx, depthM, cam);

            // Compensate for the root's roll relative to the screen (_2.1_Marker_Orientation.md
            // Block 5). rootRollDeg is 0 whenever the root is screen-aligned (the default and
            // the only mode this method shipped against before Block 5), so this is a no-op
            // there and only takes effect for world_up / yaw_only / wall_fixed.
            Vector3 screenUpWorld = MarkerOrientationResolver.ScreenUpWorld(cam);
            float rootRollDeg = MarkerOrientationResolver.RootRollDeg(transform.rotation, screenUpWorld);
            worldOffset = MarkerOrientationResolver.Rotate2D(worldOffset, -rootRollDeg);

            // The label's anchoredPosition is expressed in this marker root's LOCAL space,
            // which is not guaranteed to be unscaled: MarkerRevealEffect animates this exact
            // root's localScale 0->1 over its reveal duration right after Initialise() (see
            // _2.3_Marker_Hierarchy.md Section 5's "settled baseline" contract -- other
            // continuous systems, this one included, must compute correctly against this
            // channel even mid-transition, not only once it settles at 1). A world-space
            // offset must be divided by the root's current lossyScale to become a correct
            // local-space delta -- skipping this collapses the offset toward zero while the
            // root is still scaling up from its reveal-in pop (e.g. a lossyScale of 0.05
            // shrinks an intended 40px screen separation down to ~2px), which is the actual
            // root cause of the screen-position error/drift previously misdiagnosed as a
            // billboard-rotation/geometry problem.
            Vector3 rootScale = transform.lossyScale;
            float sx = Mathf.Approximately(rootScale.x, 0f) ? 1f : rootScale.x;
            float sy = Mathf.Approximately(rootScale.y, 0f) ? 1f : rootScale.y;
            Vector2 localOffset = new Vector2(worldOffset.x / sx, worldOffset.y / sy);

            rt.anchoredPosition = _baseLabelAnchoredPosition + localOffset;
            _hasLabelOffset = true;
        }

        // Restore the label to its layout-time base position (below the symbol, per
        // MarkerLayout.Apply's labelGap -- NOT the marker's center/zero).
        public void ClearLabelOffset()
        {
            if (!_hasLabelOffset) return;
            _hasLabelOffset = false;
            if (labelText == null) return;

            EnsureLabelBaseCaptured();
            var rt = (RectTransform)labelText.transform;
            rt.anchoredPosition = _baseLabelAnchoredPosition;
        }

        private void EnsureLabelBaseCaptured()
        {
            // Capture the layout-time base anchoredPosition on first use. Guarded so it is
            // idempotent whether the first caller is ApplyLabelState (every ApplyVisuals)
            // or ApplyLabelOffset/ClearLabelOffset (only when displacement runs).
            if (!_hasLabelPosition && labelText != null)
            {
                _baseLabelAnchoredPosition = ((RectTransform)labelText.transform).anchoredPosition;
                _hasLabelPosition = true;
            }
        }

        // Shift this marker's ROOT transform by screenOffsetPx in screen space,
        // converted to a camera-relative world delta (Block 4, spec Section 4).
        // The root's world position is displaced along cam.transform.right and
        // cam.transform.up so the offset stays screen-aligned regardless of camera
        // orientation. This is the marker/both displace_target path; label
        // displacement uses ApplyLabelOffset which writes to anchoredPosition instead.
        public void ApplyMarkerOffset(Camera cam, Vector2 screenOffsetPx)
        {
            if (_anchor == null) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            EnsureMarkerWorldBaseCaptured();

            // Depth along the camera's forward axis -- same perspective-depth formula
            // as ApplyLabelOffset. NOT Euclidean distance to the marker.
            float depthM = Vector3.Dot(transform.position - cam.transform.position, cam.transform.forward);

            // Convert screen pixels to world units at this depth, then project onto
            // the camera's right/up basis so the offset stays screen-aligned even at
            // grazing angles (where cam.forward has no screen-axis component).
            Vector2 world2D = MarkerLayout.ScreenPixelsToWorld(screenOffsetPx, depthM, cam);
            Vector3 worldDelta = cam.transform.right * world2D.x + cam.transform.up * world2D.y;

            transform.position = _baseWorldPosition + worldDelta;
            _hasMarkerOffset = true;
        }

        // Restore the marker root to its baseline world position.
        public void ClearMarkerOffset()
        {
            if (!_hasMarkerOffset) return;
            _hasMarkerOffset = false;
            if (_anchor == null) return;

            transform.position = _baseWorldPosition;
        }

        private void EnsureMarkerWorldBaseCaptured()
        {
            // Capture the world-space baseline position on first use. Idempotent.
            // Unlike label anchoredPosition, root world position is independent of
            // MarkerBillboard's per-frame rotation, so no stale-rotation concern.
            if (!_hasWorldBasePosition)
            {
                _baseWorldPosition = transform.position;
                _hasWorldBasePosition = true;
            }
        }

        // Leader line color (spec Section 6): reads the same resolved category
        // color that the Symbol uses for its background fill. Exposed so
        // MarkerLeaderLine can pick up the correct tint without re-resolving.
        internal Color LeaderLineColor => _resolvedCategoryColor;

        // Delegate to the MarkerLeaderLine component on this prefab, if present.
        // Called from ApplyDisplacement every LOD cycle; the component then
        // self-updates in LateUpdate for per-frame visibility (camera distance).
        public void UpdateLeaderLine(Camera cam, DisplacementSettings settings)
        {
            if (!TryGetComponent<MarkerLeaderLine>(out var leaderLine)) return;
            if (_hasWorldBasePosition)
            {
                leaderLine.Configure(_baseWorldPosition, _resolvedCategoryColor);
            }
            leaderLine.UpdateVisibility(cam, settings);
        }

        // Test seam (InternalsVisibleTo -> TileStories.Tests.Runtime). Exposes the live label
        // RectTransform so Tier-0 label-displacement tests can assert its screen position.
        internal RectTransform LabelRect => labelText != null ? (RectTransform)labelText.transform : null;

        internal bool HasLabelOffset => _hasLabelOffset;

        // Visual extent in world-space metres (radius from centre to outermost visual element).
        // Combines hierarchy size (symbol + ring + badge + label) and animated effect expansions
        // so the displacement engine can compute true bounds rather than using a static guess.
        public float GetVisualRadiusWorld()
        {
            float symbolRadius = (symbol != null ? symbol.RectTransform.sizeDelta.x : (_hierarchyStyle.SizeCm / 100f)) * 0.5f;
            float maxRadius = symbolRadius * (layout != null ? layout.ringSizeRatio : 1.18f);

            // If badge is visible, badge edge extends beyond symbol radius
            if (_settings.UseBadge && badge != null)
            {
                float badgeRadius = symbolRadius * (layout != null ? layout.badgeSizeRatio : 0.36f);
                Vector2 badgeOffset = Vector2.Scale(layout != null ? layout.badgeDirection : new Vector2(0.7f, 0.7f), new Vector2(symbolRadius, symbolRadius));
                float badgeDist = badgeOffset.magnitude + badgeRadius;
                if (badgeDist > maxRadius) maxRadius = badgeDist;
            }

            // Every effect animates outward, so any active one needs the maximum expansion envelope
            if (ActiveEffectFlags != MarkerEffectFlags.None)
            {
                maxRadius *= 1.35f; // Max envelope expansion during animated peak
            }

            return maxRadius;
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
            pulseEffect?.Configure(symbol.RectTransform);

            if (rippleEffect == null && allowCreate)
                rippleEffect = GetComponent<MarkerRippleEffect>() ?? gameObject.AddComponent<MarkerRippleEffect>();
            rippleEffect?.Configure(symbol.RectTransform);

            if (haloEffect == null && allowCreate)
                haloEffect = GetComponent<MarkerHaloEffect>() ?? gameObject.AddComponent<MarkerHaloEffect>();
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
