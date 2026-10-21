using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace TileStories.Tests
{
    // Phase B of the in-card facets (_3.2 step 18, section 5.8) on the REAL LivingRoomScene with The Lamp's real card (56 targets, 8
    // families, Grouped By Family) and real fingers (CardTestInput, 4.2.3's focus settings). What a chip must keep is worked out here
    // WITHOUT the code under test: the kind's own Format / Needs, the block's own Reading Time field and the length of its clip loaded
    // straight from Resources. Then:
    //   - real taps on the drawer's chips narrow it to exactly the matching blocks (every section's and kind's count), chips combine, Clear
    //     gives the whole outline back
    //   - a real tap on a section line or a kind line of a FILTERED drawer lands the first block that passes, not the section's first
    //   - the same on the big tiles, on both axes
    //   - the stops do not move; renders at 390 px in English and Portuguese, unfiltered and filtered
    public class PoiCardFacetSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;
        private CardMenuOverlay Overlay => Sheet.Overlay;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private IEnumerator UseMenu(string menu, string language = null)
        {
            var copy = ConfigCopy();
            var nav = copy.card_settings.navigation;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.top = CardOptions.NavNone;
            nav.bottom = CardOptions.NavNone;
            nav.menu = menu;
            if (language != null) copy.card_settings.preview_language = language;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator OpenTheLamp(SheetStopRule.Stop stop)
        {
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // A real tap on a button the visitor sees: inside the card layer and, for one in a scroll, inside its visible part
        private IEnumerator Tap(VisualElement button, string name, ScrollView inside = null)
        {
            var screen = Sheet.Layer.worldBound;
            Assert.IsTrue(screen.Contains(button.worldBound.center), "precondition: " + name + " is on screen " + button.worldBound + " in " + screen);
            if (inside != null)
                Assert.IsTrue(inside.contentViewport.worldBound.Contains(button.worldBound.center),
                    "precondition: " + name + " is in the visible part of its list " + button.worldBound + " in " + inside.contentViewport.worldBound);
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, button.worldBound.center, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
        }

        private IEnumerator OpenTheMenu()
        {
            yield return Tap(Sheet.MenuButton, "the menu button");
            Assert.IsTrue(Overlay.IsModal, "precondition: the menu is open");
        }

        // A real tap on the chip the rule offers that `which` picks: the strip is brought to it first, as a finger would
        private IEnumerator TapChip(NavFacetBar bar, Func<NavFacetRule.Chip, bool> which, string name)
        {
            int i = bar.Offered.ToList().FindIndex(c => which(c));
            Assert.GreaterOrEqual(i, 0, "precondition: the card offers the chip " + name + " (" + string.Join(", ", bar.Offered.Select(c => c.Dimension + ":" + c.Value)) + ")");
            bar.Strip.Show(bar.Chips[i].Button);
            yield return CardTestInput.Settle(0.2f);
            yield return Tap(bar.Chips[i].Button, name);
        }

        private IEnumerator TapClear(NavFacetBar bar)
        {
            Assert.IsTrue(bar.HasClear, "precondition: a facet is on, so Clear shows");
            var clear = bar.Chips[bar.Chips.Count - 1].Button;
            bar.Strip.Show(clear);
            yield return CardTestInput.Settle(0.2f);
            yield return Tap(clear, "Clear");
        }

        // A block's top meets the viewport's top (+-1 px), or -- a late block -- the stack sits at the end of its range
        private void AssertLanded(string block, string name)
        {
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (slot.layout.y <= Range - OnePixel)
                Assert.AreEqual(0f, slot.worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its block's top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // ---------------- what a chip must keep, worked out without the code under test ----------------

        // A block's minutes read the long way: its Reading Time field, or the length of its clip loaded straight from Resources
        private int MinutesOf(BlockStackBuilder.Entry entry)
        {
            var kind = entry.Definition;
            if (kind.ClipLengthField == null)
            {
                var field = kind.Field(BlockKindDefinition.MinutesField);
                return field == null ? 0 : Mathf.RoundToInt(new BlockFieldReader(entry.Instance, "en", "en").Number(field));
            }
            var clipField = kind.Field(kind.ClipLengthField);
            string path = new BlockFieldReader(entry.Instance, "en", "en").ValidAsset(clipField.Key, clipField.Media);
            if (path.Length == 0) return 0;
            string resource = ConfigCopy().card_settings.media_resources_path + "/" + System.IO.Path.ChangeExtension(path, null);
            float seconds = clipField.Media == MediaKind.Audio
                ? (Resources.Load<AudioClip>(resource) is AudioClip a ? a.length : 0f)
                : (Resources.Load<VideoClip>(resource) is VideoClip v ? (float)v.length : 0f);
            Assert.Greater(seconds, 0f, "precondition: the clip of " + entry.Instance.key + " loads from " + resource);
            return Mathf.CeilToInt(seconds / 60f);
        }

        private BlockStackBuilder.Entry EntryOf(string block)
        {
            Assert.IsTrue(Nav.TryGetEntry(block, out var entry), block + " is on the card");
            return entry;
        }

        // The Lamp's targets that pass, by the plain meaning of each facet
        private List<string> Keep(Func<BlockStackBuilder.Entry, bool> passes) => Model.Targets.Where(t => passes(EntryOf(t))).ToList();

        private bool NeedsAny(BlockStackBuilder.Entry e, string need) => e.Definition.Needs != null && e.Definition.Needs.Contains(need);

        // The drawer must list exactly these blocks: a section line per section that holds one, each with its count, and a kind line per kind
        // of a section with a choice of kinds, each with its count -- all counted from `kept`, never from the view
        private void AssertTheDrawerListsExactly(DrawerTreeNavView drawer, List<string> kept, string when)
        {
            var sections = Enumerable.Range(0, Model.Sections.Count).Where(s => Model.Sections[s].Targets.Any(kept.Contains)).ToList();
            Assert.AreEqual(sections.Count, drawer.SectionRows.Count, when + ": a line per section that holds a kept block");
            for (int i = 0; i < sections.Count; i++)
            {
                Assert.AreEqual(sections[i], drawer.SectionOf(i), when + ": line " + i);
                Assert.AreEqual(Model.Sections[sections[i]].Targets.Count(kept.Contains).ToString(), drawer.SectionRows[i].Count.text, when + ": the count of " + Model.Sections[sections[i]].Key);
            }
            var expectedKinds = sections.SelectMany(s =>
            {
                var kinds = Model.Sections[s].Kinds.Where(k => k.Targets.Any(kept.Contains)).ToList();
                return kinds.Count >= 2 ? kinds.Select(k => (s, k)) : Enumerable.Empty<(int, CardNavModel.Kind)>();
            }).ToList();
            Assert.AreEqual(expectedKinds.Count, drawer.KindRows.Count, when + ": a line per kept kind of a section with a choice");
            for (int i = 0; i < expectedKinds.Count; i++)
            {
                Assert.AreSame(expectedKinds[i].Item2, drawer.KindRows[i].Kind, when + ": kind line " + i);
                Assert.AreEqual(expectedKinds[i].Item2.Targets.Count(kept.Contains).ToString(), drawer.KindRows[i].Row.Count.text, when + ": the count of kind " + expectedKinds[i].Item2.Key);
            }
        }

        // ---------------- the stops ----------------

        [UnityTest]
        public IEnumerator TheLampsStops_DoNotMove_WithTheFacetChipsInTheMenu()
        {
            foreach (string menu in new[] { BuiltInNavElements.DrawerTree, BuiltInNavElements.GridOverlay })
            {
                yield return UseMenu(menu);
                foreach (var (stop, height) in new[] { (SheetStopRule.Stop.Full, 796.00f), (SheetStopRule.Stop.Half, 337.60f), (SheetStopRule.Stop.Peek, 195.00f) })
                {
                    yield return OpenTheLamp(stop);
                    Assert.AreEqual(height, Sheet.Stops.HeightOf(stop), 0.05f, menu + " at " + stop + ": the stops are the card's own (the chips live in the menu layer)");
                    Assert.AreEqual(height, Sheet.Root.layout.height, OnePixel, menu + " at " + stop + ": and the sheet rests there, to the physical pixel");
                }
            }
        }

        // ---------------- the drawer ----------------

        [UnityTest]
        public IEnumerator TheDrawer_RealTapsOnChips_NarrowItToExactlyTheMatchingBlocks_TheyCombine_AndClearBringsItAllBack()
        {
            yield return UseMenu(BuiltInNavElements.DrawerTree);
            yield return OpenTheLamp(SheetStopRule.Stop.Full);
            yield return OpenTheMenu();
            var drawer = (DrawerTreeNavView)Sheet.Slots.MenuView;
            var bar = drawer.Facets;
            AssertTheDrawerListsExactly(drawer, Model.Targets.ToList(), "unfiltered");
            Assert.AreEqual(56, Model.Targets.Count, "precondition: The Lamp's 56 targets");

            // - the chips offered: both time limits, headphones and camera (the audio guide, place in AR and show on wall are on The Lamp), no network
            CollectionAssert.AreEqual(new[] { "2", "5" }, bar.Offered.Where(c => c.Dimension == NavFacetRule.Dimension.Time).Select(c => c.Value));
            CollectionAssert.AreEqual(new[] { CardOptions.NeedHeadphones, CardOptions.NeedCamera },
                bar.Offered.Where(c => c.Dimension == NavFacetRule.Dimension.Without).Select(c => c.Value), "network: nothing on The Lamp needs it");
            Assert.IsFalse(bar.HasClear, "no Clear while nothing is on");

            // - Up to 5 min: exactly the blocks with a time of 1..5 min (the four timed ones, the audio guide and the clips that short)
            var fast = Keep(e => { int m = MinutesOf(e); return m >= 1 && m <= NavFacetRule.MediumMinutes; });
            Assert.GreaterOrEqual(fast.Count, 5, "precondition: at least the four timed blocks and the audio guide fit five minutes");
            Assert.Less(fast.Count, 56, "precondition: the limit really narrows");
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Time && c.Minutes == 5, "Up to 5 min");
            AssertTheDrawerListsExactly(drawer, fast, "up to 5 min");
            Assert.IsTrue(bar.HasClear, "Clear appears");

            // - and without headphones: the same list less the audio guide, so Listen (its only block) leaves the outline
            var fastNoPhones = fast.Where(t => !NeedsAny(EntryOf(t), CardOptions.NeedHeadphones)).ToList();
            Assert.Less(fastNoPhones.Count, fast.Count, "precondition: the audio guide was in the short list");
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Without && c.Value == CardOptions.NeedHeadphones, "No headphones");
            AssertTheDrawerListsExactly(drawer, fastNoPhones, "up to 5 min and no headphones");
            Assert.IsFalse(Enumerable.Range(0, drawer.SectionRows.Count).Any(i => Model.Sections[drawer.SectionOf(i)].Key == BuiltInFamilies.Guides),
                "Listen holds only the audio guide: it left the outline");

            // - a second tap on a chip that is on switches it off; Clear switches the rest off
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Without && c.Value == CardOptions.NeedHeadphones, "No headphones (off)");
            AssertTheDrawerListsExactly(drawer, fast, "headphones chip off again");
            yield return TapClear(bar);
            AssertTheDrawerListsExactly(drawer, Model.Targets.ToList(), "cleared");
            Assert.IsFalse(bar.Facets.IsActive);
            Assert.IsFalse(bar.HasClear);
        }

        [UnityTest]
        public IEnumerator TheDrawer_AFormatChip_KeepsOnlyThatFormat_AndTheOtherChipsStillAnswerAgainstWhatIsLeft()
        {
            yield return UseMenu(BuiltInNavElements.DrawerTree);
            yield return OpenTheLamp(SheetStopRule.Stop.Full);
            yield return OpenTheMenu();
            var drawer = (DrawerTreeNavView)Sheet.Slots.MenuView;
            var bar = drawer.Facets;
            var watch = Keep(e => e.Definition.Format == CardOptions.FormatWatch);
            Assert.IsNotEmpty(watch, "precondition: The Lamp holds Watch blocks (its videos)");
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Format && c.Value == CardOptions.FormatWatch, "Watch");
            AssertTheDrawerListsExactly(drawer, watch, "Watch");
            // - nothing is left that needs headphones or a camera: those chips would change nothing, so they are not offered over this list
            Assert.IsFalse(bar.Offered.Any(c => c.Dimension == NavFacetRule.Dimension.Without), "no needs chip over blocks that need nothing");
            Assert.IsTrue(bar.Offered.Any(c => c.Dimension == NavFacetRule.Dimension.Format && c.Value == CardOptions.FormatWatch && c.Active), "the chip that is on stays");
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Format && c.Value == CardOptions.FormatListen, "Listen (switch)");
            AssertTheDrawerListsExactly(drawer, Keep(e => e.Definition.Format == CardOptions.FormatListen), "Listen replaces Watch: one format at a time");
        }

        [UnityTest]
        public IEnumerator AJumpFromAFilteredDrawer_LandsTheFirstBlockThatPasses_NotTheSectionsFirst_OneNavJump()
        {
            yield return UseMenu(BuiltInNavElements.DrawerTree);
            yield return OpenTheLamp(SheetStopRule.Stop.Full);
            yield return OpenTheMenu();
            var drawer = (DrawerTreeNavView)Sheet.Slots.MenuView;
            var bar = drawer.Facets;
            var fast = Keep(e => { int m = MinutesOf(e); return m >= 1 && m <= NavFacetRule.MediumMinutes; });
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Time && c.Minutes == 5, "Up to 5 min");

            // - a SECTION line whose first block is not timed: it must land its first timed block
            int row = Enumerable.Range(0, drawer.SectionRows.Count).First(i =>
            {
                var section = Model.Sections[drawer.SectionOf(i)];
                return !fast.Contains(section.Targets[0]) && drawer.List.contentViewport.worldBound.Contains(drawer.SectionRows[i].Button.worldBound.center);
            });
            var picked = Model.Sections[drawer.SectionOf(row)];
            string expected = picked.Targets.First(fast.Contains);
            Assert.AreNotEqual(picked.Targets[0], expected, "precondition: the section's first block is not the first that passes");
            int before = NavJumps.Count;
            yield return Tap(drawer.SectionRows[row].Button, picked.Key + "'s line", drawer.List);
            Assert.IsFalse(Overlay.IsOpen, "the pick closed the drawer");
            yield return Landed();
            AssertLanded(expected, picked.Key);
            Assert.AreEqual(before + 1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual(expected, NavJumps.Last().BlockKey, "to the first block that passes");
            Assert.AreEqual(BuiltInNavElements.DrawerTree, NavJumps.Last().Variant, "reported as the drawer's");

            // - the facets stay for the card: reopened, the drawer is still narrowed
            yield return OpenTheMenu();
            AssertTheDrawerListsExactly(drawer, fast, "reopened over the same card");

            // - a KIND line: the first block of that kind that passes
            var kindRow = drawer.KindRows.FirstOrDefault(k => k.Kind.Targets[0] != k.Kind.Targets.FirstOrDefault(fast.Contains)
                && drawer.List.contentViewport.worldBound.Contains(k.Row.Button.worldBound.center));
            if (kindRow.Row != null)
            {
                string expectedKind = kindRow.Kind.Targets.First(fast.Contains);
                before = NavJumps.Count;
                yield return Tap(kindRow.Row.Button, kindRow.Kind.Key + "'s line", drawer.List);
                yield return Landed();
                AssertLanded(expectedKind, kindRow.Kind.Key);
                Assert.AreEqual(before + 1, NavJumps.Count, "one nav_jump");
                Assert.AreEqual(expectedKind, NavJumps.Last().BlockKey, "the kind's first block that passes");
            }
        }

        [UnityTest]
        public IEnumerator AnotherCard_StartsWithEveryFacetOff()
        {
            yield return UseMenu(BuiltInNavElements.DrawerTree);
            yield return OpenTheLamp(SheetStopRule.Stop.Full);
            yield return OpenTheMenu();
            var bar = ((DrawerTreeNavView)Sheet.Slots.MenuView).Facets;
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Time && c.Minutes == 2, "Up to 2 min");
            Assert.IsTrue(bar.Facets.IsActive, "precondition: a facet is on");
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            yield return OpenTheLamp(SheetStopRule.Stop.Full);
            Assert.IsFalse(((DrawerTreeNavView)Sheet.Slots.MenuView).Facets.Facets.IsActive, "a card opened afresh starts with nothing filtered");
        }

        // ---------------- the big tiles ----------------

        [UnityTest]
        public IEnumerator TheTiles_RealTapsOnChips_NarrowThemOnBothAxes_AndATapLandsTheFirstBlockThatPasses()
        {
            yield return UseMenu(BuiltInNavElements.GridOverlay);
            yield return OpenTheLamp(SheetStopRule.Stop.Full);
            yield return OpenTheMenu();
            var grid = (GridOverlayNavView)Sheet.Slots.MenuView;
            var bar = grid.Facets;
            var noCamera = Keep(e => !NeedsAny(e, CardOptions.NeedCamera));
            var camera = Model.Targets.Except(noCamera).ToList();
            Assert.GreaterOrEqual(camera.Count, 2, "precondition: The Lamp's place in AR and show on wall blocks");
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Without && c.Value == CardOptions.NeedCamera, "No camera");

            // - Purpose: a tile per family that keeps a block, each counting only what is kept
            var byFamily = Enumerable.Range(0, Model.Sections.Count).Where(s => Model.Sections[s].Targets.Any(noCamera.Contains)).ToList();
            Assert.AreEqual(byFamily.Count, grid.Tiles.Count, "a tile per family that keeps a block");
            for (int i = 0; i < byFamily.Count; i++)
                Assert.AreEqual(Model.Sections[byFamily[i]].Targets.Count(noCamera.Contains).ToString(), grid.Tiles[i].Count.text, "tile " + i + " counts only the kept blocks");
            Assert.AreEqual(noCamera.Count, grid.Tiles.Sum(t => int.Parse(t.Count.text)), "the counts add up to the kept blocks");

            // - Format: the same chips filter the other axis
            yield return Tap(grid.ByFormat, "By Format");
            var formats = Model.By(CardOptions.GroupByFormat);
            var byFormat = Enumerable.Range(0, formats.Sections.Count).Where(s => formats.Sections[s].Targets.Any(noCamera.Contains)).ToList();
            Assert.AreEqual(byFormat.Count, grid.Tiles.Count, "the Format side is narrowed by the same chip");
            Assert.AreEqual(noCamera.Count, grid.Tiles.Sum(t => int.Parse(t.Count.text)), "and counts the same blocks");
            yield return Tap(grid.ByPurpose, "By Purpose");

            // - the In AR family is all camera: no tile for it with No camera on, and it is back with Clear
            Assert.IsFalse(Enumerable.Range(0, grid.Tiles.Count).Any(i => Model.Sections[byFamily[i]].Key == BuiltInFamilies.Ar), "In AR left the tiles");
            yield return TapClear(bar);
            Assert.AreEqual(Model.Sections.Count, grid.Tiles.Count, "Clear: a tile per family again");

            // - a real tap on a tile whose first block does NOT pass lands the first one that does (a section with one kind left goes straight)
            var fast = Keep(e => { int m = MinutesOf(e); return m >= 1 && m <= NavFacetRule.MediumMinutes; });
            yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Time && c.Minutes == 5, "Up to 5 min");
            var kept = Enumerable.Range(0, Model.Sections.Count).Where(s => Model.Sections[s].Targets.Any(fast.Contains)).ToList();
            Assert.AreEqual(kept.Count, grid.Tiles.Count, "a tile per family that keeps a timed block");
            int tile = kept.FindIndex(s => !NavOntologyRule.OffersAKindChoice(Model.Sections[s].Kinds.Count(k => k.Targets.Any(fast.Contains)))
                && !fast.Contains(Model.Sections[s].Targets[0]));
            Assert.GreaterOrEqual(tile, 0, "precondition: a family with one kept kind whose first block is not timed (Media: its videos)");
            var section = Model.Sections[kept[tile]];
            string expected = section.Targets.First(fast.Contains);
            int before = NavJumps.Count;
            yield return Tap(grid.Tiles[tile].Button, "tile " + tile, grid.Scroll);
            Assert.IsFalse(Overlay.IsOpen, "the pick closed the tiles");
            yield return Landed();
            AssertLanded(expected, section.Key);
            Assert.AreEqual(before + 1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual(expected, NavJumps.Last().BlockKey, "the first block that passes, not " + section.Targets[0]);
            Assert.AreEqual(BuiltInNavElements.GridOverlay, NavJumps.Last().Variant);
        }

        // ---------------- renders ----------------

        [UnityTest]
        public IEnumerator TheMenusWithFacets_RenderAt390_UnfilteredAndFiltered_InEnglishAndPortuguese([Values("en", "pt")] string language)
        {
            foreach (string menu in new[] { BuiltInNavElements.DrawerTree, BuiltInNavElements.GridOverlay })
            {
                yield return UseMenu(menu, language);
                yield return OpenTheLamp(SheetStopRule.Stop.Full);
                yield return OpenTheMenu();
                var view = Sheet.Slots.MenuView;
                var bar = view is DrawerTreeNavView drawer ? drawer.Facets : ((GridOverlayNavView)view).Facets;
                yield return CardGalleryChecks.Render("Nav_Lamp_" + menu + "_facets_rest_" + language + "_390");
                yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Time && c.Minutes == 5, "Up to 5 min");
                yield return TapChip(bar, c => c.Dimension == NavFacetRule.Dimension.Without && c.Value == CardOptions.NeedHeadphones, "No headphones");
                yield return CardGalleryChecks.Render("Nav_Lamp_" + menu + "_facets_filtered_" + language + "_390");
                yield return TapClear(bar);
                Overlay.Close();
                yield return CardTestInput.Settle();
            }
        }
    }
}
