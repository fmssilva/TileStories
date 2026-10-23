using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's FLOATING slot (_3.2 sections 4.2 and 5.4, step 9): a row in the card layer resting just above the sheet's top-right edge at
    // every stop -- the placed-model chip on its left, the wall's Floating Button element (fab_dial, fab_radial) on its right. It costs no
    // reading space, so the chrome floor never applies to it.
    //   - a LAYER, not a part of the sheet: the sheet clips what it holds (its rounded top, its slid-away bottom area), so a row above its
    //     edge could not live inside it. It sits right BEFORE the card's menu layer (CardMenuOverlay) in the card layer: over the sheet,
    //     under an open menu and the full view, by child order alone
    //   - it follows the sheet's top edge on every geometry change of the sheet (each frame of its height transition and of a drag), where
    //     FloatingSlotRule.Bottom says: a gap above the sheet, never off the screen (at Full it rests in the sheet's top gap)
    //   - it shows over an open card only, and only while it holds something (FloatingSlotRule.Shown)
    //   - while its element holds something open (the dial's column, the radial's arc) a scrim of its own, right UNDER it, dims the card and
    //     the camera view, so the open items read against it and not over the card's own words; a tap on the scrim closes them. (The menu
    //     layer's scrim cannot: it lies over the slot.) A floating element opens under its own element key (FloatingSlotRule.Dims)
    // The sheet owns one; NavSlots binds the element into Elements, the sheet binds the chip with the card's context.
    public sealed class FloatingSlot
    {
        public VisualElement Root { get; }
        // The dim behind an open floating element: the card layer's child right under Root
        public VisualElement Scrim { get; }
        public bool IsDimmed => Scrim.style.display == DisplayStyle.Flex;
        // Where the Floating Button element is drawn (NavSlots binds it here)
        public VisualElement Elements { get; }
        public PlacedModelChipView Chip { get; }

        public bool IsShown => Root.style.display == DisplayStyle.Flex;

        private static readonly CustomStyleProperty<float> GapProperty = new("--ts-float-gap");
        private readonly PoiCardSheetView _sheet;
        private float _gap;

        // Built by the sheet before its slots; the sheet puts Root in its layer once the menu layer exists (InsertUnder)
        public FloatingSlot(PoiCardSheetView sheet)
        {
            _sheet = sheet;
            // - never takes a finger itself: a tap beside the chip and the button reaches the camera view under it
            Root = new VisualElement { name = "poi-card-floating", pickingMode = PickingMode.Ignore };
            // - the card's own tokens live on these classes (CardTokens.uss: its gap is read here), the theme's arrive from the app root;
            //   the slot is the sheet's sibling, like the menu layer
            Root.AddToClassList("ts-card");
            Root.AddToClassList("ts-theme-default");
            Root.AddToClassList("poi-card-floating");
            Chip = new PlacedModelChipView();
            Chip.Redrawn += Refresh;
            Root.Add(Chip.Root);
            Elements = new VisualElement { name = "poi-card-floating-element", pickingMode = PickingMode.Ignore };
            Elements.AddToClassList("poi-card-floating__element");
            Elements.style.display = DisplayStyle.None;
            Root.Add(Elements);
            Root.style.display = DisplayStyle.None;
            Scrim = new VisualElement { name = "poi-card-floating-scrim" };
            Scrim.AddToClassList("ts-card");
            Scrim.AddToClassList("ts-theme-default");
            Scrim.AddToClassList("card-menu__scrim");
            Scrim.AddManipulator(new Clickable(() => _sheet.Overlay?.Close()));
            Scrim.style.display = DisplayStyle.None;

            // - every geometry change of the sheet (its animated height, a drag, the layer resized) or of the slot itself (the chip came) re-places it
            sheet.Root.RegisterCallback<GeometryChangedEvent>(_ => Place());
            sheet.Layer.RegisterCallback<GeometryChangedEvent>(_ => Place());
            Root.RegisterCallback<GeometryChangedEvent>(_ => Place());
            Root.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyle);
        }

        // Put the slot (and its scrim right under it) in the card layer right before the menu layer: over the sheet, under an open menu and the
        // full view; the scrim follows what the overlay owner holds open
        public void InsertUnder(CardMenuOverlay overlay)
        {
            _sheet.Layer.Insert(_sheet.Layer.IndexOf(overlay.Root), Scrim);
            _sheet.Layer.Insert(_sheet.Layer.IndexOf(overlay.Root), Root);
            overlay.Changed += () => Dim(overlay);
        }

        private void Dim(CardMenuOverlay overlay) =>
            Scrim.style.display = FloatingSlotRule.Dims(overlay.OpenKey, overlay.IsModal, _sheet.Slots?.FloatingKey) ? DisplayStyle.Flex : DisplayStyle.None;

        // The open card's context: the chip draws the AR placement owner for this card's point, in its words
        public void Bind(BlockBindContext context)
        {
            Chip.Bind(context);
            Refresh();
        }

        // The card closed: the chip lets go of the owner, the slot hides
        public void Unbind()
        {
            Chip.Unbind();
            Refresh();
        }

        // Show the slot over an open card while it holds the chip or an element, and put it where the rule says
        public void Refresh()
        {
            bool holds = Chip.IsShown || _sheet.Slots?.FloatingView != null;
            Root.style.display = FloatingSlotRule.Shown(_sheet.IsOpen, holds) ? DisplayStyle.Flex : DisplayStyle.None;
            Place();
        }

        // Where the slot rests now: FloatingSlotRule over the sheet's height as laid out this frame (mid-transition or mid-drag too)
        public void Place()
        {
            float sheet = _sheet.Root.layout.height, layer = _sheet.Layer.layout.height, slot = Root.layout.height;
            if (float.IsNaN(sheet) || float.IsNaN(layer)) return;
            float bottom = FloatingSlotRule.Bottom(sheet, layer, float.IsNaN(slot) ? 0f : slot, _gap);
            // - only a real move is written: writing the same value would raise another geometry change for nothing
            if (Root.style.bottom.keyword != StyleKeyword.Undefined || Mathf.Abs(Root.style.bottom.value.value - bottom) > 0.01f)
                Root.style.bottom = bottom;
        }

        private void OnCustomStyle(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(GapProperty, out float gap) && !Mathf.Approximately(gap, _gap))
            {
                _gap = gap;
                Place();
            }
        }
    }
}
