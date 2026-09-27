using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // BlockStackBuilder, BlockLibraryRule and BlockRegistry (_3.1 section 3): which blocks a POI's card shows, in
    // which order and variant, and why any other is left out. Kinds are registered through the public
    // BlockRegistry API into a registry of the test's own (the extension point an app uses), so the shared
    // registry is never touched.
    public class BlockStackBuilderTests
    {
        private sealed class PlainView : IBlockView
        {
            public VisualElement Root { get; } = new();
            public void Bind(BlockInstanceData instance, BlockBindContext context) { }
            public void Unbind() { }
        }

        private static BlockKindDefinition Kind(string key, string family = "about", bool requiredBody = false) => new()
        {
            Key = key,
            Family = family,
            DisplayName = key,
            Variants = new[] { "plain", "fancy" },
            DefaultVariant = "plain",
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition { Key = "title", Type = BlockFieldType.LocalizedText, Required = key == BuiltInBlocks.HeaderKind },
                new BlockFieldDefinition { Key = "body", Type = BlockFieldType.LocalizedLongText, Required = requiredBody },
            },
        };

        private static BlockRegistry Registry()
        {
            var r = new BlockRegistry();
            r.Register(Kind(BuiltInBlocks.HeaderKind), () => new PlainView());
            r.Register(Kind("rich_text", requiredBody: true), () => new PlainView());
            r.Register(Kind("fun_fact"), () => new PlainView());
            return r;
        }

        private static BlockInstanceData Block(string key, string kind, string variant = "", string title = null, string body = null)
        {
            var b = new BlockInstanceData { key = key, kind = kind, variant = variant };
            if (title != null) b.fields.Add(new BlockFieldValue { key = "title", text = new List<LocalizedEntry> { new() { lang = "en", value = title } } });
            if (body != null) b.fields.Add(new BlockFieldValue { key = "body", text = new List<LocalizedEntry> { new() { lang = "pt", value = body } } });
            return b;
        }

        private static POIData Poi(params BlockInstanceData[] blocks)
        {
            var poi = new POIData { id = "p", name = "North Tower", summary = "Built in 1640." };
            poi.card.blocks.AddRange(blocks);
            return poi;
        }

        private static List<string> Keys(BlockStackBuilder.Result r) => r.Entries.Select(e => e.Instance.key).ToList();

        [Test]
        public void AnEmptyCard_ShowsOnlyAHeader_MadeFromTheNameAndSummary()
        {
            var r = BlockStackBuilder.Build(Poi(), new CardSettings(), Registry());
            Assert.AreEqual(1, r.Entries.Count);
            var header = r.Entries[0];
            Assert.IsTrue(header.Synthesized);
            Assert.AreEqual(BuiltInBlocks.HeaderKind, header.Definition.Key);
            Assert.AreEqual("plain", header.Variant, "the kind's default variant");
            var read = new BlockFieldReader(header.Instance, "pt", "en");
            Assert.AreEqual("North Tower", read.Text(BlockStackBuilder.HeaderTitleField), "the name, written in the fallback language, read in any");
            Assert.AreEqual("Built in 1640.", read.Text(BlockStackBuilder.HeaderSubtitleField));
            CollectionAssert.IsEmpty(r.Skipped);
        }

        [Test]
        public void AuthoredBlocks_KeepTheirOrder_AfterTheOneHeader()
        {
            var r = BlockStackBuilder.Build(Poi(Block("block_1", "fun_fact", title: "Did you know"), Block("block_2", "rich_text", body: "Text"),
                Block("block_3", BuiltInBlocks.HeaderKind, title: "Castle")), new CardSettings(), Registry());
            CollectionAssert.AreEqual(new[] { "block_3", "block_1", "block_2" }, Keys(r), "the authored header moves to the top; the rest keep their order");
            Assert.IsFalse(r.Entries[0].Synthesized);
        }

        [Test]
        public void ASecondHeader_IsSkipped_WithItsReason()
        {
            var r = BlockStackBuilder.Build(Poi(Block("block_1", BuiltInBlocks.HeaderKind, title: "One"), Block("block_2", BuiltInBlocks.HeaderKind, title: "Two")),
                new CardSettings(), Registry());
            CollectionAssert.AreEqual(new[] { "block_1" }, Keys(r));
            Assert.AreEqual(BlockStackBuilder.SkipReason.ExtraHeader, r.Skipped.Single().Reason);
        }

        [Test]
        public void AnUnknownKind_IsSkipped_AndTheCardStillShows()
        {
            var r = BlockStackBuilder.Build(Poi(Block("block_1", "hologram", title: "x"), Block("block_2", "fun_fact")), new CardSettings(), Registry());
            CollectionAssert.AreEqual(new[] { "", "block_2" }, Keys(r), "synthesized header + the known block");
            Assert.AreEqual("block_1", r.Skipped.Single().Instance.key);
            Assert.AreEqual(BlockStackBuilder.SkipReason.UnknownKind, r.Skipped.Single().Reason);
        }

        [Test]
        public void AKindSwitchedOffInTheBlockLibrary_IsSkipped_ButTheHeaderCannotBe()
        {
            var settings = new CardSettings();
            settings.kinds.Add(new BlockKindSetting { kind = "fun_fact", enabled = false });
            settings.kinds.Add(new BlockKindSetting { kind = BuiltInBlocks.HeaderKind, enabled = false });
            var r = BlockStackBuilder.Build(Poi(Block("block_1", "fun_fact"), Block("block_2", BuiltInBlocks.HeaderKind, title: "Castle")), settings, Registry());
            CollectionAssert.AreEqual(new[] { "block_2" }, Keys(r), "the header is the card's identity: always on");
            Assert.AreEqual(BlockStackBuilder.SkipReason.KindDisabled, r.Skipped.Single().Reason);
            Assert.IsFalse(BlockLibraryRule.IsEnabled(settings, "fun_fact"));
            Assert.IsTrue(BlockLibraryRule.IsEnabled(settings, "rich_text"), "a kind with no row is on");
        }

        [Test]
        public void AnEmptyRequiredField_HidesTheBlock_WithTheFieldNamed()
        {
            var r = BlockStackBuilder.Build(Poi(Block("block_1", "rich_text", body: "  "), Block("block_2", BuiltInBlocks.HeaderKind)),
                new CardSettings(), Registry());
            Assert.AreEqual(2, r.Skipped.Count, "a blank body and a header with no title");
            Assert.IsTrue(r.Skipped.All(s => s.Reason == BlockStackBuilder.SkipReason.MissingRequired));
            CollectionAssert.AreEquivalent(new[] { "body", "title" }, r.Skipped.Select(s => s.FieldKey));
            Assert.IsTrue(r.Entries.Single().Synthesized, "the unusable authored header falls back to name + summary");
        }

        [Test]
        public void AMissingOrUnknownVariant_UsesTheBlockLibraryDefault_ThenTheKindDefault()
        {
            var settings = new CardSettings();
            var r = BlockStackBuilder.Build(Poi(Block("block_1", "fun_fact", "fancy"), Block("block_2", "fun_fact", "neon"), Block("block_3", "fun_fact")), settings, Registry());
            CollectionAssert.AreEqual(new[] { "fancy", "plain", "plain" }, r.Entries.Skip(1).Select(e => e.Variant));

            settings.kinds.Add(new BlockKindSetting { kind = "fun_fact", default_variant = "fancy" });
            r = BlockStackBuilder.Build(Poi(Block("block_2", "fun_fact", "neon")), settings, Registry());
            Assert.AreEqual("fancy", r.Entries[1].Variant, "the Block Library's default");

            settings.kinds[0].default_variant = "gone";
            Assert.AreEqual("plain", BlockLibraryRule.DefaultVariant(settings, Kind("fun_fact")), "a library default the kind lacks falls back to the kind's own");
        }

        [Test]
        public void WithoutAHeaderKindRegistered_NothingIsInvented()
        {
            var r = BlockStackBuilder.Build(Poi(), new CardSettings(), new BlockRegistry());
            CollectionAssert.IsEmpty(r.Entries);
        }

        [Test]
        public void TheRegistry_RefusesABrokenOrDuplicateKind_WithTheReason()
        {
            var r = new BlockRegistry();
            r.Register(Kind("fun_fact"), () => new PlainView());
            Assert.Throws<System.ArgumentException>(() => r.Register(Kind("fun_fact"), () => new PlainView()), "a duplicate key");

            var noDefault = Kind("a"); noDefault.DefaultVariant = "gone";
            StringAssert.Contains("default variant", BlockRegistry.Validate(noDefault));
            var dupField = Kind("b"); dupField.Fields = new[] { new BlockFieldDefinition { Key = "x" }, new BlockFieldDefinition { Key = "x" } };
            StringAssert.Contains("two fields", BlockRegistry.Validate(dupField));
            var choice = Kind("c"); choice.Fields = new[] { new BlockFieldDefinition { Key = "x", Type = BlockFieldType.Choice } };
            StringAssert.Contains("no options", BlockRegistry.Validate(choice));
            var nested = Kind("d");
            nested.Fields = new[] { new BlockFieldDefinition { Key = "rows", Type = BlockFieldType.Items, ItemFields = new[] {
                new BlockFieldDefinition { Key = "inner", Type = BlockFieldType.Items, ItemFields = new[] { new BlockFieldDefinition { Key = "t" } } } } } };
            StringAssert.Contains("cannot nest", BlockRegistry.Validate(nested));
            Assert.IsNull(BlockRegistry.Validate(Kind("e")));

            Assert.IsInstanceOf<PlainView>(r.CreateView("fun_fact"));
            Assert.IsNull(r.CreateView("hologram"));
            CollectionAssert.AreEqual(new[] { "fun_fact" }, r.All.Select(k => k.Key));
        }

        [Test]
        public void AKindWithNothingToShowForThisPoi_IsSkipped_WithItsOwnReason_AndKeptOnceItHasSomething()
        {
            var r = Registry();
            var status = Kind("status");
            status.ShowsFor = p => p.has_status;
            status.NotShownForPoiNote = "this point has no status.";
            r.Register(status, () => new PlainView());
            var poi = Poi(Block("block_2", "status"));
            poi.has_status = false;

            var skipped = BlockStackBuilder.Build(poi, new CardSettings(), r).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, skipped.Reason, "no empty ring for a POI without a status");
            Assert.AreEqual("Not shown: this point has no status.",
                POIEditorToolWindow.CardBlockSkipText(skipped.Reason, null, status.NotShownForPoiNote), "the Editor row says why, in the kind's words");

            poi.has_status = true;
            CollectionAssert.AreEqual(new[] { "", "block_2" }, Keys(BlockStackBuilder.Build(poi, new CardSettings(), r)), "with a status it is shown");
        }

        [Test]
        public void TheBuiltInStatusKind_ShowsOnlyForAPoiWithAStatus()
        {
            Assert.IsFalse(BuiltInBlocks.Status.ShowsFor(new POIData { has_status = false }));
            Assert.IsTrue(BuiltInBlocks.Status.ShowsFor(new POIData { has_status = true, status_unknown = true }), "unknown is still a status: the question mark");
            Assert.IsFalse(string.IsNullOrWhiteSpace(BuiltInBlocks.Status.NotShownForPoiNote));
        }
    }
}
