using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The rim light (_3.3 P4.4c2, R11 v3): ONE layer, the light that sweeps the rim of the block being read (LitBlockRule: the shown framed
    // block whose centre is nearest 45 % down the viewport), lighting the rim's inner band -- never its outer line, the block's measured
    // edge over a sunny wall. As the visitor scrolls it fades out of one block and into the next -- never
    // a jump, never two lit. It sits in the block's slot right after the frame's parts (behind the heading and the content), painted by
    // CardLight in the block's family colour (white on light metal, the family's hue at half strength on dark). The rim's static metal is
    // BlockFrame's, untouched. Plain C#, owned by BlockStackView; its look is PoiCard.uss (.card-rim-light).
    public sealed class RimLightView
    {
        public const string LayerClass = "card-rim-light";

        private readonly BlockStackView _stack;
        private readonly CardLight _light;
        private readonly List<LitBlockRule.Block> _blocks = new();
        private readonly List<VisualElement> _candidates = new();
        private IVisualElementScheduledItem _move;

        public RimLightView(BlockStackView stack, CardLight light)
        {
            _stack = stack;
            _light = light;
            Layer = new VisualElement { name = "card-rim-light", pickingMode = PickingMode.Ignore };
            Layer.AddToClassList(LayerClass);
            var scroll = stack.Scroll;
            scroll.verticalScroller.valueChanged += _ => Update();
            scroll.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => Update());
            scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => Update());
        }

        // The one layer, wherever it stands now
        public VisualElement Layer { get; }

        // The slot the light belongs to now (it may still be fading out of the one before), null when no block is lit
        public VisualElement LitSlot { get; private set; }

        // Find the block being read now and move the light there when it changed (and show the glass of the sheets near the view)
        public void Update()
        {
            var scroll = _stack.Scroll;
            // - the same scroll moment decides which sheets draw their glass (only those near the view cost anything)
            _stack.Sheets.ShowLightsNear(scroll.scrollOffset.y, scroll.contentViewport.layout.height);
            _blocks.Clear();
            _candidates.Clear();
            foreach (var sheet in _stack.Sheets.All)
            {
                if (sheet.Root.resolvedStyle.display == DisplayStyle.None) continue;
                foreach (var slot in sheet.Slots)
                {
                    // - only a framed block shown now catches the light (a quiet frame has no rim band but its outer line does)
                    if (!slot.ClassListContains(BlockFrame.FramedClass) || slot.resolvedStyle.display == DisplayStyle.None) continue;
                    if (float.IsNaN(slot.layout.height) || slot.layout.height <= 0f) continue;
                    float top = _stack.TopInScroll(slot);
                    _blocks.Add(new LitBlockRule.Block(top, top + slot.layout.height));
                    _candidates.Add(slot);
                }
            }
            int lit = LitBlockRule.Pick(_blocks, scroll.scrollOffset.y, scroll.contentViewport.layout.height);
            MoveTo(lit >= 0 ? _candidates[lit] : null);
        }

        // Take the light off the card (the stack unbinds)
        public void Clear()
        {
            _move?.Pause();
            Layer.RemoveFromHierarchy();
            LitSlot = null;
        }

        // Fade out of the block lit now, then into `slot` (null: out, and stay out)
        private void MoveTo(VisualElement slot)
        {
            if (slot == LitSlot) return;
            LitSlot = slot;
            _move?.Pause();
            if (Layer.parent == null)
            {
                Place();
                return;
            }
            Layer.style.opacity = 0f;
            // - wait for the fade out to finish before the light changes block (the stylesheet's --ts-duration transition)
            _move = Layer.schedule.Execute(Place).StartingIn(FadeMs());
        }

        // Put the layer into the lit slot, behind its heading and content, in its family's colour, and fade it in
        private void Place()
        {
            Layer.RemoveFromHierarchy();
            if (LitSlot == null) return;
            // - after the frame's two parts (BlockFrame.Build: shadow, rim band), before everything the block draws
            LitSlot.Insert(System.Math.Min(2, LitSlot.childCount), Layer);
            _light.Sweep(Layer, _stack.Sheets.SheetOf(LitSlot)?.Family?.ColorToken);
            Layer.style.opacity = 0f;
            // - one frame at 0 first, so the transition has a start to run from
            Layer.schedule.Execute(() => Layer.style.opacity = 1f).StartingIn(0);
        }

        private long FadeMs()
        {
            var durations = Layer.resolvedStyle.transitionDuration;
            foreach (var d in durations) return (long)(d.value * (d.unit == TimeUnit.Second ? 1000f : 1f));
            return 0;
        }
    }
}
