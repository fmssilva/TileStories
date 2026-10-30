using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The navigation elements as data (_3.2 step 5a): NavElementRegistry has BlockRegistry's shape -- its refusals, and an app's own
    // element registered with zero Framework edits --, NavStripRule (the strip's sideways arithmetic) and SwipeGrabRule (which axis a
    // finger's move belongs to).
    public class NavElementRegistryTests
    {
        // An app's own element: the registry only needs one to register
        protected sealed class AppDotsView : INavElementView
        {
            public VisualElement Root { get; } = new VisualElement { name = "app-dots" };

            public void Bind(CardNavModel model, NavController nav, BlockBindContext context) { }

            public void Unbind() { }
        }

        protected const string AppKey = "app_dots";

        // - it lists BLOCKS, one dot each (not sections)
        protected static NavElementDefinition AppDots(string slot = NavSlotKeys.Top) => new(AppKey, slot, "App Dots", "One dot per block.", NavChoice.Blocks);

        [Test]
        public void TheRegistry_RefusesANullOrBlankElement_TheWordNone_ASecondOfTheSameKey_AnUnbuiltSlot_AndNoFactory()
        {
            var registry = new NavElementRegistry();
            Assert.Throws<ArgumentNullException>(() => registry.Register(null, () => new AppDotsView()));
            Assert.Throws<ArgumentException>(() => registry.Register(new NavElementDefinition(" ", NavSlotKeys.Top, "x", "x", NavChoice.Sections), () => new AppDotsView()));
            Assert.Throws<ArgumentException>(() => registry.Register(new NavElementDefinition(CardOptions.NavNone, NavSlotKeys.Top, "x", "x", NavChoice.Sections), () => new AppDotsView()),
                "'none' is what a wall stores for no element: no element may be called that");
            // - the in-flow nav blocks (step 11) are kinds, not a slot of the sheet: no element is registered for one
            var unbuilt = Assert.Throws<ArgumentException>(() => registry.Register(AppDots("in_flow"), () => new AppDotsView()));
            StringAssert.Contains("'in_flow'", unbuilt.Message, "the refusal names the slot");
            CollectionAssert.AreEqual(new[] { NavSlotKeys.Top, NavSlotKeys.Bottom, NavSlotKeys.Menu }, NavSlotKeys.Built,
                "the slots the sheet draws (step 7a: the bottom; step 8: the menu)");
            Assert.Throws<ArgumentException>(() => registry.Register(AppDots(null), () => new AppDotsView()));
            Assert.Throws<ArgumentNullException>(() => registry.Register(AppDots(), null));
            CollectionAssert.IsEmpty(registry.All, "nothing refused was kept");

            registry.Register(AppDots(), () => new AppDotsView());
            var twice = Assert.Throws<ArgumentException>(() => registry.Register(AppDots(), () => new AppDotsView()));
            StringAssert.Contains(AppKey, twice.Message);
            Assert.AreEqual(1, registry.All.Count);
        }

        [Test]
        public void NavStripRule_KeepsTheOffsetInsideTheStrip_AndMovesTheLeastToShowAWholeItem()
        {
            Assert.AreEqual(0f, NavStripRule.MaxOffset(300f, 390f), "a strip that fits does not move");
            Assert.AreEqual(210f, NavStripRule.MaxOffset(600f, 390f));
            Assert.AreEqual(0f, NavStripRule.Clamp(-20f, 600f, 390f), "never past its start");
            Assert.AreEqual(210f, NavStripRule.Clamp(500f, 600f, 390f), "never past its end");
            Assert.AreEqual(0f, NavStripRule.MaxOffset(float.NaN, 390f), "not laid out yet");
            // - an item already in view: unchanged
            Assert.AreEqual(50f, NavStripRule.OffsetToShow(100f, 80f, 390f, 600f, 50f));
            // - to the right: its end meets the right edge
            Assert.AreEqual(500f - 390f, NavStripRule.OffsetToShow(420f, 80f, 390f, 600f, 0f));
            // - to the left: its start meets the left edge
            Assert.AreEqual(30f, NavStripRule.OffsetToShow(30f, 80f, 390f, 600f, 150f));
            // - the last item: clamped to the end, never beyond
            Assert.AreEqual(210f, NavStripRule.OffsetToShow(560f, 40f, 390f, 600f, 0f));
            Assert.AreEqual(40f, NavStripRule.OffsetToShow(float.NaN, 80f, 390f, 600f, 40f), "an item not laid out keeps the offset");
        }

        [Test]
        public void SwipeGrabRule_ClaimsVertical_IsTheOtherAxis_AndNoMoveIsClaimedByBoth()
        {
            Assert.IsFalse(SwipeGrabRule.ClaimsVertical(new Vector2(0f, SwipeGrabRule.Slop)), "exactly the slop: not yet");
            Assert.IsTrue(SwipeGrabRule.ClaimsVertical(new Vector2(0f, SwipeGrabRule.Slop + 1f)), "down past the slop");
            Assert.IsTrue(SwipeGrabRule.ClaimsVertical(new Vector2(5f, -60f)), "up, a little off straight");
            Assert.IsFalse(SwipeGrabRule.ClaimsVertical(new Vector2(60f, 20f)), "mostly sideways: the strip's");
            Assert.IsFalse(SwipeGrabRule.ClaimsVertical(new Vector2(40f, 40f)), "an exact diagonal is nobody's");
            for (float x = -60f; x <= 60f; x += 7.5f)
                for (float y = -60f; y <= 60f; y += 7.5f)
                {
                    var travel = new Vector2(x, y);
                    Assert.IsFalse(SwipeGrabRule.ClaimsPointer(travel) && SwipeGrabRule.ClaimsVertical(travel), travel + " claimed by both");
                }
        }
    }
}
