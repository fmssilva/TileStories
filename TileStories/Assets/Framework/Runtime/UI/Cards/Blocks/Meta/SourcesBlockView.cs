using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The sources block (_3.1 Tier 1, family meta): where the card's content comes from, under the stack's heading
    // (default: CardStrings sources_heading).
    // Each source is its title, then its author and licence on one wrapping line (an empty one takes no space). Two
    // looks -- list: the sources; with_confidence: the same, plus a chip saying whether the content was checked
    // (Verified) or is a draft (Content Status; no chip while none is set). Only classes here.
    public sealed class SourcesBlockView : IBlockView
    {
        public sealed class Row
        {
            public VisualElement Box;
            public Label Title;
            public Label Author;
            public Label Licence;
        }

        public VisualElement Root { get; }
        public Label Confidence { get; }
        public IReadOnlyList<Row> Rows => _rows.Shown;

        private readonly VisualElement _list;
        private readonly ElementPool<Row> _rows = new(_ => NewRow(), row => row.Box.RemoveFromHierarchy());

        public SourcesBlockView()
        {
            Root = new VisualElement { name = "card-sources" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-sources");
            Confidence = new Label { name = "card-sources-confidence" };
            Confidence.AddToClassList("card-sources__chip");
            Root.Add(Confidence);
            _list = new VisualElement();
            Root.Add(_list);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);

            string status = read.Value(BuiltInBlocks.SourcesStatusField);
            bool verified = status == BuiltInBlocks.ContentVerified, draft = status == BuiltInBlocks.ContentDraft;
            bool chip = context.Variant == BuiltInBlocks.SourcesWithConfidence && (verified || draft);
            Confidence.text = !chip ? "" : context.Strings?.Get(verified ? CardStrings.Keys.SourcesVerified : CardStrings.Keys.SourcesDraft) ?? "";
            Confidence.style.display = chip ? DisplayStyle.Flex : DisplayStyle.None;
            Confidence.EnableInClassList("card-sources__chip--verified", chip && verified);
            Confidence.EnableInClassList("card-sources__chip--draft", chip && draft);

            foreach (var item in read.Items(BuiltInBlocks.SourcesItemsField))
            {
                string title = read.ItemText(item, BuiltInBlocks.SourcesTitleField);
                string author = read.ItemText(item, BuiltInBlocks.SourcesAuthorField);
                string licence = read.ItemText(item, BuiltInBlocks.SourcesLicenceField);
                if (title.Length == 0 && author.Length == 0 && licence.Length == 0) continue;
                var row = _rows.Take();
                Show(row.Title, title);
                Show(row.Author, author);
                Show(row.Licence, licence);
                _list.Add(row.Box);
            }
        }

        public void Unbind() => _rows.ReleaseAll();

        private static void Show(Label label, string text)
        {
            label.text = text;
            label.style.display = text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // One source's row: its title over the author and licence
        private static Row NewRow()
        {
            var row = new Row { Box = new VisualElement(), Title = new Label(), Author = new Label(), Licence = new Label() };
            row.Box.AddToClassList("card-sources__row");
            row.Title.AddToClassList("card-sources__title");
            var meta = new VisualElement();
            meta.AddToClassList("card-sources__meta");
            row.Author.AddToClassList("card-sources__author");
            row.Licence.AddToClassList("card-sources__licence");
            meta.Add(row.Author);
            meta.Add(row.Licence);
            row.Box.Add(row.Title);
            row.Box.Add(meta);
            return row;
        }
    }
}
