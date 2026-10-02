using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The process_steps block (_3.1 Tier 1 group B): how something was made or done, step by step. numbered: each step's
    // number in a circle, the steps joined by a line (a rail under every number but the last), its title and an optional
    // text (a CardTextView: paragraphs, glossary words). The card numbers the shown steps itself, so a row left out (no
    // title: BlockFieldReader.ItemIsComplete) never leaves a gap in the count. Only classes here; About.uss draws it.
    public sealed class ProcessStepsBlockView : IBlockView
    {
        public sealed class Step
        {
            public VisualElement Row;
            public Label Number;
            public VisualElement Rail;
            public Label Title;
            public CardTextView Text;
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Step> Steps => _steps.Shown;

        private readonly ElementPool<Step> _steps;
        private string _variantClass;

        public ProcessStepsBlockView()
        {
            Root = new VisualElement { name = "card-steps" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-steps");
            _steps = new ElementPool<Step>(_ => NewStep(), step =>
            {
                step.Text.Clear();
                step.Row.RemoveFromHierarchy();
            });
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-steps--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var rowFields = BuiltInBlocks.ProcessSteps.Field(BuiltInBlocks.ProcessStepsItemsField).ItemFields;
            foreach (var item in read.Items(BuiltInBlocks.ProcessStepsItemsField))
            {
                if (!BlockFieldReader.ItemIsComplete(item, rowFields)) continue;
                var step = _steps.Take();
                // - the card numbers the shown steps: this one is the last taken
                step.Number.text = _steps.Shown.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
                step.Title.text = read.ItemText(item, BuiltInBlocks.ProcessStepsTitleField);
                var paragraphs = GlossaryMarkup.Paragraphs(read.ItemText(item, BuiltInBlocks.ProcessStepsTextField));
                step.Text.Bind(paragraphs, context.Glossary);
                step.Text.Root.style.display = paragraphs.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                Root.Add(step.Row);
            }
            // - the line joins a step to the NEXT one: the last step has none
            var shown = _steps.Shown;
            for (int i = 0; i < shown.Count; i++)
                shown[i].Rail.style.visibility = i < shown.Count - 1 ? Visibility.Visible : Visibility.Hidden;
        }

        public void Unbind()
        {
            _steps.ReleaseAll();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // One step's row: the number in its circle above the rail, then the title and text
        private static Step NewStep()
        {
            var step = new Step
            {
                Row = new VisualElement(), Number = new Label(), Rail = new VisualElement(), Title = new Label(),
                Text = new CardTextView("card-steps__paragraph"),
            };
            step.Row.AddToClassList("card-steps__step");
            var marker = new VisualElement();
            marker.AddToClassList("card-steps__marker");
            step.Number.AddToClassList("card-steps__number");
            step.Rail.AddToClassList("card-steps__rail");
            marker.Add(step.Number);
            marker.Add(step.Rail);
            var body = new VisualElement();
            body.AddToClassList("card-steps__body");
            step.Title.AddToClassList("card-steps__title");
            body.Add(step.Title);
            body.Add(step.Text.Root);
            step.Row.Add(marker);
            step.Row.Add(body);
            return step;
        }
    }
}
