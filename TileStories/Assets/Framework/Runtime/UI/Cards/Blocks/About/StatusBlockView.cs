using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The status block (_3.1 Tier 1, step 5b): the POI's condition drawn the way its MARKER draws it -- the same ring
    // picture (the line style of its Outline Types row, from the marker's own Rings) in the same colour, both resolved
    // by CardStatusRule through the markers' rule; the card's --ts-status tokens only when the wall draws no ring.
    // ring: one ring beside the heading and the condition's name; scale: the name, then every known Outline Types row
    // in order of damage as a small ring, this POI's row marked. An unknown condition shows the "?" ring and no current
    // step. A POI without a status never gets here (BuiltInBlocks.Status.ShowsFor). The label field overrides the name.
    public sealed class StatusBlockView : IBlockView
    {
        public sealed class Step
        {
            public VisualElement Box;
            public VisualElement Ring;
            public Label Name;
            public string Key;
        }

        public VisualElement Root { get; }
        public VisualElement Ring { get; }
        public Label UnknownMark { get; }
        public Label Heading { get; }
        public Label Level { get; }
        public VisualElement ScaleRow { get; }
        public IReadOnlyList<Step> Steps => _steps;
        // What the card resolved (for the tests and the scale)
        public CardStatusRule.Status Status { get; private set; }

        private static readonly string[] LineStyles = { "solid", "dash_long", "dash_medium", "dash_short", "dotted" };
        private readonly List<Step> _pool = new();
        private readonly List<Step> _steps = new();
        private string _variantClass;

        public StatusBlockView()
        {
            Root = new VisualElement { name = "card-status" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-status");
            var main = new VisualElement();
            main.AddToClassList("card-status__main");
            Ring = new VisualElement { name = "card-status-ring" };
            Ring.AddToClassList("card-status__ring");
            UnknownMark = new Label();
            UnknownMark.AddToClassList("card-status__unknown-mark");
            Ring.Add(UnknownMark);
            var words = new VisualElement();
            words.AddToClassList("card-status__words");
            Heading = new Label();
            Heading.AddToClassList("card-status__heading");
            Level = new Label();
            Level.AddToClassList("card-status__level");
            words.Add(Heading);
            words.Add(Level);
            main.Add(Ring);
            main.Add(words);
            Root.Add(main);
            ScaleRow = new VisualElement();
            ScaleRow.AddToClassList("card-status__scale");
            Root.Add(ScaleRow);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-status--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var status = CardStatusRule.Resolve(context.Poi, context.MarkerLook, context.Taxonomy);
            Status = status;
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);

            Heading.text = context.Strings?.Get(CardStrings.Keys.StatusHeading) ?? "";
            string label = read.Text(BuiltInBlocks.StatusLabelField);
            Level.text = label.Length > 0 ? label : NameOf(status, context.Strings);
            UnknownMark.text = status.Unknown ? context.Strings?.Get(CardStrings.Keys.StatusUnknownMark) ?? "" : "";
            UnknownMark.style.display = status.Unknown ? DisplayStyle.Flex : DisplayStyle.None;
            Paint(Ring, status);

            bool scale = context.Variant == BuiltInBlocks.StatusScale;
            Ring.style.display = scale ? DisplayStyle.None : DisplayStyle.Flex;
            ScaleRow.style.display = scale ? DisplayStyle.Flex : DisplayStyle.None;
            if (!scale) return;
            foreach (var (key, stepStatus) in CardStatusRule.Scale(context.Poi, context.MarkerLook, context.Taxonomy))
            {
                var step = Take(_steps.Count);
                step.Key = key;
                step.Name.text = stepStatus.LevelName;
                Paint(step.Ring, stepStatus);
                bool current = !status.Unknown && key == context.Poi.status_level_key;
                step.Box.EnableInClassList("card-status__step--current", current);
                ScaleRow.Add(step.Box);
                _steps.Add(step);
            }
        }

        public void Unbind()
        {
            foreach (var step in _steps) step.Box.RemoveFromHierarchy();
            _steps.Clear();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // The ring picture of the line style, tinted with the wall's colour -- or with the card's status token
        private static void Paint(VisualElement ring, CardStatusRule.Status status)
        {
            foreach (string style in LineStyles) ring.EnableInClassList("card-status__ring--" + style, style == status.LineStyle);
            for (int i = 0; i <= CardStatusRule.UnknownTokenStep; i++)
                ring.EnableInClassList("card-status__ring--tone-" + i, !status.FromWall && i == status.TokenStep);
            // - a wall colour is data from config.json, not a literal: it is written inline so it beats the token classes
            if (status.FromWall) ring.style.unityBackgroundImageTintColor = status.Color;
            else ring.style.unityBackgroundImageTintColor = StyleKeyword.Null;
        }

        // The condition's name: its Outline Types row, else "Not assessed" / "20% damaged" from the card's strings
        private static string NameOf(CardStatusRule.Status status, CardStrings strings)
        {
            if (status.LevelName.Length > 0) return status.LevelName;
            if (strings == null) return "";
            return status.Unknown
                ? strings.Get(CardStrings.Keys.StatusUnknown)
                : string.Format(strings.Get(CardStrings.Keys.StatusPercent), UnityEngine.Mathf.RoundToInt(status.Pct));
        }

        private Step Take(int index)
        {
            while (_pool.Count <= index)
            {
                var step = new Step { Box = new VisualElement(), Ring = new VisualElement(), Name = new Label() };
                step.Box.AddToClassList("card-status__step");
                step.Ring.AddToClassList("card-status__ring");
                step.Ring.AddToClassList("card-status__ring--small");
                step.Name.AddToClassList("card-status__step-name");
                step.Box.Add(step.Ring);
                step.Box.Add(step.Name);
                _pool.Add(step);
            }
            return _pool[index];
        }
    }
}
