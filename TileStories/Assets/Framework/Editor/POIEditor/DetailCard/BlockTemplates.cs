// BlockTemplates.cs
//
// Detail Card > Block Library > Templates (_3.3 R14.4, R14.5): named compositions of element rows -- each row's element, look, motion and
// tap, never content -- that a point's block is made FROM. A template is a PRESET, like a Card Starter or a navigation Preset: adding a
// block from one COPIES its rows (fresh ids) and nothing stays linked, so the point then owns its structure. "Re-apply" adds back the
// template's rows the block lacks, matched by STRUCTURE (the order of the rows' elements), never touching a row the block has or any
// content. The framework's built-in templates ship here as code; a wall's own live in card_settings.block_templates. A template is held
// as a BlockInstanceData with no content, so the story Editor's table and edits work on it unchanged. Pure: no window, no IMGUI.

using System.Collections.Generic;

namespace TileStories.Editor
{
    internal static class BlockTemplates
    {
        // A wall template's key (template_N, never shown) and the built-ins' (never stored in a wall's list)
        public const string KeyPrefix = "template_";
        public const string BuiltInPrefix = "builtin_";
        public const string MagazineKey = BuiltInPrefix + "story_magazine";
        public const string ShortKey = BuiltInPrefix + "story_short";

        // The "+ Add block" picker of templates (NamedOptionPickerPopup): one open picker of this kind at a time
        public const string PickerKind = "block-template-picker";
        public const string PickerTitle = "Add A Block From A Template";

        public static bool IsBuiltIn(string key) => key != null && key.StartsWith(BuiltInPrefix, System.StringComparison.Ordinal);

        // ---------------- the templates ----------------

        // The framework's built-in templates, new objects each call (an edit of one never reaches the code's): "Story -- magazine", the
        // shipped story's structure with its looks (a picture full bleed, eyebrow, title, lede, a drop-cap paragraph, headed parts, a callout,
        // a pull quote, a pair, a tile ornament, a carousel, a video); "Story -- short": title, lede, paragraph, picture
        public static List<BlockTemplateData> BuiltIn(ElementRegistry elements)
        {
            var magazine = NewTemplate(MagazineKey, "Story -- magazine", BuiltInBlocks.StoryKind);
            AddRow(magazine, elements, BuiltInElements.Picture, (BuiltInElements.WidthStyle, BuiltInElements.WidthFullBleed));
            AddRow(magazine, elements, BuiltInElements.Eyebrow);
            AddRow(magazine, elements, BuiltInElements.Title);
            AddRow(magazine, elements, BuiltInElements.Lede);
            AddRow(magazine, elements, BuiltInElements.Paragraph, (BuiltInElements.DropCapStyle, BuiltInElements.On));
            AddRow(magazine, elements, BuiltInElements.Picture);
            AddRow(magazine, elements, BuiltInElements.Heading);
            AddRow(magazine, elements, BuiltInElements.Paragraph);
            AddRow(magazine, elements, BuiltInElements.Callout, (BuiltInElements.ToneStyle, BuiltInElements.ToneFamily));
            AddRow(magazine, elements, BuiltInElements.Picture, (BuiltInElements.WidthStyle, BuiltInElements.WidthFullBleed));
            AddRow(magazine, elements, BuiltInElements.Heading);
            AddRow(magazine, elements, BuiltInElements.Paragraph);
            AddRow(magazine, elements, BuiltInElements.PullQuote);
            AddRow(magazine, elements, BuiltInElements.PicturePair);
            AddRow(magazine, elements, BuiltInElements.Divider, (BuiltInElements.OrnamentStyle, BuiltInElements.OrnamentAzulejo));
            AddRow(magazine, elements, BuiltInElements.Carousel);
            AddRow(magazine, elements, BuiltInElements.Video);

            var shortStory = NewTemplate(ShortKey, "Story -- short", BuiltInBlocks.StoryKind);
            AddRow(shortStory, elements, BuiltInElements.Title);
            AddRow(shortStory, elements, BuiltInElements.Lede);
            AddRow(shortStory, elements, BuiltInElements.Paragraph);
            AddRow(shortStory, elements, BuiltInElements.Picture);
            return new List<BlockTemplateData> { magazine, shortStory };
        }

