using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // What a composed block says about itself, read from its rows (_3.3 R14.3, R14.6, R14.8), pure:
    //   - Minutes:    the Reading Time set by hand, else Auto -- the Page's words at WordsPerMinute plus each media item's weight
    //                 (ElementKindDefinition.SecondsPerMedia), rounded UP to whole minutes; 0 for a Page with nothing in it
    //   - FirstWords: the first Page row with words, as one line (the BlockSummaryRule shape)
    //   - SearchWords: every language's words of the text fields each element lists for search, plus its media captions (never credits),
    //                 from the Page's and the Custom preview's OWN rows -- a reference's words belong to the row it points at
    //   - TapOf:      what a tap on a row does, against what its element offers
    // A Page reference counts as its target (ElementRefRule); a broken one, or a row of an unknown element, counts as nothing.
    public static class ElementRules
    {
        // An adult's comfortable reading speed for a story read on a phone, standing
        public const float WordsPerMinute = 200f;

        // The block's reading time in whole minutes (0 = nothing to read), in `language` (else `fallback`, else any language with words)
        public static int Minutes(ElementScope scope, ElementRegistry elements, string language, string fallback)
        {
            int byHand = BlockSurfaces.MinutesSetByHand(scope?.Holder);
            if (byHand > 0) return byHand;
            float seconds = 0f;
            foreach (var (row, element) in PageContent(scope, elements))
            {
                var read = new BlockFieldReader(null, language, fallback);
                if (element.Fields != null)
                    foreach (var field in element.Fields)
                        if (BlockSummaryRule.IsTextField(field)) seconds += WordCount(read.ItemText(row, field.Key)) * 60f / WordsPerMinute;
                seconds += new ElementRow(row).Media.Count * element.SecondsPerMedia;
            }
            return seconds <= 0f ? 0 : Mathf.CeilToInt(seconds / 60f);
        }

        // The first words of the Page (the first row whose element has a text field with words), one line; "" when none
        public static string FirstWords(ElementScope scope, ElementRegistry elements, string language, string fallback)
        {
            var read = new BlockFieldReader(null, language, fallback);
            foreach (var (row, element) in PageContent(scope, elements))
            {
                if (element.Fields == null) continue;
                foreach (var field in element.Fields)
                {
                    if (!BlockSummaryRule.IsTextField(field)) continue;
                    string text = BlockSummaryRule.OneLine(read.ItemText(row, field.Key));
                    if (text.Length > 0) return text;
                }
            }
            return "";
        }

        // Every searchable text of the block's own rows, each language's words as one entry, glossary marks read as their words, no repeats
        public static List<string> SearchWords(ElementScope scope, ElementRegistry elements)
        {
            var words = new List<string>();
            var rows = new List<BlockItemData>(BlockSurfaces.PageRows(scope?.Holder, scope?.HolderKind));
            rows.AddRange(BlockSurfaces.PreviewRows(scope?.Holder));
            foreach (var row in rows)
            {
                var element = new ElementRow(row);
                if (element.IsRef || elements == null || !elements.TryGet(element.Kind, out var definition)) continue;
                if (definition.SearchFields != null)
                    foreach (string key in definition.SearchFields)
                        AddEntries(words, element.Field(key)?.text);
                if (definition.SearchesCaptions)
                    foreach (var media in element.Media) AddEntries(words, media.Data?.text);
            }
            return words;
        }

        // The tap a row has: its stored tap when its element offers that one, else the element's default, else none
        public static string TapOf(ElementRow row, ElementKindDefinition element)
        {
            if (element?.TapChoices == null || element.TapChoices.Count == 0) return ElementOptions.TapNone;
            return element.OffersTap(row.Tap) ? row.Tap : element.TapChoices[0];
        }

        // The words of a text, glossary marks as their shown words
        public static int WordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return GlossaryMarkup.PlainText(text).Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries).Length;
        }

        // Each Page row that shows something, as (the row whose content shows, its element), in Page order
        private static IEnumerable<(BlockItemData, ElementKindDefinition)> PageContent(ElementScope scope, ElementRegistry elements)
        {
            if (scope == null || elements == null) yield break;
            foreach (var row in BlockSurfaces.PageRows(scope.Holder, scope.HolderKind))
            {
                var content = ElementRefRule.ContentOf(row, scope);
                if (content.IsBroken || !elements.TryGet(new ElementRow(content.Target).Kind, out var element)) continue;
                yield return (content.Target, element);
            }
        }

        private static void AddEntries(List<string> words, List<LocalizedEntry> entries)
        {
            if (entries == null) return;
            foreach (var e in entries)
            {
                string text = BlockSummaryRule.OneLine(e?.value);
                if (text.Length > 0 && !words.Contains(text)) words.Add(text);
            }
        }
    }
}
