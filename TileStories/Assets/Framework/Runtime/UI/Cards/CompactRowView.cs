using UnityEngine.UIElements;

namespace TileStories
{
    // The generic COMPACT ROW (_3.2 section 6.2, step 12): one look for every kind, drawn by the stack for a block whose display is Expandable
    // (BlockDisplayRule.ShowsCompact). One tappable line -- the family icon in its colour, the block's words (CompactRowRule) and under them
    // its format and minutes, a chevron -- that opens the block in place; open, the chevron turns over and a full-view button joins it at the
    // end. It draws only what CompactRowRule says and loads nothing: the stack binds the block's real view under it when it is opened.
    // Only classes here; CardParts.uss holds every size and colour.
    public sealed class CompactRowView
    {
        // The class an open row wears (the chevron turns over, the full-view button shows): what a test reads
        public const string ExpandedClass = "card-compact--expanded";

        public VisualElement Root { get; }
        // The whole line: a tap opens or closes the block
        public Button Toggle { get; }
        public VisualElement Icon { get; }
        public Label Words { get; }
        public Label Meta { get; }
        public CardIcons.VectorGlyph Chevron { get; }
        // Shown only while the row is open: the block in the full view over the card
        public Button FullView { get; }

        public bool IsExpanded { get; private set; }

        // Raised by a tap on the line / on the full-view button
        public event System.Action Toggled;
        public event System.Action FullViewRequested;

        private readonly NavTint _tint;
        private string _open = "";
        private string _close = "";

        public CompactRowView()
        {
            Root = new VisualElement { name = "card-compact" };
            Root.AddToClassList("card-compact");
            Toggle = new Button(() => Toggled?.Invoke()) { name = "card-compact-toggle" };
            Toggle.AddToClassList("card-compact__toggle");
            Toggle.AddToClassList("card-tap");
            Icon = CardIcons.Create();
            Icon.AddToClassList("card-compact__icon");
            Icon.pickingMode = PickingMode.Ignore;
            var texts = new VisualElement { pickingMode = PickingMode.Ignore };
            texts.AddToClassList("card-compact__texts");
            Words = new Label { name = "card-compact-words", pickingMode = PickingMode.Ignore };
            Words.AddToClassList("card-compact__words");
            Meta = new Label { name = "card-compact-meta", pickingMode = PickingMode.Ignore };
            Meta.AddToClassList("card-compact__meta");
            texts.Add(Words);
            texts.Add(Meta);
            Chevron = CardIcons.CreateVector(CardIcons.Shape.Chevron);
            Chevron.AddToClassList("card-compact__chevron");
            Chevron.pickingMode = PickingMode.Ignore;
            Toggle.Add(Icon);
            Toggle.Add(texts);
            Toggle.Add(Chevron);
            FullView = new Button(() => FullViewRequested?.Invoke()) { name = "card-compact-full-view" };
            FullView.AddToClassList("card-compact__full-view");
            FullView.AddToClassList("card-tap");
            var glyph = CardIcons.CreateVector(CardIcons.Shape.FullScreen);
            glyph.AddToClassList("card-compact__full-view-glyph");
            glyph.pickingMode = PickingMode.Ignore;
            FullView.Add(glyph);
            Root.Add(Toggle);
            Root.Add(FullView);
            _tint = new NavTint(Root);
        }

        // Draw `row` (CompactRowRule) with the card's words for opening / closing it and for the full view
        public void Show(CompactRowRule.Row row, CardStrings strings)
        {
            Words.text = row.Words;
            Words.EnableInClassList("card-compact__words--teaser", row.FromFirstWords);
            Meta.text = row.Meta;
            Meta.style.display = row.Meta.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            CardIcons.SetKey(Icon, row.IconKey);
            Icon.style.display = row.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
            _tint.Clear();
            _tint.Icon(Icon, row.ColorToken);
            _open = strings?.Get(CardStrings.Keys.CompactOpen) ?? "";
            _close = strings?.Get(CardStrings.Keys.CompactClose) ?? "";
            FullView.tooltip = strings?.Get(CardStrings.Keys.FullViewOpen) ?? "";
            SetExpanded(IsExpanded);
        }

        // Open or closed: the chevron's way, the full-view button, and the line's name (what a tap on it will do, then the block's words)
        public void SetExpanded(bool expanded)
        {
            IsExpanded = expanded;
            Root.EnableInClassList(ExpandedClass, expanded);
            FullView.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            string action = expanded ? _close : _open;
            Toggle.tooltip = action.Length > 0 ? action + " - " + Words.text : Words.text;
        }
    }
}
