using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The full view among its siblings (_3.2 section 5.7, step 16) on a REAL sheet with The Lamp's real card, the shipped words and the wall's
    // real media folder -- what a turn does, with no panel (Phase B swipes and taps for real, and asks the owners):
    //   - a Media block opens as one page of the 14 Media blocks, at its own place; the breadcrumb is "<title> > Media > <its words>"; a chip per
    //     sibling named by its words, the current one marked
    //   - a turn binds the next sibling's second view through the takeover's scope and gives the previous one's back, the breadcrumb and the
    //     chip follow, the card's own views never move; a chip goes straight to its sibling
    //   - Back closes the full view (never the previous sibling): the card where it was
    //   - a family of one (Listen) has no chips; a block of no section (the sources) has no section in its breadcrumb
    //   - a language switch keeps the sibling shown, in the new words
    public class FullViewSiblingsSheetTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string Lamp = "lamp";

        private static WallConfigData Shipped() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private static BlockBindContext ShowCard(PoiCardSheetView sheet, WallConfigData wall, IMediaSource media, string language = "en")
        {
            var poi = wall.pois.Single(p => p.id == Lamp);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext { Media = media }, poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
            return context;
        }

        [Test]
        public void AMediaBlock_OpensAmongItsSiblings_ATurnHandsThePageOn_TheCrumbAndChipFollow_AndBackReturnsToTheCard()
        {
            var wall = Shipped();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var sheet = Sheet();
            var context = ShowCard(sheet, wall, media);
            var model = sheet.Nav.Model;
            var section = model.Sections.Single(s => s.Key == BuiltInFamilies.Media);
            var siblings = FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, "block_58").ToList();
            Assert.AreEqual(section.Targets.ToList(), siblings, "precondition: the rule's 14 Media siblings");
            var cardsOwn = siblings.ToDictionary(k => k, k => sheet.Stack.ViewOfBlock(k));
            int cardFiles = siblings.Sum(k => sheet.Stack.HeldMediaOf(k));
            string mediaWords = context.Strings.Get(section.LabelKey);

            sheet.OpenFullView("block_58");
            var takeover = sheet.Takeover;
            int at = siblings.IndexOf("block_58");
            Assert.AreEqual(siblings.Count, takeover.PageCount, "a page per sibling");
            Assert.AreEqual(at, takeover.PageIndex, "opened at its own place among them");
            Assert.AreEqual("block_58", sheet.FullView.BlockKey);
            Assert.IsTrue(sheet.FullView.TurnsBetweenBlocks, "a swipe turns between them");
            Assert.AreEqual(sheet.Stack.HeaderView.TitleText + " > " + mediaWords + " > " + sheet.Nav.RowOf("block_58").Words, takeover.CrumbText,
                "the breadcrumb: the card, the section, the block");
            Assert.IsTrue(takeover.SectionShown);
            Assert.AreEqual(section.IconKey, CardIcons.KeyOf(takeover.CrumbSection.Children().First()), "the section's family icon");
            Assert.AreEqual(DisplayStyle.Flex, takeover.ChipStrip.style.display.value, "chips drawn");
            Assert.AreEqual(siblings.Count, takeover.Chips.childCount, "a chip per sibling");
            for (int i = 0; i < siblings.Count; i++)
            {
                Assert.AreEqual(sheet.Nav.RowOf(siblings[i]).Words, takeover.Chips[i].tooltip, "chip " + i + " is named by its block's words");
                Assert.AreEqual(i == at, takeover.Chips[i].ClassListContains(TakeoverView.CurrentChipClass), "chip " + i + " marked or not");
            }

            // - a turn: the next sibling's second view, the previous one given back, the card's own views untouched
            var first = sheet.FullView.View;
            takeover.Turn(+1);
            string next = siblings[at + 1];
            Assert.AreEqual(next, sheet.FullView.BlockKey, "the next sibling");
            // - block_58 and block_59 are both 3D models: the pool hands the SAME view back, unbound and bound again (never instantiated anew)
            Assert.AreEqual(first.GetType(), sheet.FullView.View.GetType(), "precondition: two blocks of one kind");
            Assert.AreEqual(1, sheet.FullView.Scroll.contentContainer.childCount, "ONE view on the page: the previous binding left it");
            Assert.AreNotSame(sheet.Stack.ViewOfBlock(next), sheet.FullView.View, "a second view, never the card's own");
            StringAssert.EndsWith(" > " + mediaWords + " > " + sheet.Nav.RowOf(next).Words, takeover.CrumbText, "the breadcrumb follows");
            Assert.IsTrue(takeover.Chips[at + 1].ClassListContains(TakeoverView.CurrentChipClass), "and the chip");
            Assert.IsFalse(takeover.Chips[at].ClassListContains(TakeoverView.CurrentChipClass));
            foreach (var pair in cardsOwn) Assert.AreSame(pair.Value, sheet.Stack.ViewOfBlock(pair.Key), pair.Key + ": the card's own view never moves");
            Assert.AreEqual(cardFiles, siblings.Sum(k => sheet.Stack.HeldMediaOf(k)), "and the card's files are untouched");

            // - a chip: straight to its sibling; past the ends nothing turns
            takeover.ShowPage(0);
            Assert.AreEqual(siblings[0], sheet.FullView.BlockKey);
            takeover.Turn(-1);
            Assert.AreEqual(siblings[0], sheet.FullView.BlockKey, "no page before the first");
            takeover.ShowPage(siblings.Count - 1);
            takeover.Turn(+1);
            Assert.AreEqual(siblings[siblings.Count - 1], sheet.FullView.BlockKey, "no page after the last");

            // - Back: the CARD, never the previous sibling
            Assert.IsTrue(sheet.Back());
            Assert.IsFalse(takeover.IsOpen, "one Back closes the full view");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the card where it was");
            Assert.AreEqual(0, takeover.HeldMedia, "the full view holds nothing");
            foreach (var pair in cardsOwn) Assert.AreSame(pair.Value, sheet.Stack.ViewOfBlock(pair.Key));
            sheet.Hide();
            Assert.AreEqual(0, media.LoadedCount, "closing the card holds nothing");
        }

        [Test]
        public void AFamilyOfOne_HasNoChips_ABlockOfNoSection_NoSectionInItsCrumb_AndALanguageSwitchKeepsTheSiblingShown()
        {
            var wall = Shipped();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var sheet = Sheet();
            var context = ShowCard(sheet, wall, media);
            var model = sheet.Nav.Model;
            var listen = model.Sections.Single(s => s.Key == BuiltInFamilies.Guides);
            sheet.OpenFullView(listen.Targets[0]);
            Assert.AreEqual(1, sheet.Takeover.PageCount, "Listen: one page");
            Assert.AreEqual(DisplayStyle.None, sheet.Takeover.ChipStrip.style.display.value, "no chips");
            Assert.IsFalse(sheet.FullView.TurnsBetweenBlocks, "nothing to swipe to");
            Assert.AreEqual(context.Strings.Get(listen.LabelKey), sheet.Takeover.CrumbSectionWords.text, "its breadcrumb still names Listen");
            // - the audio guide has no heading of its own: its row's words are its kind's, the section's own word -- said once, not twice
            Assert.AreEqual(context.Strings.Get(listen.LabelKey), sheet.Nav.RowOf(listen.Targets[0]).Words, "precondition: the block's words repeat the section's");
            Assert.IsTrue(sheet.Takeover.NameRepeatsSection);
            Assert.AreEqual(sheet.Stack.HeaderView.TitleText + " > " + context.Strings.Get(listen.LabelKey), sheet.Takeover.CrumbText, "\"Listen > Listen\" reads once");
            Assert.AreEqual(DisplayStyle.None, sheet.Takeover.CrumbName.style.display.value, "the repeated name is not drawn");

            var poi = wall.pois.Single(p => p.id == Lamp);
            string source = poi.card.blocks.First(b => b.kind == "sources").key;
            sheet.OpenFullView(source);
            Assert.IsTrue(sheet.FullView.IsOpen, "precondition: the sources open in the full view");
            Assert.IsFalse(sheet.Takeover.SectionShown, "a block of no section: no section in the breadcrumb");
            Assert.AreEqual(sheet.Stack.HeaderView.TitleText + " > " + sheet.Nav.RowOf(source).Words, sheet.Takeover.CrumbText);

            // - a turn, then the language switch: the sibling the visitor turned to stays, in Portuguese
            sheet.OpenFullView("block_56");
            sheet.Takeover.Turn(+1);
            string shown = sheet.FullView.BlockKey;
            Assert.AreNotEqual("block_56", shown, "precondition: turned to the next sibling");
            var pt = ShowCard(sheet, wall, media, "pt");
            Assert.IsTrue(sheet.FullView.IsOpen, "the language switch keeps the full view");
            Assert.AreEqual(shown, sheet.FullView.BlockKey, "on the sibling shown");
            var media2 = sheet.Nav.Model.Sections.Single(s => s.Key == BuiltInFamilies.Media);
            Assert.AreEqual(pt.Strings.Get(media2.LabelKey), sheet.Takeover.CrumbSectionWords.text, "the section in Portuguese");
            Assert.AreNotEqual(context.Strings.Get(media2.LabelKey), pt.Strings.Get(media2.LabelKey), "precondition: the words differ (Media / its Portuguese word, with its accent)");
            sheet.Hide();
        }

        // The block's own heading in `language`, "" when it authors none (the chip's accessible name is that heading where there is one)
        private static string AuthoredHeading(BlockInstanceData block, string language)
        {
            var field = block.fields.Find(f => f.key == BlockKindDefinition.HeadingField);
            var entry = field?.text.Find(t => t.lang == language);
            return entry?.value ?? "";
        }

        // _3.2 step 17a: a sibling chip carries its block's KIND icon instead of its number, the chip of the page being read adds that block's
        // words after its icon, every other chip stays an icon; each chip is named for a screen reader by its block's heading, in both languages
        [TestCase("en")]
        [TestCase("pt")]
        public void TheSiblingChips_CarryTheirKindsIcon_TheCurrentOneSaysWhatItIs_AndEveryOneIsNamedByItsBlocksHeading(string language)
        {
            var wall = Shipped();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var sheet = Sheet();
            ShowCard(sheet, wall, media, language);
            var poi = wall.pois.Single(p => p.id == Lamp);
            var siblings = FullViewSiblingRule.Siblings(sheet.Nav.Model, BlockRegistry.Shared, "block_58").ToList();
            sheet.OpenFullView("block_58");
            var takeover = sheet.Takeover;
            int at = siblings.IndexOf("block_58");
            int withHeading = 0;
            var icons = new System.Collections.Generic.List<string>();
            for (int i = 0; i < siblings.Count; i++)
            {
                var block = poi.card.blocks.Single(b => b.key == siblings[i]);
                BlockRegistry.Shared.TryGet(block.kind, out var kind);
                string when = language + " chip " + i + " (" + block.kind + ")";
                Assert.AreEqual(kind.IconKey, CardIcons.KeyOf(takeover.ChipIcon(i)), when + ": its block's KIND icon");
                icons.Add(kind.IconKey);
                Assert.AreEqual(DisplayStyle.Flex, takeover.ChipIcon(i).style.display.value, when + ": the icon is drawn");
                Assert.IsTrue(takeover.Chips[i].ClassListContains(TakeoverView.IconChipClass), when + ": an icon chip");
                Assert.AreEqual("", ((Button)takeover.Chips[i]).text, when + ": no number on it any more");
                // - the accessible name: the block's own heading where it has one, else the words its row shows
                string heading = AuthoredHeading(block, language);
                if (heading.Length > 0)
                {
                    Assert.AreEqual(heading, takeover.Chips[i].tooltip, when + ": named by its block's heading in " + language);
                    withHeading++;
                }
                Assert.AreEqual(sheet.Nav.RowOf(siblings[i]).Words, takeover.Chips[i].tooltip, when + ": and always by the words its row shows");
                // - only the page being read says its words
                bool current = i == at;
                Assert.AreEqual(current ? DisplayStyle.Flex : DisplayStyle.None, takeover.ChipWords(i).style.display.value, when + ": words only on the current chip");
                if (current) Assert.AreEqual(takeover.Chips[i].tooltip, takeover.ChipWords(i).text, when + ": the current chip reads its block's words");
            }
            Assert.Greater(withHeading, 3, "precondition: several Media blocks of The Lamp author a heading in " + language);
            Assert.Greater(icons.Distinct().Count(), 3, "the 14 Media blocks show several different icons, not one");

            // - a turn moves the words with the page; a chip tap goes to its page and the words follow
            takeover.Turn(+1);
            Assert.AreEqual(DisplayStyle.None, takeover.ChipWords(at).style.display.value, "the chip left shows an icon again");
            Assert.AreEqual(DisplayStyle.Flex, takeover.ChipWords(at + 1).style.display.value, "the chip arrived at says its words");
            Assert.AreEqual(sheet.Nav.RowOf(siblings[at + 1]).Words, takeover.ChipWords(at + 1).text);
            sheet.Hide();
        }

        // A takeover that is not a sibling set (the gallery's lightbox: pictures, not blocks of different kinds) keeps its page numbers
        [Test]
        public void ALightboxOfPictures_KeepsItsPageNumbers_NoIconsNoWords()
        {
            var wall = Shipped();
            var sheet = Sheet();
            ShowCard(sheet, wall, new ResourcesMediaSource(wall.card_settings.media_resources_path));
            sheet.OpenTakeover("Gallery", 3, 1, (view, container, media, page) => { });
            var takeover = sheet.Takeover;
            Assert.AreEqual(3, takeover.Chips.childCount);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual((i + 1).ToString(), ((Button)takeover.Chips[i]).text, "chip " + i + " is its number");
                Assert.IsFalse(takeover.Chips[i].ClassListContains(TakeoverView.IconChipClass));
                Assert.AreEqual(DisplayStyle.None, takeover.ChipIcon(i).style.display.value, "no icon");
                Assert.AreEqual(DisplayStyle.None, takeover.ChipWords(i).style.display.value, "no words");
            }
            sheet.Hide();
        }
    }
}
