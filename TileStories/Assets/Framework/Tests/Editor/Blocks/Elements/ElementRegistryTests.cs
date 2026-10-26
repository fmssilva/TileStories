using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The element registry and the v1 elements (_3.3 R14.1, R14.2, R14.8): the built-in set is the guide's list in its order and
    // categories; every element registers, and a NEW row of each is born filled with its defaults with only its required words left to
    // mark; the registry refuses each broken definition (one test case per rule, from a valid definition with one thing broken); an app
    // registers its own element like a framework one; and BlockRegistry knows what an Elements field may be -- the shipped kinds unchanged.
    public class ElementRegistryTests
    {
        // The v1 list of R14.2, typed once from the guide: Text, then Media, then Structure
        private static readonly (string key, ElementCategory category)[] V1 =
        {
            ("title", ElementCategory.Text), ("eyebrow", ElementCategory.Text), ("heading", ElementCategory.Text), ("lede", ElementCategory.Text),
            ("paragraph", ElementCategory.Text), ("callout", ElementCategory.Text), ("pull_quote", ElementCategory.Text), ("list", ElementCategory.Text),
            ("picture", ElementCategory.Media), ("picture_pair", ElementCategory.Media), ("carousel", ElementCategory.Media), ("video", ElementCategory.Media),
            ("divider", ElementCategory.Structure), ("spacer", ElementCategory.Structure),
        };

        [Test]
        public void TheBuiltInSet_IsTheGuidesV1List_InItsOrderAndCategories()
        {
            CollectionAssert.AreEqual(V1.Select(e => e.key + ":" + e.category),
                ElementRegistry.Shared.All.Select(e => e.Key + ":" + e.Category));
        }

        [Test]
        public void TheGuidesStyleOptions_AreOffered_WhereR14_2NamesThem()
        {
            // - Text: tone + size on every text element, drop cap on the paragraph; Media: width + frame; the divider's line / azulejo
            foreach (var e in ElementRegistry.Shared.All.Where(e => e.Category == ElementCategory.Text))
            {
                Assert.IsNotNull(e.Field(BuiltInElements.ToneStyle), e.Key + " offers a tone");
                Assert.IsNotNull(e.Field(BuiltInElements.SizeStyle), e.Key + " offers a size");
                CollectionAssert.AreEqual(new[] { "neutral", "family", "accent", "inverse" }, e.Field(BuiltInElements.ToneStyle).Options, e.Key);
            }
            foreach (var e in ElementRegistry.Shared.All.Where(e => e.Category == ElementCategory.Media))
            {
                CollectionAssert.AreEqual(new[] { "inset", "full_bleed" }, e.Field(BuiltInElements.WidthStyle).Options, e.Key);
                CollectionAssert.AreEqual(new[] { "rectangle", "arch" }, e.Field(BuiltInElements.FrameStyle).Options, e.Key);
            }
            Assert.IsNotNull(Element(BuiltInElements.Paragraph).Field(BuiltInElements.DropCapStyle));
            CollectionAssert.AreEqual(new[] { "line", "azulejo" }, Element(BuiltInElements.Divider).Field(BuiltInElements.OrnamentStyle).Options);
            // - motion: pictures may pan or zoom; text never does (it would travel under the reader's eye)
            Assert.IsTrue(Element(BuiltInElements.Picture).OffersMotion(ElementOptions.MotionPan));
            Assert.IsFalse(Element(BuiltInElements.Paragraph).OffersMotion(ElementOptions.MotionParallax));
            // - tap: a picture opens the viewer or nothing, a video plays in place or opens the player, a carousel only swipes
            CollectionAssert.AreEqual(new[] { "open_viewer", "none" }, Element(BuiltInElements.Picture).TapChoices);
            CollectionAssert.AreEqual(new[] { "play_in_place", "open_player" }, Element(BuiltInElements.Video).TapChoices);
            Assert.IsNull(Element(BuiltInElements.Carousel).TapChoices);
        }

        [TestCaseSource(nameof(V1Keys))]
        public void ANewRow_IsBornFilledWithTheElementsDefaults_OnlyItsRequiredWordsLeftToMark(string key)
        {
            var element = Element(key);
            var block = new BlockInstanceData { key = "block_1", kind = PaintingStory.StoryKindKey };
            var row = ElementRow.Create(block, element);

            Assert.AreEqual("el_1", row.Id);
            Assert.AreEqual(key, row.Kind);
            foreach (var style in element.StyleFields)
                Assert.AreEqual(style.ChoiceDefault, row.Value(style.Key), key + ": style '" + style.Key + "' stored at its default");
            Assert.AreEqual(ElementOptions.MotionAuto, row.Value(ElementRow.MotionKey));
            Assert.AreEqual(ElementOptions.StrengthSubtle, row.Value(ElementRow.StrengthKey));
            Assert.AreEqual(element.TapChoices?.First() ?? "", row.Tap, key + ": the element's first tap, none where it has no choice");
            Assert.IsFalse(row.Validated, "a new row starts to be validated (R14.8)");

            // - the empty fields are MARKED, never filled with invented words: exactly the required ones, plus a media list below its minimum
            var page = PaintingStory.Value(block, PaintingStory.PageField);
            page.items.Add(row.Data);
            var problems = ElementValidation.Of(new ElementScope(block, PaintingStory.StoryKind(), null, null, null), ElementRegistry.Shared);
            var expected = (element.Fields ?? Array.Empty<BlockFieldDefinition>()).Where(f => f.Required).Select(f => "MissingRequired:" + f.Key).ToList();
            if (element.MinMedia > 0) expected.Add("MediaCount:");
            CollectionAssert.AreEquivalent(expected, problems.Select(p => p.Kind + ":" + p.FieldKey));
        }

        private static IEnumerable<string> V1Keys() => V1.Select(e => e.key);

        // One valid definition and a way to break it, per Validate rule
        private static IEnumerable<TestCaseData> Breaks()
        {
            yield return Case("no key", d => d.Key = " ", "no key");
            yield return Case("no name", d => d.DisplayName = "", "no display name");
            yield return Case("unknown icon", d => d.IconKey = "castle", "is not one of the card's icons");
            yield return Case("a reserved key", d => d.Fields = new[] { Text("media") }, "takes a key every row keeps");
            yield return Case("an id field", d => d.Fields = new[] { Text("id") }, "takes a key every row keeps");
            yield return Case("an Items sub-field", d => d.Fields = new[] { new BlockFieldDefinition { Key = "rows", Type = BlockFieldType.Items, ItemFields = new[] { Text("a") } } },
                "inside an item");
            yield return Case("an Elements sub-field", d => d.Fields = new[] { new BlockFieldDefinition { Key = "more", Type = BlockFieldType.Elements } }, "inside a row");
            yield return Case("a style that is not a Choice", d => d.StyleFields = new[] { Text("tint") }, "is not a closed list");
            yield return Case("a style with no default", d => d.StyleFields = new[] { new BlockFieldDefinition { Key = "tint", Type = BlockFieldType.Choice, Options = new[] { "a", "b" } } },
                "is not a closed list");
            yield return Case("a style key taken by content", d => d.StyleFields = new[] { Choice("text") }, "two fields are keyed 'text'");
            yield return Case("media numbers without media", d => d.MinMedia = 1, "holds no media but sets");
            yield return Case("an empty media range", d => { d.Media = MediaKind.Image; d.MinMedia = 3; d.MaxMedia = 2; }, "is not a range");
            yield return Case("a picture role without pictures", d => d.PreviewRole = ElementPreviewRole.Picture, "plays the preview's picture");
            yield return Case("no Auto among the motions", d => d.MotionChoices = new[] { ElementOptions.MotionNone }, "do not offer Auto");
            yield return Case("an unknown motion", d => d.MotionChoices = new[] { ElementOptions.MotionAuto, "spin" }, "its motion 'spin'");
            yield return Case("Auto playing Auto", d => d.AutoMotion = ElementOptions.MotionAuto, "its Auto motion");
            yield return Case("Auto playing an effect not offered", d => d.AutoMotion = ElementOptions.MotionPan, "its Auto motion");
            yield return Case("an unknown tap", d => d.TapChoices = new[] { "zoom_in" }, "its tap 'zoom_in'");
            yield return Case("an empty tap list", d => d.TapChoices = new string[0], "lists no tap");
            yield return Case("a search field that is not text", d => d.SearchFields = new[] { "size" }, "is not one of its text fields");
        }

        private static TestCaseData Case(string name, Action<ElementKindDefinition> breakIt, string expected) =>
            new TestCaseData(breakIt, expected).SetName("Validate_Refuses_" + name.Replace(' ', '_'));

        [TestCaseSource(nameof(Breaks))]
        public void Validate_RefusesEachBrokenDefinition_WithItsReason(Action<ElementKindDefinition> breakIt, string expected)
        {
            Assert.IsNull(ElementRegistry.Validate(Valid()), "precondition: the unbroken definition is valid");
            var broken = Valid();
            breakIt(broken);
            string problem = ElementRegistry.Validate(broken);
            StringAssert.Contains(expected, problem ?? "(accepted)");
            var registry = new ElementRegistry();
            Assert.Throws<ArgumentException>(() => registry.Register(broken));
            Assert.AreEqual(0, registry.All.Count, "a refused element leaves the registry as it was");
        }

        [Test]
        public void Register_RefusesNullAndASecondElementWithTheSameKey()
        {
            var registry = new ElementRegistry();
            Assert.Throws<ArgumentException>(() => registry.Register(null));
            registry.Register(Valid());
            var ex = Assert.Throws<ArgumentException>(() => registry.Register(Valid()));
            StringAssert.Contains("already registered", ex.Message);
            Assert.AreEqual(1, registry.All.Count);
        }

        [Test]
        public void AnApp_RegistersItsOwnElement_AndItIsCheckedAndReadLikeAFrameworkOne()
        {
            // - an app's own element (a map snippet: one picture, its caption searchable) beside the framework's, in a registry of its own
            var registry = new ElementRegistry();
            BuiltInElements.Register(registry);
            var map = new ElementKindDefinition
            {
                Key = "map_snippet", DisplayName = "Map Snippet", IconKey = CardIcons.Location, Category = ElementCategory.Media,
                Help = "A small map.", Media = MediaKind.Image, MinMedia = 1, MaxMedia = 1, SecondsPerMedia = 5f, SearchesCaptions = true,
                MotionChoices = new[] { ElementOptions.MotionAuto, ElementOptions.MotionNone }, AutoMotion = ElementOptions.MotionNone,
                PreviewRole = ElementPreviewRole.Picture,
            };
            registry.Register(map);

            Assert.AreEqual(V1.Length + 1, registry.All.Count);
            Assert.IsTrue(registry.TryGet("map_snippet", out var found));
            Assert.AreSame(map, found);
            Assert.IsFalse(ElementRegistry.Shared.TryGet("map_snippet", out _), "the framework's registry did not learn it");

            var block = new BlockInstanceData { key = "block_1", kind = PaintingStory.StoryKindKey };
            var row = ElementRow.Create(block, map);
            row.AddMedia("images/paco_hero.jpg", new List<LocalizedEntry> { new() { lang = "en", value = "Where the palace stood" } }, "");
            PaintingStory.Value(block, PaintingStory.PageField).items.Add(row.Data);
            var scope = new ElementScope(block, PaintingStory.StoryKind(), null, null, null);
            Assert.AreEqual(0, ElementValidation.Of(scope, registry).Count, "its row is complete");
            Assert.AreEqual(1, ElementRules.Minutes(scope, registry, "en", "en"), "its weight counts (5 s -> 1 min)");
            CollectionAssert.Contains(ElementRules.SearchWords(scope, registry), "Where the palace stood");
            Assert.AreEqual(1, ElementPreviewRule.Of(scope, registry).Entries.Count, "it can be the preview's picture");
        }

        [Test]
        public void BlockRegistry_KnowsWhatAnElementsFieldMayBe_AndTheShippedKindsCarryNoSurfaceField()
        {
            var story = PaintingStory.StoryKind();
            Assert.IsNull(BlockRegistry.Validate(story));
            CollectionAssert.AreEqual(new[] { "heading", "minutes", "page", "preview_mode", "preview_rows", "component_overrides" }, story.Fields.Select(f => f.Key),
                "the common fields, the Page, then the Preview surface (Page first, R17.7)");

            var twoPages = PaintingStory.StoryKind();
            twoPages.Fields = new[] { Page("page"), Page("more") };
            StringAssert.Contains("a kind has one Page", BlockRegistry.Validate(twoPages));
            var nested = PaintingStory.StoryKind();
            nested.Fields = new[] { new BlockFieldDefinition { Key = "rows", Type = BlockFieldType.Items, Label = "Rows", ItemFields = new[] { Page("page") } } };
            StringAssert.Contains("inside a row", BlockRegistry.Validate(nested));
            var listed = PaintingStory.StoryKind();
            listed.Fields = new[] { new BlockFieldDefinition { Key = "page", Type = BlockFieldType.Elements, Label = "Page", ItemFields = new[] { Text("a") } } };
            StringAssert.Contains("lists item fields", BlockRegistry.Validate(listed));
            // - a surface key is the card's even on a kind with no Page (as 'minutes' is on a clip kind): with a Page it would be two fields
            var takesASurfaceKey = PaintingStory.StoryKind();
            takesASurfaceKey.Fields = new[] { new BlockFieldDefinition { Key = BlockSurfaces.PreviewModeField, Type = BlockFieldType.Toggle, Label = "x" } };
            StringAssert.Contains("takes a key every kind shares", BlockRegistry.Validate(takesASurfaceKey));
            var twiceWithAPage = PaintingStory.StoryKind();
            twiceWithAPage.Fields = new[] { Page("page"), new BlockFieldDefinition { Key = BlockSurfaces.PreviewModeField, Type = BlockFieldType.Toggle, Label = "x" } };
            StringAssert.Contains("two fields are keyed 'preview_mode'", BlockRegistry.Validate(twiceWithAPage));

            // - no shipped kind composes: each keeps exactly the fields it had, and BlockSurfaces sees no Page on it
            foreach (var kind in BlockRegistry.Shared.All)
            {
                Assert.IsFalse(BlockSurfaces.Composes(kind), kind.Key);
                Assert.IsFalse(kind.Fields.Any(f => BlockSurfaces.FieldKeys.Contains(f.Key)), kind.Key + " carries no surface field");
                Assert.IsFalse(kind.Fields.Any(f => f.Type == BlockFieldType.Elements), kind.Key);
            }
        }

        private static ElementKindDefinition Element(string key)
        {
            Assert.IsTrue(ElementRegistry.Shared.TryGet(key, out var e), "precondition: '" + key + "' is registered");
            return e;
        }

        // A small valid text element: one required text, one closed style, reveal on entry
        private static ElementKindDefinition Valid() => new()
        {
            Key = "note", DisplayName = "Note", IconKey = CardIcons.Info, Category = ElementCategory.Text, Help = "A note.",
            Fields = new[] { new BlockFieldDefinition { Key = "text", Type = BlockFieldType.LocalizedLongText, Label = "Text", Required = true } },
            StyleFields = new[] { Choice("size") },
            MotionChoices = new[] { ElementOptions.MotionAuto, ElementOptions.MotionNone, ElementOptions.MotionReveal }, AutoMotion = ElementOptions.MotionReveal,
            PreviewRole = ElementPreviewRole.Text, SearchFields = new[] { "text" },
        };

        private static BlockFieldDefinition Text(string key) => new() { Key = key, Type = BlockFieldType.LocalizedText, Label = key };

        private static BlockFieldDefinition Choice(string key) =>
            new() { Key = key, Type = BlockFieldType.Choice, Label = key, Options = new[] { "s", "m" }, ChoiceDefault = "m" };

        private static BlockFieldDefinition Page(string key) => new() { Key = key, Type = BlockFieldType.Elements, Label = key };
    }
}
