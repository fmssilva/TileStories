using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // What opens in the full view over the card (_3.2 steps 12, 15b, 16): a block among its siblings, a block's own pages (a gallery's
    // lightbox), a bento section as one page; the breadcrumb's section and the jump its tap makes; and, over a rebind of the SAME point,
    // the block or section that was open, opened again
    public sealed class CardFullViewOpener
    {
        private readonly PoiCardSheetView _sheet;
        private readonly BlockRegistry _registry;
        // The section the open full view's breadcrumb names (its index in the model; -1 = none): what a tap on it jumps to
        private int _crumbSection = -1;
        // What was open before a rebind of the same point, opened again once the card is bound (null = nothing)
        private string _reopenBlock;
        private string _reopenSection;

        public CardFullViewOpener(PoiCardSheetView sheet, BlockRegistry registry)
        {
            _sheet = sheet;
            _registry = registry;
            // - bento: a jump opens its section in the full view; what the visitor reads there counts as read, and closing it is the overview
            //   again (the reading asked again: the card may now have been read to its end)
            sheet.Layout.SectionOpener = OpenSectionView;
            sheet.FullView.Viewed += sheet.Nav.ViewedInFullView;
            sheet.FullView.SectionLeft += () =>
            {
                sheet.Layout.SectionViewClosed();
                if (sheet.IsOpen) sheet.Stack.ReadAgain();
            };
            // - a jump moves the card under the full view: the visitor would see nothing of it, so the full view closes (an Open Block
            //   button or a hotspot pin used inside it lands on the card)
            sheet.Nav.Jumped += sheet.Takeover.Close;
            sheet.Takeover.SectionTapped += JumpToCrumbSection;
        }

        // Before the card rebinds to `next`: a block in the full view stays there over a rebind of the SAME point (a live edit, a language
        // switch) -- remember it, to draw it again from the new stack. Another point replaces the full-screen view of the old one
        public void KeepAcrossRebind(BlockBindContext next)
        {
            var fullView = _sheet.FullView;
            bool samePoint = fullView.IsOpen && next.Poi?.id != null && next.Poi?.id == _sheet.Context?.Poi?.id;
            _reopenBlock = samePoint ? fullView.BlockKey : null;
            _reopenSection = samePoint ? fullView.SectionKey : null;
        }

        // Once the card is bound again: reopen what KeepAcrossRebind remembered, once. A block the rebind dropped (a live edit deleted it)
        // is not reopened: OpenFullView finds no entry; a section only while the card is still drawn as bento and still has it
        public void ReopenAfterRebind()
        {
            string block = _reopenBlock, section = _reopenSection;
            _reopenBlock = null;
            _reopenSection = null;
            if (block != null) OpenFullView(block);
            else if (section != null) _sheet.Layout.ReopenSection(section);
        }

        // A block asked for the full-screen view: the breadcrumb starts with this card's title and ends with `name`
        public void OpenTakeover(string name, int pageCount, int startPage, TakeoverPageDrawer drawPage) =>
            OpenPages(_ => name, pageCount, startPage, drawPage, returnTo: ReturnToThePageOpenNow());

        // The full-screen view with a name per page; no section in the breadcrumb until ShowCrumbSection names one. `returnTo`: what Back opens
        // again once it closes (null: the card)
        private void OpenPages(Func<int, string> nameOfPage, int pageCount, int startPage, TakeoverPageDrawer drawPage, Func<int, string> iconOfPage = null,
            Action returnTo = null)
        {
            var context = _sheet.Context;
            if (!_sheet.IsOpen || context == null) return;
            var stack = _sheet.Stack;
            string title = stack.BoundViews.Count > 0 && stack.BoundViews[0] is HeaderBlockView header ? header.TitleText : "";
            // - never stacked: the full view replaces what was open over the card
            _sheet.Overlay.Close();
            _crumbSection = -1;
            _sheet.Takeover.Open(title, nameOfPage, pageCount, startPage, context.Media, drawPage, context.Strings, iconOfPage, returnTo,
                PageChromeRule.Wall(context.Settings?.components));
        }

        // What Back returns to when a block asks for the full-screen view from inside its page (_3.3 S2b: a story's picture opens its viewer, its
        // video its player): that block's page again, at the scroll the visitor left it -- one level, never a chain. Null over the card itself
        private Action ReturnToThePageOpenNow()
        {
            var fullView = _sheet.FullView;
            string block = fullView.IsOpen ? fullView.BlockKey : null;
            if (block == null) return null;
            float offset = fullView.Scroll.scrollOffset.y;
            return () =>
            {
                OpenFullView(block);
                if (fullView.BlockKey == block) fullView.ReturnTo(offset);
            };
        }

        // A block asked for its full view (an open compact row's button, a bento tile): the page holds a second view of its kind, one page per
        // SIBLING (FullViewSiblingRule: its section's blocks that support the full view, a swipe or a chip away), each named by the words its
        // compact row shows; the breadcrumb names the section between the card's title and the block (_3.2 step 16). A key that is not on this
        // card opens nothing
        public void OpenFullView(string blockKey)
        {
            var context = _sheet.Context;
            var stack = _sheet.Stack;
            if (!_sheet.IsOpen || context == null || !stack.TryGetEntryOfBlock(blockKey, out var entry)) return;
            var pages = new List<BlockStackBuilder.Entry>();
            foreach (string sibling in FullViewSiblingRule.Siblings(_sheet.Nav.Model, _registry, blockKey))
                if (stack.TryGetEntryOfBlock(sibling, out var siblingEntry)) pages.Add(siblingEntry);
            if (pages.Count < 2) pages = new List<BlockStackBuilder.Entry> { entry };
            int start = Mathf.Max(0, pages.FindIndex(p => p.Instance.key == blockKey));
            Func<string, string> words = context.Strings != null ? context.Strings.Get : null;
            string language = context.Language, fallback = context.FallbackLanguage;
            // - a page with siblings: each chip carries its block's KIND icon (a page alone has no chips to tell apart: its number stays)
            OpenPages(page => CompactRowRule.Of(pages[page], _registry, language, fallback, words).Words, pages.Count, start,
                _sheet.FullView.PagesFor(pages, context), page => pages[page].Definition?.IconKey);
            ShowCrumbSection(FullViewSiblingRule.SectionOf(_sheet.Nav.Model, blockKey));
        }

        // The breadcrumb's middle segment: section `index` of the card's map, in the card's words, with its family icon
        private void ShowCrumbSection(int index)
        {
            var model = _sheet.Nav.Model;
            if (!_sheet.Takeover.IsOpen || model == null || index < 0 || index >= model.Sections.Count) return;
            var section = model.Sections[index];
            var strings = _sheet.Context.Strings;
            string words = strings != null && section.LabelKey != null ? strings.Get(section.LabelKey) : "";
            _crumbSection = index;
            _sheet.Takeover.ShowSection(string.IsNullOrEmpty(words) ? section.Key : words);
        }

        // The visitor tapped the breadcrumb's section: the card's ONE jump to that section (the full view closes on Jumped; in bento the jump
        // opens the section's page instead) -- no second path
        private void JumpToCrumbSection()
        {
            if (_sheet.IsOpen && _crumbSection >= 0) _sheet.Nav.JumpTo(_crumbSection, TakeoverView.CrumbElementKey);
        }

        // Bento: section `index` of the card's map in the full view -- its targets one under the other, landing on `landingKey` (null: its
        // top); the breadcrumb ends with the section's words. CardLayoutView asks (SectionOpener); false when nothing could be drawn
        private bool OpenSectionView(int index, string landingKey)
        {
            var model = _sheet.Nav.Model;
            var context = _sheet.Context;
            if (!_sheet.IsOpen || context == null || model == null || index < 0 || index >= model.Sections.Count) return false;
            var section = model.Sections[index];
            var stack = _sheet.Stack;
            var entries = new List<BlockStackBuilder.Entry>();
            foreach (string target in section.Targets)
                if (stack.TryGetEntryOfBlock(target, out var entry)) entries.Add(entry);
            if (entries.Count == 0) return false;
            string words = context.Strings != null && section.LabelKey != null ? context.Strings.Get(section.LabelKey) : "";
            OpenPages(_ => string.IsNullOrEmpty(words) ? section.Key : words, 1, 0,
                _sheet.FullView.SectionPageFor(entries, section.Key, stack.HeadingOfBlock, context, landingKey));
            return _sheet.FullView.IsOpen;
        }
    }
}
