using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // _3.2 step 4 on the REAL LivingRoomScene, with real input: The Lamp's Open Block buttons (a real tap scrolls the card to their
    // block, through the same NavController every element uses), and system Back -- the Escape key of a real Input System keyboard
    // (where Android's Back arrives in a Unity player): it closes the full view, steps the sheet down Full -> Half -> Peek, closes the
    // card at Peek like the X, and does nothing with Back Button = none. First of all: Escape is not already taken by the search.
    public partial class PoiCardNavSceneTests
    {
        private BackgroundSafeInput _backgroundInput;
        private Keyboard _keyboard;

        // The keyboard a test presses goes away with the test (and the input settings it was added under are given back)
        [UnityTearDown]
        public IEnumerator RemoveTheTestKeyboard()
        {
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            _keyboard = null;
            _backgroundInput?.Dispose();
            _backgroundInput = null;
            yield break;
        }

        // A real press and release of Escape on an Input System keyboard (added under BackgroundSafeInput: a run with Unity in the
        // background still delivers it), one frame each, the way a person's key arrives
        private IEnumerator PressEscape()
        {
            if (_keyboard == null)
            {
                _backgroundInput = new BackgroundSafeInput();
                _keyboard = BackgroundSafeInput.AddEnabled<Keyboard>();
            }
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Escape));
            yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return null;
        }

        // ---------------- Escape and the search (the probe the brief asks for BEFORE Back is wired) ----------------

        // The search UI listens to Escape nowhere in its own code, but its TextField is UI Toolkit's: does a real Escape change anything the
        // search shows? In this scene UI Toolkit takes its input from the EventSystem's InputSystemUIInputModule, so a real Escape reaches
        // the focused field as ONE NavigationCancelEvent (the UI Cancel action; no KeyDownEvent) -- found by probing it, 2026-10-15. The
        // precondition is that this cancel really arrived; then the words, the search, its results and the focus are all unchanged
        [UnityTest]
        public IEnumerator Escape_WhileTheSearchFieldHoldsTheWords_LeavesTheSearchAsItWas()
        {
            var field = Host.Root.Q<TextField>("search-field");
            Assert.IsNotNull(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(),
                "precondition: the wall scene has the EventSystem its UI takes input from");
            var cancels = new List<VisualElement>();
            Host.Root.RegisterCallback<NavigationCancelEvent>(e => cancels.Add(e.target as VisualElement), TrickleDown.TrickleDown);
            field.Focus();
            string typed = ConfigCopy().pois.Single(p => p.id == "lamp").name;   // the wall's own word for the Lamp, read from its config
            field.value = typed;
            yield return Wait(0.4f);
            Assert.AreEqual(typed, Host.Query, "precondition: the search ran on the typed words");
            var focused = field.panel.focusController.focusedElement;
            Assert.IsNotNull(focused, "precondition: the field holds the focus");
            var results = ListIds();
            Assert.IsNotEmpty(results, "precondition: the search found something");

            yield return PressEscape();
            yield return Wait(0.4f);

            Assert.AreEqual(1, cancels.Count, "precondition: the real Escape reached the search's panel, as one Cancel");
            Assert.IsTrue(cancels[0] == field || field.Contains(cancels[0]), "precondition: ... sent to the focused field (its inner text element)");
            Assert.AreEqual(typed, field.value, "Escape leaves the typed words");
            Assert.AreEqual(typed, Host.Query, "and the search");
            CollectionAssert.AreEquivalent(results, ListIds(), "and its results");
            Assert.AreSame(focused, field.panel.focusController.focusedElement, "and the focus");
        }

        // ---------------- Open Block: a real tap on The Lamp's buttons ----------------

        // The Lamp's pill-row Actions block and its Open Block rows, read from the wall's own config (never re-typed)
        private (string Block, List<string> Targets) LampsOpenBlockButtons()
        {
            var lamp = ConfigCopy().pois.Single(p => p.id == Lamp);
            foreach (var block in lamp.card.blocks)
            {
                if (block.kind != BuiltInBlocks.ActionsKind || block.variant != BuiltInBlocks.ActionsPillRow) continue;
                var read = new BlockFieldReader(block, "en", "en");
                var targets = read.Items(BuiltInBlocks.ActionsItemsField)
                    .Where(r => read.ItemValue(r, BuiltInBlocks.ActionsActionField) == BuiltInBlocks.ActionOpenBlock)
                    .Select(r => read.ItemValue(r, BuiltInBlocks.ActionsTargetField)).ToList();
                if (targets.Count > 0) return (block.key, targets);
            }
            Assert.Fail("precondition: The Lamp has a pill-row Actions block with Open Block rows");
            return default;
        }

        // The bound Actions view of a block of the open card, and its button that opens `target`
        private ActionsBlockView.Action OpenBlockButton(string actionsBlock, string target)
        {
            var slot = Slot(actionsBlock);
            Assert.IsNotNull(slot, "precondition: " + actionsBlock + " is on the card");
            var view = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Single(v => Sheet.Stack.SlotOf(v) == slot);
            return view.Actions.Single(a => a.Kind == BuiltInBlocks.ActionOpenBlock && a.Target == target);
        }

        [UnityTest]
        public IEnumerator ARealTapOnAnOpenBlockButton_RaisesTheCard_AndLandsItsBlockAtTheTopOfTheViewport_ThroughTheNavController()
        {
            var (actions, targets) = LampsOpenBlockButtons();
            Assert.AreEqual(2, targets.Count, "precondition: two Open Block rows, to two DIFFERENT blocks");
            Assert.AreNotEqual(targets[0], targets[1]);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            // - at Full's geometry (where the jump lands): the first target lands without the clamp, the second one is the card's end
            Assert.Less(Slot(targets[0]).layout.y, Range - OnePixel, "precondition: " + targets[0] + " can land at the top without the clamp");
            var view = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Single(v => Sheet.Stack.SlotOf(v) == Slot(actions));
            CollectionAssert.AreEqual(new[] { BuiltInBlocks.ActionShowOnWall, BuiltInBlocks.ActionOpenBlock, BuiltInBlocks.ActionOpenBlock },
                view.Actions.Select(a => a.Kind), "every row drawn: both targets are blocks the card scrolls to");
            Assert.AreEqual(Sheet.Stack.HeadingOf(Sheet.Stack.BoundViews.Single(v => Sheet.Stack.SlotOf(v) == Slot(targets[0]))).text,
                OpenBlockButton(actions, targets[0]).Label.text, "a button with no words of its own says its block's heading");

            // - the pill row as a visitor sees it, with the Open Block icon (judged item by item in _3.2's step 4 proof)
            Scroll.ScrollTo(view.Root);
            yield return CardTestInput.Settle(0.3f);
            yield return Capture("Card_OpenBlock_PillRow_Lamp_390");

            Sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            yield return CardTestInput.TapInView(Scroll, OpenBlockButton(actions, targets[0]).Button);
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "Jump Opens Card To = Full: the tap raised the card");
            yield return Landed();
            Assert.AreEqual(0f, TopInViewport(targets[0]), OnePixel, targets[0] + "'s slot top meets the viewport's top (offset " + Scroll.scrollOffset.y + ")");
            var jump = NavJumps.Last();
            Assert.AreEqual(targets[0], jump.BlockKey, "nav_jump: the block landed on");
            Assert.AreEqual(BuiltInBlocks.ActionsKind, jump.Variant, "nav_jump: the asking block's kind");

            yield return CardTestInput.TapInView(Scroll, OpenBlockButton(actions, targets[1]).Button);
            yield return Landed();
            Assert.AreEqual(Mathf.Min(Slot(targets[1]).layout.y, Range), Scroll.scrollOffset.y, OnePixel, targets[1] + ": landed, clamped to the range");
            Assert.AreEqual(targets[1], NavJumps.Last().BlockKey);
            Assert.AreEqual(2, NavJumps.Count, "one nav_jump per tap");
        }

        [UnityTest]
        public IEnumerator AnOpenBlockButton_WhoseTargetIsGone_IsNotDrawn_AfterALiveEdit()
        {
            var (actions, targets) = LampsOpenBlockButtons();
            yield return Push(c =>
            {
                var lamp = c.pois.Single(p => p.id == Lamp);
                lamp.card.blocks.RemoveAll(b => b.key == targets[0]);
            });
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var view = Sheet.Stack.BoundViews.OfType<ActionsBlockView>().Single(v => Sheet.Stack.SlotOf(v) == Slot(actions));
            CollectionAssert.AreEqual(new[] { "", targets[1] }, view.Actions.Select(a => a.Target),
                "the button to the deleted block is gone; Show On The Wall and the other Open Block stay");
        }

        // ---------------- system Back: real Escape presses ----------------

        [UnityTest]
        public IEnumerator RealEscapePresses_CloseTheFullView_ThenStepFullHalfPeek_ThenCloseTheCardLikeTheX()
        {
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            IBlockHost host = Sheet;
            host.OpenTakeover("Back test", 1, 0, (takeover, container, media, page) => container.Add(new Label("page")));
            yield return null;
            Assert.IsTrue(Sheet.Takeover.IsOpen, "precondition: the full view is open over the full card");

            yield return PressEscape();
            Assert.IsFalse(Sheet.Takeover.IsOpen, "press 1: the full view closes");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "press 1: and only that");
            yield return CardTestInput.Settle();

            yield return PressEscape();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "press 2: Full -> Half");
            yield return CardTestInput.Settle();
            yield return PressEscape();
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "press 3: Half -> Peek");
            Assert.AreEqual(Lamp, SelectionEventBus.CurrentPoiId, "still selected at peek");
            yield return CardTestInput.Settle();

            yield return PressEscape();
            Assert.IsFalse(Sheet.IsOpen, "press 4: at Peek the card closes");
            Assert.IsNull(SelectionEventBus.CurrentPoiId, "through the selection bus, like the X: the marker is no longer selected");
            Assert.IsNull(Card.ShownPoiId);

            yield return PressEscape();
            Assert.IsFalse(Sheet.IsOpen, "press 5: no card, nothing happens (Back is the app's)");
        }

        [UnityTest]
        public IEnumerator BackButtonNone_RealEscapeLeavesTheCardAsItIs_AndSwitchingItBackLiveStepsDownAgain()
        {
            yield return Push(c => c.card_settings.navigation.back_button = CardOptions.BackButtonNone);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            IBlockHost host = Sheet;
            host.OpenTakeover("Back test", 1, 0, (takeover, container, media, page) => container.Add(new Label("page")));
            yield return null;

            yield return PressEscape();
            yield return PressEscape();
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.Takeover.IsOpen, "none: the full view stays open");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "none: the card stays at Full");
            Assert.AreEqual(Lamp, SelectionEventBus.CurrentPoiId, "none: still selected");

            // - the option is live: the next press follows the wall's new setting (a live edit rebinds the open card)
            yield return Push(c => c.card_settings.navigation.back_button = CardOptions.BackButtonStepDown);
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: the live edit kept the card's stop");
            Assert.IsFalse(Sheet.Takeover.IsOpen, "precondition: a rebind replaces the full view of the old card (PoiCardSheetView.Show)");
            yield return PressEscape();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "step_down again: Full -> Half");
        }
    }
}
