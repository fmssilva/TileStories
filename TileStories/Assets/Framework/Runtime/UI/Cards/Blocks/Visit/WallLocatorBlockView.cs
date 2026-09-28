using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The wall_locator block (_3.1 Tier 2 group B): where this POI is along its wall, from the wall's POI positions
    // (WallAxisRule: nothing authored). The POIs are the running wall's (BlockBindContext.Taxonomy.pois).
    //   strip      -- the wall as a line: a dot per POI at its place along the wall, this POI's dot marked and titled,
    //                 and the visitor's own place ("You are here", CardStrings) while the card's host knows it
    //                 (IBlockHost.TryGetViewer: the camera in the app); it follows the visitor while the block is shown
    //   neighbours -- the nearest POI on each side along the wall, each a button with its card title; a tap selects that
    //                 POI through the card's host (the selection bus: the card rebinds, zoom-on-select as a marker tap)
    // Only classes and positions here; Visit.uss draws it.
    public sealed class WallLocatorBlockView : IBlockView
    {
        // How often the "You are here" mark follows the visitor (milliseconds)
        public const long ViewerRefreshMs = 250;

        public VisualElement Root { get; }
        public VisualElement Strip { get; }
        public VisualElement Track { get; }
        public VisualElement Self { get; }
        public Label SelfTitle { get; }
        public VisualElement You { get; }
        // The legend under the strip: this POI's mark + its title, the visitor's mark + "You are here"
        public VisualElement YouLegend { get; }
        public Label YouLabel { get; }
        public IReadOnlyList<VisualElement> Dots => _dots;
        public VisualElement NeighbourRow { get; }
        public Button Left { get; }
        public Label LeftTitle { get; }
        public Button Right { get; }
        public Label RightTitle { get; }
        public string LeftPoiId { get; private set; }
        public string RightPoiId { get; private set; }
        // This POI's place on the strip and the visitor's (0 = the wall's left end, 1 = its right end; -1 = not shown)
        public float SelfShare { get; private set; } = -1f;
        public float YouShare { get; private set; } = -1f;

        private readonly List<VisualElement> _dots = new();
        private readonly List<VisualElement> _dotPool = new();
        private WallPlaces _places;
        private float _min, _max;
        private IBlockHost _host;
        private string _variantClass;
        private IVisualElementScheduledItem _follow;

        public WallLocatorBlockView()
        {
            Root = new VisualElement { name = "card-locator" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-locator");

            Strip = new VisualElement();
            Strip.AddToClassList("card-locator__strip");
            Track = new VisualElement { pickingMode = PickingMode.Ignore };
            Track.AddToClassList("card-locator__track");
            var line = new VisualElement { pickingMode = PickingMode.Ignore };
            line.AddToClassList("card-locator__line");
            Track.Add(line);
            Self = new VisualElement { pickingMode = PickingMode.Ignore };
            Self.AddToClassList("card-locator__dot");
            Self.AddToClassList("card-locator__dot--self");
            You = new VisualElement { pickingMode = PickingMode.Ignore };
            You.AddToClassList("card-locator__you");
            Track.Add(You);
            Track.Add(Self);
            var labels = new VisualElement { pickingMode = PickingMode.Ignore };
            labels.AddToClassList("card-locator__legend");
            SelfTitle = new Label { pickingMode = PickingMode.Ignore };
            SelfTitle.AddToClassList("card-locator__legend-text");
            YouLabel = new Label { pickingMode = PickingMode.Ignore };
            YouLabel.AddToClassList("card-locator__legend-text");
            labels.Add(LegendItem("card-locator__dot--self", SelfTitle));
            YouLegend = LegendItem("card-locator__you", YouLabel);
            labels.Add(YouLegend);
            Strip.Add(Track);
            Strip.Add(labels);

            NeighbourRow = new VisualElement();
            NeighbourRow.AddToClassList("card-locator__neighbours");
            (Left, LeftTitle) = NeighbourButton("card-locator__neighbour--left", () => Pick(LeftPoiId));
            (Right, RightTitle) = NeighbourButton("card-locator__neighbour--right", () => Pick(RightPoiId));
            NeighbourRow.Add(Left);
            NeighbourRow.Add(Right);

            Root.Add(Strip);
            Root.Add(NeighbourRow);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _host = context.Host;
            _variantClass = "card-locator--" + context.Variant;
            Root.AddToClassList(_variantClass);
            _places = WallAxisRule.Places(context.Taxonomy?.pois);
            string selfId = context.Poi?.id;
            bool neighbours = context.Variant == BuiltInBlocks.WallLocatorNeighbours;
            Strip.style.display = neighbours ? DisplayStyle.None : DisplayStyle.Flex;
            NeighbourRow.style.display = neighbours ? DisplayStyle.Flex : DisplayStyle.None;
            if (neighbours) BindNeighbours(selfId, context);
            else BindStrip(selfId, context);
        }

        public void Unbind()
        {
            _follow?.Pause();
            _follow = null;
            foreach (var dot in _dots)
            {
                dot.RemoveFromHierarchy();
                _dotPool.Add(dot);
            }
            _dots.Clear();
            _places = null;
            _host = null;
            LeftPoiId = RightPoiId = null;
            SelfShare = YouShare = -1f;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        private void BindStrip(string selfId, BlockBindContext context)
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            foreach (float a in _places.Along)
            {
                _min = Mathf.Min(_min, a);
                _max = Mathf.Max(_max, a);
            }
            for (int i = 0; i < _places.Pois.Count; i++)
            {
                if (_places.Ids[i] == selfId) continue;
                var dot = _dotPool.Count > 0 ? _dotPool[_dotPool.Count - 1] : NewDot();
                if (_dotPool.Count > 0) _dotPool.RemoveAt(_dotPool.Count - 1);
                dot.style.left = Length.Percent(ShareOf(_places.Along[i]) * 100f);
                // - behind this POI's own dot and the visitor's mark
                Track.Insert(0, dot);
                _dots.Add(dot);
            }
            int self = _places.IndexOf(selfId);
            SelfShare = self >= 0 ? ShareOf(_places.Along[self]) : -1f;
            Self.style.display = self >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            Self.style.left = Length.Percent(Mathf.Max(0f, SelfShare) * 100f);
            SelfTitle.text = BlockStackBuilder.CardTitleOf(context.Poi, context.Language, context.FallbackLanguage);
            YouLabel.text = context.Strings?.Get(CardStrings.Keys.WallLocatorYou) ?? "";
            PlaceViewer();
            // - the visitor walks: the mark follows while the block is shown
            _follow = Root.schedule.Execute(PlaceViewer).Every(ViewerRefreshMs);
        }

        // The visitor's mark at their place along the wall (clamped to its ends), hidden while no viewer is known
        private void PlaceViewer()
        {
            if (_places == null || _host == null || !_host.TryGetViewer(out var viewer))
            {
                YouShare = -1f;
                You.style.display = DisplayStyle.None;
                YouLegend.style.display = DisplayStyle.None;
                return;
            }
            YouShare = Mathf.Clamp01(ShareOf(_places.Axis.Along(viewer)));
            You.style.display = DisplayStyle.Flex;
            YouLegend.style.display = DisplayStyle.Flex;
            You.style.left = Length.Percent(YouShare * 100f);
        }

        // A place along the wall as a share of the strip (0 = the left-most POI, 1 = the right-most; the middle when all
        // POIs stand at one place)
        private float ShareOf(float along) => _max - _min > 1e-4f ? (along - _min) / (_max - _min) : 0.5f;

        private void BindNeighbours(string selfId, BlockBindContext context)
        {
            var (left, right) = _places.NeighboursOf(selfId);
            LeftPoiId = left?.id;
            RightPoiId = right?.id;
            LeftTitle.text = BlockStackBuilder.CardTitleOf(left, context.Language, context.FallbackLanguage);
            RightTitle.text = BlockStackBuilder.CardTitleOf(right, context.Language, context.FallbackLanguage);
            // - visibility, not display: a missing side keeps its half, so the other stays on its own side
            Left.style.visibility = left != null ? Visibility.Visible : Visibility.Hidden;
            Right.style.visibility = right != null ? Visibility.Visible : Visibility.Hidden;
            Left.tooltip = LeftTitle.text;
            Right.tooltip = RightTitle.text;
        }

        private void Pick(string poiId)
        {
            if (!string.IsNullOrEmpty(poiId)) _host?.SelectPoi(poiId);
        }

        // One legend entry: a small copy of a mark, then its words
        private static VisualElement LegendItem(string markClass, Label text)
        {
            var item = new VisualElement { pickingMode = PickingMode.Ignore };
            item.AddToClassList("card-locator__legend-item");
            var mark = new VisualElement { pickingMode = PickingMode.Ignore };
            mark.AddToClassList("card-locator__legend-mark");
            mark.AddToClassList(markClass);
            item.Add(mark);
            item.Add(text);
            return item;
        }

        private static VisualElement NewDot()
        {
            var dot = new VisualElement { pickingMode = PickingMode.Ignore };
            dot.AddToClassList("card-locator__dot");
            return dot;
        }

        // One neighbour button: a chevron pointing its way and the POI's title
        private static (Button, Label) NeighbourButton(string sideClass, System.Action clicked)
        {
            var button = new Button(clicked);
            button.AddToClassList("card-locator__neighbour");
            button.AddToClassList(sideClass);
            button.AddToClassList("card-tap");
            var chevron = new VisualElement { pickingMode = PickingMode.Ignore };
            chevron.AddToClassList("card-locator__chevron");
            var title = new Label { pickingMode = PickingMode.Ignore };
            title.AddToClassList("card-locator__neighbour-title");
            button.Add(chevron);
            button.Add(title);
            return (button, title);
        }
    }
}
