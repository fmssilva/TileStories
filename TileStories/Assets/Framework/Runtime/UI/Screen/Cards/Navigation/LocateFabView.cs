using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's "Show me where it is" button (_3.3 P2 review e, P3d): a disc in the theme's accent carrying the locate mark in the ink made
    // for it (--ts-on-accent), at the bottom right of the sheet, over the hero and after it -- where and when LocateFabRule says, placed by
    // the sheet. A tap lowers the card to Peek with the point's marker in view (the Show On Wall block's own path, IBlockHost.ShowOnWall);
    // its name is that block's words (CardStrings: what a screen reader and a long press read). The whole disc is the touch target
    // (--ts-fab-size, over the 44 pt minimum). Hide While Scrolling slides it away with the bar (HiddenClass, PoiCard.uss), and while away
    // it takes no finger. The sheet owns one.
    public sealed class LocateFabView
    {
        public const string RootClass = "poi-card__fab";
        public const string MarkClass = "poi-card__fab-mark";
        // Slid away with the bar (Hide While Scrolling): below its place and faded out, over the chrome's own transition
        public const string HiddenClass = RootClass + "--hidden";

        public Button Root { get; }
        public VisualElement Mark { get; }
        // Placed now (display) and not slid away: what the visitor sees and can tap
        public bool IsShown => Root.style.display == DisplayStyle.Flex && !Root.ClassListContains(HiddenClass);

        // Its numbers (CardTokens.uss, read on the card root): the disc's size, its gap above what lies under it, its inset from the side
        public float Size => Token(SizeToken);
        public float Gap => Token(GapToken);
        public float Inset => Token(InsetToken);

        private static readonly CustomStyleProperty<float> SizeToken = new("--ts-fab-size");
        private static readonly CustomStyleProperty<float> GapToken = new("--ts-fab-gap");
        private static readonly CustomStyleProperty<float> InsetToken = new("--ts-navbar-inset");

        public LocateFabView(System.Action tap)
        {
            Root = new Button(tap) { name = "poi-card-fab" };
            Root.AddToClassList(RootClass);
            Mark = CardIcons.Create();
            CardIcons.SetKey(Mark, CardIcons.ShowOnWall);
            Mark.AddToClassList(MarkClass);
            Root.Add(Mark);
            Root.style.display = DisplayStyle.None;
            // - the mark's ink follows the mode (a painted icon does not follow a mode class)
            Root.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= Paint; ThemeController.ModeChanged += Paint; Paint(); });
            Root.RegisterCallback<DetachFromPanelEvent>(_ => ThemeController.ModeChanged -= Paint);
            Root.RegisterCallback<CustomStyleResolvedEvent>(_ => Paint());
        }

        // The open card's words: the button's name
        public void Bind(CardStrings strings) => Root.tooltip = strings?.Get(CardStrings.Keys.ShowOnWallButton) ?? "";

        // Placed (`placed`: the bar and its Locate Button on, at Half or Full) `bottom` above the sheet's foot, or slid away while the scroll
        // hides the bar (`shown` false while placed)
        public void Apply(bool placed, bool shown, float bottom)
        {
            Root.style.display = placed ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList(HiddenClass, placed && !shown);
            Root.pickingMode = placed && shown ? PickingMode.Position : PickingMode.Ignore;
            float size = Size;
            if (size > 0f && !Mathf.Approximately(Root.resolvedStyle.width, size))
            {
                Root.style.width = Root.style.height = size;
                Root.style.borderTopLeftRadius = Root.style.borderTopRightRadius =
                    Root.style.borderBottomLeftRadius = Root.style.borderBottomRightRadius = size * 0.5f;
            }
            // - only a real move is written: the same value again would raise another geometry change for nothing
            if (Root.style.bottom.keyword != StyleKeyword.Undefined || Mathf.Abs(Root.style.bottom.value.value - bottom) > 0.01f) Root.style.bottom = bottom;
            float inset = Inset;
            if (Root.style.right.keyword != StyleKeyword.Undefined || Mathf.Abs(Root.style.right.value.value - inset) > 0.01f) Root.style.right = inset;
        }

        private void Paint()
        {
            if (CardTokenColours.TryResolve(Root, "--ts-on-accent", out Color ink)) CardIcons.Tint(Mark, ink);
        }

        // A plain-number token read on the card root (.ts-card), 0 when it sits under none
        private float Token(CustomStyleProperty<float> token)
        {
            for (var e = Root.parent; e != null; e = e.parent)
                if (e.ClassListContains("ts-card")) return e.customStyle.TryGetValue(token, out float value) ? value : 0f;
            return 0f;
        }
    }
}
