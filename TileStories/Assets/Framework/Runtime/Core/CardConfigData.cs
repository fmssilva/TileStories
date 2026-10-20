using System;
using System.Collections.Generic;

namespace TileStories
{
    // The POI Detail Card's config (_3.1_POI_Card_Blocks.md section 5). Two places in config.json:
    //   card_settings (wall level, WallConfigData.card_settings) -- the card container and the Block Library
    //   card          (per POI, POIData.card)                      -- the POI's ordered block instances
    // Block content is GENERIC (_3.1 section 3.1, option C): every block is one BlockInstanceData whose
    // fields are keyed values; the kind's BlockKindDefinition (Runtime/Blocks) says which keys exist and
    // their types. JsonUtility cannot store a list of different C# types, so this is the one shape that
    // lets an app add a kind without editing Framework.
    // JsonUtility keeps a field's initializer when the file leaves the field out, so every initializer
    // here IS the runtime default (a config written before this domain loads as "card on, header only").
    [Serializable]
    public class CardSettings
    {
        // Master switch: off = selection still works, no card opens
        public bool enabled = true;
        // Language codes every localized field is authored in; the first one is the fallback
        public List<string> languages = new() { "en", "pt" };
        // Resources-relative folder holding this wall's card media (images, audio...), e.g. "LivingRoom/CardMedia".
        // An Asset field stores a path under it. Empty = the wall has no card media yet.
        public string media_resources_path = "";
        // Resources-relative path to this wall's own CardMediaLibrary.asset (_3.1 step 13), same pattern as
        // marker_icon_library_resources_path: a "default:<key>" value resolves here first, then the Framework's
        // shipped library. Empty = the wall has no override library; every default key resolves from the Framework's.
        public string default_media_library_resources_path = "";
        public CardContainerSettings container = new();
        // How the card orders its blocks and what each family does there (_3.2 section 3, Detail Card > Navigation)
        public CardNavigationSettings navigation = new();
        // The Block Library: one row per kind the developer changed. A kind with no row is enabled with
        // its own default variant (BlockLibraryRule), so a new built-in kind needs no config edit.
        public List<BlockKindSetting> kinds = new();
        // Card Texts: this wall's own wording of the card's UI texts ("Did you know?", the close label...). A key
        // with no row here, or a language with no text, uses the framework's default (CardStrings).
        public List<CardStringEntry> strings = new();
        // The wall's glossary: a card text marks a word as [[term]] (or [[shown words|term]]) and the visitor taps it
        // for this definition (GlossaryMarkup, CardGlossary)
        public List<GlossaryEntry> glossary = new();
        // Developer-only: open one POI's card by itself in Play Mode (Detail Card > Card Container > Test > Show demo card). Off by default,
        // ignored by release builds (CardDemoRule.IsAllowed), registered in DevFeatureBuildGuard
        public CardDemoSettings demo_card = new();
        // Developer-only: the language the card opens in while the developer tests (Detail Card > Card Container > Test > Preview Language).
        // "" = the wall's first language, the default. A visitor's own pick on the card wins; release builds ignore it (CardLanguageRule.Shown,
        // CardDemoRule.IsAllowed) and DevFeatureBuildGuard warns when a value other than the default is left in the config
        public string preview_language = "";
    }

