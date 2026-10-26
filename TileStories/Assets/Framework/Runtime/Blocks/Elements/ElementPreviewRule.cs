using System.Collections.Generic;

namespace TileStories
{
    // What a composed block's Preview shows (_3.3 R14.3, R17.1, R17.7), pure and recomputed on every Page edit:
    //   Automatic -- the Page's first picture-like row, first title-like row and first text row (ElementKindDefinition.PreviewRole), in PAGE
    //               order; a Page reference plays its target's role; a broken one, or a row of an unknown element, plays none
    //   Custom    -- the Preview Rows as listed: an own row as itself, a linked row as its target (the link row keeps its own style and
    //               motion), a broken link as a Broken entry (the visitor sees nothing; the Editor shows a warning row)
    // Result.UsedPageRows: the ids of the Page rows the preview shows -- the Editor's "P" marks, and the live "follows the Page" line.
    public static class ElementPreviewRule
    {
        // Where an entry's content comes from
        public enum Source
        {
            // Automatic: a Page row (its own content, or its reference's target)
            Page,
            // Custom: a row only the preview holds
            Own,
            // Custom: a link to another row, resolved
            Linked,
            // Custom: a link that resolves to nothing
            Broken,
        }

        public readonly struct Entry
        {
            // The row on its surface: the Page row (Automatic) or the Preview row (Custom)
            public readonly BlockItemData Row;
            // The row whose content shows (the target for a link); null when Broken
            public readonly BlockItemData Content;
            public readonly Source Source;
            public readonly ElementRefRule.Problem Problem;
            // The id of the Page row this entry shows, "" when it shows none (an own row, a link outside the Page, a broken link)
            public readonly string PageRowId;

            public Entry(BlockItemData row, BlockItemData content, Source source, ElementRefRule.Problem problem, string pageRowId)
            {
                Row = row;
                Content = content;
                Source = source;
                Problem = problem;
                PageRowId = pageRowId ?? "";
            }
        }

        public sealed class Result
        {
            public string Mode;
            public readonly List<Entry> Entries = new();
            public readonly List<string> UsedPageRows = new();
        }

        // The preview of the block `scope` holds
        public static Result Of(ElementScope scope, ElementRegistry elements)
        {
            var block = scope?.Holder;
            var result = new Result { Mode = BlockSurfaces.PreviewMode(block) };
            if (result.Mode == ElementOptions.PreviewCustom) Custom(result, scope);
            else Automatic(result, scope, elements);
            return result;
        }

        // The first row of each role, in Page order; a title lead (an eyebrow) right before the title taken comes with it, just above it
        private static void Automatic(Result result, ElementScope scope, ElementRegistry elements)
        {
            var taken = new HashSet<ElementPreviewRole>();
            // - the row just before this one, when it is a title lead (else null): only a lead touching the title joins
            (BlockItemData Row, BlockItemData Content)? lead = null;
            foreach (var row in BlockSurfaces.PageRows(scope.Holder, scope.HolderKind))
            {
                var content = ElementRefRule.ContentOf(row, scope);
                if (content.IsBroken || elements == null || !elements.TryGet(new ElementRow(content.Target).Kind, out var element))
                {
                    lead = null;
                    continue;
                }
                var role = element.PreviewRole;
                var before = lead;
                lead = role == ElementPreviewRole.TitleLead ? (row, content.Target) : null;
                if (role == ElementPreviewRole.None || role == ElementPreviewRole.TitleLead || !taken.Add(role)) continue;
                if (role == ElementPreviewRole.Title && before.HasValue) Add(result, before.Value.Row, before.Value.Content);
                Add(result, row, content.Target);
            }
        }

        private static void Add(Result result, BlockItemData row, BlockItemData content)
        {
            string id = new ElementRow(row).Id;
            result.Entries.Add(new Entry(row, content, Source.Page, ElementRefRule.Problem.None, id));
            result.UsedPageRows.Add(id);
        }

        // The Preview Rows as listed, links resolved
        private static void Custom(Result result, ElementScope scope)
        {
            foreach (var row in BlockSurfaces.PreviewRows(scope.Holder))
            {
                var element = new ElementRow(row);
                if (!element.IsRef)
                {
                    result.Entries.Add(new Entry(row, row, Source.Own, ElementRefRule.Problem.None, ""));
                    continue;
                }
                var resolved = ElementRefRule.Resolve(element.Ref, scope);
                if (resolved.IsBroken)
                {
                    result.Entries.Add(new Entry(row, null, Source.Broken, resolved.Problem, ""));
                    continue;
                }
                string pageRow = element.Ref.Scope == ElementOptions.ScopePage ? element.Ref.Row : "";
                result.Entries.Add(new Entry(row, resolved.Target, Source.Linked, ElementRefRule.Problem.None, pageRow));
                if (pageRow.Length > 0 && !result.UsedPageRows.Contains(pageRow)) result.UsedPageRows.Add(pageRow);
            }
        }
    }
}
