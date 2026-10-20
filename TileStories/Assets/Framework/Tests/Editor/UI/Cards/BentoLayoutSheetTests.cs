using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The bento layout (_3.2 section 6.1, step 15b) on a REAL sheet with The Lamp's real card, the shipped words and the wall's real media
    // folder -- what a jump does in this layout and what Current means, with no panel (Phase B does the same with real taps on the scene):
    //   - every section set aside (no view, no file), a tile per section in the model's order with its words and count, the most-read one leading
    //   - a jump to a block of a section opens THAT section in the full view (its targets in order, a second view each, through the takeover's
    //     scope), one nav_jump; a jump to the sources (no section) scrolls the overview and closes the full view
    //   - Current: none on the overview, the open section while it is open (a Top Bar marks the same), none again after Back
    //   - no sheet on a sheet: an open menu closes before the full view opens, a second jump replaces the page, ONE Back reaches the bento
    //   - Back and closing give every file back; a rebind of the same point keeps the section open; a short card scrolls
    public class BentoLayoutSheetTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private static WallConfigData BentoWall(Action<CardNavigationSettings> more = null)
        {
            var wall = JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));
            wall.card_settings.navigation.layout = CardOptions.LayoutBento;
            more?.Invoke(wall.card_settings.navigation);
            return wall;
        }

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private static BlockBindContext ShowCard(PoiCardSheetView sheet, WallConfigData wall, IMediaSource media, ICardEvents events,
            string poiId = Lamp, string language = "en")
        {
            var poi = wall.pois.Single(p => p.id == poiId);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext { Media = media }, poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            context.Events = events;
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
            return context;
        }

        private static List<CardEvent> NavJumps(RecordingCardEvents events) => events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        [Test]
        public void TheLamp_SetsEverySectionAside_DrawsATilePerSection_AndAJumpOpensThatSectionInTheFullView()
        {
            var wall = BentoWall();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var events = new RecordingCardEvents();
            var sheet = Sheet();
            var context = ShowCard(sheet, wall, media, events);
            var model = sheet.Nav.Model;
            Assert.AreEqual(8, model.Sections.Count, "precondition: The Lamp's eight sections");
            Assert.AreEqual(CardOptions.LayoutBento, sheet.Layout.Effective);
            Assert.IsTrue(sheet.Bento.IsOn);
            var firstSlot = sheet.Stack.Scroll.contentContainer.Children().First(c => c.ClassListContains("card-block-slot"));
            Assert.AreEqual(sheet.Stack.Scroll.contentContainer.IndexOf(firstSlot) - 1, sheet.Stack.Scroll.contentContainer.IndexOf(sheet.Bento.Host),
                "the tiles' host right before the first block");
            Assert.AreSame(sheet.Stack.Scroll.contentContainer, sheet.Bento.Host.hierarchy.parent, "in the stack's scroll, where the blocks were");

            // - the overview: every target set aside, the sources kept under the tiles
            foreach (string target in model.Targets)
            {
                Assert.IsTrue(sheet.Stack.IsParked(target), target + ": set aside");
                Assert.IsNull(sheet.Stack.ViewOfBlock(target), target + ": no view");
                Assert.AreEqual(0, sheet.Stack.HeldMediaOf(target), target + ": no file");
            }
            var sources = wall.pois.Single(p => p.id == Lamp).card.blocks.Where(b => b.kind == "sources").Select(b => b.key).ToList();
            Assert.AreEqual(2, sources.Count, "precondition: The Lamp's two sources blocks");
            foreach (string key in sources)
            {
                Assert.IsFalse(sheet.Stack.IsParked(key), key + ": a block of no section stays on the overview");
                Assert.IsNotNull(sheet.Stack.ViewOfBlock(key), key + ": drawn");
            }

            // - a tile per section, its words and count, the section with the most blocks leading
            Assert.AreEqual(model.Sections.Count, sheet.Bento.Tiles.Count);
            int lead = BentoLayoutRule.Lead(model.Sections.Select(s => s.Count).ToArray());
            for (int i = 0; i < model.Sections.Count; i++)
            {
                var tile = sheet.Bento.Tiles[i];
                Assert.AreEqual(context.Strings.Get(model.Sections[i].LabelKey), tile.Words.text, "tile " + i + ": its section's words");
                Assert.AreEqual(model.Sections[i].Count.ToString(), tile.Count.text, "tile " + i + ": its block count");
                Assert.AreEqual(model.Sections[i].IconKey != null, tile.Icon.style.display.value == DisplayStyle.Flex, "tile " + i + ": its family icon");
                Assert.AreEqual(i == lead, tile.Button.ClassListContains(CardBentoView.LeadClass), "tile " + i + ": the lead or not");
            }
            Assert.AreEqual(-1, sheet.Nav.Current, "the overview: no section is being read");
            Assert.IsNull(sheet.Nav.CurrentBlock, "and no block");
            int loaded = media.LoadedCount;

            // - a jump to a section: that section in the full view, one nav_jump
            int mediaIndex = model.Sections.ToList().FindIndex(s => s.Key == "media");
            Assert.GreaterOrEqual(mediaIndex, 0, "precondition: The Lamp has a Media section");
            var media1 = model.Sections[mediaIndex];
            Assert.IsTrue(sheet.Nav.JumpTo(mediaIndex, CardBentoView.ElementKey), "the jump happened");
            Assert.IsTrue(sheet.FullView.IsOpen, "its section is open in the full view");
            Assert.AreEqual(media1.Key, sheet.FullView.SectionKey);
            CollectionAssert.AreEqual(media1.Targets, sheet.FullView.SectionBlocks.ToList(), "every target of the section, in order");
            foreach (string target in media1.Targets)
            {
                Assert.IsNotNull(sheet.FullView.SectionViewOf(target), target + ": a view on the page");
                Assert.IsNull(sheet.Stack.ViewOfBlock(target), target + ": the card's own stays set aside (bound once)");
            }
            Assert.Greater(sheet.Takeover.HeldMedia, 0, "the page loads its pictures through the takeover's scope");
            StringAssert.EndsWith(" > " + context.Strings.Get(media1.LabelKey), sheet.Takeover.Crumb.text, "the breadcrumb ends with the section's words");
            Assert.AreEqual(mediaIndex, sheet.Nav.Current, "Current: the open section");
            Assert.AreEqual(media1.Targets[0], sheet.Nav.CurrentBlock, "CurrentBlock: the block it opened at");
            Assert.AreEqual(mediaIndex, sheet.Layout.OpenSection);
            var jumps = NavJumps(events);
            Assert.AreEqual(1, jumps.Count, "one nav_jump");
            Assert.AreEqual(CardBentoView.ElementKey, jumps[0].Variant);
            Assert.AreEqual(media1.Key, jumps[0].Value, "to the section");
            Assert.AreEqual(media1.Targets[0], jumps[0].BlockKey);

            // - Back: the overview again, nothing being read, every file of the page given back, the card where it was
            Assert.IsTrue(sheet.Back());
            Assert.IsFalse(sheet.FullView.IsOpen);
            Assert.IsNull(sheet.FullView.SectionKey);
            Assert.AreEqual(-1, sheet.Nav.Current, "Current: none again");
            Assert.IsNull(sheet.Nav.CurrentBlock);
            Assert.AreEqual(0, sheet.Takeover.HeldMedia, "the page holds nothing");
            Assert.AreEqual(loaded, media.LoadedCount, "the card holds exactly what the overview held");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "one Back reached the bento, the card did not move");

            // - a jump to a block in the middle of a section: that section, at that block
            int storiesIndex = model.Sections.ToList().FindIndex(s => s.Targets.Count >= 3 && s.Key != "media");
            var stories = model.Sections[storiesIndex];
            string middle = stories.Targets[2];
            Assert.IsTrue(sheet.Nav.JumpToBlock(middle, "nav_contents"));
            Assert.AreEqual(stories.Key, sheet.FullView.SectionKey, "the block's section");
            Assert.AreEqual(middle, sheet.Layout.OpenBlock, "opened at the block");
            Assert.AreEqual(storiesIndex, sheet.Nav.Current);
            Assert.AreEqual(middle, sheet.Nav.CurrentBlock);

            // - a jump to the sources: no section -- the full view closes and the overview takes the jump
            events.Raised.Clear();
            Assert.IsTrue(sheet.Nav.JumpToBlock(sources[0], "actions"), "the sources are in the overview's scroll");
            Assert.IsFalse(sheet.FullView.IsOpen, "the full view closed (the card moves under it)");
            Assert.AreEqual(1, NavJumps(events).Count);
            Assert.AreEqual("", NavJumps(events)[0].Value, "a block of no section");

            sheet.Hide();
            Assert.AreEqual(0, media.LoadedCount, "closing the card holds nothing");
            Assert.IsFalse(sheet.Bento.IsOn);
            Assert.IsNull(sheet.Bento.Host.parent, "closed: the host left the scroll");
        }

        // NN/g bottom sheets (Laubheimer 2023): never a sheet over a sheet. The card's ONE overlay owner (step 8) is used, no second rule: an
        // open menu closes as the jump starts, the full view opens alone; a second jump from inside it REPLACES the page; one Back reaches the
        // bento, the next lowers the card
        [Test]
        public void NoSheetOnASheet_AMenuClosesBeforeTheFullView_ASecondJumpReplacesIt_AndOneBackReachesTheBento()
        {
            var wall = BentoWall(nav => nav.menu = BuiltInNavElements.DrawerTree);
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var events = new RecordingCardEvents();
            var sheet = Sheet();
            ShowCard(sheet, wall, media, events);
            var model = sheet.Nav.Model;

            sheet.Overlay.OpenMenu();
            Assert.IsTrue(sheet.Overlay.IsOpen, "precondition: the drawer is open over the card");
            Assert.IsTrue(sheet.Nav.JumpTo(2, BuiltInNavElements.DrawerTree), "a pick in the drawer");
            Assert.IsFalse(sheet.Overlay.IsOpen, "the drawer closed");
            Assert.IsTrue(sheet.FullView.IsOpen, "the section opened alone");
            Assert.AreEqual(model.Sections[2].Key, sheet.FullView.SectionKey);

            int heldBefore = sheet.Takeover.HeldMedia;
            Assert.IsTrue(sheet.Nav.JumpTo(1, "actions"), "an Open Block from inside the full view");
            Assert.IsTrue(sheet.Takeover.IsOpen && sheet.FullView.IsOpen);
            Assert.AreEqual(model.Sections[1].Key, sheet.FullView.SectionKey, "the page was replaced, not stacked");
            CollectionAssert.AreEqual(model.Sections[1].Targets, sheet.FullView.SectionBlocks.ToList(), "only the new section's blocks are on it");
            Assert.IsNull(sheet.FullView.SectionViewOf(model.Sections[2].Targets[0]), "the old section's views left");
            Assert.AreEqual(1, sheet.Nav.Current);

            Assert.IsTrue(sheet.Back());
            Assert.IsFalse(sheet.Takeover.IsOpen, "ONE Back: the bento");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);
            Assert.IsTrue(sheet.Back());
            Assert.AreEqual(SheetStopRule.Stop.Half, sheet.Stop, "the next one lowers the card");
            Assert.Greater(heldBefore, -1);
        }

        // An element bound in a slot must never mark something false: the Text Tabs over the bento mark nothing on the overview, the open
        // section while it is open (a tab tap opens it), nothing after Back
        [Test]
        public void ATopBarOverTheBento_MarksNothingOnTheOverview_AndTheOpenSectionWhileItIsOpen()
        {
            var wall = BentoWall(nav => nav.top = BuiltInNavElements.TextTabs);
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var sheet = Sheet();
            ShowCard(sheet, wall, media, new RecordingCardEvents());
            var tabs = sheet.Slots.TopView as TextTabsNavView;
            Assert.IsNotNull(tabs, "precondition: the wall's Top Bar is drawn over the bento");
            Assert.AreEqual(-1, tabs.CurrentIndex, "the overview: no tab marked");
            Assert.IsFalse(tabs.Tabs.Any(t => t.Button.ClassListContains(NavTab.CurrentClass)));

            sheet.Nav.JumpTo(3, BuiltInNavElements.TextTabs);
            Assert.AreEqual(3, tabs.CurrentIndex, "a tab's jump: its section open, its tab marked");
            Assert.IsTrue(tabs.Tabs.Single(t => t.Index == 3).Button.ClassListContains(NavTab.CurrentClass));
            sheet.Back();
            Assert.AreEqual(-1, tabs.CurrentIndex, "back on the overview: none again");
        }

        // A rebind of the same point (a language switch, a live edit) keeps the section open, drawn again in the new words; another point
        // closes it; a card with nothing to switch between (Lamp - Military, under Show Navigation From) simply scrolls -- no tiles, nothing set aside
        [Test]
        public void ASamePointRebind_KeepsTheSectionOpen_AnotherPointClosesIt_AndAShortCardScrolls()
        {
            var wall = BentoWall();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var sheet = Sheet();
            ShowCard(sheet, wall, media, new RecordingCardEvents());
            string key = sheet.Nav.Model.Sections[1].Key;
            sheet.Nav.JumpTo(1, CardBentoView.ElementKey);
            var pt = ShowCard(sheet, wall, media, new RecordingCardEvents(), language: "pt");
            Assert.IsTrue(sheet.FullView.IsOpen, "a language switch keeps it open");
            Assert.AreEqual(key, sheet.FullView.SectionKey);
            StringAssert.EndsWith(" > " + pt.Strings.Get(sheet.Nav.Model.Sections[1].LabelKey), sheet.Takeover.Crumb.text, "in Portuguese");
            Assert.AreEqual(1, sheet.Nav.Current);

            ShowCard(sheet, wall, media, new RecordingCardEvents(), LampMilitary);
            Assert.IsFalse(sheet.FullView.IsOpen, "another point closes it");
            Assert.AreEqual(CardOptions.LayoutScroll, sheet.Layout.Effective, "Military: nothing to switch between");
            Assert.IsFalse(sheet.Bento.IsOn);
            Assert.IsNull(sheet.Bento.Host.parent, "no tiles: the host is not in the scroll, which holds exactly the blocks as in Scroll");
            foreach (string target in sheet.Nav.Model.Targets) Assert.IsFalse(sheet.Stack.IsParked(target), target + ": nothing set aside");
            sheet.Hide();
            Assert.AreEqual(0, media.LoadedCount);
        }
    }
}
