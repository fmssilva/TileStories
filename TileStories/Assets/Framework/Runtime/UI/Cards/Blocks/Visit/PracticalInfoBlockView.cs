using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The practical_info block (_3.1 Tier 1 group B): what a visitor needs to plan a visit. rows: one row per item -- an icon
    // from the card's one USS-drawn set (CardIcons, the same the actions use), the label and the value -- split by hairlines.
    // A row with no icon keeps its words in line with the others (the icon's space stays); a row with no label is left out
    // (ItemIsComplete). Only classes here; Visit.uss draws it.
    public sealed class PracticalInfoBlockView : IBlockView
    {
        public sealed class Row
        {
            public VisualElement Box;
            public VisualElement Icon;
            public Label Label;
            public Label Value;
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Row> Rows => _rows.Shown;

        private readonly ElementPool<Row> _rows = new(_ => NewRow(), row => row.Box.RemoveFromHierarchy());
        private string _variantClass;

        public PracticalInfoBlockView()
        {
            Root = new VisualElement { name = "card-practical" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-practical");
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-practical--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var rowFields = BuiltInBlocks.PracticalInfo.Field(BuiltInBlocks.PracticalInfoItemsField).ItemFields;
            foreach (var item in read.Items(BuiltInBlocks.PracticalInfoItemsField))
            {
                if (!BlockFieldReader.ItemIsComplete(item, rowFields)) continue;
                var row = _rows.Take();
                // - only the practical keys: an action's icon (or a stale word) draws nothing, the words stay in line
                string icon = read.ItemValue(item, BuiltInBlocks.PracticalInfoIconField);
                bool drawn = System.Array.IndexOf(BuiltInBlocks.PracticalInfoIcons, icon) >= 0;
                CardIcons.SetKey(row.Icon, drawn ? icon : null);
                // - no icon: no empty badge either, but its space stays so every row's words start on one line
                row.Icon.style.visibility = drawn ? Visibility.Visible : Visibility.Hidden;
                row.Label.text = read.ItemText(item, BuiltInBlocks.PracticalInfoLabelField);
                string value = read.ItemText(item, BuiltInBlocks.PracticalInfoValueField);
                row.Value.text = value;
                row.Value.style.display = value.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                Root.Add(row.Box);
            }
        }

        public void Unbind()
        {
            _rows.ReleaseAll();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // One row: its icon beside the label and value
        private static Row NewRow()
        {
            var row = new Row { Box = new VisualElement(), Icon = CardIcons.Create(), Label = new Label(), Value = new Label() };
            row.Box.AddToClassList("card-practical__row");
            row.Icon.AddToClassList("card-practical__icon");
            var words = new VisualElement();
            words.AddToClassList("card-practical__words");
            row.Label.AddToClassList("card-practical__label");
            row.Value.AddToClassList("card-practical__value");
            words.Add(row.Label);
            words.Add(row.Value);
            row.Box.Add(row.Icon);
            row.Box.Add(words);
            return row;
        }
    }
}
