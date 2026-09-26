using System.Collections.Generic;

namespace TileStories
{
    // What a POI's card WILL show (_3.1 section 3), as one pure rule so "why is block X missing?" is a unit test,
    // not a debug session. In authored order, each instance is kept or skipped with its reason:
    //   - an unknown kind (not registered)            -> skipped (UnknownKind)
    //   - a kind switched off in the Block Library    -> skipped (KindDisabled)
    //   - a required field with nothing in it         -> skipped (MissingRequired)
    //   - a second header                             -> skipped (ExtraHeader)
    //   - a variant the kind does not have (or none)  -> kept, with the Block Library's default variant
    // The stack always starts with ONE header: the first authored header (moved to the top), else one made from
    // the POI's name and summary -- so a POI with no card still gets a card, with zero authoring.
    public static class BlockStackBuilder
    {
        public enum SkipReason { UnknownKind, KindDisabled, MissingRequired, ExtraHeader }

        public readonly struct Entry
        {
            public readonly BlockInstanceData Instance;
            public readonly BlockKindDefinition Definition;
            public readonly string Variant;
            // True for the header made from name + summary (it is in no config)
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
            // MissingRequired: the empty field's key
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

        public static Result Build(POIData poi, CardSettings settings, BlockRegistry registry)
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
                    string empty = FirstEmptyRequiredField(instance, definition);
                    if (empty != null)
                    {
                        result.Skipped.Add(new Skipped(instance, SkipReason.MissingRequired, empty));
                        continue;
                    }

                    string variant = definition.HasVariant(instance.variant) ? instance.variant : BlockLibraryRule.DefaultVariant(settings, definition);
                    var entry = new Entry(instance, definition, variant, synthesized: false);
                    if (definition.Key != BuiltInBlocks.HeaderKind) body.Add(entry);
                    else if (header == null) header = entry;
                    else result.Skipped.Add(new Skipped(instance, SkipReason.ExtraHeader));
                }
            }

            if (header == null && poi != null && registry.TryGet(BuiltInBlocks.HeaderKind, out var headerKind))
                header = new Entry(HeaderFromNameAndSummary(poi, settings), headerKind, BlockLibraryRule.DefaultVariant(settings, headerKind), synthesized: true);

            if (header.HasValue) result.Entries.Add(header.Value);
            result.Entries.AddRange(body);
            return result;
        }

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

        private static string FirstEmptyRequiredField(BlockInstanceData instance, BlockKindDefinition definition)
        {
            if (definition.Fields == null) return null;
            foreach (var field in definition.Fields)
            {
                if (!field.Required) continue;
                var value = instance.fields?.Find(v => v != null && v.key == field.Key);
                if (!BlockFieldReader.HasContent(value, field.Type)) return field.Key;
            }
            return null;
        }
    }
}
