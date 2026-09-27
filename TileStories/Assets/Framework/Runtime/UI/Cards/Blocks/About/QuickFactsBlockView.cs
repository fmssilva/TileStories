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
        public IReadOnlyList<(VisualElement Cell, Label Label, Label Value)> Facts => _shown;

        private readonly List<(VisualElement Cell, Label Label, Label Value)> _pool = new();
        private readonly List<(VisualElement, Label, Label)> _shown = new();
        private string _variantClass;

        public QuickFactsBlockView()
        {
            Root = new VisualElement { name = "card-quick-facts" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-facts");
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
                var fact = Take(_shown.Count);
                fact.Label.text = label;
                fact.Label.style.display = label.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                fact.Value.text = value;
                fact.Value.style.display = value.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                Root.Add(fact.Cell);
                _shown.Add(fact);
            }
        }

        public void Unbind()
        {
            foreach (var (cell, _, _) in _shown) cell.RemoveFromHierarchy();
            _shown.Clear();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        private (VisualElement Cell, Label Label, Label Value) Take(int index)
        {
            while (_pool.Count <= index)
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
                _pool.Add((cell, label, value));
            }
            return _pool[index];
        }
    }
}
