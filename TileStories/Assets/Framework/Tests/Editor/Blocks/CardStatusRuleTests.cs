using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The card's status colour (_3.1 step 5b) comes from the MARKERS' outline rule, on the SHIPPED wall's own Outline
    // Types table: the same colour and line style as the POI's marker ring in every outline mode, the tokens only when
    // the wall draws no ring. Palettes are configured exactly as WallSession does (MarkerVisualSettings.ApplyPalettes).
    public class CardStatusRuleTests
    {
        private static WallConfigData Shipped(string mode)
        {
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            if (mode != null) config.marker_outline_mode = mode;
            return config;
        }

        private static MarkerVisualSettings LookOf(WallConfigData config)
        {
            MarkerVisualSettings.ApplyPalettes(config);
            return MarkerVisualSettings.Resolve(config, null);
        }

        private static POIData Poi(WallConfigData config, string id) => config.pois.Single(p => p.id == id);

        [TearDown]
        public void ResetPalettes()
        {
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();
        }

        [TestCase("uniform"), TestCase("per_type"), TestCase("same_hue"), TestCase("none")]
        public void EveryShippedPoi_TheCardShowsExactlyItsMarkersRing_InEveryOutlineMode(string mode)
        {
            var config = Shipped(mode);
            var look = LookOf(config);
            int withStatus = 0;
            foreach (var poi in config.pois)
            {
                var card = CardStatusRule.Resolve(poi, look, config);
                var marker = MarkerVisualResolver.Resolve(poi, look);
                Assert.AreEqual(poi.has_status, card.Shows, poi.id + ": shows only with a status");
                if (!poi.has_status) continue;
                withStatus++;
                Assert.AreEqual(marker.ShowRing, card.FromWall, poi.id + ": wall colour exactly when the marker draws a ring");
                if (marker.ShowRing)
                {
                    Color ring = marker.RingUsesCategoryHue ? marker.RingHueColor : marker.RingLevel.RingColor;
                    Assert.AreEqual(ring, card.Color, poi.id + " (" + mode + "): the card's colour is the marker ring's");
                    Assert.AreEqual(marker.RingLevel.RingSpriteKey, card.LineStyle, poi.id + ": the ring's line style");
                }
            }
            Assert.Greater(withStatus, 10, "not vacuous: the shipped wall has many POIs with a status");
        }

        [Test]
        public void TheShippedWall_Uniform_IsTheWallsGold_WithEachRowsOwnDash_NeverTheGreenToRedTokens()
        {
            var config = Shipped(null);
            Assert.AreEqual("uniform", config.marker_outline_mode, "precondition: the shipped wall is uniform");
            var look = LookOf(config);
            ColorUtility.TryParseHtmlString(config.outline_uniform_color_hex, out var gold);

            var partial = CardStatusRule.Resolve(Poi(config, "lamp_military"), look, config);
            Assert.IsTrue(partial.FromWall);
            Assert.AreEqual(gold, partial.Color, "the uniform gold");
            Assert.AreEqual("dash_long", partial.LineStyle, "Partial Damage's own dash");
            Assert.AreEqual("Partial Damage", partial.LevelName, "the row's name, never its key");

            var destroyed = CardStatusRule.Resolve(Poi(config, "lamp_economic"), look, config);
            Assert.AreEqual(gold, destroyed.Color, "uniform: the same gold");
            Assert.AreEqual("dash_short", destroyed.LineStyle, "...told apart by the dash, as on the markers");
        }

        [Test]
        public void PerType_EachRowsOwnColour_AndAnUnsetColourFallsBackLikeTheMarkers()
        {
            var config = Shipped("per_type");
            var look = LookOf(config);
            var unknown = CardStatusRule.Resolve(Poi(config, "lamp_infrastructure"), look, config);
            Assert.IsTrue(unknown.Unknown);
            ColorUtility.TryParseHtmlString("#71717A", out var grey);
            Assert.AreEqual(grey, unknown.Color, "the Unknown row's own colour");
            Assert.AreEqual("dotted", unknown.LineStyle);
            Assert.AreEqual("Unknown", unknown.LevelName);
            var intact = CardStatusRule.Resolve(Poi(config, "lamp"), look, config);
            Assert.AreEqual(StatusRamp.Levels[0].RingColor, intact.Color, "a row with no colour uses the stock ramp, like its marker");
        }

        [Test]
        public void SameHue_ShadesThePoisOwnCategory()
        {
            var config = Shipped("same_hue");
            var look = LookOf(config);
            var military = CardStatusRule.Resolve(Poi(config, "lamp_military"), look, config);
            var residential = CardStatusRule.Resolve(Poi(config, "lamp_residential"), look, config);
            Assert.AreEqual(Poi(config, "lamp_military").status_level_key, Poi(config, "lamp_residential").status_level_key, "precondition: same row");
            Assert.AreNotEqual(military.Color, residential.Color, "same row, different categories: different shades");
        }

        [Test]
        public void NoRingOnTheWall_TheCardFallsBackToItsTokens()
        {
            var none = Shipped("none");
            var status = CardStatusRule.Resolve(Poi(none, "lamp_military"), LookOf(none), none);
            Assert.IsTrue(status.Shows);
            Assert.IsFalse(status.FromWall, "outline mode none: no ring colour to borrow");
            Assert.AreEqual(1, status.TokenStep, "20% -> --ts-status-1");

            var noRows = Shipped(null);
            noRows.outline_levels.Clear();
            var unknown = CardStatusRule.Resolve(Poi(noRows, "lamp_infrastructure"), LookOf(noRows), noRows);
            Assert.IsFalse(unknown.FromWall, "no Outline Types rows: tokens");
            Assert.AreEqual(CardStatusRule.UnknownTokenStep, unknown.TokenStep);
            Assert.AreEqual("dotted", unknown.LineStyle, "unknown keeps a dotted line, never colour alone");
            Assert.AreEqual("", unknown.LevelName, "no row: no name (never the stored key)");
        }

        [Test]
        public void NoStatus_ShowsNothing()
        {
            var config = Shipped(null);
            Assert.IsFalse(CardStatusRule.Resolve(Poi(config, "dev_marker_nostatus"), LookOf(config), config).Shows);
        }

        [Test]
        public void TheScale_IsEveryKnownRowByDamage_UnknownLeftOut_EachColouredByTheMarkersRule()
        {
            var config = Shipped("per_type");
            var look = LookOf(config);
            var scale = CardStatusRule.Scale(Poi(config, "lamp_military"), look, config);
            CollectionAssert.AreEqual(new[] { "intact", "partial_damage", "destroyed" }, scale.Select(s => s.Key), "by pct, no unknown row");
            foreach (var (key, step) in scale)
            {
                var probe = new POIData { id = "p", category = "military", has_status = true, status_level_key = key, status_pct = step.Pct };
                Assert.AreEqual(MarkerVisualResolver.Resolve(probe, look).RingLevel.RingColor, step.Color, key + ": the marker rule's colour for that row");
            }
        }
    }
}
