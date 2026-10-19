using System;
using System.Collections.Generic;

namespace TileStories
{
    // The map of ONE open card (_3.2 section 4.1): what every navigation element shows and where each of its items jumps.
    // Built from the stack BlockStackBuilder made (the blocks the card really shows, in the order it shows them), so the
    // elements never compute structure themselves -- tabs, chips, segments, a bottom bar, a drawer and a contents list all read
    // this one model, the "one result set every surface shows" rule of the search domain applied to the card.
    //
    //   Section (L1) -- a family (group_by = family, the default) or a format (group_by = format), with its CardStrings label and,
    //                   for a family, its icon and colour token
    //   Kind    (L2) -- the kinds of that section that have a target here, with their words and their family's colour
    //   Target  (L3) -- the key of one block a jump can land on (NavOntologyRule.IsTarget)
    //   Group        -- the wall's Groups (rule 2.5.6, NavGroupRule): FAMILIES merged, whatever Group By says (the segmented element)
    //
    // Sections and groups come in the order the stack shows their first target: with Block Order by_family that IS the families'
    // order, and with any other order an element's items still follow what the visitor scrolls through. Kinds keep that order
    // inside a section, targets the stack's. It is rebuilt, never patched: on every Show and when a waiting block is revealed.
    public sealed class CardNavModel
    {
        // One kind of a section, with the blocks of it a jump can land on
        public sealed class Kind
        {
            public readonly string Key;
            // The visitor's words for the kind (BlockKindDefinition.LabelKey) and its family's colour token (a chip's dot)
            public readonly string LabelKey;
            public readonly string ColorToken;
            public readonly IReadOnlyList<string> Targets;
            // Rule 4: the one block this kind holds here, so its item jumps straight to it (L3 skipped); null with several
            public readonly string DirectTarget;
            public int Count => Targets.Count;

            public Kind(string key, string labelKey, string colorToken, IReadOnlyList<string> targets)
            {
                Key = key;
                LabelKey = labelKey;
                ColorToken = colorToken;
                Targets = targets;
                DirectTarget = NavOntologyRule.DirectTarget(targets);
            }
        }

        // One section of the navigation: a family or a format
        public sealed class Section
        {
            // The family key or the format (CardOptions.Formats)
            public readonly string Key;
            // The visitor's words for it: a CardStrings key (the family's LabelKey, or the format's)
            public readonly string LabelKey;
            // A family's icon (CardIcons) and colour token; null for a format, which has neither (it always shows its words)
            public readonly string IconKey;
            public readonly string ColorToken;
            public readonly IReadOnlyList<Kind> Kinds;
            // Every target of the section, in the stack's order
            public readonly IReadOnlyList<string> Targets;
            // Rule 4: the one block a single-target section jumps straight to (L2 skipped); null with several
            public readonly string DirectTarget;
            public int Count => Targets.Count;

            public Section(string key, string labelKey, string iconKey, string colorToken, IReadOnlyList<Kind> kinds, IReadOnlyList<string> targets)
            {
                Key = key;
                LabelKey = labelKey;
                IconKey = iconKey;
                ColorToken = colorToken;
                Kinds = kinds;
                Targets = targets;
                DirectTarget = NavOntologyRule.DirectTarget(targets);
            }
        }

        // One of the wall's Groups that has a target on this card (rule 6): its words and the targets of every family in it
        public sealed class Group
        {
            public readonly NavGroupRule.Group Definition;
            public readonly IReadOnlyList<string> Targets;
            public string Key => Definition.Key;
            public int Count => Targets.Count;

            public Group(NavGroupRule.Group definition, IReadOnlyList<string> targets)
            {
                Definition = definition;
                Targets = targets;
            }
        }

