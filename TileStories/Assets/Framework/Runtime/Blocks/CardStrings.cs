using System.Collections.Generic;

namespace TileStories
{
    // The card's own UI texts (_3.1 step 5b, 30-ui-content.md: no visitor string in code). The framework ships a
    // default per key and language (CardStringTable, CardStrings.asset); an APP adds its own rows for the kinds it ships
    // (CardStringSources, step 11-fix); a wall rewords any of them in card_settings.strings. A text is looked up in this
    // order, so a wall's English wording never hides the framework's Portuguese one from a Portuguese visitor:
    //   wall [language] -> app [language] -> framework [language] -> wall [fallback] -> app [fallback] -> framework [fallback]
    //   -> any language the key has (wall, app, framework) -> the key
    // where `fallback` is the wall's FIRST language (CardLanguageRule). The key only shows for a key missing from every table --
    // CardStringsTests guard that for the framework's and the app's.
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
            // knowledge_check: its default heading, "Question {0} of {1}", the verdict words, the two fixed choices of a true / false
            // statement, the swipe hint under it and the two buttons that move between questions
            public const string KnowledgeCheckHeading = "knowledge_check_heading";
            public const string KnowledgeQuestionOf = "knowledge_question_of";
            public const string KnowledgeCorrect = "knowledge_correct";
            public const string KnowledgeWrong = "knowledge_wrong";
            public const string KnowledgeTrue = "knowledge_true";
            public const string KnowledgeFalse = "knowledge_false";
            public const string KnowledgeSwipeHint = "knowledge_swipe_hint";
            public const string KnowledgePrevious = "knowledge_previous";
            public const string KnowledgeNext = "knowledge_next";
            // feedback: the question each look asks when the block names none, the two thumbs, a star's name ("{0} of {1} stars")
            // and the thank-you after a vote
            public const string FeedbackThumbsQuestion = "feedback_thumbs_question";
            public const string FeedbackStarsQuestion = "feedback_stars_question";
            public const string FeedbackThumbUp = "feedback_thumb_up";
            public const string FeedbackThumbDown = "feedback_thumb_down";
            public const string FeedbackStar = "feedback_star";
            public const string FeedbackThanks = "feedback_thanks";

            // poll: "Your choice" on the option the visitor picked, the thank-you after voting, and "{0}%" for a results bar (only
            // ever drawn when an IPollResults gives numbers)
            public const string PollYourChoice = "poll_your_choice";
            public const string PollThanks = "poll_thanks";
            public const string PollPercent = "poll_percent";
            // collect: its default heading, the two states of its button, and "{0} of {1} collected" ({0} = the visitor's, {1} = this wall's)
            public const string CollectHeading = "collect_heading";
            public const string CollectAdd = "collect_add";
            public const string CollectCollected = "collect_collected";
            public const string CollectProgress = "collect_progress";
            // dialogue: the button that reveals the next line, the one that starts over, and the visitor's own name in the chat
            public const string DialogueContinue = "dialogue_continue";
            public const string DialogueAgain = "dialogue_again";
            public const string DialogueYou = "dialogue_you";
            // show_on_wall: its button, and the caption over the neighbours of the with_neighbours look
            public const string ShowOnWallButton = "show_on_wall_button";
            public const string ShowOnWallNearby = "show_on_wall_nearby";
            // actions: an Open Block button with no words of its own whose target has no heading either (_3.2 step 4)
            public const string OpenBlockButton = "open_block_button";
            // audio_guide and the mini-player (step 9A): the names of the play / pause button, the speed chip, the captions switch and the
            // scrubber (tooltips and accessible names -- the chip's own text is a number), "Up next" for an audio waiting in the queue,
            // the state of a clip that cannot be loaded, and the mini-player's way back to the card
            public const string AudioPlay = "audio_play";
            public const string AudioPause = "audio_pause";
            public const string AudioSpeed = "audio_speed";
            public const string AudioCaptions = "audio_captions";
            public const string AudioSeek = "audio_seek";
            public const string AudioQueued = "audio_queued";
            public const string AudioUnavailable = "audio_unavailable";
            public const string MiniPlayerOpen = "mini_player_open";
            // the mini-player's stop / dismiss button: ends the audio and sends the bar away
            public const string MiniPlayerStop = "mini_player_stop";
            // video (step 9B): the full-screen button (and the teaser's words), the state of a clip that cannot be loaded, the name of the
            // chapter buttons' row. Play / pause / captions / the bar reuse the audio words above
            public const string VideoFullScreen = "video_full_screen";
            public const string VideoUnavailable = "video_unavailable";
            public const string VideoChapters = "video_chapters";
            // model_3d (step 10A.2b.3): the hint under a turntable, and its words while the model is still loading
            // (a failed/missing model shows CardImage's own MediaUnavailable, on the fallback picture). The Display
            // Takeover teaser's open-full-screen button reuses VideoFullScreen's exact wording (step 10A.3.1).
            public const string Model3DHint = "model_3d_hint";
            public const string Model3DLoading = "model_3d_loading";
            // panorama_360 (step 10A.4.2): the hint under the viewer for each look (the gyro look shows the drag one where the device has no
            // motion sensor), and its words while the picture is still loading (a failed/missing panorama shows CardImage's own MediaUnavailable
            // on the fallback picture). The teaser's open-full-screen button reuses VideoFullScreen's wording, as the model's does.
            public const string Panorama360DragHint = "panorama_360_drag_hint";
            public const string Panorama360GyroHint = "panorama_360_gyro_hint";
            public const string Panorama360Loading = "panorama_360_loading";
            // place_in_ar (step 10B.2, placed state 15.2.3): its button (unless the block authors its own Button Label), the same button's words while
            // the model stands (it takes the model away), the line under it saying so, and the line under the disabled button while the wall is
            // not localised
            public const string PlaceInArButton = "place_in_ar_button";
            public const string PlaceInArRemove = "place_in_ar_remove";
            public const string PlaceInArPlaced = "place_in_ar_placed";
            public const string PlaceInArNotLocalised = "place_in_ar_not_localised";
            // The language chip beside the close button (15.3.2): its tooltip and accessible name
            public const string LanguageSwitch = "language_switch";

