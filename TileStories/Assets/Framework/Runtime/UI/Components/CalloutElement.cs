using UnityEngine.UIElements;

namespace TileStories
{
    // The callout element (_3.3 R14.2, the prototype's "What was inside" box): a highlighted box with a bar down its leading edge, an optional
    // short title in small capitals and its text. The TONE is the box's: neutral (a quiet well), family (the bar and title in the block
    // family's colour), accent (in the accent), inverse (light words on a dark plate). The words themselves stay the page's ink on the
    // surface (family colours keep 7:1 there, not on a tinted well -- measured, R12.6).
    public sealed class CalloutElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;
        public VisualElement Bar { get; }
        public Label Title { get; }
        public CardTextView Text { get; }

        private readonly NavTint _tint;
        private float _leading;
        private ElementRow _row;
        private ElementBindContext _context;

        public CalloutElement()
        {
            ElementStyle.Init(this, BuiltInElements.Callout);
            Bar = new VisualElement { name = "ts-el-callout-bar", pickingMode = PickingMode.Ignore };
            Bar.AddToClassList("ts-el-callout__bar");
            var column = new VisualElement { pickingMode = PickingMode.Ignore };
            column.AddToClassList("ts-el-callout__body");
            Title = new Label { name = "ts-el-callout-title", pickingMode = PickingMode.Ignore };
            Title.AddToClassList("ts-el-callout__title");
            Text = new CardTextView("ts-el__paragraph");
            column.Add(Title);
            column.Add(Text.Root);
            Add(Bar);
            Add(column);
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
            Text.Clear();
            Title.text = "";
            Unpaint();
        }

        private void Show()
        {
            ElementStyle.Apply(this, _row, _context);
            var read = new BlockFieldReader(null, _context.Language, _context.Fallback);
            string title = read.ItemText(_row.Data, BuiltInElements.CalloutTitleField);
            Title.text = ElementStyle.Capitals(title, _context.Language);
            Title.style.display = title.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Text.Leading = _leading;
            Text.Bind(GlossaryMarkup.Paragraphs(read.ItemText(_row.Data, BuiltInElements.TextField)), _context.Block?.Glossary);
            Unpaint();
            if (!ElementStyle.IsFamilyTone(_row, _context)) return;
            _tint.Fill(Bar, _context.FamilyToken);
            _tint.Ink(Title, _context.FamilyToken);
        }

        private void Unpaint()
        {
            _tint.Clear();
            Bar.style.backgroundColor = StyleKeyword.Null;
            Title.style.color = StyleKeyword.Null;
        }
    }
}