    // Detail Card > Navigation (_3.2 section 3): how a visitor moves inside ONE open card. Each field lands with the code
    // that reads it: the two that order the stack (BlockStackBuilder) and the three the nav model reads (CardNavModel,
    // NavOntologyRule); the nav elements, layouts and their fields come later, as do the Editor rows of these three (steps 5-7).
    [Serializable]
    public class CardNavigationSettings
    {
        // Block Order: "authored" | "end_families_last" (the default) | "by_family" (CardOptions.BlockOrders,
        // applied by BlockStackBuilder after the skip rules, with the header always first)
        public string block_order = CardOptions.BlockOrderEndFamiliesLast;
        // The Families table: ONE row per family the wall actually changed. A family with no row keeps the order
        // and the At The End of its registered FamilyDefinition, so a new built-in family needs no config edit.
        public List<CardFamilySetting> families = new();
        // Group By: "family" (the visitor's intent, the default) | "format" (CardOptions.GroupBys) -- what the sections are
        public string group_by = CardOptions.GroupByFamily;
        // Show Navigation From: a card with fewer targets than this shows NO navigation element (a short card stays clean)
        public int min_blocks = 8;
        // Bar Slots: how many sections a bar shows before its last slot becomes More (a navigation bar holds 3 to 5)
        public int max_slots = 5;
        // Follow Scroll: the section a navigation element marks follows what the visitor scrolls through (off: only a jump moves it)
        public bool follow_scroll = true;
        // Jump Opens Card To: "full" (the default: a jump first raises the sheet to Full) | "keep" (CardOptions.JumpRaises)
        public string jump_raises_to = CardOptions.JumpRaisesFull;
        // Jump Motion: "smooth" (the default: the stack glides over --ts-duration) | "instant" (CardOptions.JumpMotions)
        public string jump_motion = CardOptions.JumpMotionSmooth;
        // Back Button: "step_down" (the default: each system Back press undoes the innermost thing -- the full view, then an open menu or
        // list over the card, then one stop down, then closes the card, NavBackRule) | "none" (Back is left to the app) (CardOptions.BackButtons)
        public string back_button = CardOptions.BackButtonStepDown;
        // Top Bar: the element in the strip under the header (a NavElementRegistry key of the top slot, "text_tabs" the default) |
        // "none" (CardOptions.NavNone). A key nothing registered draws no strip (the Editor shows it as missing)
        public string top = BuiltInNavElements.TextTabs;
        // Bottom Bar: the element under the stack, BELOW the sticky CTA footer, in the thumb zone (a NavElementRegistry key of the bottom
        // slot) | "none" (CardOptions.NavNone, the default). The footer and the bar are ONE bottom area for CardChromeRule
        public string bottom = CardOptions.NavNone;
        // Menu: the element the header's menu button opens over the card, on a scrim (a NavElementRegistry key of the menu slot: drawer_tree,
        // grid_overlay) | "none" (CardOptions.NavNone, the default: a menu hides the map behind a button, so it is never the only way in).
        // When set, a bar's More slot opens it instead of its small list
        public string menu = CardOptions.NavNone;
        // Floating Button: the element in the card's floating slot, just above the sheet's top-right edge at every stop (a NavElementRegistry
        // key of the floating slot: fab_dial, fab_radial) | "none" (CardOptions.NavNone, the default)
        public string floating = CardOptions.NavNone;
        // Edge: the element down the right edge of the stack's viewport (a NavElementRegistry key of the edge slot: edge_scrubber) | "none"
        // (CardOptions.NavNone, the default). It sits over the content, so it is opt-in
        public string edge = CardOptions.NavNone;
        // Auto Nav Block: a nav block (a registered kind of the `navigation` family: nav_icon_grid) put right after the header of every card
        // with at least Show Navigation From targets -- unless the point already shows a nav block of its own | "none" (CardOptions.NavNone,
        // the default). Inserted by BlockStackBuilder.InsertAutoNavBlock, after the skip rules and the Block Order
        public string auto_nav_block = CardOptions.NavNone;
        // Placed Model Control: "chip" (the default: while a model placed with place_in_ar stands, a chip above the sheet says so at every
        // stop, with a Remove on the card that placed it) | "none" (CardOptions.PlacedModelControls, PlacedModelChipRule)
        public string placed_model_control = CardOptions.PlacedModelChip;
        // Show Navigation At: "half_and_full" (the default) | "full_only" (CardOptions.ShowNavigationAts) -- the stops the Top Bar shows at,
        // never at peek. CardChromeRule may hide it at a stop with too little room left for the content; this option only hides more
        public string show_at = CardOptions.ShowNavHalfAndFull;
        // Hide While Scrolling: "down" (the default: the bottom area -- the sticky CTA footer -- slides away while the content moves down and
        // comes back on a move up or at the top / end) | "never" (CardOptions.HideOnScrolls). The Top Bar always stays: it is the map
        public string hide_on_scroll = CardOptions.HideOnScrollDown;
        // Labels: what an icon-capable element's items show -- "icon_and_text" (the default) | "icon" | "text" (CardOptions.Labels). A
        // section with no icon (a format) always shows its words (NavLabelRule)
        public string labels = CardOptions.LabelsIconAndText;
        // Show Counts: an item shows how many blocks it leads to
        public bool show_counts = false;
        // The Groups table (the segmented element, _3.2 section 3): the wall's own groups, in order. Empty = the framework's four
        // (NavGroupRule.Defaults: Discover, Watch, Explore, Visit); a family joins one through its Families row's Group
        public List<CardNavGroup> groups = new();
        // Layout: how the card arranges its blocks (CardLayoutRule) -- "scroll" (the default: one scroll) | "accordion" (every navigation target
        // a compact row that opens in place) | "tabs" (one section shown at a time) | "pages" (one section per sideways page, the next one
        // peeking; offered, never the default) (CardOptions.Layouts)
        public string layout = CardOptions.LayoutScroll;
        // Open One At A Time (accordion only): opening a row closes the others
        public bool accordion_one_open = false;

