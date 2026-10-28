using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of SectionStyleRule (_3.3 fourth round, P3): v6's defaults and every option read with its default, an old config loading
    // unchanged, the three levels (the group's Section Style over Vary over the card's Start), no divider before the first section, the
    // tint's amounts (Bands every other section, the fade over 340), the sRGB mix painted exactly through the panel's linear blend, and the
    // contrast the Intensity cap keeps -- every eyebrow token and the body ink at 7:1 over every Start at Intensity max, light and dark,
    // over a black and a white camera (the theme's own values)
    public class SectionStyleRuleTests
    {
        [Test]
        public void TheDefaults_AreV6s_AndAnOldConfigLoadsThemUnchanged()
        {
            var s = new CardSectionSettings();
            Assert.AreEqual(CardOptions.SectionStartTiles, SectionStyleRule.Start(s));
            Assert.AreEqual(CardOptions.SectionDividerNone, SectionStyleRule.Divider(s));
            Assert.AreEqual(CardOptions.SectionColourGroup, SectionStyleRule.Colour(s));
            Assert.AreEqual(0.12f, SectionStyleRule.Intensity(s), 1e-6f);
            Assert.AreEqual(CardOptions.SectionMotionReveal, SectionStyleRule.Motion(s));
            Assert.AreEqual(CardOptions.SectionVarySame, SectionStyleRule.Vary(s));
            Assert.IsTrue(SectionStyleRule.IsOpen(new CardContainerSettings()), "Blocks Open by default");
            // - a config written before P3 has no sections, no blocks, no Section Style: it loads as the defaults
            var old = JsonUtility.FromJson<CardSettings>("{\"container\":{\"open_stop\":\"half\"},\"navigation\":{\"bar\":{\"items\":[{\"key\":\"about\",\"families\":[\"about\"]}]}}}");
            Assert.AreEqual(CardOptions.SectionStartTiles, old.sections.start);
            Assert.AreEqual(CardOptions.BlocksOpen, old.container.blocks);
            Assert.AreEqual("", old.navigation.bar.items[0].section_style);
            Assert.AreEqual(CardOptions.StopHalf, old.container.open_stop, "its own values kept");
            // - an unknown value reads as the default; intensity clamped to the cap
            var odd = new CardSectionSettings { start = "zigzag", divider = "?", colour = "pink", motion = "spin", vary = "x", intensity = 0.3f };
            Assert.AreEqual(CardOptions.SectionStartTiles, SectionStyleRule.Start(odd));
            Assert.AreEqual(CardOptions.SectionDividerNone, SectionStyleRule.Divider(odd));
            Assert.AreEqual(CardOptions.SectionColourGroup, SectionStyleRule.Colour(odd));
            Assert.AreEqual(CardOptions.SectionMotionReveal, SectionStyleRule.Motion(odd));
            Assert.AreEqual(CardOptions.SectionVarySame, SectionStyleRule.Vary(odd));
            Assert.AreEqual(CardSectionSettings.IntensityMax, SectionStyleRule.Intensity(odd), "v6's 0.30 reads as the cap");
            Assert.IsFalse(SectionStyleRule.IsOpen(new CardContainerSettings { blocks = CardOptions.BlocksCards }));
            CollectionAssert.AreEqual(new[] { "Plain", "Bands", "Fade", "Wave", "Tiles" }, CardOptions.SectionStartLabels, "v6's names and order");
            CollectionAssert.AreEqual(new[] { "None", "Hairline", "Ornament", "Azulejo", "Mosaic", "Image" }, CardOptions.SectionDividerLabels);
        }

        [Test]
        public void TheNearestLevelWins_TheGroupsSectionStyle_OverVary_OverTheCardsStart_AndTheFirstSectionHasNoDivider()
        {
            var s = new CardSectionSettings { start = CardOptions.SectionStartWave, divider = CardOptions.SectionDividerAzulejo };
            Assert.AreEqual(CardOptions.SectionStartWave, SectionStyleRule.Of(s, 1, 3, "").Start, "the card's Start");
            Assert.AreEqual(CardOptions.SectionStartBands, SectionStyleRule.Of(s, 1, 3, CardOptions.SectionStartBands).Start, "the group's own wins");
            Assert.AreEqual(CardOptions.SectionStartWave, SectionStyleRule.Of(s, 1, 3, "nonsense").Start, "an unknown group value is the card's");
            s.vary = CardOptions.SectionVaryByGroup;
            CollectionAssert.AreEqual(new[] { "fade", "wave", "tiles", "fade", "wave", "tiles" },
                Enumerable.Range(0, 6).Select(g => SectionStyleRule.Of(s, g, g, "").Start), "By Group: v6's walk, group by group");
            Assert.AreEqual(CardOptions.SectionStartPlain, SectionStyleRule.Of(s, 2, 2, CardOptions.SectionStartPlain).Start, "...the group's own still wins");
            Assert.AreEqual(CardOptions.SectionDividerNone, SectionStyleRule.Of(s, 0, 0, "").Divider, "nothing stands before the first section");
            Assert.AreEqual(CardOptions.SectionDividerAzulejo, SectionStyleRule.Of(s, 1, 1, "").Divider);
            Assert.AreEqual("--ts-family-about", SectionStyleRule.TintToken(CardOptions.SectionColourGroup, "--ts-family-about"));
            Assert.AreEqual(SectionStyleRule.AccentToken, SectionStyleRule.TintToken(CardOptions.SectionColourAccent, "--ts-family-about"));
            Assert.AreEqual(SectionStyleRule.NeutralToken, SectionStyleRule.TintToken(CardOptions.SectionColourNeutral, "--ts-family-about"));
            Assert.AreEqual("--ts-family-about-text", SectionStyleRule.TextOf("--ts-family-about"), "the eyebrow's words: the colour's text token");
        }

        [Test]
        public void TheTint_FadesOverV6s340_BandsTintEveryOtherSectionFlat_PlainNone()
        {
            var tiles = SectionStyleRule.Of(new CardSectionSettings { intensity = 0.2f }, 1, 1, "");
            Assert.AreEqual(0.2f, SectionStyleRule.AmountAt(tiles, 1, 0f), 1e-6f, "the intensity where it begins");
            Assert.AreEqual(0.1f, SectionStyleRule.AmountAt(tiles, 1, 170f), 1e-6f, "half way down the fade");
            Assert.AreEqual(0f, SectionStyleRule.AmountAt(tiles, 1, 340f), 1e-6f, "gone at 340");
            var bands = SectionStyleRule.Of(new CardSectionSettings { start = CardOptions.SectionStartBands, intensity = 0.2f }, 1, 1, "");
            Assert.AreEqual(0.14f, SectionStyleRule.AmountAt(bands, 1, 600f), 1e-6f, "an odd section: flat at 0.7 of it, all the way down");
            Assert.AreEqual(0f, SectionStyleRule.AmountAt(bands, 2, 0f), "an even one: none");
            var plain = SectionStyleRule.Of(new CardSectionSettings { start = CardOptions.SectionStartPlain }, 1, 1, "");
            Assert.IsFalse(plain.HasTint);
            Assert.AreEqual(0, SectionStyleRule.Stops(plain, 1, 0f).Count, "plain draws no tint");
            var stops = SectionStyleRule.Stops(tiles, 1, 24f);
            Assert.AreEqual((24f, 0f), stops[0], "clear above the ground's top");
            Assert.AreEqual(24f + 340f, stops.Last().Offset, 1e-4f, "the fade ends 340 under it");
        }

        [Test]
        public void TheTint_PaintedThroughTheLinearBlend_LandsOnV6sSrgbMix()
        {
            var sheet = new Color(15 / 255f, 16 / 255f, 18 / 255f);
            var tint = new Color(232 / 255f, 168 / 255f, 90 / 255f);
            foreach (float a in new[] { 0.04f, 0.12f, 0.2f })
            {
                var drawn = SectionStyleRule.Tint(sheet, tint, a);
                var seen = SectionStyleRule.BlendLinear(sheet, drawn);
                var want = Color.Lerp(sheet, tint, a);
                Assert.AreEqual(a, drawn.a, 1e-6f, "drawn at the amount");
                Assert.AreEqual(want.r, seen.r, 0.004f, a + ": red as v6's mix");
                Assert.AreEqual(want.g, seen.g, 0.004f, a + ": green");
                Assert.AreEqual(want.b, seen.b, 0.004f, a + ": blue");
            }
            // - why: the raw alpha of the bright tint over the dark sheet, blended in linear, is far brighter than v6's glow
            var raw = SectionStyleRule.BlendLinear(sheet, new Color(tint.r, tint.g, tint.b, 0.12f));
            Assert.Greater(raw.r, Color.Lerp(sheet, tint, 0.12f).r + 0.1f, "precondition of the rule: a raw alpha reads far brighter");
        }

        // The light card: a DARK tint over the light sheet lands on v6's mix too, every channel -- at the amount it would need a colour darker
        // than black (each hue clamped to the same black: Group, Accent and Neutral drew one grey), so it is drawn a little more opaque
        [Test]
        public void ADarkTint_OverTheLightSheet_LandsOnV6sMix_AndKeepsItsHue()
        {
            var sheet = new Color(248 / 255f, 246 / 255f, 242 / 255f);
            var navy = new Color(31 / 255f, 58 / 255f, 138 / 255f);
            var plum = new Color(120 / 255f, 40 / 255f, 90 / 255f);
            foreach (float a in new[] { 0.04f, 0.12f, 0.2f })
                foreach (var tint in new[] { navy, plum })
                {
                    var drawn = SectionStyleRule.Tint(sheet, tint, a);
                    var seen = SectionStyleRule.BlendLinear(sheet, drawn);
                    var want = Color.Lerp(sheet, tint, a);
                    Assert.GreaterOrEqual(drawn.a, a, "never fainter than the amount");
                    Assert.AreEqual(want.r, seen.r, 0.004f, a + " " + (Color32)tint + ": red as v6's mix");
                    Assert.AreEqual(want.g, seen.g, 0.004f, a + " " + (Color32)tint + ": green");
                    Assert.AreEqual(want.b, seen.b, 0.004f, a + " " + (Color32)tint + ": blue");
                }
            var navySeen = SectionStyleRule.BlendLinear(sheet, SectionStyleRule.Tint(sheet, navy, 0.2f));
            var plumSeen = SectionStyleRule.BlendLinear(sheet, SectionStyleRule.Tint(sheet, plum, 0.2f));
            Assert.Greater(((Vector4)navySeen - (Vector4)plumSeen).magnitude, 0.05f, "two hues stay two hues");
            Assert.Greater(navySeen.b, navySeen.r + 0.03f, "...the navy one reads blue");
        }

        // The cap's reason, on the theme's own values: at Intensity max every eyebrow token and the body ink keep 7:1 over every Start's
        // tint (each family's, the accent, the neutral), the open sheet over a black and a white camera, light and dark
        [Test]
        public void AtIntensityMax_EveryEyebrowAndTheBodyInk_Keep7To1_OverEveryTint_LightAndDark_OverAnyCamera()
        {
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeAsset>("Assets/Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset");
            Assert.IsNotNull(theme, "precondition: the framework theme");
            string[] families = { "meta", "about", "media", "guides", "stories", "ar", "play", "visit", "community", "navigation" };
            float max = CardSectionSettings.IntensityMax, worst = float.MaxValue;
            string worstWhat = "";
            foreach (string mode in CardOptions.Modes)
            {
                var r = theme.ResolverFor(mode);
                Color Of(string token)
                {
                    Assert.IsTrue(r.TryColour(token, out Color c), mode + ": the theme has " + token);
                    return c;
                }
                Color sheet = Of("--ts-sheet-open");
                var tints = families.Select(f => Of("--ts-family-" + f)).Append(Of("--ts-accent")).Append(Of(SectionStyleRule.NeutralToken)).ToList();
                var words = families.Select(f => ("--ts-family-" + f + "-text", Of("--ts-family-" + f + "-text"))).Append(("--ts-ink", Of("--ts-ink")));
                foreach (var tint in tints)
                    foreach (float a in new[] { 0f, max })
                        foreach (var camera in new[] { Color.black, Color.white })
                        {
                            var opaque = new Color(sheet.r, sheet.g, sheet.b, 1f);
                            var section = SectionStyleRule.BlendLinear(opaque, SectionStyleRule.Tint(opaque, tint, a));
                            var ground = SectionStyleRule.BlendLinear(camera, new Color(section.r, section.g, section.b, sheet.a));
                            foreach (var (token, ink) in words)
                            {
                                float ratio = UIAccessibility.ContrastRatio(ink, ground);
                                if (ratio < worst) { worst = ratio; worstWhat = $"{mode} {token} over {(Color32)tint} at {a}, camera {(Color32)camera}"; }
                            }
                        }
            }
            Assert.GreaterOrEqual(worst, 7f, "worst: " + worstWhat + " = " + worst.ToString("0.00") + ":1");
        }
    }
}