        // CardOptions.GroupByFamily or GroupByFormat (NavOntologyRule.GroupBy: anything else reads as family)
        public readonly string GroupBy;
        // Rule 2: only sections with a target, never an empty one
        public readonly IReadOnlyList<Section> Sections;
        // Rule 6: only the groups with a target; a family in no group of this wall is in none of them
        public readonly IReadOnlyList<Group> Groups;
        // Every target, in the stack's order (what a spy that follows single blocks reads)
        public readonly IReadOnlyList<string> Targets;
        public int TargetCount => Targets.Count;
        // Rule 3: false = the card shows no navigation element at all (fewer targets than Show Navigation From)
        public readonly bool ShowsNavigation;
        // Rule 5: what a bar's slots show by themselves, and what its More slot lists (empty when every section fits)
        public readonly IReadOnlyList<Section> BarSections;
        public readonly IReadOnlyList<Section> MoreSections;

        // Where each target sits: its section, its kind inside it, its group (-1 = none)
        private readonly Dictionary<string, (int Section, int Kind, int Group)> _place;
        // The same card on the other axis (By): built on first ask, from the same entries, by the same Build
        private Func<string, CardNavModel> _buildOn;
        private CardNavModel _pivot;

        private CardNavModel(string groupBy, List<Section> sections, List<Group> groups, List<string> targets,
            Dictionary<string, (int, int, int)> place, CardNavigationSettings settings)
        {
            GroupBy = groupBy;
            Sections = sections;
            Groups = groups;
            Targets = targets;
            _place = place;
            ShowsNavigation = NavOntologyRule.ShowsNavigation(targets.Count, settings);
            int inBar = NavOntologyRule.SectionsInBar(sections.Count, NavOntologyRule.MaxSlots(settings));
            BarSections = sections.GetRange(0, inBar);
            MoreSections = sections.GetRange(inBar, sections.Count - inBar);
        }

        // The index in Sections of the section a target belongs to, -1 for a block that is no target
        public int SectionOf(string blockKey) => blockKey != null && _place.TryGetValue(blockKey, out var at) ? at.Section : -1;

        // The kind a target belongs to (inside its section), null for a block that is no target
        public Kind KindOf(string blockKey) =>
            blockKey != null && _place.TryGetValue(blockKey, out var at) ? Sections[at.Section].Kinds[at.Kind] : null;

        // The index in Groups of the group a target belongs to, -1 for none (no target, or its family is in no group of this wall)
        public int GroupOf(string blockKey) => blockKey != null && _place.TryGetValue(blockKey, out var at) ? at.Group : -1;

        // The same card's map with its sections on `groupBy` (CardOptions.GroupByFamily / GroupByFormat): this model on its own axis, else
        // the pivot -- the SAME targets regrouped by Build itself, so an element that switches axis (grid_overlay's Purpose / Format, step 8)
        // never regroups blocks on its own. Everything but the sections (targets, groups, ShowsNavigation, bar / More) is the same card's
        public CardNavModel By(string groupBy)
        {
            string axis = groupBy == CardOptions.GroupByFormat ? CardOptions.GroupByFormat : CardOptions.GroupByFamily;
            if (axis == GroupBy) return this;
            if (_pivot == null)
            {
                _pivot = _buildOn(axis);
                _pivot._pivot = this;
            }
            return _pivot;
        }

        // The model of the card these stack entries make. `isRevealed` says whether a block's ShowAfterViewed gate has opened
        // (the host passes CardLocalState.Seen for this POI); null = none has, so every such block is still waiting.
        public static CardNavModel Build(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockRegistry registry, CardSettings settings,
            Func<string, bool> isRevealed = null) =>
            Build(entries, registry, settings, isRevealed, NavOntologyRule.GroupBy(settings?.navigation));

