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
    }
}
