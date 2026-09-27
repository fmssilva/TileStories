using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The actions block (_3.1 Tier 1): buttons that DO something for the visitor, each an authored label and one of the
    // framework's actions (BuiltInBlocks.ActionOptions), asked of the card through IBlockHost. Three looks -- circles:
    // a round icon with its label under it; pill_row: icon + label pills in a wrapping row; sticky_cta: ONE call to action
    // (the first button) full width, pinned to the card's footer (BlockKindDefinition.FooterVariants) -- one button is what
    // fits under the header at every stop; a stack of them would not be sticky at all. An action with no label, or one
    // this framework does not know (a later tier's), is left out: never a button that does nothing. Icons are drawn by USS.
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
        public IReadOnlyList<Action> Actions => _shown;

        private readonly List<Action> _pool = new();
        private readonly List<Action> _shown = new();
        private string _variantClass;
        private IBlockHost _host;

        public ActionsBlockView()
        {
            Root = new VisualElement { name = "card-actions" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-actions");
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _host = context.Host;
            _variantClass = "card-actions--" + context.Variant;
            Root.AddToClassList(_variantClass);
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            bool onlyOne = context.Variant == BuiltInBlocks.ActionsStickyCta;
            foreach (var item in read.Items(BuiltInBlocks.ActionsItemsField))
            {
                if (onlyOne && _shown.Count == 1) break;
                string label = read.ItemText(item, BuiltInBlocks.ActionsLabelField);
                string kind = read.ItemValue(item, BuiltInBlocks.ActionsActionField);
                if (label.Length == 0 || !IsKnown(kind)) continue;
                var action = Take(_shown.Count);
                action.Kind = kind;
                action.Label.text = label;
                foreach (string known in BuiltInBlocks.ActionOptions)
                    action.Icon.EnableInClassList("card-action__icon--" + known, known == kind);
                Root.Add(action.Button);
                _shown.Add(action);
            }
        }

        public void Unbind()
        {
            foreach (var a in _shown) a.Button.RemoveFromHierarchy();
            _shown.Clear();
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
            _host = null;
        }

        // Do what this button's action says, through the card
        public void Run(Action action)
        {
            if (action.Kind == BuiltInBlocks.ActionShowOnWall) _host?.ShowOnWall();
        }

        private static bool IsKnown(string kind)
        {
            foreach (string known in BuiltInBlocks.ActionOptions)
                if (known == kind) return true;
            return false;
        }

        private Action Take(int index)
        {
            while (_pool.Count <= index)
            {
                var action = new Action { Icon = new VisualElement { pickingMode = PickingMode.Ignore }, Label = new Label { pickingMode = PickingMode.Ignore } };
                action.Button = new Button(() => Run(action));
                action.Button.AddToClassList("card-action");
                action.Button.AddToClassList("card-tap");
                action.Icon.AddToClassList("card-action__icon");
                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList("card-action__icon-dot");
                action.Icon.Add(dot);
                action.Label.AddToClassList("card-action__label");
                action.Button.Add(action.Icon);
                action.Button.Add(action.Label);
                _pool.Add(action);
            }
            return _pool[index];
        }
    }
}
