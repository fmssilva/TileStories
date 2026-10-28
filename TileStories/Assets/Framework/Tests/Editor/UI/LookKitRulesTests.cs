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
    // the Mosaic (v6's band, a seamless period), the Reveal (stagger, overshoot, Reduce Motion, the enter line) and the Drift (a seamless
    // loop, Reduce Motion still). And the kit's boundary: its assembly references nothing of the project, its sources name no card type
    public class LookKitRulesTests
    {
        private static readonly Color Blue = new(0.1f, 0.3f, 0.6f, 1f);

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

        [Test]
        public void TheReveal_StaggersEachPiece_OverShootsAndSettles_AndReduceMotionShowsEverythingAtOnce()
        {
            var o = new RevealOptions();
            Assert.AreEqual(0.045f, o.Stagger, "~45 ms from one tile to the next (the brief)");
            Assert.AreEqual(0f, RevealRule.Turn(0, 0f, o), "the first tile starts at nothing");
            Assert.AreEqual(0f, RevealRule.Turn(3, 0.13f, o), "the fourth waits its 135 ms");
            Assert.Greater(RevealRule.Turn(3, 0.14f, o), 0f, "...then turns");
            float peak = 0f;
            for (float t = 0f; t <= 0.7f; t += 0.01f) peak = Mathf.Max(peak, RevealRule.Turn(0, t, o));
            Assert.Greater(peak, 1.01f, "v6's cubic-bezier(.3, 1.35, .5, 1) passes whole");
            Assert.AreEqual(1f, RevealRule.Turn(0, 0.7f, o), 1e-4f, "...and settles at its 0.7 s");
            Assert.AreEqual(1f, RevealRule.Alpha(0, 0.4f, o), 1e-4f, "faded in after 0.4 s");
            Assert.AreEqual(15 * 0.045f + 0.7f, RevealRule.Length(16, o), 1e-4f, "16 tiles: the last starts 675 ms in");
            // - once faded in a piece STAYS in: every tile whole at the Reveal's end and after (the ease's parabola fell back to 0 past twice
            //   the fade, and a finished row showed only its last two tiles)
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(1f, RevealRule.Alpha(i, RevealRule.Length(16, o), o), 1e-4f, "tile " + i + " drawn whole at the end");
                Assert.AreEqual(1f, RevealRule.Alpha(i, 60f, o), 1e-4f, "tile " + i + " a minute later");
                float last = 0f;
                for (float t = 0f; t <= 2f; t += 0.01f)
                {
                    float a = RevealRule.Alpha(i, t, o);
                    Assert.GreaterOrEqual(a, last - 1e-5f, "tile " + i + " never fades back out (at " + t + " s)");
                    last = a;
                }
            }
            var still = new RevealOptions { ReduceMotion = true };
            Assert.AreEqual(1f, RevealRule.Turn(15, 0f, still), "Reduce Motion: every tile whole at once");
            Assert.AreEqual(1f, RevealRule.Alpha(15, 0f, still));
            Assert.AreEqual(0f, RevealRule.Length(16, still));
            // - the enter line: the section's top above the view's foot by the margin
            Assert.IsFalse(RevealRule.Entered(1000f, 200f, 800f, 30f), "top at 800 in an 800 view: not yet");
            Assert.IsFalse(RevealRule.Entered(971f, 200f, 800f, 30f), "771 in: still not past the 30 margin");
            Assert.IsTrue(RevealRule.Entered(969f, 200f, 800f, 30f), "769 in: entered");
            Assert.IsFalse(RevealRule.Entered(float.NaN, 0f, 800f, 30f), "not laid out: never");
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
