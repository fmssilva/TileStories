using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TileStories.LookKit;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Look Kit's rules (_3.3 fourth round, P3), pure: the Fade's ramp (stops, hard edges, ends held, cuts), the tile's path (v6's
    // parts, inside its square, the petals turned, the Reveal's squeeze about the centre line), the Wave's path (v6's S handles reflected),
    // the Mosaic (v6's band, a seamless period), the Reveal (the row's place both ways, the tiles in order, Once, Reduce Motion) and the Drift (a seamless
    // loop, Reduce Motion still). And the kit's boundary: its assembly references nothing of the project, its sources name no card type
    public class LookKitRulesTests
    {
        private static readonly Color Blue = new(0.1f, 0.3f, 0.6f, 1f);

        // The Tone (_3.3 P3c addendum, Words On Picture): the prototype's gradients exactly -- the radial tone .55 at its anchor, .32 / .10 at
        // 38 / 62 % of its ellipse, nothing past 80 %, never rising going out along any ray; the edge scrim .66 at and below its anchor,
        // nothing past 66 % up, the same at every x; the strength scales every stop (never past 1); and what the panel DRAWS is the page's gamma
        // darkening reproduced in linear light (1 - (1 - a)^2.2): the tone looks as dark as the prototype, not paler
        [Test]
        public void TheTone_IsThePrototypesGradient_DrawnAsDarkAsThePageDrawsIt()
        {
            var tone = HeroWordsRule.Layer(HeroWordsRule.ToneHalo, 390f, 0f, new Rect(24f, 560f, 120f, 40f), Color.black);
            Assert.AreEqual(ToneShape.Radial, tone.Shape);
            Assert.AreEqual(new Vector2(78f, 600f), tone.Anchor, "20 % across, at the words' foot");
            Assert.Less(Vector2.Distance(new Vector2(507f, 450f), tone.Reach), 1e-3f, "130 % of the width, 75 % of the height seen down to the words' foot");
            Vector2 Along(float r, float angle) => tone.Anchor + new Vector2(Mathf.Cos(angle) * tone.Reach.x, -Mathf.Sin(angle) * tone.Reach.y) * r;
            foreach (float angle in new[] { 0f, 0.6f, 1.2f, Mathf.PI * 0.5f, 2.5f })
            {
                Assert.AreEqual(0.55f, ToneRule.PageAlpha(tone, Along(0f, angle)), 1e-4f, "at the anchor: .55");
                Assert.AreEqual(0.32f, ToneRule.PageAlpha(tone, Along(0.38f, angle)), 1e-4f, "38 %: .32");
                Assert.AreEqual(0.10f, ToneRule.PageAlpha(tone, Along(0.62f, angle)), 1e-4f, "62 %: .10");
                Assert.AreEqual(0f, ToneRule.PageAlpha(tone, Along(0.80f, angle)), 1e-4f, "80 %: nothing");
                Assert.AreEqual(0f, ToneRule.PageAlpha(tone, Along(1.2f, angle)), 1e-6f, "past it: nothing");
                float last = 1f;
                for (float r = 0f; r <= 1f; r += 0.01f)
                {
                    float a = ToneRule.PageAlpha(tone, Along(r, angle));
                    Assert.LessOrEqual(a, last + 1e-6f, "never rising going out, at " + r);
                    last = a;
                }
            }
            Assert.AreEqual(1f - Mathf.Pow(0.45f, 2.2f), ToneRule.AlphaAt(tone, tone.Anchor), 1e-4f, "drawn as the page's gamma darkening: .55 -> .83");
            Assert.AreEqual(0.4f, ToneRule.Page(ToneRule.Drawn(0.4f)), 1e-5f, "Drawn and Page undo each other");
            tone.Strength = 0.5f;
            Assert.AreEqual(0.275f, ToneRule.PageAlpha(tone, tone.Anchor), 1e-4f, "the strength scales every stop");
            tone.Strength = 3f;
            Assert.AreEqual(1f, ToneRule.PageAlpha(tone, tone.Anchor), 1e-6f, "never past full");

            var scrim = HeroWordsRule.Layer(HeroWordsRule.EdgeScrim, 390f, 0f, new Rect(24f, 560f, 120f, 40f), Color.black);
            Assert.AreEqual(ToneShape.Edge, scrim.Shape);
            foreach (float x in new[] { 0f, 200f, 390f })
            {
                Assert.AreEqual(0.66f, ToneRule.PageAlpha(scrim, new Vector2(x, 640f)), 1e-4f, "below the words' foot: .66");
                Assert.AreEqual(0.66f, ToneRule.PageAlpha(scrim, new Vector2(x, 600f)), 1e-4f, "at it: .66");
                Assert.AreEqual(0.30f, ToneRule.PageAlpha(scrim, new Vector2(x, 600f - 0.36f * 600f)), 1e-4f, "36 % up: .30");
                Assert.AreEqual(0f, ToneRule.PageAlpha(scrim, new Vector2(x, 600f - 0.66f * 600f)), 1e-4f, "66 % up: nothing");
            }
            // - words reaching far from the corner: the ellipse grows, its shape kept, until their farthest corner lies at the 38 % stop
            var tall = HeroWordsRule.Layer(HeroWordsRule.ToneHalo, 390f, 0f, new Rect(24f, 420f, 340f, 180f), Color.black);
            Assert.AreEqual(507f / 450f, tall.Reach.x / tall.Reach.y, 1e-3f, "the prototype's shape kept");
            Assert.Greater(tall.Reach.x, 507f, "...grown");
            Assert.AreEqual(HeroWordsRule.ToneDense, ToneRule.Distance(tall, new Vector2(364f, 420f)), 1e-3f, "the farthest word corner at the 38 % stop");
            Assert.IsNull(HeroWordsRule.Layer(HeroWordsRule.HaloOnly, 390f, 0f, new Rect(24f, 560f, 120f, 40f), Color.black), "Halo Only: nothing on the picture");
            Assert.AreEqual(HeroWordsRule.ToneHalo, HeroWordsRule.Of("anything"), "an unknown look reads as the default");
            Assert.IsTrue(HeroWordsRule.HasHalo(HeroWordsRule.ToneHalo) && HeroWordsRule.HasHalo(HeroWordsRule.HaloOnly) && !HeroWordsRule.HasHalo(HeroWordsRule.EdgeScrim));
        }

        // Tone Spread (the header's field): the tone's ellipse and the scrim's rise are the prototype's times the spread -- smaller ends the
        // darkening sooner, larger fades it over more -- clamped to its range (an unset 0 = the default); and however small, never past the
        // words: the grow still puts their farthest corner inside the dense part
        [Test]
        public void ToneSpread_ScalesTheReach_InsideItsRange_AndNeverShrinksPastTheWords()
        {
            var words = new Rect(24f, 560f, 120f, 40f);
            var one = HeroWordsRule.Layer(HeroWordsRule.ToneHalo, 390f, 0f, words, Color.black);
            foreach (float spread in new[] { HeroWordsRule.SpreadMin, 0.7f, 1f, 1.3f, HeroWordsRule.SpreadMax })
            {
                var tone = HeroWordsRule.Layer(HeroWordsRule.ToneHalo, 390f, 0f, words, Color.black, spread);
                var grown = tone.Reach.x > one.Reach.x * spread + 1e-3f;
                if (!grown) Assert.AreEqual(one.Reach.x * spread, tone.Reach.x, 1e-3f, "spread " + spread + ": the reach times the spread");
                Assert.AreEqual(one.Reach.x / one.Reach.y, tone.Reach.x / tone.Reach.y, 1e-3f, "...its shape kept");
                foreach (var corner in new[] { words.min, words.max, new Vector2(words.xMax, words.yMin), new Vector2(words.xMin, words.yMax) })
                    Assert.LessOrEqual(ToneRule.Distance(tone, corner), HeroWordsRule.ToneDense + 1e-3f, "spread " + spread + ": every word corner in the dense part");
                var scrim = HeroWordsRule.Layer(HeroWordsRule.EdgeScrim, 390f, 0f, words, Color.black, spread);
                Assert.AreEqual(Mathf.Max(600f * spread, 40f / HeroWordsRule.ScrimDense), scrim.Reach.y, 1e-3f, "spread " + spread + ": the scrim's rise");
            }
            var least = HeroWordsRule.Layer(HeroWordsRule.ToneHalo, 390f, 0f, words, Color.black, HeroWordsRule.SpreadMin);
            Assert.Less(least.Reach.x, one.Reach.x, "the least spread reaches less than the default");
            Assert.AreEqual(HeroWordsRule.SpreadMin, HeroWordsRule.Spread(0.01f), "below the range: its least");
            Assert.AreEqual(HeroWordsRule.SpreadMax, HeroWordsRule.Spread(9f), "above it: its most");
            Assert.AreEqual(HeroWordsRule.SpreadDefault, HeroWordsRule.Spread(0f), "unset: the default");
            Assert.AreEqual(HeroWordsRule.SpreadDefault, HeroWordsRule.Spread(float.NaN), "unreadable: the default");
            // - the field the Editor draws carries the same range and default
            var field = BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderToneSpreadField);
            Assert.AreEqual(BlockFieldType.Number, field.Type);
            Assert.AreEqual(HeroWordsRule.SpreadMin, field.NumberMin);
            Assert.AreEqual(HeroWordsRule.SpreadMax, field.NumberMax);
            Assert.AreEqual(HeroWordsRule.SpreadDefault, field.NumberDefault);
        }

        // The tone's strength from the picture (ADAPTIVE): the alpha a word needs is the least that keeps it at the ratio in the panel's linear
        // blend (worked back through the blend, the ratio lands exactly; a ground already dark enough needs none; a shade too weak for the
        // ground gives all it has); the strength draws that need where the stops give less than their peak, and never turns the peak past full
        [Test]
        public void TheTonesStrength_IsTheLeastThatKeepsEveryWordAtTheRatio_ZeroOnADarkGround()
        {
            var shade = new Color(0.04f, 0.03f, 0.02f, 1f);
            float white = ToneRule.Luminance(Color.white), shadeY = ToneRule.Luminance(shade);
            Assert.AreEqual(1f, white, 1e-4f);
            foreach (float ground in new[] { 0.25f, 0.5f, 0.8f, 1f })
            {
                float a = ToneRule.NeededAlpha(ground, white, shadeY, 4.5f);
                float left = ground * (1f - a) + shadeY * a;
                Assert.AreEqual(4.5f, (white + 0.05f) / (left + 0.05f), 0.01f, "ground " + ground + ": exactly 4.5:1, never more shade than that");
            }
            Assert.AreEqual(0f, ToneRule.NeededAlpha(0.15f, white, shadeY, 4.5f), "a dark ground (1.05 / 0.2 = 5.25:1 already): none");
            Assert.AreEqual(1f, ToneRule.NeededAlpha(1f, white, 0.5f, 4.5f), "a shade lighter than the ratio allows: all of it");
            Assert.AreEqual(1f, ToneRule.NeededAlpha(float.NaN, white, shadeY, 4.5f), "nothing read: all of it");

            Assert.AreEqual(0f, ToneRule.StrengthFor(0f, 0.32f, 0.55f), "no need: no tone");
            float need = 0.6f;
            float s = ToneRule.StrengthFor(need, 0.32f, 0.55f);
            Assert.AreEqual(need, ToneRule.Drawn(s * 0.32f), 1e-4f, "at a word the stops give .32: the strength draws exactly its need");
            Assert.AreEqual(1f / 0.55f, ToneRule.StrengthFor(0.99f, 0.05f, 0.55f), 1e-4f, "a word the stops barely reach: capped at a full peak");
            Assert.AreEqual(1f / 0.55f, ToneRule.StrengthFor(0.5f, 0f, 0.55f), 1e-4f, "a word past the stops: the cap");
        }

        // The ink of a wrapped text (HeroTitleFitRule.LineWords; the Local Shade hugs these): greedy, each word at its place in its line and
        // its own width; LineWidths and Lines read the same wrap
        [Test]
        public void LineWords_AreEachWordsPlaceAndWidth_LineByLine_AndLineWidthsAndLinesCountThem()
        {
            float Measure(string run) => run.Length * 10f;
            var lines = HeroTitleFitRule.LineWords("Ribeira Palace Lisbon", 150f, Measure);
            Assert.AreEqual(2, lines.Count, "\"Ribeira Palace\" (14 chars) fits 150, \"Lisbon\" wraps");
            CollectionAssert.AreEqual(new[] { (0f, 70f), (80f, 60f) }, lines[0], "Ribeira at 0, Palace after its space");
            CollectionAssert.AreEqual(new[] { (0f, 60f) }, lines[1]);
            CollectionAssert.AreEqual(new[] { 140f, 60f }, HeroTitleFitRule.LineWidths("Ribeira Palace Lisbon", 150f, Measure));
            CollectionAssert.AreEqual(new[] { 140f }, HeroTitleFitRule.LineWidths("Ribeira Palace", 400f, Measure), "a short title: one line, its own width");
            CollectionAssert.AreEqual(new[] { 50f, 50f, 20f }, HeroTitleFitRule.LineWidths("Lisboaaaaaaa", 50f, Measure), "a word wider than the line fills it, the rest on its last line");
            Assert.AreEqual(0, HeroTitleFitRule.LineWords(" ", 100f, Measure).Count, "empty: no line");
            Assert.AreEqual(2, HeroTitleFitRule.Lines("Ribeira Palace Lisbon", 150f, Measure));
        }

        // A picture's brightness read back (PictureLuminance: a card's pictures are not CPU-readable): a picture white at its top and black
        // at its foot reads 1 at the top and 0 at the foot -- rows the right way up -- and a grey square reads its own luminance
        [Test]
        public void PictureLuminance_ReadsAPicturesBrightnessTheRightWayUp()
        {
            var picture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var grey = new Color(0.5f, 0.5f, 0.5f, 1f);
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    // - a texture's row 0 is its FOOT: rows 4..7 are the top half
                    picture.SetPixel(x, y, y >= 4 ? Color.white : x < 4 ? grey : Color.black);
            picture.Apply();
            try
            {
                var read = PictureLuminance.Read(picture, 8, 8);
                Assert.AreEqual(1f, read.MaxIn(new Rect(0f, 0f, 1f, 0.5f)), 0.02f, "the top half is white");
                Assert.AreEqual(ToneRule.Luminance(grey), read.MaxIn(new Rect(0f, 0.5f, 0.5f, 0.5f)), 0.02f, "the foot's left quarter: its grey");
                Assert.AreEqual(0f, read.MaxIn(new Rect(0.5f, 0.5f, 0.5f, 0.5f)), 0.02f, "the foot's right quarter: black");
                Assert.IsNaN(read.MaxIn(new Rect(1.2f, 0f, 0.5f, 1f)), "outside the picture: nothing read");
            }
            finally { Object.DestroyImmediate(picture); }
        }

        [Test]
        public void TheFade_IsStraightBetweenItsStops_HoldsItsEnds_AndCutsAHardEdgeWhereTwoStopsMeet()
        {
            var ramp = FadeOptions.Ramp(Blue, 20f, 340f);
            Assert.AreEqual(0f, FadeRule.ColourAt(ramp.Stops, 10f).a, 1e-5f, "clear above its start");
            Assert.AreEqual(1f, FadeRule.ColourAt(ramp.Stops, 20f, below: true).a, 1e-5f, "just under the start: the whole colour (a hard edge)");
            Assert.AreEqual(0f, FadeRule.ColourAt(ramp.Stops, 20f, below: false).a, 1e-5f, "...just above it: still clear");
            Assert.AreEqual(0.5f, FadeRule.ColourAt(ramp.Stops, 20f + 170f).a, 1e-5f, "halfway: half");
            Assert.AreEqual(0.25f, FadeRule.ColourAt(ramp.Stops, 20f + 255f).a, 1e-5f, "three quarters: a quarter (a straight line)");
            Assert.AreEqual(0f, FadeRule.ColourAt(ramp.Stops, 400f).a, 1e-5f, "below the end: the end held");
            Assert.AreEqual(Blue.r, FadeRule.ColourAt(ramp.Stops, 100f).r, 1e-5f, "the colour itself never changes in a ramp, only its alpha");
            CollectionAssert.AreEqual(new[] { 0f, 20f, 360f, 500f }, FadeRule.Cuts(ramp.Stops, 500f), "one band per pair of cuts, each line once");
            CollectionAssert.AreEqual(new[] { 0f, 20f, 100f }, FadeRule.Cuts(ramp.Stops, 100f), "a stop past the foot is no cut");
            Assert.AreEqual(Color.clear, FadeRule.ColourAt(new List<FadeStop>(), 5f), "no stops: nothing");
        }

        [Test]
        public void TheTile_IsV6s_GroundCornersPetalsDot_EveryPartInsideItsSquare_ThePetalsTurnedAboutTheCentre()
        {
            var shapes = TileRule.Shapes();
            CollectionAssert.AreEqual(new[] { TileRule.Part.Ground }, shapes.Take(1).Select(s => s.Part));
            Assert.AreEqual(4, shapes.Count(s => s.Part == TileRule.Part.Corner), "four corner quarter discs");
            Assert.AreEqual(4, shapes.Count(s => s.Part == TileRule.Part.Petal), "four petals");
            Assert.AreEqual(TileRule.Part.Dot, shapes.Last().Part, "the centre dot last, over the petals");
            foreach (var shape in shapes)
            {
                Assert.AreEqual(0, shape.Cubics.Count % 3, shape.Part + ": whole cubic segments");
                Assert.AreEqual(shape.Start, shape.Cubics[shape.Cubics.Count - 1], shape.Part + ": a closed path");
                foreach (var p in shape.Cubics.Append(shape.Start))
                {
                    Assert.That(p.x, Is.InRange(-1e-4f, TileRule.Unit + 1e-4f), shape.Part + " inside the tile");
                    Assert.That(p.y, Is.InRange(-1e-4f, TileRule.Unit + 1e-4f), shape.Part + " inside the tile");
                }
            }
            // - the petals: v6's up petal, then each a quarter turn clockwise -- the right petal's tip is v6's "M20.8 12"
            var petals = shapes.Where(s => s.Part == TileRule.Part.Petal).ToList();
            Assert.Less((petals[0].Start - new Vector2(12f, 3.2f)).magnitude, 1e-4f, "up");
            Assert.AreEqual(20.8f, petals[1].Start.x, 1e-4f, "right: v6's M20.8 12");
            Assert.AreEqual(12f, petals[1].Start.y, 1e-4f);
            Assert.AreEqual(new Vector2(17.4f, 14.8f), petals[1].Cubics[0], "...its first handle v6's C17.4 14.8");
            Assert.AreEqual(20.8f, petals[2].Start.y, 1e-4f, "down");
            Assert.AreEqual(3.2f, petals[3].Start.x, 1e-4f, "left");
            // - a corner disc: from its corner, round radius 7 (its arc's ends 7 along each side), the dot r 1.7 round the centre
            var first = shapes.First(s => s.Part == TileRule.Part.Corner);
            Assert.AreEqual(Vector2.zero, first.Start, "the first corner disc is the top-left one");
            Assert.AreEqual(new Vector2(TileRule.CornerRadius, 0f), first.Cubics[2], "along the top edge to radius 7");
            Assert.AreEqual(new Vector2(0f, TileRule.CornerRadius), first.Cubics[5], "round to radius 7 down the left edge");
            var dot = shapes.Last();
            Assert.AreEqual(TileRule.Centre.y - TileRule.DotRadius, dot.Start.y, 1e-4f);
            Assert.AreEqual(0.16f, TileRule.AlphaOf(TileRule.Part.Ground));
            Assert.AreEqual(0.55f, TileRule.AlphaOf(TileRule.Part.Corner));
            Assert.AreEqual(1f, TileRule.AlphaOf(TileRule.Part.Petal));
        }

        [Test]
        public void ARowOfTiles_CoversItsWidth_AndTheRevealSqueezesATileAboutItsCentreLine()
        {
            Assert.AreEqual(16, TileRule.CountFor(390f, 24f, 1f), "v6: 16 tiles of 24 with 1 pt joints cover the phone's 390");
            Assert.AreEqual(1, TileRule.CountFor(24f, 24f, 1f));
            Assert.AreEqual(0, TileRule.CountFor(0f, 24f, 1f));
            // - the tile's centre line stays put whatever the squeeze; its edges come in symmetrically
            Assert.AreEqual(new Vector2(62f, 12f), TileRule.Place(TileRule.Centre, 50f, 0f, 24f, 0.3f), "the centre line holds");
            var left = TileRule.Place(new Vector2(0f, 0f), 50f, 0f, 24f, 0.5f);
            var right = TileRule.Place(new Vector2(24f, 0f), 50f, 0f, 24f, 0.5f);
            Assert.AreEqual(56f, left.x, 1e-4f, "half width: the left edge 6 in");
            Assert.AreEqual(68f, right.x, 1e-4f, "...the right edge 6 in");
            Assert.AreEqual(new Vector2(98f, 48f), TileRule.Place(new Vector2(24f, 24f), 50f, 0f, 48f, 1f), "scaled to its size: a 48 pt tile's far corner, crisp at any scale");
        }

        [Test]
        public void TheWave_IsV6sPath_TheSHandlesReflected_StretchedToTheWidth_RisingFromTheFoot()
        {
            var c = WaveRule.Cubics;
            Assert.AreEqual(9, c.Count, "three cubics");
            // - S's first handle is the previous handle reflected through the previous point: 2 * (150,12) - (98,2), 2 * (318,20) - (262,30)
            Assert.AreEqual(new Vector2(202f, 22f), c[3]);
            Assert.AreEqual(new Vector2(374f, 10f), c[6]);
            Assert.AreEqual(new Vector2(390f, 10f), c[8], "ends on v6's right edge");
            Assert.AreEqual(new Vector2(0f, 17f), WaveRule.Place(WaveRule.Start, 390f, 30f, 1f), "v6's box unchanged at 390 x 30");
            Assert.AreEqual(180f, WaveRule.Place(new Vector2(195f, 30f), 360f, 30f, 1f).x, 1e-4f, "stretched to the width");
            Assert.AreEqual(30f, WaveRule.Place(new Vector2(150f, 12f), 390f, 30f, 0f).y, 1e-4f, "risen 0: flat on the foot");
            Assert.AreEqual(21f, WaveRule.Place(new Vector2(150f, 12f), 390f, 30f, 0.5f).y, 1e-4f, "risen half: half its height above the foot");
        }

        [Test]
        public void TheMosaic_IsV6sWovenBand_AndItsPeriodTilesWithoutASeam()
        {
            Assert.AreEqual(0.92f, MosaicRule.AlphaOf(0, 1), "on the band (column 0: the band at 1.5)");
            Assert.AreEqual(0.42f, MosaicRule.AlphaOf(0, 3), "beside it");
            Assert.AreEqual(0.92f, MosaicRule.AlphaOf(14, 1), "one period on: the same (14 columns)");
            for (int row = 0; row < MosaicRule.Rows; row++)
                Assert.AreEqual(MosaicRule.AlphaOf(0, row), MosaicRule.AlphaOf(MosaicRule.Period, row), "column 14 = column 0: no seam");
            var pixels = MosaicRule.Pixels(2, out int w, out int h);
            Assert.AreEqual(196, w);
            Assert.AreEqual(56, h);
            // - the joints between cells are clear, a cell's middle is its alpha
            Assert.AreEqual(0, pixels[(h - 1 - 13) * w + 13].a, "the joint between two cells (6.5 .. 7 units) is clear");
            Assert.AreEqual(Mathf.RoundToInt(MosaicRule.AlphaOf(0, 0) * 255f), pixels[(h - 1 - 6) * w + 6].a, 1, "the first cell's middle");
        }

        // The Reveal follows the row's PLACE (_3.3 P3c review c, second pass b): 0 as its bottom clears the start line (a bar's top), 1 a third
        // up the view; the tiles turn in order with it, the same function both ways (scrolling back turns them out); Once keeps a whole row
        // whole; Reduce Motion is whole throughout
        [Test]
        public void TheReveal_FollowsTheRowsPlaceBothWays_TheTilesTurnInOrder_OnceStaysWhole_AndReduceMotionIsWhole()
        {
            var o = new RevealOptions();
            // - the travel at the phone's Full: a 844 view, a bar's band resting 82 above its foot
            float start = 844f - 82f, end = RevealRule.EndLine(844f, o);
            Assert.AreEqual(844f * 2f / 3f, end, 1e-3f, "the end line a third of the way up the view");
            Assert.AreEqual(0f, RevealRule.Progress(start + 30f, start, end), "under the bar's top: nothing yet");
            Assert.AreEqual(0f, RevealRule.Progress(start, start, end), "just clearing it: 0");
            Assert.AreEqual(0.5f, RevealRule.Progress((start + end) * 0.5f, start, end), 1e-5f, "half way up the travel: 0.5");
            Assert.AreEqual(1f, RevealRule.Progress(end, start, end), 1e-5f, "a third up: 1");
            Assert.AreEqual(1f, RevealRule.Progress(10f, start, end), "higher still: 1");
            Assert.AreEqual(0f, RevealRule.Progress(float.NaN, start, end), "not laid out: never");
            Assert.AreEqual(1f, RevealRule.Progress(99f, 100f, 300f), "a start line over the end (a tall bar) still has a travel: no division by 0");

            // - 16 tiles (the phone's 390): in order, each over a quarter of the travel; half way up = half of them past half their turn
            const int n = 16;
            int PastHalf(float p) => Enumerable.Range(0, n).Count(i => RevealRule.PieceProgress(i, n, p, o) >= 0.5f);
            Assert.AreEqual(0, PastHalf(0f), "at 0 none");
            Assert.AreEqual(8, PastHalf(0.5f), "half way: half the tiles");
            Assert.AreEqual(n, PastHalf(1f), "at 1 all");
            for (float p = 0f; p <= 1f; p += 0.01f)
                for (int i = 1; i < n; i++)
                    Assert.GreaterOrEqual(RevealRule.PieceProgress(i - 1, n, p, o), RevealRule.PieceProgress(i, n, p, o), "in order at " + p);
            for (int i = 0; i < n; i++)
            {
                Assert.AreEqual(0f, RevealRule.Turn(i, n, 0f, o), "tile " + i + " unturned at 0");
                Assert.AreEqual(0f, RevealRule.Alpha(i, n, 0f, o), "...and unseen");
                Assert.AreEqual(1f, RevealRule.Turn(i, n, 1f, o), 1e-4f, "tile " + i + " whole at 1 (the last ends its window there)");
                Assert.AreEqual(1f, RevealRule.Alpha(i, n, 1f, o), 1e-4f, "...at alpha 1");
                // - the alpha never falls back while the progress rises (the ease is clamped before the parabola)
                float last = 0f;
                for (float p = 0f; p <= 1f; p += 0.01f)
                {
                    float a = RevealRule.Alpha(i, n, p, o);
                    Assert.GreaterOrEqual(a, last - 1e-5f, "tile " + i + " fades in monotonically (at " + p + ")");
                    last = a;
                }
            }
            float peak = 0f;
            for (float p = 0f; p <= 1f; p += 0.005f) peak = Mathf.Max(peak, RevealRule.Turn(0, n, p, o));
            Assert.Greater(peak, 1.01f, "v6's cubic-bezier(.3, 1.35, .5, 1) passes whole on the way");
            // - both ways: the same place gives the same tiles, whichever way the visitor came to it
            Assert.AreEqual(RevealRule.Turn(5, n, 0.4f, o), RevealRule.Turn(5, n, 0.4f, o));

            // - Replay: Every Pass follows the place, Once keeps a whole row whole (a row not yet whole still follows it)
            Assert.AreEqual(0.2f, RevealRule.Held(1f, 0.2f, false), 1e-6f, "Every Pass: back down, the tiles turn out");
            Assert.AreEqual(1f, RevealRule.Held(1f, 0.2f, true), "Once: whole stays whole");
            Assert.AreEqual(0.2f, RevealRule.Held(0.6f, 0.2f, true), 1e-6f, "Once, not yet whole: it follows");

            var still = new RevealOptions { ReduceMotion = true };
            Assert.AreEqual(1f, RevealRule.Turn(15, n, 0f, still), "Reduce Motion: every tile whole at once");
            Assert.AreEqual(1f, RevealRule.Alpha(15, n, 0f, still));
            Assert.AreEqual(1f, Easing.CubicBezier(0.3f, 1.35f, 0.5f, 1f, 1f));
        }

        [Test]
        public void TheDrift_SlidesAtItsSpeed_LoopsEveryPeriodWithoutASeam_AndReduceMotionHoldsIt()
        {
            var o = new DriftOptions();
            Assert.AreEqual(10f, o.Speed, "v6: 400 px in 40 s");
            Assert.AreEqual(5f, DriftRule.Offset(0.5f, 25f, o), 1e-4f);
            Assert.AreEqual(0f, DriftRule.Offset(2.5f, 25f, o), 1e-4f, "one period on: back where it began (one tile and joint)");
            Assert.AreEqual(DriftRule.Offset(0.3f, 25f, o), DriftRule.Offset(2.8f, 25f, o), 1e-3f, "periodic");
            Assert.AreEqual(0f, DriftRule.Offset(7f, 25f, new DriftOptions { ReduceMotion = true }), "Reduce Motion: still");
        }

        // The kit never depends on the card (the brief): its assembly references nothing, its sources name no card type
        [Test]
        public void TheLookKit_IsItsOwnAssembly_ReferencingNothingOfTheProject_AndNamesNoCardType()
        {
            const string folder = "Assets/Framework/Runtime/UI/LookKit";
            string asmdef = File.ReadAllText(Path.Combine(folder, "TileStories.LookKit.asmdef"));
            StringAssert.Contains("\"references\": []", asmdef, "the kit references no assembly");
            var compiled = typeof(TileRule).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
            Assert.IsFalse(compiled.Any(n => n.StartsWith("TileStories")), "the compiled kit references no TileStories assembly: " + string.Join(", ", compiled));
            foreach (string file in Directory.GetFiles(folder, "*.cs"))
            {
                string source = File.ReadAllText(file);
                StringAssert.Contains("namespace TileStories.LookKit", source, file + ": one namespace");
                foreach (string card in new[] { "PoiCard", "BlockStack", "SectionSheet", "CardSettings", "BlockBindContext", "NavBar" })
                    StringAssert.DoesNotContain(card, source, file + " names the card's " + card);
            }
        }
    }
}
