using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's chapters in the scroll (_3.3 P4.4c1, R11, R10.7): the stack's scroll blocks, cut by SectionSheetRule into runs of one
    // family, each run WRAPPED in one glass sheet -- its opener (SectionOpenerView), then its blocks' slots -- the sheets one after the other
    // in the scroll, each laid over the one before (a negative top margin and a shadow cast up). The sheet is tinted with its family's glass
    // (the theme's "<colour token>-glass", painted by NavTint so a mode switch repaints it); its blocks stay opaque frames (BlockFrame) and
    // its words sit on the opener's filled pill: no text on the glass (D8). Inside the glass a sibling layer under the opener and the blocks
    // carries its living light (CardLight, P4.4c2). A bare run (the navigation family) is a plain wrapper.
    //   - Build wraps once per bind (a view is never moved between parents while bound); Refresh re-reads which slots are shown (a layout
    //     sets blocks aside, a block is revealed) and shows / hides each sheet and its opener, with its count and minutes, by the rule.
    //   - A slot's layout is now relative to its SHEET: anything that needs a block's place in the scroll asks BlockStackView.TopInScroll.
    // OPEN (Look Blocks = Open, the default; _3.3 fourth round, v6): no glass and no opener -- the scroll is cut by Navigation Bar GROUP
    // (NavBarRule: a group's families are one section), and each section draws its own parts (OpenSectionView: the divider, the start, its
    // tinted ground, the eyebrow) as SectionStyleRule resolves card_settings.sections and the group's Section Style; its blocks are open
    // (BlockStackView). Cards keeps the family sheets above, unchanged.
    // The look is PoiCard.uss (.card-section*, .card-open-section*). Plain C#, owned by BlockStackView.
    public sealed class SectionSheets
    {
        public const string SheetClass = "card-section";
        public const string BareClass = "card-section--bare";
        public const string OverClass = "card-section--over";
        public const string ContinuesClass = "card-section--continues";
        public const string ShadowClass = "card-section__shadow";
        public const string EdgeClass = "card-section__edge";
        public const string LightClass = "card-section__light";
        public const string OpenClass = "card-section--open";
        public const string FirstOpenClass = "card-section--open-first";
        // An open section's blocks after its first: a hairline and space above each (v6)
        public const string FollowsClass = "card-block-slot--follows";

        // One sheet: its element, its shadow part, its opener, its colours, and the blocks it holds now
        public sealed class Sheet
        {
            public VisualElement Root;
            public VisualElement Shadow;
            // The living glass: the soft lights of its family drifting inside it (CardLight; none on a bare run)
            public VisualElement Light;
            public SectionOpenerView Opener;
            public NavTint Tint;
            public SectionSheetRule.Run Run;
            public FamilyDefinition Family;
            public readonly List<VisualElement> Slots = new();
            public readonly List<int> Minutes = new();
            // How it shows now (SectionSheetRule.Looks)
            public SectionSheetRule.Look Look;
            // Open only: its own parts, its group (the bar's order index, -1 for none: a family no group holds) and the group's row
            public OpenSectionView Open;
            public int GroupIndex = -1;
            public NavBarRule.Item Group;
            public string Shown = "";
        }

        private readonly VisualElement _content;
        private readonly BlockRegistry _registry;
        private readonly CardLight _light;
        private readonly Stack<Sheet> _pool = new();
        private readonly List<Sheet> _sheets = new();
        private System.Func<string, string> _words;
        private readonly List<bool> _shown = new();
        private readonly List<SectionSheetRule.Run> _runs = new();
        private BlockBindContext _context;
        private List<NavBarRule.Item> _groups = new();
        private List<CardNavBarItem> _groupRows = new();
        private string _dividerImagePath;
        private Texture2D _dividerImage;
        // - a group's key in a run is marked, so a group named like a family ("about") is never read as that family
        private const string GroupKeyPrefix = "group:";

        // Whether the bound card's sections are open (Look Blocks = Open)
        public bool IsOpen { get; private set; }

        public SectionSheets(VisualElement content, BlockRegistry registry, CardLight light)
        {
            _content = content;
            _registry = registry;
            _light = light;
        }

        // The sheets of the bound card, in scroll order
        public IReadOnlyList<Sheet> All => _sheets;

        // Whether the openers say their words (the family's name, its count and minutes): the card's layout decides it
        // (CardLayoutRule.OpenerShowsWords, set by CardLayoutView before it sets blocks aside -- which refreshes); the next Refresh applies it
        public bool OpenerWords { get; set; } = true;

        // Wrap the scroll's blocks -- their slots and entries, in stack order -- into sheets appended to the scroll, then show them as the
        // slots are shown now
        public void Build(IReadOnlyList<VisualElement> slots, IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context)
        {
            Release();
            _context = context;
            _words = context.Strings != null ? context.Strings.Get : null;
            IsOpen = SectionStyleRule.IsOpen(context.Settings?.container);
            var nav = context.Settings?.navigation;
            _groups = NavBarRule.Items(nav, _registry);
            _groupRows = nav?.bar?.items != null && nav.bar.items.Count > 0 ? nav.bar.items : NavBarRule.DefaultRows(nav, _registry);
            // - open: a run is a GROUP's (its families together); a family no group holds keeps a run of its own. Cards: by family
            var keys = new List<string>(entries.Count);
            foreach (var entry in entries) keys.Add(IsOpen ? SectionKeyOf(entry.Definition.Family) : entry.Definition.Family);
            _runs.Clear();
            _runs.AddRange(SectionSheetRule.Runs(keys, key => GroupAt(key) >= 0 || !_registry.TryGetFamily(key, out var f) || f.OpensSection));
            foreach (var run in _runs)
            {
                var sheet = Take();
                sheet.Run = run;
                sheet.GroupIndex = IsOpen ? GroupAt(run.Family) : -1;
                sheet.Group = sheet.GroupIndex >= 0 ? _groups[sheet.GroupIndex] : default;
                sheet.Family = null;
                _registry.TryGetFamily(sheet.GroupIndex >= 0 ? FirstFamily(sheet.Group) : run.Family, out sheet.Family);
                sheet.Shown = "";
                sheet.Root.EnableInClassList(BareClass, run.Bare);
                sheet.Root.EnableInClassList(OpenClass, IsOpen && !run.Bare);
                // - a pooled sheet may come from an open card: its open parts go back to the stylesheet's say (hidden off an open sheet)
                if (!IsOpen || run.Bare)
                    sheet.Open.Edge.style.display = sheet.Open.Eyebrow.style.display = sheet.Open.Ground.style.display = StyleKeyword.Null;
                for (int i = run.Start; i < run.Start + run.Count; i++)
                {
                    sheet.Slots.Add(slots[i]);
                    sheet.Minutes.Add(ReadingTimeRule.Minutes(entries[i].Definition, entries[i].Instance, 0f));
                    sheet.Root.Add(slots[i]);
                }
                sheet.Tint.Clear();
                if (!IsOpen && !run.Bare && sheet.Family != null) sheet.Tint.Fill(sheet.Root, SectionSheetRule.GlassOf(sheet.Family.ColorToken));
                // - each sheet's lights start their drift at their own place in the card, so two sheets in view never move in step (open: none)
                _light.Glass(sheet.Light, run.Bare || IsOpen ? null : sheet.Family?.ColorToken, _sheets.Count);
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
            // - an open section's place among the sections shown (the first has no divider; Bands tints every other one)
            int position = 0;
            for (int i = 0; i < _sheets.Count; i++)
            {
                var sheet = _sheets[i];
                sheet.Look = looks[i];
                sheet.Root.style.display = looks[i].Shown ? DisplayStyle.Flex : DisplayStyle.None;
                sheet.Root.EnableInClassList(OverClass, !IsOpen && looks[i].Overlaps);
                sheet.Root.EnableInClassList(ContinuesClass, looks[i].Continues);
                if (IsOpen)
                {
                    if (!looks[i].Shown) continue;
                    ShowOpen(sheet, looks[i], looks[i].Opener ? position++ : -1);
                    continue;
                }
                sheet.Opener.Root.style.display = looks[i].Opener ? DisplayStyle.Flex : DisplayStyle.None;
                if (!looks[i].Opener || sheet.Family == null) continue;
                int minutes = 0;
                for (int s = 0; s < sheet.Slots.Count; s++)
                    if (IsShown(sheet.Slots[s])) minutes += sheet.Minutes[s];
                string name = sheet.Family.LabelKey != null && _words != null ? _words(sheet.Family.LabelKey) : sheet.Family.Key;
                // - tabs: the strip names the section, the opener keeps only its colour (pill, icon, line)
                sheet.Opener.Show(sheet.Family.IconKey, sheet.Family.ColorToken, OpenerWords ? name : "",
                    OpenerWords ? SectionSheetRule.Meta(CountShown(sheet), minutes, _words) : "");
            }
        }

        // One scroll moment (offset, viewport: the scroll's). Cards: draw a sheet's living glass only while the sheet is within half a viewport
        // of the visible part -- an off-screen material still costs a SetPass and a batch every frame (P4.4c2, measured on The Lamp: 15
        // sheets lit 7.9 -> 40 SetPass, only those in view 13.8); null hands the decision back to the stylesheet. Open: a section shown whose
        // top came into the view has entered it (its Reveal starts, once: RevealRule.Entered) -- only once the scroll's places are final
        // (`laidOut`, BlockStackView.LaidOut)
        public void OnScrolled(float offset, float viewport, bool laidOut)
        {
            float margin = viewport * 0.5f;
            foreach (var sheet in _sheets)
            {
                var r = sheet.Root.layout;
                if (IsOpen)
                {
                    sheet.Light.style.display = DisplayStyle.None;
                    if (laidOut && sheet.Root.style.display != DisplayStyle.None && !float.IsNaN(r.height) && r.height > 0f
                        && LookKit.RevealRule.Entered(r.yMin, offset, viewport, EnterMargin))
                        sheet.Open.Entered();
                    continue;
                }
                bool near = !float.IsNaN(r.height) && r.yMax > offset - margin && r.yMin < offset + viewport + margin;
                sheet.Light.style.display = near ? StyleKeyword.Null : DisplayStyle.None;
            }
        }

        // How far into the view an open section must come to have entered it (v6: 30)
        private static readonly float EnterMargin = new LookKit.RevealOptions().EnterMargin;

        // An open section shown at `position` among the shown sections (-1: it continues the one before -- no edge, no eyebrow): its parts,
        // shown again only when what they show changed (a re-show restarts the Reveal)
        private void ShowOpen(Sheet sheet, SectionSheetRule.Look look, int position)
        {
            sheet.Opener.Root.style.display = DisplayStyle.None;
            bool parts = look.Opener;
            sheet.Open.Edge.style.display = parts ? DisplayStyle.Flex : DisplayStyle.None;
            sheet.Open.Eyebrow.style.display = parts ? DisplayStyle.Flex : DisplayStyle.None;
            sheet.Open.Ground.style.display = parts ? DisplayStyle.Flex : DisplayStyle.None;
            sheet.Root.EnableInClassList(FirstOpenClass, position == 0);
            bool first = true;
            foreach (var slot in sheet.Slots)
            {
                bool shown = IsShown(slot);
                slot.EnableInClassList(FollowsClass, shown && !first);
                if (shown) first = false;
            }
            if (!parts) return;
            string groupStart = sheet.GroupIndex >= 0 ? RowOf(sheet.Group.Key)?.section_style ?? "" : "";
            var style = SectionStyleRule.Of(_context?.Settings?.sections, position, Mathf.Max(0, sheet.GroupIndex), groupStart);
            string colour = sheet.GroupIndex >= 0 ? sheet.Group.ColorToken : sheet.Family?.ColorToken;
            string name = sheet.GroupIndex >= 0 ? NavBarRule.Words(sheet.Group, _context?.Strings)
                : sheet.Family?.LabelKey != null && _words != null ? _words(sheet.Family.LabelKey) : sheet.Run.Family;
            string shownNow = string.Join("|", style.Start, style.Divider, style.Colour, style.Intensity, style.Motion, position, colour, name, OpenerWords);
            if (shownNow == sheet.Shown) return;
            sheet.Shown = shownNow;
            sheet.Open.Show(sheet.Run, style, position, colour, name, DividerImage(style), _context?.ReduceMotion ?? false, OpenerWords);
        }

        // Divider Image's picture (a path under the wall's media folder), loaded through the card's media source once per bind
        private Texture2D DividerImage(SectionStyleRule.Style style)
        {
            if (style.Divider != CardOptions.SectionDividerImage) return null;
            string path = _context?.Settings?.sections?.divider_image;
            if (string.IsNullOrEmpty(path) || _context?.Media == null) return null;
            if (_dividerImagePath != path)
            {
                if (_dividerImagePath != null) _context.Media.Release(_dividerImagePath);
                _dividerImagePath = path;
                _dividerImage = _context.Media.Load<Texture2D>(path);
            }
            return _dividerImage;
        }

        // The section key of a family on an open card: its group's key (the first row holding it), else the family itself
        private string SectionKeyOf(string family)
        {
            int at = NavBarRule.ItemOfFamily(_groups, family);
            return at >= 0 ? GroupKeyPrefix + _groups[at].Key : family;
        }

        private int GroupAt(string sectionKey)
        {
            if (sectionKey == null || !sectionKey.StartsWith(GroupKeyPrefix)) return -1;
            string key = sectionKey.Substring(GroupKeyPrefix.Length);
            for (int i = 0; i < _groups.Count; i++)
                if (_groups[i].Key == key) return i;
            return -1;
        }

        private CardNavBarItem RowOf(string key)
        {
            foreach (var row in _groupRows)
                if (row != null && row.key == key) return row;
            return null;
        }

        private static string FirstFamily(NavBarRule.Item item) => item.Families != null && item.Families.Count > 0 ? item.Families[0] : null;

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
                foreach (var slot in sheet.Slots) slot.RemoveFromClassList(FollowsClass);
                sheet.Slots.Clear();
                sheet.Minutes.Clear();
                sheet.Open.Stop();
                sheet.Root.RemoveFromHierarchy();
                _pool.Push(sheet);
            }
            _sheets.Clear();
            _runs.Clear();
            if (_dividerImagePath != null) _context?.Media?.Release(_dividerImagePath);
            _dividerImagePath = null;
            _dividerImage = null;
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
            // - the lights and the edge are laid over the sheet's ground but under its content (the opener and the blocks come after them)
            var light = new VisualElement { name = "card-section-light", pickingMode = PickingMode.Ignore };
            light.AddToClassList(LightClass);
            var edge = new VisualElement { name = "card-section-edge", pickingMode = PickingMode.Ignore };
            edge.AddToClassList(EdgeClass);
            var opener = new SectionOpenerView();
            var open = new OpenSectionView(root);
            root.Add(shadow);
            root.Add(light);
            root.Add(edge);
            // - an open section's ground at the back, its edge first in the flow, its eyebrow after the (hidden) opener; the stylesheet shows
            //   them only on an open sheet
            root.Add(open.Ground);
            root.Add(open.Edge);
            root.Add(opener.Root);
            root.Add(open.Eyebrow);
            return new Sheet { Root = root, Shadow = shadow, Light = light, Opener = opener, Open = open, Tint = new NavTint(root) };
        }
    }
}
