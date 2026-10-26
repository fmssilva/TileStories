using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The dialogue block (_3.1 Tier 3, step 8B; look choices): a conversation told one line at a time, as chat bubbles. The first
    // line shows when the card opens; each tap on Continue shows the next. A line may offer up to three replies (DialogueRule:
    // one choice level, flat slots): Continue then gives way to the replies, the one picked appears as the visitor's own bubble,
    // the speaker's answer follows, and the conversation goes on. At the end Start again begins it over.
    // How far the visitor has got is VIEW state: it starts again at every bind and is never stored (CardLocalState is for what
    // the visitor did, not where they are in a block). Only classes here; Stories.uss draws it.
    public sealed class DialogueBlockView : IBlockView
    {
        // One bubble of the thread: who speaks and what is said; the visitor's own replies sit on the other side
        public sealed class Bubble
        {
            public VisualElement Box;
            public Label Speaker;
            public Label Text;
        }

        // One reply the visitor may pick
        public sealed class ChoiceButton
        {
            public Button Button;
            public Label Label;
        }

        public VisualElement Root { get; }
        public VisualElement Thread { get; }
        public VisualElement Actions { get; }
        public VisualElement ChoicesColumn { get; }
        public Button Continue { get; }
        public Button Again { get; }
        public IReadOnlyList<Bubble> Bubbles => _bubbles.Shown;
        public IReadOnlyList<ChoiceButton> Choices => _choices.Shown;

        // How many lines of the block have been said (replies and answers are not counted); view state
        public int Reached { get; private set; }

        // How many lines the block has
        public int Count => _lines.Count;

        // Whether the conversation waits for the visitor to pick a reply
        public bool AwaitsChoice => _awaiting != null;

        private readonly List<DialogueRule.Line> _lines = new();
        private readonly ElementPool<Bubble> _bubbles = new(_ => NewBubble(), bubble => bubble.Box.RemoveFromHierarchy());
        private readonly ElementPool<ChoiceButton> _choices;
        private readonly Label _continueLabel;
        private readonly Label _againLabel;
        private CardStrings _strings;
        private string _variantClass;
        private DialogueRule.Line _awaiting;

        public DialogueBlockView()
        {
            Root = new VisualElement { name = "card-dialogue" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-dialogue");
            _choices = new ElementPool<ChoiceButton>(NewChoice, choice => choice.Button.RemoveFromHierarchy());
            Thread = new VisualElement();
            Thread.AddToClassList("card-dialogue__thread");
            Actions = new VisualElement();
            Actions.AddToClassList("card-dialogue__actions");
            ChoicesColumn = new VisualElement();
            ChoicesColumn.AddToClassList("card-dialogue__choices");
            (Continue, _continueLabel) = PillButton("card-dialogue__continue", ContinueTapped);
            (Again, _againLabel) = PillButton("card-dialogue__again", Restart);
            Actions.Add(ChoicesColumn);
            Actions.Add(Continue);
            Actions.Add(Again);
            Root.Add(Thread);
            Root.Add(Actions);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variantClass = "card-dialogue--" + context.Variant;
            Root.AddToClassList(_variantClass);
            _strings = context.Strings;
            _continueLabel.text = _strings?.Get(CardStrings.Keys.DialogueContinue) ?? "";
            _againLabel.text = _strings?.Get(CardStrings.Keys.DialogueAgain) ?? "";
            _lines.AddRange(DialogueRule.Lines(new BlockFieldReader(instance, context.Language, context.FallbackLanguage)));
            Restart();
        }

        public void Unbind()
        {
            ClearThread();
            _lines.Clear();
            _awaiting = null;
            Reached = 0;
            _strings = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Begin the conversation: only its first line is said
        public void Restart()
        {
            ClearThread();
            _awaiting = null;
            Reached = 0;
            if (_lines.Count > 0) Say(_lines[Reached++], visitor: false);
            Refresh();
        }

        // Say the next line (nothing while a reply is awaited or when the conversation has ended)
        public void ContinueTapped()
        {
            if (_awaiting != null || Reached >= _lines.Count) return;
            Say(_lines[Reached++], visitor: false);
            Refresh();
            KeepActionsInView();
        }

        // The visitor picks reply `index` of the line just said: it appears as their own bubble, the speaker's answer follows
        public void Pick(int index)
        {
            var line = _awaiting;
            if (line == null || index < 0 || index >= line.Choices.Count) return;
            _awaiting = null;
            var choice = line.Choices[index];
            AddBubble(_strings?.Get(CardStrings.Keys.DialogueYou) ?? "", choice.Label, visitor: true);
            if (choice.Reply.Length > 0) AddBubble(line.Speaker, choice.Reply, visitor: false);
            Refresh();
            KeepActionsInView();
        }

        private void Say(DialogueRule.Line line, bool visitor)
        {
            AddBubble(line.Speaker, line.Text, visitor);
            // - a line with replies waits for the visitor to pick one
            _awaiting = line.Choices.Count > 0 ? line : null;
        }

        // What the visitor can do now: pick a reply, continue, start again -- or nothing (a one-line block)
        private void Refresh()
        {
            _choices.ReleaseAll();
            if (_awaiting != null)
                for (int i = 0; i < _awaiting.Choices.Count; i++)
                {
                    var choice = _choices.Take();
                    choice.Label.text = _awaiting.Choices[i].Label;
                    choice.Button.tooltip = _awaiting.Choices[i].Label;
                    ChoicesColumn.Add(choice.Button);
                }
            bool ended = _awaiting == null && Reached >= _lines.Count;
            ChoicesColumn.style.display = _awaiting != null ? DisplayStyle.Flex : DisplayStyle.None;
            Continue.style.display = _awaiting == null && !ended ? DisplayStyle.Flex : DisplayStyle.None;
            // - a conversation of one line has nothing to start again
            Again.style.display = ended && _lines.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            Actions.style.display = _awaiting != null || !ended || _lines.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // The buttons move down as the thread grows: bring them into view (only after the visitor's own tap, never on open)
        private void KeepActionsInView()
        {
            Root.schedule.Execute(() => Root.GetFirstAncestorOfType<ScrollView>()?.ScrollTo(Actions));
        }

        private void ClearThread()
        {
            _bubbles.ReleaseAll();
            _choices.ReleaseAll();
        }

        private void AddBubble(string speaker, string words, bool visitor)
        {
            var bubble = _bubbles.Take();
            bubble.Speaker.text = speaker;
            bubble.Speaker.style.display = speaker.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            bubble.Text.text = words;
            bubble.Box.EnableInClassList("card-dialogue__bubble--visitor", visitor);
            Thread.Add(bubble.Box);
        }

        // One chat bubble: who speaks over what they say
        private static Bubble NewBubble()
        {
            var bubble = new Bubble { Box = new VisualElement(), Speaker = new Label(), Text = new Label() };
            bubble.Box.AddToClassList("card-dialogue__bubble");
            bubble.Speaker.AddToClassList("card-dialogue__speaker");
            bubble.Text.AddToClassList("card-dialogue__text");
            bubble.Box.Add(bubble.Speaker);
            bubble.Box.Add(bubble.Text);
            return bubble;
        }

        // The reply button of slot `slot`: a tap picks that reply of the line waiting for one
        private ChoiceButton NewChoice(int slot)
        {
            var choice = new ChoiceButton { Button = new Button(), Label = new Label { pickingMode = PickingMode.Ignore } };
            choice.Button.AddToClassList("card-dialogue__choice");
            choice.Button.AddToClassList("card-tap");
            choice.Label.AddToClassList("card-dialogue__choice-label");
            choice.Button.Add(choice.Label);
            choice.Button.clicked += () => Pick(slot);
            return choice;
        }

        private static (Button, Label) PillButton(string className, System.Action onClick)
        {
            var button = new Button(onClick);
            // - the card's one pill shape (CardParts.uss), as story_chapters' and knowledge_check's buttons
            button.AddToClassList("card-pill");
            button.AddToClassList("card-tap");
            button.AddToClassList("card-dialogue__button");
            button.AddToClassList(className);
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("card-dialogue__button-label");
            button.Add(label);
            return (button, label);
        }
    }
}
