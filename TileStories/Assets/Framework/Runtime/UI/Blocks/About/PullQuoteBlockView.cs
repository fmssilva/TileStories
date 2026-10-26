using UnityEngine.UIElements;

namespace TileStories
{
    // The pull_quote block (_3.1 Tier 1): a quotation set apart from the text, with who said it and where. Two looks --
    // serif: large italic text behind an accent bar, the author and the source stacked under it; minimal: body-size italic
    // text, the author and the source on one line. Author and source are optional (an empty one takes no space). The
    // quote is a CardTextView (paragraphs, glossary words). Only classes here. (A true serif face arrives with the
    // _3.3 font tokens; until then "serif" is the display treatment.)
    public sealed class PullQuoteBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public CardTextView Quote { get; }
        public VisualElement Attribution { get; }
        public Label Author { get; }
        public Label Source { get; }

        private string _variantClass;

        public PullQuoteBlockView()
        {
            Root = new VisualElement { name = "card-pull-quote" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-quote");
            Quote = new CardTextView("card-quote__paragraph");
            Root.Add(Quote.Root);
            Attribution = new VisualElement();
            Attribution.AddToClassList("card-quote__attribution");
            Author = new Label();
            Author.AddToClassList("card-quote__author");
            Source = new Label();
            Source.AddToClassList("card-quote__source");
            Attribution.Add(Author);
            Attribution.Add(Source);
            Root.Add(Attribution);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-quote--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            Quote.Bind(GlossaryMarkup.Paragraphs(read.Text(BuiltInBlocks.PullQuoteTextField)), context.Glossary);
            Show(Author, read.Text(BuiltInBlocks.PullQuoteAuthorField));
            Show(Source, read.Text(BuiltInBlocks.PullQuoteSourceField));
            Attribution.style.display = Author.text.Length > 0 || Source.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Unbind()
        {
            Quote.Clear();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        private static void Show(Label label, string text)
        {
            label.text = text;
            label.style.display = text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
