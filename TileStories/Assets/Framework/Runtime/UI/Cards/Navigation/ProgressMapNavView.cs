using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The progress_map navigation element (_3.2 section 5.1, canvas E2, an L1 map in almost no space): a thin bar split into one segment
    // per section, each as wide as its share of the card's targets (ProgressMapRule) but never narrower than a whole tap target. Each
    // segment wears its family's icon above its stretch of bar, both in the family's colour -- the icon names it, never the colour alone;
    // a format section (no icon) shows its words. The bar reads as progress: the sections up to the one being read are solid, the rest
    // faint, and the current one's bar is thicker and its icon / words bolder. A tap jumps to the section's first block; when the minimums
    // are wider than the card the bar scrolls sideways (NavStrip).
    public sealed class ProgressMapNavView : INavElementView
    {
        public const string CurrentClass = "nav-progress__segment--current";
        public const string PassedClass = "nav-progress__segment--passed";

        // One segment: its button, its icon (or its words), and its stretch of the bar
        public sealed class Segment
        {
            public readonly int Index;
            public readonly Button Button;
            public readonly VisualElement Icon;
            public readonly Label Label;
            public readonly VisualElement Bar;

            public Segment(int index, Button button, VisualElement icon, Label label, VisualElement bar)
            {
                Index = index;
                Button = button;
                Icon = icon;
                Label = label;
                Bar = bar;
            }
        }

        public VisualElement Root { get; }
        public NavStrip Strip { get; }
        public IReadOnlyList<Segment> Segments => _segments.Shown;
        public int CurrentIndex { get; private set; } = -1;
        // The widths ProgressMapRule gave the segments at the last layout (what a test checks the drawn ones against)
        public IReadOnlyList<float> Widths => _widths;

        private readonly ElementPool<Segment> _segments;
        private readonly NavTint _tint;
        private NavController _nav;
        private CardNavModel _model;
        private float[] _widths = new float[0];
        private float _sizedFor = float.NaN;

        public ProgressMapNavView()
        {
            Root = new VisualElement { name = "nav-progress" };
            Root.AddToClassList("nav-progress");
            Strip = new NavStrip("nav-progress", "nav-progress__viewport", "nav-progress__row");
            Root.Add(Strip.Viewport);
            _segments = new ElementPool<Segment>(NewSegment, segment => segment.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
            // - the widths depend on the strip's width and each segment's minimum: re-share whenever either changes
            Strip.Viewport.RegisterCallback<GeometryChangedEvent>(_ => Resize());
            Strip.Row.RegisterCallback<GeometryChangedEvent>(_ => Resize());
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _model = model;
            var strings = context?.Strings;
            foreach (var section in model.Sections)
            {
                var segment = _segments.Take();
                string words = section.LabelKey != null && strings != null ? strings.Get(section.LabelKey) : "";
                segment.Label.text = string.IsNullOrEmpty(words) ? section.Key : words;
                segment.Button.tooltip = segment.Label.text;
                bool hasIcon = section.IconKey != null;
                CardIcons.SetKey(segment.Icon, section.IconKey);
                segment.Icon.style.display = hasIcon ? DisplayStyle.Flex : DisplayStyle.None;
                segment.Label.style.display = hasIcon ? DisplayStyle.None : DisplayStyle.Flex;
                _tint.Icon(segment.Icon, section.ColorToken);
                _tint.Fill(segment.Bar, section.ColorToken);
                Strip.Row.Add(segment.Button);
            }
            _sizedFor = float.NaN;
            Strip.Reset();
            Resize();
            nav.CurrentChanged += OnCurrentChanged;
            OnCurrentChanged(nav.Current);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentChanged -= OnCurrentChanged;
            _nav = null;
            _model = null;
            _segments.ReleaseAll();
            _tint.Clear();
            _widths = new float[0];
            CurrentIndex = -1;
        }

        private Segment NewSegment(int index)
        {
            var button = new Button(() => _nav?.JumpTo(index, BuiltInNavElements.ProgressMap)) { name = "nav-progress-segment-" + index };
            button.AddToClassList("nav-progress__segment");
            var icon = CardIcons.Create();
            icon.AddToClassList("nav-progress__icon");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("nav-progress__label");
            var bar = new VisualElement { pickingMode = PickingMode.Ignore };
            bar.AddToClassList("nav-progress__bar");
            button.Add(icon);
            button.Add(label);
            button.Add(bar);
            return new Segment(index, button, icon, label, bar);
        }

        // Share the strip among the segments: by target count, each at least its own minimum (the touch token from the stylesheet's
        // min-width, or its words' width for a segment with no icon)
        private void Resize()
        {
            if (_model == null) return;
            float available = Strip.VisibleWidth;
            if (float.IsNaN(available) || available <= 0f || available.Equals(_sizedFor)) return;
            var shown = _segments.Shown;
            var counts = new int[shown.Count];
            var minimums = new float[shown.Count];
            for (int i = 0; i < shown.Count; i++)
            {
                var segment = shown[i];
                counts[i] = _model.Sections[i].Count;
                float min = segment.Button.resolvedStyle.minWidth.value;
                if (segment.Label.resolvedStyle.display == DisplayStyle.Flex)
                {
                    var words = segment.Label.MeasureTextSize(segment.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    float padding = segment.Button.resolvedStyle.paddingLeft + segment.Button.resolvedStyle.paddingRight;
                    if (!float.IsNaN(words.x) && words.x + padding > min) min = words.x + padding;
                }
                // - a segment whose style is not resolved yet has no minimum to give: share when it has (the row's next layout)
                if (float.IsNaN(min) || min <= 0f) return;
                minimums[i] = min;
            }
            _widths = ProgressMapRule.Widths(counts, minimums, available);
            for (int i = 0; i < shown.Count; i++) shown[i].Button.style.width = _widths[i];
            _sizedFor = available;
        }

        // The spy moved: the sections up to it are passed, it is current; bring it into view
        private void OnCurrentChanged(int index)
        {
            CurrentIndex = index;
            foreach (var segment in _segments.Shown)
            {
                segment.Button.EnableInClassList(CurrentClass, segment.Index == index);
                segment.Button.EnableInClassList(PassedClass, segment.Index <= index);
            }
            if (index >= 0 && index < _segments.Shown.Count) Strip.Show(_segments.Shown[index].Button);
        }
    }
}
