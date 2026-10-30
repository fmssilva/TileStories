using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's FLOATING slot (_3.2 sections 4.2 and 5.4, step 9): a row in the card layer resting just above the sheet's top-right edge at
    // every stop, holding the placed-model chip. It costs no reading space, so the chrome floor never applies to it.
    //   - a LAYER, not a part of the sheet: the sheet clips what it holds (its rounded top, its slid-away bottom area), so a row above its
    //     edge could not live inside it. It sits right BEFORE the card's menu layer (CardMenuOverlay) in the card layer: over the sheet,
    //     under an open menu and the full view, by child order alone
    //   - it follows the sheet's top edge on every geometry change of the sheet (each frame of its height transition and of a drag), where
    //     FloatingSlotRule.Bottom says: a gap above the sheet, never off the screen (in a camera strip's gap at Full), never over the chrome
    //     row (at Full to the top: under it)
    //   - it shows over an open card only, and only while it holds something (FloatingSlotRule.Shown)
    // The sheet owns one and binds the chip with the card's context.
    public sealed class FloatingSlot
    {
        public VisualElement Root { get; }
        public PlacedModelChipView Chip { get; }

        public bool IsShown => Root.style.display == DisplayStyle.Flex;

        private static readonly CustomStyleProperty<float> GapProperty = new("--ts-float-gap");
        private readonly PoiCardSheetView _sheet;
        private float _gap;

        // Built by the sheet before its slots; the sheet puts Root in its layer once the menu layer exists (InsertUnder)
        public FloatingSlot(PoiCardSheetView sheet)
        {
            _sheet = sheet;
            // - never takes a finger itself: a tap beside the chip reaches the camera view under it
            Root = new VisualElement { name = "poi-card-floating", pickingMode = PickingMode.Ignore };
            // - the card's own tokens live on these classes (CardTokens.uss: its gap is read here), the theme's arrive from the app root;
            //   the slot is the sheet's sibling, like the menu layer
            Root.AddToClassList("ts-card");
            Root.AddToClassList("ts-theme-default");
            Root.AddToClassList("poi-card-floating");
            Chip = new PlacedModelChipView();
            Chip.Redrawn += Refresh;
            Root.Add(Chip.Root);
            Root.style.display = DisplayStyle.None;

            // - every geometry change of the sheet (its animated height, a drag, the layer resized) or of the slot itself (the chip came) re-places it
            sheet.Root.RegisterCallback<GeometryChangedEvent>(_ => Place());
            sheet.Layer.RegisterCallback<GeometryChangedEvent>(_ => Place());
            Root.RegisterCallback<GeometryChangedEvent>(_ => Place());
            Root.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyle);
        }

        // Put the slot in the card layer right before the menu layer: over the sheet, under an open menu and the full view
        public void InsertUnder(CardMenuOverlay overlay) => _sheet.Layer.Insert(_sheet.Layer.IndexOf(overlay.Root), Root);

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

        // Show the slot over an open card while it holds the chip, and put it where the rule says
        public void Refresh()
        {
            Root.style.display = FloatingSlotRule.Shown(_sheet.IsOpen, Chip.IsShown) ? DisplayStyle.Flex : DisplayStyle.None;
            Place();
        }

        // Where the slot rests now: FloatingSlotRule over the sheet's height as laid out this frame (mid-transition or mid-drag too)
        public void Place()
        {
            float sheet = _sheet.Root.layout.height, layer = _sheet.Layer.layout.height, slot = Root.layout.height;
            if (float.IsNaN(sheet) || float.IsNaN(layer)) return;
            // - the chrome row's foot in the sheet's frame: the X's button (the row's buttons share its top and height)
            float chrome = _sheet.CloseButton.layout.yMax;
            float bottom = FloatingSlotRule.Bottom(sheet, layer, float.IsNaN(slot) ? 0f : slot, _gap, float.IsNaN(chrome) ? 0f : chrome);
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
