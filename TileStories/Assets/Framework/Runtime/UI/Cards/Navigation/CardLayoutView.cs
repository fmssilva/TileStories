using System;
using System.Collections.Generic;

namespace TileStories
{
    // The card's LAYOUT applied to its stack (_3.2 section 6.1, step 13): sheet-owned beside NavController, which asks it before every jump.
    // CardLayoutRule decides; this class only moves the SAME bound slots -- the tabs layout sets the sections not shown aside
    // (BlockStackView.SetParked: unbound, so a hidden section holds no media), the accordion closes the other rows when Open One At A Time
    // is on (the accordion's rows themselves are the stack's: CardLayoutRule.ShowsCompact). A kind's view never learns the layout.
    //   - tabs: one section shown; a jump first shows the section of its block; the scroll-spy names the shown section; a rebind of the
    //     same point keeps the section shown, another point starts at its first
    //   - any layout: a jump to a block drawn as a closed row opens that row first
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

        public CardLayoutView(BlockStackView stack)
        {
            _stack = stack;
            _stack.ExpandedChanged += OnExpandedChanged;
        }

        // Before the stack binds a card: the blocks that start set aside (null = none). `model` is built from the same entries with the reveal
        // the stack will start from, so the first frame already holds only the shown section's views
        public Func<string, bool> ParkedAtBind(CardNavModel model, BlockBindContext context)
        {
            if (CardLayoutRule.Effective(context?.Settings?.navigation, model) != CardOptions.LayoutTabs) return null;
            int shown = SectionToShow(model, context?.Poi?.id);
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
            if (Effective == CardOptions.LayoutTabs && shown >= 0) Show(shown);
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
            Effective = CardOptions.LayoutScroll;
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

        // The section the scroll-spy names, from what it measured
        public int SpySection(int measured) => CardLayoutRule.SpySection(Effective, ShownSection, measured);

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
