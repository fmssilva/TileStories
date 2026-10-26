using UnityEngine.UIElements;

namespace TileStories
{
    // The pull quote element (_3.3 R14.2, the prototype's quote): an opening quotation mark drawn as a vector (no glyph a font must hold),
    // the quoted words large in the display face, who said it under them, and a short bar closing it. The TONE colours the mark and the bar
    // (neutral: the line colour, family, accent; inverse puts the whole quote on a dark plate); the words stay the page's ink. Centred by
    // default (alignment Start lines everything up on the text's margin).
    public sealed class PullQuoteElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;
        public CardIcons.VectorGlyph Mark { get; }
        public CardTextView Quote { get; }
        public Label Attribution { get; }
        public VisualElement Bar { get; }

        private readonly NavTint _tint;
        private float _leading;
        private ElementRow _row;
        private ElementBindContext _context;

        public PullQuoteElement()
        {
            ElementStyle.Init(this, BuiltInElements.PullQuote);
            Mark = CardIcons.CreateVector(CardIcons.Shape.QuoteOpen);
            Mark.name = "ts-el-quote-mark";
            Mark.AddToClassList("ts-el-pull-quote__mark");
            Quote = new CardTextView("ts-el__paragraph");
            Quote.Root.AddToClassList("ts-el-pull-quote__words");
            Quote.Root.AddToClassList(ElementStyle.DisplayClass);
            Attribution = new Label { name = "ts-el-quote-attribution", pickingMode = PickingMode.Ignore };
            Attribution.AddToClassList("ts-el-pull-quote__attribution");
            Bar = new VisualElement { name = "ts-el-quote-bar", pickingMode = PickingMode.Ignore };
            Bar.AddToClassList("ts-el-pull-quote__bar");
            Add(Mark);
            Add(Quote.Root);
            Add(Attribution);
            Add(Bar);
            _tint = new NavTint(this);
            RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                float leading = ElementStyle.LeadingOf(this);
                if (leading == _leading) return;
                _leading = leading;
                if (_context != null) Show();
            });
        }

        public void Bind(ElementRow row, ElementBindContext context)
        {
            _row = row;
            _context = context;
            Show();
        }

        public void Unbind()
        {
            _row = default;
            _context = null;
            Quote.Clear();
            Attribution.text = "";
            Unpaint();
        }

        private void Show()
        {
            ElementStyle.Apply(this, _row, _context);
            var read = new BlockFieldReader(null, _context.Language, _context.Fallback);
            Quote.Leading = _leading;
            Quote.Bind(GlossaryMarkup.Paragraphs(read.ItemText(_row.Data, BuiltInElements.TextField)), _context.Block?.Glossary);
            Attribution.text = read.ItemText(_row.Data, BuiltInElements.AttributionField);
            Attribution.style.display = Attribution.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Unpaint();
            if (!ElementStyle.IsFamilyTone(_row, _context)) return;
            _tint.Ink(Mark, _context.FamilyToken);
            _tint.Fill(Bar, _context.FamilyToken);
        }

        private void Unpaint()
        {
            _tint.Clear();
            Mark.style.color = StyleKeyword.Null;
            Bar.style.backgroundColor = StyleKeyword.Null;
        }
    }
}
