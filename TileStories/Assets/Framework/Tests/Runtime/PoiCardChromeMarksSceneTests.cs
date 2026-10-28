using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the premium target's chrome (_3.3 S4 third round, v5; _Test_Suite_1.md F2) on the REAL LivingRoomScene: the X and the
    // settings marks read at 7:1 against what they are drawn over -- measured on the real Game view's render, not on token arithmetic: over
    // the hero's picture (white marks on the dark hero glass, the top shade above the picture) and after it (the card's ink on the light
    // glass), for every hero picture of the test wall (its darkest and its lightest among them), light and dark, at every stop. Each
    // stop's render is the evidence the visual check reads (TestEvidence/Premium/P1/). After the hero the chrome row sits on one dense band
    // (no text through it, the title bare and at 7:1; TestEvidence/Premium/P2/).
    public class PoiCardChromeMarksSceneTests : SearchSceneFixture
    {
        private const string Domain = "Premium/P1";
        private const float Aaa = 7f;

        private PoiCardSheetView Sheet => Card.Sheet;
        private HeaderBlockView Header => Sheet.Stack.HeaderView;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private static IEnumerable<string> HeroPictures() => new[] { "lamp", "painting", "lamp_military", "lamp_religious" };

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // Where a mark is read: at rest at a stop, or at Full past the hero (the bar's own glass then)
        public enum Place { Peek, Half, Full, AfterTheHero }

        [UnityTest]
        public IEnumerator TheXAndSettings_ReadAt7To1_OnTheRealRender_AtEveryStop_OverTheHeroAndAfterIt(
            [ValueSource(nameof(HeroPictures))] string poi, [Values("light", "dark")] string mode)
        {
            ThemeController.SetMode(mode);
            // - the looks that move by themselves are measured still (the video loop on its poster, the turntable at rest)
            Session.CardSettings.container.reduce_motion = true;
            CardTestInput.HoldTheWallStill(Sheet);
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "precondition: " + poi + "'s card is open");
            Assert.IsTrue(Header.HasPicture, "precondition: " + poi + " has a hero picture");
            var report = new List<string>();
            float worstOfAll = float.MaxValue;
            string worstWhat = "";
            foreach (var place in new[] { Place.Peek, Place.Half, Place.Full, Place.AfterTheHero })
            {
                var stop = place == Place.Peek ? SheetStopRule.Stop.Peek : place == Place.Half ? SheetStopRule.Stop.Half : SheetStopRule.Stop.Full;
                Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
                Sheet.SetStop(stop);
                yield return CardTestInput.Settle();
                if (place == Place.AfterTheHero)
                {
                    // - a card whose blocks are too short to scroll its hero away never shows the chrome after the hero: nothing to read there
                    var scroll = Sheet.Stack.Scroll;
                    float range = scroll.contentContainer.layout.height - scroll.contentViewport.layout.height;
                    if (range < Header.HeroBottom())
                    {
                        report.Add($"{poi} {mode} {place}: none -- the card scrolls {range:0} pt, its hero ends at {Header.HeroBottom():0}");
                        continue;
                    }
                    scroll.scrollOffset = new Vector2(0f, Mathf.Min(range, Header.HeroBottom() + 200f));
                    yield return CardTestInput.Settle();
                }
                yield return CardTestInput.UntilStill(Sheet);
                CardTestInput.HoldSectionsStill(Sheet);
                bool onHero = Sheet.Root.ClassListContains(BlockStackView.BarsOnHeroClass);
                // - the place is what it says: the marks over the picture at Half and Full at the top, off it at Peek and past the hero
                Assert.AreEqual(place == Place.Half || place == Place.Full, onHero, "precondition: at " + place + " the marks are " + (onHero ? "" : "not ") + "over the hero");

                var marks = new[] { ("X", Sheet.CloseGlyph), ("settings", Sheet.SettingsGlyph) };
                var colours = marks.ToDictionary(m => m.Item2, m => m.Item2.resolvedStyle.color);
                Texture2D drawn = null, under = null;
                try
                {
                    yield return ThemePixelSet.GrabStill($"marks_{poi}_{mode}_{place}", null, r => drawn = r);
                    // - the marks made clear, everything else the same: what each is read against
                    foreach (var (_, glyph) in marks) { glyph.style.color = Color.clear; glyph.MarkDirtyRepaint(); }
                    yield return ThemePixelSet.GrabStill($"marks_under_{poi}_{mode}_{place}", null, r => under = r);
                    foreach (var (name, glyph) in marks)
                    {
                        var area = ThemePixelSet.ScreenRectOf(glyph.parent, drawn);
                        var (ratio, worst, inked) = ThemePixelSet.RingContrast(drawn, under, area, colours[glyph]);
                        Assert.Greater(inked, 0, "precondition: the " + name + " mark draws pixels at " + place);
                        report.Add($"{poi} {mode} {place} {name}: {ratio:0.00}:1 (worst pixel about its strokes {(Color32)worst}, mark {(Color32)colours[glyph]})");
                        if (ratio < worstOfAll) { worstOfAll = ratio; worstWhat = $"{name} at {place}"; }
                    }
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"{poi}_{mode}_{place}.png"), drawn.EncodeToPNG());
                }
                finally
                {
                    foreach (var (_, glyph) in marks) { glyph.style.color = StyleKeyword.Null; glyph.MarkDirtyRepaint(); }
                    if (drawn != null) Object.Destroy(drawn);
                    if (under != null) Object.Destroy(under);
                }
            }
            System.IO.File.AppendAllText(TestEvidence.PathFor(Domain, "marks_ratios.txt"), string.Join("\n", report) + "\n");
            Debug.Log("[Card] Chrome marks " + poi + " " + mode + ": worst " + worstOfAll.ToString("0.00") + ":1 (" + worstWhat + ")");
            Assert.GreaterOrEqual(worstOfAll, Aaa, "the X and settings keep 7:1 against what they are drawn over -- worst: " + worstWhat + "\n" + string.Join("\n", report));
        }

        // The chrome row after the hero (_3.3 S4 third round, v6; P2 PART 4 + its addendum), per Top Band: FADE (the default) -- the sheet's own
        // colour, solid through the row, fading to nothing below it, no hairline -- or GLASS -- a dense band with a hairline at its foot. Either
        // way the block text under the row never reads (ink seen through it against the surface seen through it stays under 1.25:1, blended
        // in linear light as the project renders, at the row's lowest line), the title is bare (no pill, no outline) and at 7:1 on the real
        // render; over the hero the band is gone. Renders in TestEvidence/Premium/P2/.
        [UnityTest]
        public IEnumerator AfterTheHero_TheChromeRowSitsOnItsTopBand_NoTextThroughIt_TheTitleBare_At7To1(
            [Values("painting", "lamp")] string poi, [Values("light", "dark")] string mode,
            [Values(CardOptions.TopBandFade, CardOptions.TopBandGlass)] string topBand)
        {
            const string P2 = "Premium/P2";
            ThemeController.SetMode(mode);
            Session.CardSettings.container.reduce_motion = true;
            var copy = ConfigCopy();
            copy.card_settings.container.reduce_motion = true;
            copy.card_settings.container.top_band = topBand;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            // - the push re-applies the wall's Default Mode: the mode measured is set after it, and proven
            ThemeController.SetMode(mode);
            yield return null;
            Assert.AreEqual(mode, ThemeController.CurrentMode, "precondition: the card shows " + mode);
            bool fade = topBand == CardOptions.TopBandFade;
            CardTestInput.HoldTheWallStill(Sheet);
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var band = Sheet.ChromeBand;
            Assert.IsTrue(Sheet.Root.ClassListContains(BlockStackView.BarsOnHeroClass), "precondition: at the top of Full the chrome is on the hero");
            Assert.Less(band.resolvedStyle.opacity, 0.01f, "over the hero the band is gone (the picture shows to the top)");
            Assert.AreEqual(!fade, band.ClassListContains(PoiCardSheetView.ChromeBandGlassClass), "the band the wall's Top Band picks");

            var scroll = Sheet.Stack.Scroll;
            float range = scroll.contentContainer.layout.height - scroll.contentViewport.layout.height;
            Assert.Greater(range, Header.HeroBottom() + 200f, "precondition: " + poi + "'s card scrolls its hero away");
            scroll.scrollOffset = new Vector2(0f, Header.HeroBottom() + 200f);
            yield return CardTestInput.Settle();
            yield return CardTestInput.UntilStill(Sheet);
            CardTestInput.HoldSectionsStill(Sheet);
            Assert.IsFalse(Sheet.Root.ClassListContains(BlockStackView.BarsOnHeroClass), "precondition: past the hero");

            // - the band covers the whole chrome row: from the sheet's top to below the marks and the title
            var row = band.worldBound;
            Assert.AreEqual(Sheet.Root.worldBound.yMin, row.yMin, OnePixel, "from the sheet's top");
            Assert.AreEqual(Sheet.Root.worldBound.width, row.width, OnePixel, "edge to edge");
            Assert.GreaterOrEqual(row.yMax, Sheet.CloseButton.worldBound.yMax - OnePixel, "below the X's touch target");
            Assert.GreaterOrEqual(row.yMax, Sheet.Stack.HeaderSlot.worldBound.yMax - OnePixel, "below the title's row");
            Assert.Greater(band.resolvedStyle.opacity, 0.99f, "past the hero the band shows");
            Assert.IsTrue(CardTokenColours.TryResolve(band, "--ts-ink", out Color ink), "precondition: the theme's ink");
            Assert.IsTrue(CardTokenColours.TryResolve(band, "--ts-surface", out Color surface), "precondition: the theme's surface");
            float through;
            if (fade)
            {
                Assert.AreEqual(0f, band.resolvedStyle.borderBottomWidth, 0.001f, "Fade: no hairline, no edge");
                // - the fade's own stops (the Look Kit's Fade the band draws: its source) at the row's lowest line, and at its foot
                var stops = Sheet.TopFade.Options.Stops;
                Assert.Greater(stops.Count, 1, "precondition: Fade draws a ramp");
                float rowFoot = Mathf.Max(Sheet.CloseButton.worldBound.yMax, Sheet.Stack.HeaderSlot.worldBound.yMax) - row.yMin;
                var tint = LookKit.FadeRule.ColourAt(stops, rowFoot);
                float alpha = 1f;
                float footAlpha = LookKit.FadeRule.ColourAt(stops, row.height, below: false).a;
                Assert.Less(footAlpha, 0.02f, "the content dissolves under it: the ramp ends at nothing");
                Assert.Greater(row.height, rowFoot + 20f, "...below the row, over a fade of its own");
                Assert.LessOrEqual(row.height, rowFoot + 28f, "...a SHORT one (_3.3 P2 review (b): no wide white area): " + (row.height - rowFoot) + " pt under the row");
                Color Over(Color under)
                {
                    var t = tint.linear;
                    var u = under.linear;
                    return Color.Lerp(u, new Color(t.r, t.g, t.b, 1f), alpha * tint.a).gamma;
                }
                through = CardTestInput.Contrast(Over(ink), Over(surface));
            }
            else
            {
                Assert.Greater(band.resolvedStyle.borderBottomWidth, 0f, "Glass: a hairline at its foot");
                through = CardTestInput.Contrast(CardTestInput.GroundOver(band, ink, Sheet.Root), CardTestInput.GroundOver(band, surface, Sheet.Root));
            }
            Assert.Less(through, 1.25f, "block text under the band: " + through.ToString("0.00") + ":1 against its own page, seen through the band");
            // - the handle and the bar row are clear: the band is the ONE ground
            Assert.Less(Sheet.Handle.resolvedStyle.backgroundColor.a, 0.01f, "the grab row adds no second glass");
            Assert.Less(Sheet.Stack.HeaderSlot.Q(className: "poi-card__header")?.resolvedStyle.backgroundColor.a ?? 0f, 0.01f, "nor the bar row");

            // - the title bare: no outline, no pill
            var title = Header.Root.Q<Label>(className: "card-header__title");
            Assert.IsNotNull(title, "precondition: the bar's title");
            Assert.IsTrue(Header.BarTitleShown, "precondition: past the hero the bar shows the title");
            Assert.AreEqual(0f, title.resolvedStyle.borderTopWidth + title.resolvedStyle.borderBottomWidth + title.resolvedStyle.borderLeftWidth
                + title.resolvedStyle.borderRightWidth, 0.001f, "no outline round the title");
            Assert.Less(title.resolvedStyle.backgroundColor.a, 0.01f, "no pill behind it");

            // - the title at 7:1 on the real render (its ink against the band and whatever lies under it, as drawn)
            var titleColour = title.resolvedStyle.color;
            Texture2D drawn = null, under = null;
            try
            {
                yield return ThemePixelSet.GrabStill($"band_{poi}_{mode}", null, r => drawn = r);
                title.style.color = Color.clear;
                yield return null;
                yield return ThemePixelSet.GrabStill($"band_under_{poi}_{mode}", null, r => under = r);
                var (ratio, worst, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(title, drawn), titleColour);
                Assert.Greater(inked, 0, "precondition: the title draws pixels");
                System.IO.File.WriteAllBytes(TestEvidence.PathFor(P2, $"{poi}_{mode}_{topBand}_AfterTheHero.png"), drawn.EncodeToPNG());
                System.IO.File.AppendAllText(TestEvidence.PathFor(P2, "band_ratios.txt"),
                    $"{poi} {mode} {topBand}: title {ratio:0.00}:1 (worst {(Color32)worst}), text through the band {through:0.00}:1\n");
                Assert.GreaterOrEqual(ratio, Aaa, "the title keeps 7:1 on the band: " + ratio.ToString("0.00") + ":1 (worst pixel " + (Color32)worst + ")");
            }
            finally
            {
                title.style.color = StyleKeyword.Null;
                if (drawn != null) Object.Destroy(drawn);
                if (under != null) Object.Destroy(under);
            }
        }
    }
}
