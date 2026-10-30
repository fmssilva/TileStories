using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using FontAsset = UnityEngine.TextCore.Text.FontAsset;

namespace TileStories.Tests
{
    // A bright, high-contrast page (_3.3 fourth round, "P3d review" a) on the REAL scene, the Ribeira Palace's card at Full on the defaults:
    //   - the words at FULL ink on the render where the developer looked (About's facts and the story preview): every text's darkest
    //     (light) / brightest (dark) pixel reaches its own ink, the body ink <= 50 on the light paper, every text >= 7:1 -- the tight UI
    //     atlases draw them; only the hero's halo copies draw the wide-SDF halo twins
    //   - the ground after the hero is SOLID paper: in the middle of a section (past Tint Depth) the render is the paper token (+-2) for every
    //     Start x Colour, light and dark, where the camera's world behind it (the wall, its markers) is measured to differ -- nothing of it shows
    //   - nothing of the hero's words, nor their halo copies, has a rect in view once the visitor scrolled past the hero
    // Renders in TestEvidence/Premium/P3e/.
    public class PoiCardBrightPageSceneTests : SearchSceneFixture
    {
        private const string Domain = "Premium/P3e";
        private const string Painting = "painting";
        private const float Aaa = 7f;
        // - the darkest body pixel the P3 renders had (the Architect's 43; 104 with the wide atlas)
        private const int DarkestBodyInk = 50;
        // - one render step either side (8-bit rounding of the blend) plus one: the brief's +-2
        private const float PaperTolerance = 2f / 255f;

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private ScrollView Scroll => Stack.Scroll;

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // The Ribeira Palace's card at Full on the framework's card defaults (`edit` over them), in `mode`, scrolled to `offset`, still
        private IEnumerator Open(string mode, float offset, System.Action<CardSettings> edit = null) => Open(mode, () => offset, edit);

        // The developer's capture spot: the last About facts in the middle of the view (their block's foot, read from the card's layout)
        private float FactsAndStory()
        {
            var facts = Stack.SlotOfBlock(ReferenceCard.BlockOf(BuiltInBlocks.QuickFactsKind));
            return facts.worldBound.yMax - Scroll.contentContainer.worldBound.yMin - Scroll.contentViewport.layout.height * 0.5f;
        }

        // The same, the offset worked out once the card is open at Full
        private IEnumerator Open(string mode, System.Func<float> offset, System.Action<CardSettings> edit = null)
        {
            CardTestInput.HoldTheWallStill(Sheet);
            var copy = ConfigCopy();
            copy.card_settings.sections = new CardSectionSettings();
            edit?.Invoke(copy.card_settings);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            if (!Sheet.IsOpen || Card.ShownPoiId != Painting) SelectionEventBus.Select(Painting);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "precondition: the Ribeira Palace's card is open");
            Sheet.SetStop(SheetStopRule.Stop.Full);
            ThemeController.SetMode(mode);
            yield return CardTestInput.Settle(0.6f);
            Scroll.scrollOffset = new Vector2(0f, offset());
            yield return CardTestInput.Settle(0.3f);
            CardTestInput.HoldSectionsStill(Sheet);
            yield return CardTestInput.UntilStill(Sheet);
        }

        private static Color Theme(VisualElement from, string token)
        {
            Assert.IsTrue(CardTokenColours.TryResolve(from, token, out Color c), "precondition: the theme has " + token);
            return c;
        }

        // The texts the visitor reads in the stack now: shown, with words, whole inside the part of the view no chrome covers
        private List<Label> ReadTexts()
        {
            var seen = CardTestInput.VisibleStack(Sheet);
            float foot = Scroll.contentViewport.worldBound.yMax - Stack.FootInset();
            return Scroll.contentContainer.Query<Label>().ToList().Where(l => !string.IsNullOrWhiteSpace(l.text) && l.worldBound.height > 0f
                && l.worldBound.yMin >= seen.yMin && l.worldBound.yMax <= Mathf.Min(seen.yMax, foot) && CardTestInput.IsShown(l, Sheet.Root)
                && l.resolvedStyle.color.a > 0.99f).ToList();
        }

