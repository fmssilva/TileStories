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

        private static BlockKindDefinition Kind(string key, string family = "about", bool requiredBody = false, BlockFieldDefinition[] fields = null) => new()
        {
            Key = key,
            Family = family,
            DisplayName = key,
            Variants = new[] { "plain", "fancy" },
            DefaultVariant = "plain",
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = fields ?? new[]
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
            var dupField = Kind("b", fields: new[] { new BlockFieldDefinition { Key = "x" }, new BlockFieldDefinition { Key = "x" } });
            StringAssert.Contains("two fields", BlockRegistry.Validate(dupField));
            // - the common heading is every kind's: a kind declaring its own "heading" clashes with it (6C)
            var ownHeading = Kind("h", fields: new[] { new BlockFieldDefinition { Key = BlockKindDefinition.HeadingField } });
            StringAssert.Contains("two fields are keyed 'heading'", BlockRegistry.Validate(ownHeading));
            var choice = Kind("c", fields: new[] { new BlockFieldDefinition { Key = "x", Type = BlockFieldType.Choice } });
            StringAssert.Contains("no options", BlockRegistry.Validate(choice));
            var nested = Kind("d", fields: new[] { new BlockFieldDefinition { Key = "rows", Type = BlockFieldType.Items, ItemFields = new[] {
                new BlockFieldDefinition { Key = "inner", Type = BlockFieldType.Items, ItemFields = new[] { new BlockFieldDefinition { Key = "t" } } } } } });
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
            status.ShowsFor = (p, _, _, _) => p.has_status;
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
            Assert.IsFalse(BuiltInBlocks.Status.ShowsFor(new POIData { has_status = false }, null, BuiltInBlocks.StatusRing, null));
            Assert.IsTrue(BuiltInBlocks.Status.ShowsFor(new POIData { has_status = true, status_unknown = true }, null, BuiltInBlocks.StatusRing, null), "unknown is still a status: the question mark");
            Assert.IsFalse(string.IsNullOrWhiteSpace(BuiltInBlocks.Status.NotShownForPoiNote));
        }

        // ---------------- compare_points: a kind that points at another POI ----------------

        [Test]
        public void ACompareBlock_ShowsOnlyWhenBothPointsHaveAStatus_AndTheOtherIsOnTheWall()
        {
            var here = new POIData { id = "north", name = "North Tower", has_status = true };
            var other = new POIData { id = "gate", name = "South Gate", has_status = true };
            var block = new BlockInstanceData { key = "block_2", kind = BuiltInBlocks.ComparePointsKind };
            block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.ComparePointsOtherField, value = "gate" });
            here.card.blocks.Add(block);
            var wall = new List<POIData> { here, other };
            BlockStackBuilder.Result Build(IReadOnlyList<POIData> pois) => BlockStackBuilder.Build(here, new CardSettings(), BlockRegistry.Shared, pois);

            Assert.AreEqual(BuiltInBlocks.ComparePointsKind, Build(wall).Entries.Last().Definition.Key, "both have a status: shown");
            Assert.AreSame(other, BuiltInBlocks.OtherPoi(block, wall));

            other.has_status = false;
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, Build(wall).Skipped.Single().Reason, "the other point has no status: no half pair");
            other.has_status = true;
            here.has_status = false;
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, Build(wall).Skipped.Single().Reason, "this point has no status");
            here.has_status = true;
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, Build(new List<POIData> { here }).Skipped.Single().Reason, "the other point was deleted");
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, Build(null).Skipped.Single().Reason, "no wall at hand: nothing resolves");

            block.fields.Clear();
            var none = Build(wall).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, none.Reason, "nothing picked: no other point either (ShowsFor runs first)");
            Assert.IsNull(BuiltInBlocks.OtherPoi(block, wall));
        }

        [Test]
        public void CardTitleOf_IsTheFirstAuthoredHeaderTitle_InTheLanguage_ElseTheName()
        {
            var poi = new POIData { id = "p", name = "Lamp - Military" };
            Assert.AreEqual("Lamp - Military", BlockStackBuilder.CardTitleOf(poi, "en", "en"), "no header: the name");
            var header = Block("block_1", BuiltInBlocks.HeaderKind, title: "Castle Keep");
            header.fields[0].text.Add(new LocalizedEntry { lang = "pt", value = "Torre de Menagem" });
            poi.card.blocks.Add(header);
            Assert.AreEqual("Torre de Menagem", BlockStackBuilder.CardTitleOf(poi, "pt", "en"));
            Assert.AreEqual("Castle Keep", BlockStackBuilder.CardTitleOf(poi, "es", "en"), "a missing language falls back");
            Assert.AreEqual("", BlockStackBuilder.CardTitleOf(null, "en", "en"));
        }

        // ---------------- item rows: required sub-fields ----------------

        private static BlockKindDefinition StepsKind() => new()
        {
            Key = "steps",
            Family = "about",
            DisplayName = "Steps",
            Variants = new[] { "plain" },
            DefaultVariant = "plain",
            DisplayModes = new[] { CardOptions.DisplayInline },
            Fields = new[]
            {
                new BlockFieldDefinition
                {
                    Key = "rows", Type = BlockFieldType.Items, Required = true, ItemFields = new[]
                    {
                        new BlockFieldDefinition { Key = "title", Type = BlockFieldType.LocalizedText, Required = true },
                        new BlockFieldDefinition { Key = "colour", Type = BlockFieldType.Color },
                        new BlockFieldDefinition { Key = "note", Type = BlockFieldType.LocalizedText },
                    },
                },
            },
        };

        private static BlockItemData Row(string title, string colour = null, string note = null)
        {
            var item = new BlockItemData();
            if (title != null) item.fields.Add(new BlockItemFieldValue { key = "title", text = new List<LocalizedEntry> { new() { lang = "pt", value = title } } });
            if (colour != null) item.fields.Add(new BlockItemFieldValue { key = "colour", value = colour });
            if (note != null) item.fields.Add(new BlockItemFieldValue { key = "note", text = new List<LocalizedEntry> { new() { lang = "en", value = note } } });
            return item;
        }

        private static BlockStackBuilder.Result BuildSteps(BlockKindDefinition kind, params BlockItemData[] rows)
        {
            var r = Registry();
            r.Register(kind, () => new PlainView());
            var block = new BlockInstanceData { key = "block_2", kind = kind.Key };
            if (rows != null) block.fields.Add(new BlockFieldValue { key = "rows", items = new List<BlockItemData>(rows) });
            return BlockStackBuilder.Build(Poi(block), new CardSettings(), r);
        }

        [Test]
        public void ARequiredItemsField_WithNoRows_IsMissing_WithRowsButNoneComplete_HasNoCompleteRow_AndOneCompleteRowShowsIt()
        {
            var noField = BuildSteps(StepsKind(), null).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.MissingRequired, noField.Reason, "no rows at all");
            Assert.AreEqual(BlockStackBuilder.SkipReason.MissingRequired, BuildSteps(StepsKind()).Skipped.Single().Reason, "an empty list too");

            var incomplete = BuildSteps(StepsKind(), Row(null, note: "only a note"), Row("  ")).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.NoCompleteRow, incomplete.Reason, "rows, but every one lacks its required title");
            Assert.AreEqual("rows", incomplete.FieldKey);

            var shown = BuildSteps(StepsKind(), Row(null), Row("Dig the clay"));
            CollectionAssert.IsEmpty(shown.Skipped, "one complete row is enough: the view leaves out the other");
            Assert.AreEqual("steps", shown.Entries.Last().Definition.Key);

            Assert.IsFalse(BlockFieldReader.ItemIsComplete(Row(null, note: "x"), StepsKind().Field("rows").ItemFields), "the title is required");
            Assert.IsTrue(BlockFieldReader.ItemIsComplete(Row("Fire it"), StepsKind().Field("rows").ItemFields), "an empty optional colour and note are fine");
            Assert.IsTrue(BlockFieldReader.ItemIsComplete(new BlockItemData(), null), "a field with no sub-field definitions: every row counts");
        }

        [Test]
        public void ARequiredColour_ThatIsNotAColour_MakesItsRowIncomplete()
        {
            var kind = StepsKind();
            kind.Field("rows").ItemFields[1].Required = true;
            Assert.AreEqual(BlockStackBuilder.SkipReason.NoCompleteRow, BuildSteps(kind, Row("Cobalt", "#12"), Row("Tin", "white")).Skipped.Single().Reason,
                "'#12' and 'white' are not colours: no complete row");
            CollectionAssert.IsEmpty(BuildSteps(kind, Row("Cobalt", "#12"), Row("Tin", "#F2EEE3")).Skipped, "one real colour");
        }

        [Test]
        public void TryParseColor_AcceptsOnlyTheHexFormsTheEditorWrites()
        {
            Assert.IsTrue(BlockFieldReader.TryParseColor("#1F3F8F", out var c));
            Assert.AreEqual(0x1F / 255f, c.r, 1e-4f);
            Assert.AreEqual(0x3F / 255f, c.g, 1e-4f);
            Assert.AreEqual(0x8F / 255f, c.b, 1e-4f);
            Assert.AreEqual(1f, c.a, "opaque");
            Assert.IsTrue(BlockFieldReader.TryParseColor(" #abc ", out c), "short form, lower case, spaces around");
            Assert.AreEqual(0xAA / 255f, c.r, 1e-4f);
            foreach (string bad in new[] { null, "", "  ", "1F3F8F", "#1F3F8", "#1F3F8FF0", "#GG0000", "red", "#12" })
                Assert.IsFalse(BlockFieldReader.TryParseColor(bad, out _), "'" + bad + "' is not a colour here");
        }
    }
}