        // The model with its sections on `groupBy`, whatever the wall's Group By says (By's pivot)
        private static CardNavModel Build(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockRegistry registry, CardSettings settings,
            Func<string, bool> isRevealed, string groupBy)
        {
            var nav = settings?.navigation;
            var order = new List<string>();
            var targetsOf = new Dictionary<string, List<string>>();
            var kindsOf = new Dictionary<string, List<BlockKindDefinition>>();
            var kindTargets = new Dictionary<(string Section, string Kind), List<string>>();
            var groupOrder = new List<string>();
            var groupTargets = new Dictionary<string, List<string>>();
            var targets = new List<string>();
            var groupOfTarget = new Dictionary<string, string>();

            if (entries != null)
                foreach (var entry in entries)
                {
                    if (!NavOntologyRule.IsTarget(entry, registry, isRevealed)) continue;
                    var kind = entry.Definition;
                    string section = groupBy == CardOptions.GroupByFormat ? kind.Format : kind.Family;
                    if (!targetsOf.TryGetValue(section, out var ofSection))
                    {
                        order.Add(section);
                        targetsOf[section] = ofSection = new List<string>();
                        kindsOf[section] = new List<BlockKindDefinition>();
                    }
                    if (!kindTargets.TryGetValue((section, kind.Key), out var ofKind))
                    {
                        kindsOf[section].Add(kind);
                        kindTargets[(section, kind.Key)] = ofKind = new List<string>();
                    }
                    string block = entry.Instance.key;
                    ofSection.Add(block);
                    ofKind.Add(block);
                    targets.Add(block);

                    // - rule 6: the block's FAMILY merges into its group, whatever the sections are
                    string group = NavGroupRule.GroupOf(kind.Family, nav);
                    if (group == null) continue;
                    if (!groupTargets.TryGetValue(group, out var ofGroup))
                    {
                        groupOrder.Add(group);
                        groupTargets[group] = ofGroup = new List<string>();
                    }
                    ofGroup.Add(block);
                    groupOfTarget[block] = group;
                }

            var place = new Dictionary<string, (int, int, int)>();
            var sections = new List<Section>(order.Count);
            foreach (string key in order)
            {
                registry.TryGetFamily(key, out var family);
                bool byFamily = groupBy == CardOptions.GroupByFamily;
                var kinds = new List<Kind>();
                foreach (var kind in kindsOf[key])
                {
                    registry.TryGetFamily(kind.Family, out var kindFamily);
                    var kindBlocks = kindTargets[(key, kind.Key)];
                    foreach (string block in kindBlocks) place[block] = (sections.Count, kinds.Count, -1);
                    kinds.Add(new Kind(kind.Key, kind.LabelKey, kindFamily?.ColorToken, kindBlocks));
                }
                sections.Add(new Section(key, LabelKeyOf(key, groupBy, registry), byFamily ? family?.IconKey : null,
                    byFamily ? family?.ColorToken : null, kinds, targetsOf[key]));
            }

            var definitions = new Dictionary<string, NavGroupRule.Group>();
            foreach (var definition in NavGroupRule.Groups(nav)) definitions[definition.Key] = definition;
            var groups = new List<Group>(groupOrder.Count);
            foreach (string key in groupOrder)
            {
                foreach (string block in groupTargets[key])
                {
                    var (s, k, _) = place[block];
                    place[block] = (s, k, groups.Count);
                }
                groups.Add(new Group(definitions[key], groupTargets[key]));
            }
            return new CardNavModel(groupBy, sections, groups, targets, place, nav)
            {
                _buildOn = axis => Build(entries, registry, settings, isRevealed, axis),
            };
        }

        // The CardStrings key a section is labelled with: the registered family's own, or the format's
        private static string LabelKeyOf(string key, string groupBy, BlockRegistry registry)
        {
            if (groupBy == CardOptions.GroupByFamily) return registry.TryGetFamily(key, out var family) ? family.LabelKey : null;
            return FormatLabelKey(key);
        }

        // The CardStrings key a format (CardOptions.Formats) is named by, null for an unknown one: the ONE mapping, read by the format
        // sections here and by the compact row (CompactRowRule)
        public static string FormatLabelKey(string format)
        {
            switch (format)
            {
                case CardOptions.FormatRead: return CardStrings.Keys.FormatRead;
                case CardOptions.FormatLook: return CardStrings.Keys.FormatLook;
                case CardOptions.FormatWatch: return CardStrings.Keys.FormatWatch;
                case CardOptions.FormatListen: return CardStrings.Keys.FormatListen;
                case CardOptions.FormatModel: return CardStrings.Keys.FormatModel;
                case CardOptions.FormatInteract: return CardStrings.Keys.FormatInteract;
                default: return null;
            }
        }
    }
}
