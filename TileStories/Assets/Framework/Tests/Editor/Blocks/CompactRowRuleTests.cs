using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The compact row's words (_3.2 step 12, CompactRowRule) and the one-line summary it falls back to (BlockSummaryRule, the line the
    // Editor's Card Content rows show since _3.1 15.1 -- moved to Runtime so both read one copy). On the SHIPPED Lamp through the real
    // BlockStackBuilder, with the framework's real string table: the heading in each language, the kind's default heading, the first
    // words, the kind's words, the format and minutes -- and a clip kind that shows no minutes, because the row never loads its clip.
    public class CompactRowRuleTests
    {
        private const string Lamp = "lamp";
        private WallConfigData _config;
        private List<BlockStackBuilder.Entry> _entries;
        private IReadOnlyList<CardStringEntry> _table;

        [OneTimeSetUp]
        public void LoadTheShippedWall()
        {
            _config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            _entries = BlockStackBuilder.Build(_config.pois.Single(p => p.id == Lamp), _config.card_settings, BlockRegistry.Shared, _config.pois).Entries;
            _table = AssetDatabase.LoadAssetAtPath<CardStringTable>("Assets/Framework/Runtime/UI/Cards/CardStrings.asset").Entries();
            Assert.AreEqual(62, _entries.Count, "precondition: every one of The Lamp's blocks builds");
        }

        private BlockStackBuilder.Entry Entry(string key) => _entries.Single(e => e.Instance.key == key);

        private CardStrings Strings(string language) => new(_table, null, null, language, "en");

        private CompactRowRule.Row Row(string key, string language) =>
            CompactRowRule.Of(Entry(key), BlockRegistry.Shared, language, "en", Strings(language).Get);

        private static BlockFieldValue Text(string key, params (string lang, string value)[] texts) =>
            new() { key = key, text = texts.Select(t => new LocalizedEntry { lang = t.lang, value = t.value }).ToList() };

        // ---------------- the one-line summary (moved from the Editor) ----------------

        [Test]
        public void ABlockSummary_IsItsHeadingInTheLanguage_ElseItsFirstWords_OnOneLine()
        {
            var headed = new BlockInstanceData { kind = BuiltInBlocks.RichTextKind };
            headed.fields.Add(Text(BlockKindDefinition.HeadingField, ("pt", "Antes da reconstrução"), ("en", "Before the earthquake")));
            headed.fields.Add(Text(BuiltInBlocks.RichTextBodyField, ("en", "The tiles were made in Lisbon.")));
            Assert.AreEqual("Before the earthquake", BlockSummaryRule.FirstWords(headed, BuiltInBlocks.RichText, "en", "en"), "the heading, in the language asked");
            Assert.AreEqual("Antes da reconstrução", BlockSummaryRule.FirstWords(headed, BuiltInBlocks.RichText, "pt", "en"), "Portuguese with its real letters");
            Assert.AreEqual("Before the earthquake", BlockSummaryRule.FirstWords(headed, BuiltInBlocks.RichText, "es", "en"), "a language it lacks: the fallback's");

            var body = new BlockInstanceData { kind = BuiltInBlocks.RichTextKind };
            body.fields.Add(Text(BuiltInBlocks.RichTextBodyField, ("en", "The [[azulejo|azulejos]] were\nmade   in Lisbon.\n\nSecond paragraph.")));
            Assert.AreEqual("The azulejo were made in Lisbon. Second paragraph.", BlockSummaryRule.FirstWords(body, BuiltInBlocks.RichText, "en", "en"),
                "no heading: its first words, the glossary mark as the visitor reads it, on one line");

            var facts = new BlockInstanceData { kind = BuiltInBlocks.QuickFactsKind };
            var row = new BlockItemData();
            row.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.QuickFactsLabelField, text = new List<LocalizedEntry> { new() { lang = "en", value = "Built" } } });
            facts.fields.Add(new BlockFieldValue { key = BuiltInBlocks.QuickFactsItemsField, items = { row } });
            Assert.AreEqual("Built", BlockSummaryRule.FirstWords(facts, BuiltInBlocks.QuickFacts, "en", "en"), "words only in rows: the first row's");

            Assert.AreEqual("", BlockSummaryRule.FirstWords(new BlockInstanceData { kind = BuiltInBlocks.ShowOnWallKind }, BuiltInBlocks.ShowOnWall, "en", "en"),
                "a block with no words says nothing");
        }

        // ---------------- the compact row ----------------

        [Test]
        public void AHeadedBlock_SaysItsHeading_InEachLanguage_WithItsFormatAndMinutes()
        {
            var en = Row("block_2", "en");
            Assert.AreEqual("The story", en.Words);
            Assert.AreEqual(3, en.Minutes, "The Lamp's block_2 gives a Reading Time of 3");
            Assert.AreEqual(Strings("en").Get(CardStrings.Keys.FormatRead), en.FormatWords, "rich_text reads");
            Assert.AreEqual("Read · 3 min", en.Meta, "the format and the minutes, in the card's own wording");
            Assert.AreEqual(BuiltInFamilies.About, BlockRegistry.Shared.TryGet(BuiltInBlocks.RichTextKind, out var kind) ? kind.Family : null);
            Assert.IsTrue(BlockRegistry.Shared.TryGetFamily(BuiltInFamilies.About, out var about));
            Assert.AreEqual(about.IconKey, en.IconKey, "the family's icon");
            Assert.AreEqual(about.ColorToken, en.ColorToken, "and its colour token");

            var pt = Row("block_2", "pt");
            Assert.AreEqual("A história", pt.Words, "Portuguese with its accent");
            Assert.AreEqual(Strings("pt").Get(CardStrings.Keys.FormatRead) + " · 3 min", pt.Meta);
            Assert.AreNotEqual(en.FormatWords, pt.FormatWords, "the format's words are translated (Read / Ler)");
        }

        [Test]
        public void ABlockWithNoHeading_SaysItsKindsDefaultHeading_ElseItsFirstWords_ElseItsKindsWords()
        {
            // - sources has a default heading: the words the stack draws over it, never its first source's title
            Assert.AreEqual(Strings("en").Get(BuiltInBlocks.Sources.DefaultHeadingKey), Row("block_15", "en").Words);
            Assert.AreEqual(Strings("pt").Get(BuiltInBlocks.Sources.DefaultHeadingKey), Row("block_15", "pt").Words);

            // - no heading, no default: the block's first words, exactly the Editor's summary of it
            var funFact = Entry("block_9");
            string first = BlockSummaryRule.FirstWords(funFact.Instance, funFact.Definition, "en", "en");
            Assert.Greater(first.Length, 0, "precondition: The Lamp's first fun fact has words");
            Assert.AreEqual(first, Row("block_9", "en").Words);
            Assert.IsTrue(Row("block_9", "en").FromFirstWords, "a teaser of its text: one line, cut by the stylesheet");
            Assert.IsFalse(Row("block_15", "en").FromFirstWords, "a heading is shown whole");
            Assert.IsFalse(Row("block_2", "en").FromFirstWords);

            // - no words at all: the kind's own words (what a chip calls it), so a row is never blank
            var bare = new BlockStackBuilder.Entry(new BlockInstanceData { key = "block_x", kind = BuiltInBlocks.ShowOnWallKind },
                BuiltInBlocks.ShowOnWall, BuiltInBlocks.ShowOnWall.Variants[0], false);
            var row = CompactRowRule.Of(bare, BlockRegistry.Shared, "pt", "en", Strings("pt").Get);
            Assert.AreEqual(Strings("pt").Get(BuiltInBlocks.ShowOnWall.LabelKey), row.Words);
            Assert.IsFalse(row.FromFirstWords, "the kind's words are a name, shown whole");
            Assert.AreEqual(0, row.Minutes);
            Assert.AreEqual(row.FormatWords, row.Meta, "no minutes: the format alone");
        }

        [Test]
        public void AClipKind_ShowsItsFormatOnly_TheRowNeverLoadsTheClipForItsLength()
        {
            var audio = Row("block_55", "en");
            Assert.AreEqual("Listen", audio.Words, "the player's heading");
            Assert.AreEqual(0, audio.Minutes, "an audio's minutes are its clip's: the row does not load it");
            Assert.AreEqual(Strings("en").Get(CardStrings.Keys.FormatListen), audio.Meta);
            var video = Row("block_56", "pt");
            Assert.AreEqual("Em vídeo", video.Words);
            Assert.AreEqual(Strings("pt").Get(CardStrings.Keys.FormatWatch), video.Meta);
        }

        [Test]
        public void EveryBlockOfTheLamp_HasWordsAndAFormat_AndEveryFormatIsNamed()
        {
            foreach (var entry in _entries)
                foreach (string language in new[] { "en", "pt" })
                {
                    var row = CompactRowRule.Of(entry, BlockRegistry.Shared, language, "en", Strings(language).Get);
                    Assert.Greater(row.Words.Length, 0, entry.Instance.key + " " + language + ": a row is never blank");
                    Assert.Greater(row.FormatWords.Length, 0, entry.Instance.key + ": its format has words");
                    Assert.AreNotEqual(CardNavModel.FormatLabelKey(entry.Definition.Format), row.FormatWords, entry.Instance.key + ": words, not the key");
                    Assert.IsNotNull(row.IconKey, entry.Instance.key + ": its family has an icon");
                }
            foreach (string format in CardOptions.Formats) Assert.IsNotNull(CardNavModel.FormatLabelKey(format), format);
            Assert.IsNull(CardNavModel.FormatLabelKey("unknown"));
        }
    }
}
