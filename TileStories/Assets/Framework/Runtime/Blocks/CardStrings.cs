using System.Collections.Generic;

namespace TileStories
{
    // The card's own UI texts (_3.1 step 5b, 30-ui-content.md: no visitor string in code). The framework ships a
    // default per key and language (CardStringTable, CardStrings.asset); a wall rewords any of them in
    // card_settings.strings. A text is looked up in this order, so a wall's English wording never hides the
    // framework's Portuguese one from a Portuguese visitor:
    //   wall [language] -> framework [language] -> wall [fallback] -> framework [fallback] -> the key itself
    // (the key only shows for a key missing from the framework table -- CardStringsTests guard that).
    public sealed class CardStrings
    {
        // Every key a card view reads. A key is added with the view that first shows it.
        public static class Keys
        {
            // The close button's name (its tooltip / accessible name; the X itself is drawn by PoiCard.uss)
            public const string Close = "close";
            // The small heading of a fun_fact card
            public const string FunFactLabel = "fun_fact_label";
            // The flip look's invitation to tap
            public const string FunFactHint = "fun_fact_hint";

            // The status block's small heading
            public const string StatusHeading = "status_heading";
            // The mark inside the ring of a condition nobody assessed
            public const string StatusUnknownMark = "status_unknown_mark";
            // The words for an unknown condition when the wall's Outline Types have no row to name it
            public const string StatusUnknown = "status_unknown";
            // "{0}% damaged": a known condition the wall's Outline Types do not name ({0} = the percentage)
            public const string StatusPercent = "status_percent";

            // The sources block's default heading (BlockKindDefinition.DefaultHeadingKey), and its confidence chip for verified / draft content
            public const string SourcesHeading = "sources_heading";
            public const string SourcesVerified = "sources_verified";
            public const string SourcesDraft = "sources_draft";

            // The end point a timeline adds after its last event when Highlight Now is on
            public const string TimelineNow = "timeline_now";

            // "Chapter {0} of {1}" above a story chapter ({0} = this chapter, {1} = how many), and the two buttons under it
            public const string StoryChapterOf = "story_chapter_of";
            public const string StoryPrevious = "story_previous";
            public const string StoryNext = "story_next";

            // The default heading of a compare_points block (the two conditions side by side; DefaultHeadingKey)
            public const string CompareHeading = "compare_heading";

            // What a picture's frame says when its file cannot be loaded (missing, or a path the card may not use)
            public const string MediaUnavailable = "media_unavailable";
            // The two halves of a split_then_now header picture
            public const string HeaderThen = "header_then";
            public const string HeaderNow = "header_now";
            // The two sides of a before_after slider when the block names none
            public const string BeforeLabel = "before_label";
            public const string AfterLabel = "after_label";
            // A gallery's name in the full-screen view's breadcrumb ("St George's Castle > Gallery")
            public const string GalleryName = "gallery_name";
            // "{0} pictures" under a stacked gallery ({0} = how many)
            public const string GalleryCount = "gallery_count";
            // The full-screen view's way back to the card
            public const string TakeoverBack = "takeover_back";
            // The hint under a zoom_image picture
            public const string ZoomHint = "zoom_hint";
            // The hint under a hotspot_image picture (tap a spot)
            public const string HotspotHint = "hotspot_hint";
            // wall_locator: its default heading, and the visitor's own place on the strip
            public const string WallLocatorHeading = "wall_locator_heading";
            public const string WallLocatorYou = "wall_locator_you";
            // today_map: its default heading, the button, the bridge's two sides, and how two coordinates are written
            public const string TodayMapHeading = "today_map_heading";
            public const string TodayMapDirections = "today_map_directions";
            public const string TodayMapThen = "today_map_then";
            public const string TodayMapNow = "today_map_now";
            public const string TodayMapCoordinates = "today_map_coordinates";
            // related: its default heading
            public const string RelatedHeading = "related_heading";

            public static readonly IReadOnlyList<string> All = new[]
            {
                Close, FunFactLabel, FunFactHint, StatusHeading, StatusUnknownMark, StatusUnknown, StatusPercent,
                SourcesHeading, SourcesVerified, SourcesDraft, TimelineNow, StoryChapterOf, StoryPrevious, StoryNext, CompareHeading,
                MediaUnavailable, HeaderThen, HeaderNow, BeforeLabel, AfterLabel, GalleryName, GalleryCount, TakeoverBack, ZoomHint,
                HotspotHint, WallLocatorHeading, WallLocatorYou,
                TodayMapHeading, TodayMapDirections, TodayMapThen, TodayMapNow, TodayMapCoordinates, RelatedHeading,
            };
        }

        private readonly IReadOnlyList<CardStringEntry> _framework;
        private readonly IReadOnlyList<CardStringEntry> _wall;
        private readonly string _language;
        private readonly string _fallback;

        public CardStrings(IReadOnlyList<CardStringEntry> framework, IReadOnlyList<CardStringEntry> wall, string language, string fallbackLanguage)
        {
            _framework = framework;
            _wall = wall;
            _language = language;
            _fallback = fallbackLanguage;
        }

        public string Get(string key) =>
            Find(_wall, key, _language) ?? Find(_framework, key, _language)
            ?? Find(_wall, key, _fallback) ?? Find(_framework, key, _fallback)
            ?? key;

        // The non-blank text of `key` in `language` in one table, or null
        public static string Find(IReadOnlyList<CardStringEntry> table, string key, string language)
        {
            if (table == null || key == null || language == null) return null;
            for (int i = 0; i < table.Count; i++)
            {
                var entry = table[i];
                if (entry == null || entry.key != key || entry.text == null) continue;
                foreach (var t in entry.text)
                    if (t != null && t.lang == language && !string.IsNullOrWhiteSpace(t.value)) return t.value;
            }
            return null;
        }
    }
}
