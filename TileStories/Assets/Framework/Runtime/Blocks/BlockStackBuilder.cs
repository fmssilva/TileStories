using System.Collections.Generic;

namespace TileStories
{
    // What a POI's card WILL show (_3.1 section 3), as one pure rule so "why is block X missing?" is a unit test,
    // not a debug session. In authored order, each instance is kept or skipped with its reason:
    //   - an unknown kind (not registered)            -> skipped (UnknownKind)
    //   - a kind switched off in the Block Library    -> skipped (KindDisabled)
    //   - a kind with nothing to show for this POI    -> skipped (NotForThisPoint, BlockKindDefinition.ShowsFor)
    //   - a required field with nothing in it         -> skipped (MissingRequired)
    //   - a required Items field with no complete row -> skipped (NoCompleteRow, BlockFieldReader.ItemIsComplete)
    //   - a required Asset whose path MediaPathRule refuses (outside the media folder, wrong file type)
    //                                                 -> skipped (InvalidMedia)
    //   - a second header                             -> skipped (ExtraHeader)
    //   - a variant the kind does not have (or none)  -> kept, with the Block Library's default variant
    // The stack always starts with ONE header: the first authored header (moved to the top), else one made from
    // the POI's name and summary -- so a POI with no card still gets a card, with zero authoring. What follows is
    // ordered by the wall's Block Order (_3.2 section 2.3, Detail Card > Navigation), applied after the skip rules:
    // authored, end_families_last (the default: the At The End families, Sources among them, close the card) or
    // by_family (each family one contiguous run, by its effective order). Last, the wall's Auto Nav Block (_3.2 section 3) may add ONE
    // nav block right after the header (InsertAutoNavBlock) -- a step of its own, after the order, so it can never move another block.
    public static class BlockStackBuilder
    {
        public enum SkipReason { UnknownKind, KindDisabled, NotForThisPoint, MissingRequired, NoCompleteRow, InvalidMedia, ExtraHeader }

        public readonly struct Entry
        {
            public readonly BlockInstanceData Instance;
            public readonly BlockKindDefinition Definition;
            public readonly string Variant;
            // True for a block that is in no config: the header made from name + summary, a block Auto Nav Block inserted
            public readonly bool Synthesized;

            public Entry(BlockInstanceData instance, BlockKindDefinition definition, string variant, bool synthesized)
            {
                Instance = instance;
                Definition = definition;
                Variant = variant;
                Synthesized = synthesized;
            }
        }

        public readonly struct Skipped
        {
            public readonly BlockInstanceData Instance;
            public readonly SkipReason Reason;
            // MissingRequired / NoCompleteRow / InvalidMedia: the field's key
            public readonly string FieldKey;

            public Skipped(BlockInstanceData instance, SkipReason reason, string fieldKey = null)
            {
                Instance = instance;
                Reason = reason;
                FieldKey = fieldKey;
            }
        }

        public sealed class Result
        {
            public readonly List<Entry> Entries = new();
            public readonly List<Skipped> Skipped = new();
        }

        // The field keys of the header made from the POI's name and summary
        public const string HeaderTitleField = "title";
        public const string HeaderSubtitleField = "subtitle";

        // The key of a block Auto Nav Block inserted (it is in no config): "auto:" + its kind. An authored key is generated as block_N and
        // never holds a ':', so the two can never collide -- and it is the same on every open, so what keys off a block (a BlockRef, the
        // spy, CardLocalState) sees one stable block
        public const string AutoNavBlockKeyPrefix = "auto:";
        public static string AutoNavBlockKey(string kind) => AutoNavBlockKeyPrefix + kind;

