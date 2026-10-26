using UnityEngine.UIElements;

namespace TileStories
{
    // The title, eyebrow and heading elements (_3.3 R14.2): one run of words. Title and heading speak in the display face; the eyebrow is
    // the small tracked capitals above a title (the prototype's "THE PALACE ON THE WATER": the words are written in sentence case and drawn
    // in capitals for the visitor's language). Tone colours the words, size and alignment are classes (Components.uss).
    public sealed class LineElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;
        public Label Text { get; }
        public string Kind { get; }

        private readonly NavTint _tint;
        private float _leading;
        private ElementRow _row;
        private ElementBindContext _context;

        public LineElement(string kind)
        {
            Kind = kind;
            ElementStyle.Init(this, kind);
            Text = new Label { name = "ts-el-text", enableRichText = true, pickingMode = PickingMode.Ignore };
            Text.AddToClassList("ts-el__text");
            if (kind != BuiltInElements.Eyebrow) Text.AddToClassList(ElementStyle.DisplayClass);
            Add(Text);
            _tint = new NavTint(this);
            // - the line height is this component's own token: shown again when it resolves (or changes with a class)
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
            Text.text = "";
            _tint.Clear();
            Text.style.color = StyleKeyword.Null;
        }

        private void Show()
        {
            ElementStyle.Apply(this, _row, _context);
            string words = new BlockFieldReader(null, _context.Language, _context.Fallback).ItemText(_row.Data, BuiltInElements.TextField);
            if (Kind == BuiltInElements.Eyebrow) words = ElementStyle.Capitals(words, _context.Language);
            Text.text = ElementStyle.WithLeading(words, _leading);
            _tint.Clear();
            Text.style.color = StyleKeyword.Null;
            if (ElementStyle.IsFamilyTone(_row, _context)) _tint.Ink(Text, _context.FamilyToken);
        }
    }
}
