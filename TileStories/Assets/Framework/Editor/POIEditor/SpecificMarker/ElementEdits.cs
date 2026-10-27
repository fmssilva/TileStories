// ElementEdits.cs
//
// Pure: every edit the story's Editor makes to a composed block (_3.3 R14.5, R14.8, R17.1, R17.5), on the block's own data -- the POI
// Editor's element table decides WHAT while drawing and calls one of these after its row loop, inside the tab's DrawConfigMutationScope
// (one undo step, the unsaved flag, the live Play Mode push). No window, no IMGUI: a test runs each with `new` data.

using System.Collections.Generic;
using UnityEngine;

namespace TileStories.Editor
{
    internal static class ElementEdits
    {
        // ---------------- rows of a surface ----------------

        // The block's Page rows as stored, the list created by the first edit (never by drawing)
        public static List<BlockItemData> PageList(BlockInstanceData block, BlockKindDefinition kind) =>
            Ensure(block, BlockSurfaces.PageFieldOf(kind).Key).items;

        // The block's Custom preview rows as stored, created by the first edit
        public static List<BlockItemData> PreviewList(BlockInstanceData block) => Ensure(block, BlockSurfaces.PreviewRowsField).items;

        // A new row of `element` at the end of `rows`, born with the element's defaults and a fresh id (ElementRow.Create)
        public static ElementRow Add(BlockInstanceData block, List<BlockItemData> rows, ElementKindDefinition element)
        {
            var row = ElementRow.Create(block, element);
            rows.Add(row.Data);
            return row;
        }

        // A copy of row `at`, right under it: every value and media item, a fresh id, not validated (ElementRow.Duplicate)
        public static ElementRow Duplicate(BlockInstanceData block, List<BlockItemData> rows, int at)
        {
            var copy = ElementRow.Duplicate(block, new ElementRow(rows[at]));
            rows.Insert(at + 1, copy.Data);
            return copy;
        }

        // Move row `from` to place `to` (a drag, or one step up / down); out of range does nothing
        public static void Move(List<BlockItemData> rows, int from, int to)
        {
            if (rows == null || from < 0 || from >= rows.Count || to < 0 || to >= rows.Count || from == to) return;
            var row = rows[from];
            rows.RemoveAt(from);
            rows.Insert(to, row);
        }

        // Delete row `at`. A preview link to a deleted Page row stays, as a warning row (Unlink / Remove): never a silent blank
        public static void Remove(List<BlockItemData> rows, int at)
        {
            if (rows != null && at >= 0 && at < rows.Count) rows.RemoveAt(at);
        }

        // ---------------- a row's media items ----------------

        // Move media item `from` of a row to place `to` (the items keep the row's other sub-fields where they are); out of range does nothing
        public static void MoveMedia(ElementRow row, int from, int to)
        {
            var places = MediaPlaces(row);
            if (from < 0 || from >= places.Count || to < 0 || to >= places.Count || from == to) return;
            var fields = row.Data.fields;
            var items = new List<BlockItemFieldValue>();
            foreach (int p in places) items.Add(fields[p]);
            var moved = items[from];
            items.RemoveAt(from);
            items.Insert(to, moved);
            for (int i = 0; i < places.Count; i++) fields[places[i]] = items[i];
        }

        // Delete media item `at` of a row
        public static void RemoveMedia(ElementRow row, int at)
        {
            var places = MediaPlaces(row);
            if (at >= 0 && at < places.Count) row.Data.fields.RemoveAt(places[at]);
        }

        // Where a row's media items sit among its sub-fields, in order
        private static List<int> MediaPlaces(ElementRow row)
        {
            var places = new List<int>();
            if (row.Data?.fields == null) return places;
            for (int i = 0; i < row.Data.fields.Count; i++)
                if (row.Data.fields[i]?.key == ElementRow.MediaKey) places.Add(i);
            return places;
        }

        // ---------------- the Preview: Automatic / Custom, links ----------------

        // Switch the preview to `mode`. Switching to Custom with no rows of its own yet starts it as the Automatic preview showed it: one
        // link per row the Automatic preview took, so nothing on the card changes at the switch. Switching back keeps the Custom rows
        public static void SetPreviewMode(ElementScope scope, ElementRegistry elements, string mode)
        {
            var block = scope.Holder;
            mode = ElementOptions.OneOf(mode, ElementOptions.PreviewModes);
            if (mode == ElementOptions.PreviewCustom && BlockSurfaces.PreviewMode(block) != ElementOptions.PreviewCustom
                && BlockSurfaces.PreviewRows(block).Count == 0)
            {
                var picks = ElementPreviewRule.Of(scope, elements);
                var rows = PreviewList(block);
                foreach (var entry in picks.Entries) rows.Add(Link(scope, entry.PageRowId).Data);
            }
            Ensure(block, BlockSurfaces.PreviewModeField).value = mode;
        }

        // A Page row into the preview (R17.1): `linked` -- a link (same content, its own style; edited once, on the Page) -- else a copy (an
        // own row the preview alone holds). The preview turns Custom first (SetPreviewMode), so the Automatic picks stay where they were
        public static ElementRow UseInPreview(ElementScope scope, ElementRegistry elements, string pageRowId, bool linked)
        {
            SetPreviewMode(scope, elements, ElementOptions.PreviewCustom);
            var rows = PreviewList(scope.Holder);
            if (linked)
            {
                var link = Link(scope, pageRowId);
                rows.Add(link.Data);
                return link;
            }
            var source = Find(BlockSurfaces.PageRows(scope.Holder, scope.HolderKind), pageRowId);
            var copy = ElementRow.Duplicate(scope.Holder, new ElementRow(source));
            rows.Add(copy.Data);
            return copy;
        }

