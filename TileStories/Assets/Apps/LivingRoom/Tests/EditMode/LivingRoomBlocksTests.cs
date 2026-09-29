using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TileStories.Editor;
using TileStories.Editor.Tests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.LivingRoom.Tests
{
    // The extension proof of the POI Detail Card (_3.1 step 11): the living room app adds its own block kind (size_comparison) and its own
    // card service through the Framework's PUBLIC registries, from its own assembly, with no Framework edit. What is proven here, on the
    // real registries, the real POI Editor window and the shipped config: the kind and the service are there in Edit Mode (no Play Mode),
    // the Editor lists and draws the kind, the builder judges its fields, its view reads the service through the context, and the
    // Framework assemblies never reference the app.
    public class LivingRoomBlocksTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string AppAssembly = "TileStories.LivingRoom";
        private const string AppFolder = "Assets/Apps/LivingRoom";
        private PoiEditorWindowHost _window;

        [TearDown]
        public void TearDown() => _window?.Close();

        private static System.Collections.Generic.List<LocalizedEntry> Words(string en, string pt = null)
        {
            var list = new System.Collections.Generic.List<LocalizedEntry> { new() { lang = "en", value = en } };
            if (pt != null) list.Add(new LocalizedEntry { lang = "pt", value = pt });
            return list;
        }

        // A size_comparison block with every field written
        private static BlockInstanceData FullBlock(string objectKey = FamiliarObjects.Smartphone, float width = 32f, float height = 58f)
        {
            var block = new BlockInstanceData { key = "block_2", kind = SizeComparisonBlock.Kind, variant = "", display = CardOptions.DisplayInline };
            block.fields.Add(new BlockFieldValue { key = BlockKindDefinition.HeadingField, text = Words("How big?", "Que tamanho?") });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.ObjectField, value = objectKey });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.WidthField, number = width });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.HeightField, number = height });
            block.fields.Add(new BlockFieldValue { key = SizeComparisonBlock.CaptionField, text = Words("Four phones tall.", "Quatro telem\u00f3veis de altura.") });
            return block;
        }

        private static POIData PoiWith(BlockInstanceData block)
        {
            var poi = new POIData { id = "poi_1", name = "North Tower" };
            poi.card.blocks.Add(block);
            return poi;
        }

        private static WallConfigData TwoPoiConfig()
        {
            var config = new WallConfigData { wall_id = "t" };
            config.pois.Add(new POIData { id = "poi_1", name = "North Tower" });
            config.pois.Add(new POIData { id = "poi_2", name = "South Gate" });
            return config;
        }

        private static WallConfigData ShippedConfig() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText(AppFolder + "/config.json"));

        // ---------------- registration ----------------

        [Test]
        public void TheKindAndItsService_AreRegisteredByTheAppsOwnStartup_InEditMode_WithNoPlayMode()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet(SizeComparisonBlock.Kind, out var definition), "the shared registry lists the app's kind");
            Assert.AreSame(SizeComparisonBlock.Definition, definition);
            Assert.IsNull(BlockRegistry.Validate(definition), "the registry accepts it: family, variants, fields");
            Assert.AreEqual("about", definition.Family);
            Assert.IsInstanceOf<SizeComparisonBlockView>(BlockRegistry.Shared.CreateView(SizeComparisonBlock.Kind), "the registered factory builds the app's view");
            Assert.IsInstanceOf<FamiliarObjects>(CardServices.Shared.Get<IFamiliarObjects>(), "the app's service is on the card's shared registry");

            var builtIn = new BlockRegistry();
            BuiltInBlocks.Register(builtIn);
            var all = BlockRegistry.Shared.All.Select(k => k.Key).ToList();
            Assert.AreEqual(builtIn.All.Count + 1, all.Count, "the app added exactly its one kind to the framework's");
            CollectionAssert.AreEqual(builtIn.All.Select(k => k.Key), all.Take(builtIn.All.Count), "the framework's kinds keep their order; the app's follows");
        }

        [Test]
        public void Register_FillsAnyRegistriesItIsGiven_AndAskingTwiceIsHarmless()
        {
            var blocks = new BlockRegistry();
            BuiltInBlocks.Register(blocks);
            var services = new CardServices();
            var texts = new CardStringSources();
            int before = blocks.All.Count;
            LivingRoomBlocks.Register(blocks, services, texts);
            Assert.AreEqual(before + 1, blocks.All.Count);
            var service = services.Get<IFamiliarObjects>();
            Assert.IsNotNull(service);
            Assert.IsTrue(texts.Has(LivingRoomCardTexts.AppName), "the app's card texts are registered under its name");
            var table = texts.All.Single().Table;
            Assert.DoesNotThrow(() => LivingRoomBlocks.Register(blocks, services, texts), "Play Mode after a script reload runs both startup entry points");
            Assert.AreEqual(before + 1, blocks.All.Count, "no second copy of the kind");
            Assert.AreSame(service, services.Get<IFamiliarObjects>(), "the first service instance stays");
            Assert.AreEqual(1, texts.All.Count, "no second copy of the table");
            Assert.AreSame(table, texts.All.Single().Table);
            Assert.DoesNotThrow(() => LivingRoomBlocks.Register(BlockRegistry.Shared, CardServices.Shared, CardStringSources.Shared));
            Assert.IsTrue(CardStringSources.Shared.Has(LivingRoomCardTexts.AppName), "the app's startup put its words on the card's shared registry, in Edit Mode");
        }

        // ---------------- the Framework never references the app ----------------

        [Test]
        public void NoFrameworkAssembly_ReferencesTheApp_NotCompiled_NotInAnAsmdef_NotInItsSource()
        {
            // - compiled: what each loaded framework assembly (runtime, Editor and both test assemblies) was built against
            var loaded = AppDomain.CurrentDomain.GetAssemblies();
            foreach (string name in new[] { "TileStories", "TileStories.Editor", "TileStories.Tests.Runtime", "TileStories.Editor.Tests" })
            {
                var assembly = loaded.FirstOrDefault(a => a.GetName().Name == name);
                Assert.IsNotNull(assembly, name + " is loaded");
                var references = assembly.GetReferencedAssemblies().Select(r => r.Name).ToList();
                Assert.IsFalse(references.Any(r => r.StartsWith(AppAssembly)), name + " must not reference the app: " + string.Join(", ", references.Where(r => r.StartsWith("TileStories"))));
            }
            var app = loaded.FirstOrDefault(a => a.GetName().Name == AppAssembly);
            Assert.IsNotNull(app, "the app's own assembly is loaded");
            Assert.IsTrue(app.GetReferencedAssemblies().Any(r => r.Name == "TileStories"), "the app references the framework: the one direction");

            // - the asmdefs: by name and by GUID
            string meta = File.ReadAllText(AppFolder + "/Scripts/" + AppAssembly + ".asmdef.meta");
            string guid = Regex.Match(meta, @"guid:\s*([0-9a-f]{32})").Groups[1].Value;
            Assert.AreEqual(32, guid.Length, "precondition: the app asmdef's GUID was read");
            var asmdefs = Directory.GetFiles("Assets/Framework", "*.asmdef", SearchOption.AllDirectories);
            Assert.GreaterOrEqual(asmdefs.Length, 4, "not vacuous: runtime, Editor and both test assemblies");
            foreach (string path in asmdefs)
            {
                string text = File.ReadAllText(path);
                StringAssert.DoesNotContain(AppAssembly, text, path);
                StringAssert.DoesNotContain(guid, text, path + " (by GUID)");
            }

            // - the source of the framework's shipped code (runtime + Editor): no name of the app's code
            int scanned = 0;
            foreach (string folder in new[] { "Assets/Framework/Runtime", "Assets/Framework/Editor" })
                foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
                {
                    scanned++;
                    string source = File.ReadAllText(file);
                    foreach (string name in new[] { "TileStories.LivingRoom", "LivingRoomBlocks", "SizeComparison", "FamiliarObject", "size_comparison" })
                        StringAssert.DoesNotContain(name, source, file + " names the app's code");
                }
            Assert.Greater(scanned, 200, "not vacuous: the whole shipped framework was scanned");
        }

        // ---------------- the kind's definition, the service and the pure rule ----------------

        [Test]
        public void TheObjectChoice_OffersExactlyTheServicesObjects_EachWithARealSize()
        {
            var choice = SizeComparisonBlock.Definition.Field(SizeComparisonBlock.ObjectField);
            CollectionAssert.AreEqual(FamiliarObjects.Keys, choice.Options, "one source: the table's keys");
            Assert.AreEqual(choice.Options.Count, choice.OptionLabels.Count, "a name for every option");
            var service = new FamiliarObjects();
            foreach (string key in FamiliarObjects.Keys)
            {
                Assert.IsTrue(service.TryGet(key, out var familiar), key);
                Assert.AreEqual(key, familiar.Key);
                Assert.Greater(familiar.WidthCm, 0f, key);
                Assert.Greater(familiar.HeightCm, 0f, key);
            }
            Assert.IsFalse(service.TryGet("giraffe", out _), "an object the app does not know");
            Assert.IsFalse(service.TryGet("", out _));
            Assert.IsFalse(service.TryGet(null, out _));
        }

        [Test]
        public void TheFit_DrawsBothShapesAtOneScale_AsLargeAsTheStageAllows_NeverCropped()
        {
            // - a tall point next to a small object in a wide stage: the height limits, one scale for both
            Assert.IsTrue(SizeComparisonRule.TryFit(400f, 200f, 16f, new Vector2(32f, 58f), new Vector2(7.2f, 15f), 0f, 0f, out var tall));
            Assert.AreEqual(200f / 58f, tall.PixelsPerCm, 1e-4f, "the taller of the two fills the stage");
            Assert.AreEqual(58f * tall.PixelsPerCm, tall.Poi.y, 1e-3f);
            Assert.AreEqual(15f * tall.PixelsPerCm, tall.Object.y, 1e-3f, "the object at the same scale: true to the point");
            Assert.AreEqual(32f / 58f, tall.Poi.x / tall.Poi.y, 1e-4f, "the point's proportions kept");
            Assert.LessOrEqual(tall.Poi.x + 16f + tall.Object.x, 400f + 1e-3f, "and the pair fits the width");

            // - a wide point in a narrow stage: the width limits, gap included
            Assert.IsTrue(SizeComparisonRule.TryFit(300f, 400f, 20f, new Vector2(100f, 50f), new Vector2(50f, 25f), 0f, 0f, out var wide));
            Assert.AreEqual((300f - 20f) / 150f, wide.PixelsPerCm, 1e-4f, "the pair with its gap fills the stage width");
            Assert.AreEqual(300f, wide.Poi.x + 20f + wide.Object.x, 1e-3f);

            // - the object bigger than the point: it is the larger drawing
            Assert.IsTrue(SizeComparisonRule.TryFit(400f, 200f, 16f, new Vector2(8f, 10f), new Vector2(21f, 29.7f), 0f, 0f, out var bigger));
            Assert.Greater(bigger.Object.y, bigger.Poi.y);
            Assert.AreEqual(200f, bigger.Object.y, 1e-3f, "the taller one is the object now");

            // - no object (the service does not know it): the point alone, no gap taken from the width
            Assert.IsTrue(SizeComparisonRule.TryFit(400f, 200f, 16f, new Vector2(100f, 50f), null, 0f, 0f, out var alone));
            Assert.AreEqual(Vector2.zero, alone.Object);
            Assert.AreEqual(4f, alone.PixelsPerCm, 1e-4f, "100 x 50 cm alone: the width fills 400 px");
            Assert.AreEqual(0f, alone.ObjectSlotWidth, "no object, no slot");
        }

        [Test]
        public void TheFit_GivesEachShapeASlotAsWideAsItsName_TheSlotsNotTheShapesFitTheStage_AndAnEnormousNameWraps()
        {
            // - a name narrower than its shape changes nothing: the slots are the shapes
            Assert.IsTrue(SizeComparisonRule.TryFit(400f, 200f, 16f, new Vector2(32f, 58f), new Vector2(7.2f, 15f), 40f, 20f, out var plain));
            Assert.AreEqual(200f / 58f, plain.PixelsPerCm, 1e-4f);
            Assert.AreEqual(plain.Poi.x, plain.PoiSlotWidth, 1e-3f, "the point's slot is its shape");
            Assert.AreEqual(plain.Object.x, plain.ObjectSlotWidth, 1e-3f, "the object's slot is its shape");

            // - a coin a few pixels wide with a name 80 px wide: its slot is the name, and the point gives up width for it
            var coin = new Vector2(2.575f, 2.575f);
            Assert.IsTrue(SizeComparisonRule.TryFit(200f, 200f, 16f, new Vector2(32f, 58f), coin, 0f, 80f, out var named));
            Assert.AreEqual(80f, named.ObjectSlotWidth, 1e-3f, "the coin's slot is as wide as its name");
            Assert.Less(named.Object.x, 20f, "the coin itself stays a few pixels wide, true to scale");
            Assert.AreEqual(200f, named.PoiSlotWidth + 16f + named.ObjectSlotWidth, 0.05f, "the two slots and the gap fill the stage width exactly");
            Assert.AreEqual((200f - 16f - 80f) / 32f, named.PixelsPerCm, 1e-3f, "the point shrank so its slot plus the coin's name fit");
            Assert.AreEqual(58f / 32f, named.Poi.y / named.Poi.x, 1e-3f, "one scale: the point keeps its proportions");

            // - a name that would take more than half of the room wraps: the slot never takes more than its share
            Assert.IsTrue(SizeComparisonRule.TryFit(300f, 200f, 16f, new Vector2(32f, 58f), coin, 0f, 500f, out var wrapped));
            Assert.AreEqual((300f - 16f) / 2f, wrapped.ObjectSlotWidth, 1e-3f, "capped at half of what is left");
            Assert.LessOrEqual(wrapped.PoiSlotWidth + 16f + wrapped.ObjectSlotWidth, 300f + 0.05f);

            // - the point alone with a long name: its slot may take the whole row
            Assert.IsTrue(SizeComparisonRule.TryFit(300f, 200f, 16f, new Vector2(32f, 58f), null, 250f, 0f, out var alone));
            Assert.AreEqual(250f, alone.PoiSlotWidth, 1e-3f);
        }

        [Test]
        public void TheFit_RefusesWhatCannotBeDrawn()
        {
            Assert.IsFalse(SizeComparisonRule.TryFit(400f, 200f, 16f, new Vector2(0f, 58f), new Vector2(7f, 15f), 0f, 0f, out _), "no width");
            Assert.IsFalse(SizeComparisonRule.TryFit(400f, 200f, 16f, new Vector2(32f, 0f), new Vector2(7f, 15f), 0f, 0f, out _), "no height");
            Assert.IsFalse(SizeComparisonRule.TryFit(0f, 200f, 16f, new Vector2(32f, 58f), null, 0f, 0f, out _), "a stage with no width yet");
            Assert.IsFalse(SizeComparisonRule.TryFit(400f, 0f, 16f, new Vector2(32f, 58f), null, 0f, 0f, out _), "a stage with no height yet");
            Assert.IsFalse(SizeComparisonRule.TryFit(10f, 200f, 16f, new Vector2(32f, 58f), new Vector2(7f, 15f), 0f, 0f, out _), "a stage narrower than the gap");
            Assert.IsFalse(SizeComparisonRule.HasSize(0f, 1f));
            Assert.IsTrue(SizeComparisonRule.HasSize(0.1f, 0.1f));
        }

        // ---------------- config and the builder ----------------

        [Test]
        public void AFullBlock_SurvivesAJsonRoundTrip_FieldForField_InBothLanguages()
        {
            var poi = PoiWith(FullBlock(FamiliarObjects.CreditCard, 12.5f, 7.25f));
            var back = JsonUtility.FromJson<POIData>(JsonUtility.ToJson(poi)).card.blocks.Single();
            var en = new BlockFieldReader(back, "en", "en");
            var pt = new BlockFieldReader(back, "pt", "en");
            Assert.AreEqual(SizeComparisonBlock.Kind, back.kind);
            Assert.AreEqual(FamiliarObjects.CreditCard, en.Value(SizeComparisonBlock.ObjectField));
            Assert.AreEqual(12.5f, en.Number(SizeComparisonBlock.WidthField));
            Assert.AreEqual(7.25f, en.Number(SizeComparisonBlock.HeightField));
            Assert.AreEqual("Four phones tall.", en.Text(SizeComparisonBlock.CaptionField));
            Assert.AreEqual("Quatro telem\u00f3veis de altura.", pt.Text(SizeComparisonBlock.CaptionField));
            Assert.AreEqual("Que tamanho?", pt.Text(BlockKindDefinition.HeadingField));
        }

        [Test]
        public void TheBuilder_JudgesEveryField_TheWidthAndHeightNeedToBeWritten_TheObjectAndCaptionAreRequired()
        {
            var settings = new CardSettings();
            BlockStackBuilder.Result Build(BlockInstanceData block) => BlockStackBuilder.Build(PoiWith(block), settings, BlockRegistry.Shared);

            var ok = Build(FullBlock());
            CollectionAssert.IsEmpty(ok.Skipped);
            Assert.AreEqual(SizeComparisonBlock.SideBySide, ok.Entries.Single(e => e.Definition.Key == SizeComparisonBlock.Kind).Variant, "the one look is the default");

            foreach (var (label, block, reason) in new[]
            {
                ("width 0", FullBlock(width: 0f), BlockStackBuilder.SkipReason.NotForThisPoint),
                ("height 0", FullBlock(height: 0f), BlockStackBuilder.SkipReason.NotForThisPoint),
            })
            {
                var skipped = Build(block).Skipped.Single();
                Assert.AreEqual(reason, skipped.Reason, label);
            }
            Assert.AreEqual("the Width and the Height must both be above 0 centimetres.", SizeComparisonBlock.Definition.NotShownForPoiNote);

            var noObject = FullBlock();
            noObject.fields.RemoveAll(f => f.key == SizeComparisonBlock.ObjectField);
            var skippedObject = Build(noObject).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.MissingRequired, skippedObject.Reason);
            Assert.AreEqual(SizeComparisonBlock.ObjectField, skippedObject.FieldKey);

            var noCaption = FullBlock();
            noCaption.fields.RemoveAll(f => f.key == SizeComparisonBlock.CaptionField);
            var skippedCaption = Build(noCaption).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.MissingRequired, skippedCaption.Reason);
            Assert.AreEqual(SizeComparisonBlock.CaptionField, skippedCaption.FieldKey);

            // - the wall can switch the whole kind off in its Block Library, like any other
            settings.kinds.Add(new BlockKindSetting { kind = SizeComparisonBlock.Kind, enabled = false });
            Assert.AreEqual(BlockStackBuilder.SkipReason.KindDisabled, Build(FullBlock()).Skipped.Single().Reason);
        }

        [Test]
        public void TheShippedLamp_HoldsOneSizeComparison_BeforeItsSources_AndTheBuilderShowsIt()
        {
            var config = ShippedConfig();
            var lamp = config.pois.Single(p => p.id == "lamp");
            var built = BlockStackBuilder.Build(lamp, config.card_settings, BlockRegistry.Shared, config.pois);
            CollectionAssert.IsEmpty(built.Skipped, "every authored block of The Lamp is shown");
            var kinds = built.Entries.Select(e => e.Definition.Key).ToList();
            Assert.AreEqual(1, kinds.Count(k => k == SizeComparisonBlock.Kind), "one on The Lamp");
            Assert.Less(kinds.IndexOf(SizeComparisonBlock.Kind), kinds.IndexOf("sources"), "before the sources, which always close the card");
            Assert.Greater(kinds.IndexOf(SizeComparisonBlock.Kind), kinds.LastIndexOf("show_on_wall"), "after the group that comes before it");
            var block = lamp.card.blocks.Single(b => b.kind == SizeComparisonBlock.Kind);
            var read = new BlockFieldReader(block, "en", "en");
            Assert.AreEqual(FamiliarObjects.Smartphone, read.Value(SizeComparisonBlock.ObjectField));
            Assert.AreEqual(32f, read.Number(SizeComparisonBlock.WidthField));
            Assert.AreEqual(58f, read.Number(SizeComparisonBlock.HeightField));
            foreach (string lang in new[] { "en", "pt" })
                Assert.IsNotEmpty(new BlockFieldReader(block, lang, "en").Text(SizeComparisonBlock.CaptionField), "the caption is written in " + lang);
            Assert.AreEqual(1, lamp.card.blocks.Select(b => b.key).Distinct().Count(k => k == block.key), "its key is unique on the card");
        }

        // ---------------- the view reads the service through the context ----------------

        [Test]
        public void TheView_AsksTheContextForItsService_WithNoServiceOrAnUnknownObject_ThePointIsDrawnAlone()
        {
            var view = new SizeComparisonBlockView();
            BlockBindContext Context(CardServices services) => new()
            {
                Poi = new POIData { id = "p", name = "P" }, Language = "en", FallbackLanguage = "en", Variant = SizeComparisonBlock.SideBySide, Services = services,
            };
            var with = new CardServices();
            with.Add<IFamiliarObjects>(new FamiliarObjects());

            view.Bind(FullBlock(), Context(with));
            Assert.IsTrue(view.ObjectKnown, "the service knew the smartphone");
            Assert.AreEqual(DisplayStyle.Flex, view.ObjectShape.style.display.value);
            Assert.AreEqual("Four phones tall.", view.Caption.text, "the authored words, never a string of the view's own");

            view.Bind(FullBlock(), Context(new CardServices()));
            Assert.IsFalse(view.ObjectKnown, "no service registered: nothing to compare with");
            Assert.AreEqual(DisplayStyle.None, view.ObjectShape.style.display.value, "the object shape is hidden, not drawn at a made-up size");
            Assert.AreEqual("Four phones tall.", view.Caption.text, "the caption still reads");

            view.Bind(FullBlock("giraffe"), Context(with));
            Assert.IsFalse(view.ObjectKnown, "an object the app does not know");
            view.Bind(FullBlock(), new BlockBindContext { Poi = new POIData(), Language = "en", FallbackLanguage = "en", Variant = SizeComparisonBlock.SideBySide });
            Assert.IsFalse(view.ObjectKnown, "a caller with no registry at all");
            view.Unbind();
        }

        [Test]
        public void TheKindsSource_HasNoVisitorWordAndNoLiteralLook_TokensOnly()
        {
            // - the view and the rule: every string literal is an identifier (a USS class or a name), never a sentence; no colour built in code
            foreach (string file in new[] { "SizeComparisonBlockView.cs", "SizeComparisonRule.cs" })
            {
                string source = File.ReadAllText(AppFolder + "/Scripts/SizeComparison/" + file);
                var literals = Regex.Matches(source, @"""((?:[^""\\\n]|\\.)*)""").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
                foreach (string literal in literals)
                    // - an empty string is "no words yet", not a visitor word
                    StringAssert.IsMatch("^([a-z][a-z0-9_-]*)?$", literal, file + ": '" + literal + "' is not a class or a name: visitor words come from the block's fields and the app's card texts");
                StringAssert.DoesNotContain("new Color", source, file);
                StringAssert.DoesNotContain("Color.", source, file);
                StringAssert.DoesNotContain("Color32", source, file);
            }
            // - the style sheet: every value is a token of CardTokens.uss (or a unitless keyword), no colour or size of its own
            string tokens = File.ReadAllText("Assets/Framework/Runtime/UI/Cards/CardTokens.uss");
            string uss = Regex.Replace(File.ReadAllText(AppFolder + "/Scripts/SizeComparison/SizeComparison.uss"), @"/\*.*?\*/", "", RegexOptions.Singleline);
            StringAssert.DoesNotMatch(@"\d+(\.\d+)?(px|%|em|deg|s)\b", uss, "no literal size");
            StringAssert.DoesNotMatch(@"#[0-9a-fA-F]{3,8}\b|rgba?\(", uss, "no literal colour");
            var used = Regex.Matches(uss, @"var\((--[a-z0-9-]+)\)").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
            Assert.Greater(used.Count, 5, "not vacuous: the sheet is written with tokens");
            foreach (string token in used)
            {
                StringAssert.StartsWith("--ts-", token);
                StringAssert.Contains(token + ":", tokens, "'" + token + "' is a token of the framework's token file");
            }
        }

        // ---------------- the real POI Editor window ----------------

        private void OpenTab(string tab)
        {
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", Enum.Parse(tabField.FieldType, tab));
        }

        [UnityTest]
        public IEnumerator TheRealWindow_ListsTheKindInTheBlockLibrary_ARealClickEditsItsRow_AndUndoTakesItBack()
        {
            _window = new PoiEditorWindowHost(TwoPoiConfig(), "_showCardBlockLibrary");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            // - the table lists kinds by family (Ordered), so the row index is the kind's place there
            int row = BlockRegistry.Shared.Ordered.ToList().FindIndex(k => k.Key == SizeComparisonBlock.Kind);
            Assert.GreaterOrEqual(row, 0, "the app's kind is in the registry the window reads");
            _window.RectOf("Block Library enabled#" + row);
            // - the app's row is the last of the table: a person scrolls down to it, so the test does
            _window.SetWindowField("_scrollPos", new Vector2(0f, 300f));
            yield return _window.WaitForRepaint();
            var screen = _window.RectOf("Block Library enabled#" + row);
            Assert.That(screen.yMin, Is.GreaterThan(40f).And.LessThan(940f - screen.height), "precondition: the row is inside the host window (40 .. 940 on screen), where a click can reach it");
            CollectionAssert.IsEmpty(_window.Config.card_settings.kinds, "drawing the table creates no row");

            _window.Click("Block Library enabled#" + row);
            yield return _window.WaitForRepaint();
            var setting = _window.Config.card_settings.kinds.Single();
            Assert.AreEqual(SizeComparisonBlock.Kind, setting.kind, "a real click on the app kind's Enabled box made ITS row");
            Assert.IsFalse(setting.enabled, "it was on: now off");
            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.kinds, "Ctrl+Z takes it back");
        }

        [UnityTest]
        public IEnumerator TheRealWindow_AddBlockOffersTheKind_ItsRowDrawsTheKindsFields_AndATypedCaptionIsStored()
        {
            _window = new PoiEditorWindowHost(TwoPoiConfig(), "_showCardContainer");
            OpenTab("SpecificMarker");
            _window.SetWindowField("_showPoiCardContent", true);
            var foldouts = (System.Collections.Generic.Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_poiFoldouts", Instance).GetValue(_window.Editor);
            foldouts["poi_1"] = true;
            // - the "+ Add block" picker lists every registered kind, family / name: the app's is one of them
            int index = BlockRegistry.Shared.Ordered.ToList().FindIndex(k => k.Key == SizeComparisonBlock.Kind);
            typeof(POIEditorToolWindow).GetField("_newCardBlockKindIndex", Instance).SetValue(_window.Editor, index);
            yield return _window.WaitForRepaint();

            _window.Click("Card add block#0");
            yield return _window.WaitForRepaint();
            var block = _window.Config.pois[0].card.blocks.Single();
            Assert.AreEqual(SizeComparisonBlock.Kind, block.kind, "a real click on + Add block added the app's kind");
            Assert.AreEqual(CardOptions.DisplayInline, block.display);
            var skipped = BlockStackBuilder.Build(_window.Config.pois[0], _window.Config.card_settings, BlockRegistry.Shared).Skipped.Single();
            Assert.AreEqual(BlockStackBuilder.SkipReason.NotForThisPoint, skipped.Reason, "empty sizes: the row says why it is not shown");

            // - the row drew every field of the definition, through the framework's generic drawer
            foreach (string probe in new[] { "Block field object#0", "Block field width_cm#0", "Block field height_cm#0", "Block field caption en#0", "Block field heading en#0" })
                _window.RectOf(probe);
            yield return _window.ReplaceText("Block field caption en#0", "As tall as a phone.");
            yield return _window.ClickAway();
            Assert.AreEqual("As tall as a phone.", new BlockFieldReader(_window.Config.pois[0].card.blocks[0], "en", "en").Text(SizeComparisonBlock.CaptionField));
            yield return _window.PressUndo();
            Assert.AreEqual("", new BlockFieldReader(_window.Config.pois[0].card.blocks[0], "en", "en").Text(SizeComparisonBlock.CaptionField), "Ctrl+Z takes the typed words back");
        }

        // ---------------- the app's own visitor words (step 11-fix) ----------------

        [Test]
        public void TheAppsCardTexts_HaveARowForEveryKeyTheAppReads_EnglishAndPortuguese_AndNoKeyClashesWithTheFramework()
        {
            var table = UnityEngine.Resources.Load<CardStringTable>(LivingRoomCardTexts.TableResourcePath);
            CardStringTableChecks.AssertTableHasEveryKey(table, LivingRoomCardTexts.Keys.All, "LivingRoom");
            // - the same source scan the framework's table gets: every key the app's code reads is listed (a key read but not listed would never be guarded)
            CardStringTableChecks.AssertEveryKeyReadInSourceIsListed(AppFolder + "/Scripts", "LivingRoomCardTexts.Keys.", typeof(LivingRoomCardTexts.Keys),
                LivingRoomCardTexts.Keys.All, "LivingRoom");
            CollectionAssert.IsEmpty(LivingRoomCardTexts.Keys.All.Intersect(CardStrings.Keys.All), "an app key never repeats a framework key");
            foreach (string key in LivingRoomCardTexts.Keys.All)
                StringAssert.StartsWith("living_room_", key, "the app's word starts every key, so no other app can clash with it");
            // - every familiar object names itself with one of the app's keys
            var service = new FamiliarObjects();
            foreach (string objectKey in FamiliarObjects.Keys)
            {
                Assert.IsTrue(service.TryGet(objectKey, out var familiar));
                CollectionAssert.Contains(LivingRoomCardTexts.Keys.All, familiar.NameKey, objectKey + " has a name in the app's table");
            }
            Assert.IsTrue(service.TryGet(FamiliarObjects.TwoEuroCoin, out var coin) && coin.Round, "the coin is drawn round");
            Assert.IsFalse(service.TryGet(FamiliarObjects.Smartphone, out var phone) && phone.Round, "a phone is a rectangle");
        }

        [Test]
        public void TheAppsWords_ReachTheLookup_InBothLanguages_AndAWallCanOverrideThem()
        {
            var framework = CardStringTableChecks.FrameworkEntries();
            var app = CardStringSources.Shared.Entries();
            Assert.AreEqual("2 euro coin", new CardStrings(framework, app, null, "en", "en").Get(LivingRoomCardTexts.Keys.ObjectTwoEuroCoin));
            Assert.AreEqual("Moeda de 2 euros", new CardStrings(framework, app, null, "pt", "en").Get(LivingRoomCardTexts.Keys.ObjectTwoEuroCoin));
            var wall = new System.Collections.Generic.List<CardStringEntry>
                { new() { key = LivingRoomCardTexts.Keys.ObjectTwoEuroCoin, text = Words("Two euros") } };
            Assert.AreEqual("Two euros", new CardStrings(framework, app, wall, "en", "en").Get(LivingRoomCardTexts.Keys.ObjectTwoEuroCoin), "the wall's own wording wins");
            Assert.AreEqual("Moeda de 2 euros", new CardStrings(framework, app, wall, "pt", "en").Get(LivingRoomCardTexts.Keys.ObjectTwoEuroCoin), "the visitor's language before the wall's fallback wording");
            Assert.AreEqual("Fechar", new CardStrings(framework, app, wall, "pt", "en").Get(CardStrings.Keys.Close), "the app's table never hides a framework text");
        }

        [Test]
        public void TheBlockRegistry_ListsTheAppsKindWithItsFamily_NotAfterEveryBuiltInKind()
        {
            var ordered = BlockRegistry.Shared.Ordered.Select(k => k.Key).ToList();
            var all = BlockRegistry.Shared.All.Select(k => k.Key).ToList();
            CollectionAssert.AreEquivalent(all, ordered, "the same kinds, only regrouped");
            var aboutKeys = BlockRegistry.Shared.Ordered.Where(k => k.Family == "about").Select(k => k.Key).ToList();
            int firstAbout = ordered.IndexOf(aboutKeys.First());
            Assert.AreEqual(aboutKeys.Count, ordered.Skip(firstAbout).TakeWhile(k => aboutKeys.Contains(k)).Count(), "the about kinds sit together");
            Assert.AreEqual(SizeComparisonBlock.Kind, aboutKeys.Last(), "the app's kind follows the built-in about kinds, in registration order");
            Assert.Less(ordered.IndexOf(SizeComparisonBlock.Kind), ordered.IndexOf("timeline"), "and comes before the next family's kinds (stories)");
            Assert.Less(ordered.IndexOf(SizeComparisonBlock.Kind), ordered.IndexOf("sources"), "...and before every later family");
        }

        [UnityTest]
        public IEnumerator TheRealWindow_TheBlockLibraryDrawsTheAppsKindInItsFamilyGroup_WithTheOrderTheRegistrySays()
        {
            _window = new PoiEditorWindowHost(TwoPoiConfig(), "_showCardBlockLibrary");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            var ordered = BlockRegistry.Shared.Ordered;
            int app = ordered.ToList().FindIndex(k => k.Key == SizeComparisonBlock.Kind);
            Assert.Less(app, ordered.Count - 1, "the app's kind is no longer the last row");
            // - the rows sit one under another in registry order: each Enabled box is lower on screen than the one above it
            float previous = float.MinValue;
            for (int i = 0; i < ordered.Count; i++)
            {
                float y = _window.RectOf("Block Library enabled#" + i).y;
                Assert.Greater(y, previous, "row " + i + " (" + ordered[i].DisplayName + ") is drawn below the row before it");
                previous = y;
            }
            Assert.Greater(_window.RectOf("Block Library enabled#" + app).y, _window.RectOf("Block Library enabled#" + (app - 1)).y, "the app's row follows the last built-in about kind");
        }

        [UnityTest]
        public IEnumerator TheRealWindow_CardTexts_ListTheAppsRows_RealTypingMakesTheWallsWording_AndCtrlZTakesItBack()
        {
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showCardTexts");
            OpenTab("DetailCard");
            yield return _window.WaitForRepaint();
            foreach (string key in LivingRoomCardTexts.Keys.All)
                foreach (string lang in new[] { "en", "pt" })
                    _window.RectOf("Card text " + key + " " + lang + "#0");
            Assert.IsFalse(_window.Unsaved, "drawing the app's rows writes nothing");
            CollectionAssert.IsEmpty(_window.Config.card_settings.strings, "precondition: the shipped wall keeps the app's and the framework's wording");

            string key2 = LivingRoomCardTexts.Keys.ObjectTwoEuroCoin;
            string field = "Card text " + key2 + " pt#0";
            // - the app's rows come after every framework row: a person scrolls down to them, so the test does (the click must land on the window)
            float contentY = _window.Local(_window.RectOf(field).center).y;
            _window.SetWindowField("_scrollPos", new Vector2(0f, Mathf.Max(0f, contentY - 300f)));
            yield return _window.WaitForRepaint();
            Assert.That(_window.Local(_window.RectOf(field).center).y, Is.InRange(20f, 860f), "precondition: the app's row is inside the host window, where a click can reach it");
            yield return _window.ReplaceText(field, "Dois euros");
            yield return _window.ClickAway();
            var s = _window.Config.card_settings;
            Assert.AreEqual("Dois euros", CardStringTableChecks.WallWording(s, key2, "pt"), "real typing on an app row wrote the wall's wording");
            Assert.AreEqual("", CardStringTableChecks.WallWording(s, key2, "en"), "the other language stays the app's");
            var framework = CardStringTableChecks.FrameworkEntries();
            var app = CardStringSources.Shared.Entries();
            Assert.AreEqual("Dois euros", new CardStrings(framework, app, s.strings, "pt", "en").Get(key2), "the card reads the wall's wording first");
            Assert.AreEqual("2 euro coin", new CardStrings(framework, app, s.strings, "en", "en").Get(key2), "an untouched language keeps the app's wording");

            yield return _window.PressUndo();
            CollectionAssert.IsEmpty(_window.Config.card_settings.strings, "one Ctrl+Z: back to the app's wording, no empty row left");
        }
    }
}