        // Every template a wall offers, in the table's order: the built-ins, then the wall's own
        public static List<BlockTemplateData> All(CardSettings settings, ElementRegistry elements)
        {
            var all = BuiltIn(elements);
            if (settings?.block_templates != null)
                foreach (var t in settings.block_templates) if (t != null) all.Add(t);
            return all;
        }

        // The template with this key (a built-in, or the wall's own), or null
        public static BlockTemplateData Find(CardSettings settings, ElementRegistry elements, string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            foreach (var t in All(settings, elements))
                if (t.key == key) return t;
            return null;
        }

        // The words a template is shown by ("(unnamed template)" for a blank name: never its key)
        public static string NameOf(BlockTemplateData template) =>
            template == null ? "" : string.IsNullOrWhiteSpace(template.name) ? "(unnamed template)" : template.name;

        // A template's rows, in order
        public static IReadOnlyList<BlockItemData> RowsOf(BlockTemplateData template) =>
            BlockSurfaces.PageRows(template?.block, KindOf(template));

        // The kind a template makes, or null when no such kind is registered
        public static BlockKindDefinition KindOf(BlockTemplateData template) =>
            template?.block != null && BlockRegistry.Shared.TryGet(template.block.kind, out var kind) ? kind : null;

        // "Story, 4 rows: Title, Lede, Paragraph, Picture" (the picker's line under a template's name)
        public static string Describe(BlockTemplateData template, ElementRegistry elements)
        {
            var rows = RowsOf(template);
            var names = new List<string>();
            foreach (var row in rows) names.Add(elements.TryGet(new ElementRow(row).Kind, out var e) ? e.DisplayName : new ElementRow(row).Kind);
            string kind = KindOf(template)?.DisplayName ?? template?.block?.kind ?? "";
            return kind + ", " + (rows.Count == 1 ? "1 row" : rows.Count + " rows") + ": " + string.Join(", ", names);
        }

        // ---------------- copying rows ----------------

        // A NEW row of `into` with `source`'s structure: the element's defaults (ElementRow.Create: a fresh id, not validated, no content),
        // then the source's look, motion and tap. Null when the source's element is not registered (nothing to build it from)
        public static ElementRow? CopyStructure(BlockInstanceData into, ElementRow source, ElementRegistry elements)
        {
            if (!elements.TryGet(source.Kind, out var element)) return null;
            var row = ElementRow.Create(into, element);
            if (element.StyleFields != null)
                foreach (var style in element.StyleFields)
                    if (source.Field(style.Key) != null) row.SetValue(style.Key, source.Value(style.Key));
            row.SetMotion(source.Motion, source.Strength);
            if (element.TapChoices != null && source.Tap.Length > 0) row.SetValue(ElementRow.TapKey, source.Tap);
            return row;
        }

        // ---------------- the wall's templates ----------------

        // "Save as template" (Card Content): a new wall template from `block`'s Page -- each row's structure, no content -- named `name`.
        // The block is then made from it (its Template line names it). Returns the template
        public static BlockTemplateData SaveAs(CardSettings settings, IReadOnlyList<POIData> pois, BlockInstanceData block, BlockKindDefinition kind,
            string name, ElementRegistry elements)
        {
            var template = NewTemplate(NextKey(settings, pois), name, block.kind);
            template.block.variant = block.variant ?? "";
            template.block.display = block.display;
            var rows = TemplatePage(template, kind);
            foreach (var source in BlockSurfaces.PageRows(block, kind))
            {
                var copy = CopyStructure(template.block, new ElementRow(source), elements);
                if (copy != null) rows.Add(copy.Value.Data);
            }
            settings.block_templates ??= new List<BlockTemplateData>();
            settings.block_templates.Add(template);
            BlockSurfaces.SetTemplate(block, template.key);
            return template;
        }

        // A wall copy of any template (a built-in's is how a developer makes one to edit), named "<name> (copy)", right after the wall's own
        public static BlockTemplateData Duplicate(CardSettings settings, IReadOnlyList<POIData> pois, BlockTemplateData source, ElementRegistry elements)
        {
            var copy = NewTemplate(NextKey(settings, pois), NameOf(source) + " (copy)", source.block.kind);
            copy.block.variant = source.block.variant ?? "";
            copy.block.display = source.block.display;
            var kind = KindOf(source);
            var rows = TemplatePage(copy, kind);
            foreach (var row in RowsOf(source))
            {
                var made = CopyStructure(copy.block, new ElementRow(row), elements);
                if (made != null) rows.Add(made.Value.Data);
            }
            settings.block_templates ??= new List<BlockTemplateData>();
            settings.block_templates.Add(copy);
            return copy;
        }

