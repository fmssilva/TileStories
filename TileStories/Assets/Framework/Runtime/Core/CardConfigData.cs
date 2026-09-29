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
        public CardContainerSettings container = new();
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

        public const string DisplayInline = "inline";
        public const string DisplayExpandable = "expandable";
        public const string DisplayTakeover = "takeover";
    }
}
