using System;
using UnityEngine;

namespace TileStories
{
    // The Select, Filter & Search domain's config block (WallConfigData.select_filter_search,
    // _2.6_Select_Filter_Search.md section 3). One sub-block per job, and one Editor Tab
    // sub-foldout per sub-block:
    //   selection -- what a tap does (highlight dim, zoom-on-select)
    //   search    -- how a typed / spoken query matches POIs
    //   filter    -- which facets the visitor can filter by and what happens to the rest
    //   results   -- how results are shown (default view, recent searches, suggestions)
    //   minimap   -- the 2D overview of every POI
    //   voice     -- speech input
    // The keyword VOCABULARY (custom search_fields, synonym_groups, every taxonomy row's
    // search_keywords) stays on WallConfigData next to the tables it belongs to.
    [Serializable]
    public class SelectFilterSearchSettings
    {
        // Master switch: off = no search UI, no filters, no selection responders.
        public bool enabled = true;
        public SelectionSettings selection = new();
        public SearchSettings search = new();
        public FilterSettings filter = new();
        public ResultsSettings results = new();
        public MinimapSettings minimap = new();
        public VoiceSettings voice = new();
    }

    [Serializable]
    public class SelectionSettings
    {
        // Dim every other marker while one is selected
        public bool highlight_enabled = true;
        // Opacity of the non-selected markers while a selection is active (0 = gone, 1 = no dim)
        public float dim_alpha = 0.3f;
        public ZoomOnSelectSettings zoom = new();

        public const float DimAlphaMin = 0f;
        public const float DimAlphaMax = 1f;
    }

    [Serializable]
    public class ZoomOnSelectSettings
    {
        public bool enabled = true;
        // "marker" | "cluster" | "both": which tap target may zoom
        public string trigger = SelectFilterSearchOptions.TriggerMarker;
        // A tapped marker counts the other visible markers within this screen radius (about a fingertip
        // and a half: markers this close are hard to tell apart with a finger)...
        public float neighbour_radius_px = 60f;
        // ...and zooms only when at least this many are that close (a crowd worth separating)
        public int min_neighbours = 2;
        // Zoom multiplier per zoom-on-select (the Zoom section's Min/Max Zoom still clamp it)
        public float factor = 2f;

        public const float NeighbourRadiusMin = 10f;
        public const float NeighbourRadiusMax = 600f;
        public const int MinNeighboursMin = 0;
        public const int MinNeighboursMax = 20;
        public const float FactorMin = 1f;
        public const float FactorMax = 8f;
    }

    [Serializable]
    public class SearchSettings
    {
        // "dynamic" (search as you type) | "explicit" (search on Enter)
        public string mode = SelectFilterSearchOptions.ModeDynamic;
        // "all" (every word must match) | "any" (one word is enough); typed and voice alike
        public string match_mode = SelectFilterSearchOptions.MatchAll;
        // "off" | "name_only" | "all_fields": whether a partial word ("chu") matches ("church")
        public string prefix_matching = SelectFilterSearchOptions.PrefixAllFields;
        // Spelling mistakes forgiven per word: 0 = exact, 1, or 2 (long words only)
        public int typo_tolerance = 1;
        // Shown when nothing matches; "{query}" is replaced by the visitor's text
        public string no_results_message = "No matches for \"{query}\" - try removing a filter.";
        // Shown when filters alone (no typed text) leave nothing -- a {query} there would read as ""
        public string no_results_filters_message = "Nothing matches these filters - try removing one.";

        public const int TypoToleranceMax = 2;
    }

    [Serializable]
    public class FilterSettings
    {
        // Which facet groups the visitor sees in the filter tray
        public bool category_facet = true;
        public bool badge_facet = true;
        public bool status_facet = true;
        public bool hierarchy_facet = true;
        // "hide" | "dim": markers outside the result set
        public string mismatch = SelectFilterSearchOptions.MismatchHide;
        // Opacity of those markers in "dim"
        public float dim_alpha = 0.3f;
        // With no results and 2+ active filters, offer the one filter whose removal helps most
        public bool relax_suggestion = true;
    }

    [Serializable]
    public class ResultsSettings
    {
        // "list" | "minimap" | "camera_highlight"
        public string default_view = SelectFilterSearchOptions.ViewList;
        // Reopen with the visitor's last chosen view instead of default_view
        public bool remember_last_view;
        // Recent searches remembered on the device (0 = off)
        public int recent_count = 5;
        public bool suggestions_enabled = true;
        // "category_distribution" | "recent_first"
        public string suggestion_source = SelectFilterSearchOptions.SuggestCategoryDistribution;

        public const int RecentCountMax = 20;
    }

