using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Phase A data list of the POI Detail Card gallery (_3.1 section 7, 40-testing.md 4.4), fabricated content -- no AR,
    // no config.json. The ONE list both CardGalleryHarness (to look at) and CardGalleryTests (to assert) read, so the two
    // cannot drift apart:
    //   - header entries: every header variant x content (short, long, no subtitle) x stop
    //   - block entries: every other kind x variant x content (short, long, and the states that kind has: an optional
    //     field left empty, unknown...), each shown at the full stop under a short header
    public static partial class CardGalleryDefinitions
    {
        public readonly struct Entry
        {
            public readonly string Name;
            public readonly string Kind;
            public readonly string Variant;
            public readonly string Content;
            public readonly SheetStopRule.Stop Stop;
            // Header entries: the header's own texts
            public readonly string Title;
            public readonly string Subtitle;
            // Block entries: the block shown under the header, and what the POI must hold for it (a status...)
            public readonly BlockInstanceData Block;
            public readonly System.Action<POIData> Setup;
            // Block entries: a change to the fabricated wall this entry needs (e.g. no Outline Types rows)
            public readonly System.Action<WallConfigData> WallSetup;
            // Where the visitor stands in the wall's frame (wall_locator's "You are here"); null = no viewer known
            public readonly Vector3? Viewer;

            public bool IsHeader => Kind == BuiltInBlocks.HeaderKind;

            public Entry(string variant, string content, string title, string subtitle, SheetStopRule.Stop stop)
            {
                Name = "header_" + variant + "_" + content + "_" + stop.ToString().ToLowerInvariant();
                Kind = BuiltInBlocks.HeaderKind;
                Variant = variant;
                Content = content;
                Stop = stop;
                Title = title;
                Subtitle = subtitle;
                Block = null;
                Setup = null;
                WallSetup = null;
                Viewer = null;
            }

            public Entry(string kind, string variant, string content, BlockInstanceData block, System.Action<POIData> setup = null,
                System.Action<WallConfigData> wallSetup = null, Vector3? viewer = null)
            {
                Name = kind + "_" + variant + "_" + content;
                Kind = kind;
                Variant = variant;
                Content = content;
                Stop = SheetStopRule.Stop.Full;
                Title = "Gate";
                Subtitle = "";
                Block = block;
                Setup = setup;
                WallSetup = wallSetup;
                Viewer = viewer;
            }
        }

        public const string CategoryKey = "category_1";
        public const string LevelKey = "level_1";

        public static readonly IReadOnlyList<Entry> All = Build();

        private static List<Entry> Build()
        {
            var list = new List<Entry>();
            AddHeaders(list);
            AddStatus(list);
            AddRichText(list);
            AddQuickFacts(list);
            AddFunFacts(list);
            AddPullQuotes(list);
            AddProcessSteps(list);
            AddSwatches(list);
            AddTimelines(list);
            AddPeople(list);
            AddStoryChapters(list);
            AddComparePoints(list);
            AddPracticalInfo(list);
            AddSources(list);
            AddActions(list);
            AddGalleries(list);
            AddBeforeAfter(list);
            AddZoomImages(list);
            AddHotspots(list);
            AddVideos(list);
            AddModel3D(list);
            AddWallLocators(list);
            AddTodayMaps(list);
            AddRelated(list);
            AddKnowledgeChecks(list);
            AddPolls(list);
            AddCollects(list);
            AddFeedback(list);
            AddDialogues(list);
            AddShowOnWall(list);
            AddAudioGuides(list);
            AddHeadings(list);
            return list;
        }

        // The heading every block may have (the stack draws it, _3.1 step 6C): the first entry of each kind x variant
        // carries one, so every kind is seen with a heading above it and without one. A kind with a default heading
        // (compare, sources) gets it on its "long" entry instead: there the authored heading must win over the default.
        public const string HeadingPrefix = "About the ";

        private static void AddHeadings(List<Entry> list)
        {
            var seen = new HashSet<string>();
            foreach (var entry in list)
            {
                if (entry.IsHeader) continue;
                bool hasDefault = BlockRegistry.Shared.TryGet(entry.Kind, out var kind) && kind.DefaultHeadingKey != null;
                if (hasDefault && entry.Content != "long") continue;
                if (!seen.Add(entry.Kind + "/" + entry.Variant)) continue;
                if (entry.Block.fields.Exists(f => f.key == BlockKindDefinition.HeadingField)) continue;
                entry.Block.fields.Add(Text(BlockKindDefinition.HeadingField, HeadingPrefix + entry.Kind.Replace('_', ' ')));
            }
        }

        // Whether this entry's block has an authored heading
        public static bool HasHeading(Entry entry) =>
            !entry.IsHeader && entry.Block.fields.Exists(f => f.key == BlockKindDefinition.HeadingField);

        private static void AddHeaders(List<Entry> list)
        {
            var contents = new[]
            {
                ("short", "Gate", "1640"),
                ("long", "The Royal Palace of the Kings of Portugal and of the Algarves, before the earthquake",
                    "Seen from the river on the panel, with the Customs House, the chapel and the long arcade that the fire destroyed"),
                ("nosubtitle", "Old Cathedral", ""),
            };
            foreach (var variant in BuiltInBlocks.Header.Variants)
                foreach (var (content, title, subtitle) in contents)
                    foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
                        list.Add(new Entry(variant, content, title, subtitle, stop));
            // - a picture look with no picture written (the video loop: no clip): the text-only look, no hero
            foreach (string variant in BuiltInBlocks.HeaderImageVariants)
                list.Add(new Entry(variant, NoPicture, "Gate", "1640", SheetStopRule.Stop.Full));
            list.Add(new Entry(BuiltInBlocks.HeaderVideoLoop, NoPicture, "Gate", "1640", SheetStopRule.Stop.Full));
            list.Add(new Entry(BuiltInBlocks.HeaderModelTurntable, NoPicture, "Gate", "1640", SheetStopRule.Stop.Full));
        }

        // The header video_loop entries' clip (a generated gallery video, CardGalleryDefinitions.Videos)
        public const string LoopClip = "long.mp4";

        // ---------------- pictures (Tier 2) ----------------

        public const string NoPicture = "nopicture";

        // The gallery's pictures by name: made in memory by CardGalleryMedia, each its own colour (a render pixel tells them
        // apart) and size (landscape, portrait, square). Any other name is a missing file.
        public static readonly IReadOnlyDictionary<string, (Vector2Int Size, Color Colour)> Pictures = new Dictionary<string, (Vector2Int, Color)>
        {
            ["wide.png"] = (new Vector2Int(768, 432), new Color(0.12f, 0.25f, 0.56f)),
            ["then.png"] = (new Vector2Int(512, 384), new Color(0.72f, 0.52f, 0.18f)),
            ["now.png"] = (new Vector2Int(512, 384), new Color(0.20f, 0.45f, 0.70f)),
            ["one.png"] = (new Vector2Int(512, 384), new Color(0.70f, 0.20f, 0.18f)),
            ["two.png"] = (new Vector2Int(512, 384), new Color(0.20f, 0.55f, 0.30f)),
            ["three.png"] = (new Vector2Int(384, 512), new Color(0.45f, 0.25f, 0.45f)),
            ["four.png"] = (new Vector2Int(512, 384), new Color(0.85f, 0.66f, 0.23f)),
            ["before.png"] = (new Vector2Int(512, 384), new Color(0.60f, 0.35f, 0.20f)),
            ["after.png"] = (new Vector2Int(512, 384), new Color(0.25f, 0.35f, 0.60f)),
            ["detail.png"] = (new Vector2Int(1024, 1024), new Color(0.15f, 0.30f, 0.50f)),
        };

        // Where a spotlight entry's focus is and how much it enlarges
        public static readonly Vector2 SpotlightFocus = new(0.25f, 0.4f);
        public const float SpotlightZoom = 2.5f;

        private static void AddPictureFields(BlockInstanceData header, string variant)
        {
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = variant == BuiltInBlocks.HeaderSplitThenNow ? "then.png" : "wide.png" });
            if (variant == BuiltInBlocks.HeaderSplitThenNow)
                header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderSecondImageField, asset = "now.png" });
            if (variant != BuiltInBlocks.HeaderSpotlightCrop) return;
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderFocusXField, number = SpotlightFocus.x });
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderFocusYField, number = SpotlightFocus.y });
            header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderZoomField, number = SpotlightZoom });
        }

        // ---------------- the fabricated wall ----------------

        // One category and one level (the header chip), a glossary with the terms the texts use
        public static WallConfigData Taxonomy()
        {
            var config = new WallConfigData
            {
                category_styles = new List<CategoryStyleEntry> { new() { key = CategoryKey, label = "Civic Buildings" } },
                hierarchy_levels = new List<HierarchyLevelEntry> { new() { key = LevelKey, level_name = "Landmark" } },
            };
            config.card_settings.languages = new List<string> { "en" };
            // - the markers' Outline Types, per type with the stock colours (an empty colour = the framework ramp)
            config.marker_outline_mode = "per_type";
            config.outline_levels = new List<OutlineLevelEntry>
            {
                new() { key = OutlineIntact, label = "Intact", pct = 0f, line_style = "solid" },
                new() { key = OutlinePartial, label = "Partial Damage", pct = 20f, line_style = "dash_long" },
                new() { key = OutlineHeavy, label = "Heavy Damage", pct = 60f, line_style = "dash_short" },
                new() { key = OutlineDestroyed, label = "Destroyed", pct = 100f, line_style = "dotted" },
                new() { key = OutlineUnknown, label = "Unknown", pct = 100f, line_style = "dotted", color_hex = "#71717A" },
            };
            config.card_settings.glossary = new List<GlossaryEntry>
            {
                Glossary("keep", "The strongest tower of a castle, its last refuge."),
                Glossary("curtain wall", "The wall that joins the towers of a castle."),
                Glossary("biscuit", "Clay fired once, hard but not yet glazed."),
            };
            return config;
        }

        // The POI of one entry: a header entry's own header, or a short header + the entry's block
        public static POIData Poi(Entry e)
        {
            var poi = new POIData { id = e.Name, name = e.Title, category = CategoryKey, hierarchy_level_key = LevelKey };
            var header = new BlockInstanceData { key = "block_1", kind = BuiltInBlocks.HeaderKind, variant = e.IsHeader ? e.Variant : BuiltInBlocks.HeaderTextOnly };
            header.fields.Add(Text(BlockStackBuilder.HeaderTitleField, e.Title));
            if (e.Subtitle.Length > 0) header.fields.Add(Text(BlockStackBuilder.HeaderSubtitleField, e.Subtitle));
            if (e.IsHeader && System.Array.IndexOf(BuiltInBlocks.HeaderImageVariants, e.Variant) >= 0 && e.Content != NoPicture)
                AddPictureFields(header, e.Variant);
            // - the video loop: the gallery's short clip, the wide picture as its poster
            if (e.IsHeader && e.Variant == BuiltInBlocks.HeaderVideoLoop && e.Content != NoPicture)
            {
                header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderLoopClipField, asset = LoopClip });
                header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "wide.png" });
            }
            // - model_turntable (10A.3.2): the Framework's own default model, the wide picture as its Fallback
            if (e.IsHeader && e.Variant == BuiltInBlocks.HeaderModelTurntable && e.Content != NoPicture)
            {
                header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderModelField, asset = MediaPathRule.PathForDefaultKey("azulejo_arch") });
                header.fields.Add(new BlockFieldValue { key = BuiltInBlocks.HeaderImageField, asset = "wide.png" });
            }
            poi.card.blocks.Add(header);
            if (e.Block != null) poi.card.blocks.Add(e.Block);
            e.Setup?.Invoke(poi);
            return poi;
        }

        private static BlockFieldValue Text(string key, string value) =>
            new() { key = key, text = new List<LocalizedEntry> { new() { lang = "en", value = value } } };

        private static BlockItemFieldValue ItemText(string key, string value) =>
            new() { key = key, text = new List<LocalizedEntry> { new() { lang = "en", value = value } } };

        private static BlockItemData Item(params BlockItemFieldValue[] fields) => new() { fields = new List<BlockItemFieldValue>(fields) };

        private static GlossaryEntry Glossary(string term, string definition) =>
            new() { term = term, definition = new List<LocalizedEntry> { new() { lang = "en", value = definition } } };
    }
}
