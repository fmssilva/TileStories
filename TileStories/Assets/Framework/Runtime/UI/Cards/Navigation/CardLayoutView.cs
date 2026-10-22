using System;
using System.Collections.Generic;

namespace TileStories
{
    // The card's LAYOUT applied to its stack (_3.2 section 6.1, steps 13-14): sheet-owned beside NavController, which asks it before every
    // jump. CardLayoutRule decides; this class only moves the SAME bound slots -- tabs and pages set the sections not shown aside
    // (BlockStackView.SetParked: unbound, so a hidden section holds no media), the accordion closes the other rows when Open One At A Time
    // is on (the accordion's rows themselves are the stack's: CardLayoutRule.ShowsCompact). A kind's view never learns the layout.
    //   - tabs / pages: one section shown (pages: one page; CardPagerView turns them sideways); a jump first shows the section of its
    //     block; the scroll-spy names the shown section; a rebind of the same point keeps the section shown, another point starts at its first
    //   - any layout: a jump to a block drawn as a closed row opens that row first
    //   - bento (step 15b): every section set aside (CardBentoView's tiles stand for them); a jump to a block of a section opens that section
    //     in the full view (the sheet's OpenSectionView, handed over as SectionOpener) -- the section open there is what the spy names, none
    //     on the overview; a rebind of the same point keeps it open (the sheet reopens it by key)
    public sealed class CardLayoutView
    {
        private readonly BlockStackView _stack;
        private CardNavModel _model;
        private CardNavigationSettings _settings;
        // The section tabs shows, by key (what a rebind of the same point keeps), and the point it belongs to
        private string _shownKey;
        private string _poi;

        // The layout the shown card is drawn in (CardLayoutRule.Effective: tabs falls back to scroll on a card with nothing to switch between)
        public string Effective { get; private set; } = CardOptions.LayoutScroll;

        // The index in the model's Sections of the section tabs shows (-1 in any other layout)
        public int ShownSection { get; private set; } = -1;

        // Bento: the section open in the full view (-1 on the overview), its key, and the block it opened at
        public int OpenSection { get; private set; } = -1;
        public string OpenSectionKey { get; private set; }
        public string OpenBlock { get; private set; }
        // Raised when the section open in the full view changes (opened, closed): what the controller's spy follows
        public event Action Changed;
        // Opens section i in the full view, landing on a block (null: its top): the sheet's, which owns the full view. False when it opened nothing
        internal Func<int, string, bool> SectionOpener { get; set; }

        public CardLayoutView(BlockStackView stack)
        {
            _stack = stack;
            _stack.ExpandedChanged += OnExpandedChanged;
        }

        // Before the stack binds a card: the blocks that start set aside (null = none). `model` is built from the same entries with the reveal
        // the stack will start from, so the first frame already holds only the shown section's views
        public Func<string, bool> ParkedAtBind(CardNavModel model, BlockBindContext context)
        {
            string effective = CardLayoutRule.Effective(context?.Settings?.navigation, model);
            if (!CardLayoutRule.ParksSections(effective)) return null;
            int shown = CardLayoutRule.OpensSectionsInFullView(effective) ? -1 : SectionToShow(model, context?.Poi?.id);
            return key => !CardLayoutRule.ShowsWith(SectionOf(model, key), shown);
        }

        // The shown card's map is (re)built (a Show, a reveal): apply its layout -- before NavController binds, so the spy reads it
        public void Bind(CardNavModel model, BlockBindContext context)
        {
            string poi = context?.Poi?.id;
            int shown = SectionToShow(model, poi);
            _model = model;
            _settings = context?.Settings?.navigation;
            _poi = poi;
            Effective = CardLayoutRule.Effective(_settings, model);
            // - bento: no section in the stack; a section open in the full view is found again by key (a reveal rebuilt the model)
            if (CardLayoutRule.OpensSectionsInFullView(Effective))
            {
                ShownSection = -1;
                _shownKey = null;
                OpenSection = IndexOf(model, OpenSectionKey);
                if (OpenSection < 0) ClearOpen();
                _stack.SetParked(key => SectionOf(model, key) >= 0);
                return;
            }
            ClearOpen();
            if (CardLayoutRule.ParksSections(Effective) && shown >= 0) Show(shown);
            else
            {
                ShownSection = -1;
                _shownKey = null;
                _stack.SetParked(null);
            }
        }

        // The card closed
        public void Unbind()
        {
            _model = null;
            _settings = null;
            _poi = null;
            _shownKey = null;
            ShownSection = -1;
            ClearOpen();
            Effective = CardOptions.LayoutScroll;
        }

        // Bento: open section `index` in the full view, landing on `blockKey` (null: its top). False in any other layout or when nothing opened
        public bool OpenSectionView(int index, string blockKey)
        {
            if (_model == null || !CardLayoutRule.OpensSectionsInFullView(Effective) || index < 0 || index >= _model.Sections.Count) return false;
            if (SectionOpener == null || !SectionOpener(index, blockKey)) return false;
            OpenSection = index;
            OpenSectionKey = _model.Sections[index].Key;
            OpenBlock = blockKey ?? _model.Sections[index].Targets[0];
            Changed?.Invoke();
            return true;
        }

