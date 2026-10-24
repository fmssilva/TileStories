using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's chapters in the scroll (_3.3 P4.4c1, R11, R10.7): the stack's scroll blocks, cut by SectionSheetRule into runs of one
    // family, each run WRAPPED in one glass sheet -- its opener (SectionOpenerView), then its blocks' slots -- the sheets one after the other
    // in the scroll, each laid over the one before (a negative top margin and a shadow cast up). The sheet is tinted with its family's glass
    // (the theme's "<colour token>-glass", painted by NavTint so a mode switch repaints it); its blocks stay opaque frames (BlockFrame) and
    // its words sit on the opener's filled pill: no text on the glass (D8). A bare run (the navigation family) is a plain wrapper.
    //   - Build wraps once per bind (a view is never moved between parents while bound); Refresh re-reads which slots are shown (a layout
    //     sets blocks aside, a block is revealed) and shows / hides each sheet and its opener, with its count and minutes, by the rule.
    //   - A slot's layout is now relative to its SHEET: anything that needs a block's place in the scroll asks BlockStackView.TopInScroll.
    // The look is PoiCard.uss (.card-section*). Plain C#, owned by BlockStackView.
    public sealed class SectionSheets
    {
        public const string SheetClass = "card-section";
        public const string BareClass = "card-section--bare";
        public const string OverClass = "card-section--over";
        public const string ContinuesClass = "card-section--continues";
        public const string ShadowClass = "card-section__shadow";
        public const string EdgeClass = "card-section__edge";

        // One sheet: its element, its shadow part, its opener, its colours, and the blocks it holds now
        public sealed class Sheet
        {
            public VisualElement Root;
            public VisualElement Shadow;
            public SectionOpenerView Opener;
            public NavTint Tint;
            public SectionSheetRule.Run Run;
            public FamilyDefinition Family;
            public readonly List<VisualElement> Slots = new();
            public readonly List<int> Minutes = new();
            // How it shows now (SectionSheetRule.Looks)
            public SectionSheetRule.Look Look;
        }

        private readonly VisualElement _content;
        private readonly BlockRegistry _registry;
        private readonly Stack<Sheet> _pool = new();
        private readonly List<Sheet> _sheets = new();
        private System.Func<string, string> _words;
        private readonly List<bool> _shown = new();
        private readonly List<SectionSheetRule.Run> _runs = new();

        public SectionSheets(VisualElement content, BlockRegistry registry)
        {
            _content = content;
            _registry = registry;
        }

        // The sheets of the bound card, in scroll order
        public IReadOnlyList<Sheet> All => _sheets;

        // Wrap the scroll's blocks -- their slots and entries, in stack order -- into sheets appended to the scroll, then show them as the
        // slots are shown now
        public void Build(IReadOnlyList<VisualElement> slots, IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context)
        {
            Release();
            _words = context.Strings != null ? context.Strings.Get : null;
            var families = new List<string>(entries.Count);
            foreach (var entry in entries) families.Add(entry.Definition.Family);
            _runs.Clear();
            _runs.AddRange(SectionSheetRule.Runs(families, family => !_registry.TryGetFamily(family, out var f) || f.OpensSection));
            foreach (var run in _runs)
            {
                var sheet = Take();
                sheet.Run = run;
                _registry.TryGetFamily(run.Family, out sheet.Family);
                sheet.Root.EnableInClassList(BareClass, run.Bare);
                for (int i = run.Start; i < run.Start + run.Count; i++)
                {
                    sheet.Slots.Add(slots[i]);
                    sheet.Minutes.Add(ReadingTimeRule.Minutes(entries[i].Definition, entries[i].Instance, 0f));
                    sheet.Root.Add(slots[i]);
                }
                sheet.Tint.Clear();
                if (!run.Bare && sheet.Family != null) sheet.Tint.Fill(sheet.Root, SectionSheetRule.GlassOf(sheet.Family.ColorToken));
                _content.Add(sheet.Root);
                _sheets.Add(sheet);
            }
            Refresh();
        }

        // Show each sheet, its opener (with what it holds now) and whether it is laid over the one before, by the slots shown now
        public void Refresh()
        {
            _shown.Clear();
            foreach (var sheet in _sheets) _shown.Add(CountShown(sheet) > 0);
            var looks = SectionSheetRule.Looks(_runs, _shown);
            for (int i = 0; i < _sheets.Count; i++)
            {
                var sheet = _sheets[i];
                sheet.Look = looks[i];
                sheet.Root.style.display = looks[i].Shown ? DisplayStyle.Flex : DisplayStyle.None;
                sheet.Root.EnableInClassList(OverClass, looks[i].Overlaps);
                sheet.Root.EnableInClassList(ContinuesClass, looks[i].Continues);
                sheet.Opener.Root.style.display = looks[i].Opener ? DisplayStyle.Flex : DisplayStyle.None;
                if (!looks[i].Opener || sheet.Family == null) continue;
                int minutes = 0;
                for (int s = 0; s < sheet.Slots.Count; s++)
                    if (IsShown(sheet.Slots[s])) minutes += sheet.Minutes[s];
                string name = sheet.Family.LabelKey != null && _words != null ? _words(sheet.Family.LabelKey) : sheet.Family.Key;
                sheet.Opener.Show(sheet.Family.IconKey, sheet.Family.ColorToken, name, SectionSheetRule.Meta(CountShown(sheet), minutes, _words));
            }
        }

        // The sheet holding this slot (null when it is in none: the header, the footer, nothing bound)
        public Sheet SheetOf(VisualElement slot)
        {
            foreach (var sheet in _sheets)
                if (slot != null && slot.parent == sheet.Root) return sheet;
            return null;
        }

        // Whether this slot is the first block its sheet shows (a jump to it lands on the sheet's top: its opener in view)
        public bool IsFirstShown(VisualElement slot)
        {
            var sheet = SheetOf(slot);
            if (sheet == null) return false;
            foreach (var s in sheet.Slots)
                if (IsShown(s)) return s == slot;
            return false;
        }

        // Take every sheet off the scroll and back to the pool (the slots are the stack's: it takes them out first)
        public void Release()
        {
            foreach (var sheet in _sheets)
            {
                foreach (var slot in sheet.Slots)
                    if (slot.parent == sheet.Root) slot.RemoveFromHierarchy();
                sheet.Slots.Clear();
                sheet.Minutes.Clear();
                sheet.Root.RemoveFromHierarchy();
                _pool.Push(sheet);
            }
            _sheets.Clear();
            _runs.Clear();
        }

        private static bool IsShown(VisualElement slot) => slot.style.display.value != DisplayStyle.None;

        private static int CountShown(Sheet sheet)
        {
            int shown = 0;
            foreach (var slot in sheet.Slots)
                if (IsShown(slot)) shown++;
            return shown;
        }

        // A free sheet: its element, the shadow at its back, the opener first in it
        private Sheet Take()
        {
            if (_pool.Count > 0) return _pool.Pop();
            var root = new VisualElement { name = "card-section" };
            root.AddToClassList(SheetClass);
            var shadow = new VisualElement { name = "card-section-shadow", pickingMode = PickingMode.Ignore };
            shadow.AddToClassList(ShadowClass);
            // - the edge is laid over the sheet's ground but under its content (the opener and the blocks come after it)
            var edge = new VisualElement { name = "card-section-edge", pickingMode = PickingMode.Ignore };
            edge.AddToClassList(EdgeClass);
            var opener = new SectionOpenerView();
            root.Add(shadow);
            root.Add(edge);
            root.Add(opener.Root);
            return new Sheet { Root = root, Shadow = shadow, Opener = opener, Tint = new NavTint(root) };
        }
    }
}
