using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The list element (_3.3 R14.2): one item per line of its text, each behind a marker -- a round bullet (a drawn dot, no glyph) or its
    // number. Tone colours the words and the markers; size and alignment are classes. The rows come from an ElementPool (made once per place).
    public sealed class ListElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;
        // The shown items in order: the row, its bullet, its number and its words
        public IReadOnlyList<(VisualElement Row, VisualElement Bullet, Label Number, Label Words)> Items => _items.Shown;

        private readonly ElementPool<(VisualElement Row, VisualElement Bullet, Label Number, Label Words)> _items;
        private readonly NavTint _tint;
        private float _leading;
        private ElementRow _row;
        private ElementBindContext _context;

        public ListElement()
        {
            ElementStyle.Init(this, BuiltInElements.List);
            _items = new ElementPool<(VisualElement Row, VisualElement Bullet, Label Number, Label Words)>(_ => NewItem(), item =>
            {
                item.Words.style.color = StyleKeyword.Null;
                item.Number.style.color = StyleKeyword.Null;
                item.Bullet.style.backgroundColor = StyleKeyword.Null;
                item.Row.RemoveFromHierarchy();
            });
            _tint = new NavTint(this);
            RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                float leading = ElementStyle.LeadingOf(this);
                if (leading == _leading) return;
                _leading = leading;
                if (_context != null) Show();
            });
        }

        // Whether the bound row numbers its items
        public bool Numbered => _context != null && _context.StyleOf(BuiltInElements.ListStyle, _row) == BuiltInElements.ListNumbers;

        public void Bind(ElementRow row, ElementBindContext context)
        {
            _row = row;
            _context = context;
            Show();
        }

        public void Unbind()
        {
            _row = default;
            _context = null;
            _tint.Clear();
            _items.ReleaseAll();
        }

        // The items: the text's lines with words, in order
        public static List<string> ItemsOf(string text)
        {
            var items = new List<string>();
            if (string.IsNullOrEmpty(text)) return items;
            foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
                if (line.Trim().Length > 0) items.Add(line.Trim());
            return items;
        }

        private void Show()
        {
            ElementStyle.Apply(this, _row, _context);
            _tint.Clear();
            _items.ReleaseAll();
            bool family = ElementStyle.IsFamilyTone(_row, _context);
            var glossary = _context.Block?.Glossary;
            var lines = ItemsOf(new BlockFieldReader(null, _context.Language, _context.Fallback).ItemText(_row.Data, BuiltInElements.TextField));
            for (int i = 0; i < lines.Count; i++)
            {
                var item = _items.Take();
                item.Bullet.style.display = Numbered ? DisplayStyle.None : DisplayStyle.Flex;
                item.Number.style.display = Numbered ? DisplayStyle.Flex : DisplayStyle.None;
                // - a number is a figure, never a word to translate
                item.Number.text = (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) + ".";
                string words = GlossaryMarkup.ToRichText(GlossaryMarkup.Parse(lines[i]), t => glossary != null && glossary.Knows(t));
                item.Words.text = ElementStyle.WithLeading(words, _leading);
                Add(item.Row);
                if (!family) continue;
                _tint.Ink(item.Words, _context.FamilyToken);
                _tint.Ink(item.Number, _context.FamilyToken);
                _tint.Fill(item.Bullet, _context.FamilyToken);
            }
        }

        private static (VisualElement, VisualElement, Label, Label) NewItem()
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("ts-el-list__item");
            var bullet = new VisualElement { pickingMode = PickingMode.Ignore };
            bullet.AddToClassList("ts-el-list__bullet");
            var number = new Label { pickingMode = PickingMode.Ignore };
            number.AddToClassList("ts-el-list__number");
            var words = new Label { enableRichText = true, pickingMode = PickingMode.Ignore };
            words.AddToClassList("ts-el-list__words");
            row.Add(bullet);
            row.Add(number);
            row.Add(words);
            return (row, bullet, number, words);
        }
    }
}