        // Bento: open again the section that was open before a rebind of the same point (by key; nothing when the card no longer has it)
        public bool ReopenSection(string sectionKey) => OpenSectionView(IndexOf(_model, sectionKey), null);

        // Bento: the full view closed -- back on the overview, no section being read
        public void SectionViewClosed()
        {
            if (OpenSection < 0 && OpenSectionKey == null) return;
            ClearOpen();
            Changed?.Invoke();
        }

        private void ClearOpen()
        {
            OpenSection = -1;
            OpenSectionKey = null;
            OpenBlock = null;
        }

        private static int IndexOf(CardNavModel model, string sectionKey)
        {
            if (model == null || sectionKey == null) return -1;
            for (int i = 0; i < model.Sections.Count; i++)
                if (model.Sections[i].Key == sectionKey) return i;
            return -1;
        }

        // Tabs: show section `index` -- its blocks get their views back, every other section's are set aside
        public void Show(int index)
        {
            if (_model == null || index < 0 || index >= _model.Sections.Count) return;
            ShownSection = index;
            _shownKey = _model.Sections[index].Key;
            var model = _model;
            _stack.SetParked(key => !CardLayoutRule.ShowsWith(SectionOf(model, key), index));
        }

        // A jump is about to go to `blockKey` (of section `section`, -1 for a block of none): show its section (tabs), open its row
        public void PrepareJump(string blockKey, int section)
        {
            if (_model == null) return;
            if (CardLayoutRule.ShowsSectionBeforeJump(Effective) && section >= 0 && section != ShownSection) Show(section);
            bool closedRow = _stack.CompactRowOf(blockKey) != null && !_stack.IsExpanded(blockKey);
            if (CardLayoutRule.OpensRowBeforeJump(closedRow)) _stack.Expand(blockKey);
        }

        // The section the scroll-spy names, from what it measured (bento: the one open in the full view)
        public int SpySection(int measured) =>
            CardLayoutRule.SpySection(Effective, CardLayoutRule.OpensSectionsInFullView(Effective) ? OpenSection : ShownSection, measured);

        // The target the block spy names, from what it measured (bento: the block the full view opened at; tabs and pages: a block of the
        // section shown, never one of a section set aside)
        public string SpyBlock(string measured)
        {
            bool shownValid = _model != null && ShownSection >= 0 && ShownSection < _model.Sections.Count;
            string first = shownValid ? _model.Sections[ShownSection].Targets[0] : null;
            bool onShown = shownValid && measured != null && SectionOf(_model, measured) == ShownSection;
            return CardLayoutRule.SpyBlock(Effective, OpenBlock, measured, onShown, first);
        }

        // Each section as the card's reading sees it (ContentSeenRule.HasReadCard): shown now, or set aside -- and then read to its end when
        // its last target showing content was viewed earlier in this binding (`viewed`: the controller's memory, NavController.HasViewed)
        public IEnumerable<ContentSeenRule.Section> Reading(Func<string, bool> viewed)
        {
            if (_model == null) yield break;
            var seen = new List<bool>();
            var closedRow = new List<bool>();
            foreach (var section in _model.Sections)
            {
                if (!_stack.IsParked(section.Targets[0]))
                {
                    yield return new ContentSeenRule.Section(shownNow: true, readToItsEnd: false);
                    continue;
                }
                seen.Clear();
                closedRow.Clear();
                foreach (string target in section.Targets)
                {
                    seen.Add(viewed(target));
                    closedRow.Add(_stack.CompactRowOf(target) != null && !_stack.IsExpanded(target));
                }
                yield return new ContentSeenRule.Section(shownNow: false, ContentSeenRule.ReadToItsEnd(seen, closedRow));
            }
        }

        // A row opened: with Open One At A Time the others close for it (the opened row keeps its place on screen)
        private void OnExpandedChanged(string key)
        {
            if (!_stack.IsExpanded(key)) return;
            var open = new List<string>(_stack.OpenRows);
            foreach (string other in AccordionRule.ClosedByOpening(open, key, CardLayoutRule.OneOpenAtATime(_settings)))
                _stack.CollapseFor(other, key);
        }

        // The section kept over a rebind of the same point (found again by key: indices may move), else the first
        private int SectionToShow(CardNavModel model, string poi)
        {
            if (model == null || model.Sections.Count == 0) return -1;
            if (poi != null && poi == _poi && _shownKey != null)
                for (int i = 0; i < model.Sections.Count; i++)
                    if (model.Sections[i].Key == _shownKey) return i;
            return 0;
        }

        // The section a block belongs to (-1: no section -- the sources, a nav block, a block not shown yet)
        private static int SectionOf(CardNavModel model, string blockKey)
        {
            for (int i = 0; i < model.Sections.Count; i++)
                foreach (string target in model.Sections[i].Targets)
                    if (target == blockKey) return i;
            return -1;
        }
    }
}
