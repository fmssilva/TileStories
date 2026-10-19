using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace TileStories
{
    // The nav_contents block (_3.2 section 5.6, D2): the card's contents at L3 -- under each section's name (the family icon in its colour and
    // its words) one row per TARGET block, in the stack's order: a marker, the block's heading (else its kind's words) and its minutes
    // (ReadingTimeRule: the Reading Time field, or an audio / video clip's own length). A tap moves the card to that block (nav_jump
    // "nav_contents"). A row the visitor has had in view is marked READ twice, never by the tick alone: the marker fills (an empty ring
    // becomes a filled disc holding a tick) and the heading drops from bold to regular -- the way an inbox marks a read message.
    // It computes no structure and no geometry: the rows are NavController.Model's targets, drawn when the controller is Bound (the model is
    // built from the shown stack, after this block bound), and "in view" is the controller's TargetViewed (NavScrollRule's spy line). The
    // view remembers each one in CardLocalState (MarkViewed: per wall, POI and block), so the ticks are there the next time the card opens;
    // with no CardLocalState it keeps them for this binding only. It lists blocks, not sections, so rule 2.5.7 does not apply: a card whose
    // targets are all in one section still gets its rows.
    // Rows (_3.2 step 13, NavContentsRule): Sections First -- each section's title is a compact row (CompactRowView: the card's one accordion
    // look, AccordionRule) saying how many of its blocks were read, and a tap on it opens or folds its rows; Every Block -- every row shows
    // under a plain title. The card's own inserted contents block always starts folded.
    public sealed class NavContentsBlockView : IBlockView
    {
        // What nav_jump names as the element that asked
        public const string ElementKey = BuiltInBlocks.NavContentsKind;
        // The class a read row wears (the marker fills, the heading drops to regular): what a test reads
        public const string ReadClass = "nav-contents__row--read";

        // One row of the list: the block it leads to and its parts
        public sealed class Row
        {
            public readonly Button Button;
            public readonly VisualElement Marker;
            public readonly CardIcons.VectorGlyph Tick;
            public readonly Label Heading;
            public readonly Label Minutes;
            public string BlockKey { get; internal set; }
            // The section the row is listed under (its title folds it away with Sections First)
            public string SectionKey { get; internal set; }
            public bool IsRead => Button.ClassListContains(ReadClass);

            internal Row(Button button, VisualElement marker, CardIcons.VectorGlyph tick, Label heading, Label minutes)
            {
                Button = button;
                Marker = marker;
                Tick = tick;
                Heading = heading;
                Minutes = minutes;
            }
        }

        // One section's name above its rows: a plain title (Every Block), or a compact row that opens / folds them (Sections First)
        public sealed class SectionTitle
        {
            public readonly VisualElement Root;
            public readonly VisualElement Icon;
            public readonly Label Words;
            // The folding title (shown instead of the plain one with Sections First): its line opens or folds the section's rows
            public readonly CompactRowView Header;
            public string SectionKey { get; internal set; }

            internal SectionTitle(VisualElement root, VisualElement icon, Label words, CompactRowView header)
            {
                Root = root;
                Icon = icon;
                Words = words;
                Header = header;
            }
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Row> Rows => _rows.Shown;
        public IReadOnlyList<SectionTitle> Sections => _sections.Shown;
        // Whether the rows fold under their section titles now (NavContentsRule.SectionsFirst)
        public bool SectionsFirst { get; private set; }
        // Whether a section's rows show (always with Every Block)
        public bool IsSectionOpen(string sectionKey) => !SectionsFirst || _openSections.Contains(sectionKey);

        private readonly ElementPool<Row> _rows;
        private readonly ElementPool<SectionTitle> _sections;
        private readonly NavTint _tint;
        // The visitor's reads in this binding (what a card with no CardLocalState still shows) and the clip lengths loaded for it, by path
        private readonly HashSet<string> _viewedHere = new();
        private readonly Dictionary<string, float> _clipSeconds = new();
        // The sections opened in this binding, in the order they were opened (AccordionRule; kept over a redraw of the same binding)
        private readonly List<string> _openSections = new();
        private string _blockKey;
        private NavController _nav;
        // The model the rows were drawn from: a TargetViewed for any other (raised while the stack rebinds over the previous card's model)
        // is not about these rows and is ignored
        private CardNavModel _drawn;
        private BlockBindContext _context;

        public NavContentsBlockView()
        {
            Root = new VisualElement { name = "card-nav-contents" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("nav-contents");
            _rows = new ElementPool<Row>(NewRow, row => row.Button.RemoveFromHierarchy());
            _sections = new ElementPool<SectionTitle>(NewSection, section => section.Root.RemoveFromHierarchy());
            _tint = new NavTint(Root);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _context = context;
            _nav = context.Nav;
            _blockKey = instance?.key;
            // - nothing to draw yet: the sheet binds the controller to THIS card's model right after the stack (Bound)
            Root.style.display = DisplayStyle.None;
            if (_nav == null) return;
            _nav.Bound += Draw;
            _nav.TargetViewed += OnViewed;
            // - bound later than the stack (an expandable row opened): the controller holds this card's model already
            if (_nav.HasCurrentModel) Draw();
        }

        public void Unbind()
        {
            if (_nav != null)
            {
                _nav.Bound -= Draw;
                _nav.TargetViewed -= OnViewed;
            }
            _nav = null;
            _context = null;
            _drawn = null;
            _blockKey = null;
            _openSections.Clear();
            _viewedHere.Clear();
            _clipSeconds.Clear();
            _rows.ReleaseAll();
            _sections.ReleaseAll();
            _tint.Clear();
        }

        // One section title per section of the bound model and one row per target under it, in the stack's order; nothing with no target
        private void Draw()
        {
            _rows.ReleaseAll();
            _sections.ReleaseAll();
            _tint.Clear();
            var model = _nav?.Model;
            _drawn = model;
            bool shown = model != null && model.TargetCount > 0;
            Root.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            if (!shown) return;
            // - this block's own entry says how its rows are listed (a full view of it reads the same entry)
            SectionsFirst = _nav.TryGetEntry(_blockKey, out var self) && NavContentsRule.SectionsFirst(self, _context?.Settings);
            foreach (var section in model.Sections)
            {
                var title = _sections.Take();
                Root.Add(title.Root);
                title.SectionKey = section.Key;
                title.Words.text = Words(section.LabelKey, section.Key);
                // - a format section has no icon: its words say it alone (NavLabelRule)
                CardIcons.SetKey(title.Icon, section.IconKey);
                title.Icon.style.display = section.IconKey != null && !SectionsFirst ? DisplayStyle.Flex : DisplayStyle.None;
                title.Words.style.display = SectionsFirst ? DisplayStyle.None : DisplayStyle.Flex;
                title.Header.Root.style.display = SectionsFirst ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(title.Icon, section.ColorToken);
                foreach (string target in section.Targets)
                {
                    var row = _rows.Take();
                    Root.Add(row.Button);
                    row.BlockKey = target;
                    row.SectionKey = section.Key;
                    string heading = _nav.HeadingOf(target);
                    row.Heading.text = heading.Length > 0 ? heading : Words(model.KindOf(target)?.LabelKey, "");
                    int minutes = MinutesOf(target);
                    row.Minutes.text = minutes > 0
                        ? string.Format(CultureInfo.InvariantCulture, Words(CardStrings.Keys.NavContentsMinutes, "{0}"), minutes) : "";
                    row.Minutes.style.display = minutes > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    SetRead(row, WasViewed(target));
                }
            }
            ShowSections();
        }

        // Each section's folding title (its words, how many of its blocks were read, open or not) and whether its rows show
        private void ShowSections()
        {
            var model = _drawn;
            if (model == null) return;
            foreach (var title in _sections.Shown)
            {
                if (!SectionsFirst) continue;
                CardNavModel.Section section = null;
                foreach (var s in model.Sections)
                    if (s.Key == title.SectionKey) section = s;
                if (section == null) continue;
                int read = NavContentsRule.ReadCount(section.Targets, WasViewed);
                string meta = string.Format(CultureInfo.InvariantCulture, Words(CardStrings.Keys.NavContentsSectionRead, "{0}/{1}"), read, section.Targets.Count);
                title.Header.Show(new CompactRowRule.Row(title.Words.text, false, section.IconKey, section.ColorToken, "", 0, meta),
                    _context?.Strings, offersFullView: false);
                title.Header.SetExpanded(IsSectionOpen(section.Key));
            }
            foreach (var row in _rows.Shown)
                row.Button.style.display = IsSectionOpen(row.SectionKey) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // A tap on a folding title: its rows open under it, or fold away (sections open independently: AccordionRule with no One At A Time)
        public void ToggleSection(string sectionKey)
        {
            if (!SectionsFirst || sectionKey == null) return;
            if (_openSections.Remove(sectionKey))
            {
                ShowSections();
                return;
            }
            foreach (string closed in AccordionRule.ClosedByOpening(_openSections, sectionKey, oneAtATime: false)) _openSections.Remove(closed);
            _openSections.Add(sectionKey);
            ShowSections();
        }

        // The controller saw the visitor have a target in view: remember it on the device and tick its row
        private void OnViewed(string blockKey)
        {
            if (_drawn == null || !ReferenceEquals(_nav?.Model, _drawn)) return;
            var row = RowOf(blockKey);
            if (row == null) return;
            _viewedHere.Add(blockKey);
            _context?.State?.MarkViewed(_context.Poi?.id, blockKey);
            SetRead(row, true);
            // - its section's folded title counts it
            if (SectionsFirst) ShowSections();
        }

        private bool WasViewed(string blockKey) =>
            _viewedHere.Contains(blockKey) || (_context?.State?.Viewed(_context.Poi?.id, blockKey) ?? false);

        private void SetRead(Row row, bool read)
        {
            row.Button.EnableInClassList(ReadClass, read);
            row.Tick.style.visibility = read ? Visibility.Visible : Visibility.Hidden;
            // - the tick is never the only signal: a screen reader and a long press read the word too
            string readWords = read ? Words(CardStrings.Keys.NavContentsRead, "") : "";
            row.Button.tooltip = readWords.Length > 0 ? row.Heading.text + " - " + readWords : row.Heading.text;
        }

        private Row RowOf(string blockKey)
        {
            foreach (var row in _rows.Shown)
                if (row.BlockKey == blockKey) return row;
            return null;
        }

        // The block's minutes (ReadingTimeRule): its Reading Time, or its clip's own length -- the clip loaded through this view's media scope
        // (released with the view), once per path
        private int MinutesOf(string blockKey)
        {
            if (!_nav.TryGetEntry(blockKey, out var entry)) return 0;
            var clipField = ReadingTimeRule.ClipField(entry.Definition);
            float seconds = clipField != null ? ClipSeconds(entry.Instance, clipField) : 0f;
            return ReadingTimeRule.Minutes(entry.Definition, entry.Instance, seconds);
        }

        private float ClipSeconds(BlockInstanceData instance, BlockFieldDefinition clipField)
        {
            string path = new BlockFieldReader(instance, _context?.Language, _context?.FallbackLanguage).ValidAsset(clipField.Key, clipField.Media);
            if (path.Length == 0) return 0f;
            if (_clipSeconds.TryGetValue(path, out float known)) return known;
            var media = _context?.Media;
            float seconds = 0f;
            if (clipField.Media == MediaKind.Audio)
            {
                var clip = media?.Load<AudioClip>(path);
                seconds = clip != null ? clip.length : 0f;
            }
            else if (clipField.Media == MediaKind.Video)
            {
                var clip = media?.Load<VideoClip>(path);
                seconds = clip != null ? (float)clip.length : 0f;
            }
            _clipSeconds[path] = seconds;
            return seconds;
        }

        private string Words(string key, string fallback)
        {
            string words = key != null && _context?.Strings != null ? _context.Strings.Get(key) : "";
            return string.IsNullOrEmpty(words) ? fallback : words;
        }

        private Row NewRow(int index)
        {
            var row = (Row)null;
            var button = new Button(() => Tap(row.BlockKey)) { name = "nav-contents-row-" + index };
            button.AddToClassList("nav-contents__row");
            var marker = new VisualElement { pickingMode = PickingMode.Ignore };
            marker.AddToClassList("nav-contents__marker");
            var tick = CardIcons.CreateVector(CardIcons.Shape.Tick);
            tick.AddToClassList("nav-contents__tick");
            marker.Add(tick);
            var heading = new Label { pickingMode = PickingMode.Ignore };
            heading.AddToClassList("nav-contents__heading");
            var minutes = new Label { pickingMode = PickingMode.Ignore };
            minutes.AddToClassList("nav-contents__minutes");
            button.Add(marker);
            button.Add(heading);
            button.Add(minutes);
            row = new Row(button, marker, tick, heading, minutes);
            return row;
        }

        private SectionTitle NewSection(int index)
        {
            var root = new VisualElement { name = "nav-contents-section-" + index, pickingMode = PickingMode.Ignore };
            root.AddToClassList("nav-contents__section");
            var icon = CardIcons.Create();
            icon.AddToClassList("nav-contents__section-icon");
            var words = new Label { pickingMode = PickingMode.Ignore };
            words.AddToClassList("nav-contents__section-words");
            var header = new CompactRowView();
            header.Root.AddToClassList("nav-contents__section-header");
            root.Add(icon);
            root.Add(words);
            root.Add(header.Root);
            var title = new SectionTitle(root, icon, words, header);
            header.Toggled += () => ToggleSection(title.SectionKey);
            return title;
        }

        // A row moves the card to its block, as Up next or a drawer row would
        public void Tap(string blockKey)
        {
            if (blockKey != null) _nav?.JumpToBlock(blockKey, ElementKey);
        }
    }
}
