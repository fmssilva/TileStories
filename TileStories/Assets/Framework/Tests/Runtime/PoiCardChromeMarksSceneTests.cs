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
    // stop's render is the evidence the visual check reads (TestEvidence/Premium/P1/).
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
    }
}
