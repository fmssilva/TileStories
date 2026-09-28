using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The developer-only demo card's decisions (_3.1 step 12), from plain data: which builds may show it, which POI it asks for, which stop it
    // rests at, and when its request counts as new. Pure, so every combination is asserted here; PoiCardLiveUpdateTests proves the card really
    // opens (PlayMode, the real scene).
    public class CardDemoRuleTests
    {
        private static readonly List<POIData> Pois = new() { new POIData { id = "lamp" }, new POIData { id = "painting" } };

        private static CardDemoSettings Demo(bool enabled = true, string poi = "lamp", string stop = "half") => new() { enabled = enabled, poi_id = poi, stop = stop };

        [Test]
        public void IsAllowed_TheEditorAndDevelopmentBuilds_NeverAReleaseBuild()
        {
            Assert.IsTrue(CardDemoRule.IsAllowed(isEditor: true, isDebugBuild: false), "the Editor");
            Assert.IsTrue(CardDemoRule.IsAllowed(isEditor: false, isDebugBuild: true), "a development build");
            Assert.IsTrue(CardDemoRule.IsAllowed(isEditor: true, isDebugBuild: true));
            Assert.IsFalse(CardDemoRule.IsAllowed(isEditor: false, isDebugBuild: false), "a release build ignores a demo left ON in the shipped config");
        }

        [Test]
        public void ItIsOffByDefault_AndTheShippedWallShipsItOff()
        {
            Assert.IsFalse(new CardDemoSettings().enabled, "the schema default");
            Assert.IsFalse(new CardSettings().demo_card.enabled, "a fresh wall");
            foreach (string path in new[] { "Assets/Apps/LivingRoom/config.json", "Assets/StreamingAssets/LivingRoom/config.json" })
            {
                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));
                Assert.IsFalse(config.card_settings.demo_card.enabled, path + ": the shipped config has the demo card OFF");
            }
        }

        [Test]
        public void PoiToOpen_OnlyWhenOn_Allowed_APointIsPicked_AndItIsOnTheWall()
        {
            Assert.AreEqual("lamp", CardDemoRule.PoiToOpen(Demo(), true, Pois));
            Assert.IsNull(CardDemoRule.PoiToOpen(Demo(enabled: false), true, Pois), "switched off");
            Assert.IsNull(CardDemoRule.PoiToOpen(Demo(), false, Pois), "not allowed in this build");
            Assert.IsNull(CardDemoRule.PoiToOpen(Demo(poi: ""), true, Pois), "no point picked");
            Assert.IsNull(CardDemoRule.PoiToOpen(Demo(poi: null), true, Pois), "no point picked (null)");
            Assert.IsNull(CardDemoRule.PoiToOpen(Demo(poi: "gone"), true, Pois), "a point the wall no longer has");
            Assert.IsNull(CardDemoRule.PoiToOpen(null, true, Pois), "no settings at all");
            Assert.IsNull(CardDemoRule.PoiToOpen(Demo(), true, null), "no POIs yet (the wall has not spawned)");
            Assert.AreEqual("painting", CardDemoRule.PoiToOpen(Demo(poi: "painting"), true, Pois));
        }

        [Test]
        public void StopOf_ReadsEveryDemoStop_AndAnythingElseOpensAtHalf()
        {
            Assert.AreEqual(SheetStopRule.Stop.Peek, CardDemoRule.StopOf(Demo(stop: CardOptions.StopPeek)));
            Assert.AreEqual(SheetStopRule.Stop.Half, CardDemoRule.StopOf(Demo(stop: CardOptions.StopHalf)));
            Assert.AreEqual(SheetStopRule.Stop.Full, CardDemoRule.StopOf(Demo(stop: CardOptions.StopFull)));
            Assert.AreEqual(SheetStopRule.Stop.Half, CardDemoRule.StopOf(Demo(stop: "sideways")), "an unknown word");
            Assert.AreEqual(SheetStopRule.Stop.Half, CardDemoRule.StopOf(null));
            foreach (string stop in CardOptions.DemoStops)
                Assert.AreNotEqual(SheetStopRule.Stop.Dismissed, CardDemoRule.StopOf(Demo(stop: stop)), stop + " is a stop the card rests at");
        }

        [Test]
        public void Request_IsEmptyWhenNothingIsAsked_AndChangesWithThePointAndTheStop()
        {
            Assert.AreEqual("", CardDemoRule.Request(Demo(enabled: false), true, Pois));
            Assert.AreEqual("", CardDemoRule.Request(Demo(), false, Pois));
            string lampHalf = CardDemoRule.Request(Demo(), true, Pois);
            Assert.IsNotEmpty(lampHalf);
            Assert.AreEqual(lampHalf, CardDemoRule.Request(Demo(), true, Pois), "the same request twice is the same text: the card is not reopened");
            Assert.AreNotEqual(lampHalf, CardDemoRule.Request(Demo(stop: "full"), true, Pois), "another stop is a new request");
            Assert.AreNotEqual(lampHalf, CardDemoRule.Request(Demo(poi: "painting"), true, Pois), "another point is a new request");
        }

        [Test]
        public void DemoStops_AreThePeekHalfAndFullTheCardKnows_OneMoreThanTheWallsOpenAt()
        {
            CollectionAssert.AreEqual(new[] { "peek", "half", "full" }, CardOptions.DemoStops);
            CollectionAssert.IsSubsetOf(CardOptions.OpenStops, CardOptions.DemoStops);
            CollectionAssert.DoesNotContain(CardOptions.OpenStops, CardOptions.StopFull, "the wall's own Open At never opens the full card");
        }
    }
}