        // Delete the wall's template `key` (a built-in cannot be). The blocks made from it keep every row; their Template line then says
        // the template is gone. Returns whether one went
        public static bool Remove(CardSettings settings, string key)
        {
            if (IsBuiltIn(key) || settings?.block_templates == null) return false;
            return settings.block_templates.RemoveAll(t => t != null && t.key == key) > 0;
        }

        // How many blocks of the wall were made from template `key`
        public static int BlocksMadeFrom(IReadOnlyList<POIData> pois, string key)
        {
            int count = 0;
            if (pois == null || string.IsNullOrEmpty(key)) return 0;
            foreach (var poi in pois)
                if (poi?.card?.blocks != null)
                    foreach (var block in poi.card.blocks)
                        if (BlockSurfaces.TemplateOf(block) == key) count++;
            return count;
        }

        // The key of a NEW wall template: template_N with the smallest N no template holds AND no block still names -- a block made from a
        // deleted template must never come to point at a new one (its Template line and Re-apply would follow the wrong rows)
        public static string NextKey(CardSettings settings, IReadOnlyList<POIData> pois)
        {
            var used = new HashSet<string>();
            if (settings?.block_templates != null)
                foreach (var t in settings.block_templates) if (t?.key != null) used.Add(t.key);
            if (pois != null)
                foreach (var poi in pois)
                    if (poi?.card?.blocks != null)
                        foreach (var block in poi.card.blocks) used.Add(BlockSurfaces.TemplateOf(block));
            int n = 1;
            while (used.Contains(KeyPrefix + n)) n++;
            return KeyPrefix + n;
        }

        // ---------------- using a template ----------------

        // A NEW block of `card` made from `template`: the next block key, the template's kind and look, a COPY of every row (fresh ids, the
        // rows' structure, no content) and the template's key remembered. The caller adds it to the card
        public static BlockInstanceData NewBlock(POICardData card, BlockTemplateData template, ElementRegistry elements)
        {
            var kind = KindOf(template);
            var block = new BlockInstanceData
            {
                key = TaxonomyRowKeys.TakeBlockKey(card),
                kind = template.block.kind,
                variant = template.block.variant ?? "",
                display = kind != null ? BlockDisplayRule.Resolve(kind, template.block.display) : CardOptions.DisplayInline,
            };
            if (kind != null)
            {
                var rows = ElementEdits.PageList(block, kind);
                foreach (var row in RowsOf(template))
                {
                    var copy = CopyStructure(block, new ElementRow(row), elements);
                    if (copy != null) rows.Add(copy.Value.Data);
                }
            }
            BlockSurfaces.SetTemplate(block, template.key);
            return block;
        }

        // "Re-apply" (R14.4): add to `block` the template's rows it lacks, by STRUCTURE -- the longest run of the template's elements that
        // the block's Page already holds in the same order counts as present (rows the developer added, moved apart or wrote stay as they
        // are); every other template row is added as a fresh copy, in its template place: right before the block row that holds the next
        // template row found, else right after the row of the last one found. Never removes, reorders or writes into a row the block has.
        // Returns the rows added, in order
        public static List<ElementRow> Reapply(BlockInstanceData block, BlockKindDefinition kind, BlockTemplateData template, ElementRegistry elements)
        {
            var added = new List<ElementRow>();
            var templateRows = RowsOf(template);
            var have = BlockSurfaces.PageRows(block, kind);
            var matchedTo = MatchByStructure(templateRows, have);

            var missing = new List<int>();
            for (int t = 0; t < templateRows.Count; t++)
                if (matchedTo[t] < 0 && elements.TryGet(new ElementRow(templateRows[t]).Kind, out _)) missing.Add(t);
            if (missing.Count == 0) return added;

            // - each missing row's anchor, fixed before inserting: the block row held by the next template row that is present (it goes right
            //   before it); with none after it, the row held by the last present one (it goes after it, so a row the developer added at the
            //   end stays last); with none at all, the end
            int lastPresent = -1;
            for (int t = 0; t < templateRows.Count; t++) if (matchedTo[t] >= 0) lastPresent = t;
            var before = new List<BlockItemData>();
            foreach (int t in missing)
            {
                BlockItemData anchor = null;
                for (int next = t + 1; next < templateRows.Count && anchor == null; next++)
                    if (matchedTo[next] >= 0) anchor = have[matchedTo[next]];
                before.Add(anchor);
            }
            var after = lastPresent >= 0 ? have[matchedTo[lastPresent]] : null;
            var rows = ElementEdits.PageList(block, kind);
            int trailing = 0;
            for (int m = 0; m < missing.Count; m++)
            {
                var copy = CopyStructure(block, new ElementRow(templateRows[missing[m]]), elements).Value;
                int at;
                if (before[m] != null) at = rows.IndexOf(before[m]);
                else if (after != null) at = rows.IndexOf(after) + 1 + trailing++;
                else at = rows.Count;
                rows.Insert(at < 0 || at > rows.Count ? rows.Count : at, copy.Data);
                added.Add(copy);
            }
            return added;
        }

