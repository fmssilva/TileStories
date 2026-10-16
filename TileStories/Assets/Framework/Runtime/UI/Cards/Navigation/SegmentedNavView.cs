using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The segmented navigation element (_3.2 section 5.1, canvas A4, merged L1): the wall's Groups (families merged, rule 2.5.6) as
    // equal segments across the strip that always fit -- no scroll; a long word wraps inside its segment instead of being cut. The segment
    // of the group being read (the controller's CurrentBlock through the model's GroupOf) is filled AND its words are bolder, never colour
    // alone; a tap jumps to the group's first block through the controller. With Show Counts on a segment shows its block count.
    public sealed class SegmentedNavView : INavElementView
    {
        public const string CurrentClass = "nav-segment--current";

        // One segment: its button, words and count, and the group it stands for now
        public sealed class Segment
        {
            public readonly int Index;
            public readonly Button Button;
            public readonly Label Label;
            public readonly Label Count;

            public Segment(int index, Button button, Label label, Label count)
            {
                Index = index;
                Button = button;
                Label = label;
                Count = count;
            }
        }

        public VisualElement Root { get; }
        public VisualElement Track { get; }
        public IReadOnlyList<Segment> Segments => _segments.Shown;
        // The marked segment's index in the model's Groups (-1 = none)
        public int CurrentIndex { get; private set; } = -1;

        private readonly ElementPool<Segment> _segments;
        private NavController _nav;
        private CardNavModel _model;

        public SegmentedNavView()
        {
            Root = new VisualElement { name = "nav-segmented" };
            Root.AddToClassList("nav-segmented");
            Track = new VisualElement { name = "nav-segmented-track" };
            Track.AddToClassList("nav-segmented__track");
            Root.Add(Track);
            _segments = new ElementPool<Segment>(NewSegment, segment => segment.Button.RemoveFromHierarchy());
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _model = model;
            bool counts = context?.Settings?.navigation?.show_counts ?? false;
            foreach (var group in model.Groups)
            {
                var segment = _segments.Take();
                segment.Label.text = NavGroupRule.Words(group.Definition, context?.Strings);
                segment.Button.tooltip = segment.Label.text;
                NavCount.Set(segment.Count, counts, group.Count);
                Track.Add(segment.Button);
            }
            nav.CurrentBlockChanged += OnCurrentBlockChanged;
            OnCurrentBlockChanged(nav.CurrentBlock);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= OnCurrentBlockChanged;
            _nav = null;
            _model = null;
            _segments.ReleaseAll();
            CurrentIndex = -1;
        }

        private Segment NewSegment(int index)
        {
            var button = new Button(() =>
            {
                if (_model != null && index < _model.Groups.Count) _nav?.JumpToBlock(_model.Groups[index].Targets[0], BuiltInNavElements.Segmented);
            }) { name = "nav-segment-" + index };
            button.AddToClassList("nav-segment");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("nav-segment__label");
            var count = NavCount.Create();
            button.Add(label);
            button.Add(count);
            return new Segment(index, button, label, count);
        }

        // The block being read moved: mark its group's segment (the first one before any block is read)
        private void OnCurrentBlockChanged(string block)
        {
            int group = _model?.GroupOf(block) ?? -1;
            if (group < 0 && block == null && _segments.Shown.Count > 0) group = 0;
            CurrentIndex = group;
            foreach (var segment in _segments.Shown) segment.Button.EnableInClassList(CurrentClass, segment.Index == group);
        }
    }
}
