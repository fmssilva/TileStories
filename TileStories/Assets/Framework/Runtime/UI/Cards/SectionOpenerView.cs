using UnityEngine.UIElements;

namespace TileStories
{
    // A section's opener (_3.3 P4.4c1, R11, R10.7; R17.5's shared "Section Opener" component): the family's FILLED pill -- its icon, its
    // name in the display face, one meta line ("4 blocks · 9 min") -- and a full-width line in the same colour. Not sticky, not a button:
    // it names the chapter the sheet holds. Host-agnostic (R15.2): it knows no sheet, scroll or controller -- it draws what Show is given,
    // its colours painted from the theme BY NAME (NavTint: the family's fill, the ink on it), so a mode switch repaints it. Its look is
    // PoiCard.uss (.card-section-opener*); built to move into Runtime/UI/Components with the R16 3b folder step.
    public sealed class SectionOpenerView
    {
        public const string RootClass = "card-section-opener";

        public VisualElement Root { get; }
        public VisualElement Pill { get; }
        public VisualElement Icon { get; }
        public Label Name { get; }
        public Label Meta { get; }
        public VisualElement Line { get; }

        private readonly NavTint _tint;

        public SectionOpenerView()
        {
            Root = new VisualElement { name = "card-section-opener", pickingMode = PickingMode.Ignore };
            Root.AddToClassList(RootClass);
            Pill = new VisualElement { name = "card-section-opener-pill", pickingMode = PickingMode.Ignore };
            Pill.AddToClassList(RootClass + "__pill");
            Icon = CardIcons.Create();
            Icon.AddToClassList(RootClass + "__icon");
            Name = new Label { name = "card-section-opener-name", pickingMode = PickingMode.Ignore };
            Name.AddToClassList(RootClass + "__name");
            Meta = new Label { name = "card-section-opener-meta", pickingMode = PickingMode.Ignore };
            Meta.AddToClassList(RootClass + "__meta");
            Line = new VisualElement { name = "card-section-opener-line", pickingMode = PickingMode.Ignore };
            Line.AddToClassList(RootClass + "__line");
            Pill.Add(Icon);
            Pill.Add(Name);
            Pill.Add(Meta);
            Root.Add(Pill);
            Root.Add(Line);
            _tint = new NavTint(Root);
        }

        // Show a family's opener: its icon, its colour token (the fill; the ink is the theme's "<token>-ink"), its words and its meta line
        public void Show(string iconKey, string colorToken, string name, string meta)
        {
            CardIcons.SetKey(Icon, iconKey);
            Name.text = name ?? "";
            Meta.text = meta ?? "";
            Meta.style.display = Meta.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            string ink = SectionSheetRule.InkOf(colorToken);
            _tint.Clear();
            _tint.Fill(Pill, colorToken);
            _tint.Fill(Line, colorToken);
            _tint.Icon(Icon, ink);
            _tint.Ink(Name, ink);
            _tint.Ink(Meta, ink);
        }
    }
}
