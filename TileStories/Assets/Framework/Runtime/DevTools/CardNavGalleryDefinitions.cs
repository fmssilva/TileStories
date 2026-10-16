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

            public Entry(string elementKey, Case fixtureCase)
            {
                ElementKey = elementKey;
                Case = fixtureCase;
                Name = "Nav_" + elementKey + "_" + fixtureCase.Key;
            }
        }

        // The elements the gallery shows: one row per element, every case drawn for each
        public static readonly IReadOnlyList<string> ElementKeys = new[]
        {
            BuiltInNavElements.TextTabs, BuiltInNavElements.IconTabs, BuiltInNavElements.IconTabsChips, BuiltInNavElements.KindChips,
            BuiltInNavElements.Segmented, BuiltInNavElements.ProgressMap, BuiltInNavElements.BottomBar,
            BuiltInNavElements.Dock, BuiltInNavElements.BarSheet, BuiltInNavElements.NextUp,
        };

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

        public static readonly IReadOnlyList<Entry> All = ElementKeys.SelectMany(key => Cases.Select(c => new Entry(key, c))).ToList();

        // The fabricated wall of an entry: the card gallery's own, with the entry's language offered and navigation from 2 targets. The
        // element is the wall's pick for ITS slot and the other slot holds nothing, so each render judges one element alone
        public static WallConfigData Wall(Entry entry)
        {
            var wall = CardGalleryDefinitions.Taxonomy();
            bool bottom = NavElementRegistry.Shared.TryGet(entry.ElementKey, out var element) && element.Slot == NavSlotKeys.Bottom;
            wall.card_settings.navigation.top = bottom ? CardOptions.NavNone : entry.ElementKey;
            wall.card_settings.navigation.bottom = bottom ? entry.ElementKey : CardOptions.NavNone;
            wall.card_settings.navigation.min_blocks = ShowNavigationFrom;
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
                    poi.card.blocks.Add(copy);
                }
            }
            return poi;
        }
    }
}