        public const int MinBlocksMin = 2;
        public const int MinBlocksMax = 30;
        // Bar Slots also caps fab_radial's arc (its last item becomes More, like a bar's)
        public const int MaxSlotsMin = 3;
        public const int MaxSlotsMax = 5;
    }

    // One Families row: what this wall changed about one registered family
    [Serializable]
    public class CardFamilySetting
    {
        // The family's key (FamilyDefinition.Key), never its visitor label
        public string family;
        // Where the family sits among the others, overriding its registered Order
        public int order;
        // Whether this family's blocks close the card, overriding its registered AtEnd
        public bool at_end;
        // The Groups row this family joins (its key); blank = the framework's default group for it (NavGroupRule.DefaultGroupOf)
        public string group = "";
    }

    // One row of the Groups table: a generated key (what a Families row points at, never shown) and the visitor's words per language.
    // A default row's words may stay empty: the framework's own word for its key is read then (CardStrings nav_group_<key>)
    [Serializable]
    public class CardNavGroup
    {
        public string key;
        public List<LocalizedEntry> label = new();
    }

    // The developer-only demo card: which POI's card opens on its own, and at which stop
    [Serializable]
    public class CardDemoSettings
    {
        public bool enabled = false;
        // The POI whose card opens (its id: the POI Editor's popup names it by its list title); blank = none picked yet
        public string poi_id = "";
        // "peek" | "half" | "full" (CardOptions.DemoStops): unlike the wall's Open At, the demo may also open the full card
        public string stop = CardOptions.StopHalf;
    }

    // One glossary word and its definition in every language it is written in
    [Serializable]
    public class GlossaryEntry
    {
        // What a card text writes between [[ ]] (matched ignoring case and surrounding spaces)
        public string term;
        public List<LocalizedEntry> definition = new();
    }

    // One card UI text in every language it is written in, by its framework key (CardStrings.Keys)
    [Serializable]
    public class CardStringEntry
    {
        public string key;
        public List<LocalizedEntry> text = new();
    }

    [Serializable]
    public class CardContainerSettings
    {
        // Where a selection opens the card: "peek" (header only) or "half"; never "full" (never auto-expanded)
        public string open_stop = CardOptions.StopPeek;
        // Cap of the half stop, as a share of the safe-area height
        public float half_max_ratio = 0.40f;
        // A tap on empty camera space closes the card (and clears the selection)
        public bool dismiss_on_tap_outside = true;
        // Audio started in the card keeps playing in a mini-player after the card closes (Tier 4)
        public bool keep_audio_on_close = true;
        // What starting an audio does while another one plays: "switch" (fade the playing one out, start the new) or "queue" (the new
        // one waits for the playing one to end); never two at once (CardOptions.AudioModes, AudioSwitchRule)
        public string audio_when_another_starts = CardOptions.AudioSwitch;
        // Android only: also ask the platform every second or two whether Bluetooth earbuds are still the output, because
        // AudioSettings.OnAudioConfigurationChanged is reported not to fire reliably there (work plan Stage 2 item 3). Off until a device
        // test shows the callback alone is not enough
        public bool audio_android_output_poll;
        // The wall's default for motion the visitor did not ask for (step 9B): on, a header's video loop shows its poster only. The visitor's
        // own reduce-motion setting (_3.3) will join it; today this is the only switch
        public bool reduce_motion;
        // The horizontal swipe tracks (timeline, related carousel) show a slice of the next card at rest -- the usual "there is more" cue --
        // and end on a whole last card (CardTrackRule). Off: whole cards only, at rest and at the end. On by default: an old config loads as on
        public bool peek_next_card = true;

        public const float HalfMaxRatioMin = 0.25f;
        public const float HalfMaxRatioMax = 0.40f;
    }

    // One Block Library row: the wall-wide switch and default variant of one kind
    [Serializable]
    public class BlockKindSetting
    {
        public string kind;
        public bool enabled = true;
        // Empty = the kind's own default variant
        public string default_variant = "";
        // Developer note (the table's Details popup)
        public string details = "";
        // The wall's default of a Choice field the kind marks LibraryDefault (e.g. a model's Fit): a block that stores
        // no value of its own reads this one (BlockLibraryRule.Choice). No entry = the field's own ChoiceDefault
        public List<BlockFieldDefault> field_defaults = new();
    }

