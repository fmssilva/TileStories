using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // _3.3 P4.4b part 2: how many lines a card's title takes in its hero -- HeroTitleFitRule (pure greedy wrap over a measure), the Editor's
    // HeroTitleMeasure (the theme's real display face at the phone's hero size, on the phone's line) and the Card Content notice it feeds.
    // The agreement with the REAL hero label on the card is PoiCardHeroTitleFitSceneTests (PlayMode).
    public class HeroTitleFitRuleTests
    {
        // A measure where every character is 10 px wide (spaces too): the expected wraps are plain arithmetic
        private static float Mono(string run) => run.Length * 10f;

        [Test]
        public void Lines_WrapsWholeWordsGreedily_AtTheWidth()
        {
            Assert.AreEqual(1, HeroTitleFitRule.Lines("The Lamp", 100f, Mono), "8 characters = 80 px in 100");
            Assert.AreEqual(1, HeroTitleFitRule.Lines("abcd efghi", 100f, Mono), "exactly the width still fits (100 in 100)");
            Assert.AreEqual(2, HeroTitleFitRule.Lines("abcd efghij", 100f, Mono), "one more character: the second word takes its own line");
            Assert.AreEqual(3, HeroTitleFitRule.Lines("aaaa bbbb cccc dddd eeee", 100f, Mono), "two words per line: 3 lines");
            Assert.AreEqual(1, HeroTitleFitRule.Lines("  The   Lamp  ", 100f, Mono), "extra spaces are one space");
        }

        [Test]
        public void Lines_AWordWiderThanTheLine_TakesTheLinesItIsWide_AndNothingIsZero()
        {
            Assert.AreEqual(3, HeroTitleFitRule.Lines(new string('a', 25), 100f, Mono), "250 px in 100: broken over 3 lines");
            Assert.AreEqual(4, HeroTitleFitRule.Lines("ab " + new string('a', 25), 100f, Mono), "a short word, then the long one from a new line");
            Assert.AreEqual(0, HeroTitleFitRule.Lines("", 100f, Mono));
            Assert.AreEqual(0, HeroTitleFitRule.Lines("   ", 100f, Mono));
            Assert.AreEqual(0, HeroTitleFitRule.Lines("The Lamp", 0f, Mono), "no room: no answer");
            Assert.AreEqual(0, HeroTitleFitRule.Lines("The Lamp", 100f, null));
        }

        [Test]
        public void NeedsNotice_FromThreeLines()
        {
            Assert.IsFalse(HeroTitleFitRule.NeedsNotice(0));
            Assert.IsFalse(HeroTitleFitRule.NeedsNotice(2));
            Assert.IsTrue(HeroTitleFitRule.NeedsNotice(HeroTitleFitRule.NoticeAtLines));
            Assert.AreEqual(3, HeroTitleFitRule.NoticeAtLines, "the Architect's review of P4.4a: a notice when a title would need 3 lines");
        }

        [Test]
        public void TheMeasure_ReadsTheThemesDisplayFace_AtThePhonesHeroSize_OnThePhonesLine()
        {
            Assert.IsTrue(HeroTitleMeasure.TryLine(out var font, out float size, out float width), "the theme, its display face and the panel are found");
            StringAssert.StartsWith("Fraunces", font.faceInfo.familyName, "the display role (R12.3)");
            Assert.AreEqual(40f, size, 0.01f, "--ts-size-hero-narrow: a phone is narrower than HeroRule.NarrowBelow");
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeAsset>(HeroTitleMeasure.ThemePath);
            float footPadding = theme.Shared.lengths.Single(t => t.name == HeroTitleMeasure.FootPaddingToken).value.value;
            Assert.AreEqual(390f - 2f * footPadding, width, 0.01f, "the 390 pt reference width less the foot's --ts-space-5 each side (the real label's width: "
                + "PoiCardHeroSceneTests.TheTitleFitRule_...)");
            Assert.Greater(HeroTitleFitRule.AdvanceWidth(font, size, "WWW"), HeroTitleFitRule.AdvanceWidth(font, size, "iii"), "real advances, not one width per letter");
            Assert.AreEqual(2f * HeroTitleFitRule.AdvanceWidth(font, size, "Lamp"), HeroTitleFitRule.AdvanceWidth(font, size, "LampLamp"), 0.01f, "a run is the sum of its glyphs");
        }

        // The shipped wall's picture headers, every language: none needs the notice today (the notice is for what an author types next)
        [Test]
        public void TheShippedTitles_FitTwoLines_InEveryLanguage()
        {
            var config = ShippedWall.WithFrameworkNavigation();
            var measured = new List<string>();
            foreach (var poi in config.pois)
            {
                var header = poi.card?.blocks?.FirstOrDefault(b => b.kind == BuiltInBlocks.HeaderKind);
                if (header == null || !BuiltInBlocks.HeaderShowsPicture(header.variant, header)) continue;
                foreach (string language in config.card_settings.languages)
                {
                    string title = new BlockFieldReader(header, language, config.card_settings.languages[0]).Text(BlockStackBuilder.HeaderTitleField);
                    if (title.Length == 0) title = poi.name;
                    int lines = HeroTitleMeasure.Lines(title);
                    measured.Add(poi.id + "/" + language + " \"" + title + "\" " + lines);
                    Assert.That(lines, Is.InRange(1, 2), poi.id + " in " + language + ": \"" + title + "\" takes " + lines + " lines");
                }
            }
            Assert.GreaterOrEqual(measured.Count, 4, "precondition: The Lamp and The Painting (at least) have picture heroes in two languages: " + string.Join("; ", measured));
        }

        // The Card Content notice: a picture header whose title needs 3 lines in one language says so for that language only; a text-only
        // header never does (no picture to push out)
        [Test]
        public void CardContent_WarnsAbout_AThreeLineTitle_InItsLanguage_OnAPictureHeaderOnly()
        {
            const string longTitle = "The Royal Ribeira Palace and its Great Earthquake Ruins";
            Assert.GreaterOrEqual(HeroTitleMeasure.Lines(longTitle), 3, "precondition: the long title needs 3+ lines in the real face");
            var settings = new CardSettings { languages = new List<string> { "en", "pt" } };
            var poi = new POIData { id = "poi_1", name = "Palace" };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderImageParallax };
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "images/castle_hero.png" });
            header.fields.Add(new BlockFieldValue { key = BlockStackBuilder.HeaderTitleField, text = new List<LocalizedEntry>
                { new() { lang = "en", value = longTitle }, new() { lang = "pt", value = "O Palacio" } } });
            poi.card.blocks.Add(header);

            var warnings = POIEditorToolWindow.CardBlockWarnings(header, BuiltInBlocks.Header, settings, poi);
            Assert.AreEqual(1, warnings.Count, "one notice, for English: " + string.Join(" | ", warnings));
            StringAssert.StartsWith("Title (en) \"" + longTitle + "\" takes " + HeroTitleMeasure.Lines(longTitle) + " lines", warnings[0]);

            header.variant = BuiltInBlocks.HeaderTextOnly;
            Assert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(header, BuiltInBlocks.Header, settings, poi), "the text-only look: no picture to push out");
        }
    }
}
