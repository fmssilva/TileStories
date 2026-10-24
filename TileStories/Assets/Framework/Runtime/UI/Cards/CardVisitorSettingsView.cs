using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The visitor's settings (_3.3 P4.4b, R16): the small panel the bar's gear button opens over the card -- Language (the wall's
    // languages by their native names, only when it offers two or more: it replaces the language chip of _3.1 15.3) and Appearance (Light / Dark). The marked
    // button is the current choice; a pick marks itself at once and raises LanguagePicked / ModePicked, and the card's owner remembers it
    // (CardLocalState). Groups for text size and reduce motion go below these two later. Plain C#: PoiCardSheetView opens it in the
    // card's overlay layer (CardMenuOverlay.OpenPopover); Navigation.uss styles it.
    public sealed class CardVisitorSettingsView
    {
        public const string ChoiceClass = "card-settings__choice";
        public const string ChosenClass = "card-settings__choice--chosen";
        private const string ChoiceNamePrefix = "card-settings-";

        public VisualElement Root { get; }

        // A language button was tapped: its code
        public event Action<string> LanguagePicked;
        // A mode button was tapped: its key (CardOptions.Modes)
        public event Action<string> ModePicked;

        private readonly Label _title = new();
        private readonly VisualElement _languageGroup = new();
        private readonly Label _languageTitle = new();
        private readonly VisualElement _languages = new();
        private readonly Label _appearanceTitle = new();
        private readonly VisualElement _modes = new();

        public CardVisitorSettingsView()
        {
            Root = new VisualElement { name = "card-settings" };
            Root.AddToClassList("card-settings");
            _title.AddToClassList("card-settings__title");
            Root.Add(_title);
            foreach (var (group, title, row) in new[] { (_languageGroup, _languageTitle, _languages), (new VisualElement(), _appearanceTitle, _modes) })
            {
                group.AddToClassList("card-settings__group");
                title.AddToClassList("card-settings__group-title");
                row.AddToClassList("card-settings__row");
                group.Add(title);
                group.Add(row);
                Root.Add(group);
            }
            // - the mode can change while the panel is open without a tap here (a live edit of the wall's Default Mode): the mark follows
            //   it; subscribed only while the panel is on a panel, so the static event never holds a closed card
            Root.RegisterCallback<AttachToPanelEvent>(_ => ThemeController.ModeChanged += MarkCurrentMode);
            Root.RegisterCallback<DetachFromPanelEvent>(_ => ThemeController.ModeChanged -= MarkCurrentMode);
        }

        // Mark the mode every screen shows now
        private void MarkCurrentMode()
        {
            foreach (var choice in _modes.Children()) choice.EnableInClassList(ChosenClass, choice.name == ChoiceNamePrefix + ThemeController.CurrentMode);
        }

        // Show the wall's languages with `language` marked and the modes with `mode` marked, worded through `words` (CardStrings keys)
        public void Bind(IReadOnlyList<string> languages, string language, string mode, Func<string, string> words)
        {
            _title.text = words(CardStrings.Keys.VisitorSettings);
            _languageTitle.text = words(CardStrings.Keys.LanguageSwitch);
            _appearanceTitle.text = words(CardStrings.Keys.SettingsAppearance);

            var choices = CardLanguageRule.Choices(languages);
            _languageGroup.style.display = choices.Count >= 2 ? DisplayStyle.Flex : DisplayStyle.None;
            _languages.Clear();
            // - a language's button names it as its speakers do ("English", "Portugues" with its accents: the card texts' language_name_
            //   rows, P4.4b part 2), so a visitor finds theirs whatever the card is in
            foreach (string code in choices)
                _languages.Add(MakeChoice(code, LanguageLabel(code, words), code == language, _languages, v => LanguagePicked?.Invoke(v)));
            _modes.Clear();
            for (int i = 0; i < CardOptions.Modes.Length; i++)
            {
                string key = CardOptions.Modes[i];
                string word = words(key == CardOptions.ModeLight ? CardStrings.Keys.ModeLight : CardStrings.Keys.ModeDark);
                _modes.Add(MakeChoice(key, word, key == mode, _modes, v => ModePicked?.Invoke(v)));
            }
        }

        // The card texts' rows naming a language: this prefix + its code (CardStrings.Keys.LanguageNameEn / Pt are the ones shipped)
        private const string LanguageNamePrefix = "language_name_";

        // The words for language `code`: its native name from the card texts, else (no row names it: a wall's own language) its code
        public static string LanguageLabel(string code, Func<string, string> words)
        {
            string key = LanguageNamePrefix + code;
            string name = words?.Invoke(key);
            return string.IsNullOrEmpty(name) || name == key ? code.ToUpperInvariant() : name;
        }

        // The choice button for a language code or a mode key (null when not shown): what a test taps
        public Button Choice(string value) => Root.Q<Button>(ChoiceNamePrefix + value);

        private static Button MakeChoice(string value, string words, bool chosen, VisualElement row, Action<string> picked)
        {
            Button button = null;
            button = new Button(() =>
            {
                // - the pick shows at once, whatever the owner does with it
                foreach (var other in row.Children()) other.EnableInClassList(ChosenClass, other == button);
                picked?.Invoke(value);
            }) { name = ChoiceNamePrefix + value, text = words };
            button.AddToClassList(ChoiceClass);
            button.EnableInClassList(ChosenClass, chosen);
            return button;
        }
    }
}
