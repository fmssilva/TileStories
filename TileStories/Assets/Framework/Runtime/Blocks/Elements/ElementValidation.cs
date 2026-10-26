using System.Collections.Generic;

namespace TileStories
{
    // What is wrong with a composed block's rows (_3.3 R14.1, R17.8), pure: every problem names its ROW -- the surface, the row's number and
    // its element's name -- never an id alone, so the Editor can say "Page row 3 (Paragraph): Text is empty" (S3 words it). Checked per row:
    //   - an element kind the registry does not know          -> UnknownElement
    //   - a required content field with nothing in it         -> MissingRequired (the field named)
    //   - a media file the wall's media rule refuses           -> InvalidMedia (the item named)
    //   - fewer or more media items than the element holds     -> MediaCount
    //   - a reference that resolves to nothing                 -> BrokenRef (with ElementRefRule's reason)
    //   - a blank id / an id an earlier row of the block holds -> MissingId / DuplicateId (ids are unique across Page AND Preview)
    // A row with no UnknownElement / MissingRequired / InvalidMedia / MediaCount / BrokenRef problem is COMPLETE: what the card can show.
    public static class ElementValidation
    {
        public enum Surface { Page, Preview }

        public enum Kind { UnknownElement, MissingRequired, InvalidMedia, MediaCount, BrokenRef, MissingId, DuplicateId }

        public readonly struct Problem
        {
            public readonly Surface Surface;
            // 1-based, as the Editor's table numbers its rows
            public readonly int Row;
            public readonly string RowId;
            // The element's display name ("Paragraph"); "" for a reference or an unknown element
            public readonly string ElementName;
            public readonly Kind Kind;
            // MissingRequired: the field's key and label
            public readonly string FieldKey;
            public readonly string FieldLabel;
            // InvalidMedia: the 1-based media item; MediaCount: how many items the row holds
            public readonly int Media;
            public readonly ElementRefRule.Problem RefProblem;

            public Problem(Surface surface, int row, string rowId, string elementName, Kind kind, string fieldKey = "", string fieldLabel = "",
                int media = 0, ElementRefRule.Problem refProblem = ElementRefRule.Problem.None)
            {
                Surface = surface;
                Row = row;
                RowId = rowId ?? "";
                ElementName = elementName ?? "";
                Kind = kind;
                FieldKey = fieldKey ?? "";
                FieldLabel = fieldLabel ?? "";
                Media = media;
                RefProblem = refProblem;
            }
        }

        // Every problem of the block `scope` holds: its Page rows, then its Preview rows, each in order
        public static List<Problem> Of(ElementScope scope, ElementRegistry elements)
        {
            var problems = new List<Problem>();
            var ids = new HashSet<string>();
            Check(problems, ids, Surface.Page, BlockSurfaces.PageRows(scope?.Holder, scope?.HolderKind), scope, elements);
            Check(problems, ids, Surface.Preview, BlockSurfaces.PreviewRows(scope?.Holder), scope, elements);
            return problems;
        }

        // Whether one row is complete: the card can show it (its identity does not matter here: an id problem never hides content)
        public static bool RowIsComplete(BlockItemData row, ElementScope scope, ElementRegistry elements)
        {
            var problems = new List<Problem>();
            CheckContent(problems, Surface.Page, 1, row, scope, elements);
            return problems.Count == 0;
        }

        private static void Check(List<Problem> problems, HashSet<string> ids, Surface surface, IReadOnlyList<BlockItemData> rows,
            ElementScope scope, ElementRegistry elements)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var row = new ElementRow(rows[i]);
                string name = NameOf(row, elements);
                if (string.IsNullOrWhiteSpace(row.Id)) problems.Add(new Problem(surface, i + 1, row.Id, name, Kind.MissingId));
                else if (!ids.Add(row.Id)) problems.Add(new Problem(surface, i + 1, row.Id, name, Kind.DuplicateId));
                CheckContent(problems, surface, i + 1, rows[i], scope, elements);
            }
        }

        // The problems that keep a row from showing
        private static void CheckContent(List<Problem> problems, Surface surface, int number, BlockItemData data, ElementScope scope, ElementRegistry elements)
        {
            var row = new ElementRow(data);
            if (row.IsRef)
            {
                var resolved = ElementRefRule.Resolve(row.Ref, scope);
                if (resolved.IsBroken) problems.Add(new Problem(surface, number, row.Id, "", Kind.BrokenRef, refProblem: resolved.Problem));
                return;
            }
            if (elements == null || !elements.TryGet(row.Kind, out var element))
            {
                problems.Add(new Problem(surface, number, row.Id, "", Kind.UnknownElement));
                return;
            }
            if (element.Fields != null)
                foreach (var field in element.Fields)
                    if (field.Required && !BlockFieldReader.HasItemContent(row.Field(field.Key), field))
                        problems.Add(new Problem(surface, number, row.Id, element.DisplayName, Kind.MissingRequired, field.Key, field.Label));
            if (element.Media == MediaKind.None) return;
            var media = row.Media;
            for (int m = 0; m < media.Count; m++)
                if (!MediaPathRule.IsValid(media[m].Asset, element.Media))
                    problems.Add(new Problem(surface, number, row.Id, element.DisplayName, Kind.InvalidMedia, media: m + 1));
            if (media.Count < element.MinMedia || media.Count > element.MaxMedia)
                problems.Add(new Problem(surface, number, row.Id, element.DisplayName, Kind.MediaCount, media: media.Count));
        }

        private static string NameOf(ElementRow row, ElementRegistry elements) =>
            !row.IsRef && elements != null && elements.TryGet(row.Kind, out var element) ? element.DisplayName : "";
    }
}