            // The words a visitor reads for each family of the card's ontology (_3.2 section 2.2, BuiltInFamilies).
            // One per registered family: what a navigation element labels its section with. `guides` reads "Listen"
            // because its only kind is the audio guide, and `meta` reads "Sources" (its own key stays what it is).
            public const string FamilyAbout = "family_about";
            public const string FamilyMedia = "family_media";
            public const string FamilyGuides = "family_guides";
            public const string FamilyStories = "family_stories";
            public const string FamilyAr = "family_ar";
            public const string FamilyPlay = "family_play";
            public const string FamilyVisit = "family_visit";
            public const string FamilyCommunity = "family_community";
            public const string FamilyMeta = "family_meta";
            public const string FamilyNavigation = "family_navigation";

            // The words a visitor reads for each format (_3.2 section 2.4, CardOptions.Formats): what a navigation element labels
            // its section with when the wall groups the card by format (Group By = format) instead of by family
            public const string FormatRead = "format_read";
            public const string FormatLook = "format_look";
            public const string FormatWatch = "format_watch";
            public const string FormatListen = "format_listen";
            public const string FormatModel = "format_model";
            public const string FormatInteract = "format_interact";

            // The words a visitor reads for each block KIND (BlockKindDefinition.LabelKey, _3.2 step 6): what a chip of the kind_chips
            // and icon_tabs_chips elements says. One per framework kind; an app kind names a row of its own table
            public const string KindHeader = "kind_header";
            public const string KindStatus = "kind_status";
            public const string KindQuickFacts = "kind_quick_facts";
            public const string KindRichText = "kind_rich_text";
            public const string KindFunFact = "kind_fun_fact";
            public const string KindPullQuote = "kind_pull_quote";
            public const string KindProcessSteps = "kind_process_steps";
            public const string KindSwatches = "kind_swatches";
            public const string KindActions = "kind_actions";
            public const string KindGallery = "kind_gallery";
            public const string KindBeforeAfter = "kind_before_after";
            public const string KindZoomImage = "kind_zoom_image";
            public const string KindHotspotImage = "kind_hotspot_image";
            public const string KindVideo = "kind_video";
            public const string KindModel3D = "kind_model_3d";
            public const string KindPanorama360 = "kind_panorama_360";
            public const string KindAudioGuide = "kind_audio_guide";
            public const string KindTimeline = "kind_timeline";
            public const string KindPerson = "kind_person";
            public const string KindStoryChapters = "kind_story_chapters";
            public const string KindComparePoints = "kind_compare_points";
            public const string KindDialogue = "kind_dialogue";
            public const string KindShowOnWall = "kind_show_on_wall";
            public const string KindPlaceInAr = "kind_place_in_ar";
            public const string KindKnowledgeCheck = "kind_knowledge_check";
            public const string KindPoll = "kind_poll";
            public const string KindCollect = "kind_collect";
            public const string KindPracticalInfo = "kind_practical_info";
            public const string KindWallLocator = "kind_wall_locator";
            public const string KindTodayMap = "kind_today_map";
            public const string KindRelated = "kind_related";
            public const string KindFeedback = "kind_feedback";
            public const string KindNavIconGrid = "kind_nav_icon_grid";
            public const string KindNavContents = "kind_nav_contents";
            public const string KindNavRails = "kind_nav_rails";
            public const string KindNavBento = "kind_nav_bento";
            public const string KindSources = "kind_sources";

