using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The card's OPEN sections on the REAL scene (_3.3 fourth round, P3; SectionSheets + OpenSectionView over the Look Kit) with the Ribeira
    // Palace's card and real input:
    //   - the default: Blocks Open (no frame, no glass sheet, no opener), one section per Navigation Bar group, each opened by its Start
    //     (Tiles) and its eyebrow (the 28 x 3 rule in the group's colour over its name), its blocks open with a hairline between two
    //   - every option value changes what the visitor sees (one case per value), the pairs that interact (Start x Colour on the render,
    //     Motion x Reduce Motion), the group's Section Style over the card's Start, Cards keeping today's framed look
    //   - the Reveal follows the Start row's place under REAL drags, both ways (Replay Every Pass / Once), Reduce Motion whole throughout
    //   - Tint Depth: a band under the Start fading to the paper at its depth, on the render
    //   - every eyebrow and body text >= 7:1 on the render over every Start at Intensity max, light and dark
    // Renders in TestEvidence/Premium/P3/.
    public class PoiCardSectionsSceneTests : SearchSceneFixture
    {
        private const string Domain = "Premium/P3";
        private const string Painting = "painting";
        private const float Aaa = 7f;

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private ScrollView Scroll => Stack.Scroll;

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // The card's settings pushed live as `edit` sets them over the framework's defaults
        private IEnumerator Push(System.Action<CardSettings> edit)
        {
            var copy = ConfigCopy();
            copy.card_settings.sections = new CardSectionSettings();
            edit?.Invoke(copy.card_settings);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The Ribeira Palace's card at `stop`, scrolled to its top, the hero held still
        private IEnumerator Open(System.Action<CardSettings> edit = null, SheetStopRule.Stop stop = SheetStopRule.Stop.Full)
        {
            CardTestInput.HoldTheWallStill(Sheet);
            yield return Push(edit);
            if (!Sheet.IsOpen || Card.ShownPoiId != Painting) SelectionEventBus.Select(Painting);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "precondition: the Ribeira Palace's card is open");
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle(0.6f);
            Scroll.scrollOffset = Vector2.zero;
            Stack.HeaderView.Clock = () => 0f;
            Stack.HeaderView.Animate();
            yield return CardTestInput.UntilStill(Sheet);
        }

        // The open sections shown now, in scroll order (a bare navigation run is none)
        private List<SectionSheets.Sheet> Sections =>
            Stack.Sheets.All.Where(s => s.Root.ClassListContains(SectionSheets.OpenClass) && s.Root.resolvedStyle.display == DisplayStyle.Flex
                && s.Open.Edge.resolvedStyle.display == DisplayStyle.Flex).ToList();

        // Scroll a section's top just under the floating chrome -- its Start row high in the view, so its Reveal is whole -- the motion held
        // still (a still picture to read)
        private IEnumerator ShowSection(SectionSheets.Sheet section)
        {
            Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(section.Root) - Stack.LandingInset);
            yield return CardTestInput.Settle(0.3f);
            CardTestInput.HoldSectionsStill(Sheet);
            yield return CardTestInput.UntilStill(Sheet);
        }

        private IEnumerator Save(string name)
        {
            Texture2D shot = null;
            yield return ThemePixelSet.GrabStill(name, null, r => shot = r);
            if (shot == null) yield break;
            System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }

        private static Color Theme(VisualElement from, string token)
        {
            Assert.IsTrue(CardTokenColours.TryResolve(from, token, out Color c), "precondition: the theme has " + token);
            return c;
        }

        // The ground's strongest stop is the theme's tint at `amount` over the open sheet, as the tint rule draws it (SectionStyleRule.Tint:
        // v6's mix, EditMode-tested) -- read from the theme by the section's own Colour token
        private static void TintedAt(SectionSheets.Sheet section, float amount, string what)
        {
            var o = section.Open;
            var sheet = Theme(o.Edge, "--ts-sheet-open");
            sheet.a = 1f;
            var tint = Theme(o.Edge, SectionStyleRule.TintToken(o.Style.Colour, section.GroupIndex >= 0 ? section.Group.ColorToken : section.Family?.ColorToken));
            tint.a = 1f;
            var strongest = o.GroundOptions.Stops.OrderByDescending(s => s.Colour.a).First().Colour;
            var expected = SectionStyleRule.Tint(sheet, tint, amount);
            Assert.Less(((Vector4)expected - (Vector4)strongest).magnitude, 0.01f, what + ": expected " + (Color32)expected + ", drawn " + (Color32)strongest);
        }

        private static void AreSame(Color expected, Color actual, string what) =>
            Assert.Less(((Vector4)expected - (Vector4)actual).magnitude, 0.01f, what + ": expected " + (Color32)expected + ", drawn " + (Color32)actual);

        [UnityTest]
        public IEnumerator TheDefault_IsOpen_OneSectionPerBarGroup_EachOpenedByItsTilesAndEyebrow_ItsBlocksOpen()
        {
            yield return Open();
            Assert.IsTrue(Scroll.ClassListContains(BlockStackView.OpenStackClass), "the stack on the dense open ground");
            var slots = Stack.Sheets.All.SelectMany(s => s.Slots).ToList();
            Assert.Greater(slots.Count, 4, "precondition: the Ribeira Palace's card has blocks in its scroll");
            Assert.IsFalse(slots.Any(s => s.ClassListContains(BlockFrame.FramedClass)), "no block framed");
            Assert.IsTrue(slots.All(s => s.ClassListContains(BlockStackView.OpenSlotClass)), "every scroll block open");
            // - the sections are RUNS of one bar group, in the scroll's order: the card's targets walked in order, each read as its bar group
            //   (a family no group holds as itself), neighbours of one group joined -- an authored order that comes back to a group opens it again
            var bar = Sheet.Slots.Bar;
            var sections = Sections;
            var items = NavBarRule.Items(Card.Sheet.Context.Settings.navigation, BlockRegistry.Shared);
            var expected = new List<string>();
            foreach (string target in Sheet.Nav.Model.Targets)
            {
                Assert.IsTrue(Sheet.Nav.TryGetEntry(target, out var entry), "precondition: target " + target + " is bound");
                int group = NavBarRule.ItemOfFamily(items, entry.Definition.Family);
                string key = group >= 0 ? items[group].Key : entry.Definition.Family;
                if (expected.Count == 0 || expected[^1] != key) expected.Add(key);
            }
            // - a section that holds no navigation target (the meta family's Sources, "always last", which the bar does not take you to) is
            //   still drawn; the walk above names the others
            var targetSlots = new HashSet<VisualElement>(Sheet.Nav.Model.Targets.Select(t => Stack.SlotOfBlock(t)));
            var navigated = sections.Where(s => s.Slots.Any(targetSlots.Contains)).ToList();
            Assert.LessOrEqual(sections.Count - navigated.Count, 1, "precondition: at most the meta run holds no target (" + string.Join(", ", sections.Except(navigated).Select(s => s.Run.Family)) + ")");
            CollectionAssert.AreEqual(expected, navigated.Select(s => s.GroupIndex >= 0 ? s.Group.Key : s.Run.Family), "one section per run of a group, in the scroll's order");
            CollectionAssert.AreEquivalent(bar.Entries.Select(e => e.Item.Key), sections.Where(s => s.GroupIndex >= 0).Select(s => s.Group.Key).Distinct(),
                "every group the bar shows opens a section, and no other");
            for (int i = 0; i < sections.Count; i++)
            {
                var o = sections[i].Open;
                string what = "section " + i + " (" + sections[i].Group.Key + ")";
                if (sections[i].GroupIndex < 0) continue;
                Assert.AreEqual(CardOptions.SectionStartTiles, o.Style.Start, what + ": v6's default Start");
                Assert.AreEqual(DisplayStyle.Flex, o.StartTiles.resolvedStyle.display, what + ": its tile row drawn");
                Assert.AreEqual(24f, o.StartTiles.layout.height, 0.5f, what + ": 24 pt tiles");
                Assert.AreEqual(Sheet.Root.layout.width, o.StartTiles.layout.width, 1f, what + ": edge to edge");
                Assert.AreEqual(24f, o.StartTiles.resolvedStyle.borderTopLeftRadius, 0.5f, what + ": rounded top corners 24");
                Assert.AreEqual(DisplayStyle.None, sections[i].Opener.Root.resolvedStyle.display, what + ": no group pill");
                Assert.AreEqual(DisplayStyle.None, o.Divider.resolvedStyle.display, what + ": Divider None");
                AreSame(Theme(o.Edge, sections[i].Group.ColorToken), o.StartTileOptions.Colour, what + ": tiles in the group's colour");
                AreSame(Theme(o.Edge, sections[i].Group.ColorToken), o.Rule.resolvedStyle.backgroundColor, what + ": the 28 x 3 rule in it");
                Assert.AreEqual(28f, o.Rule.layout.width, 0.5f);
                Assert.AreEqual(3f, o.Rule.layout.height, 0.5f);
                AreSame(Theme(o.Edge, SectionStyleRule.TextOf(sections[i].Group.ColorToken)), o.Name.resolvedStyle.color, what + ": the name in the colour's words");
                int barIndex = bar.Entries.ToList().FindIndex(e => e.Item.Key == sections[i].Group.Key);
                Assert.AreEqual(bar.Items[barIndex].Label.text.ToUpperInvariant(), o.Name.text, what + ": the group's name, the bar's own words in capitals");
                TintedAt(sections[i], 0.12f, what + ": the fade at Intensity 0.12 where it begins");
                Assert.AreEqual(CardSectionSettings.TintDepthDefault, o.GroundOptions.Stops.Last().Offset - o.GroundOptions.Stops.First().Offset, 0.5f,
                    what + ": over Tint Depth's 120");
                // - blocks inside a section: apart by space and a hairline
                var shown = sections[i].Slots.Where(s => s.resolvedStyle.display == DisplayStyle.Flex).ToList();
                for (int b = 1; b < shown.Count; b++)
                    Assert.AreEqual(1f, shown[b].resolvedStyle.borderTopWidth, 0.01f, what + " block " + b + ": a hairline above it");
                if (shown.Count > 0) Assert.AreEqual(0f, shown[0].resolvedStyle.borderTopWidth, 0.01f, what + ": none above the first");
            }
            // - PART 4: the body inside an open section is the theme's open size (17), the lede its own
            var paragraphs = slots.SelectMany(s => s.Query<Label>(className: "card-text__paragraph").ToList())
                .Where(l => !l.ClassListContains("card-text__lede") && l.resolvedStyle.display == DisplayStyle.Flex).ToList();
            var prose = paragraphs.Where(l => !l.ClassListContains("ts-el__paragraph")).ToList();
            Assert.Greater(prose.Count, 2, "precondition: the card's blocks draw body paragraphs");
            float open = CardTestInput.TokenPx("--ts-size-body-open");
            Assert.AreEqual(17f, open, "the theme's open body size");
            foreach (var p in prose) Assert.AreEqual(open, p.resolvedStyle.fontSize, 0.01f, "body '" + p.text.Substring(0, Mathf.Min(24, p.text.Length)) + "' at the open size");
            // - a composed block's paragraph or lede keeps its row's Size option (Components.uss's sizes), never the open body size over it
            string components = System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Components/Components.uss");
            int composed = 0;
            foreach (var p in paragraphs.Where(l => l.ClassListContains("ts-el__paragraph")))
            {
                VisualElement element = p.parent;
                while (element != null && !element.ClassListContains("ts-el")) element = element.parent;
                Assert.IsNotNull(element, "precondition: a composed paragraph sits in its element");
                string kind = element.ClassListContains("ts-el-lede") ? "lede" : element.ClassListContains("ts-el-paragraph") ? "body" : null;
                string size = new[] { "s", "m", "l" }.FirstOrDefault(k => element.ClassListContains("ts-el--size-" + k));
                if (kind == null || size == null) continue;
                // - the size's own value: a px number, or the theme's token it names
                var m = System.Text.RegularExpressions.Regex.Match(components, "--ts-el-" + kind + "-" + size + @":\s*(?:([0-9.]+)px|var\((--[a-z0-9-]+)\))");
                Assert.IsTrue(m.Success, "precondition: Components.uss sizes --ts-el-" + kind + "-" + size);
                float want = m.Groups[1].Success ? float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : CardTestInput.TokenPx(m.Groups[2].Value);
                Assert.AreEqual(want, p.resolvedStyle.fontSize, 0.01f, "the story's " + kind + " (size " + size + ") keeps its own size");
                composed++;
            }
            Assert.Greater(composed, 0, "precondition: the Ribeira Palace's story preview draws composed text in an open section");
            yield return ShowSection(sections[1]);
            yield return Save("default_full_light");
        }

        private static IEnumerable<string> OptionValues()
        {
            foreach (string v in CardOptions.SectionStarts) yield return "start=" + v;
            foreach (string v in CardOptions.SectionDividers) yield return "divider=" + v;
            foreach (string v in CardOptions.SectionColours) yield return "colour=" + v;
            foreach (string v in CardOptions.SectionMotions) yield return "motion=" + v;
            foreach (string v in CardOptions.SectionVaries) yield return "vary=" + v;
            foreach (string v in CardOptions.Blocks) yield return "blocks=" + v;
        }

        // One case per option value: the value is what the running card's sections draw, read off the card and rendered (light and dark)
        [UnityTest]
        public IEnumerator EveryOptionValue_ChangesWhatTheSectionsDraw([ValueSource(nameof(OptionValues))] string option)
        {
            string key = option.Split('=')[0], value = option.Split('=')[1];
            yield return Open(c =>
            {
                var s = c.sections;
                if (key == "start") s.start = value;
                // - a divider stands only before a start that is no ornament of its own (one ornament per boundary): read on a Fade start
                if (key == "divider") { s.divider = value; s.divider_image = "images/paco_gallery_1"; s.start = CardOptions.SectionStartFade; }
                if (key == "colour") s.colour = value;
                if (key == "motion") s.motion = value;
                if (key == "vary") s.vary = value;
                if (key == "blocks") c.container.blocks = value;
            });
            if (key == "blocks" && value == CardOptions.BlocksCards)
            {
                Assert.IsFalse(Scroll.ClassListContains(BlockStackView.OpenStackClass), "Cards: the glass ground");
                Assert.IsTrue(Stack.Sheets.All.SelectMany(s => s.Slots).All(s => s.ClassListContains(BlockFrame.FramedClass)), "Cards: every block framed");
                Assert.IsTrue(Stack.Sheets.All.Any(s => s.Opener.Root.resolvedStyle.display == DisplayStyle.Flex), "Cards: the family pills");
                Assert.IsFalse(Stack.Sheets.All.Any(s => s.Open.Edge.resolvedStyle.display == DisplayStyle.Flex), "Cards: no open part draws");
                var body = Stack.Sheets.All.SelectMany(s => s.Slots).SelectMany(s => s.Query<Label>(className: "card-text__paragraph").ToList())
                    .FirstOrDefault(l => !l.ClassListContains("card-text__lede") && !l.ClassListContains("ts-el__paragraph") && l.resolvedStyle.display == DisplayStyle.Flex);
                Assert.IsNotNull(body, "precondition: a body paragraph");
                Assert.AreEqual(CardTestInput.TokenPx("--ts-size-body"), body.resolvedStyle.fontSize, 0.01f, "Cards: the body keeps the theme's body size");
                yield return ShowSectionAt(1);
                yield return Renders(option);
                yield break;
            }
            var sections = Sections;
            Assert.Greater(sections.Count, 2, "precondition: three sections at least");
            var second = sections[1].Open;
            switch (key)
            {
                case "start":
                    Assert.AreEqual(value, second.Style.Start);
                    Assert.AreEqual(value == CardOptions.SectionStartTiles, second.StartTiles.resolvedStyle.display == DisplayStyle.Flex, "the tile row only under Tiles");
                    Assert.AreEqual(value == CardOptions.SectionStartWave, second.StartWave.resolvedStyle.display == DisplayStyle.Flex, "the wave only under Wave");
                    float top = second.GroundOptions.Stops.Count > 0 ? second.GroundOptions.Stops.Max(s => s.Colour.a) : 0f;
                    if (value == CardOptions.SectionStartPlain) Assert.AreEqual(0f, top, "Plain: no tint");
                    else if (value == CardOptions.SectionStartBands)
                    {
                        TintedAt(sections[1], 0.12f * SectionStyleRule.BandsShare, "Bands: the second section flat at 0.7 of it");
                        Assert.AreEqual(0f, sections[2].Open.GroundOptions.Stops.Max(s => s.Colour.a), 0.001f, "...the third untinted");
                    }
                    else TintedAt(sections[1], 0.12f, value + ": the fade at the intensity where it begins");
                    break;
                case "divider":
                    Assert.AreEqual(DisplayStyle.None, sections[0].Open.Divider.resolvedStyle.display, "never before the first section");
                    bool drawn = value != CardOptions.SectionDividerNone;
                    Assert.AreEqual(drawn, second.Divider.resolvedStyle.display == DisplayStyle.Flex, value + ": the divider before the second");
                    if (value == CardOptions.SectionDividerHairline) Assert.AreEqual(1f, second.Divider.layout.height, 0.5f, "a 1 pt line");
                    if (value == CardOptions.SectionDividerOrnament)
                    {
                        Assert.AreEqual(22f, second.DividerTiles.layout.width, 0.5f, "one 22 pt tile");
                        Assert.AreEqual(DisplayStyle.Flex, second.OrnamentLeft.resolvedStyle.display, "between two lines");
                    }
                    if (value == CardOptions.SectionDividerAzulejo) Assert.AreEqual(second.Edge.layout.width, second.DividerTiles.layout.width, 1f, "a strip of tiles across");
                    if (value == CardOptions.SectionDividerMosaic || value == CardOptions.SectionDividerImage)
                        Assert.IsNotNull(second.DividerStrip.resolvedStyle.backgroundImage.texture, value + ": the strip's picture repeated");
                    // - one ornament per boundary: the divider stands before a plain start (Fade here), which draws no row of its own
                    Assert.AreEqual(DisplayStyle.None, second.StartTiles.resolvedStyle.display, value + ": no tile row under the divider");
                    break;
                case "colour":
                    string token = SectionStyleRule.TintToken(value, sections[1].Group.ColorToken);
                    AreSame(Theme(second.Edge, token), second.StartTileOptions.Colour, value + ": the tiles in " + token);
                    AreSame(Theme(second.Edge, sections[1].Group.ColorToken), second.Rule.resolvedStyle.backgroundColor, "the rule keeps the group's colour");
                    break;
                case "motion":
                    Assert.AreEqual(value, second.Style.Motion);
                    break;
                case "vary":
                    // - By Group walks the bar's groups (a run of a family no group holds has no place in that walk)
                    if (value == CardOptions.SectionVaryByGroup)
                        foreach (var s in sections.Where(s => s.GroupIndex >= 0))
                            Assert.AreEqual(SectionStyleRule.ByGroupStarts[s.GroupIndex % 3], s.Open.Style.Start, s.Group.Key + ": v6's walk");
                    else Assert.IsTrue(sections.All(s => s.Open.Style.Start == CardOptions.SectionStartTiles), "Same: every section Tiles");
                    break;
                case "blocks":
                    Assert.IsTrue(Scroll.ClassListContains(BlockStackView.OpenStackClass));
                    break;
            }
            yield return ShowSection(sections[1]);
            yield return Renders(option);
        }

        private IEnumerator ShowSectionAt(int index)
        {
            var sheets = Stack.Sheets.All.Where(s => s.Root.resolvedStyle.display == DisplayStyle.Flex).ToList();
            Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(sheets[Mathf.Min(index, sheets.Count - 1)].Root) - Stack.LandingInset);
            yield return CardTestInput.Settle(0.3f);
            yield return CardTestInput.UntilStill(Sheet);
        }

        // The option's renders: light and dark at Full (Starts and Dividers also at Half, scrolled to the same section)
        private IEnumerator Renders(string option)
        {
            string name = option.Replace('=', '_');
            float offset = Scroll.scrollOffset.y;
            foreach (string mode in CardOptions.Modes)
            {
                ThemeController.SetMode(mode);
                yield return CardTestInput.Settle(0.2f);
                yield return Save(name + "_full_" + mode);
                if (option.StartsWith("start") || option.StartsWith("divider"))
                {
                    Sheet.SetStop(SheetStopRule.Stop.Half);
                    yield return CardTestInput.Settle(0.6f);
                    Scroll.scrollOffset = new Vector2(0f, offset);
                    yield return CardTestInput.UntilStill(Sheet);
                    yield return Save(name + "_half_" + mode);
                    Sheet.SetStop(SheetStopRule.Stop.Full);
                    yield return CardTestInput.Settle(0.6f);
                    Scroll.scrollOffset = new Vector2(0f, offset);
                    yield return CardTestInput.UntilStill(Sheet);
                }
            }
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return CardTestInput.Settle(0.2f);
        }

        // Start x Colour on the render: the section's ground, just under its start, takes each colour's tint -- three different pixels for
        // the three colours, and none at all under Plain
        [UnityTest]
        public IEnumerator StartTimesColour_TheGroundUnderTheStart_IsTintedByEachColour_OnTheRender()
        {
            foreach (string start in new[] { CardOptions.SectionStartFade, CardOptions.SectionStartWave, CardOptions.SectionStartTiles, CardOptions.SectionStartPlain })
            {
                var drawn = new Dictionary<string, Color>();
                foreach (string colour in CardOptions.SectionColours)
                {
                    yield return Open(c => { c.sections.start = start; c.sections.colour = colour; c.sections.intensity = CardSectionSettings.IntensityMax; });
                    var section = Sections[1];
                    yield return ShowSection(section);
                    // - the section's top a third down the view: its start and the tint under it in sight, clear of the chrome over the top
                    Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(section.Root) - Scroll.contentViewport.layout.height / 3f);
                    yield return CardTestInput.UntilStill(Sheet);
                    Texture2D shot = null;
                    yield return ThemePixelSet.GrabStill($"ground_{start}_{colour}", null, r => shot = r);
                    // - 8 pt under the start (the edge: its divider and shape), where the tint is at its strongest, at the left gutter (no word there)
                    var edge = ThemePixelSet.ScreenRectOf(section.Open.Edge, shot);
                    var ground = ThemePixelSet.ScreenRectOf(section.Open.Ground, shot);
                    float scale = 1f / ThemePixelSet.PanelUnitsPerPixel(section.Open.Ground);
                    int y = (section.Open.Edge.layout.height > 0f ? edge.yMin : ground.yMax) - Mathf.RoundToInt(8f * scale);
                    Assert.Greater(y, ground.yMin, "precondition: the pixel read lies on the section's ground, on screen");
                    var pixel = shot.GetPixel(Mathf.RoundToInt(ground.xMin + 6f * scale), y);
                    drawn[colour] = pixel;
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"start_x_colour_{start}_{colour}.png"), shot.EncodeToPNG());
                    Object.Destroy(shot);
                }
                float Spread(Color a, Color b) => ((Vector4)a - (Vector4)b).magnitude;
                if (start == CardOptions.SectionStartPlain)
                    Assert.Less(Spread(drawn["group"], drawn["neutral"]) + Spread(drawn["group"], drawn["accent"]), 0.01f, "Plain: no tint, the same ground whatever the colour");
                else
                {
                    Assert.Greater(Spread(drawn["group"], drawn["accent"]), 0.01f, start + ": Group and Accent tint the ground differently " + (Color32)drawn["group"] + " / " + (Color32)drawn["accent"]);
                    Assert.Greater(Spread(drawn["group"], drawn["neutral"]), 0.01f, start + ": Group and Neutral differ");
                }
            }
        }

        // How many of a section's Start tiles have turned at least half way now, and whether every one is whole (turned and at alpha 1)
        private static int TurnedTiles(SectionSheets.Sheet section, int count) =>
            Enumerable.Range(0, count).Count(i => (section.Open.StartTileOptions.ScaleOf?.Invoke(i) ?? 1f) >= 0.5f);
        private static bool AllWhole(SectionSheets.Sheet section, int count) =>
            Enumerable.Range(0, count).All(i => Mathf.Abs((section.Open.StartTileOptions.ScaleOf?.Invoke(i) ?? 1f) - 1f) < 1e-3f
                && Mathf.Abs((section.Open.StartTileOptions.AlphaOf?.Invoke(i) ?? 1f) - 1f) < 1e-3f);

        // The Reveal's progress the row's place gives, measured on the screen and NOT asked of the card: 0 as the Start row's bottom clears
        // the Navigation Bar's top (its band, as drawn), 1 once it is a third of the way up the visible card (the stack's viewport)
        private float MeasuredProgress(SectionSheets.Sheet section)
        {
            float rowBottom = section.Open.Edge.worldBound.yMax;
            float barTop = Sheet.Slots.Bar.Row.worldBound.yMin;
            var view = Scroll.contentViewport.worldBound;
            float third = view.yMax - view.height / 3f;
            return Mathf.Clamp01((barTop - rowBottom) / (barTop - third));
        }

        // P3d PART 1 (the P3c review c, second pass b) with REAL finger strokes: the Start row's tiles turn in order with its place -- none
        // while it is under the Navigation Bar, about half of them half way up its travel, all of them a third of the way up the card; real
        // strokes back down past the bar turn them out again (Replay Every Pass) or leave them whole (Once); Reduce Motion whole throughout.
        // Renders of the half-way row in TestEvidence/Premium/P3d/
        [UnityTest]
        public IEnumerator RealDrags_TheStartRowsTilesFollowItsPlace_BothWays_EveryPassOrOnce_ReduceMotionWholeThroughout(
            [Values(CardOptions.SectionReplayEveryPass, CardOptions.SectionReplayOnce, "reduce")] string pass)
        {
            bool reduce = pass == "reduce", once = pass == CardOptions.SectionReplayOnce;
            yield return Open(c =>
            {
                c.container.reduce_motion = reduce;
                if (once) c.sections.replay = CardOptions.SectionReplayOnce;
            });
            var bar = Sheet.Slots.Bar;
            Assert.IsNotNull(bar.Root.parent, "precondition: the default Navigation Bar floats at the bottom");
            var section = Sections[1];
            int count = LookKit.TileRule.CountFor(section.Open.StartTilesRow.contentRect.width, section.Open.StartTileOptions.Size, section.Open.StartTileOptions.Joint);
            Assert.AreEqual(16, count, "precondition: 16 tiles across the phone's 390");
            Assert.Greater(section.Open.Edge.worldBound.yMax, bar.Row.worldBound.yMin, "precondition: at the card's top the row is under the bar's top");
            Assert.AreEqual(reduce ? count : 0, TurnedTiles(section, count), reduce ? "Reduce Motion: whole before it moved" : "under the bar: no tile turned");

            // - small real strokes up the card (each moves the stack ~1.7x the finger: about a fifth of the row's travel), the reading taken
            //   at rest after each, until the row is past a third of the way up
            var readings = new List<(float P, int Turned)>();
            float halfP = float.NaN;
            int halfTurned = -1;
            for (int stroke = 0; stroke < 60 && MeasuredProgress(section) < 1f; stroke++)
            {
                var reach = CardTestInput.ReachableStack(Sheet);
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(reach.center.x, reach.center.y), new Vector2(0f, -22f));
                yield return CardTestInput.UntilStill(Sheet);
                float p = MeasuredProgress(section);
                int turned = TurnedTiles(section, count);
                readings.Add((p, turned));
                if (reduce) Assert.IsTrue(AllWhole(section, count), "Reduce Motion: whole throughout (at " + p.ToString("0.00") + ")");
                if (!reduce && p > 0.3f && p < 0.7f && (float.IsNaN(halfP) || Mathf.Abs(p - 0.5f) < Mathf.Abs(halfP - 0.5f)))
                {
                    halfP = p;
                    halfTurned = turned;
                    yield return SaveRow(section, pass + "_half");
                }
            }
            string log = string.Join(", ", readings.Select(r => r.P.ToString("0.00") + ":" + r.Turned));
            Assert.GreaterOrEqual(MeasuredProgress(section), 1f, "precondition: real strokes carried the row a third of the way up (" + log + ")");
            if (!reduce)
            {
                Assert.IsFalse(float.IsNaN(halfP), "precondition: a rest half way up the travel (0.3 .. 0.7): " + log);
                Assert.AreEqual(count * halfP, halfTurned, 3f, "half way up (" + halfP.ToString("0.00") + "): about half the tiles turned -- " + log);
                // - in order with the place: more of the travel never turns fewer tiles
                for (int i = 1; i < readings.Count; i++)
                    if (readings[i].P >= readings[i - 1].P) Assert.GreaterOrEqual(readings[i].Turned, readings[i - 1].Turned, "the tiles follow the place: " + log);
            }
            Assert.IsTrue(AllWhole(section, count), "a third of the way up: every tile whole");

            // - real strokes back DOWN the card until the row is under the bar's top again
            for (int stroke = 0; stroke < 60 && MeasuredProgress(section) > 0f; stroke++)
            {
                var reach = CardTestInput.ReachableStack(Sheet);
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(reach.center.x, reach.yMin + 20f), new Vector2(0f, 60f));
                yield return CardTestInput.UntilStill(Sheet);
                if (reduce) Assert.IsTrue(AllWhole(section, count), "Reduce Motion: whole on the way back too");
            }
            Assert.AreEqual(0f, MeasuredProgress(section), "precondition: real strokes brought the row back under the bar");
            if (reduce || once) Assert.IsTrue(AllWhole(section, count), (once ? "Once" : "Reduce Motion") + ": still whole under the bar");
            else Assert.AreEqual(0, TurnedTiles(section, count), "Every Pass: back under the bar, the tiles turned out");
        }

        // The Start row as drawn now, saved for the look
        private IEnumerator SaveRow(SectionSheets.Sheet section, string name)
        {
            CardTestInput.HoldSectionsStill(Sheet);
            Texture2D shot = null;
            yield return ThemePixelSet.GrabStill("reveal_" + name, null, r => shot = r);
            if (shot == null) yield break;
            System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3d", "reveal_" + name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }

        // A revealed Start row shows every tile on the render; Drift slides the divider, the Start rests; Reduce Motion holds both still. One
        // ornament per boundary: the Tiles Start is read on the default card (no divider before it), the sliding divider before a Fade start
        [UnityTest]
        public IEnumerator ARevealedRow_DrawsEveryTile_DriftSlidesTheDividerUnlessReduced_TheStartRests()
        {
            foreach (bool reduce in new[] { false, true })
            {
                yield return Open(c => c.container.reduce_motion = reduce);
                var row = Sections[1];
                yield return ShowSection(row);
                yield return EveryTileOfTheStartRowIsDrawnWhole(row, reduce ? "reduced" : "revealed");
            }
            foreach (bool reduce in new[] { false, true })
            {
                yield return Open(c => { c.sections.motion = CardOptions.SectionMotionDrift; c.sections.divider = CardOptions.SectionDividerAzulejo; c.container.reduce_motion = reduce; });
                var section = Sections[1];
                Assert.AreEqual(DisplayStyle.None, section.Open.Divider.resolvedStyle.display, "a Tiles start stands alone: no divider before it");
                double now = 0;
                foreach (var s in Sections) s.Open.Clock = () => now;
                Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(section.Root) - Stack.LandingInset);
                yield return CardTestInput.Settle(0.3f);
                now = 1.3;
                for (int f = 0; f < 3; f++) yield return null;
                Assert.AreEqual(0f, section.Open.StartTileOptions.Offset, "the Start never slides: it rests");
                Assert.IsTrue(AllWhole(section, 16), "...and is whole up there");

                yield return Open(c => { c.sections.motion = CardOptions.SectionMotionDrift; c.sections.divider = CardOptions.SectionDividerAzulejo; c.sections.start = CardOptions.SectionStartFade; c.container.reduce_motion = reduce; });
                section = Sections[1];
                Assert.AreEqual(DisplayStyle.Flex, section.Open.Divider.resolvedStyle.display, "precondition: before a Fade start the divider is drawn");
                now = 0;
                // - the push re-showed the sections while the card still held the pass before's scroll (this row in view): its drift began on
                //   that pass's clock. Stopped here, it starts again on the test's clock the moment the row shows below
                foreach (var s in Sections)
                {
                    s.Open.Clock = () => now;
                    s.Open.Stop();
                }
                Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(section.Root) - Stack.LandingInset);
                yield return CardTestInput.Settle(0.3f);
                Assert.AreEqual(!reduce, section.Open.Drifting, reduce ? "Reduce Motion: no drift" : "Drift runs once the row shows");
                now = 1.3;
                for (int f = 0; f < 3; f++) yield return null;
                if (reduce) Assert.AreEqual(0f, section.Open.DividerTileOptions.Offset, "Reduce Motion: the divider holds still");
                else Assert.AreEqual(13f, section.Open.DividerTileOptions.Offset, 0.5f, "Drift: the divider's tiles slide 10 pt a second");
            }
        }

        // P3d PART 2 (the P3c review d): the tint is a band under the Start that fades to the paper at Tint Depth -- read on the RENDER, the
        // same pixel at the same scroll in renders that differ ONLY in the tint (the blocks over the ground identical, so they cancel out):
        //   - right under the Start the tint is there (the default depth at Intensity max vs min)
        //   - the default 120 still tints at 60 where a 40 depth is the paper; at 130 both are the paper
        //   - 340 still tints at 160 where 120 is the paper; at 380 both are the paper
        // Light and dark (TestEvidence/Premium/P3d/)
        [UnityTest]
        public IEnumerator TheTint_IsABandUnderTheStart_FadingToThePaperAtTintDepth_OnTheRender([Values("light", "dark")] string mode)
        {
            var shots = new Dictionary<string, Texture2D>();
            RectInt ground = default;
            float px = 1f, groundTop = 0f;
            try
            {
                foreach (var (name, depth, intensity) in new[]
                {
                    ("min", CardSectionSettings.TintDepthMin, CardSectionSettings.IntensityMax),
                    ("default", CardSectionSettings.TintDepthDefault, CardSectionSettings.IntensityMax),
                    ("max", CardSectionSettings.TintDepthMax, CardSectionSettings.IntensityMax),
                    ("faint", CardSectionSettings.TintDepthDefault, CardSectionSettings.IntensityMin),
                })
                {
                    yield return Open(c => { c.sections.tint_depth = depth; c.sections.intensity = intensity; });
                    ThemeController.SetMode(mode);
                    yield return CardTestInput.Settle(0.2f);
                    var section = Sections[1];
                    yield return ShowSection(section);
                    Assert.AreEqual(depth, section.Open.Style.TintDepth, "precondition: the card reads Tint Depth " + depth);
                    Texture2D shot = null;
                    yield return ThemePixelSet.GrabStill($"tint_{name}_{mode}", null, r => shot = r);
                    Assert.IsNotNull(shot, "precondition: a render");
                    shots[name] = shot;
                    ground = ThemePixelSet.ScreenRectOf(section.Open.Ground, shot);
                    px = 1f / ThemePixelSet.PanelUnitsPerPixel(section.Open.Ground);
                    groundTop = section.Open.Edge.layout.height;
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3d", $"tint_{name}_{mode}.png"), shot.EncodeToPNG());
                }
                // - the right gutter (no words there), `y` down the ground from under the Start
                Color At(string shot, float y) => shots[shot].GetPixel(Mathf.RoundToInt(ground.xMax - 8f * px), Mathf.RoundToInt(ground.yMax - (groundTop + y) * px));
                float Diff(string a, string b, float y) => ((Vector4)At(a, y) - (Vector4)At(b, y)).magnitude;
                string Pair(string a, string b, float y) => " (" + (Color32)At(a, y) + " / " + (Color32)At(b, y) + " at " + y + ")";
                const float Same = 2.5f / 255f, Tinted = 4f / 255f;
                Assert.Greater(Diff("default", "faint", 4f), Tinted, "right under the Start the tint is there" + Pair("default", "faint", 4f));
                Assert.Greater(Diff("default", "min", 60f), Tinted, "120 still tints at 60, where 40 is the paper" + Pair("default", "min", 60f));
                Assert.Less(Diff("default", "min", 130f), Same, "at 130 both are the paper: 120's tint ended at its depth" + Pair("default", "min", 130f));
                Assert.Greater(Diff("max", "default", 160f), Tinted, "340 still tints at 160, where 120 is the paper" + Pair("max", "default", 160f));
                Assert.Less(Diff("max", "default", 380f), Same, "at 380 both are the paper" + Pair("max", "default", 380f));
            }
            finally
            {
                foreach (var t in shots.Values) Object.Destroy(t);
            }
        }

        // PART 3a: a revealed Start row shows EVERY tile -- each turned whole, at alpha 1, inside the card's width -- and each is drawn on the
        // render: its up petal (full tint) stands out of the ground under the row. A finished Reveal once left only its last two tiles
        private IEnumerator EveryTileOfTheStartRowIsDrawnWhole(SectionSheets.Sheet section, string what)
        {
            var o = section.Open;
            var row = o.StartTilesRow;
            Assert.AreEqual(Sheet.Root.layout.width, row.contentRect.width, 1f, what + ": the row spans the card");
            var tiles = o.StartTileOptions;
            int count = LookKit.TileRule.CountFor(row.contentRect.width, tiles.Size, tiles.Joint);
            Assert.AreEqual(16, count, "precondition: 16 tiles cover the phone's 390");
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(1f, tiles.ScaleOf?.Invoke(i) ?? 1f, 0.001f, what + ": tile " + i + " turned whole");
                Assert.AreEqual(1f, tiles.AlphaOf?.Invoke(i) ?? 1f, 0.001f, what + ": tile " + i + " at alpha 1");
                Assert.Less(i * tiles.Pitch, row.contentRect.width, what + ": tile " + i + " starts inside the card");
            }
            // - the section's top a third down the view (its row in sight, clear of the chrome and the bar); every section runs on the test's
            //   frozen clock, so the render holds still -- and is NOT finished by hand: the row is read as the Reveal itself left it
            Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(section.Root) - Scroll.contentViewport.layout.height / 3f);
            yield return CardTestInput.UntilStill(Sheet);
            Texture2D shot = null;
            yield return ThemePixelSet.GrabStill("start_row_" + what, null, r => shot = r);
            try
            {
                var area = ThemePixelSet.ScreenRectOf(row, shot);
                float px = 1f / ThemePixelSet.PanelUnitsPerPixel(row);
                // - the ground right under the row: the paper the tiles are told apart from
                Color ground = shot.GetPixel(area.xMin + Mathf.RoundToInt(6f * px), area.yMin - Mathf.RoundToInt(6f * px));
                for (int i = 0; i < count; i++)
                {
                    // - the DOWN petal's middle, unit point (12, 17): the row's rounded top corners (radius 24) clip the first and last
                    //   tiles' upper halves by design
                    float x = i * tiles.Pitch + 12f, y = 17f;
                    if (x >= row.contentRect.width) continue;
                    var petal = shot.GetPixel(area.xMin + Mathf.RoundToInt(x * px), area.yMax - Mathf.RoundToInt(y * px));
                    Assert.Greater(((Vector4)petal - (Vector4)ground).magnitude, 0.1f,
                        what + ": tile " + i + " drawn on the render (petal " + (Color32)petal + " vs the ground " + (Color32)ground + ")");
                }
                System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, "start_row_" + what + ".png"), shot.EncodeToPNG());
            }
            finally { Object.Destroy(shot); }
        }

        // The group's Section Style overrides the card's Start for that group alone
        [UnityTest]
        public IEnumerator AGroupsSectionStyle_OverridesTheCardsStart_ForThatGroupAlone()
        {
            yield return Open();
            var groups = Sections.Select(s => s.Group.Key).ToList();
            string target = groups[1];
            yield return Open(c =>
            {
                var rows = NavBarRule.DefaultRows(c.navigation, BlockRegistry.Shared);
                rows.Single(r => r.key == target).section_style = CardOptions.SectionStartWave;
                c.navigation.bar.items = rows;
            });
            foreach (var s in Sections)
                Assert.AreEqual(s.Group.Key == target ? CardOptions.SectionStartWave : CardOptions.SectionStartTiles, s.Open.Style.Start,
                    s.Group.Key + ": " + (s.Group.Key == target ? "its own Section Style" : "the card's Start"));
            yield return ShowSection(Sections[1]);
            yield return Save("group_section_style_wave");
        }

        // Every eyebrow and every body text of the section's first block keep 7:1 on the render, over every Start at Intensity max, light and
        // dark (the words drawn, then cleared: the ink against what lies under it)
        [UnityTest]
        public IEnumerator EveryEyebrowAndBodyText_Keeps7To1_OverEveryStart_AtIntensityMax_LightAndDark()
        {
            float worst = float.MaxValue;
            string worstWhat = "";
            var report = new List<string>();
            foreach (string start in CardOptions.SectionStarts)
                foreach (string mode in CardOptions.Modes)
                {
                    yield return Open(c => { c.sections.start = start; c.sections.intensity = CardSectionSettings.IntensityMax; });
                    ThemeController.SetMode(mode);
                    yield return CardTestInput.Settle(0.2f);
                    // - Bands tints every other section: the second section is a tinted one
                    var section = Sections[1];
                    yield return ShowSection(section);
                    var firstSlot = section.Slots.First(s => s.resolvedStyle.display == DisplayStyle.Flex);
                    var words = new List<Label> { section.Open.Name };
                    words.AddRange(firstSlot.Query<Label>().ToList().Where(l => l.resolvedStyle.display == DisplayStyle.Flex && !string.IsNullOrWhiteSpace(l.text)
                        && l.worldBound.yMax < Scroll.contentViewport.worldBound.yMax && l.worldBound.height > 0f).Take(6));
                    var colours = words.ToDictionary(l => l, l => l.resolvedStyle.color);
                    // - what each label's INLINE colour was (most have none: their colour is the stylesheet's, per mode): put back as it was --
                    //   restoring the resolved colour as an inline one pinned this mode's ink on the pooled labels for the next mode's pass
                    var inline = words.ToDictionary(l => l, l => l.style.color);
                    Texture2D drawn = null, under = null;
                    try
                    {
                        yield return ThemePixelSet.GrabStill($"contrast_{start}_{mode}", null, r => drawn = r);
                        foreach (var l in words) l.style.color = Color.clear;
                        yield return null;
                        yield return ThemePixelSet.GrabStill($"contrast_under_{start}_{mode}", null, r => under = r);
                        foreach (var l in words)
                        {
                            var (ratio, worstPixel, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(l, drawn), colours[l]);
                            if (inked == 0) continue;
                            report.Add($"{start} {mode} '{l.text}': {ratio:0.00}:1 (worst {(Color32)worstPixel})");
                            if (ratio < worst) { worst = ratio; worstWhat = $"'{l.text}' {start} {mode}"; }
                        }
                        Assert.Greater(report.Count(r => r.StartsWith(start + " " + mode)), 1, "precondition: the eyebrow and words of the first block measured");
                        System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"contrast_{start}_{mode}.png"), drawn.EncodeToPNG());
                    }
                    finally
                    {
                        foreach (var l in words) l.style.color = inline[l];
                        if (drawn != null) Object.Destroy(drawn);
                        if (under != null) Object.Destroy(under);
                    }
                }
            System.IO.File.WriteAllText(TestEvidence.PathFor(Domain, "contrast_ratios.txt"), string.Join("\n", report) + "\n");
            Assert.GreaterOrEqual(worst, Aaa, "every eyebrow and body text keeps 7:1 -- worst: " + worstWhat + "\n" + string.Join("\n", report));
        }

        // _3.3 P3a review (e): a REAL tap on each Navigation Bar group lands its section's Start fully below the Top Band -- the tile row's top
        // at the band's foot (one pixel), never dissolved in its fade -- with the band as Fade and as Glass. A section the scroll's end clamps
        // lands lower, never higher. Renders in TestEvidence/Premium/P3b/
        [UnityTest]
        public IEnumerator ARealJumpToEachGroup_LandsItsStartWholeBelowTheTopBand([Values(CardOptions.TopBandFade, CardOptions.TopBandGlass)] string band)
        {
            yield return Open(c => c.container.top_band = band);
            var bar = Sheet.Slots.Bar;
            Assert.IsNotNull(bar, "precondition: the default Navigation Bar");
            Assert.Greater(bar.Entries.Count, 2, "precondition: the Ribeira Palace has several groups");
            float onePixel = Sheet.Layer.panel.visualTree.layout.height / Screen.height;
            int landed = 0;
            for (int i = 1; i < bar.Entries.Count; i++)
            {
                var button = bar.Items[i].Button;
                yield return CardTestInput.TouchDrag(Sheet.Root.panel, button.worldBound.center, Vector2.zero, frames: 1);
                yield return CardTestInput.Settle(0.8f);
                yield return CardTestInput.UntilStill(Sheet);
                var slot = Stack.SlotOfBlock(bar.Entries[i].FirstTarget);
                var section = Stack.Sheets.SheetOf(slot);
                Assert.IsNotNull(section, "precondition: group " + i + "'s first block sits in an open section");
                Assert.IsTrue(Stack.Sheets.IsFirstShown(slot), "precondition: the jump lands the section, its Start in view");
                Assert.IsFalse(Sheet.Root.Q(className: BlockStackView.BarsOnHeroClass) != null || Sheet.Root.ClassListContains(BlockStackView.BarsOnHeroClass),
                    "precondition: past the hero, the Top Band is drawn");
                var start = section.Open.Edge.worldBound;
                float bandFoot = Sheet.ChromeBand.worldBound.yMax;
                string what = $"{band} group {i} '{bar.Items[i].Label.text}'";
                Assert.GreaterOrEqual(start.yMin, bandFoot - onePixel, what + ": the Start's top below the Top Band's foot (" + start.yMin + " vs " + bandFoot + ")");
                float range = Scroll.contentContainer.layout.height - Scroll.contentViewport.layout.height;
                if (Scroll.scrollOffset.y < range - 1f)
                {
                    Assert.AreEqual(bandFoot, start.yMin, onePixel, what + ": ...right under it, not lower");
                    landed++;
                }
                CardTestInput.HoldSectionsStill(Sheet);
                yield return CardTestInput.UntilStill(Sheet);
                Texture2D shot = null;
                yield return ThemePixelSet.GrabStill($"jump_{band}_{i}", null, r => shot = r);
                if (shot != null)
                {
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3b", $"jump_{band}_{i}.png"), shot.EncodeToPNG());
                    Object.Destroy(shot);
                }
            }
            Assert.Greater(landed, 0, "precondition: at least one group lands unclamped by the scroll's end");
        }
    }
}
