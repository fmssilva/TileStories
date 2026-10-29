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
        // The mode the app opens in on this wall (Detail Card > Card Container > Default Mode, _3.3 P4.4b): "light" (default) | "dark"
        // (CardOptions.Modes). A visitor's own pick in the card's Aa settings wins (ThemeModeRule); a config that never wrote it opens light
        public string default_mode = CardOptions.ModeLight;
        // The shared card components' wall defaults (_3.3 R17.5, Detail Card > Card Components): what every block draws unless its own
        // Component Overrides row says otherwise
        public CardComponentSettings components = new();
        // The wall's own block templates (_3.3 R14.4, Detail Card > Block Library > Templates): named compositions of element rows with their
        // looks and no content. A block made from one COPIES its rows (a preset: nothing stays linked); the framework's built-in templates
        // ship as Editor code beside them. Empty = none of the wall's own
        public List<BlockTemplateData> block_templates = new();
        // The card's sections (_3.3 fourth round, v6's Tweaks "Sections"; Detail Card > Sections): how each section of an OPEN card begins,
        // what stands between two of them, their colour, how strong it is and how it moves. Under Look Cards (container.blocks) none of it
        // draws. An old config loads with v6's defaults
        public CardSectionSettings sections = new();
    }

    // The Sections options (v6's Tweaks "Sections", same names, order and defaults): every choice a CardOptions.Section* key, an unknown value
    // read as its default (SectionStyleRule). A Bar Groups row's Section Style overrides Start for its group (the nearest level wins)
    [Serializable]
    public class CardSectionSettings
    {
        // Start: how a section begins -- "plain" | "bands" (every other section on a faint tint) | "fade" (its colour fading down) | "wave"
        // (a soft swell, then the fade) | "tiles" (the default: a row of azulejo tiles, rounded top corners, then the fade)
        public string start = CardOptions.SectionStartTiles;
        // Divider: what stands between two sections -- "none" (the default) | "hairline" | "ornament" (one tile between two lines) |
        // "azulejo" (a strip of tiles) | "mosaic" (woven tesserae) | "image" (the developer's own tileable strip, Divider Image)
        public string divider = CardOptions.SectionDividerNone;
        // Divider Image: a tileable strip picture, a path under the card's Media Folder (read only with Divider Image)
        public string divider_image = "";
        // Colour: the tint a section begins in -- "group" (the default: its Navigation Bar group's colour) | "accent" | "neutral"
        public string colour = CardOptions.SectionColourGroup;
        // Intensity: how strong the tint is where the section begins (IntensityMin..IntensityMax; v6's 0.30 is out of reach: 7:1 for the
        // words over it, measured)
        public float intensity = 0.12f;
        // Motion: "still" | "reveal" (the default: the tiles turn in one after the other as the section enters the view) | "drift" (the
        // same turn, then the divider's tiles or strip slide slowly sideways; the start rests). The card's Reduce Motion always shows them still
        public string motion = CardOptions.SectionMotionReveal;
        // Vary: "same" (the default: every section begins with Start) | "by_group" (Fade, Wave, Tiles in turn, group by group)
        public string vary = CardOptions.SectionVarySame;

        public const float IntensityMin = 0.04f;
        public const float IntensityMax = 0.20f;
    }

    // One block template of the wall (_3.3 R14.4): a PRESET -- "+ Add block" from it copies its rows into the point's new block, and the
    // block's "Re-apply" adds back the template rows it lacks. The card never reads it
    [Serializable]
    public class BlockTemplateData
    {
        // Generated template_N, never shown; a block made from it stores it (BlockSurfaces.TemplateValue), so a rename touches no block
        public string key;
        // The developer's name for it: the Templates table, the "+ Add block" picker, a block's Template line
        public string name = "";
        // The composition: a block of a composing kind whose Page rows hold each element's look, motion and tap -- never content
        public BlockInstanceData block = new();
    }

    // The shared card components (_3.3 R17.5): one value per component the wall sets once; a block overrides it in its Component Overrides
    [Serializable]
    public class CardComponentSettings
    {
        // The Open Row's emphasis (_3.3 R14.8): "plain" | "nudge" (default: the chevron steps towards the page -- it says "open me" without
        // a glow, so the card keeps ONE glowing element, D5) | "shine" (ElementOptions.OpenRowEmphases). Reduce Motion always shows plain
        public string open_row_emphasis = ElementOptions.EmphasisNudge;
        // The Page Chrome's parts (_3.3 R17.5): the block page's one-line breadcrumb beside its Back chevron, and the Previous / Next row at
        // the page's foot. Both on by default; a block overrides them in its Component Overrides row (PageChromeRule)
        public bool page_chrome_breadcrumb = true;
        public bool page_chrome_prev_next = true;
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
        // Top Bar: the element in the strip under the header (a NavElementRegistry key of the top slot: text_tabs, ...) | "none"
        // (CardOptions.NavNone, the default: the Navigation Bar is the card's way round). A key nothing registered draws no strip (the Editor
        // shows it as missing)
        public string top = CardOptions.NavNone;
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
        // peeking; offered, never the default) | "bento" (the sections as tiles sized by their block counts; a tile opens its section in the full
        // view) (CardOptions.Layouts)
        public string layout = CardOptions.LayoutScroll;
        // Open One At A Time (accordion only): opening a row closes the others
        public bool accordion_one_open = false;
        // The Navigation Bar (_3.3 S4 third round, Detail Card > Navigation > Navigation Bar): ONE configurable bar of the card's groups --
        // where it sits, its shape and fill, how the chosen group shows, when it shows, and its groups table. While it is on (the default)
        // the card draws it in place of the Top Bar / Bottom Bar / Menu / Floating Button / Edge elements (their values are kept)
        public CardNavBarSettings bar = new();

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

    // The Navigation Bar as a whole (_3.3 S4 third round, v5's Tweaks "Navigation Bar", in that order): what NavBarView draws and NavBarRule
    // reads. Every choice is a CardOptions.NavBar* key; an unknown value reads as the option's default, so the Editor row and the card agree.
    // A config written before v5 loads unchanged: its "under_title" reads as Top, and the keys v5 dropped (corner_radius, fill_opacity, ...)
    // are simply not read
    [Serializable]
    public class CardNavBarSettings
    {
        // On by default: the bar is the visitor's main way round the card (the old Top / Bottom / Menu / Floating / Edge elements default
        // to none, and are drawn only while it is off)
        public bool enabled = true;
        // Position: "bottom" (the default: floating over the screen's foot, in the thumb zone) | "top" (under the chrome row)
        public string position = CardOptions.NavBarBottom;
        // Shape: "floating" (the default: a rounded bar inset from the screen's sides) | "full_width" (edge to edge, square corners)
        public string shape = CardOptions.NavBarFloating;
        // Fill: "solid" (the default: the theme's opaque bar colour, the only fill with a shadow) | "glass" (see-through) | "clear"
        // (transparent over the hero, solid after it)
        public string fill = CardOptions.NavBarSolid;
        // Indicator: "raised" (the default: the bar's edge bends into a bump under the chosen group, its icon riding up in an accent disc)
        // | "pill" | "underline" | "none"
        public string indicator = CardOptions.NavBarIndicatorRaised;
        // Raised Depth (under Raised): -1..+1 -- +1 (the default) the bell hugging the disc, 0 a flat bar, below 0 a cradle dipping round it
        public float raised_depth = 1f;
        // Chosen Icon: "filled" (the default: an accent disc behind the chosen icon at its own place, with any indicator) | "plain"
        public string chosen_icon = CardOptions.NavBarChosenIconFilled;
        // Labels: "icon_and_text" (the default) | "icon" | "text" | "selected_only" (every icon, and the chosen group's words)
        public string labels = CardOptions.NavBarLabelsIconAndText;
        // Icon Colour: "group" (the default: every icon in its group's readable -text tone) | "accent" | "neutral" (the idle ink)
        public string icon_color = CardOptions.NavBarIconGroup;
        // Chosen Colour (the chosen group's disc / pill / underline): "group" (the default: its row's colour) | "accent" | "ink"
        public string selected_color = CardOptions.NavBarColourGroup;
        // Bold Chosen Icon: the chosen group's icon drawn with a heavier stroke
        public bool bold_selected = true;
        // Show: "always" (the default) | "after_hero" (it comes once the visitor scrolled the hero's picture away)
        public string show = CardOptions.NavBarShowAlways;
        // Hide While Scrolling: "never" (the default, off) | "down" (on: it slides away while the visitor reads down, back on a move up)
        public string hide_on_scroll = CardOptions.NavBarHideNever;
        // The Bar Groups table, in the bar's order. Empty = the framework's six groups (NavBarRule.DefaultRows); the first edit of the table
        // writes them all, so their order is the wall's from then on
        public List<CardNavBarItem> items = new();
    }

    // One row of the Bar Groups table: a group of the bar -- the families whose blocks it leads to, its words, its two icons and its colour
    [Serializable]
    public class CardNavBarItem
    {
        // Generated (a family's key for a default row, group_N for a row the developer added), never shown
        public string key;
        // Off = the row stays in the table, the bar leaves the group out
        public bool shown = true;
        // The visitor's words per language; empty = the first family's own word (its CardStrings label), else the key
        public List<LocalizedEntry> label = new();
        // CardIcons keys: the group's icon, and the one it wears while chosen ("" = the same icon)
        public string icon = "";
        public string icon_selected = "";
        // The name of a THEME colour (--ts-family-*, --ts-accent); "" = the first family's own colour
        public string color = "";
        // The families (FamilyDefinition keys) whose blocks the group leads to: a tap lands on the first of them on the card
        public List<string> families = new();
        // Section Style: how this group's sections begin on an open card (a Sections Start: plain / bands / fade / wave / tiles); "" = the
        // card's (Detail Card > Sections)
        public string section_style = "";
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
        // How high the Full stop rises: "top" (the top of the safe area) or "camera_strip" (short of it by the card's --ts-sheet-top-gap);
        // CardOptions.FullHeights, SheetStopRule.FullTopGap. An old config loads as top
        public string full_height = CardOptions.FullHeightTop;
        // What the chrome row (the X, the title, settings) sits on once the hero is read (_3.3 S4 third round, v6): "fade" (the default: the
        // sheet's own colour, opaque through the row, then fading to nothing -- no edge) | "glass" (a dense glass band with a hairline at its
        // foot); CardOptions.TopBands. An old config loads as fade
        public string top_band = CardOptions.TopBandFade;
        // Blocks (v6's Look): "open" (the default: sections open on the sheet -- no frames, each block its own heading, pictures edge to edge,
        // a hairline between blocks; Detail Card > Sections styles them) | "cards" (each family on its glass sheet, every block in its frame)
        public string blocks = CardOptions.BlocksOpen;
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
        // The highest N of a block_N key the Editor ever handed out on this card (TaxonomyRowKeys.TakeBlockKey): a deleted block's key is
        // never given to a new block, so what the visitor's device saved for the old one (an answer, a Contents tick) never lands on it
        public int last_block_number;
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
        // "normal" | "featured" | "quiet" (CardOptions.Emphases): the block frame's variant (Card Content > the block's Emphasis row)
        public string emphasis = CardOptions.EmphasisNormal;
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
        public string asset = "";                   // Asset: path under card_settings.media_resources_path, for every language
        // A Per Language Asset field's own file for one language (a narration recorded in Portuguese); a language with no
        // entry, or a blank one, uses `asset` (BlockFieldReader.Asset)
        public List<LocalizedEntry> asset_by_language = new();
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
        public List<LocalizedEntry> asset_by_language = new();
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

        // How high the Full stop rises (CardContainerSettings.full_height, _3.3 S4a), and the Editor's words for them: to the top of the
        // screen's safe area (the glass keeps the camera in view), or short of it, leaving a strip of the camera above the card
        public const string FullHeightTop = "top";
        public const string FullHeightCameraStrip = "camera_strip";
        public static readonly string[] FullHeights = { FullHeightTop, FullHeightCameraStrip };
        public static readonly string[] FullHeightLabels = { "To The Top", "Leave A Camera Strip" };

        // What the chrome row sits on after the hero (CardContainerSettings.top_band, v6), and the Editor's words; the first is the default
        public const string TopBandFade = "fade";
        public const string TopBandGlass = "glass";
        public static readonly string[] TopBands = { TopBandFade, TopBandGlass };
        public static readonly string[] TopBandLabels = { "Fade", "Glass" };

        // Blocks (CardContainerSettings.blocks, v6's Look) and the Editor's words; the first is the default
        public const string BlocksOpen = "open";
        public const string BlocksCards = "cards";
        public static readonly string[] Blocks = { BlocksOpen, BlocksCards };
        public static readonly string[] BlockLabels = { "Open", "Cards" };

        // The Sections options (CardSectionSettings, v6's Tweaks "Sections") and the Editor's words, each list in v6's order
        public const string SectionStartPlain = "plain";
        public const string SectionStartBands = "bands";
        public const string SectionStartFade = "fade";
        public const string SectionStartWave = "wave";
        public const string SectionStartTiles = "tiles";
        public static readonly string[] SectionStarts = { SectionStartPlain, SectionStartBands, SectionStartFade, SectionStartWave, SectionStartTiles };
        public static readonly string[] SectionStartLabels = { "Plain", "Bands", "Fade", "Wave", "Tiles" };
        public const string SectionDividerNone = "none";
        public const string SectionDividerHairline = "hairline";
        public const string SectionDividerOrnament = "ornament";
        public const string SectionDividerAzulejo = "azulejo";
        public const string SectionDividerMosaic = "mosaic";
        public const string SectionDividerImage = "image";
        public static readonly string[] SectionDividers =
            { SectionDividerNone, SectionDividerHairline, SectionDividerOrnament, SectionDividerAzulejo, SectionDividerMosaic, SectionDividerImage };
        public static readonly string[] SectionDividerLabels = { "None", "Hairline", "Ornament", "Azulejo", "Mosaic", "Image" };
        public const string SectionColourGroup = "group";
        public const string SectionColourAccent = "accent";
        public const string SectionColourNeutral = "neutral";
        public static readonly string[] SectionColours = { SectionColourGroup, SectionColourAccent, SectionColourNeutral };
        public static readonly string[] SectionColourLabels = { "Group", "Accent", "Neutral" };
        public const string SectionMotionStill = "still";
        public const string SectionMotionReveal = "reveal";
        public const string SectionMotionDrift = "drift";
        public static readonly string[] SectionMotions = { SectionMotionStill, SectionMotionReveal, SectionMotionDrift };
        public static readonly string[] SectionMotionLabels = { "Still", "Reveal", "Drift" };
        public const string SectionVarySame = "same";
        public const string SectionVaryByGroup = "by_group";
        public static readonly string[] SectionVaries = { SectionVarySame, SectionVaryByGroup };
        public static readonly string[] SectionVaryLabels = { "Same", "By Group" };

        // The Navigation Bar's choices (CardNavBarSettings, _3.3 S4 third round = v5's Tweaks) and the Editor's words for them, each list
        // in v5's order; an unknown value reads as the option's default (NavBarRule)
        public const string NavBarBottom = "bottom";
        public const string NavBarTop = "top";
        // A config written before v5 calls Top "under_title": it reads as Top (NavBarRule.Position)
        public const string NavBarUnderTitle = "under_title";
        public static readonly string[] NavBarPositions = { NavBarBottom, NavBarTop };
        public static readonly string[] NavBarPositionLabels = { "Bottom", "Top" };
        public const string NavBarFloating = "floating";
        public const string NavBarFullWidth = "full_width";
        public static readonly string[] NavBarShapes = { NavBarFloating, NavBarFullWidth };
        public static readonly string[] NavBarShapeLabels = { "Floating", "Full Width" };
        public const string NavBarSolid = "solid";
        public const string NavBarGlass = "glass";
        public const string NavBarClear = "clear";
        public static readonly string[] NavBarFills = { NavBarSolid, NavBarGlass, NavBarClear };
        public static readonly string[] NavBarFillLabels = { "Solid", "Glass", "Clear" };
        public const string NavBarIndicatorRaised = "raised";
        public const string NavBarIndicatorPill = "pill";
        public const string NavBarIndicatorUnderline = "underline";
        public const string NavBarIndicatorNone = "none";
        public static readonly string[] NavBarIndicators = { NavBarIndicatorRaised, NavBarIndicatorPill, NavBarIndicatorUnderline, NavBarIndicatorNone };
        public static readonly string[] NavBarIndicatorLabels = { "Raised", "Pill", "Underline", "None" };
        public const string NavBarChosenIconPlain = "plain";
        public const string NavBarChosenIconFilled = "filled";
        public static readonly string[] NavBarChosenIcons = { NavBarChosenIconPlain, NavBarChosenIconFilled };
        public static readonly string[] NavBarChosenIconLabels = { "Plain", "Filled" };
        public const string NavBarLabelsIconAndText = "icon_and_text";
        public const string NavBarLabelsIcon = "icon";
        public const string NavBarLabelsText = "text";
        public const string NavBarLabelsSelectedOnly = "selected_only";
        public static readonly string[] NavBarLabels = { NavBarLabelsIconAndText, NavBarLabelsIcon, NavBarLabelsText, NavBarLabelsSelectedOnly };
        public static readonly string[] NavBarLabelLabels = { "Icon And Text", "Icon", "Text", "Text On The Chosen" };
        public const string NavBarColourAccent = "accent";
        public const string NavBarColourInk = "ink";
        public const string NavBarColourGroup = "group";
        public static readonly string[] NavBarSelectedColours = { NavBarColourAccent, NavBarColourInk, NavBarColourGroup };
        public static readonly string[] NavBarSelectedColourLabels = { "Accent", "Ink", "Group" };
        public const string NavBarIconGroup = "group";
        public const string NavBarIconAccent = "accent";
        public const string NavBarIconNeutral = "neutral";
        public static readonly string[] NavBarIconColours = { NavBarIconGroup, NavBarIconAccent, NavBarIconNeutral };
        public static readonly string[] NavBarIconColourLabels = { "Group", "Accent", "Neutral" };
        public const string NavBarShowAlways = "always";
        public const string NavBarShowAfterHero = "after_hero";
        public static readonly string[] NavBarShows = { NavBarShowAlways, NavBarShowAfterHero };
        public static readonly string[] NavBarShowLabels = { "Always", "After The Hero" };
        // Hide While Scrolling, a switch in the Editor: off = "never", on = "down" (it slides away while the visitor reads down)
        public const string NavBarHideNever = "never";
        public const string NavBarHideDown = "down";
        public static readonly string[] NavBarHides = { NavBarHideNever, NavBarHideDown };

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

        // What a kind NEEDS from the visitor's phone or place (BlockKindDefinition.Needs, _3.2 section 5.8): a FIXED list like the formats, set on
        // the kind and never on a block. The card's menus offer "No headphones / No camera / No network" chips for the ones a card holds.
        public const string NeedHeadphones = "headphones";
        public const string NeedCamera = "camera";
        public const string NeedNetwork = "network";
        public static readonly string[] Needs = { NeedHeadphones, NeedCamera, NeedNetwork };

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

        // Default Mode (CardSettings.default_mode, _3.3 P4.4b): the theme's two modes, light first (the default), and the Editor's words
        public const string ModeLight = "light";
        public const string ModeDark = "dark";
        public static readonly string[] Modes = { ModeLight, ModeDark };
        public static readonly string[] ModeLabels = { "Light", "Dark" };

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

        // Layout (CardNavigationSettings.layout, _3.2 section 6.1): how the card arranges its blocks, and the Editor's words for them
        public const string LayoutScroll = "scroll";
        public const string LayoutAccordion = "accordion";
        public const string LayoutTabs = "tabs";
        public const string LayoutPages = "pages";
        public const string LayoutBento = "bento";
        public static readonly string[] Layouts = { LayoutScroll, LayoutAccordion, LayoutTabs, LayoutPages, LayoutBento };
        public static readonly string[] LayoutLabels = { "Scroll", "Accordion", "Tabs", "Pages", "Bento" };

        // Group By's Editor words (GroupBys above)
        public static readonly string[] GroupByLabels = { "Family", "Format" };

        public const string DisplayInline = "inline";
        public const string DisplayExpandable = "expandable";
        public const string DisplayTakeover = "takeover";

        // A block's Emphasis (BlockInstanceData.emphasis, _3.3 R13.1, P4.4b): the frame's named variants -- normal, featured (a stronger
        // shadow, a larger title: the 1-2 blocks a wall pushes), quiet (a hairline edge, no rim light or shadow, a smaller title: meta
        // blocks such as sources). The first is the default the card reads for an unknown value too
        public const string EmphasisNormal = "normal";
        public const string EmphasisFeatured = "featured";
        public const string EmphasisQuiet = "quiet";
        public static readonly string[] Emphases = { EmphasisNormal, EmphasisFeatured, EmphasisQuiet };
        public static readonly string[] EmphasisLabels = { "Normal", "Featured", "Quiet" };
    }
}
