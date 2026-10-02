using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The actions block (_3.1 Tier 1): buttons that DO something for the visitor, each an authored label and one of the
    // framework's actions (BuiltInBlocks.ActionOptions), asked of the card through IBlockHost. Three looks -- circles:
    // a round icon with its label under it; pill_row: icon + label pills in a wrapping row; sticky_cta: ONE call to action
    // (the first button) full width, pinned to the card's footer (BlockKindDefinition.FooterVariants) -- one button is what
    // fits under the header at every stop; a stack of them would not be sticky at all. A button with no words of its own
    // reads its action's card text (ActionsRule); an action this framework does not know (a later tier's) is left out:
    // never a button that does nothing. Icons are the card's one USS-drawn set (CardIcons), keyed by the action.
    public sealed class ActionsBlockView : IBlockView
    {
        public sealed class Action
        {
            public Button Button;
            public VisualElement Icon;
            public Label Label;
            public string Kind;
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Action> Actions => _actions.Shown;

        private readonly ElementPool<Action> _actions;
        private string _variantClass;
        private IBlockHost _host;

        public ActionsBlockView()
        {
            Root = new VisualElement { name = "card-actions" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-actions");
            _actions = new ElementPool<Action>(_ => NewAction(), action => action.Button.RemoveFromHierarchy());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _host = context.Host;
            _variantClass = "card-actions--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            foreach (var button in ActionsRule.Buttons(read, context.Variant))
            {
                // - a row with no words of its own reads its action's card text: one wording per action, whichever block draws it
                string defaultKey = ActionsRule.DefaultWordsKey(button.Action);
                string label = button.Words.Length > 0 ? button.Words : defaultKey != null ? context.Strings?.Get(defaultKey) ?? "" : "";
                var action = _actions.Take();
                action.Kind = button.Action;
                action.Label.text = label;
                CardIcons.SetKey(action.Icon, button.Action);
                // - the pill row wears the card's one pill shape (CardParts.uss), shared with story_chapters' buttons
                action.Button.EnableInClassList("card-pill", context.Variant == BuiltInBlocks.ActionsPillRow);
                Root.Add(action.Button);
            }
        }

        public void Unbind()
        {
            _actions.ReleaseAll();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
            _host = null;
        }

        // Do what this button's action says, through the card
        public void Run(Action action)
        {
            if (action.Kind == BuiltInBlocks.ActionShowOnWall) _host?.ShowOnWall();
        }

        // One button: its icon and words, its click wired once to whatever action it carries at the time
        private Action NewAction()
        {
            var action = new Action { Icon = CardIcons.Create(), Label = new Label { pickingMode = PickingMode.Ignore } };
            action.Button = new Button(() => Run(action));
            action.Button.AddToClassList("card-action");
            action.Button.AddToClassList("card-tap");
            action.Icon.AddToClassList("card-action__icon");
            action.Label.AddToClassList("card-action__label");
            action.Button.Add(action.Icon);
            action.Button.Add(action.Label);
            return action;
        }
    }
}
