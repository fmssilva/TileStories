using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The rich_text block (_3.1 Tier 1): the body's paragraphs with glossary words (CardTextView), in four looks --
    // plain; drop_cap (a raised initial beside the first paragraph); lede (a larger first paragraph); sections (the body
    // as an introduction, then each Sections item as a heading and its own paragraphs). Only classes here.
    public sealed class RichTextBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public CardTextView Body { get; }
        // The shown sections, in order (sections variant only)
        public IReadOnlyList<(VisualElement Box, Label Heading, CardTextView Text)> Sections => _sections.Shown;

        private readonly VisualElement _sectionsRoot;
        private readonly ElementPool<(VisualElement Box, Label Heading, CardTextView Text)> _sections = new(_ => NewSection(), section =>
        {
            section.Text.Clear();
            section.Box.RemoveFromHierarchy();
        });

        public RichTextBlockView()
        {
            Root = new VisualElement { name = "card-rich-text" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-rich");
            Body = new CardTextView("card-rich__paragraph");
            Root.Add(Body.Root);
            _sectionsRoot = new VisualElement();
            _sectionsRoot.AddToClassList("card-rich__sections");
            Root.Add(_sectionsRoot);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string variant = context.Variant;
            var first = variant == BuiltInBlocks.RichTextDropCap ? CardTextView.FirstParagraph.DropCap
                : variant == BuiltInBlocks.RichTextLede ? CardTextView.FirstParagraph.Lede
                : CardTextView.FirstParagraph.Normal;
            Body.Bind(GlossaryMarkup.Paragraphs(read.Text(BuiltInBlocks.RichTextBodyField)), context.Glossary, first);

            _sections.ReleaseAll();
            if (variant != BuiltInBlocks.RichTextSections) return;
            var items = read.Items(BuiltInBlocks.RichTextSectionsField);
            foreach (var item in items)
            {
                string heading = read.ItemText(item, BuiltInBlocks.RichTextSectionTitleField);
                var paragraphs = GlossaryMarkup.Paragraphs(read.ItemText(item, BuiltInBlocks.RichTextSectionBodyField));
                if (heading.Length == 0 && paragraphs.Count == 0) continue;
                var section = _sections.Take();
                section.Heading.text = heading;
                section.Heading.style.display = heading.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                section.Text.Bind(paragraphs, context.Glossary);
                _sectionsRoot.Add(section.Box);
            }
        }

        public void Unbind()
        {
            Body.Clear();
            _sections.ReleaseAll();
        }

        // One Sections row: its heading over its own long text
        private static (VisualElement Box, Label Heading, CardTextView Text) NewSection()
        {
            var box = new VisualElement();
            box.AddToClassList("card-rich__section");
            var heading = new Label();
            heading.AddToClassList("card-rich__heading");
            var text = new CardTextView("card-rich__paragraph");
            box.Add(heading);
            box.Add(text.Root);
            return (box, heading, text);
        }
    }
}
