using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The quick_facts block (_3.1 Tier 1): short label + value pairs ("Built" 1147), in three looks -- chips (pills that
    // wrap), grid_hairline (two columns split by thin lines), big_numbers (the value large, the label under it). A row
    // with neither text is left out; a row with only one of them shows that one. Only classes here.
    public sealed class QuickFactsBlockView : IBlockView
    {
        public VisualElement Root { get; }
        // The shown facts, in order
        public IReadOnlyList<(VisualElement Cell, Label Label, Label Value)> Facts => _facts.Shown;

        private readonly ElementPool<(VisualElement Cell, Label Label, Label Value)> _facts;
        private string _variantClass;

        public QuickFactsBlockView()
        {
            Root = new VisualElement { name = "card-quick-facts" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-facts");
            _facts = new ElementPool<(VisualElement Cell, Label Label, Label Value)>(_ => NewFact(), fact => fact.Cell.RemoveFromHierarchy());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-facts--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            foreach (var item in read.Items(BuiltInBlocks.QuickFactsItemsField))
            {
                string label = read.ItemText(item, BuiltInBlocks.QuickFactsLabelField);
                string value = read.ItemText(item, BuiltInBlocks.QuickFactsValueField);
                if (label.Length == 0 && value.Length == 0) continue;
                var fact = _facts.Take();
                fact.Label.text = label;
                fact.Label.style.display = label.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                fact.Value.text = value;
                fact.Value.style.display = value.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                Root.Add(fact.Cell);
            }
        }

        public void Unbind()
        {
            _facts.ReleaseAll();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // One fact's cell: its label and value
        private static (VisualElement Cell, Label Label, Label Value) NewFact()
        {
            var cell = new VisualElement();
            cell.AddToClassList("card-facts__cell");
            var value = new Label();
            value.AddToClassList("card-facts__value");
            var label = new Label();
            label.AddToClassList("card-facts__label");
            // - the look's USS orders them (chips read "label value", big numbers "value over label")
            cell.Add(label);
            cell.Add(value);
            return (cell, label, value);
        }
    }
}