        // A link of the block's preview to its Page row `pageRowId`. It also records the element it shows, so a link whose row is later
        // deleted still knows what it was (Unlink then gives an empty row of that element to write again)
        public static ElementRow Link(ElementScope scope, string pageRowId)
        {
            var link = ElementRow.CreateRef(scope.Holder, ElementRef.ToPage(pageRowId));
            var target = Find(BlockSurfaces.PageRows(scope.Holder, scope.HolderKind), pageRowId);
            if (target != null) link.SetValue(ElementRow.ElementKey, new ElementRow(target).Kind);
            return link;
        }

        // Turn preview row `at` from a link into a row of its own ("Unlink (make a copy)"): the target's content and media copied, with
        // the link's own style, motion, tap, id and Validated mark. A broken link becomes an empty row of the element it showed
        public static void Unlink(ElementScope scope, ElementRegistry elements, int at)
        {
            var rows = PreviewList(scope.Holder);
            if (at < 0 || at >= rows.Count) return;
            var link = new ElementRow(rows[at]);
            if (!link.IsRef) return;
            var resolved = ElementRefRule.Resolve(link.Ref, scope);
            BlockItemData own;
            if (!resolved.IsBroken) own = ElementRow.Duplicate(scope.Holder, new ElementRow(resolved.Target)).Data;
            else if (elements != null && elements.TryGet(link.Kind, out var element)) own = ElementRow.Create(scope.Holder, element).Data;
            else
            {
                rows.RemoveAt(at);
                return;
            }
            var made = new ElementRow(own);
            // - what the link set for itself wins over the copied row's: its look, its motion, its tap, its identity and its mark
            ElementKindDefinition kind = null;
            elements?.TryGet(made.Kind, out kind);
            if (kind?.StyleFields != null)
                foreach (var style in kind.StyleFields)
                    if (link.Field(style.Key) != null) made.SetValue(style.Key, link.Value(style.Key));
            made.SetMotion(link.Motion, link.Strength);
            if (link.Tap.Length > 0) made.SetValue(ElementRow.TapKey, link.Tap);
            made.SetValue(ElementRow.IdKey, link.Id);
            made.SetValidated(link.Validated);
            rows[at] = own;
        }

        // ---------------- the block's own settings ----------------

        // Turn the block's override of a shared component on or off (R17.5, the _2.0 label-style pattern): on SEEDS it from the wall's
        // values, so the card does not jump; off keeps the block's values (ignored until it is turned on again, which seeds again)
        public static void SetOverride(BlockInstanceData block, string component, bool on, CardComponentSettings wall)
        {
            wall ??= new CardComponentSettings();
            var row = OverrideRow(block, component);
            Item(row, BlockSurfaces.OverrideKey).flag = on;
            if (!on) return;
            if (component == ElementOptions.ComponentOpenRow)
                Item(row, BlockSurfaces.EmphasisKey).value = ElementOptions.OneOf(wall.open_row_emphasis, ElementOptions.OpenRowEmphases);
            else if (component == ElementOptions.ComponentPageChrome)
            {
                Item(row, BlockSurfaces.BreadcrumbKey).value = wall.page_chrome_breadcrumb ? ElementOptions.ChromeShown : ElementOptions.ChromeHidden;
                Item(row, BlockSurfaces.PrevNextKey).value = wall.page_chrome_prev_next ? ElementOptions.ChromeShown : ElementOptions.ChromeHidden;
            }
        }

        // The block's Component Overrides row of `component` -- the very row the card reads (BlockSurfaces.OverrideOf: the first naming it) --
        // created when missing
        public static BlockItemData OverrideRow(BlockInstanceData block, string component)
        {
            var existing = BlockSurfaces.OverrideOf(block, component).Row;
            if (existing != null) return existing;
            var rows = Ensure(block, BlockSurfaces.ComponentOverridesField).items;
            var made = new BlockItemData();
            made.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.ComponentKey, value = component });
            rows.Add(made);
            return made;
        }

        // Reading Time: 0 = Auto (computed from the Page), else the minutes set by hand
        public static void SetMinutes(BlockInstanceData block, int minutes) =>
            Ensure(block, BlockKindDefinition.MinutesDefinition.Key).number = Mathf.Max(0, minutes);

        // ---------------- helpers ----------------

        // The row with this id in `rows`, or null
        public static BlockItemData Find(IReadOnlyList<BlockItemData> rows, string id)
        {
            if (rows == null || string.IsNullOrEmpty(id)) return null;
            foreach (var row in rows)
                if (row != null && new ElementRow(row).Id == id) return row;
            return null;
        }

        // The block's field of this key, created when missing (only an edit calls it)
        private static BlockFieldValue Ensure(BlockInstanceData block, string key)
        {
            block.fields ??= new List<BlockFieldValue>();
            var value = block.fields.Find(f => f != null && f.key == key);
            if (value == null) block.fields.Add(value = new BlockFieldValue { key = key });
            value.items ??= new List<BlockItemData>();
            return value;
        }

        // A row's sub-field of this key, created when missing
        private static BlockItemFieldValue Item(BlockItemData row, string key)
        {
            row.fields ??= new List<BlockItemFieldValue>();
            var value = row.fields.Find(f => f != null && f.key == key);
            if (value == null) row.fields.Add(value = new BlockItemFieldValue { key = key });
            return value;
        }
    }
}