    // One Block Library default: a field key of the kind and the option it defaults to
    [Serializable]
    public class BlockFieldDefault
    {
        public string key;
        public string value;
    }

    // A POI's card: its ordered blocks. No blocks = a header-only card from the POI's name and summary.
    [Serializable]
    public class POICardData
    {
        public List<BlockInstanceData> blocks = new();
    }

    // One block on one POI's card
    [Serializable]
    public class BlockInstanceData
    {
        // Generated block_N, never shown, never reused (the TaxonomyRowKeys identity rule)
        public string key;
        // The BlockKindDefinition key ("header", "rich_text"...)
        public string kind;
        // One of the kind's variants; empty or unknown = the Block Library's default for the kind
        public string variant = "";
        // "inline" | "expandable" | "takeover" (CardOptions)
        public string display = CardOptions.DisplayInline;
        public List<BlockFieldValue> fields = new();
    }

    // One field value of a block. Only the member matching the field's BlockFieldType is read.
    [Serializable]
    public class BlockFieldValue
    {
        public string key;
        public List<LocalizedEntry> text = new();   // LocalizedText / LocalizedLongText
        public float number;                        // Number
        public bool flag;                           // Toggle
        public string value = "";                   // Choice option, PoiRef POI id
        public string asset = "";                   // Asset: path under card_settings.media_resources_path
        public List<BlockItemData> items = new();   // Items (a repeater: facts, gallery images, quiz options...)
    }

    // One row of an Items field. It holds its OWN keyed sub-fields (a fact = label + value). Not a
    // BlockFieldValue list: that would make the type recursive, which Unity's serializer cannot store.
    [Serializable]
    public class BlockItemData
    {
        public List<BlockItemFieldValue> fields = new();
    }

    // A sub-field of one item: a BlockFieldValue without nested items
    [Serializable]
    public class BlockItemFieldValue
    {
        public string key;
        public List<LocalizedEntry> text = new();
        public float number;
        public bool flag;
        public string value = "";
        public string asset = "";
    }

    // One language's text of a localized field
    [Serializable]
    public class LocalizedEntry
    {
        public string lang;
        public string value;
    }

    // Every option string of the card config, shared by the runtime and the Editor dropdowns
    public static class CardOptions
    {
        public const string StopPeek = "peek";
        public const string StopHalf = "half";
        public const string StopFull = "full";
        public static readonly string[] OpenStops = { StopPeek, StopHalf };
        // The stops the developer-only demo card may open at (the wall's own Open At never opens the full card)
        public static readonly string[] DemoStops = { StopPeek, StopHalf, StopFull };

        // What starting an audio does while another plays (CardContainerSettings.audio_when_another_starts), and the Editor's words for them
        public const string AudioSwitch = "switch";
        public const string AudioQueue = "queue";
        public static readonly string[] AudioModes = { AudioSwitch, AudioQueue };
        public static readonly string[] AudioModeLabels = { "Switch To The New One", "Queue The New One" };

        // Block Order (CardNavigationSettings.block_order, _3.2 section 2.3), and the Editor's words for them.
        // It replaces the old container option Sources At The End: end_families_last does what that did, through
        // the family flag At The End instead of a second switch that only knew about Sources.
        public const string BlockOrderAuthored = "authored";
        public const string BlockOrderEndFamiliesLast = "end_families_last";
        public const string BlockOrderByFamily = "by_family";
        public static readonly string[] BlockOrders = { BlockOrderAuthored, BlockOrderEndFamiliesLast, BlockOrderByFamily };
        public static readonly string[] BlockOrderLabels = { "As Authored", "End Families Last", "Grouped By Family" };

        // A kind's Format (BlockKindDefinition.Format, _3.2 section 2.4): HOW a visitor takes the block in. A FIXED list --
        // formats are universal, families are a wall's vocabulary -- so an app picks one of these and never adds its own.
        // model = anything spatial: a 3D model, a 360 panorama, something shown in the room or on the wall (AR).
        public const string FormatRead = "read";
        public const string FormatLook = "look";
        public const string FormatWatch = "watch";
        public const string FormatListen = "listen";
        public const string FormatModel = "model";
        public const string FormatInteract = "interact";
        public static readonly string[] Formats = { FormatRead, FormatLook, FormatWatch, FormatListen, FormatModel, FormatInteract };

