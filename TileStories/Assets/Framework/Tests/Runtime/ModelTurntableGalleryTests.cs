using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A of model_3d, turntable (_3.1 step 10A.2b.3): the real CardPreviewStage and CardPreviewService behind the
    // gallery card (CardGalleryHarness wires the SAME real stage as the wall's card -- a `default:` model resolves
    // through the real Framework library regardless of media source, so this proves the seam production uses). A real
    // one-finger drag rotates the model (CardTestInput.DragFrom, exactly the pointer events a finger sends); the pinch
    // is driven through ModelTurntableBlockView.Pinch (a mouse alone cannot produce two simultaneous pointers, the same
    // reason ZoomImageBlockView exposes ZoomAbout for its own tests) -- the pointer-tracking code that turns a real
    // second finger into that same call is plain, inspectable C# (OnPointerMove), not re-derived here. Generic fit /
    // contrast / tap-target / heading / render checks of every entry are CardGalleryTests' own.
    public class ModelTurntableGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the gallery scene holds CardGalleryHarness");
            _harness.EnsureBuilt();
            yield return null;
        }

        private IEnumerator ShowBlock(string name, System.Action<ModelTurntableBlockView> got)
        {
            _harness.Show(IndexOf(name));
            yield return CardGalleryChecks.SettledBlock(_harness, name, v => got((ModelTurntableBlockView)v));
        }

        // The average colour of the slot's own RenderTexture: reads the real pixels the surface draws, not a screen
        // capture (a lit 3D render has no flat "darkest pixel" the way a gallery picture does, so CardGalleryChecks.PixelAt's
        // picture-tuned heuristic is the wrong tool here)
        private static Color AverageColorOf(RenderTexture tex)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = tex;
            var pixels = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            pixels.Apply();
            RenderTexture.active = prev;
            var raw = pixels.GetPixels32();
            long r = 0, g = 0, b = 0;
            foreach (var p in raw) { r += p.r; g += p.g; b += p.b; }
            Object.Destroy(pixels);
            int n = raw.Length;
            return new Color(r / (255f * n), g / (255f * n), b / (255f * n));
        }

        [UnityTest]
        public IEnumerator DefaultModel_LoadsAndRenders_ARealDragRotatesIt_APinchZoomsWithinLimits()
        {
            ModelTurntableBlockView view = null;
            yield return ShowBlock("model_3d_turntable_default", v => view = v);
            yield return null; // Request()'s onReady already fired synchronously; one frame settles the first Refresh

            Assert.IsTrue(view.ShowsModel, "the Framework's own default model loaded and is drawn");
            Assert.AreEqual("Drag to rotate, pinch to zoom", view.Hint.text);

            Color before = AverageColorOf(view.Texture);

            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(120f, 0f));
            yield return null;
            Assert.AreNotEqual(0f, view.State.Yaw, "a real one-finger drag rotated the model");

            Color afterDrag = AverageColorOf(view.Texture);
            Assert.AreNotEqual(before, afterDrag, "the rotation actually changed what is drawn, not just the state");

            float yawAfterDrag = view.State.Yaw;
            view.Pinch(1.5f);
            yield return null;
            Assert.AreEqual(yawAfterDrag, view.State.Yaw, "a pinch never turns the model, only its zoom");
            Assert.AreEqual(1.5f, view.State.Zoom, 0.001f);

            view.Pinch(100f); // - way past the limit
            yield return null;
            Assert.AreEqual(TurntableRule.MaxZoom, view.State.Zoom, 0.001f, "zoom stays inside TurntableRule's own limits");
        }

        // _3.1 10A.3-fix.2: a real 90 degree drag (DegreesPerPixel 0.3 -> 300 px) is the turn that used to put the arch's base on
        // the frame's edge. Its real drawn pixels, read back from the slot's own texture, stay inside with a margin.
        [UnityTest]
        public IEnumerator DefaultModel_ARealNinetyDegreeDrag_KeepsEveryPixelInsideTheStage()
        {
            ModelTurntableBlockView view = null;
            yield return ShowBlock("model_3d_turntable_default", v => view = v);
            yield return null;
            Assert.IsTrue(view.ShowsModel, "precondition: the model is drawn");
            PreviewPixels.AssertDrawnInsideWithMargin(view.Texture, "at rest");

            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(300f, 0f));
            yield return null;
            Assert.AreEqual(90f, view.State.Yaw, 1f, "precondition: the real drag turned the model a quarter turn");
            PreviewPixels.AssertDrawnInsideWithMargin(view.Texture, "after a real 90 degree drag");
        }

        [UnityTest]
        public IEnumerator AutoSpin_TurnsOnItsOwnAfterAPause_AndAFingerStopsIt()
        {
            ModelTurntableBlockView view = null;
            yield return ShowBlock("model_3d_turntable_autospin", v => view = v);
            yield return null;
            Assert.IsTrue(view.ShowsModel);

            float startYaw = view.State.Yaw;
            // - TurntableRule.AutoSpinResumeAfter (2s) then it turns: real wall-clock time, the UI Toolkit scheduler's own
            yield return new WaitForSecondsRealtime(2.6f);
            Assert.AreNotEqual(startYaw, view.State.Yaw, "auto spin turned the model on its own after the pause");

            // - a real touch stops it: the drag itself resets the idle clock (TurntableRule.Drag's IdleSeconds = 0)
            float yawOnTouch = view.State.Yaw;
            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, Vector2.zero, frames: 1);
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.AreEqual(yawOnTouch, view.State.Yaw, 0.5f, "just touched: the pause has not elapsed again yet, so no new spin");
        }

        // _3.1 10A.2c.2: auto-spin used to tick (and render) every 33 ms for as long as the block was bound, even
        // scrolled out of the stack's viewport or sitting under the sheet's Peek stop. Scrolling the block out (a
        // real ScrollView.ScrollTo, the same one the stack itself uses) proves the state stops advancing; scrolling
        // it back proves it resumes without needing a rebind.
        [UnityTest]
        public IEnumerator AutoSpin_StopsAdvancing_WhileScrolledOutOfTheViewport_AndResumesBackInView()
        {
            ModelTurntableBlockView view = null;
            yield return ShowBlock("model_3d_turntable_autospin", v => view = v);
            yield return null;
            Assert.IsTrue(view.ShowsModel);

            var stack = _harness.Sheet.Stack;
            var slot = stack.SlotOf(view);
            var farAway = new VisualElement { style = { height = 4000f } };
            stack.Scroll.contentContainer.Add(farAway);
            yield return null; // let the new spacer's height enter layout before asking to scroll to it
            stack.Scroll.ScrollTo(farAway);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsFalse(slot.worldBound.Overlaps(stack.Scroll.contentViewport.worldBound), "the block's slot is really out of the viewport now");

            float yawWhileHidden = view.State.Yaw;
            yield return new WaitForSecondsRealtime(2.6f); // past TurntableRule.AutoSpinResumeAfter
            Assert.AreEqual(yawWhileHidden, view.State.Yaw, "scrolled out: auto spin asked for nothing, so the state never advanced");

            stack.Scroll.ScrollTo(slot);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(slot.worldBound.Overlaps(stack.Scroll.contentViewport.worldBound), "back in the viewport");
            yield return new WaitForSecondsRealtime(2.6f);
            Assert.AreNotEqual(yawWhileHidden, view.State.Yaw, "back in view: auto spin resumed on its own, no rebind needed");

            stack.Scroll.contentContainer.Remove(farAway);
        }

        [UnityTest]
        public IEnumerator AMissingModel_WithAFallbackPicture_ShowsTheFallback_NeverTheSurface()
        {
            ModelTurntableBlockView view = null;
            yield return ShowBlock("model_3d_turntable_missing-with-fallback", v => view = v);
            yield return null;
            Assert.IsFalse(view.ShowsModel);
            Assert.AreEqual(DisplayStyle.Flex, view.Fallback.Root.resolvedStyle.display);
            Assert.IsNotNull(view.Fallback.Texture, "the authored Fallback Picture loaded and shows in place of the model");
        }

        [UnityTest]
        public IEnumerator AMissingModel_WithNoFallbackPicture_ShowsCardImagesOwnUnavailableWords()
        {
            ModelTurntableBlockView view = null;
            yield return ShowBlock("model_3d_turntable_missing-no-fallback", v => view = v);
            yield return null;
            Assert.IsFalse(view.ShowsModel);
            Assert.AreEqual(DisplayStyle.Flex, view.Fallback.Root.resolvedStyle.display);
            Assert.AreNotEqual("", view.Fallback.Unavailable.text, "no Fallback Picture authored: CardImage's own 'unavailable' words");
        }

        // _3.1 10A.3.1: Display Takeover shows a non-interactive teaser (the same first render, the model's name, an
        // open-full-screen button); a real tap opens the model full screen through a SECOND preview slot (the card's
        // own resting slot, and its picture, are untouched); Back closes it and releases that second slot, leaving
        // nothing behind.
        [UnityTest]
        public IEnumerator DisplayTakeover_IsANonInteractiveTeaser_ARealTapOpensItFullScreen_ARealDragRotatesIt_BackReleasesTheSecondSlot()
        {
            ModelTurntableBlockView view = null;
            yield return ShowBlock("model_3d_turntable_takeover", v => view = v);
            yield return null;
            Assert.AreEqual(CardOptions.DisplayTakeover, view.Display);
            Assert.AreEqual(DisplayStyle.None, view.Hint.resolvedStyle.display, "no drag hint on a teaser that cannot be dragged");
            Assert.AreEqual(DisplayStyle.Flex, view.TeaserOpenButton.resolvedStyle.display);
            Assert.AreEqual("The stone arch", view.TeaserName.text);
            Assert.IsTrue(view.ShowsModel, "the teaser's own static first render (no fallback authored)");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.TeaserOpenButton.worldBound.width, view.TeaserOpenButton.worldBound.height));
            yield return CardGalleryChecks.Render("Card_model3d_takeover_teaser");

            // - dragging the teaser's own picture must never rotate it: only the open-full-screen button reacts
            float restingYaw = view.State.Yaw;
            yield return CardTestInput.DragFrom(view.Frame.panel, view.Frame.worldBound.center, new Vector2(120f, 0f));
            yield return null;
            Assert.AreEqual(restingYaw, view.State.Yaw, "the teaser itself does not rotate on drag");

            int before = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;

            yield return CardTestInput.Tap(view.TeaserOpenButton.panel, view.TeaserOpenButton.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            var takeover = _harness.Sheet.Takeover;
            Assert.IsTrue(takeover.IsOpen, "a real tap on the teaser's button opened it full screen");
            Assert.IsTrue(takeover.Crumb.text.EndsWith(" > The stone arch"), "the card's title, then the model's own");

            var full = view.FullScreenView;
            Assert.IsNotNull(full, "a second instance over a second preview slot");
            Assert.AreEqual(CardOptions.DisplayInline, full.Display, "full screen is never itself a teaser");
            Assert.AreEqual(DisplayStyle.Flex, full.Hint.resolvedStyle.display, "the full-screen view offers the real drag/pinch hint");
            Assert.AreNotSame(view.Texture, full.Texture, "a second slot: its own RenderTexture, not the teaser's");
            Assert.IsTrue(full.ShowsModel);
            yield return CardGalleryChecks.Render("Card_model3d_takeover_fullscreen");

            float before2 = full.State.Yaw;
            yield return CardTestInput.DragFrom(full.Frame.panel, full.Frame.worldBound.center, new Vector2(120f, 0f));
            yield return null;
            Assert.AreNotEqual(before2, full.State.Yaw, "a real one-finger drag rotates it full screen");
            Assert.AreEqual(restingYaw, view.State.Yaw, "and never touches the teaser's own (resting) state");

            yield return CardTestInput.Tap(takeover.Back.panel, takeover.Back.worldBound.center);
            yield return CardTestInput.Settle(0.15f);
            Assert.IsFalse(takeover.IsOpen, "Back closed it");
            Assert.IsNull(view.FullScreenView, "and the second slot was let go");
            Assert.IsTrue(view.ShowsModel, "the card's own teaser picture is untouched");

            yield return null;
            int after = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(before, after, "nothing was left behind: the second slot's model/root are gone, the shared rig stays");
        }
    }
}
