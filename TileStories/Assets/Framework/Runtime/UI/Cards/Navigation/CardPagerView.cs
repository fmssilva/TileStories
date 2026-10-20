using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The pages layout's sideways turn (_3.2 section 6.1, step 14). The section shown (CardLayoutView: the others set aside, unbound) is one
    // PAGE that scrolls up and down in the stack; the next page PEEKS at the viewport's right edge as the cue (a panel with its family icon
    // and words, --ts-page-peek wide at rest: the stack's content is inset by it); a sideways drag slides the page with the finger and the
    // next or previous cue in beside it, and on release SwipePageRule says whether it turns and where it lands -- the turn is
    // NavController.JumpTo(page, "pages"), the same jump every element makes (its section shown, the others' media released, the spy, one
    // nav_jump), set at once since the slide already moved. THE RULE: a vertical drag never turns a page and a sideways one never scrolls it
    // -- SwipeGrab judges a drag ONCE as it leaves the slop and keeps that answer; a drag that starts on a part that drags by itself (a
    // filmstrip, the true / false card, a 3D stage) is that part's (SwipeGrabRule.LeavesToPart). Sheet-owned beside CardLayoutView; it reads
    // the layout, never decides it. Reduce motion turns at once.
    public sealed class CardPagerView
    {
        // What a page turned by a swipe reports as its element (nav_jump's Variant); the chevrons report family_pager
        public const string SwipeKey = "pages";
        // On the sheet while the card is drawn as pages: PoiCard.uss insets the stack's content by --ts-page-peek for the cue
        public const string PagedClass = "poi-card--paged";
        public const string SettlingClass = "card-pager--settling";
        // On a cue while it lists its page's blocks (a drag is claimed): its title moves to the top
        public const string TeaserClass = "card-pager__peek--teaser";

        // The next page's cue at the right edge (shown while there is a next page)
        public VisualElement Peek => _next.Root;
        // The compact rows the next page's cue lists now: empty at rest, the page's blocks while a drag is claimed until the turn is done
        public System.Collections.Generic.IReadOnlyList<CompactRowView> NextRows => _next.Rows.Shown;
        // The previous page's cue: on screen only while a drag pulls it in from the left
        public VisualElement PeekBack => _back.Root;
        public Label PeekWords => _next.Words;
        // Whether the card is drawn as pages now (CardLayoutRule.TurnsPages)
        public bool IsOn { get; private set; }
        // How far the page follows the finger now (0 at rest; panel units, negative = towards the next page)
        public float Offset { get; private set; }
        // The page width the last placement used: the viewport less the peek
        public float PageWidth { get; private set; }

        private readonly VisualElement _sheetRoot;
        private readonly ScrollView _scroll;
        private readonly CardLayoutView _layout;
        private readonly NavController _nav;
        private readonly SwipeGrab _grab;
        private readonly Cue _next;
        private readonly Cue _back;
        private CardNavModel _model;
        private CardStrings _strings;
        private bool _reduceMotion;
        private IVisualElementScheduledItem _settle;

        // One page's cue: a panel the size of a page holding its family icon and words, laid beside the shown page; while a drag is claimed
        // it also lists the page's blocks as compact rows (the arriving page is never blank under the finger)
        private sealed class Cue
        {
            public readonly VisualElement Root;
            public readonly VisualElement Icon;
            public readonly Label Words;
            public readonly NavTint Tint;
            public readonly ElementPool<CompactRowView> Rows;
            // The section this cue names (-1: none, the cue is hidden)
            public int Section = -1;
            private readonly VisualElement _list;

            public Cue(string name, string sideClass)
            {
                Root = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
                Root.AddToClassList("card-pager__peek");
                Root.AddToClassList(sideClass);
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("card-pager__peek-row");
                Icon = CardIcons.Create();
                Icon.pickingMode = PickingMode.Ignore;
                Icon.AddToClassList("card-pager__peek-icon");
                Words = new Label { pickingMode = PickingMode.Ignore };
                Words.AddToClassList("card-pager__peek-words");
                row.Add(Icon);
                row.Add(Words);
                Root.Add(row);
                _list = new VisualElement { pickingMode = PickingMode.Ignore };
                _list.AddToClassList("card-pager__peek-list");
                Root.Add(_list);
                Tint = new NavTint(Root);
                Rows = new ElementPool<CompactRowView>(_ => NewLine(), line => line.Root.RemoveFromHierarchy());
            }

            // A row of the teaser: the card's compact row, drawn only -- no tap, no full-view button
            private CompactRowView NewLine()
            {
                var line = new CompactRowView();
                line.Root.pickingMode = PickingMode.Ignore;
                line.Toggle.pickingMode = PickingMode.Ignore;
                line.Root.AddToClassList("card-pager__peek-line");
                return line;
            }

            // List `targets` as compact rows under the title (the cue's own look moves its title to the top)
            public void ShowRows(System.Collections.Generic.IReadOnlyList<string> targets, System.Func<string, CompactRowRule.Row> rowOf, CardStrings strings)
            {
                ClearRows();
                Root.AddToClassList(TeaserClass);
                foreach (string target in targets)
                {
                    var line = Rows.Take();
                    _list.Add(line.Root);
                    line.Show(rowOf(target), strings, offersFullView: false);
                }
            }

            public void ClearRows()
            {
                Rows.ReleaseAll();
                Root.RemoveFromClassList(TeaserClass);
            }
        }

        public CardPagerView(VisualElement sheetRoot, ScrollView scroll, CardLayoutView layout, NavController nav)
        {
            _sheetRoot = sheetRoot;
            _scroll = scroll;
            _layout = layout;
            _nav = nav;
            _next = new Cue("card-pager-peek", "card-pager__peek--next");
            _back = new Cue("card-pager-peek-back", "card-pager__peek--back");
            // - the cues are the scroll's siblings, overlays laid over its box: the WHOLE scroll slides with the finger (its own content's
            //   translate is how a ScrollView scrolls, so it is not ours to move), and a cue inside it would slide and clip with it. Added after
            //   what the sheet holds so far, never between the stack and the bottom area that closes it in the flow
            var parent = scroll.parent;
            parent.Add(_back.Root);
            parent.Add(_next.Root);
            scroll.RegisterCallback<GeometryChangedEvent>(_ => Place());
            scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => Place());
            _grab = new SwipeGrab(scroll.contentViewport, travel => IsOn && SwipeGrabRule.ClaimsPointer(travel), leavesToParts: true);
            _grab.Claimed += _ =>
            {
                StopSettling();
                ShowTeasers();
            };
            _grab.Moved += travel => DragTo(travel.x);
            _grab.Released += Release;
            _grab.Cancelled += () => SettleTo(0f, -1);
            _nav.CurrentChanged += _ => ShowCues();
            Apply(false);
        }

        // The card's map is (re)built: pages are on when the layout says so (after CardLayoutView.Bind)
        public void Bind(CardNavModel model, BlockBindContext context)
        {
            _model = model;
            _strings = context?.Strings;
            _reduceMotion = context?.ReduceMotion ?? false;
            StopSettling();
            Offset = 0f;
            Apply(CardLayoutRule.TurnsPages(_layout.Effective));
        }

        // The card closed
        public void Unbind()
        {
            StopSettling();
            _model = null;
            _strings = null;
            Offset = 0f;
            Apply(false);
        }

        private void Apply(bool on)
        {
            IsOn = on;
            _sheetRoot.EnableInClassList(PagedClass, on);
            if (!on) _scroll.style.translate = StyleKeyword.Null;
            ShowCues();
            Place();
            // - the inset the class just set is resolved at a later style pass: place again on every panel update until the page width uses it
            //   (a card whose size does not change after this would otherwise keep a peek of 0, found by the gallery)
            if (on) _scroll.schedule.Execute(Place).Every(0).Until(() => !IsOn || UsesTheInset());
        }

        // Whether the last placement read the content's resolved inset (the peek) into the page width
        private bool UsesTheInset()
        {
            float inset = _scroll.contentContainer.resolvedStyle.paddingRight;
            return inset > 0f && Mathf.Abs(_scroll.contentViewport.layout.width - inset - PageWidth) < 0.5f;
        }

        // The cues name the pages beside the one shown: the next one always (while there is one), the previous one for a drag back
        private void ShowCues()
        {
            int page = _layout.ShownSection;
            int count = _model?.Sections.Count ?? 0;
            Fill(_next, IsOn && page >= 0 && page + 1 < count ? page + 1 : -1);
            Fill(_back, IsOn && page > 0 && page - 1 < count ? page - 1 : -1);
            Place();
        }

        // A finger claimed the page: each cue lists its page's blocks as the card's compact rows -- words only, no view bound and no file
        // loaded (the real views bind at the turn, one page at a time: a second bound page would need a second stack and would keep a
        // page left from letting its media go)
        private void ShowTeasers()
        {
            foreach (var cue in new[] { _next, _back })
                if (cue.Section >= 0 && cue.Section < (_model?.Sections.Count ?? 0)) cue.ShowRows(_model.Sections[cue.Section].Targets, _nav.RowOf, _strings);
        }

        private void ClearTeasers()
        {
            _next.ClearRows();
            _back.ClearRows();
        }

        private void Fill(Cue cue, int section)
        {
            cue.ClearRows();
            cue.Section = section;
            cue.Root.style.display = section >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (section < 0) return;
            var s = _model.Sections[section];
            string words = s.LabelKey != null && _strings != null ? _strings.Get(s.LabelKey) : "";
            cue.Words.text = string.IsNullOrEmpty(words) ? s.Key : words;
            CardIcons.SetKey(cue.Icon, s.IconKey);
            cue.Icon.style.display = s.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
            cue.Tint.Clear();
            cue.Tint.Icon(cue.Icon, s.ColorToken);
        }

        // The page and both cues at the current offset: the scroll slides by it, each cue is a page-wide panel beside it, over the scroll's box
        private void Place()
        {
            var box = _scroll.layout;
            float width = _scroll.contentViewport.layout.width;
            if (float.IsNaN(width) || width <= 0f || float.IsNaN(box.width)) return;
            // - the peek is the content's own inset (PoiCard.uss, --ts-page-peek): read back, so the token is the one place it lives
            float peek = IsOn ? _scroll.contentContainer.resolvedStyle.paddingRight : 0f;
            PageWidth = width - (float.IsNaN(peek) ? 0f : peek);
            if (!IsOn) return;
            _scroll.style.translate = new Translate(Offset, 0f);
            foreach (var cue in new[] { _next, _back })
            {
                cue.Root.style.top = box.y;
                cue.Root.style.height = box.height;
                cue.Root.style.width = PageWidth;
            }
            _next.Root.style.left = box.x + PageWidth + Offset;
            _back.Root.style.left = box.x + Offset - PageWidth;
        }

        // A claimed sideways finger moved: the page follows it, with resistance towards a page that does not exist
        private void DragTo(float travelX)
        {
            int page = _layout.ShownSection;
            int count = _model?.Sections.Count ?? 0;
            Offset = SwipePageRule.DragOffset(travelX, page > 0, page >= 0 && page < count - 1);
            Place();
        }

        // The finger let go: turn to the page SwipePageRule lands on, or slide back
        private void Release(Vector2 travel)
        {
            int page = _layout.ShownSection;
            int count = _model?.Sections.Count ?? 0;
            int landing = SwipePageRule.Landing(page, count, SwipePageRule.Direction(travel, PageWidth));
            if (landing < 0 || landing == page) SettleTo(0f, -1);
            else SettleTo(landing > page ? -PageWidth : PageWidth, landing);
        }

        // Slide to `offset` over --ts-duration (at once under reduce motion), then turn to `landing` (-1 = stay)
        private void SettleTo(float offset, int landing)
        {
            StopSettling();
            float seconds = _reduceMotion ? 0f : _nav.Duration;
            if (seconds <= 0f)
            {
                Finish(landing);
                return;
            }
            _sheetRoot.AddToClassList(SettlingClass);
            Offset = offset;
            Place();
            _settle = _scroll.schedule.Execute(() => Finish(landing)).StartingIn((long)(seconds * 1000f));
        }

        // The slide is over: the page turns (the layout shows its section, the stack lands on its first block) and the content is back in place
        private void Finish(int landing)
        {
            _settle = null;
            _sheetRoot.RemoveFromClassList(SettlingClass);
            Offset = 0f;
            // - the real views bind with the turn (the layout shows the section): the teaser has done its job
            ClearTeasers();
            if (landing >= 0) _nav.JumpTo(landing, SwipeKey, landsAtOnce: true);
            Place();
        }

        private void StopSettling()
        {
            _settle?.Pause();
            _settle = null;
            _sheetRoot.RemoveFromClassList(SettlingClass);
        }
    }
}
