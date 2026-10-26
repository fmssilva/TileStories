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
        // given, is a PoiCardSheetView whose drag's Clock (SheetDrag.Clock) this drives on a fixed per-frame timestep instead of real time, so a
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
            if (slowSheet != null) slowSheet.Drag.Clock = () => fakeTime;
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
            if (slowSheet != null) slowSheet.Drag.Clock = () => Time.unscaledTime;
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

        // The text of the two places a --ts-* token is defined: the card's own (CardTokens.uss) and the app theme (generated from the theme
        // asset; ThemeUssStaleTests holds it equal to the asset). One place each, never both (ThemeTokenGuardTests)
        public static string TokenFiles() =>
            System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Screen/Cards/CardTokens.uss") + "\n" +
            System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Theme/Generated/Theme.default.uss");

        // A --ts-* size token as its one source defines it (the card's tokens or the theme): "24px", or a plain number the C#
        // reads (panel units, like --ts-sheet-top-gap)
        public static float TokenPx(string token)
        {
            var match = System.Text.RegularExpressions.Regex.Match(TokenFiles(), token + @":\s*([0-9.]+)(px)?\s*;");
            Assert.IsTrue(match.Success, token + " is defined in CardTokens.uss or the theme");
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

        // Which of `slots` (each section's first slot, or each target's, in the stack's order) the reader is in, read from the SCREEN:
        // each top is measured where the slot is DRAWN (its distance below the viewport's top, plus the offset) and NavScrollRule's own
        // spy answers -- the rule is the spec (NavScrollRuleTests), the screen is the input, never the controller's bookkeeping. A slot
        // not shown (none, display none, set aside) has reached no line. One helper for every fixture: the spy's rule changed once
        // (_3.3 P1b, a short section at the top is the one read) and nine private copies of the old one had to follow
        // The element a slot's chapter starts at: its section sheet when the slot is the first block the sheet shows, else the slot (read
        // from the tree itself, not from the stack's own answer)
        public static VisualElement ChapterStart(VisualElement slot)
        {
            var sheet = slot.parent;
            if (sheet == null || !sheet.ClassListContains(SectionSheets.SheetClass)) return slot;
            var first = sheet.Children().FirstOrDefault(c => c.ClassListContains("card-block-slot") && c.resolvedStyle.display == DisplayStyle.Flex);
            return first == slot ? sheet : slot;
        }

        public static int UnderTheSpyLine(ScrollView scroll, System.Collections.Generic.IEnumerable<VisualElement> slots)
        {
            var viewport = scroll.contentViewport.worldBound;
            float offset = scroll.scrollOffset.y;
            // - _3.3 P4.4c1: a section's place is where its chapter starts -- the sheet's top, when its first block is the sheet's first shown
            var tops = slots.Select(s => s == null || !IsShown(s, scroll.contentContainer) ? float.PositiveInfinity
                : ChapterStart(s).worldBound.yMin - viewport.yMin + offset).ToList();
            return NavScrollRule.CurrentSection(tops, offset, scroll.contentContainer.layout.height, scroll.contentViewport.layout.height);
        }

        // The SECTION the reader is in, read from the screen (_3.3 R10.7): the card's targets cut by hand into runs (a new run wherever the
        // section changes, from the model's own Targets and SectionOf -- never Model.Runs, the code under test), the run under the spy line
        // found by UnderTheSpyLine over each run's first slot, and its section named. With one run per section it is the old per-section
        // reading exactly; under an interleaved order a family that comes back is named again
        public static int SectionUnderTheSpyLine(ScrollView scroll, CardNavModel model, System.Func<string, VisualElement> slotOf)
        {
            var firsts = new System.Collections.Generic.List<string>();
            var sections = new System.Collections.Generic.List<int>();
            foreach (string target in model.Targets)
            {
                int section = model.SectionOf(target);
                if (sections.Count > 0 && sections[sections.Count - 1] == section) continue;
                firsts.Add(target);
                sections.Add(section);
            }
            int run = UnderTheSpyLine(scroll, firsts.Select(slotOf));
            return run >= 0 ? sections[run] : -1;
        }

        // A finger's drag from panel position `start` by `delta` (panel units), over `frames` frames: touch pointer events, what a
        // phone sends and what a ScrollView drag-scrolls its content on
        public static IEnumerator TouchDrag(IPanel panel, Vector2 start, Vector2 delta, int frames = 12)
        {
            Send(panel, EventType.MouseDown, start, UnityEngine.PointerType.Touch);
            yield return null;
            for (int i = 1; i <= frames; i++)
            {
                Send(panel, EventType.MouseDrag, start + delta * i / frames, UnityEngine.PointerType.Touch);
                yield return null;
            }
            Send(panel, EventType.MouseUp, start + delta, UnityEngine.PointerType.Touch);
            yield return null;
        }

        // A finger put down, moved and lifted BY HAND, one event each, for a test that must look at the card while the finger is still down
        // (TouchDrag does the whole motion in one go): the same touch events TouchDrag sends
        public static void TouchDown(IPanel panel, Vector2 position) => Send(panel, EventType.MouseDown, position, UnityEngine.PointerType.Touch);
        public static void TouchMove(IPanel panel, Vector2 position) => Send(panel, EventType.MouseDrag, position, UnityEngine.PointerType.Touch);
        public static void TouchUp(IPanel panel, Vector2 position) => Send(panel, EventType.MouseUp, position, UnityEngine.PointerType.Touch);

        private static void Send(IPanel panel, EventType type, Vector2 position, UnityEngine.PointerType pointer = UnityEngine.PointerType.Mouse)
        {
            var e = new Event { type = type, mousePosition = position, button = 0, clickCount = 1, pointerType = pointer };
            EventBase evt = type switch
            {
                EventType.MouseDown => PointerDownEvent.GetPooled(e),
                EventType.MouseUp => PointerUpEvent.GetPooled(e),
                _ => PointerMoveEvent.GetPooled(e),
            };
            using (evt) panel.visualTree.SendEvent(evt);
        }

        // A real finger reads the card down until its hero has left the viewport (_3.3 P4.4a: the footer waits for the hero), then a small
        // move back up -- reading down hid the bottom area (Hide While Scrolling = down), a move up past ShowAfterTravel is the reader
        // looking for it -- with the hero still out of view. `dragStack` is the fixture's own real drag of the stack (dy: - = down the card)
        public static IEnumerator ReadPastTheHero(PoiCardSheetView sheet, System.Func<float, IEnumerator> dragStack)
        {
            var header = sheet.Stack.HeaderView;
            var scroll = sheet.Stack.Scroll;
            for (int i = 0; i < 10 && scroll.scrollOffset.y < header.HeroBottom() + 120f; i++) yield return dragStack(-120f);
            yield return dragStack(30f);
            Assert.IsFalse(HeroRule.InView(scroll.scrollOffset.y, header.HeroBottom()), "precondition: the hero scrolled away (offset "
                + scroll.scrollOffset.y + ", hero bottom " + header.HeroBottom() + ")");
        }

        // Wait until the sheet's height transition has finished (it animates for --ts-duration)
        // Since _3.3 P4.4b the sheet's ground between blocks is translucent glass: the live scene behind it (animated markers) shows through
        // and a render of the sheet never holds still. A measurement of the CARD puts a still backdrop behind it (where the camera feed
        // would be), once per card layer; it stays for the test's scene
        // ...and the card's own moving light (_3.3 P4.4c2: the glass lights and the rim light run on CardLight.Clock) stands at time 0
        public static void HoldTheWallStill(PoiCardSheetView sheet)
        {
            var light = sheet.Stack.Light;
            light.Clock = () => 0f;
            light.Tick();
            var root = sheet.Layer.parent;
            if (root == null || root.Q("test-still-backdrop") != null) return;
            var backdrop = new VisualElement { name = "test-still-backdrop", pickingMode = PickingMode.Ignore };
            backdrop.style.position = Position.Absolute;
            backdrop.style.left = 0; backdrop.style.top = 0; backdrop.style.right = 0; backdrop.style.bottom = 0;
            backdrop.style.backgroundColor = new Color(0.42f, 0.4f, 0.37f);
            root.Insert(0, backdrop);
        }

        // The language the old chip's one tap switched to: the wall's next language after the shown one (wrapping round)
        public static string NextLanguage(PoiCardSheetView sheet)
        {
            var choices = CardLanguageRule.Choices(sheet.Context?.Settings?.languages);
            Assert.GreaterOrEqual(choices.Count, 2, "precondition: the wall offers two languages to switch between");
            return choices[(choices.IndexOf(sheet.Context.Language) + 1) % choices.Count];
        }

        // Switch the card to its next language with REAL taps (_3.3 P4.4b): the Aa button -- checked to be what a finger at its centre
        // touches -- then that language in the visitor's settings
        public static IEnumerator TapNextLanguage(PoiCardSheetView sheet)
        {
            string next = NextLanguage(sheet);
            var aa = sheet.SettingsButton;
            var touched = aa.panel.Pick(aa.worldBound.center);
            Assert.IsTrue(touched == aa || aa.Contains(touched), "precondition: a tap at Aa's centre lands on it, not on '" + touched?.name + "'");
            yield return Tap(aa.panel, aa.worldBound.center);
            yield return null;
            Assert.AreEqual(PoiCardSheetView.SettingsKey, sheet.Overlay.OpenKey, "the tap on Aa opened the visitor's settings");
            var choice = sheet.Settings.Choice(next);
            Assert.IsNotNull(choice, "the settings list " + next);
            yield return Tap(choice.panel, choice.worldBound.center);
            yield return Settle();
            Assert.AreEqual(next, sheet.Context?.Language, "the tap on " + next + " switched the card");
        }

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
                        TestRunRecorder.Write(TestRunRecorder.GameViewSize, "pinned " + label + " (index " + previous + " -> " + existing + ")");
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
                    TestRunRecorder.Write(TestRunRecorder.GameViewSize, "restored index " + pinned.PreviousIndex);
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
