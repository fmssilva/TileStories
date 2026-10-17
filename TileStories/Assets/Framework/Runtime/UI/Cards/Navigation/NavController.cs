using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The ONE driver of moving inside the open card (_3.2 sections 4.1 and 4.4): every navigation element (tabs, bars, a menu, a
    // block asking through IBlockHost.ScrollToBlock) jumps through it, and every element marks the section it names (the
    // scroll-spy). The sheet owns one, next to its stack, and binds it to each card's CardNavModel; it owns no element and no
    // element owns it. The arithmetic is NavScrollRule's; this class only applies it to the stack's ScrollView:
    //   - a jump raises the sheet to Full first (Jump Opens Card To), then sets the stack's scroll offset so the target block's slot
    //     top meets the viewport's top (clamped to the range) -- at once (instant / reduce motion) or gliding over --ts-duration on
    //     the panel's scheduler, re-reading the landing every frame (the sheet may still be growing to Full)
    //   - while a jump glides, Current is frozen on the jump's section, then released to the scroll's answer
    //   - each jump raises one nav_jump CardEvent through the card's ICardEvents
    // Never ScrollView.ScrollTo: its docs do not say where the child lands (_3.2 references).
    public sealed class NavController
    {
        private readonly BlockStackView _stack;
        private readonly Action _raiseToFull;
        private readonly List<float> _sectionTops = new();
        private readonly List<float> _targetTops = new();
        private BlockBindContext _context;
        private string _currentKey;

        // The running jump (null block = none): where it goes, the section and the target it freezes the spy on, where and when it started
        private string _jumpBlock;
        private int _jumpSection = -1;
        private int _jumpTarget = -1;
        private float _jumpFrom;
        private float _jumpStartTime;
        private float _jumpDuration;
        private float _lastLanding = float.NaN;
        private IVisualElementScheduledItem _glide;

        // The map of the card bound now (null while the card is closed)
        public CardNavModel Model { get; private set; }

        // The index in Model.Sections of the section the reader is in (-1 with no section); CurrentChanged says when it moves
        public int Current { get; private set; } = -1;
        public CardNavModel.Section CurrentSection => Model != null && Current >= 0 && Current < Model.Sections.Count ? Model.Sections[Current] : null;
        public event Action<int> CurrentChanged;

        // The target block the reader is at (a key of Model.Targets; null with none): the same spy line over every target's top, frozen
        // on the jump's block while a jump glides. What an element that marks a KIND or a GROUP follows (kind chips, segmented): the
        // model says which kind and group a target is in. CurrentBlockChanged says when it moves
        public string CurrentBlock { get; private set; }
        public event Action<string> CurrentBlockChanged;

        // Whether a jump is moving the stack now (the spy is frozen meanwhile)
        public bool IsJumping => _jumpBlock != null;

        // Raised as every jump starts, whoever asked for it: what closes an open menu or list over the card (CardMenuOverlay)
        public event Action Jumped;

        // The words the stack draws above a block ("" when it draws none): what an element that names a block says (Up next)
        public string HeadingOf(string blockKey) => _stack.HeadingOfBlock(blockKey);

        // How long a smooth jump glides, in seconds: --ts-duration, handed over by the sheet once the card's tokens resolve
        public float Duration { get; internal set; }

        // The glide's clock: Time.unscaledTime in the app. A test sets its own, so a glide is exactly as far along as the test says
        internal Func<float> Clock { get; set; } = () => Time.unscaledTime;

        private ScrollView Scroll => _stack.Scroll;
        private CardNavigationSettings Settings => _context?.Settings?.navigation;

        public NavController(BlockStackView stack, Action raiseToFull)
        {
            _stack = stack;
            _raiseToFull = raiseToFull;
            // - the spy follows every scroll, and every size change of the content or the visible part (a sheet raised to another stop,
            //   a picture loading) that moves what sits under the spy line without a scroll event
            Scroll.verticalScroller.valueChanged += _ => UpdateSpy();
            Scroll.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => UpdateSpy());
            Scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => UpdateSpy());
        }

        // Drive this card: its model (rebuilt on every Show and reveal) and the context it was bound with (its settings, reduce motion,
        // where events go). A new binding (every Show makes a new context) ends a running jump; a reveal re-binds the SAME context and
        // keeps it -- a jump to the end reveals the blocks that wait for it, the card grows, and the jump must follow to the new end.
        // On the same point (a live edit, a language switch, a reveal) the section the reader was in is kept where the new model still
        // has it; another point starts at its first
        public void Bind(CardNavModel model, BlockBindContext context)
        {
            if (!ReferenceEquals(context, _context)) EndJump();
            bool samePoint = context?.Poi?.id == _context?.Poi?.id;
            string kept = samePoint ? _currentKey : null;
            string keptBlockKey = samePoint ? CurrentBlock : null;
            Model = model;
            _context = context;
            var jumpSlot = IsJumping ? ScrollingSlot(_jumpBlock) : null;
            if (jumpSlot != null)
            {
                // - the same jump goes on from where the stack is now to its new landing, its section found again in the rebuilt
                //   model (indices may have moved)
                int section = SectionOfBlock(_jumpBlock);
                _jumpSection = section >= 0 ? section : SectionAtOffset(Landing(jumpSlot));
                _jumpTarget = TargetFrozenFor(_jumpBlock, jumpSlot);
                _jumpFrom = Scroll.scrollOffset.y;
                _jumpStartTime = Clock();
                _lastLanding = float.NaN;
                SetCurrent(_jumpSection);
                SetCurrentBlock(_jumpTarget);
                return;
            }
            EndJump();
            int keptIndex = IndexOfSection(kept);
            int start = keptIndex >= 0 ? keptIndex : Model.Sections.Count > 0 ? 0 : -1;
            bool follows = NavScrollRule.FollowsScroll(Settings);
            SetCurrent(NavScrollRule.Spy(follows, -1, MeasuredSection(), start));
            int keptBlock = keptBlockKey != null ? IndexOfTarget(keptBlockKey) : -1;
            SetCurrentBlock(NavScrollRule.Spy(follows, -1, MeasuredTarget(), keptBlock >= 0 ? keptBlock : Model.Targets.Count > 0 ? 0 : -1));
        }

        // The card closed: nothing to drive
        public void Unbind()
        {
            EndJump();
            Model = null;
            _context = null;
            SetCurrent(-1);
            SetCurrentBlock(-1);
        }

        // Jump to a section (its index in Model.Sections) as `elementKey` asked: to the section's first target. False when there is none
        public bool JumpTo(int sectionIndex, string elementKey)
        {
            if (Model == null || sectionIndex < 0 || sectionIndex >= Model.Sections.Count) return false;
            var section = Model.Sections[sectionIndex];
            return Jump(section.Targets[0], sectionIndex, section.Key, elementKey);
        }

        // Jump to one block of this card (its instance key) as `elementKey` asked. A block that is no navigation target (sources) can
        // still be jumped to while it sits in the scrolling stack; false when it does not (hidden, pinned, in the footer, not here)
        public bool JumpToBlock(string blockKey, string elementKey)
        {
            if (Model == null) return false;
            int section = SectionOfBlock(blockKey);
            return Jump(blockKey, section, section >= 0 ? Model.Sections[section].Key : "", elementKey);
        }

        private bool Jump(string blockKey, int sectionIndex, string sectionKey, string elementKey)
        {
            if (ScrollingSlot(blockKey) == null) return false;
            EndJump();
            if (NavScrollRule.RaisesToFull(Settings)) _raiseToFull?.Invoke();
            _jumpBlock = blockKey;
            _jumpFrom = Scroll.scrollOffset.y;
            _jumpStartTime = Clock();
            _jumpDuration = NavScrollRule.IsInstant(Settings, _context?.ReduceMotion ?? false) ? 0f : Duration;
            _lastLanding = float.NaN;
            // - frozen on the jump's section; a block of no section freezes on what the spy will name where it lands
            _jumpSection = sectionIndex >= 0 ? sectionIndex : SectionAtOffset(Landing(ScrollingSlot(blockKey)));
            _jumpTarget = TargetFrozenFor(blockKey, ScrollingSlot(blockKey));
            SetCurrent(_jumpSection);
            SetCurrentBlock(_jumpTarget);
            Report(blockKey, sectionKey, elementKey);
            Jumped?.Invoke();
            // - the first step runs now (an instant jump is done by it); a glide goes on every panel update until it lands
            Step();
            if (IsJumping) _glide = Scroll.schedule.Execute(Step).Every(0);
            return true;
        }

        // One frame of the running jump: the landing re-read (the sheet may still grow), the offset eased towards it, released once landed
        private void Step()
        {
            if (!IsJumping) return;
            var slot = ScrollingSlot(_jumpBlock);
            if (slot == null)
            {
                EndJump();
                return;
            }
            float landing = Landing(slot);
            float elapsed = Clock() - _jumpStartTime;
            SetOffset(NavScrollRule.Glide(_jumpFrom, landing, NavScrollRule.Progress(elapsed, _jumpDuration)));
            // - landed: the landing held still for a frame AND the stack really sits on it (the ScrollView clamps an offset its
            //   current range cannot reach -- at peek, before the raise has grown the sheet, that is 0)
            bool landed = landing.Equals(_lastLanding) && Mathf.Abs(Scroll.scrollOffset.y - landing) <= NavScrollRule.LandingTolerance;
            _lastLanding = landing;
            if (NavScrollRule.JumpDone(elapsed, _jumpDuration, Duration, landed)) EndJump();
        }

        // Stop a running jump and release the spy to the scroll's answer
        private void EndJump()
        {
            if (!IsJumping) return;
            _jumpBlock = null;
            _jumpSection = -1;
            _jumpTarget = -1;
            _glide?.Pause();
            _glide = null;
            UpdateSpy();
        }

        // Where the stack must rest for this slot's top to meet the viewport's top
        private float Landing(VisualElement slot) =>
            NavScrollRule.TargetOffset(slot.layout.y, Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private void SetOffset(float y) => Scroll.scrollOffset = new Vector2(Scroll.scrollOffset.x, y);

        private void UpdateSpy()
        {
            if (Model == null) return;
            bool follows = NavScrollRule.FollowsScroll(Settings);
            SetCurrent(NavScrollRule.Spy(follows, _jumpSection, MeasuredSection(), Current));
            SetCurrentBlock(NavScrollRule.Spy(follows, _jumpTarget, MeasuredTarget(), IndexOfTarget(CurrentBlock)));
        }

        // The section the scroll shows now (NavScrollRule's spy line)
        private int MeasuredSection() => SectionAtOffset(Scroll.scrollOffset.y);

        // The target the scroll shows now: the same spy line over every target's own top
        private int MeasuredTarget() => TargetAtOffset(Scroll.scrollOffset.y);

        private int TargetAtOffset(float offset)
        {
            _targetTops.Clear();
            foreach (string target in Model.Targets)
            {
                float top = ScrollingSlot(target)?.layout.y ?? float.NaN;
                _targetTops.Add(float.IsNaN(top) ? float.PositiveInfinity : top);
            }
            return NavScrollRule.CurrentSection(_targetTops, offset, Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
        }

        // The target a jump freezes the block spy on: the block itself, or -- for a block that is no target (sources) -- the one the spy
        // will name where it lands
        private int TargetFrozenFor(string blockKey, VisualElement slot)
        {
            int index = IndexOfTarget(blockKey);
            return index >= 0 || slot == null ? index : TargetAtOffset(Landing(slot));
        }

        private int IndexOfTarget(string blockKey)
        {
            if (blockKey == null || Model == null) return -1;
            for (int i = 0; i < Model.Targets.Count; i++)
                if (Model.Targets[i] == blockKey) return i;
            return -1;
        }

        private void SetCurrentBlock(int index)
        {
            string key = Model != null && index >= 0 && index < Model.Targets.Count ? Model.Targets[index] : null;
            if (key == CurrentBlock) return;
            CurrentBlock = key;
            CurrentBlockChanged?.Invoke(key);
        }

        private int SectionAtOffset(float offset)
        {
            _sectionTops.Clear();
            foreach (var section in Model.Sections)
            {
                // - a first target not laid out yet has not reached any line
                float top = ScrollingSlot(section.Targets[0])?.layout.y ?? float.NaN;
                _sectionTops.Add(float.IsNaN(top) ? float.PositiveInfinity : top);
            }
            return NavScrollRule.CurrentSection(_sectionTops, offset, Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
        }

        // The slot of a block that sits shown in the scrolling stack, else null (the stack hides a waiting block inline)
        private VisualElement ScrollingSlot(string blockKey)
        {
            var slot = _stack.SlotOfBlock(blockKey);
            return slot != null && slot.parent == Scroll.contentContainer && slot.style.display.value != DisplayStyle.None ? slot : null;
        }

        private int SectionOfBlock(string blockKey)
        {
            for (int i = 0; i < Model.Sections.Count; i++)
                foreach (string target in Model.Sections[i].Targets)
                    if (target == blockKey) return i;
            return -1;
        }

        private int IndexOfSection(string key)
        {
            if (key == null || Model == null) return -1;
            for (int i = 0; i < Model.Sections.Count; i++)
                if (Model.Sections[i].Key == key) return i;
            return -1;
        }

        private void SetCurrent(int index)
        {
            string key = Model != null && index >= 0 && index < Model.Sections.Count ? Model.Sections[index].Key : null;
            if (index == Current && key == _currentKey) return;
            Current = index;
            _currentKey = key;
            CurrentChanged?.Invoke(index);
        }

        // One nav_jump event per jump: which element the visitor used, to which section and block
        private void Report(string blockKey, string sectionKey, string elementKey)
        {
            string wallId = _context?.State?.WallId ?? _context?.Taxonomy?.wall_id ?? "";
            _context?.Events?.Raise(new CardEvent(CardEventKinds.NavJump, wallId, _context.Poi?.id, blockKey, elementKey ?? "", sectionKey));
        }
    }
}
