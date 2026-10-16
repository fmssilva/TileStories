using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The text_tabs navigation element (_3.2 section 5.1, canvas A1): one word per section of the card in a strip under the header.
    // The section being read is marked twice -- an underline AND a bolder label, never colour alone -- and follows the controller's
    // Current (the scroll-spy); a tap jumps through the controller. A strip wider than the screen scrolls sideways under a finger:
    //   - built as a clipped viewport and a row moved by NavStripRule's offset, not a horizontal ScrollView: Unity's ScrollView takes
    //     a touch that moved 10 px sideways even when it mostly moved up or down, and offers no way to judge it first
    //   - the row's drag is a SwipeGrab (never captured at touch-down, claimed only when clearly sideways), so a tap still reaches the
    //     tab under the finger and an up / down drag still reaches the sheet behind it
    // Views only: the sections, their order and their words come from the CardNavModel.
    public sealed class TextTabsNavView : INavElementView
    {
        public const string CurrentClass = "nav-tabs__tab--current";

        // One tab: the button (its whole area is the tap target), its label and its underline
        public sealed class Tab
        {
            public readonly int Index;
            public readonly Button Button;
            public readonly Label Label;
            public readonly VisualElement Underline;

            public Tab(int index, Button button, Label label, VisualElement underline)
            {
                Index = index;
                Button = button;
                Label = label;
                Underline = underline;
            }
        }

        public VisualElement Root { get; }
        // What clips the strip, and the row of tabs it moves
        public VisualElement Viewport { get; }
        public VisualElement Row { get; }
        // How far the row is moved left now (NavStripRule's offset)
        public float Offset { get; private set; }
        public IReadOnlyList<Tab> Tabs => _tabs.Shown;
        // The index of the marked tab (-1 = none)
        public int CurrentIndex { get; private set; } = -1;

        private readonly ElementPool<Tab> _tabs;
        private readonly SwipeGrab _drag;
        private NavController _nav;
        private float _dragStartOffset;
        // The marked tab still has to be brought into view once the row is laid out
        private bool _showCurrentPending;

        public TextTabsNavView()
        {
            Root = new VisualElement { name = "nav-text-tabs" };
            Root.AddToClassList("nav-tabs");
            Viewport = new VisualElement { name = "nav-text-tabs-viewport" };
            Viewport.AddToClassList("nav-tabs__viewport");
            Row = new VisualElement { name = "nav-text-tabs-row" };
            Row.AddToClassList("nav-tabs__row");
            Viewport.Add(Row);
            Root.Add(Viewport);
            _tabs = new ElementPool<Tab>(NewTab, tab => tab.Button.RemoveFromHierarchy());
            _drag = new SwipeGrab(Viewport, SwipeGrabRule.ClaimsPointer);
            _drag.Claimed += _ => _dragStartOffset = Offset;
            _drag.Moved += travel => SetOffset(_dragStartOffset - travel.x);
            // - a new width (another card's words, a rotated screen) can leave the row past its end, or the marked tab out of view
            Row.RegisterCallback<GeometryChangedEvent>(_ => Relayout());
            Viewport.RegisterCallback<GeometryChangedEvent>(_ => Relayout());
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            var strings = context?.Strings;
            for (int i = 0; i < model.Sections.Count; i++)
            {
                var section = model.Sections[i];
                var tab = _tabs.Take();
                // - the visitor's word for the section (its family's, or its format's), the key only when no table names it
                string words = section.LabelKey != null && strings != null ? strings.Get(section.LabelKey) : "";
                tab.Label.text = string.IsNullOrEmpty(words) ? section.Key : words;
                Row.Add(tab.Button);
            }
            SetOffset(0f);
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _nav = null;
            _tabs.ReleaseAll();
            CurrentIndex = -1;
        }

        private Tab NewTab(int index)
        {
            var button = new Button(() => _nav?.JumpTo(index, BuiltInNavElements.TextTabs)) { name = "nav-text-tab-" + index };
            button.AddToClassList("nav-tabs__tab");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("nav-tabs__label");
            var underline = new VisualElement { pickingMode = PickingMode.Ignore };
            underline.AddToClassList("nav-tabs__underline");
            button.Add(label);
            button.Add(underline);
            return new Tab(index, button, label, underline);
        }

        // The spy moved (a scroll, a jump): mark that tab and bring it into view
        private void OnCurrentChanged(int index)
        {
            CurrentIndex = index;
            foreach (var tab in _tabs.Shown) tab.Button.EnableInClassList(CurrentClass, tab.Index == index);
            _showCurrentPending = true;
            Relayout();
        }

        private void Relayout()
        {
            if (_showCurrentPending && CurrentIndex >= 0 && CurrentIndex < _tabs.Shown.Count)
            {
                var button = _tabs.Shown[CurrentIndex].Button.layout;
                if (float.IsNaN(button.width) || float.IsNaN(VisibleWidth)) return;
                _showCurrentPending = false;
                SetOffset(NavStripRule.OffsetToShow(button.x, button.width, VisibleWidth, Row.layout.width, Offset));
                return;
            }
            SetOffset(Offset);
        }

        // What the row can be seen through: the viewport less its padding (the row starts at the padding, so its end must stop there too)
        private float VisibleWidth => Viewport.contentRect.width;

        private void SetOffset(float offset)
        {
            Offset = NavStripRule.Clamp(offset, Row.layout.width, VisibleWidth);
            Row.style.translate = new Translate(-Offset, 0f);
        }
    }
}
