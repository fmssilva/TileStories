using System;
using System.Collections.Generic;

namespace TileStories
{
    // The map of ONE open card (_3.2 section 4.1): what every navigation element shows and where each of its items jumps.
    // Built from the stack BlockStackBuilder made (the blocks the card really shows, in the order it shows them), so the
    // elements never compute structure themselves -- tabs, a bottom bar, a drawer and a contents list all read this one model,
    // the "one result set every surface shows" rule of the search domain applied to the card.
    //
    //   Section (L1) -- a family (group_by = family, the default) or a format (group_by = format), with its CardStrings label
    //   Kind    (L2) -- the kinds of that section that have a target here
    //   Target  (L3) -- the key of one block a jump can land on (NavOntologyRule.IsTarget)
    //
    // Sections come in the order the stack shows their first target: with Block Order by_family that IS the families' order,
    // and with any other order an element's sections still follow what the visitor scrolls through. Kinds keep that order
    // inside a section, targets the stack's. It is rebuilt, never patched: on every Show and when a waiting block is revealed.
    // No icon or colour token yet: they land with the family icons and --ts-family-* tokens (_3.2 step 6).
    public sealed class CardNavModel
    {
        // One kind of a section, with the blocks of it a jump can land on
        public sealed class Kind
        {
            public readonly string Key;
            public readonly IReadOnlyList<string> Targets;
            // Rule 4: the one block this kind holds here, so its item jumps straight to it (L3 skipped); null with several
            public readonly string DirectTarget;

            public Kind(string key, IReadOnlyList<string> targets)
            {
                Key = key;
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
            public readonly IReadOnlyList<Kind> Kinds;
            // Every target of the section, in the stack's order
            public readonly IReadOnlyList<string> Targets;
            // Rule 4: the one block a single-target section jumps straight to (L2 skipped); null with several
            public readonly string DirectTarget;
            public int Count => Targets.Count;

            public Section(string key, string labelKey, IReadOnlyList<Kind> kinds, IReadOnlyList<string> targets)
            {
                Key = key;
                LabelKey = labelKey;
                Kinds = kinds;
                Targets = targets;
                DirectTarget = NavOntologyRule.DirectTarget(targets);
            }
        }

        // CardOptions.GroupByFamily or GroupByFormat (NavOntologyRule.GroupBy: anything else reads as family)
        public readonly string GroupBy;
        // Rule 2: only sections with a target, never an empty one
        public readonly IReadOnlyList<Section> Sections;
        public readonly int TargetCount;
        // Rule 3: false = the card shows no navigation element at all (fewer targets than Show Navigation From)
        public readonly bool ShowsNavigation;
        // Rule 5: what a bar's slots show by themselves, and what its More slot lists (empty when every section fits)
        public readonly IReadOnlyList<Section> BarSections;
        public readonly IReadOnlyList<Section> MoreSections;

        private CardNavModel(string groupBy, List<Section> sections, int targetCount, CardNavigationSettings settings)
        {
            GroupBy = groupBy;
            Sections = sections;
            TargetCount = targetCount;
            ShowsNavigation = NavOntologyRule.ShowsNavigation(targetCount, settings);
            int inBar = NavOntologyRule.SectionsInBar(sections.Count, NavOntologyRule.MaxSlots(settings));
            BarSections = sections.GetRange(0, inBar);
            MoreSections = sections.GetRange(inBar, sections.Count - inBar);
        }

        // The model of the card these stack entries make. `isRevealed` says whether a block's ShowAfterViewed gate has opened
        // (the host passes CardLocalState.Seen for this POI); null = none has, so every such block is still waiting.
        public static CardNavModel Build(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockRegistry registry, CardSettings settings,
            Func<string, bool> isRevealed = null)
        {
            var nav = settings?.navigation;
            string groupBy = NavOntologyRule.GroupBy(nav);
            var order = new List<string>();
            var targetsOf = new Dictionary<string, List<string>>();
            var kindsOf = new Dictionary<string, List<string>>();
            var kindTargets = new Dictionary<(string Section, string Kind), List<string>>();
            int targetCount = 0;

            if (entries != null)
                foreach (var entry in entries)
                {
                    if (!NavOntologyRule.IsTarget(entry, registry, isRevealed)) continue;
                    string section = groupBy == CardOptions.GroupByFormat ? entry.Definition.Format : entry.Definition.Family;
                    string kind = entry.Definition.Key;
                    if (!targetsOf.TryGetValue(section, out var targets))
                    {
                        order.Add(section);
                        targetsOf[section] = targets = new List<string>();
                        kindsOf[section] = new List<string>();
                    }
                    if (!kindTargets.TryGetValue((section, kind), out var ofKind))
                    {
                        kindsOf[section].Add(kind);
                        kindTargets[(section, kind)] = ofKind = new List<string>();
                    }
                    targets.Add(entry.Instance.key);
                    ofKind.Add(entry.Instance.key);
                    targetCount++;
                }

            var sections = new List<Section>(order.Count);
            foreach (string key in order)
            {
                var kinds = new List<Kind>();
                foreach (string kind in kindsOf[key]) kinds.Add(new Kind(kind, kindTargets[(key, kind)]));
                sections.Add(new Section(key, LabelKeyOf(key, groupBy, registry), kinds, targetsOf[key]));
            }
            return new CardNavModel(groupBy, sections, targetCount, nav);
        }

        // The CardStrings key a section is labelled with: the registered family's own, or the format's
        private static string LabelKeyOf(string key, string groupBy, BlockRegistry registry)
        {
            if (groupBy == CardOptions.GroupByFamily) return registry.TryGetFamily(key, out var family) ? family.LabelKey : null;
            switch (key)
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
