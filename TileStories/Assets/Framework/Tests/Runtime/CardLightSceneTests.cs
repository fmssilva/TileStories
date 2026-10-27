using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the card's moving light (_3.3 P4.4c2, R11) on the REAL LivingRoomScene, The Lamp at Full, an in-memory CardLocalState, a
    // still backdrop behind the glass and the card's light clock set by the test (CardLight.Clock -- time is the test's):
    //   - every section sheet carries its family's living glass as a SIBLING layer under its edge, opener and blocks; its colours are the
    //     theme's, repainted (same material) after a real tap on Dark; a bare run carries none;
    //   - the glass lights drift with the clock and stand still under Reduce Motion (pushed live), the still frame still lit;
    //   - ONE rim light in the card, in the block LitBlockRule names, following real drags block by block with a fade, never two lit;
    //     it turns with the clock, full white on light metal, the family's hue at about half strength and narrower on dark.
    // Frame sequences (every 0.5 s of clock over 6 s) and the move on scroll are saved under TestEvidence/Light/ for the visual check.
    public class CardLightSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private CardLight Light => Stack.Light;

        [UnityTearDown]
        public IEnumerator RestoreTheMode()
        {
            ThemeController.SetMode("");
            yield return null;
        }

        private IEnumerator OpenFull(string poi)
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - Sheet.Stops.Full) > 0.5f; frame++) yield return null;
            Assert.IsTrue(Sheet.IsOpen, "precondition: " + poi + "'s card is open at Full");
            CardTestInput.HoldTheWallStill(Sheet);
            LookMeasure.FreezeHero(Sheet);
            SetClock(0f);
        }

        private void SetClock(float seconds)
        {
            Light.Clock = () => seconds;
            Light.Tick();
        }

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator TapMode(string mode)
        {
            var gear = Sheet.SettingsButton;
            yield return CardTestInput.Tap(gear.panel, gear.worldBound.center);
            yield return null;
            Assert.AreEqual(PoiCardSheetView.SettingsKey, Sheet.Overlay.OpenKey, "a real tap on the gear opened the card's settings");
            var choice = Sheet.Settings.Choice(mode);
            yield return CardTestInput.Tap(choice.panel, choice.worldBound.center);
            yield return CardTestInput.Settle();
            var app = Stack.Scroll.panel.visualTree.Q(className: ThemeController.AppClass);
            Assert.IsTrue(app != null && app.ClassListContains(ThemeController.ModeClassPrefix + mode), "the tap on " + mode + " switched the card");
            // - the settings panel stays open after a pick: put it away, the renders are of the card
            Sheet.Overlay.Close();
            yield return CardTestInput.Settle(0.3f);
        }

        private static Vector4 Linear(VisualElement under, string token)
        {
            Assert.IsTrue(CardTokenColours.TryResolve(under, token, out Color colour), "the theme holds " + token);
            var linear = colour.linear;
            return new Vector4(linear.r, linear.g, linear.b, colour.a);
        }

        private static Material MaterialOf(VisualElement element) => element.resolvedStyle.unityMaterial.material;

        private List<SectionSheets.Sheet> ShownSheets() =>
            Stack.Sheets.All.Where(s => s.Root.resolvedStyle.display == DisplayStyle.Flex).ToList();

        // The lit block as the rule names it, from the slots as they are drawn now (the rule itself: LitBlockRuleTests)
        private VisualElement ExpectedLit()
        {
            var slots = new List<VisualElement>();
            var blocks = new List<LitBlockRule.Block>();
            foreach (var sheet in ShownSheets())
                foreach (var slot in sheet.Slots.Where(s => s.ClassListContains(BlockFrame.FramedClass) && s.resolvedStyle.display == DisplayStyle.Flex && s.layout.height > 0f))
                {
                    float top = Stack.TopInScroll(slot);
                    blocks.Add(new LitBlockRule.Block(top, top + slot.layout.height));
                    slots.Add(slot);
                }
            int lit = LitBlockRule.Pick(blocks, Stack.Scroll.scrollOffset.y, Stack.Scroll.contentViewport.layout.height);
            return lit >= 0 ? slots[lit] : null;
        }

        private List<VisualElement> RimLayers() => Sheet.Root.panel.visualTree.Query(className: RimLightView.LayerClass).ToList();

        [UnityTest]
        public IEnumerator EverySheet_CarriesItsFamilysLivingGlass_BehindItsContent_RepaintedOnARealTapOnDark()
        {
            yield return OpenFull("lamp");
            var sheets = ShownSheets();
            Assert.GreaterOrEqual(sheets.Count(s => !s.Run.Bare), 6, "precondition: The Lamp shows its families' sheets");
            var materials = new Dictionary<SectionSheets.Sheet, Material>();
            foreach (var sheet in sheets)
            {
                var light = sheet.Light;
                Assert.AreEqual(sheet.Root, light.parent, "the light is a layer of its own sheet");
                int at = sheet.Root.IndexOf(light);
                Assert.Less(at, sheet.Root.IndexOf(sheet.Root.Q(className: SectionSheets.EdgeClass)), "under the sheet's edge");
                Assert.Less(at, sheet.Root.IndexOf(sheet.Opener.Root), "under the opener");
                foreach (var slot in sheet.Slots) Assert.Less(at, sheet.Root.IndexOf(slot), "under every block: a sibling, never their parent (R8 S4)");
                Assert.AreEqual(PickingMode.Ignore, light.pickingMode, "the light never takes a finger");
                if (sheet.Run.Bare)
                {
                    Assert.AreEqual(DisplayStyle.None, light.resolvedStyle.display, "a bare run (navigation) has no glass light");
                    continue;
                }
                var material = MaterialOf(light);
                Assert.IsNotNull(material, sheet.Family.Key + ": the sheet's light carries a material");
                StringAssert.EndsWith("SectionGlass", material.shader.name);
                Assert.AreEqual(Linear(light, SectionSheetRule.LightOf(sheet.Family.ColorToken)), material.GetVector(CardLight.HueId), sheet.Family.Key + ": light 1 is the theme's");
                Assert.AreEqual(Linear(light, SectionSheetRule.Light2Of(sheet.Family.ColorToken)), material.GetVector(CardLight.Hue2Id), sheet.Family.Key + ": light 2 is the theme's");
                Assert.AreEqual(1f, material.GetFloat(CardLight.AnimateId), "moving by default");
                // - only a sheet near the view draws its glass (an off-screen material costs a SetPass every frame, measured)
                var view = Stack.Scroll.contentViewport.layout.height;
                float offset = Stack.Scroll.scrollOffset.y;
                var r = sheet.Root.layout;
                bool near = r.yMax > offset - view * 0.5f && r.yMin < offset + view * 1.5f;
                Assert.AreEqual(near ? DisplayStyle.Flex : DisplayStyle.None, light.resolvedStyle.display,
                    sheet.Family.Key + " at " + r.yMin.ToString("0") + ": its glass draws only near the view (offset " + offset.ToString("0") + ")");
                if (!near) continue;
                var size = material.GetVector(CardLight.SizeId);
                Assert.AreEqual(light.layout.width, size.x, 0.01f, "the material knows the sheet's width");
                Assert.AreEqual(light.layout.height, size.y, 0.01f, "and its height (the lights tile its bands)");
                materials[sheet] = material;
            }

            yield return TapMode(CardOptions.ModeDark);
            foreach (var (sheet, material) in materials)
            {
                Assert.AreSame(material, MaterialOf(sheet.Light), sheet.Family.Key + ": the same material, no rebuild");
                Assert.IsTrue(ThemeController.TryResolverFor(sheet.Light, out var theme), "precondition: under the theme");
                Assert.IsTrue(theme.TryColour(SectionSheetRule.LightOf(sheet.Family.ColorToken), out var dark), "precondition: dark colours");
                Assert.AreEqual(Linear(sheet.Light, SectionSheetRule.LightOf(sheet.Family.ColorToken)), material.GetVector(CardLight.HueId), sheet.Family.Key + ": repainted dark");
                Assert.AreEqual(dark.a, material.GetVector(CardLight.HueId).w, 0.001f, "the dark (smoked) light's own alpha");
            }
        }

        // The colours along the first lit sheet's left padding strip (the glass between the sheet's edge and its blocks: no block covers it)
        private IEnumerator GlassStrip(SectionSheets.Sheet sheet, System.Action<Color[]> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var panel = sheet.Root.panel;
                var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(panel, Vector2.right).x - origin.x;
                var view = Stack.Scroll.contentViewport.worldBound;
                var bound = sheet.Root.worldBound;
                float x = bound.xMin + CardTestInput.TokenPx("--ts-space-4") * 0.5f;
                var colours = new List<Color>();
                for (float y = Mathf.Max(bound.yMin, view.yMin) + 4f; y < Mathf.Min(bound.yMax, view.yMax) - 4f; y += 8f)
                {
                    var screen = (new Vector2(x, y) - origin) / unit;
                    colours.Add(tex.GetPixel(Mathf.RoundToInt(screen.x), tex.height - 1 - Mathf.RoundToInt(screen.y)));
                }
                Assert.Greater(colours.Count, 20, "precondition: the sheet's strip is in view (" + colours.Count + " samples)");
                got(colours.ToArray());
            }
            finally { Object.Destroy(tex); }
        }

        private static float Difference(Color[] a, Color[] b)
        {
            float sum = 0f;
            for (int i = 0; i < Mathf.Min(a.Length, b.Length); i++) sum += Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b);
            return sum;
        }

        // The first non-bare sheet, scrolled so its top part fills the viewport
        private IEnumerator ShowFirstSheet(System.Action<SectionSheets.Sheet> got)
        {
            var sheet = ShownSheets().First(s => !s.Run.Bare);
            Stack.Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(sheet.Root));
            yield return CardTestInput.Settle(0.3f);
            got(sheet);
        }

        [UnityTest]
        public IEnumerator TheGlassLights_DriftWithTheClock_AndStandStillUnderReduceMotion_StillLit()
        {
            yield return OpenFull("lamp");
            SectionSheets.Sheet sheet = null;
            yield return ShowFirstSheet(s => sheet = s);
            Color[] at0 = null, at6 = null, still0 = null, still6 = null, unlit = null;
            yield return GlassStrip(sheet, c => at0 = c);
            SetClock(6.5f);
            yield return GlassStrip(sheet, c => at6 = c);
            float moving = Difference(at0, at6);

            yield return Push(c => c.card_settings.container.reduce_motion = true);
            Assert.IsTrue(Light.ReduceMotion, "Reduce Motion reached the card's light");
            yield return ShowFirstSheet(s => sheet = s);
            foreach (var s in ShownSheets().Where(s => !s.Run.Bare))
                Assert.AreEqual(0f, MaterialOf(s.Light).GetFloat(CardLight.AnimateId), s.Family.Key + ": _Animate 0");
            SetClock(0f);
            yield return GlassStrip(sheet, c => still0 = c);
            SetClock(6.5f);
            yield return GlassStrip(sheet, c => still6 = c);
            float standing = Difference(still0, still6);
            // - the still frame still reads as lit glass: the same strip with the lights' colours taken away differs
            var material = MaterialOf(sheet.Light);
            Vector4 hue = material.GetVector(CardLight.HueId), hue2 = material.GetVector(CardLight.Hue2Id);
            material.SetVector(CardLight.HueId, Vector4.zero);
            material.SetVector(CardLight.Hue2Id, Vector4.zero);
            yield return GlassStrip(sheet, c => unlit = c);
            material.SetVector(CardLight.HueId, hue);
            material.SetVector(CardLight.Hue2Id, hue2);
            float lit = Difference(still0, unlit);
            Debug.Log($"[Card] glass strip ({at0.Length} samples): moving {moving:0.000}, reduce motion {standing:0.000}, still vs unlit {lit:0.000}");

            Assert.Greater(moving, 0.5f, "between clock 0 and 6.5 s the lights moved along the strip (summed rgb difference " + moving + ")");
            Assert.Less(standing, 0.02f, "under Reduce Motion the same clocks draw the same strip (" + standing + ")");
            Assert.Greater(lit, 0.5f, "the still frame is lit glass, not plain glass (" + lit + ")");
        }

        [UnityTest]
        public IEnumerator OneRimLight_FollowsTheBlockBeingRead_AsRealDragsScroll_FadingBetweenBlocks()
        {
            yield return OpenFull("lamp");
            Assert.AreEqual(1, RimLayers().Count, "one rim light in the card");
            Assert.AreEqual(ExpectedLit(), Stack.RimLight.LitSlot, "at the top: the block the rule names");
            var visited = new List<VisualElement> { Stack.RimLight.LitSlot };
            bool sawFade = false;
            for (int drag = 0; drag < 8; drag++)
            {
                VisualElement before = Stack.RimLight.LitSlot;
                float offsetBefore = Stack.Scroll.scrollOffset.y;
                var viewport = Stack.Scroll.contentViewport.worldBound;
                // - a real finger stroke up the viewport (a phone's ScrollView scrolls by touch)
                yield return CardTestInput.TouchDrag(Stack.Scroll.panel, new Vector2(viewport.center.x, viewport.yMax - 40f), new Vector2(0f, -viewport.height * 0.35f));
                Assert.Greater(Stack.Scroll.scrollOffset.y, offsetBefore + 20f, "precondition: drag " + drag + " really scrolled the stack (" + offsetBefore + " -> " + Stack.Scroll.scrollOffset.y + ")");
                // - the frame the light changes block: it fades out where it was, never jumps
                if (Stack.RimLight.LitSlot != before && Stack.RimLight.Layer.parent == before && Stack.RimLight.Layer.resolvedStyle.opacity < 1f) sawFade = true;
                yield return RestAfterTheGlide();
                var expected = ExpectedLit();
                Assert.AreEqual(expected, Stack.RimLight.LitSlot, "after drag " + drag + " the light is on the block the rule names");
                Assert.AreEqual(expected, Stack.RimLight.Layer.parent, "and stands in it");
                Assert.AreEqual(1f, Stack.RimLight.Layer.resolvedStyle.opacity, 0.001f, "faded in");
                Assert.AreEqual(1, RimLayers().Count, "never two lit");
                Assert.AreEqual(2, expected.IndexOf(Stack.RimLight.Layer), "behind the block's heading and content, after the frame's two parts");
                if (visited[^1] != expected) visited.Add(expected);
            }
            Assert.GreaterOrEqual(visited.Count, 3, "the drags walked the light over several blocks (" + visited.Count + ")");
            for (int i = 1; i < visited.Count; i++)
                Assert.Greater(Stack.TopInScroll(visited[i]), Stack.TopInScroll(visited[i - 1]), "down the card, block after block");
            // - a fade the drags may have stepped over is caught here on purpose: one step of scroll moves the light
            if (!sawFade)
            {
                var from = Stack.RimLight.LitSlot;
                Stack.Scroll.scrollOffset += new Vector2(0f, from.layout.height);
                yield return null;
                yield return null;
                Assert.AreNotEqual(from, Stack.RimLight.LitSlot, "precondition: the step moved the light");
                Assert.AreEqual(from, Stack.RimLight.Layer.parent, "the light first fades out where it was");
                Assert.Less(Stack.RimLight.Layer.resolvedStyle.opacity, 1f, "fading");
            }
        }

        // A finger's stroke leaves the stack gliding: wait until the scroll held still for 10 frames, then until the light stands faded in
        // where it belongs (conditions, never seconds; 600 frames at most)
        private IEnumerator RestAfterTheGlide()
        {
            float last = float.NaN;
            int still = 0;
            for (int frame = 0; frame < 600 && still < 10; frame++)
            {
                yield return null;
                float now = Stack.Scroll.scrollOffset.y;
                still = Mathf.Abs(now - last) < 0.01f ? still + 1 : 0;
                last = now;
            }
            Assert.GreaterOrEqual(still, 10, "precondition: the stack came to rest after the stroke");
            var rim = Stack.RimLight;
            for (int frame = 0; frame < 600 && (rim.Layer.parent != rim.LitSlot || rim.Layer.resolvedStyle.opacity < 1f); frame++) yield return null;
        }

        // The rim's inner band of the lit block as drawn: rows of pixels just inside the slot's outer line, top and bottom (the outer line
        // itself is the block's measured edge and never lit)
        private IEnumerator RimRow(System.Action<Color[]> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var slot = Stack.RimLight.LitSlot;
                var panel = slot.panel;
                var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(panel, Vector2.right).x - origin.x;
                var bound = slot.worldBound;
                var colours = new List<Color>();
                for (float x = bound.xMin + 24f; x < bound.xMax - 24f; x += 2f)
                    foreach (float y in new[] { bound.yMin + slot.resolvedStyle.borderTopWidth + 0.5f, bound.yMax - slot.resolvedStyle.borderBottomWidth - 0.5f })
                    {
                        var screen = (new Vector2(x, y) - origin) / unit;
                        colours.Add(tex.GetPixel(Mathf.RoundToInt(screen.x), tex.height - 1 - Mathf.RoundToInt(screen.y)));
                    }
                got(colours.ToArray());
            }
            finally { Object.Destroy(tex); }
        }

        [UnityTest]
        public IEnumerator TheRimLight_TurnsWithTheClock_FullWhiteOnLight_HalfAndNarrowerOnDark_StillUnderReduceMotion()
        {
            yield return OpenFull("lamp");
            Stack.Scroll.scrollOffset = new Vector2(0f, 700f);
            yield return CardTestInput.Settle(0.6f);
            var layer = Stack.RimLight.Layer;
            var material = MaterialOf(layer);
            Assert.IsNotNull(material, "the rim light carries its material");
            StringAssert.EndsWith("RimSweep", material.shader.name);
            string family = Stack.Sheets.SheetOf(Stack.RimLight.LitSlot).Family.ColorToken;
            Assert.IsTrue(ThemeController.TryResolverFor(layer, out var light), "precondition: under the theme");
            Assert.IsTrue(light.TryNumber(CardLight.SweepArcToken, out float lightArc), "the theme's arc");
            Assert.AreEqual(Linear(layer, SectionSheetRule.SweepOf(family)), material.GetVector(CardLight.SweepId), "the light's colour is the theme's");
            Assert.AreEqual(1f, material.GetVector(CardLight.SweepId).w, "full strength on light metal");
            Assert.AreEqual(lightArc, material.GetFloat(CardLight.ArcId), "the light mode's arc");
            Assert.AreEqual(CardLight.SweepPeriod, material.GetFloat(CardLight.PeriodId), "one turn per 5 s");

            Color[] a = null, b = null;
            yield return RimRow(c => a = c);
            SetClock(1.25f);
            yield return RimRow(c => b = c);
            float turning = Difference(a, b);
            Debug.Log($"[Card] rim band ({a.Length} samples): a quarter turn {turning:0.000}");
            // - the inner band is LIGHT metal in light mode, so white over it is a modest change per pixel (measured 0.74 summed over the
            //   band); the floor is the still reading under Reduce Motion (0.000, asserted below), far under 0.5
            Assert.Greater(turning, 0.5f, "a quarter turn later the light lit another part of the rim (" + turning + ")");

            float lightAlpha = material.GetVector(CardLight.SweepId).w;
            yield return TapMode(CardOptions.ModeDark);
            Assert.AreSame(material, MaterialOf(Stack.RimLight.Layer), "the same rim light, repainted");
            Assert.IsTrue(ThemeController.TryResolverFor(layer, out var dark), "precondition: under the theme");
            Assert.IsTrue(dark.TryNumber(CardLight.SweepArcToken, out float darkArc), "the dark arc");
            var darkSweep = material.GetVector(CardLight.SweepId);
            Assert.AreEqual(Linear(layer, SectionSheetRule.SweepOf(family)), darkSweep, "the family's hue on dark metal");
            Assert.That(darkSweep.w, Is.InRange(0.35f * lightAlpha, 0.55f * lightAlpha), "about half strength on dark metal");
            Assert.Less(material.GetFloat(CardLight.ArcId), lightArc, "narrower on dark metal");
            Assert.AreEqual(darkArc, material.GetFloat(CardLight.ArcId));

            yield return Push(c => c.card_settings.container.reduce_motion = true);
            Stack.Scroll.scrollOffset = new Vector2(0f, 700f);
            yield return CardTestInput.Settle(0.6f);
            Assert.AreEqual(0f, MaterialOf(Stack.RimLight.Layer).GetFloat(CardLight.AnimateId), "Reduce Motion: _Animate 0");
            SetClock(0f);
            yield return RimRow(c => a = c);
            SetClock(1.25f);
            yield return RimRow(c => b = c);
            Assert.Less(Difference(a, b), 0.02f, "under Reduce Motion the rim light stands still");
        }

        // Points along the MIDDLE of the lit block's metal band on its four sides (panel units), past the rounded corners
        private static List<Vector2> BandPoints(VisualElement slot, float band)
        {
            var bound = slot.worldBound;
            float line = slot.resolvedStyle.borderTopWidth, inset = line + band / 2f, corner = slot.resolvedStyle.borderTopLeftRadius + 4f;
            var points = new List<Vector2>();
            for (float x = bound.xMin + corner; x < bound.xMax - corner; x += 3f)
            {
                points.Add(new Vector2(x, bound.yMin + inset));
                points.Add(new Vector2(x, bound.yMax - inset));
            }
            for (float y = bound.yMin + corner; y < bound.yMax - corner; y += 3f)
            {
                points.Add(new Vector2(bound.xMin + inset, y));
                points.Add(new Vector2(bound.xMax - inset, y));
            }
            return points;
        }

        // The luminance (0..1, sRGB) of each point as drawn now
        private static IEnumerator Luminances(VisualElement under, List<Vector2> points, System.Action<float[]> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var origin = RuntimePanelUtils.ScreenToPanel(under.panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(under.panel, Vector2.right).x - origin.x;
                var values = new float[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    var screen = (points[i] - origin) / unit;
                    var c = tex.GetPixel(Mathf.RoundToInt(screen.x), tex.height - 1 - Mathf.RoundToInt(screen.y));
                    values[i] = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
                }
                got(values);
            }
            finally { Object.Destroy(tex); }
        }

        // _3.3 S4a: the light rides the rim's whole metal band (3 pt on light metal, the band the theme gives the mode) and is clearly visible
        // travelling round the block being read -- measured as each band point's brightness swing over one turn of the clock (12 frames);
        // dark metal keeps its quieter light (a narrower band, a smaller swing)
        [UnityTest]
        public IEnumerator TheRimLight_RidesTheWholeMetalBand_AndIsClearlyVisible_TravellingRoundTheBlock()
        {
            yield return OpenFull("lamp");
            Stack.Scroll.scrollOffset = new Vector2(0f, 700f);
            yield return CardTestInput.Settle(0.6f);
            var report = new List<string>();
            var swings = new Dictionary<string, (float Max, float Share)>();
            foreach (string mode in new[] { CardOptions.ModeLight, CardOptions.ModeDark })
            {
                if (mode == CardOptions.ModeDark)
                {
                    yield return TapMode(mode);
                    Stack.Scroll.scrollOffset = new Vector2(0f, 700f);
                    yield return CardTestInput.Settle(0.6f);
                }
                var slot = Stack.RimLight.LitSlot;
                Assert.IsNotNull(slot, "precondition: a block is lit");
                var rim = slot.Q(className: BlockFrame.RimClass);
                float band = rim.resolvedStyle.borderTopWidth, line = slot.resolvedStyle.borderTopWidth;
                Assert.IsTrue(ThemeController.TryResolverFor(slot, out var theme), "precondition: under the theme");
                Assert.AreEqual(mode == CardOptions.ModeLight ? 3f : 1f, band, 0.01f, "the metal band the theme gives " + mode + " (--ts-rim-band)");
                Assert.AreEqual(Mathf.Max(2f * line, band), Stack.Light.SweepBand(Stack.RimLight.Layer), 0.01f, "the light rides that band (never under two lines)");
                var points = BandPoints(slot, Mathf.Max(2f * line, band));
                var low = new float[points.Count];
                var high = new float[points.Count];
                for (int i = 0; i < points.Count; i++) { low[i] = float.MaxValue; high[i] = float.MinValue; }
                for (int frame = 0; frame < 12; frame++)
                {
                    SetClock(frame * CardLight.SweepPeriod / 12f);
                    float[] values = null;
                    yield return Luminances(slot, points, v => values = v);
                    for (int i = 0; i < points.Count; i++) { low[i] = Mathf.Min(low[i], values[i]); high[i] = Mathf.Max(high[i], values[i]); }
                }
                float max = 0f;
                int seen = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    float swing = high[i] - low[i];
                    max = Mathf.Max(max, swing);
                    if (swing >= 40f / 255f) seen++;
                }
                float share = (float)seen / points.Count;
                swings[mode] = (max, share);
                report.Add($"{mode}: band {band} pt (light rides {Mathf.Max(2f * line, band)}), {points.Count} band points over one turn: largest swing {max * 255f:0} / 255, {share:P0} of the band swing >= 40 / 255");
            }
            System.IO.File.WriteAllText(TestEvidence.PathFor("Light", "band_swing.txt"), string.Join("\n", report) + "\n");
            Debug.Log("[Card] " + string.Join(" | ", report));
            Assert.GreaterOrEqual(swings[CardOptions.ModeLight].Max, 100f / 255f, "light: the light brightens the band clearly where it passes (" + report[0] + ")");
            Assert.GreaterOrEqual(swings[CardOptions.ModeLight].Share, 0.3f, "light: it is seen travelling round most of the band, not one spot (" + report[0] + ")");
            Assert.Less(swings[CardOptions.ModeDark].Max, swings[CardOptions.ModeLight].Max, "dark keeps its quieter light (" + report[1] + ")");
        }

        // Evidence for the visual check: the first sheet's glass and the lit block's rim every 0.5 s of clock over 6 s, light and dark, a
        // contact sheet per sequence; the reduce-motion still frames; the light moving to the next block on scroll
        [UnityTest]
        public IEnumerator Evidence_FrameSequences()
        {
            yield return OpenFull("lamp");
            foreach (string mode in new[] { CardOptions.ModeLight, CardOptions.ModeDark })
            {
                if (mode == CardOptions.ModeDark) yield return TapMode(mode);
                SectionSheets.Sheet sheet = null;
                yield return ShowFirstSheet(s => sheet = s);
                yield return Sequence("glass_" + mode, Stack.Scroll.contentViewport);
                Stack.Scroll.scrollOffset = new Vector2(0f, 700f);
                yield return CardTestInput.Settle(0.6f);
                yield return Sequence("sweep_" + mode, Stack.RimLight.LitSlot);
            }
            yield return Push(c => c.card_settings.container.reduce_motion = true);
            SectionSheets.Sheet first = null;
            yield return ShowFirstSheet(s => first = s);
            yield return Sequence("reduced_glass_dark", Stack.Scroll.contentViewport, frames: 3);
            // - the move on scroll: before, the fade, after
            var frames = new List<Texture2D>();
            Stack.Scroll.scrollOffset = new Vector2(0f, 700f);
            yield return CardTestInput.Settle(0.6f);
            yield return Shot(Stack.Scroll.contentViewport, t => frames.Add(t));
            Stack.Scroll.scrollOffset += new Vector2(0f, Stack.RimLight.LitSlot.layout.height);
            for (int i = 0; i < 4; i++)
            {
                yield return CardTestInput.Settle(0.08f);
                yield return Shot(Stack.Scroll.contentViewport, t => frames.Add(t));
            }
            yield return CardTestInput.Settle(0.6f);
            yield return Shot(Stack.Scroll.contentViewport, t => frames.Add(t));
            SaveContactSheet("move_on_scroll", frames);
        }

        private IEnumerator Sequence(string name, VisualElement crop, int frames = 13)
        {
            var shots = new List<Texture2D>();
            for (int i = 0; i < frames; i++)
            {
                SetClock(i * 0.5f);
                yield return Shot(crop, t => shots.Add(t));
            }
            SaveContactSheet(name, shots);
        }

        private static IEnumerator Shot(VisualElement crop, System.Action<Texture2D> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var panel = crop.panel;
                var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(panel, Vector2.right).x - origin.x;
                var r = crop.worldBound;
                int x = Mathf.Clamp(Mathf.RoundToInt((r.xMin - origin.x) / unit), 0, tex.width - 1);
                int w = Mathf.Clamp(Mathf.RoundToInt(r.width / unit), 1, tex.width - x);
                int top = Mathf.RoundToInt((Mathf.Max(r.yMin, 0f) - origin.y) / unit);
                int h = Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(r.height, 700f) / unit), 1, tex.height - top);
                var cut = new Texture2D(w, h, TextureFormat.RGBA32, false);
                cut.SetPixels(tex.GetPixels(x, tex.height - top - h, w, h));
                cut.Apply();
                got(cut);
            }
            finally { Object.Destroy(tex); }
        }

        // The frames side by side, in rows of up to seven
        private static void SaveContactSheet(string name, List<Texture2D> frames)
        {
            int w = frames.Max(f => f.width), h = frames.Max(f => f.height), across = Mathf.Min(7, frames.Count);
            int rows = (frames.Count + across - 1) / across;
            var sheet = new Texture2D(across * (w + 4), rows * (h + 4), TextureFormat.RGBA32, false);
            sheet.SetPixels32(Enumerable.Repeat(new Color32(40, 40, 40, 255), sheet.width * sheet.height).ToArray());
            for (int i = 0; i < frames.Count; i++)
                sheet.SetPixels((i % across) * (w + 4), (rows - 1 - i / across) * (h + 4) + (h - frames[i].height), frames[i].width, frames[i].height, frames[i].GetPixels());
            sheet.Apply();
            System.IO.File.WriteAllBytes(TestEvidence.PathFor("Light", name + ".png"), sheet.EncodeToPNG());
            Object.Destroy(sheet);
            foreach (var f in frames) Object.Destroy(f);
        }
    }
}