            // The words a visitor reads for each of the framework's default navigation Groups (_3.2 section 3, the segmented element): a
            // wall's own Groups row reads its own label first; these name the four rows a wall gets with no Groups of its own
            public const string NavGroupDiscover = "nav_group_discover";
            public const string NavGroupWatch = "nav_group_watch";
            public const string NavGroupExplore = "nav_group_explore";
            public const string NavGroupVisit = "nav_group_visit";
            // The last slot of a navigation bar when the card has more sections than Bar Slots (_3.2 step 7a): it opens the rest
            public const string NavMore = "nav_more";
            // The Up next bar (_3.2 step 7b): its small line, "{0} of {1}" (place, total), its Next button, and what it says at the last block
            public const string NavNextUp = "nav_next_up";
            public const string NavNextUpPosition = "nav_next_up_position";
            public const string NavNextButton = "nav_next_button";
            public const string NavNextUpDone = "nav_next_up_done";
            // The card's Menu (_3.2 step 8): the header button's name and the menu's title; the grid's Purpose / Format switch and its way
            // back from one section's kinds to every section
            public const string NavMenu = "nav_menu";
            public const string NavMenuByPurpose = "nav_menu_by_purpose";
            public const string NavMenuByFormat = "nav_menu_by_format";
            public const string NavMenuAll = "nav_menu_all";
            // The card's floating slot (_3.2 step 9): the round button that opens the sections (fab_dial, fab_radial); the placed-model chip's
            // Remove on the card that placed the model, and its words on another point's card ({0} = that point's name; "Placed by the wall"
            // on the placing card is place_in_ar_placed)
            public const string NavFab = "nav_fab";
            public const string PlacedModelRemove = "placed_model_remove";
            public const string PlacedModelOther = "placed_model_other";
            // The contents list (nav_contents, _3.2 step 11b): a row's time ("{0} min", {0} = whole minutes) and what a row the visitor has had
            // in view is called beside its tick (its accessible name and tooltip: the tick is never the only signal)
            public const string NavContentsMinutes = "nav_contents_minutes";
            public const string NavContentsRead = "nav_contents_read";
            // A section title of the contents list when its rows collapse under it (_3.2 step 13, Rows = Sections First): how many of its
            // blocks were read ("{0} of {1} read")
            public const string NavContentsSectionRead = "nav_contents_section_read";
            // The generic compact row (_3.2 step 12, display Expandable): its second line "{0} - {1}" (format, time), the row's name while
            // collapsed / open, and the open row's full-view button
            public const string CompactMeta = "compact_meta";
            public const string CompactOpen = "compact_open";
            public const string CompactClose = "compact_close";
            public const string FullViewOpen = "full_view_open";
            // The Family Pager (_3.2 step 14, the pages layout's header): "{0} of {1}" (the section's place, how many) and its two chevrons' names
            public const string NavPagerPosition = "nav_pager_position";
            public const string NavPagerPrevious = "nav_pager_previous";
            public const string NavPagerNext = "nav_pager_next";
            // The last card of a rail that holds fewer cards than its section has blocks (nav_rails, _3.2 step 15a): "All {0}" ({0} = how many
            // blocks the section holds); a tap moves the card to the section
            public const string NavRailsAll = "nav_rails_all";

