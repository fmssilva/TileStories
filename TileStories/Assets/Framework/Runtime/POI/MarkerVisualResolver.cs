using UnityEngine;

namespace TileStories
{
    public enum SymbolBackgroundState { Untouched, Hidden, Visible }

    public enum BadgeSource { Hidden, UnknownStatus, Category, StatusFallback }

    // What one marker should look like, as plain values (no sprites, no scene). MarkerView applies it.
    public struct MarkerVisualState
    {
        public bool HasConfiguredCategory;
        public SymbolBackgroundState Background;
        public bool DrawShapeFill;          // paint the shape sprite with SymbolFill
        public Color SymbolFill;
        public bool ShowIcon;
        public string IconKey;
        public float IconOpacity;

        public bool ShowRing;
        public StatusLevel RingLevel;
        public bool RingUsesCategoryHue;
        public Color RingHueColor;

        public BadgeSource Badge;
        public Color BadgeColor;
        public string BadgeIconKey;         // null = reuse the symbol's icon
    }

    // The marker visual rules in one pure place (_2.2.1 section 4): category colour and icon,
    // status ring, badge and the same-hue fill. Static palettes are read, nothing is written.
    public static class MarkerVisualResolver
    {
        public const string FallbackUnknownIconKey = "unknown";
        public const string FallbackUnknownBadgeCategoryKey = "unknown_damage";
        public const string FallbackUnknownStatusLevelKey = "unknown";

        public static MarkerVisualState Resolve(POIData poi, MarkerVisualSettings settings)
        {
            var state = new MarkerVisualState { IconOpacity = 1f };
            if (poi == null || settings == null) return state;

            bool hasCategory = CategoryPalette.TryResolveConfigured(poi.category, out Color categoryColor, out string categoryIconKey);
            bool isUnknown = poi.has_status && poi.status_unknown;
            bool knownStatus = poi.has_status && !isUnknown;
            bool sameHueFill = settings.OutlineMode == MarkerOutlineMode.SameHue && knownStatus;
            StatusLevel knownLevel = default;
            float statusPct = poi.status_pct;
            if (knownStatus)
                statusPct = ResolveKnownLevel(poi, out knownLevel);

            // Symbol: coloured by category; a custom symbol replaces just the icon
            state.HasConfiguredCategory = hasCategory;
            state.IconKey = poi.has_custom_symbol && !string.IsNullOrWhiteSpace(poi.custom_symbol_key)
                ? poi.custom_symbol_key
                : categoryIconKey;
            state.SymbolFill = sameHueFill ? StatusRamp.ShadeTowardBlack(categoryColor, statusPct) : categoryColor;
            state.IconOpacity = sameHueFill ? Mathf.Lerp(1f, 0.28f, Mathf.Clamp01(statusPct / 100f)) : 1f;

            bool paintCategory = settings.HasCategoryDefinitions && hasCategory;
            state.ShowIcon = paintCategory;
            if (settings.Shape == MarkerShape.None)
                state.Background = SymbolBackgroundState.Hidden;
            else if (paintCategory || settings.HasShapeFromConfig)
            {
                state.Background = SymbolBackgroundState.Visible;
                state.DrawShapeFill = settings.HasShapeFromConfig && hasCategory;
            }

            // Ring: needs a status axis and an outline mode; same-hue also needs a known category
            bool canRing = settings.HasOutlineLevels && poi.has_status && settings.OutlineMode != MarkerOutlineMode.None
                && (settings.OutlineMode != MarkerOutlineMode.SameHue || hasCategory);
            StatusLevel unknownLevel = default;
            bool hasUnknownLevel = isUnknown && TryResolveUnknownLevel(poi, out unknownLevel);
            state.ShowRing = canRing && (knownStatus || hasUnknownLevel);
            if (state.ShowRing)
            {
                state.RingLevel = knownStatus ? knownLevel : unknownLevel;
                state.RingUsesCategoryHue = sameHueFill;
                if (sameHueFill) state.RingHueColor = ShadeRingTowardBlack(categoryColor, statusPct);
            }

            // Badge: hidden without a status axis (_2.2.2 rule 1 -- a badge category left over after
            // "Has status" was switched off must not keep showing); the "?" for an unknown status
            // always wins; then the POI's badge category; then the status colour with the symbol icon
            if (!poi.has_status)
            {
                state.Badge = BadgeSource.Hidden;
            }
            else if (settings.HasOutlineLevels && isUnknown)
            {
                var def = ResolveUnknownBadge(poi);
                state.Badge = BadgeSource.UnknownStatus;
                state.BadgeColor = def.Color;
                state.BadgeIconKey = def.IconKey;
            }
            else if (settings.HasOutlineLevels && settings.UseBadge && !string.IsNullOrWhiteSpace(poi.badge_category)
                && BadgeCategoryPalette.TryResolve(poi.badge_category, out var badgeDef))
            {
                state.Badge = BadgeSource.Category;
                state.BadgeColor = badgeDef.Color;
                state.BadgeIconKey = badgeDef.IconKey;
            }
            else if (settings.HasOutlineLevels && settings.UseBadge && knownStatus)
            {
                state.Badge = BadgeSource.StatusFallback;
                state.BadgeColor = knownLevel.RingColor;
            }
            return state;
        }

        // A known status is the Outline Types row the developer picked (status_level_key): two rows
        // can share a percentage (e.g. a "destroyed" and an "unknown" row both at 100), so snapping the
        // percentage alone drew the wrong row. Only a POI with no (or a stale) key falls back to the
        // nearest percentage (the "Status %" slider of a wall with no Outline Types rows yet).
        // Returns the percentage that goes with the resolved level (for the same-hue shading).
        private static float ResolveKnownLevel(POIData poi, out StatusLevel level)
        {
            if (!string.IsNullOrWhiteSpace(poi.status_level_key) && StatusRamp.TryResolveByKey(poi.status_level_key, out level))
                return level.Pct;
            level = StatusRamp.Resolve(poi.status_pct);
            return poi.status_pct;
        }

        // Same-hue ring stays in the category hue family while darkening with severity
        public static Color ShadeRingTowardBlack(Color categoryColor, float pct)
        {
            float t = Mathf.Clamp01(pct / 100f);
            Color shaded = Color.Lerp(categoryColor, Color.black, 0.62f * t);
            shaded.a = 1f;
            return shaded;
        }

        private static BadgeCategoryPalette.BadgeDefinition ResolveUnknownBadge(POIData poi)
        {
            if (!string.IsNullOrWhiteSpace(poi.badge_category) &&
                BadgeCategoryPalette.TryResolve(poi.badge_category, out var selected))
                return selected;

            if (BadgeCategoryPalette.TryResolve(FallbackUnknownBadgeCategoryKey, out var fallback))
                return fallback;

            return new BadgeCategoryPalette.BadgeDefinition(StatusRamp.UnknownColor, FallbackUnknownIconKey);
        }

        private static bool TryResolveUnknownLevel(POIData poi, out StatusLevel level)
        {
            if (!string.IsNullOrWhiteSpace(poi.status_level_key) && StatusRamp.TryResolveByKey(poi.status_level_key, out level))
                return true;
            if (StatusRamp.TryResolveByKey(FallbackUnknownStatusLevelKey, out level))
                return true;
            level = StatusRamp.UnknownFallbackLevel;
            return true;
        }
    }
}
