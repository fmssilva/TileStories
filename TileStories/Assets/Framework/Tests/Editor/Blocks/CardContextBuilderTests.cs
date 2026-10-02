using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // CardContextBuilder (_3.1 15.4.2): what a card is bound with, decided once for both hosts. On the REAL LivingRoom wall (its en + pt
    // Languages, its glossary, the framework's CardStrings.asset); expected words are read from their sources, never retyped. The hosts'
    // real cards (PoiCardSceneTests, PoiCardLanguageSceneTests, CardGalleryTests) prove the context reaches every block.
    public class CardContextBuilderTests
    {
        private const string ConfigPath = "Assets/Apps/LivingRoom/config.json";
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";

        private static WallConfigData Wall() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText(ConfigPath));

        private static CardStringTable FrameworkTable() => AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath);

        // A word of the framework table in one language, as the table itself holds it
        private static string TableWord(string key, string lang) =>
            FrameworkTable().Entries().First(e => e.key == key).text.First(t => t.lang == lang).value;

        [Test]
        public void Languages_TheVisitorsPickWins_ThenAnAllowedPreview_ThenTheWallsFirst_AndTheFallbackIsAlwaysTheFirst()
        {
            var settings = Wall().card_settings;
            Assert.AreEqual(new[] { "en", "pt" }, settings.languages.ToArray(), "precondition: the shipped wall speaks en then pt");

            Assert.AreEqual(("en", "en"), CardContextBuilder.Languages(settings, "", previewAllowed: true), "no pick, no preview: the first language");
            Assert.AreEqual(("pt", "en"), CardContextBuilder.Languages(settings, "pt", previewAllowed: true), "a Portuguese pick still falls back to English");

            settings.preview_language = "pt";
            Assert.AreEqual(("pt", "en"), CardContextBuilder.Languages(settings, "", previewAllowed: true), "the developer's preview");
            Assert.AreEqual(("en", "en"), CardContextBuilder.Languages(settings, "", previewAllowed: false), "a release build ignores the preview");
            Assert.AreEqual(("en", "en"), CardContextBuilder.Languages(settings, "en", previewAllowed: true), "the visitor's own pick wins over the preview");

            settings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            settings.preview_language = "";
            Assert.AreEqual(("pt", "pt"), CardContextBuilder.Languages(settings, "", previewAllowed: true), "the fallback follows the Languages order");
        }

        [Test]
        public void NeedsMediaSource_OnlyWithoutOne_OrWhenTheMediaFolderReallyChanged()
        {
            var settings = Wall().card_settings;
            string root = new ResourcesMediaSource(settings.media_resources_path).Root;
            Assert.IsTrue(CardContextBuilder.NeedsMediaSource(null, settings), "no source yet");
            Assert.IsFalse(CardContextBuilder.NeedsMediaSource(root, settings), "the same folder keeps its source (and its loaded media)");
            settings.media_resources_path = " /" + settings.media_resources_path + "/ ";
            Assert.IsFalse(CardContextBuilder.NeedsMediaSource(root, settings), "stray slashes and spaces are the same folder");
            settings.media_resources_path = "LivingRoom/OtherMedia";
            Assert.IsTrue(CardContextBuilder.NeedsMediaSource(root, settings), "another folder");
        }

        [Test]
        public void Build_CompletesTheHostsOwnParts_WithThePoint_TheLanguages_AndTheWordsAndGlossaryInThem()
        {
            var wall = Wall();
            var lamp = wall.pois.First(p => p.id == "lamp");
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var state = new CardLocalState(new MemoryCardStateStore(), wall.wall_id);
            var events = new LogCardEvents();
            var services = new CardServices();
            var hostParts = new BlockBindContext { Media = media, State = state, Events = events, Services = services };

            var context = CardContextBuilder.Build(hostParts, lamp, wall, wall.card_settings, "pt", FrameworkTable().Entries(), null);

            Assert.AreSame(hostParts, context, "the host's own object, completed");
            Assert.AreSame(media, context.Media, "what the host owns is kept");
            Assert.AreSame(state, context.State);
            Assert.AreSame(events, context.Events);
            Assert.AreSame(services, context.Services);
            Assert.AreSame(lamp, context.Poi);
            Assert.AreSame(wall, context.Taxonomy);
            Assert.AreSame(wall.card_settings, context.Settings, "the live settings the Block Library defaults are read from");
            Assert.AreEqual("pt", context.Language);
            Assert.AreEqual("en", context.FallbackLanguage, "the wall's first language");

            Assert.AreEqual(TableWord(CardStrings.Keys.Close, "pt"), context.Strings.Get(CardStrings.Keys.Close), "the card's words in Portuguese");
            var keep = wall.card_settings.glossary.First(g => g.term == "keep");
            Assert.AreEqual(keep.definition.First(d => d.lang == "pt").value, context.Glossary.Definition("keep"), "the glossary in Portuguese");
            Assert.IsFalse(context.ReduceMotion, "the shipped wall moves");

            wall.card_settings.container.reduce_motion = true;
            wall.card_settings.strings.Add(new CardStringEntry
            {
                key = CardStrings.Keys.Close,
                text = { new LocalizedEntry { lang = "pt", value = "Sair" } },
            });
            var again = CardContextBuilder.Build(new BlockBindContext(), lamp, wall, wall.card_settings, "pt", FrameworkTable().Entries(), null);
            Assert.AreEqual("Sair", again.Strings.Get(CardStrings.Keys.Close), "a wall's own wording wins over the framework's");
            Assert.IsTrue(again.ReduceMotion, "Reduce Motion reaches the card");
        }

        [Test]
        public void Build_WithNoSettings_ReadsTheDefaults_AndAnEnglishOnlyTextShowsOnAPortugueseCard()
        {
            var lamp = Wall().pois.First(p => p.id == "lamp");
            var context = CardContextBuilder.Build(null, lamp, null, null, "pt", FrameworkTable().Entries(), null);
            Assert.IsNotNull(context.Settings, "a missing settings object reads as the defaults");
            Assert.AreEqual(new CardSettings().languages[0], context.FallbackLanguage, "the default wall's first language");
            Assert.IsFalse(context.ReduceMotion);
        }

        [Test]
        public void SkipWarning_NamesThePoint_TheBlock_TheReason_AndTheFieldToBlame()
        {
            var block = new BlockInstanceData { key = "block_7", kind = "gallery" };
            Assert.AreEqual("[Card] lamp: block 'block_7' (gallery) not shown: NoCompleteRow 'items'",
                CardContextBuilder.SkipWarning("lamp", new BlockStackBuilder.Skipped(block, BlockStackBuilder.SkipReason.NoCompleteRow, "items")));
            Assert.AreEqual("[Card] lamp: block 'block_7' (gallery) not shown: KindDisabled",
                CardContextBuilder.SkipWarning("lamp", new BlockStackBuilder.Skipped(block, BlockStackBuilder.SkipReason.KindDisabled)));
        }

        [Test]
        public void BothHosts_BuildTheirContextHere_NeverByHand()
        {
            // - the two hosts once copied the same words / glossary / fallback wiring by hand; one copy drifting is how a block reads
            //   one language on the wall and another in the gallery
            foreach (string path in new[] { "Assets/Framework/Runtime/UI/Cards/PoiCardHost.cs", "Assets/Framework/Runtime/DevTools/CardGalleryHarness.cs" })
            {
                string code = Regex.Replace(File.ReadAllText(path), @"//.*", "");
                StringAssert.Contains("CardContextBuilder.Build(", code, path + " builds its context through CardContextBuilder");
                Assert.IsFalse(Regex.IsMatch(code, @"new\s+CardStrings\s*\(|new\s+CardGlossary\s*\(|FallbackLanguage\s*="),
                    path + " decides no words, glossary or fallback of its own");
            }
        }
    }
}
