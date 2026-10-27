using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileStories
{
    // Phase A data list of the card's NAVIGATION ELEMENTS (_3.2 section 9, 40-testing.md 4.4): every element x every fixture case,
    // fabricated -- no AR, no config.json. The ONE list both CardNavGalleryHarness (to look at, Assets/Dev/CardNavGallery) and
    // CardNavGalleryTests (to assert) read, so the two cannot drift apart. To add an element: add its key to ElementKeys (one row);
    // to add a case: add one row to Cases. A case's card is made of real gallery blocks (CardGalleryDefinitions' self-contained
    // entries), copied as often as the case asks, each copy its own block, so the element runs over a real stack.
    // The in-flow nav BLOCKS (_3.2 section 5.6: nav_icon_grid) are rows of BlockKinds: their wall sets Auto Nav Block to the kind and
    // every slot to none, so the block reaches the card through the real BlockStackBuilder step, right after the header.
    // The card LAYOUTS (_3.2 section 6.1, step 13: accordion, tabs) are rows of Layouts: their wall sets the layout with Text Tabs at the
    // top (what tabs switches sections with, and a jump for the accordion), every other slot empty.
    public static class CardNavGalleryDefinitions
    {
        // One fixture card: which gallery block, how many copies of it (in this order, after the header)
        public sealed class Case
        {
            public readonly string Key;
            public readonly string Language;
            public readonly IReadOnlyList<(string GalleryEntry, int Copies)> Blocks;

            public Case(string key, string language, params (string, int)[] blocks)
            {
                Key = key;
                Language = language;
                Blocks = blocks;
            }
        }

        public readonly struct Entry
        {
            public readonly string Name;
            public readonly string ElementKey;
            public readonly Case Case;
            // True for a nav block kind (BlockKinds), drawn in the stack's flow instead of a slot
            public readonly bool IsBlock;
            // The card layout the entry judges (Layouts), null for an element or a nav block entry
            public readonly string Layout;

            public Entry(string elementKey, Case fixtureCase, bool isBlock = false, string layout = null)
            {
                ElementKey = elementKey;
                Case = fixtureCase;
                IsBlock = isBlock;
                Layout = layout;
                Name = layout != null ? "Nav_layout_" + layout + "_" + fixtureCase.Key : "Nav_" + elementKey + "_" + fixtureCase.Key;
            }
        }

        // The elements the gallery shows: one row per element, every case drawn for each
        public static readonly IReadOnlyList<string> ElementKeys = new[]
        {
            BuiltInNavElements.TextTabs, BuiltInNavElements.IconTabs, BuiltInNavElements.IconTabsChips, BuiltInNavElements.KindChips,
            BuiltInNavElements.Segmented, BuiltInNavElements.ProgressMap, BuiltInNavElements.FamilyPager, BuiltInNavElements.BottomBar,
            BuiltInNavElements.Dock, BuiltInNavElements.BarSheet, BuiltInNavElements.NextUp,
            BuiltInNavElements.DrawerTree, BuiltInNavElements.GridOverlay,
            BuiltInNavElements.FabDial, BuiltInNavElements.FabRadial,
            BuiltInNavElements.EdgeScrubber,
        };

        // The nav block kinds the gallery shows (_3.2 section 5.6), every case drawn for each, the same way
        public static readonly IReadOnlyList<string> BlockKinds = new[]
        {
            BuiltInBlocks.NavIconGridKind, BuiltInBlocks.NavContentsKind, BuiltInBlocks.NavRailsKind, BuiltInBlocks.NavBentoKind,
        };

        // The blocks (step 15a) the bento's tiles lead to: the first four blocks after the header, in every case. Their words come from the
        // blocks themselves; the first tile also has a picture and the third opens the full view, so one card judges both outcomes
        public static readonly IReadOnlyList<string> BentoTargets = new[] { "block_2", "block_3", "block_4", "block_5" };
        public const string BentoKey = "block_900";

        // The card layouts the gallery shows (_3.2 section 6.1), every case drawn for each, with Text Tabs at the top (pages: the Family Pager)
        public static readonly IReadOnlyList<string> Layouts = new[]
            { CardOptions.LayoutAccordion, CardOptions.LayoutTabs, CardOptions.LayoutPages, CardOptions.LayoutBento };

        // The Reading Time a contents entry's card gives the FIRST copy of each gallery block that carries the field (_3.2 step 11b), so its
        // list shows typed minutes beside the rows with none and the audio clip's own length
        public const int ContentsMinutes = 4;

        // What SlotOf says for a nav block: no slot, the stack's own flow
        public const string InFlow = "in_flow";

        // The gallery's walls show navigation from 2 targets, so the small cases draw their element too (the default threshold is
        // Phase B's to prove, on the real wall)
        public const int ShowNavigationFrom = 2;

        // _3.2 section 9's four fixture cases, and the one-family card with a second family (rule 2.5.7: an element between sections is
        // drawn from two sections, so one_family shows no strip and two_families does)
        public static readonly IReadOnlyList<Case> Cases = new[]
        {
            // - The Lamp's size: 62 targets over the eight families that navigate, and a Sources block (the ninth family, never a target)
            Big("big", "en"),
            // - a short card: 6 targets in 3 families
            new Case("small", "en", ("rich_text_plain_short", 2), ("quick_facts_chips_short", 1), ("gallery_carousel_short", 2), ("timeline_vertical_short", 1)),
            // - one family only: one section
            new Case("one_family", "en", OneFamily),
            // - the big card in Portuguese: the longest section words (Historias, Comunidade...) at the phone width
            Big("pt_long", "pt"),
            // - the same one-family card and one Media block: two sections
            new Case("two_families", "en", OneFamily.Append(("gallery_carousel_short", 1)).ToArray()),
        };

        private static (string, int)[] OneFamily => new[] { ("rich_text_plain_short", 3), ("pull_quote_serif_short", 1), ("process_steps_numbered_short", 1) };

        private static Case Big(string key, string language) => new(key, language,
            ("rich_text_plain_short", 8), ("quick_facts_chips_short", 4), ("fun_fact_flip_short", 3), ("pull_quote_serif_short", 3), ("process_steps_numbered_short", 2),
            ("gallery_carousel_short", 8), ("before_after_slider_short", 4), ("zoom_image_pinch_short", 4),
            ("audio_guide_player_short", 1),
            ("timeline_vertical_short", 3), ("person_row_short", 3), ("story_chapters_segmented_short", 2),
            ("place_in_ar_button_short", 3),
            ("poll_bars_short", 2), ("knowledge_check_multiple_choice_short", 2),
            ("practical_info_rows_short", 4), ("today_map_static_short", 3),
            ("feedback_thumbs_short", 3),
            ("sources_list_short", 1));

        // Every element over every case, then every nav block over every case, then every layout over every case
        public static readonly IReadOnlyList<Entry> All = ElementKeys.SelectMany(key => Cases.Select(c => new Entry(key, c)))
            .Concat(BlockKinds.SelectMany(key => Cases.Select(c => new Entry(key, c, isBlock: true))))
            .Concat(Layouts.SelectMany(layout => Cases.Select(c => new Entry(BuiltInNavElements.TextTabs, c, layout: layout)))).ToList();

        // The slot an entry's element is registered for (top when the registry does not know it; InFlow for a nav block)
        public static string SlotOf(Entry entry) =>
            entry.IsBlock ? InFlow : NavElementRegistry.Shared.TryGet(entry.ElementKey, out var element) ? element.Slot : NavSlotKeys.Top;

        // A menu element (step 8): judged OPEN over the card, the way the header's menu button shows it
        public static bool IsMenu(Entry entry) => SlotOf(entry) == NavSlotKeys.Menu;

        // The fabricated wall of an entry: the card gallery's own, with the entry's language offered and navigation from 2 targets. The
        // element is the wall's pick for ITS slot and every other slot holds nothing, so each render judges one element alone; a nav block
        // is the wall's Auto Nav Block with every slot empty
        public static WallConfigData Wall(Entry entry)
        {
            var wall = CardGalleryDefinitions.Taxonomy();
            string slot = SlotOf(entry);
            wall.card_settings.navigation.top = slot == NavSlotKeys.Top ? entry.ElementKey : CardOptions.NavNone;
            wall.card_settings.navigation.bottom = slot == NavSlotKeys.Bottom ? entry.ElementKey : CardOptions.NavNone;
            wall.card_settings.navigation.menu = slot == NavSlotKeys.Menu ? entry.ElementKey : CardOptions.NavNone;
            wall.card_settings.navigation.floating = slot == NavSlotKeys.Floating ? entry.ElementKey : CardOptions.NavNone;
            // - a Floating Button rests in the camera strip Full leaves (_3.3 S4a: Full Height = Leave A Camera Strip; To The Top leaves none)
            if (slot == NavSlotKeys.Floating) wall.card_settings.container.full_height = CardOptions.FullHeightCameraStrip;
            wall.card_settings.navigation.edge = slot == NavSlotKeys.Edge ? entry.ElementKey : CardOptions.NavNone;
            // - a nav block comes in through Auto Nav Block (the builder's own step), every slot empty; one that needs authored content (the
            //   bento) cannot: Poi() authors it into the card
            wall.card_settings.navigation.auto_nav_block = entry.IsBlock && !NeedsAuthoring(entry) ? entry.ElementKey : CardOptions.NavNone;
            wall.card_settings.navigation.min_blocks = ShowNavigationFrom;
            if (entry.Layout != null) wall.card_settings.navigation.layout = entry.Layout;
            if (entry.Case.Language != "en") wall.card_settings.languages = new List<string> { "en", entry.Case.Language };
            return wall;
        }

        // The entry's point: a text header, then the case's blocks -- each a copy of its gallery block under a key of its own
        public static POIData Poi(Entry entry)
        {
            var poi = new POIData
            {
                id = entry.Name, name = "Castle", category = CardGalleryDefinitions.CategoryKey, hierarchy_level_key = CardGalleryDefinitions.LevelKey,
                card = new POICardData(),
            };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = BuiltInBlocks.HeaderTextOnly };
            header.fields.Add(new BlockFieldValue { key = BlockStackBuilder.HeaderTitleField, text = new List<LocalizedEntry> { new() { lang = "en", value = "Castle" } } });
            poi.card.blocks.Add(header);
            foreach (var (galleryEntry, copies) in entry.Case.Blocks)
            {
                var source = CardGalleryDefinitions.All.First(e => e.Name == galleryEntry).Block;
                for (int i = 0; i < copies; i++)
                {
                    var copy = JsonUtility.FromJson<BlockInstanceData>(JsonUtility.ToJson(source));
                    copy.key = "block_" + (poi.card.blocks.Count + 1);
                    bool carries = BlockRegistry.Shared.TryGet(copy.kind, out var kind) && kind.Field(BlockKindDefinition.MinutesField) != null;
                    if (entry.ElementKey == BuiltInBlocks.NavContentsKind && i == 0 && carries)
                        copy.fields.Add(new BlockFieldValue { key = BlockKindDefinition.MinutesField, number = ContentsMinutes });
                    poi.card.blocks.Add(copy);
                }
            }
            // - a block the builder cannot insert by itself is authored: right after the header, where Auto Nav Block puts the others
            if (NeedsAuthoring(entry)) poi.card.blocks.Insert(1, Bento());
            return poi;
        }

        // Whether this entry's block is authored into its card instead of inserted by Auto Nav Block (BlockKindDefinition.NeedsAuthoredContent)
        public static bool NeedsAuthoring(Entry entry) =>
            entry.IsBlock && BlockRegistry.Shared.TryGet(entry.ElementKey, out var kind) && kind.NeedsAuthoredContent;

        // The bento block of the gallery: four tiles over BentoTargets -- the first with a picture and words of its own (English and
        // Portuguese), the second with words only, the third opening its block in the full view, the fourth plain (the block's own words)
        private static BlockInstanceData Bento()
        {
            BlockItemData Tile(string target, string en = null, string pt = null, string picture = null, string opens = null)
            {
                var item = new BlockItemData();
                item.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.NavBentoTargetField, value = target });
                if (en != null)
                    item.fields.Add(new BlockItemFieldValue
                    {
                        key = BuiltInBlocks.NavBentoWordsField,
                        text = new List<LocalizedEntry> { new() { lang = "en", value = en }, new() { lang = "pt", value = pt } },
                    });
                if (picture != null) item.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.NavBentoPictureField, asset = picture });
                if (opens != null) item.fields.Add(new BlockItemFieldValue { key = BuiltInBlocks.NavBentoOpensField, value = opens });
                return item;
            }
            var bento = new BlockInstanceData { key = BentoKey, kind = BuiltInBlocks.NavBentoKind, variant = BuiltInBlocks.NavBentoTiles, display = CardOptions.DisplayInline };
            bento.fields.Add(new BlockFieldValue
            {
                key = BuiltInBlocks.NavBentoTilesField,
                items = new List<BlockItemData>
                {
                    Tile(BentoTargets[0], "The castle", "O castelo", "wide.png"),
                    Tile(BentoTargets[1], "Look closer", "Ver de perto"),
                    Tile(BentoTargets[2], opens: BuiltInBlocks.NavBentoOpensFullView),
                    Tile(BentoTargets[3]),
                },
            });
            return bento;
        }
    }
}