        // Every text the visitor reads at the developer's spot: its glyphs reach its own ink on the render (the darkest pixel in light, the
        // brightest in dark, within 8 levels -- a soft atlas leaves the cores grey), the body ink <= 50 in light, and >= 7:1 measured round
        // its letters; drawn with the tight UI atlas
        [UnityTest]
        public IEnumerator EveryText_AtTheFactsAndTheStory_ReachesItsFullInk_And7To1_OnTheRender([Values("light", "dark")] string mode)
        {
            yield return Open(mode, FactsAndStory);
            var texts = ReadTexts();
            Assert.Greater(texts.Count, 8, "precondition: the About facts and the words after them are in view (" + texts.Count + ")");
            var ink = Theme(Sheet.Root, "--ts-ink");
            bool light = mode == CardOptions.ModeLight;
            var colours = texts.ToDictionary(l => l, l => l.resolvedStyle.color);
            var inline = texts.ToDictionary(l => l, l => l.style.color);
            Texture2D drawn = null, under = null;
            var report = new List<string>();
            try
            {
                yield return ThemePixelSet.GrabStill("ink_" + mode, null, r => drawn = r);
                foreach (var l in texts) l.style.color = Color.clear;
                yield return null;
                yield return ThemePixelSet.GrabStill("ink_under_" + mode, null, r => under = r);
                System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, "ink_" + mode + ".png"), drawn.EncodeToPNG());
                int bodyTexts = 0;
                var pixels = drawn.GetPixels32();
                foreach (var l in texts)
                {
                    var font = l.resolvedStyle.unityFontDefinition.fontAsset;
                    Assert.IsNotNull(font, "'" + l.text + "' draws a theme face");
                    Assert.IsFalse(font.name.EndsWith(" Halo"), "'" + l.text + "' draws the tight UI atlas, not the halo twin (" + font.name + ")");
                    var area = ThemePixelSet.ScreenRectOf(l, drawn);
                    // - the extreme pixel of the words: a channel's max (light ink: the darkest) or min (dark ink: the brightest)
                    int extreme = light ? 255 : 0;
                    for (int y = area.yMin; y < area.yMax; y++)
                        for (int x = area.xMin; x < area.xMax; x++)
                        {
                            var p = pixels[y * drawn.width + x];
                            extreme = light ? Mathf.Min(extreme, Mathf.Max(p.r, Mathf.Max(p.g, p.b))) : Mathf.Max(extreme, Mathf.Min(p.r, Mathf.Min(p.g, p.b)));
                        }
                    Color32 own = colours[l];
                    int ownExtreme = light ? Mathf.Max(own.r, Mathf.Max(own.g, own.b)) : Mathf.Min(own.r, Mathf.Min(own.g, own.b));
                    var (ratio, worst, inked) = ThemePixelSet.RingContrast(drawn, under, area, colours[l]);
                    report.Add($"{mode} '{l.text}' ink {own} extreme {extreme} ratio {ratio:0.00}:1 font {font.name}");
                    if (inked == 0) continue;
                    Assert.LessOrEqual(Mathf.Abs(extreme - ownExtreme), 8, $"'{l.text}' reaches its ink {own} on the render (extreme {extreme})");
                    Assert.GreaterOrEqual(ratio, Aaa, $"'{l.text}' >= 7:1 (worst ring pixel {(Color32)worst})");
                    if (light && ((Vector4)colours[l] - (Vector4)ink).magnitude < 0.01f)
                    {
                        bodyTexts++;
                        Assert.LessOrEqual(extreme, DarkestBodyInk, $"'{l.text}': the body ink's darkest pixel");
                    }
                }
                if (light) Assert.Greater(bodyTexts, 2, "precondition: body texts in the main ink measured");
            }
            finally
            {
                foreach (var l in texts) l.style.color = inline[l];
                if (drawn != null) Object.Destroy(drawn);
                if (under != null) Object.Destroy(under);
                System.IO.File.WriteAllText(TestEvidence.PathFor(Domain, "ink_" + mode + ".txt"), string.Join("\n", report) + "\n");
            }
        }

        // The ground in the middle of a section, past its Tint Depth, is the paper token on the render for every Start x Colour, light and
        // dark (Bands: its own flat tint) -- where the world behind the card (the wall, its markers), rendered with the card hidden, is not
        // paper: a see-through ground would show it
        [UnityTest]
        public IEnumerator TheGround_InTheMiddleOfASection_IsSolidPaper_EveryStartAndColour_LightAndDark()
        {
            var report = new List<string>();
            foreach (string mode in CardOptions.Modes)
                foreach (string start in CardOptions.SectionStarts)
                    foreach (string colour in CardOptions.SectionColours)
                    {
                        yield return Open(mode, 0f, c => { c.sections.start = start; c.sections.colour = colour; });
                        var section = Stack.Sheets.All.Where(s => s.Root.ClassListContains(SectionSheets.OpenClass)
                            && s.Root.resolvedStyle.display == DisplayStyle.Flex && s.Open.Edge.resolvedStyle.display == DisplayStyle.Flex).ElementAt(1);
                        Scroll.scrollOffset = new Vector2(0f, Stack.TopInScroll(section.Root) - Stack.LandingInset);
                        yield return CardTestInput.Settle(0.3f);
                        CardTestInput.HoldSectionsStill(Sheet);
                        yield return CardTestInput.UntilStill(Sheet);
                        var o = section.Open;
                        float groundTop = o.Edge.layout.height;
                        var paper = Theme(o.Edge, "--ts-sheet-open");
                        Assert.AreEqual(1f, paper.a, 0.001f, "the paper token is opaque");
                        Texture2D shot = null, world = null;
                        try
                        {
                            yield return ThemePixelSet.GrabStill($"ground_{start}_{colour}_{mode}", null, r => shot = r);
                            Sheet.Layer.style.visibility = Visibility.Hidden;
                            yield return null;
                            yield return ThemePixelSet.GrabStill($"world_{start}_{colour}_{mode}", null, r => world = r);
                            Sheet.Layer.style.visibility = StyleKeyword.Null;
                            var ground = ThemePixelSet.ScreenRectOf(o.Ground, shot);
                            float px = 1f / ThemePixelSet.PanelUnitsPerPixel(o.Ground);
                            var seen = CardTestInput.VisibleStack(Sheet);
                            float viewFoot = Mathf.Min(seen.yMax, Scroll.contentViewport.worldBound.yMax - Stack.FootInset());
                            int sampled = 0, worldDiffers = 0;
                            // - the right gutter (no words there), every 16 pt from past the tint to the section's or the view's foot
                            for (float y = groundTop + o.Style.TintDepth + 16f; y < o.Ground.layout.height; y += 16f)
                            {
                                float panelY = o.Ground.worldBound.yMin + y;
                                if (panelY < seen.yMin || panelY > viewFoot) continue;
                                int sx = Mathf.RoundToInt(ground.xMax - 6f * px), sy = Mathf.RoundToInt(ground.yMax - y * px);
                                Color drawnAt = shot.GetPixel(sx, sy), behind = world.GetPixel(sx, sy);
                                // - the section's own ground there: nothing (clear: the stack's paper shows) or the paper; Bands its flat tint
                                var own = LookKit.FadeRule.ColourAt(o.GroundOptions.Stops, y);
                                if (start != CardOptions.SectionStartBands)
                                    Assert.IsTrue(own.a < 0.01f || ((Vector4)own - (Vector4)paper).magnitude < 0.01f,
                                        $"{start} {colour} {mode}: past Tint Depth the ground's own colour at {y} is clear or the paper ({(Color32)own})");
                                // - Bands: its stop painted over the paper by the panel's blend (linear light): what the render shows
                                var expected = start == CardOptions.SectionStartBands && own.a >= 0.01f
                                    ? Color.Lerp(paper.linear, new Color(own.r, own.g, own.b, 1f).linear, own.a).gamma : paper;
                                expected.a = 1f;
                                drawnAt.a = 1f;
                                float off = Mathf.Max(Mathf.Abs(drawnAt.r - expected.r), Mathf.Max(Mathf.Abs(drawnAt.g - expected.g), Mathf.Abs(drawnAt.b - expected.b)));
                                Assert.LessOrEqual(off, PaperTolerance + 0.0001f,
                                    $"{start} {colour} {mode}: the ground at {y} pt is {(Color32)expected} on the render, drawn {(Color32)drawnAt} (behind: {(Color32)behind})");
                                sampled++;
                                if (((Vector4)behind - (Vector4)expected).magnitude > 30f / 255f) worldDiffers++;
                            }
                            report.Add($"{mode} {start} {colour}: {sampled} points, the world differs under {worldDiffers}");
                            Assert.Greater(sampled, 4, $"precondition ({start} {colour} {mode}): ground points past Tint Depth in view");
                            Assert.Greater(worldDiffers, 0, $"precondition ({start} {colour} {mode}): the world behind differs from the ground somewhere sampled");
                            if (colour == CardOptions.SectionColourGroup)
                                System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"ground_{start}_{mode}.png"), shot.EncodeToPNG());
                        }
                        finally
                        {
                            Sheet.Layer.style.visibility = StyleKeyword.Null;
                            if (shot != null) Object.Destroy(shot);
                            if (world != null) Object.Destroy(world);
                        }
                    }
            System.IO.File.WriteAllText(TestEvidence.PathFor(Domain, "ground.txt"), string.Join("\n", report) + "\n");
        }

        // Past the hero, nothing of its words -- nor of their halo copies -- has a rect in the view; at rest they are there, the halo copies
        // drawing the halo twins (the display face's for the title, the body's for the rest)
        [UnityTest]
        public IEnumerator PastTheHero_NoHeroWordNorHaloCopy_IsInView_AndTheHaloCopiesDrawTheHaloTwins()
        {
            yield return Open(CardOptions.ModeLight, 0f);
            var header = Stack.HeaderView;
            Assert.IsNotNull(header, "precondition: the card has its header");
            var halo = header.Words.HaloLabels;
            Assert.Greater(halo.Count, 2, "precondition: the hero's words have their halo copies");
            var display = header.HeroPart.Query<Label>(className: "card-hero__title").First();
            foreach (var copy in halo)
            {
                FontAsset font = copy.resolvedStyle.unityFontDefinition.fontAsset;
                Assert.IsNotNull(font, "the halo copy of '" + copy.text + "' has a face");
                Assert.IsTrue(font.name.EndsWith(" UITK Halo"), "the halo copy of '" + copy.text + "' draws a halo twin (" + font.name + ")");
                bool title = copy.text == display.text;
                Assert.AreEqual(title, font.name.StartsWith("Fraunces"), "'" + copy.text + "': the twin of its own face");
            }
            var view = Scroll.contentViewport.worldBound;
            var words = header.HeroPart.Query<Label>().ToList().Where(l => !string.IsNullOrWhiteSpace(l.text)).ToList();
            Assert.IsTrue(words.Any(l => l.worldBound.Overlaps(view)), "precondition: at rest the hero's words are in view");
            Scroll.scrollOffset = new Vector2(0f, header.HeroPart.layout.height + 40f);
            yield return CardTestInput.Settle(0.3f);
            yield return CardTestInput.UntilStill(Sheet);
            view = Scroll.contentViewport.worldBound;
            foreach (var l in words.Concat(halo))
                Assert.IsFalse(l.worldBound.Overlaps(view), $"'{l.text}' ({string.Join(",", l.GetClasses())}) lies at {l.worldBound}, inside the view {view}");
            yield return ThemePixelSet.GrabStill("past_hero", null, r =>
            {
                System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, "past_hero.png"), r.EncodeToPNG());
                Object.Destroy(r);
            });
        }
    }
}
