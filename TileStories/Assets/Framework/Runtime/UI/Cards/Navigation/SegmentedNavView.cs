using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The segmented navigation element (_3.2 section 5.1, canvas A4, merged L1): the wall's Groups (families merged, rule 2.5.6) as
    // equal segments across the strip that always fit -- no scroll; a long word wraps inside its segment instead of being cut. The segment
    // of the group being read (the controller's CurrentBlock through the model's GroupOf) is filled AND its words are bolder, never colour
    // alone; a tap jumps to the group's first block through the controller. With Show Counts on a segment shows its block count -- unless a
    // word and its count cannot sit side by side in a segment: then NO segment shows a count (NavFitRule: the count goes, never the word).
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
        // Whether the counts are drawn now: Show Counts is on AND every word holds its count beside it (the strip decides once for all)
        public bool CountsShown { get; private set; }
        // The marked segment's index in the model's Groups (-1 = none)
        public int CurrentIndex { get; private set; } = -1;

        private readonly ElementPool<Segment> _segments;
        // A hidden bold copy of a segment's words: the width a word and its count take once its segment is the marked one (the widest they get)
        private readonly Label _ruler;
        private bool _countsWanted;
        private NavController _nav;
        private CardNavModel _model;

        public SegmentedNavView()
        {
            Root = new VisualElement { name = "nav-segmented" };
            Root.AddToClassList("nav-segmented");
            Track = new VisualElement { name = "nav-segmented-track" };
            Track.AddToClassList("nav-segmented__track");
            Root.Add(Track);
            _ruler = new Label { name = "nav-segmented-ruler", pickingMode = PickingMode.Ignore };
            _ruler.AddToClassList("nav-segmented__ruler");
            Root.Add(_ruler);
            Track.RegisterCallback<GeometryChangedEvent>(_ => FitCounts());
            _segments = new ElementPool<Segment>(NewSegment, segment => segment.Button.RemoveFromHierarchy());
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _model = model;
            bool counts = context?.Settings?.navigation?.show_counts ?? false;
            _countsWanted = counts;
            CountsShown = counts;
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
            FitCounts();
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= OnCurrentBlockChanged;
            _nav = null;
            _model = null;
            _segments.ReleaseAll();
            CurrentIndex = -1;
            _countsWanted = false;
            CountsShown = false;
        }

        // Show Counts is on: draw every count only when each word holds its count beside it inside its segment, else none (NavFitRule).
        // The segments share the track in equal parts whatever they hold, so hiding a count never moves the room: no layout loop
        private void FitCounts()
        {
            if (!_countsWanted || _segments.Shown.Count == 0) return;
            var shown = _segments.Shown;
            var first = shown[0];
            float room = first.Button.resolvedStyle.width - first.Button.resolvedStyle.paddingLeft - first.Button.resolvedStyle.paddingRight;
            float gap = first.Count.resolvedStyle.marginLeft;
            if (float.IsNaN(room) || room <= 0f || float.IsNaN(gap)) return;
            var words = new float[shown.Count];
            var counts = new float[shown.Count];
            for (int i = 0; i < shown.Count; i++)
            {
                words[i] = Natural(shown[i].Label.text);
                counts[i] = Natural(shown[i].Count.text);
                if (float.IsNaN(words[i]) || float.IsNaN(counts[i])) return;
            }
            bool fits = NavFitRule.CountsFit(words, counts, gap, room);
            if (fits == CountsShown) return;
            CountsShown = fits;
            foreach (var segment in shown) segment.Count.style.display = fits ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // A text's width on one line, in the bold the marked segment wears
        private float Natural(string text)
        {
            _ruler.text = text;
            return _ruler.MeasureTextSize(text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;
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
