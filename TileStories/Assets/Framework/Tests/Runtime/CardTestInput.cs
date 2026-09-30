using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Real pointer input for the POI Detail Card tests: UI Toolkit pointer events sent to the panel at a panel
    // position, so the panel itself picks the element under the point (and pointer capture routes the moves),
    // exactly as a finger's events would. A drag is spread over frames so the sheet measures a real speed.
    public static class CardTestInput
    {
        // A slow drag's per-frame time (seconds) when `slowSheet` drives it (Drag below): comfortably under the flick
        // threshold for any pair of stops (a stop distance never exceeds the available height -- SheetStopRule.Compute
        // clamps every stop to it -- so this margin holds regardless of the wall or device), whatever the real frame rate is
        private const float SlowDragFrameSeconds = 0.1f;

        // Drag from the centre of `from` by `deltaY` panel units (negative = up), over `frames` frames. `slowSheet`, when
        // given, is a PoiCardSheetView whose Clock this drives on a fixed per-frame timestep instead of real time, so a
        // deliberately "slow" drag (the default frame count, no flick intended) reads as slow to SheetStopRule.Snap on any
        // machine: real `yield return null` frames can run faster than intended, which would otherwise misread the same
        // drag as a flick and overshoot a stop (_3.1 [7B], the sheet's own version of PoiCardHost.Clock's fix). Leave it
        // null for a drag that must stay real-time (a flick test drives its own explicit low frame count on purpose).
        public static IEnumerator Drag(VisualElement from, float deltaY, int frames = 12, PoiCardSheetView slowSheet = null)
        {
            Assert.IsNotNull(from?.panel, "the drag starts on an element of a live panel");
            var panel = from.panel;
            Vector2 start = from.worldBound.center;
            float fakeTime = 0f;
            if (slowSheet != null) slowSheet.Clock = () => fakeTime;
            Send(panel, EventType.MouseDown, start);
            yield return null;
            for (int i = 1; i <= frames; i++)
            {
                if (slowSheet != null) fakeTime += SlowDragFrameSeconds;
                Send(panel, EventType.MouseDrag, start + new Vector2(0f, deltaY * i / frames));
                yield return null;
            }
            Send(panel, EventType.MouseUp, start + new Vector2(0f, deltaY));
            yield return null;
            if (slowSheet != null) slowSheet.Clock = () => Time.unscaledTime;
        }

        // Drag from panel position `start` by `delta` (panel units, any direction), over `frames` frames
        public static IEnumerator DragFrom(IPanel panel, Vector2 start, Vector2 delta, int frames = 12)
        {
            Send(panel, EventType.MouseDown, start);
            yield return null;
            for (int i = 1; i <= frames; i++)
            {
                Send(panel, EventType.MouseDrag, start + delta * i / frames);
                yield return null;
            }
            Send(panel, EventType.MouseUp, start + delta);
            yield return null;
        }

        // Ctrl + mouse wheel at a panel position (the desktop's pinch: a trackpad pinch arrives as exactly this); + = out
        public static IEnumerator CtrlWheel(IPanel panel, Vector2 at, float notches, int frames = 1)
        {
            for (int i = 0; i < frames; i++)
            {
                var e = new Event { type = EventType.ScrollWheel, mousePosition = at, delta = new Vector2(0f, notches / frames), modifiers = EventModifiers.Control };
                using (var evt = WheelEvent.GetPooled(e)) panel.visualTree.SendEvent(evt);
                yield return null;
            }
        }

        // A tap (press + release, no move) at a panel position: the panel picks the element under it
        public static IEnumerator Tap(IPanel panel, Vector2 position)
        {
            Send(panel, EventType.MouseDown, position);
            yield return null;
            Send(panel, EventType.MouseUp, position);
            yield return null;
        }

        // A real tap on `target` after scrolling `scroll` to it and asserting it is inside the visible part (40-testing 4.2.4b):
        // scrolled to as a whole, a block taller than the viewport lines up its far edge and can leave a button outside the
        // view, where the tap would land on something else -- the precondition names that instead of a confusing later failure
        public static IEnumerator TapInView(ScrollView scroll, VisualElement target)
        {
            scroll.ScrollTo(target);
            yield return Settle(0.15f);
            Rect visible = scroll.contentViewport.worldBound;
            Assert.IsTrue(visible.Contains(target.worldBound.center),
                "precondition: " + target.name + " is inside the visible part of the card: " + target.worldBound + " in " + visible);
            yield return Tap(target.panel, target.worldBound.center);
        }

        // Scroll the mouse wheel over the centre of `over` (panel units per notch as the ScrollView defines them; + = down),
        // one notch per frame: the panel picks the element under the point, exactly as a real wheel's events are routed
        public static IEnumerator Wheel(VisualElement over, float notches, int frames = 1)
        {
            Assert.IsNotNull(over?.panel, "the wheel turns over an element of a live panel");
            Vector2 at = over.worldBound.center;
            for (int i = 0; i < frames; i++)
            {
                var e = new Event { type = EventType.ScrollWheel, mousePosition = at, delta = new Vector2(0f, notches / frames) };
                using (var evt = WheelEvent.GetPooled(e)) over.panel.visualTree.SendEvent(evt);
                yield return null;
            }
        }

        // The colour actually behind an element: its own and its ancestors' backgrounds composited, from the nearest
        // opaque one inwards (a translucent chip over the card is judged against what it really covers)
        public static Color EffectiveBackground(VisualElement element)
        {
            var chain = new System.Collections.Generic.List<Color>();
            for (var e = element; e != null; e = e.parent)
            {
                var c = e.resolvedStyle.backgroundColor;
                chain.Add(c);
                if (c.a >= 0.999f) break;
            }
            Color result = Color.black;
            for (int i = chain.Count - 1; i >= 0; i--)
                result = Color.Lerp(result, new Color(chain[i].r, chain[i].g, chain[i].b, 1f), chain[i].a);
            return result;
        }

        // The name of the picture an element's background draws (a sprite or a texture), or null
        public static string BackgroundPictureName(VisualElement element)
        {
            var bg = element.resolvedStyle.backgroundImage;
            return bg.sprite != null ? bg.sprite.name : bg.texture != null ? bg.texture.name : null;
        }

        // A --ts-* size token as CardTokens.uss defines it (the one place the value lives): "24px", or a plain number the C#
        // reads (panel units, like --ts-sheet-top-gap)
        public static float TokenPx(string token)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Cards/CardTokens.uss"), token + @":\s*([0-9.]+)(px)?\s*;");
            Assert.IsTrue(match.Success, token + " is defined in CardTokens.uss");
            float value = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            Assert.Greater(value, 0f, token);
            return value;
        }

        // Whether an element is laid out as shown (it and every ancestor up to `root` displayed)
        public static bool IsShown(VisualElement element, VisualElement root)
        {
            for (var e = element; e != null; e = e.parent)
            {
                if (e.resolvedStyle.display == DisplayStyle.None) return false;
                if (e == root) return true;
            }
            return false;
        }

        private static void Send(IPanel panel, EventType type, Vector2 position)
        {
            var e = new Event { type = type, mousePosition = position, button = 0, clickCount = 1 };
            EventBase evt = type switch
            {
                EventType.MouseDown => PointerDownEvent.GetPooled(e),
                EventType.MouseUp => PointerUpEvent.GetPooled(e),
                _ => PointerMoveEvent.GetPooled(e),
            };
            using (evt) panel.visualTree.SendEvent(evt);
        }

        // Wait until the sheet's height transition has finished (it animates for --ts-duration)
        public static IEnumerator Settle(float seconds = 0.45f)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        // WCAG contrast of a text colour (alpha blended over its background) against that background
        public static float Contrast(Color text, Color background)
        {
            var blended = Color.Lerp(background, new Color(text.r, text.g, text.b, 1f), text.a);
            return UIAccessibility.ContrastRatio(blended, background);
        }

        // Pins the Editor's actual Game view resolution while it lives -- for the whole run, through FixedFrameForTheRun and
        // LivingRoomFixedFrameForTheRun (_3.1 [13-fix]): the shared PanelSettings
        // scales by "Scale With Screen Size" / "Match Width Or Height" (0.5), which blends BOTH axes from the real Game
        // view's own pixel size -- so an Editor whose Game view shrank or changed shape (a previous session's own screen
        // captures did this) silently reflows text wrapping and grid row breaks. That is a real layout bug this once
        // caused (Gallery_/QuickFacts_/Swatches_ failing on absolute grid positions), not environment noise. A
        // RenderTexture-backed panel would decouple layout from the Game view too, but PixelAt reads pixels via
        // ScreenCapture (the real Game view framebuffer), so redirecting the panel's render target breaks every
        // colour-reading test instead -- the fix must pin the real window, not move rendering off it. Unity exposes no
        // public API for this: GameViewSizes/GameViewSizeGroupType/GameViewSize/GameView are all internal to
        // UnityEditor.dll, so every step below goes through reflection. Dispose to restore the Editor's own selection.
        public sealed class FixedGameViewSize : System.IDisposable
        {
#if UNITY_EDITOR
            // One custom Game view size per frame, named by its size (a label shared by two sizes would select the wrong one)
            private static string SizeLabel(int width, int height) => "TileStories_TestFrame_" + width + "x" + height;

            // One pinned window: its own previous size index, restored on Dispose (_3.1 10A.2c.4 -- the old single-window
            // version left a second open Game view at whatever size the developer had it, which broke pixel tests just as
            // surely as no pin at all: SetUp now pins every one that exists when the run starts).
            private sealed class PinnedWindow
            {
                public UnityEditor.EditorWindow Window;
                public System.Reflection.PropertyInfo SelectedSizeIndexProperty;
                public int PreviousIndex;
            }

            private readonly System.Collections.Generic.List<PinnedWindow> _pinned = new();

            public FixedGameViewSize(int width = 390, int height = 844)
            {
                var gameViewSizesType = System.Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
                var singletonType = typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
                object gameViewSizes = singletonType.GetProperty("instance").GetValue(null, null);
                object currentGroupType = gameViewSizesType.GetProperty("currentGroupType").GetValue(gameViewSizes, null);
                object group = gameViewSizesType.GetMethod("GetGroup").Invoke(gameViewSizes, new[] { currentGroupType });
                var groupType = group.GetType();

                var gameViews = Resources.FindObjectsOfTypeAll(typeof(UnityEditor.EditorWindow))
                    .Where(w => w.GetType().Name == "GameView")
                    .Cast<UnityEditor.EditorWindow>()
                    .ToList();
                Assert.IsNotEmpty(gameViews, "an open Game view (docked or floating) to pin its resolution");

                string label = SizeLabel(width, height);
                int existing = FindSize(group, label);
                if (existing < 0)
                {
                    var sizeType = System.Type.GetType("UnityEditor.GameViewSize,UnityEditor");
                    object fixedResolution = System.Type.GetType("UnityEditor.GameViewSizeType,UnityEditor")
                        .GetField("FixedResolution").GetValue(null);
                    var ctor = sizeType.GetConstructor(new[] { fixedResolution.GetType(), typeof(int), typeof(int), typeof(string) });
                    object size = ctor.Invoke(new object[] { fixedResolution, width, height, label });
                    groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                    existing = FindSize(group, label);
                }
                Assert.GreaterOrEqual(existing, 0, "the fixed test size was added to the current Game view size group");

                foreach (var gameView in gameViews)
                {
                    try
                    {
                        var property = gameView.GetType().GetProperty("selectedSizeIndex",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        int previous = (int)property.GetValue(gameView, null);
                        property.SetValue(gameView, existing, null);
                        gameView.Repaint();
                        _pinned.Add(new PinnedWindow { Window = gameView, SelectedSizeIndexProperty = property, PreviousIndex = previous });
                    }
                    catch (System.Exception e)
                    {
                        Assert.Fail($"FixedFrameForTheRun could not pin Game view '{gameView.titleContent.text}' to {width}x{height}: {e.Message}");
                    }
                }
            }

            private static int FindSize(object group, string label)
            {
                var displayTexts = (string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null);
                for (int i = 0; i < displayTexts.Length; i++)
                {
                    string text = displayTexts[i];
                    int paren = text.IndexOf(" (");
                    if (paren >= 0) text = text.Substring(0, paren);
                    if (text == label) return i;
                }
                return -1;
            }

            public void Dispose()
            {
                foreach (var pinned in _pinned)
                {
                    if (pinned.Window == null) continue; // a window closed mid-run: nothing left to restore
                    pinned.SelectedSizeIndexProperty.SetValue(pinned.Window, pinned.PreviousIndex, null);
                    pinned.Window.Repaint();
                }
                _pinned.Clear();
            }
#else
            public FixedGameViewSize(int width = 390, int height = 844) { }
            public void Dispose() { }
#endif
        }
    }
}
