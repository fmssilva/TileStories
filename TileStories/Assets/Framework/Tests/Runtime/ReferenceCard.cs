using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // The card fixtures in ONE place (00-process "Real fixtures, one home"; _Test_Suite_2.md 1.3): the GOLDEN card -- the Ribeira Palace,
    // every registered kind at its default look with real content, what every single-card test runs on -- and the other real cards a
    // test needs, each by its role. A test reaches a block by its kind (BlockOf), read from the shipped config, never by a typed
    // "block_N": adding, moving or removing a block edits the config only. ReferenceCardTests keeps the golden card whole.
    public static class ReferenceCard
    {
        public const string ConfigPath = "Assets/Apps/LivingRoom/config.json";
        // The golden card (the developer's pick): every single-card test
        public const string Golden = "painting";
        // The second real card: two cards at once (audio switch / queue, compare_points' other side, a marker tap swapping cards)
        public const string SecondCard = "lamp";

        // The edge cards, each named for its edge
        public static class Edge
        {
            // A video loop for a hero; a card without most kinds
            public const string VideoLoopHero = "lamp_military";
            // A turntable model for a hero
            public const string TurntableHero = "lamp_religious";
            // No header; a nav_contents block; video siblings
            public const string NoHeader = "lamp_residential";
            // One audio guide alone: the second audio for switch / queue
            public const string AudioOnly = "lamp_economic";
            // A point with no card, its status unknown
            public const string NoCard = "lamp_infrastructure";
            // An empty point: the Card Starter's "from scratch"
            public const string EmptyPoint = "camera";
        }

        // The registered kinds the golden card leaves out, each with its reason; ReferenceCardTests fails on a stale line
        public static readonly IReadOnlyDictionary<string, string> KeptOffTheGoldenCard = new Dictionary<string, string>
        {
            { BuiltInBlocks.NavContentsKind, "Contents, kept for a later rebuild (_3.3 TODOs); its edge card is Edge.NoHeader" },
        };

        // A fresh copy of the shipped wall (a test may edit it)
        public static WallConfigData Config() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText(ConfigPath));

        // The golden card's point in `config` (a fresh shipped config when none is given)
        public static POIData GoldenPoi(WallConfigData config = null) => (config ?? Config()).pois.Single(p => p.id == Golden);

        // The key of the golden card's block of `kind`: of `look` when one is named, else its one block of that kind -- the kind's
        // default look when the card carries several looks of it
        public static string BlockOf(string kind, string look = null) => BlockOf(GoldenPoi(), kind, look);

        // The same on any point (the second card, an edge card) of a config the test already holds
        public static string BlockOf(POIData poi, string kind, string look = null)
        {
            var blocks = (poi.card?.blocks ?? new List<BlockInstanceData>()).Where(b => b.kind == kind).ToList();
            if (look != null) blocks = blocks.Where(b => LookOf(b) == look).ToList();
            if (blocks.Count == 1) return blocks[0].key;
            if (blocks.Count > 1 && look == null && BlockRegistry.Shared.TryGet(kind, out var definition))
            {
                var atDefault = blocks.Where(b => LookOf(b) == definition.DefaultVariant).ToList();
                if (atDefault.Count == 1) return atDefault[0].key;
            }
            Assert.Fail($"{poi.name} has {blocks.Count} {kind} block(s){(look != null ? " in the " + look + " look" : "")}: " +
                (blocks.Count == 0 ? "add one to its card, or pick another kind" : "name a look"));
            return null;
        }

        // The look a block draws: its own, else its kind's default (an empty variant)
        public static string LookOf(BlockInstanceData block) =>
            !string.IsNullOrEmpty(block.variant) ? block.variant
                : BlockRegistry.Shared.TryGet(block.kind, out var definition) ? definition.DefaultVariant : "";
    }
}