        // `wallPois`: the wall's POIs, for kinds that point at another POI (compare_points); null = no other POI resolves
        public static Result Build(POIData poi, CardSettings settings, BlockRegistry registry, IReadOnlyList<POIData> wallPois = null)
        {
            var result = new Result();
            Entry? header = null;
            var body = new List<Entry>();

            var blocks = poi?.card?.blocks;
            if (blocks != null)
            {
                foreach (var instance in blocks)
                {
                    if (instance == null) continue;
                    if (!registry.TryGet(instance.kind, out var definition))
                    {
                        result.Skipped.Add(new Skipped(instance, SkipReason.UnknownKind));
                        continue;
                    }
                    if (!BlockLibraryRule.IsEnabled(settings, definition.Key))
                    {
                        result.Skipped.Add(new Skipped(instance, SkipReason.KindDisabled));
                        continue;
                    }
                    string variant = definition.HasVariant(instance.variant) ? instance.variant : BlockLibraryRule.DefaultVariant(settings, definition);
                    if (definition.ShowsFor != null && !definition.ShowsFor(poi, instance, variant, wallPois))
                    {
                        result.Skipped.Add(new Skipped(instance, SkipReason.NotForThisPoint));
                        continue;
                    }
                    var (emptyField, emptyReason) = FirstEmptyRequiredField(instance, definition);
                    if (emptyField != null)
                    {
                        result.Skipped.Add(new Skipped(instance, emptyReason, emptyField));
                        continue;
                    }

                    var entry = new Entry(instance, definition, variant, synthesized: false);
                    if (definition.Key != BuiltInBlocks.HeaderKind) body.Add(entry);
                    else if (header == null) header = entry;
                    else result.Skipped.Add(new Skipped(instance, SkipReason.ExtraHeader));
                }
            }

            if (header == null && poi != null && registry.TryGet(BuiltInBlocks.HeaderKind, out var headerKind))
                header = new Entry(HeaderFromNameAndSummary(poi, settings), headerKind, BlockLibraryRule.DefaultVariant(settings, headerKind), synthesized: true);

            // - the header is the card's identity: it stays first whatever Block Order says (_3.2 2.3)
            if (header.HasValue) result.Entries.Add(header.Value);
            result.Entries.AddRange(InBlockOrder(body, settings, registry));
            InsertAutoNavBlock(result.Entries, settings, registry);
            return result;
        }

        // Auto Nav Block (_3.2 section 3, step 11a): the wall's nav block, inserted right after the header -- in second place whatever Block
        // Order said -- when ALL of these hold, else the stack is left exactly as it was:
        //   - the wall names a registered kind of the navigation family (none / blank / unknown / another family's kind = nothing)
        //   - that kind is on in the Block Library
        //   - the card shows no nav block of its own (an authored one, in the stack, wins: never two)
        //   - the card has at least Show Navigation From targets, counted as a first open sees them (nothing revealed yet), so a reveal
        //     never adds or removes it (NavOntologyRule: the same targets and threshold every navigation element reads)
        private static void InsertAutoNavBlock(List<Entry> entries, CardSettings settings, BlockRegistry registry)
        {
            string kind = settings?.navigation?.auto_nav_block;
            if (string.IsNullOrEmpty(kind) || kind == CardOptions.NavNone || entries.Count == 0) return;
            if (registry == null || !registry.TryGet(kind, out var definition) || definition.Family != BuiltInFamilies.Navigation) return;
            if (!BlockLibraryRule.IsEnabled(settings, definition.Key)) return;
            int targets = 0;
            foreach (var entry in entries)
            {
                if (entry.Definition.Family == BuiltInFamilies.Navigation) return;
                if (NavOntologyRule.IsTarget(entry, registry, null)) targets++;
            }
            if (!NavOntologyRule.ShowsNavigation(targets, settings.navigation)) return;
            var instance = new BlockInstanceData { key = AutoNavBlockKey(definition.Key), kind = definition.Key };
            // - after the header, which the builder always puts first
            entries.Insert(1, new Entry(instance, definition, BlockLibraryRule.DefaultVariant(settings, definition), synthesized: true));
        }

