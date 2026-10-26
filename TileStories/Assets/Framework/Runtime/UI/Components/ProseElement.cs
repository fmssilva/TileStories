using UnityEngine.UIElements;

namespace TileStories
{
    // The lede and paragraph elements (_3.3 R14.2): long text through the card's CardTextView (paragraphs, glossary words with their
    // definitions). The lede is the larger opening that sums the story up; a paragraph with Drop Cap On raises its first letter in the
    // display face beside its first lines, the rest running the full width under it (CardTextView.DropCapFlow, the prototype's magazine).
    // Tone colours the words (the drop cap wears the family colour whatever the tone: it is an ornament); size and alignment are classes.
    public sealed class ProseElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;
        public CardTextView Text { get; }
        public string Kind { get; }

        private readonly NavTint _tint;
        private float _leading;
        private ElementRow _row;
        private ElementBindContext _context;

        public ProseElement(string kind)
        {
            Kind = kind;
            ElementStyle.Init(this, kind);
            Text = new CardTextView("ts-el__paragraph");
            Add(Text.Root);
            _tint = new NavTint(this);
            RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                float leading = ElementStyle.LeadingOf(this);
                if (leading == _leading) return;
                _leading = leading;
                if (_context != null) Show();
            });
        }

        // Whether the bound row raises its first letter
        public bool HasDropCap => Kind == BuiltInElements.Paragraph && _context != null &&
                                  _context.StyleOf(BuiltInElements.DropCapStyle, _row) == BuiltInElements.On;

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
            Text.Clear();
            Unpaint();
        }

        private void Show()
        {
            ElementStyle.Apply(this, _row, _context);
            string words = new BlockFieldReader(null, _context.Language, _context.Fallback).ItemText(_row.Data, BuiltInElements.TextField);
            Text.Leading = _leading;
            Text.Bind(GlossaryMarkup.Paragraphs(words), _context.Block?.Glossary,
                HasDropCap ? CardTextView.FirstParagraph.DropCapFlow : CardTextView.FirstParagraph.Normal);
            Unpaint();
            if (HasDropCap) _tint.Ink(Text.DropCap, _context.FamilyToken);
            if (!ElementStyle.IsFamilyTone(_row, _context)) return;
            foreach (var paragraph in Text.Paragraphs) _tint.Ink(paragraph, _context.FamilyToken);
        }

        private void Unpaint()
        {
            _tint.Clear();
            Text.DropCap.style.color = StyleKeyword.Null;
            foreach (var label in Text.Root.Query<Label>().ToList()) label.style.color = StyleKeyword.Null;
        }
    }
}