            public static readonly IReadOnlyList<string> All = new[]
            {
                Close, FunFactLabel, FunFactHint, StatusHeading, StatusUnknownMark, StatusUnknown, StatusPercent,
                SourcesHeading, SourcesVerified, SourcesDraft, TimelineNow, StoryChapterOf, StoryPrevious, StoryNext, CompareHeading,
                MediaUnavailable, HeaderThen, HeaderNow, BeforeLabel, AfterLabel, GalleryName, GalleryCount, TakeoverBack, ZoomHint,
                HotspotHint, WallLocatorHeading, WallLocatorYou,
                TodayMapHeading, TodayMapDirections, TodayMapThen, TodayMapNow, TodayMapCoordinates, RelatedHeading,
                KnowledgeCheckHeading, KnowledgeQuestionOf, KnowledgeCorrect, KnowledgeWrong, KnowledgeTrue, KnowledgeFalse, KnowledgeSwipeHint,
                KnowledgePrevious, KnowledgeNext,
                FeedbackThumbsQuestion, FeedbackStarsQuestion, FeedbackThumbUp, FeedbackThumbDown, FeedbackStar, FeedbackThanks,
                PollYourChoice, PollThanks, PollPercent, CollectHeading, CollectAdd, CollectCollected, CollectProgress,
                DialogueContinue, DialogueAgain, DialogueYou, ShowOnWallButton, ShowOnWallNearby, OpenBlockButton,
                AudioPlay, AudioPause, AudioSpeed, AudioCaptions, AudioSeek, AudioQueued, AudioUnavailable, MiniPlayerOpen,
                MiniPlayerStop, VideoFullScreen, VideoUnavailable, VideoChapters, Model3DHint, Model3DLoading,
                Panorama360DragHint, Panorama360GyroHint, Panorama360Loading, PlaceInArButton, PlaceInArRemove, PlaceInArPlaced, PlaceInArNotLocalised,
                LanguageSwitch,
                FamilyAbout, FamilyMedia, FamilyGuides, FamilyStories, FamilyAr, FamilyPlay, FamilyVisit, FamilyCommunity, FamilyNavigation, FamilyMeta,
                FormatRead, FormatLook, FormatWatch, FormatListen, FormatModel, FormatInteract,
                KindHeader, KindStatus, KindQuickFacts, KindRichText, KindFunFact, KindPullQuote, KindProcessSteps, KindSwatches,
                KindActions, KindGallery, KindBeforeAfter, KindZoomImage, KindHotspotImage, KindVideo, KindModel3D, KindPanorama360,
                KindAudioGuide, KindTimeline, KindPerson, KindStoryChapters, KindComparePoints, KindDialogue, KindShowOnWall, KindPlaceInAr,
                KindKnowledgeCheck, KindPoll, KindCollect, KindPracticalInfo, KindWallLocator, KindTodayMap, KindRelated, KindFeedback, KindNavIconGrid,
                KindNavContents, KindNavRails, KindNavBento, KindSources,
                NavGroupDiscover, NavGroupWatch, NavGroupExplore, NavGroupVisit,
                NavMore, NavNextUp, NavNextUpPosition, NavNextButton, NavNextUpDone,
                NavMenu, NavMenuByPurpose, NavMenuByFormat, NavMenuAll,
                NavFab, PlacedModelRemove, PlacedModelOther,
                NavContentsMinutes, NavContentsRead, NavContentsSectionRead,
                CompactMeta, CompactOpen, CompactClose, FullViewOpen,
                NavPagerPosition, NavPagerPrevious, NavPagerNext,
                NavRailsAll,
            };
        }

        private readonly IReadOnlyList<CardStringEntry> _framework;
        private readonly IReadOnlyList<CardStringEntry> _app;
        private readonly IReadOnlyList<CardStringEntry> _wall;
        private readonly string _language;
        private readonly string _fallback;

        // Any table may be null (nothing from that layer)
        public CardStrings(IReadOnlyList<CardStringEntry> framework, IReadOnlyList<CardStringEntry> app, IReadOnlyList<CardStringEntry> wall,
            string language, string fallbackLanguage)
        {
            _framework = framework;
            _app = app;
            _wall = wall;
            _language = language;
            _fallback = fallbackLanguage;
        }

        // The visitor's language and the one read when a text has none in it: what a wall's own per-language words are picked by
        public string Language => _language;
        public string FallbackLanguage => _fallback;

        public string Get(string key) => In(key, _language) ?? In(key, _fallback) ?? InAnyLanguage(key) ?? key;

        // The text of `key` in one language: the wall's wording first, then the app's, then the framework's
        private string In(string key, string language) =>
            Find(_wall, key, language) ?? Find(_app, key, language) ?? Find(_framework, key, language);

        // The last resort before the raw key: the first language the key has any text in (the same third step as BlockFieldReader.Pick)
        private string InAnyLanguage(string key) => FindAny(_wall, key) ?? FindAny(_app, key) ?? FindAny(_framework, key);

        private static string FindAny(IReadOnlyList<CardStringEntry> table, string key)
        {
            if (table == null || key == null) return null;
            for (int i = 0; i < table.Count; i++)
            {
                var entry = table[i];
                if (entry == null || entry.key != key || entry.text == null) continue;
                foreach (var t in entry.text)
                    if (t != null && !string.IsNullOrWhiteSpace(t.value)) return t.value;
            }
            return null;
        }

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
