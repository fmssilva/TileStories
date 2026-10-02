using System.Collections.Generic;

namespace TileStories
{
    // What one card is bound with (_3.1 15.4.2): the decisions a host used to make inline before showing a card -- which language the card
    // speaks and which one its missing texts read, whether the wall's media folder changed, the card's words and glossary in that language --
    // made here once, pure, for both hosts (the wall's PoiCardHost and the Phase A CardGalleryHarness). A host keeps only what it OWNS (its
    // media source, the visitor's saved state, the audio / video / preview / AR owners) and hands those in.
    public static class CardContextBuilder
    {
        // The language a wall's card shows (the visitor's pick, else the developer's preview where the build allows it, else the wall's
        // first language) and the one its missing texts read: always the wall's FIRST language, never the shown one (that is no fallback)
        public static (string Shown, string Fallback) Languages(CardSettings settings, string visitorPick, bool previewAllowed)
        {
            var languages = settings?.languages;
            return (CardLanguageRule.Shown(languages, visitorPick, settings?.preview_language, previewAllowed), CardLanguageRule.Fallback(languages));
        }

        // Whether a host whose media source reads `currentRoot` (null: none yet) needs a new one for the wall's Media Folder (compared the
        // way ResourcesMediaSource stores it: trimmed, no slashes at the ends)
        public static bool NeedsMediaSource(string currentRoot, CardSettings settings) =>
            currentRoot == null || currentRoot != (settings?.media_resources_path ?? "").Trim().Trim('/');

        // The context of one card: `hostParts` holds what the host owns (Media, MarkerLook, State, Events, Services and the four owners) and
        // gets the point, the wall's settings, `language`, the wall's first language as the fallback, and the words (framework < app < wall)
        // and glossary read in them. The same object comes back, completed.
        public static BlockBindContext Build(BlockBindContext hostParts, POIData poi, WallConfigData taxonomy, CardSettings settings, string language,
            IReadOnlyList<CardStringEntry> frameworkWords, IReadOnlyList<CardStringEntry> appWords)
        {
            var context = hostParts ?? new BlockBindContext();
            settings ??= new CardSettings();
            string fallback = CardLanguageRule.Fallback(settings.languages);
            context.Poi = poi;
            context.Taxonomy = taxonomy;
            context.Settings = settings;
            context.Language = language;
            context.FallbackLanguage = fallback;
            context.Strings = new CardStrings(frameworkWords, appWords, settings.strings, language, fallback);
            context.Glossary = new CardGlossary(settings.glossary, language, fallback);
            context.ReduceMotion = settings.container?.reduce_motion ?? false;
            return context;
        }

        // The developer's warning for a block a card leaves out: which point, which block (key and kind), why, and the field when one is to blame
        public static string SkipWarning(string poiId, BlockStackBuilder.Skipped skipped) =>
            "[Card] " + poiId + ": block '" + skipped.Instance?.key + "' (" + skipped.Instance?.kind + ") not shown: " + skipped.Reason
            + (skipped.FieldKey != null ? " '" + skipped.FieldKey + "'" : "");
    }
}