        // The body in the wall's Block Order (_3.2 section 2.3), applied AFTER the skip rules so a skipped block
        // can never change the order of the ones that stayed:
        //   authored           -- exactly what Card Content says
        //   end_families_last  -- authored order, then the blocks of every At The End family (default)
        //   by_family          -- by the family's effective Order, authored order inside a family
        // Every pass is a STABLE partition / insertion sort, so blocks of one family keep their authored order.
        private static List<Entry> InBlockOrder(List<Entry> body, CardSettings settings, BlockRegistry registry)
        {
            string order = settings?.navigation?.block_order ?? CardOptions.BlockOrderEndFamiliesLast;
            if (order == CardOptions.BlockOrderAuthored) return body;

            if (order == CardOptions.BlockOrderByFamily)
            {
                var byFamily = new List<Entry>(body.Count);
                foreach (var entry in body)
                {
                    int at = byFamily.Count;
                    int mine = FamilyOrder(entry, settings, registry);
                    // - walk back only over families that sort AFTER mine: an equal order stops the walk, so
                    //   same-family blocks (and ties) keep the order Card Content gave them
                    while (at > 0 && FamilyOrder(byFamily[at - 1], settings, registry) > mine) at--;
                    byFamily.Insert(at, entry);
                }
                return byFamily;
            }

            // end_families_last: one stable partition on the family's At The End flag
            var last = new List<Entry>(body.Count);
            foreach (var entry in body)
                if (!FamilyIsAtEnd(entry, settings, registry)) last.Add(entry);
            foreach (var entry in body)
                if (FamilyIsAtEnd(entry, settings, registry)) last.Add(entry);
            return last;
        }

        // The family's effective order: the wall's Families row if it has one, else the registered default
        private static int FamilyOrder(Entry entry, CardSettings settings, BlockRegistry registry)
        {
            var row = FamilyRow(entry, settings);
            if (row != null) return row.order;
            return registry != null && registry.TryGetFamily(entry.Definition.Family, out var family) ? family.Order : 0;
        }

        // The family's effective At The End, the same way round
        private static bool FamilyIsAtEnd(Entry entry, CardSettings settings, BlockRegistry registry)
        {
            var row = FamilyRow(entry, settings);
            if (row != null) return row.at_end;
            return registry != null && registry.TryGetFamily(entry.Definition.Family, out var family) && family.AtEnd;
        }

        // This entry's family row in the wall's Families table, or null when the wall never changed that family
        private static CardFamilySetting FamilyRow(Entry entry, CardSettings settings) =>
            settings?.navigation?.families?.Find(f => f != null && f.family == entry.Definition.Family);

        // The header a POI without an authored one gets: its name as the title, its summary as the subtitle
        public static BlockInstanceData HeaderFromNameAndSummary(POIData poi, CardSettings settings)
        {
            string lang = settings?.languages != null && settings.languages.Count > 0 ? settings.languages[0] : "";
            return new BlockInstanceData
            {
                key = "",
                kind = BuiltInBlocks.HeaderKind,
                fields = new List<BlockFieldValue>
                {
                    new() { key = HeaderTitleField, text = new List<LocalizedEntry> { new() { lang = lang, value = poi.name ?? "" } } },
                    new() { key = HeaderSubtitleField, text = new List<LocalizedEntry> { new() { lang = lang, value = poi.summary ?? "" } } },
                },
            };
        }

        // The first required field that leaves the block with nothing to show, and why (empty / no complete row)
        private static (string Field, SkipReason Reason) FirstEmptyRequiredField(BlockInstanceData instance, BlockKindDefinition definition)
        {
            if (definition.Fields == null) return (null, default);
            foreach (var field in definition.Fields)
            {
                if (!field.Required) continue;
                var value = instance.fields?.Find(v => v != null && v.key == field.Key);
                if (!BlockFieldReader.HasContent(value, field.Type)) return (field.Key, SkipReason.MissingRequired);
                if (field.Type == BlockFieldType.Asset && !MediaPathRule.IsValid(value.asset, field.Media)) return (field.Key, SkipReason.InvalidMedia);
                if (field.Type == BlockFieldType.Items && !value.items.Exists(item => BlockFieldReader.ItemIsComplete(item, field.ItemFields)))
                    return (field.Key, SkipReason.NoCompleteRow);
            }
            return (null, default);
        }

        // The title a visitor reads at the top of a POI's card: its first authored header's title in `language` (the
        // fallback language's when missing), else its name. How another block names this POI (compare_points).
        public static string CardTitleOf(POIData poi, string language, string fallbackLanguage)
        {
            if (poi == null) return "";
            var blocks = poi.card?.blocks;
            if (blocks != null)
                foreach (var block in blocks)
                {
                    if (block == null || block.kind != BuiltInBlocks.HeaderKind) continue;
                    string title = new BlockFieldReader(block, language, fallbackLanguage).Text(HeaderTitleField);
                    if (title.Length > 0) return title;
                }
            return poi.name ?? "";
        }
    }
}