        // For each template row, the index of the block row it is matched to (-1 = missing): the longest common subsequence of the two
        // lists' element kinds, ties resolved towards the earliest rows
        internal static int[] MatchByStructure(IReadOnlyList<BlockItemData> template, IReadOnlyList<BlockItemData> block)
        {
            int n = template.Count, m = block.Count;
            var lcs = new int[n + 1, m + 1];
            for (int i = n - 1; i >= 0; i--)
                for (int j = m - 1; j >= 0; j--)
                    lcs[i, j] = SameElement(template[i], block[j]) ? lcs[i + 1, j + 1] + 1 : System.Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
            var matched = new int[n];
            for (int k = 0; k < n; k++) matched[k] = -1;
            int a = 0, b = 0;
            while (a < n && b < m)
            {
                if (SameElement(template[a], block[b]) && lcs[a, b] == lcs[a + 1, b + 1] + 1) { matched[a] = b; a++; b++; }
                else if (lcs[a + 1, b] >= lcs[a, b + 1]) a++;
                else b++;
            }
            return matched;
        }

        // What Re-apply tells the developer it did (a notice: the result of their own click)
        public static string ReapplyMessage(string templateName, IReadOnlyList<ElementRow> added, ElementRegistry elements)
        {
            if (added.Count == 0) return "Nothing to add: this block already has every row of \"" + templateName + "\". Its own rows and words are unchanged.";
            var names = new List<string>();
            foreach (var row in added) names.Add(elements.TryGet(row.Kind, out var e) ? e.DisplayName : row.Kind);
            return "Added " + (added.Count == 1 ? "1 row" : added.Count + " rows") + " from \"" + templateName + "\": " + string.Join(", ", names) +
                   ". Each is empty, in its place in the template; the block's other rows and words are unchanged. Ctrl+Z takes them back.";
        }

        // ---------------- helpers ----------------

        private static bool SameElement(BlockItemData a, BlockItemData b) => new ElementRow(a).Kind == new ElementRow(b).Kind;

        private static BlockTemplateData NewTemplate(string key, string name, string kindKey)
        {
            var template = new BlockTemplateData { key = key, name = name ?? "", block = new BlockInstanceData { key = "", kind = kindKey } };
            if (BlockRegistry.Shared.TryGet(kindKey, out var kind) && kind.DisplayModes != null && kind.DisplayModes.Count > 0)
                template.block.display = kind.DisplayModes[0];
            return template;
        }

        // The template block's Page list (created), or a throwaway list for a kind that composes nothing
        private static List<BlockItemData> TemplatePage(BlockTemplateData template, BlockKindDefinition kind) =>
            kind != null && BlockSurfaces.Composes(kind) ? ElementEdits.PageList(template.block, kind) : new List<BlockItemData>();

        // A built-in row: the element's defaults, then the given looks
        private static void AddRow(BlockTemplateData template, ElementRegistry elements, string element, params (string Key, string Value)[] looks)
        {
            if (!elements.TryGet(element, out var definition)) return;
            var row = ElementRow.Create(template.block, definition);
            foreach (var (key, value) in looks) row.SetValue(key, value);
            TemplatePage(template, KindOf(template)).Add(row.Data);
        }
    }
}
