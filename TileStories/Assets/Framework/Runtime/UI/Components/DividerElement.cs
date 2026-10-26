using UnityEngine.UIElements;

namespace TileStories
{
    // The divider element (_3.3 R14.2, the prototype's azulejo divider): a pause between two parts of the page -- a thin full-width line, or
    // a small tile ornament (a vector: CardIcons.Shape.Azulejo) in the block family's colour between two hairlines. Nothing to read.
    public sealed class DividerElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;
        public VisualElement LeadingLine { get; }
        public CardIcons.VectorGlyph Ornament { get; }
        public VisualElement TrailingLine { get; }

        private readonly NavTint _tint;

        public DividerElement()
        {
            ElementStyle.Init(this, BuiltInElements.Divider);
            pickingMode = PickingMode.Ignore;
            LeadingLine = Line("ts-el-divider-line-a");
            Ornament = CardIcons.CreateVector(CardIcons.Shape.Azulejo);
            Ornament.name = "ts-el-divider-ornament";
            Ornament.AddToClassList("ts-el-divider__ornament");
            TrailingLine = Line("ts-el-divider-line-b");
            Add(LeadingLine);
            Add(Ornament);
            Add(TrailingLine);
            _tint = new NavTint(this);
        }

        // Whether the bound row draws the tile ornament
        public bool ShowsOrnament { get; private set; }

        public void Bind(ElementRow row, ElementBindContext context)
        {
            ElementStyle.Apply(this, row, context);
            ShowsOrnament = context.StyleOf(BuiltInElements.OrnamentStyle, row) == BuiltInElements.OrnamentAzulejo;
            Ornament.style.display = ShowsOrnament ? DisplayStyle.Flex : DisplayStyle.None;
            TrailingLine.style.display = ShowsOrnament ? DisplayStyle.Flex : DisplayStyle.None;
            _tint.Clear();
            Ornament.style.color = StyleKeyword.Null;
            if (ShowsOrnament) _tint.Ink(Ornament, context.FamilyToken);
        }

        public void Unbind()
        {
            _tint.Clear();
            Ornament.style.color = StyleKeyword.Null;
        }

        private static VisualElement Line(string name)
        {
            var line = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            line.AddToClassList("ts-el-divider__line");
            return line;
        }
    }
}