    [Serializable]
    public class MinimapSettings
    {
        public bool enabled = true;
        // "always" (always on screen) | "toggle" (a button shows / hides it)
        public string visibility = SelectFilterSearchOptions.VisibilityToggle;
        // "dots_only" | "category_colored_dots" | "mini_icons"
        public string icon_style = SelectFilterSearchOptions.IconCategoryDots;
        public float dot_size_px = 20f;
        public float tap_target_px = 44f;
        // "auto" | "wall" (x/y, a flat wall seen from the front) | "floor" (x/z, a room from above)
        public string projection = SelectFilterSearchOptions.ProjectionAuto;
        // "auto" (fit every POI) | "manual" (bounds_min / bounds_max, world metres)
        public string bounds_mode = SelectFilterSearchOptions.BoundsAuto;
        public Vector3 bounds_min;
        public Vector3 bounds_max;

        public const float DotSizeMin = 4f;
        public const float DotSizeMax = 40f;
        public const float TapTargetMin = 24f;
        public const float TapTargetMax = 88f;
    }

    [Serializable]
    public class VoiceSettings
    {
        // Needs a speech backend on the device; the mic stays hidden without one
        public bool enabled;
        // "mic_text" | "listen_bar"
        public string indicator_style = SelectFilterSearchOptions.IndicatorMicText;
    }

    // Every option string of the block and its parser, shared by the runtime and the Editor Tab
    // dropdowns so a value the editor offers is always one the runtime understands.
    public static class SelectFilterSearchOptions
    {
        public const string TriggerMarker = "marker";
        public const string TriggerCluster = "cluster";
        public const string TriggerBoth = "both";
        public static readonly string[] Triggers = { TriggerMarker, TriggerCluster, TriggerBoth };

        public const string ModeDynamic = "dynamic";
        public const string ModeExplicit = "explicit";
        public static readonly string[] Modes = { ModeDynamic, ModeExplicit };

        public const string MatchAll = "all";
        public const string MatchAny = "any";
        public static readonly string[] MatchModes = { MatchAll, MatchAny };

        public const string PrefixOff = "off";
        public const string PrefixNameOnly = "name_only";
        public const string PrefixAllFields = "all_fields";
        public static readonly string[] PrefixModes = { PrefixOff, PrefixNameOnly, PrefixAllFields };

        public const string MismatchHide = "hide";
        public const string MismatchDim = "dim";
        public static readonly string[] Mismatches = { MismatchHide, MismatchDim };

        public const string ViewList = "list";
        public const string ViewMinimap = "minimap";
        public const string ViewCameraHighlight = "camera_highlight";
        public static readonly string[] Views = { ViewList, ViewMinimap, ViewCameraHighlight };

        public const string SuggestCategoryDistribution = "category_distribution";
        public const string SuggestRecentFirst = "recent_first";
        public static readonly string[] SuggestionSources = { SuggestCategoryDistribution, SuggestRecentFirst };

        public const string VisibilityAlways = "always";
        public const string VisibilityToggle = "toggle";
        public static readonly string[] Visibilities = { VisibilityAlways, VisibilityToggle };

        public const string IconDotsOnly = "dots_only";
        public const string IconCategoryDots = "category_colored_dots";
        public const string IconMini = "mini_icons";
        public static readonly string[] IconStyles = { IconDotsOnly, IconCategoryDots, IconMini };

        public const string ProjectionAuto = "auto";
        public const string ProjectionWall = "wall";
        public const string ProjectionFloor = "floor";
        public static readonly string[] Projections = { ProjectionAuto, ProjectionWall, ProjectionFloor };

        public const string BoundsAuto = "auto";
        public const string BoundsManual = "manual";
        public static readonly string[] BoundsModes = { BoundsAuto, BoundsManual };

        public const string IndicatorMicText = "mic_text";
        public const string IndicatorListenBar = "listen_bar";
        public static readonly string[] IndicatorStyles = { IndicatorMicText, IndicatorListenBar };

        // Whether a tap on this kind of target may zoom ("both" covers both kinds)
        public static bool TriggerAllows(string trigger, bool isCluster)
        {
            string t = trigger ?? TriggerMarker;
            return t == TriggerBoth || (isCluster ? t == TriggerCluster : t == TriggerMarker);
        }

        // Typed and voice queries share this; anything unknown is the stricter "all"
        public static SearchMatchMode ParseMatchMode(string value) =>
            value == MatchAny ? SearchMatchMode.Any : SearchMatchMode.All;

        public static SearchPrefixScope ParsePrefix(string value) => value switch
        {
            PrefixOff => SearchPrefixScope.Off,
            PrefixNameOnly => SearchPrefixScope.NameOnly,
            _ => SearchPrefixScope.AllFields,
        };

        // The search options the index runs with, from the wall's search settings
        public static SearchOptions ToSearchOptions(SearchSettings s) => s == null
            ? SearchOptions.Default
            : new SearchOptions(ParseMatchMode(s.match_mode), ParsePrefix(s.prefix_matching),
                Mathf.Clamp(s.typo_tolerance, 0, SearchSettings.TypoToleranceMax));

        // The opacity a marker outside the result set gets
        public static float MismatchAlpha(FilterSettings f) =>
            f != null && f.mismatch == MismatchDim ? Mathf.Clamp01(f.dim_alpha) : 0f;
    }
}
