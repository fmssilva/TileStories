// POIEditorToolWindow.SearchFilterHelp.cs
//
// Partial: every text of the Select, Filter & Search section (_2.6_Select_Filter_Search.md) -- option
// labels, (i) help bodies and the three Scene / Playmode / Device Test guides (_5.1_Editor_Tab.md, "Domain
// Manual Tests"). Option VALUES come from SelectFilterSearchOptions (runtime), so the editor can only offer
// values the runtime understands. Every text is ASCII, app-agnostic and names Editor Tab controls only
// (SearchFilterEditorTabTests pins it).

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // ---- option labels, index-aligned with the SelectFilterSearchOptions value arrays ----

        private static readonly string[] ZoomTriggerLabels = { "Marker", "Cluster", "Marker or Cluster" };
        private static readonly string[] SearchModeLabels = { "As you type", "On Enter" };
        private static readonly string[] MatchModeLabels = { "Every word", "Any word" };
        private static readonly string[] PrefixModeLabels = { "Off", "Names only", "All fields" };
        private static readonly string[] MismatchLabels = { "Hide", "Dim" };
        private static readonly string[] ResultViewLabels = { "List", "Map", "Highlight" };
        private static readonly string[] SuggestionSourceLabels = { "Categories", "Recent first" };
        private static readonly string[] MinimapVisibilityLabels = { "Always", "Button" };
        private static readonly string[] MinimapIconLabels = { "Plain dots", "Category colours", "Category icons" };
        private static readonly string[] MinimapProjectionLabels = { "Auto", "Wall (front)", "Floor (top-down)" };
        private static readonly string[] MinimapBoundsLabels = { "Fit all POIs", "Manual" };
        private static readonly string[] VoiceIndicatorLabels = { "Mic text", "Listen bar" };

        // ---- (i) help ----

        private static readonly string SearchEnabledHelp =
            "Lets visitors find points of interest: tap a marker to select it, type (or say) what they look for, filter by " +
            "category, badge, status and level, and see the results as a list, on a small map, or highlighted on the " +
            "markers themselves.\n\nOff: no search bar, no filters, and tapping a marker does nothing. The settings below " +
            "are kept.";

        private static readonly string HighlightSelectionHelp =
            "While a marker is selected, every other marker fades to 'Dim Others To', so the selected one stands out. Tap " +
            "it again (or close its card) to clear the selection.";

        private static readonly string DimOthersHelp =
            "How visible the other markers stay while one is selected: 0 hides them, 1 does not dim at all.";

        private static readonly string ZoomOnSelectHelp =
            "Tapping a marker that sits in a crowd zooms the camera in, so the crowd separates and the next tap is precise. " +
            "A marker on its own never zooms. Min Zoom and Max Zoom of the Zoom section still limit it.";

        private static readonly string ZoomTriggerHelp =
            "Which tap may zoom: a marker in a crowd, a cluster (a group of markers shown as one round aggregate, created " +
            "by LOD), or both. A cluster always counts as a crowd.";

        private static readonly string CrowdRadiusHelp =
            "A tapped marker counts the other visible markers closer than this on screen (pixels). Screen pixels, like the " +
            "LOD and Displacement distances.";

        private static readonly string MinNeighboursHelp =
            "How many markers must be that close before a tap zooms. 0 = every tap zooms.";

        private static readonly string ZoomFactorHelp =
            "How much one zoom-on-select magnifies: 2 doubles the current zoom.";

        private static readonly string SearchModeHelp =
            "As you type: results follow the text after a short pause in typing. On Enter: results appear only when the " +
            "visitor presses Enter (useful for a very large wall).";

        private static readonly string MatchModeHelp =
            "For a query of several words. Every word: a point must match all of them (precise). Any word: one is enough " +
            "(finds more). Typed and spoken queries alike.";

        private static readonly string PrefixModeHelp =
            "Whether an unfinished word finds the whole word while typing ('chu' finds 'church'). Names only: only in " +
            "names. All fields: also in keywords, summaries and taxonomy words. A partial word needs 2 letters or more and " +
            "ranks a little lower than a whole word.";

        private static readonly string TypoToleranceHelp =
            "Spelling mistakes forgiven per word. A swapped pair of letters counts as one mistake. Words of 4+ letters may " +
            "have 1, words of 8+ letters may have 2 (when set to 2). 0 = exact words only. A corrected match ranks lower " +
            "than an exact one.";

        private static readonly string NoResultsHelp =
            "Shown when a typed (or spoken) search matches nothing. {query} is replaced by what the visitor searched.";

        private static readonly string NoResultsFiltersHelp =
            "Shown when the visitor typed nothing and the ticked filters alone leave no point (there is no query to " +
            "quote, so it has no {query}).";

        private static readonly string FacetGroupsHelp =
            "Which filter groups the visitor sees in the Filters panel. Each group lists the rows of its table (Marker > " +
            "Category Symbols, Badge Categories, Outline Types, Hierarchy Levels) by their label. Within a group any ticked " +
            "value passes; across groups every group must pass. A Keyword Field with Filter ticked (Keywords & Synonyms " +
            "below) adds its own group.";

        private static readonly string MismatchHelp =
            "What happens to markers outside the search / filter results. Hide: they disappear until the search is cleared. " +
            "Dim: they stay faintly visible, so the wall keeps its context.";

        private static readonly string DimFilteredHelp =
            "How visible filtered-out markers stay with Filtered-Out Markers = Dim.";

        private static readonly string RelaxHelp =
            "When nothing matches and two or more filters are on, the results list offers one button that removes the " +
            "filter whose removal brings back the most results.";

        private static readonly string DefaultViewHelp =
            "Where results appear first. List: a list at the bottom. Map: the results on the minimap. Highlight: no panel, " +
            "the markers themselves show the results (see Filtered-Out Markers). The visitor can switch while searching.";

        private static readonly string RememberViewHelp =
            "On: the app reopens with the view this device chose last time. Off: always Default View.";

        private static readonly string RecentSearchesHelp =
            "How many searches this device remembers (a search is remembered when the visitor presses Enter or picks a " +
            "result). 0 = none. They show as suggestions under the empty search field.";

        private static readonly string SuggestionsHelp =
            "Terms offered under the empty search field: the wall's categories, most used first, and (Recent first) the " +
            "device's recent searches before them.";

        private static readonly string MinimapEnabledHelp =
            "A small map of every point of interest. Tapping a dot selects that point like tapping its marker. During a " +
            "search only the results keep their dot; the selected dot grows.";

        private static readonly string MinimapVisibilityHelp =
            "Always: the map is always on screen. Button: a Map button next to the search field shows and hides it. With " +
            "Default View = Map it also opens while results are shown.";

        private static readonly string MinimapIconHelp =
            "Plain dots: all dots the same colour. Category colours: each dot in its category colour. Category icons: the " +
            "category's colour and icon.";

        private static readonly string MinimapDotSizeHelp =
            "The visible dot, in screen pixels. Small keeps a dense map readable; the tap area is Tap Target.";

        private static readonly string MinimapTapTargetHelp =
            "The invisible tap area around each dot, in screen pixels (44 is the usual accessible minimum). Where tap areas " +
            "overlap, the nearest dot wins.";

        private static readonly string MinimapProjectionHelp =
            "Which view of the points the map draws. Wall (front): as seen facing a flat wall (left-right and up-down). " +
            "Floor (top-down): as seen from above (left-right and near-far), for a room or a site. Auto: the two directions " +
            "in which the points spread most.";

        private static readonly string MinimapBoundsHelp =
            "Which area fills the map. Fit all POIs: every point with a small margin. Manual: the box between Bounds Min " +
            "and Bounds Max (scene metres), for a map that must stay fixed when points are added.";

        private static readonly string VoiceEnabledHelp =
            "A mic button next to the search field; what the visitor says is searched like typed text (Match Words applies). " +
            "It needs a speech backend on the device; without one the mic stays hidden. In the Editor the mic does not " +
            "listen: it 'hears' the text of Try a Query (Test below).";

        private static readonly string VoiceIndicatorHelp =
            "How listening is shown. Mic text: the mic button reads '...'. Listen bar: a thin bar appears under the search " +
            "bar.";

        private static readonly string KeywordFieldsHelp =
            "Search keywords come from four places, all searchable at once:\n" +
            "- each table's Search Keywords column (Category Symbols, Badge Categories, Outline Types, Hierarchy Levels): " +
            "every point in that row gets them;\n" +
            "- the Keyword Fields below: your own search topics (for example an architect or a period); each point gets " +
            "one keyword list per field in Specific Marker > Summary & Keywords;\n" +
            "- each point's Others (freeform) keywords;\n" +
            "- Synonym Groups: words that mean the same thing.\n" +
            "Ranking: a point's name first, then its own keywords, synonyms, its summary, then words it inherits from its " +
            "table rows.";

        private static readonly string KeywordFieldKeyHelp =
            "Stable id of the field; each point's keyword list for this field is stored under it. Renaming it here moves " +
            "every point's list along.";

        private static readonly string KeywordFieldLabelHelp = "The name shown above this field in each point's Search Keywords.";

        private static readonly string KeywordFieldRequiredHelp =
            "Required: a point with no keyword for this field gets a warning when the configuration is checked (on load and " +
            "on Save All to JSON). It never blocks saving.";

        private static readonly string KeywordFieldFilterHelp =
            "Filter: the visitor's Filters panel gets a group named after this field, with one value per keyword the " +
            "points hold in it (for example a Period field gives Baroque, Gothic...). Spelling differences of case and " +
            "spaces count as one value. Off: the field is searched but not a filter.";

        private static readonly string SynonymGroupsHelp =
            "Words that mean the same thing, as one group: a point that matches any word of a group also matches every other " +
            "word of it, both ways ('church' finds a point tagged 'chapel' and 'chapel' finds 'church'). A synonym match " +
            "ranks just below the point's own keywords.";

        private static readonly string SearchDemoHelp =
            "Developer only (Play Mode in the Editor and development builds; release builds ignore it). Puts a wall of " +
            "generated points on its own stage in front of the camera, one column per category, cycling through every " +
            "level, badge and status so every filter has points to act on, plus a demo Material keyword field shown as " +
            "a filter group. The wall's own points pause while it is on. Untick it before a normal build.";

        private static readonly string SearchDemoPerCategoryHelp = "Generated points per category column.";

        private static readonly string SearchDemoTestCasesHelp =
            "Adds named test points, a tight clump of five 'Crowd' points (Zoom on Select, clusters with Run LOD) and a " +
            "demo synonym group, and a test case per search behaviour: a query and the point it must find -- or must not " +
            "find, under the current Typo Tolerance, Partial Words or Match Words. The Live Search Readout judges every " +
            "case live ('Demo Test Cases'), and Demo Query runs one.";

        private static readonly string DemoQueryHelp =
            "Play Mode, with the demo's Test Cases: searches the chosen test case's query as if the visitor typed it. " +
            "The Live Search Readout says what it found and whether that is what the current settings should give.";

        private static readonly string SearchDemoLabelsHelp = "Every demo point shows its text label (off: none do).";
        private static readonly string SearchDemoDistanceHelp = "Distance from the camera to the demo wall, in metres.";
        private static readonly string SearchDemoSpacingHelp = "Distance between neighbouring demo points, in centimetres (the clump ignores it).";

        private static readonly string SearchDemoRunLodHelp =
            "Off: every demo point stays visible, so every search result can be seen. On: LOD acts on the demo too (the clump " +
            "can become a cluster, which Zoom Trigger = Cluster lets you tap).";

        private static readonly string TryQueryHelp =
            "Play Mode: searches this text in the running app exactly as if the visitor typed it, so you can test from this " +
            "window. The Live Search Readout below shows the result. In the Editor this text is also what the mic 'hears' " +
            "(Enable Voice Search).";

        private static readonly string SearchReadoutHelp =
            "Play Mode: what the running search shows right now -- the query, the active filters, how many points match and " +
            "the best matches with their score and why they matched (name, keyword, synonym, summary, taxonomy; partial " +
            "word or typo). Scores: a name 1.0, own keywords 0.7, synonyms 0.6, summary 0.4, taxonomy words 0.3; a partial " +
            "word x0.9, one typo x0.8, two typos x0.6. With the demo's Test Cases, 'Demo Test Cases' ends the readout: one " +
            "line per case, [ok] when the search does what the current settings should give, [!!] when it does not.";

        // ---- per point: Specific Marker > Summary & Keywords ----

        private static readonly string PoiSummaryHelp =
            "A short description of this point: shown on its card when the visitor selects it, and searchable (a summary " +
            "word ranks below the point's name and own keywords).";

        private static readonly string PoiKeywordFieldHelp =
            "This point's keywords for one of your Keyword Fields (Global Scene > Select, Filter & Search > Keywords & " +
            "Synonyms), comma-separated. With that field's Filter ticked, each keyword is also a filter value visitors " +
            "can tick.";

        private static readonly string PoiOthersKeywordsHelp =
            "Any other words this point should be found by, comma-separated (for example a nickname or a former name).";

        private static readonly string PoiFoundByHelp =
            "Every word the search finds this point by, read-only, from strongest to weakest: its name, its keywords " +
            "(each Keyword Field and Others), its summary, the rows it belongs to (Category, Badge, Status, Level: the " +
            "row's name and that row's Search Keywords) and the synonyms of all of these. It is the exact list the " +
            "running search uses.";

        // ---- the three Test guides ----

        private static readonly string SearchSceneTestGuide =
            "SETUP\n" +
            "- Nothing of this section shows in the Scene view: search, filters, the map and selection only run in Play Mode.\n\n" +
            "SELECTION\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "SEARCH\n" +
            "- Not possible in Scene test, use Play Mode.\n" +
            "- What you can check here: open Specific Marker > a point > Summary & Keywords. 'Found by' lists every word " +
            "the search finds that point by -- its name, keywords, summary, its table rows and synonyms -- and changes as " +
            "you type.\n\n" +
            "FILTERS\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "RESULTS AND VIEWS\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "MINIMAP\n" +
            "- Not possible in Scene test, use Play Mode.\n\n" +
            "VOICE\n" +
            "- Not possible in Scene test, use a device (Device guide).\n\n" +
            "KEYWORDS AND SYNONYMS\n" +
            "- Type keywords in a table's Search Keywords column, a Keyword Field or a point's Others; confirm Save All to " +
            "JSON keeps them after Load and Populate Rig.";

        private static readonly string SearchPlaymodeTestGuide =
            "SETUP\n" +
            "- Enable Select & Search on. Save All to JSON, then Copy to StreamingAssets (Play reads that copy). Open the " +
            "wall scene and press Play.\n" +
            "- Every row of this section is LIVE in Play Mode: edit it and the running app follows at once.\n" +
            "- Mock camera (Editor only): WASD move, Q/E down/up, right mouse or Alt+left mouse look, Z/C roll. The Game view " +
            "never rotates; the world turns around you.\n\n" +
            "DEMO CONTROLS\n" +
            "- Tick 'Add search demo': a wall of generated points appears in front of the camera (the wall's own points " +
            "pause). Markers per Category sets each column's height; Distance (m) and Spacing (cm) move and space them; " +
            "Show labels shows their names.\n" +
            "- Test Cases adds the named test points, the demo synonym group and one test case per search behaviour: " +
            "'cafe alvaro' (accents), 'lantren' (one typo), 'obsrevatroy' (two typos), 'lant' and 'grani' (partial words), " +
            "'stone lantern' (two words), 'church' (a synonym), 'granite' (a keyword field), 'spring' (a summary word), " +
            "'crowd' (the five-point clump).\n" +
            "- The Live Search Readout ends with 'Demo Test Cases': one [ok] line per case. Change Typo Tolerance, Partial " +
            "Words or Match Words and watch the lines that depend on it flip between 'finds' and 'does not find', still " +
            "[ok]. A [!!] line means the search does not do what that setting promises.\n" +
            "- Demo Query runs one test case; Try a Query searches any text from this window.\n" +
            "- Run LOD on the demo: LOD acts on the demo too, so the clump can become a cluster.\n\n" +
            "SELECTION\n" +
            "- Click a marker in the Game view: its card opens at the bottom and, with Highlight Selection, the others fade " +
            "to Dim Others To. Click it again (or X on the card): all back to full.\n" +
            "- Zoom on Select: click one of the demo's 'Crowd' points -- the view zooms in by Zoom Factor. Click a lone point -- " +
            "no zoom. Lower Crowd Radius (px) or raise Min Neighbours above the clump -- no zoom.\n" +
            "- Zoom Trigger: Marker (a crowded marker zooms), Cluster (only a cluster zooms; tick Run LOD on the demo to get " +
            "one), Marker or Cluster (both).\n\n" +
            "SEARCH\n" +
            "- Type in the search bar at the top of the Game view: the list, the map and the markers follow.\n" +
            "- Typo Tolerance: 0 = 'lantren' finds nothing, 1 = it finds 'Lantern Tower', 2 = 'obsrevatroy' also finds " +
            "'Observatory Hill'.\n" +
            "- Partial Words: type 'lant' -- Off finds nothing, Names only and All fields find 'Lantern Tower'; 'grani' only " +
            "with All fields (a keyword).\n" +
            "- Match Words: 'stone lantern' -- Every word finds nothing, Any word finds 'Stone Gate' and 'Lantern Tower'.\n" +
            "- Search Mode: As you type follows the typing; On Enter changes nothing until Enter.\n" +
            "- Type a word nothing has: the list shows the No-Results Message. Tick two filters that share no point with " +
            "the field empty: it shows No-Results (Filters) instead.\n\n" +
            "FILTERS\n" +
            "- Press Filters next to the search field and tick values: the list, the map and the markers all narrow at once. " +
            "Values within one group add up, groups narrow each other.\n" +
            "- Filtered-Out Markers: Hide removes the others, Dim keeps them at Dim Filtered To.\n" +
            "- Tick two values that share no point: the list shows the No-Results Message and, with Relax Suggestion, one " +
            "'Remove filter' button that brings results back.\n" +
            "- Untick Category Filter, Badge Filter, Status Filter or Level Filter: that group leaves the Filters panel.\n" +
            "- Tick Filter on a Keyword Field (Keywords & Synonyms): a group with that field's name joins the Filters " +
            "panel, one value per keyword the points hold in it.\n\n" +
            "RESULTS AND VIEWS\n" +
            "- With a search or filter active, List / Map / Highlight appear under the search bar. List: a list at the " +
            "bottom (click a row = click its marker). Map: results on the minimap. Highlight: no panel, only the markers.\n" +
            "- Default View sets the first one. Remember Last View keeps the visitor's last choice on this device.\n" +
            "- Click the empty search field: Suggestions. Suggestion Source: Categories (most used first) or Recent first " +
            "(your last searches first). Recent Searches sets how many are remembered (0 = none).\n\n" +
            "MINIMAP\n" +
            "- Enable Minimap on. Visibility: Always (always on screen) or Button (a Map button next to Filters). Click a dot: " +
            "that point is selected. During a search only the results keep a dot.\n" +
            "- Dot Style: Plain dots, Category colours or Category icons. Dot Size (px) and Tap Target (px) change the dots at once.\n" +
            "- Projection: Auto, Wall (front) for a flat wall, Floor (top-down) for a room; compare where the dots sit with " +
            "the scene. Bounds: Fit all POIs, or Manual with Bounds Min (m) and Bounds Max (m).\n\n" +
            "VOICE\n" +
            "- Enable Voice Search: a Mic button appears. In the Editor it does not listen: it searches the Try a Query text " +
            "(type 'lantern' there, then click the Mic).\n" +
            "- Voice Indicator: Mic text (the button reads '...') or Listen bar (a thin bar under the search bar).\n\n" +
            "KEYWORDS AND SYNONYMS\n" +
            "- Add a keyword to a point's Others, then search it: it is found at once. Add a Synonym Group (a word and the " +
            "words of the same meaning): searching any of them finds points holding another.\n\n" +
            "TEST RUNNER\n" +
            "- Window > General > Test Runner: EditMode and PlayMode, zero failures.";

        private static readonly string SearchDeviceTestGuide =
            "SETUP\n" +
            "- Untick 'Add search demo' for a normal build (a development build would otherwise show the demo).\n" +
            "- Save All to JSON, Copy to StreamingAssets, then Build And Run with the device connected (USB, or Wi-Fi: " +
            "adb pair <ip>:<port> with the pairing code, then adb connect <ip>:<port>).\n" +
            "- Logs: adb logcat -c, then adb logcat -s Unity > __logcat.txt, and read the file.\n\n" +
            "SELECTION\n" +
            "- Tap markers with a finger: the card opens and the others fade; tap again to clear. Tap one in a crowd: the " +
            "camera zooms in. Check a small or distant marker is still easy to hit.\n\n" +
            "SEARCH\n" +
            "- Type with the phone keyboard: results follow the typing without lag.\n\n" +
            "FILTERS, RESULTS AND VIEWS, MINIMAP\n" +
            "- Every button, chip, row and map dot is comfortable to tap one-handed, and text stays readable outdoors in sun.\n\n" +
            "VOICE\n" +
            "- Only with a speech backend added to the app: the permission prompt appears once, speaking a point's name " +
            "finds it. Without a backend the mic must not appear.\n\n" +
            "KEYWORDS AND SYNONYMS\n" +
            "- Nothing device-specific: same as Play Mode.";
    }
}
