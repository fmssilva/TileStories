using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The swatches block (_3.1 Tier 1 group B): the colours of the point itself, each a sample, its name, its hex code
    // (only with Show Code: a code is a workshop detail, 6C) and an optional note. grid: two columns. The sample's colour is CONTENT from config.json -- the one colour a card view
    // writes, parsed by the strict BlockFieldReader.TryParseColor; a row with no name or no real colour is left out
    // (BlockFieldReader.ItemIsComplete). Everything else (sizes, the sample's hairline border, text) is About.uss.
    public sealed class SwatchesBlockView : IBlockView
    {
        public sealed class Swatch
        {
            public VisualElement Cell;
            public VisualElement Sample;
            public Label Name;
            public Label Code;
            public Label Note;
            // What the config asked for (the tests compare the drawn sample to it)
            public Color Color;
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Swatch> Swatches => _swatches.Shown;

        private readonly ElementPool<Swatch> _swatches;
        private string _variantClass;

        public SwatchesBlockView()
        {
            Root = new VisualElement { name = "card-swatches" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-swatches");
            _swatches = new ElementPool<Swatch>(_ => NewSwatch(), swatch => swatch.Cell.RemoveFromHierarchy());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-swatches--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            var rowFields = BuiltInBlocks.Swatches.Field(BuiltInBlocks.SwatchesItemsField).ItemFields;
            bool showCode = read.Flag(BuiltInBlocks.SwatchesShowCodeField);
            foreach (var item in read.Items(BuiltInBlocks.SwatchesItemsField))
            {
                if (!BlockFieldReader.ItemIsComplete(item, rowFields) || !read.ItemColor(item, BuiltInBlocks.SwatchesColourField, out var color)) continue;
                var swatch = _swatches.Take();
                swatch.Color = color;
                // - content colour from config (the only literal colour a card may show), written inline over the class
                swatch.Sample.style.backgroundColor = color;
                swatch.Name.text = read.ItemText(item, BuiltInBlocks.SwatchesNameField);
                swatch.Code.text = HexCode(color);
                swatch.Code.style.display = showCode ? DisplayStyle.Flex : DisplayStyle.None;
                string note = read.ItemText(item, BuiltInBlocks.SwatchesNoteField);
                swatch.Note.text = note;
                swatch.Note.style.display = note.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                Root.Add(swatch.Cell);
            }
        }

        public void Unbind()
        {
            _swatches.ReleaseAll();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // The colour as "#RRGGBB" in upper case: one written form, whatever the author typed (#abc -> #AABBCC)
        private static string HexCode(Color color)
        {
            Color32 c = color;
            return $"#{c.r:X2}{c.g:X2}{c.b:X2}";
        }

        // One swatch's cell: the colour sample beside its name, code and note
        private static Swatch NewSwatch()
        {
            var swatch = new Swatch { Cell = new VisualElement(), Sample = new VisualElement(), Name = new Label(), Code = new Label(), Note = new Label() };
            swatch.Cell.AddToClassList("card-swatches__cell");
            swatch.Sample.AddToClassList("card-swatches__sample");
            var words = new VisualElement();
            words.AddToClassList("card-swatches__words");
            swatch.Name.AddToClassList("card-swatches__name");
            swatch.Code.AddToClassList("card-swatches__code");
            swatch.Note.AddToClassList("card-swatches__note");
            words.Add(swatch.Name);
            words.Add(swatch.Code);
            words.Add(swatch.Note);
            swatch.Cell.Add(swatch.Sample);
            swatch.Cell.Add(words);
            return swatch;
        }
    }
}
