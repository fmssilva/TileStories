using System.Globalization;
using UnityEngine.UIElements;

namespace TileStories
{
    // The person block (_3.1 Tier 1 group B): someone in the point's story -- a name, an optional role and text (a
    // CardTextView), beside a round monogram (the first letter of the name) that holds the place of the photo Tier 2 brings.
    // row: in line with the card; card: on its own panel, the name larger. Only classes here; Stories.uss draws both.
    public sealed class PersonBlockView : IBlockView
    {
        public VisualElement Root { get; }
        public Label Monogram { get; }
        public Label Name { get; }
        public Label Role { get; }
        public CardTextView Text { get; }

        private string _variantClass;

        public PersonBlockView()
        {
            Root = new VisualElement { name = "card-person" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-person");
            var top = new VisualElement();
            top.AddToClassList("card-person__top");
            Monogram = new Label();
            Monogram.AddToClassList("card-person__monogram");
            var words = new VisualElement();
            words.AddToClassList("card-person__words");
            Name = new Label();
            Name.AddToClassList("card-person__name");
            Role = new Label();
            Role.AddToClassList("card-person__role");
            words.Add(Name);
            words.Add(Role);
            top.Add(Monogram);
            top.Add(words);
            Text = new CardTextView("card-person__paragraph");
            Root.Add(top);
            Root.Add(Text.Root);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-person--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string name = read.Text(BuiltInBlocks.PersonNameField);
            Name.text = name;
            Monogram.text = InitialOf(name);
            string role = read.Text(BuiltInBlocks.PersonRoleField);
            Role.text = role;
            Role.style.display = role.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            var paragraphs = GlossaryMarkup.Paragraphs(read.Text(BuiltInBlocks.PersonTextField));
            Text.Bind(paragraphs, context.Glossary);
            Text.Root.style.display = paragraphs.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Unbind()
        {
            Text.Clear();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // The first letter of the name, upper case ("King Manuel I" -> "K"); a name that starts with a digit or a mark
        // skips to its first letter, and a name with none gives its first character
        public static string InitialOf(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var e = StringInfo.GetTextElementEnumerator(name.Trim());
            string first = null;
            while (e.MoveNext())
            {
                string element = e.GetTextElement();
                first ??= element;
                if (char.IsLetter(element, 0)) return element.ToUpperInvariant();
            }
            return first ?? "";
        }
    }
}