        // Group By (CardNavigationSettings.group_by): what the card's navigation sections are -- the families (the visitor's
        // intent, the default) or the formats. The same targets either way (CardNavModel)
        public const string GroupByFamily = "family";
        public const string GroupByFormat = "format";
        public static readonly string[] GroupBys = { GroupByFamily, GroupByFormat };

        // Jump Opens Card To (CardNavigationSettings.jump_raises_to, _3.2 section 4.4): a jump first raises the sheet to Full
        // (Half shows too little to land on), or keeps the stop the visitor left it at
        public const string JumpRaisesFull = "full";
        public const string JumpRaisesKeep = "keep";
        public static readonly string[] JumpRaises = { JumpRaisesFull, JumpRaisesKeep };

        // Jump Motion (CardNavigationSettings.jump_motion): the stack glides to the target over --ts-duration, or is set there
        // at once (reduce motion always sets it at once)
        public const string JumpMotionSmooth = "smooth";
        public const string JumpMotionInstant = "instant";
        public static readonly string[] JumpMotions = { JumpMotionSmooth, JumpMotionInstant };
        public static readonly string[] JumpRaiseLabels = { "Full", "Keep Its Stop" };
        public static readonly string[] JumpMotionLabels = { "Smooth", "Instant" };

        // Back Button (CardNavigationSettings.back_button, _3.2 section 4.5): system Back steps the card down (NavBackRule) or is left to the app
        public const string BackButtonStepDown = "step_down";
        public const string BackButtonNone = "none";
        public static readonly string[] BackButtons = { BackButtonStepDown, BackButtonNone };
        public static readonly string[] BackButtonLabels = { "Step The Card Down", "Leave It To The App" };

        // A navigation slot with no element in it (CardNavigationSettings.top, _3.2 section 3); never a registered element's key
        public const string NavNone = "none";
        public const string NavNoneLabel = "None";

        // Placed Model Control (CardNavigationSettings.placed_model_control, _3.2 section 5.4): the chip above the sheet while a placed model
        // stands, or nothing. The first option is the default the card reads for an unknown value too
        public const string PlacedModelChip = "chip";
        public const string PlacedModelNone = "none";
        public static readonly string[] PlacedModelControls = { PlacedModelChip, PlacedModelNone };
        public static readonly string[] PlacedModelControlLabels = { "Chip", "None" };

        // Show Navigation At (CardNavigationSettings.show_at, _3.2 section 4.3): the stops the Top Bar may show at (never at peek)
        public const string ShowNavHalfAndFull = "half_and_full";
        public const string ShowNavFullOnly = "full_only";
        public static readonly string[] ShowNavigationAts = { ShowNavHalfAndFull, ShowNavFullOnly };
        public static readonly string[] ShowNavigationAtLabels = { "Half And Full", "Full Only" };

        // Hide While Scrolling (CardNavigationSettings.hide_on_scroll, _3.2 section 4.3): the bottom area slides away while the content
        // moves down, or never
        public const string HideOnScrollDown = "down";
        public const string HideOnScrollNever = "never";
        public static readonly string[] HideOnScrolls = { HideOnScrollDown, HideOnScrollNever };
        public static readonly string[] HideOnScrollLabels = { "When Scrolling Down", "Never" };

        // Labels (CardNavigationSettings.labels, _3.2 section 3): what the items of an icon-capable navigation element show
        public const string LabelsIconAndText = "icon_and_text";
        public const string LabelsIcon = "icon";
        public const string LabelsText = "text";
        public static readonly string[] Labels = { LabelsIconAndText, LabelsIcon, LabelsText };
        public static readonly string[] LabelsLabels = { "Icon And Text", "Icon", "Text" };

        // Layout (CardNavigationSettings.layout, _3.2 section 6.1): how the card arranges its blocks, and the Editor's words for them. Bento
        // joins with its own step (15)
        public const string LayoutScroll = "scroll";
        public const string LayoutAccordion = "accordion";
        public const string LayoutTabs = "tabs";
        public const string LayoutPages = "pages";
        public static readonly string[] Layouts = { LayoutScroll, LayoutAccordion, LayoutTabs, LayoutPages };
        public static readonly string[] LayoutLabels = { "Scroll", "Accordion", "Tabs", "Pages" };

        // Group By's Editor words (GroupBys above)
        public static readonly string[] GroupByLabels = { "Family", "Format" };

        public const string DisplayInline = "inline";
        public const string DisplayExpandable = "expandable";
        public const string DisplayTakeover = "takeover";
    }
}
