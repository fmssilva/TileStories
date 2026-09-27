using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The sources block (_3.1 Tier 1, family meta): where the card's content comes from, under a heading (CardStrings).
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
        public Label Heading { get; }
        public Label Confidence { get; }
        public IReadOnlyList<Row> Rows => _shown;

        private readonly VisualElement _list;
        private readonly List<Row> _pool = new();
        private readonly List<Row> _shown = new();

        public SourcesBlockView()
        {
            Root = new VisualElement { name = "card-sources" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-sources");
            var top = new VisualElement();
            top.AddToClassList("card-sources__top");
            Heading = new Label();
            Heading.AddToClassList("card-sources__heading");
            Confidence = new Label { name = "card-sources-confidence" };
            Confidence.AddToClassList("card-sources__chip");
            top.Add(Heading);
            top.Add(Confidence);
            Root.Add(top);
            _list = new VisualElement();
            Root.Add(_list);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            Heading.text = context.Strings?.Get(CardStrings.Keys.SourcesHeading) ?? "";

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
                var row = Take(_shown.Count);
                Show(row.Title, title);
                Show(row.Author, author);
                Show(row.Licence, licence);
                _list.Add(row.Box);
                _shown.Add(row);
            }
        }

        public void Unbind()
        {
            foreach (var row in _shown) row.Box.RemoveFromHierarchy();
            _shown.Clear();
        }

        private static void Show(Label label, string text)
        {
            label.text = text;
            label.style.display = text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private Row Take(int index)
        {
            while (_pool.Count <= index)
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
                _pool.Add(row);
            }
            return _pool[index];
        }
    }
}
