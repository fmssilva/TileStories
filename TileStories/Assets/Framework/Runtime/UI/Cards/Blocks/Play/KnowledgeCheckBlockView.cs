using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The knowledge_check block (_3.1 Tier 3): questions asked one at a time, answered with no score and no penalty.
    //   multiple_choice  -- the options as buttons
    //   image_choice     -- the options as pictures (a caption under each, when it has one)
    //   true_false_swipe -- a statement card the visitor swipes right (True) or left (False), or answers with the two buttons
    // A right answer shows "Correct!" and the explanation; a wrong one a gentle "Actually..." and the same explanation, the right
    // choice marked. The answer is remembered through the card's CardLocalState under the question's authored row, so the block
    // opens showing what was answered; there is no retry. Which question shows is VIEW state (it starts at the first unanswered
    // one and is never stored). Which rows count for the look is KnowledgeCheckRule's; a card with several questions gets
    // Previous / Next. Only classes here; Play.uss draws it.
    public sealed class KnowledgeCheckBlockView : IBlockView
    {
        // One answerable choice: its tap target, its words and (image_choice) its picture
        public sealed class Choice
        {
            public Button Button;
            public Label Text;
            public CardImage Image;
            public int Index;
        }

        public VisualElement Root { get; }
        public Label Counter { get; }
        // The question, on its card: the true / false look's card is what a swipe moves
        public VisualElement Stage { get; }
        public Label Prompt { get; }
        public Label SwipeHint { get; }
        public VisualElement ChoicesRow { get; }
        public VisualElement Result { get; }
        public Label Verdict { get; }
        public Label Explanation { get; }
        public VisualElement Nav { get; }
        public Button Previous { get; }
        public Button Next { get; }
        public IReadOnlyList<Choice> Choices => _shown;

        // The question shown (0-based among the shown questions) and how many there are
        public int Index { get; private set; }
        public int Count => _questions.Count;
        // The choice made for the question shown (-1 = not answered yet)
        public int Chosen => Index >= 0 && Index < _chosen.Count ? _chosen[Index] : -1;
        public bool Answered => Chosen >= 0;
        public bool IsSwiping { get; private set; }

        private readonly List<KnowledgeCheckRule.Question> _questions = new();
        private readonly List<int> _chosen = new();
        private readonly List<Choice> _pool = new();
        private readonly List<Choice> _shown = new();
        private readonly Label _previousLabel;
        private readonly Label _nextLabel;
        private CardStrings _strings;
        private CardLocalState _state;
        private IMediaSource _media;
        private string _poiId = "";
        private string _blockKey = "";
        private string _variant = "";
        private string _variantClass;
        private bool _swipes;
        private Vector2 _swipeStart;

        public KnowledgeCheckBlockView()
        {
            Root = new VisualElement { name = "card-quiz" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("card-quiz");
            Counter = new Label();
            Counter.AddToClassList("card-quiz__counter");
            Stage = new VisualElement { name = "card-quiz-stage" };
            Stage.AddToClassList("card-quiz__stage");
            Prompt = new Label { pickingMode = PickingMode.Ignore };
            Prompt.AddToClassList("card-quiz__prompt");
            SwipeHint = new Label { pickingMode = PickingMode.Ignore };
            SwipeHint.AddToClassList("card-quiz__swipe-hint");
            Stage.Add(Prompt);
            Stage.Add(SwipeHint);
            ChoicesRow = new VisualElement();
            ChoicesRow.AddToClassList("card-quiz__choices");
            Result = new VisualElement();
            Result.AddToClassList("card-quiz__result");
            Verdict = new Label();
            Verdict.AddToClassList("card-quiz__verdict");
            Explanation = new Label();
            Explanation.AddToClassList("card-quiz__explanation");
            Result.Add(Verdict);
            Result.Add(Explanation);
            Nav = new VisualElement();
            Nav.AddToClassList("card-quiz__nav");
            (Previous, _previousLabel) = NavButton("card-quiz__previous", () => Show(Index - 1));
            (Next, _nextLabel) = NavButton("card-quiz__next", () => Show(Index + 1));
            Nav.Add(Previous);
            Nav.Add(Next);
            Root.Add(Counter);
            Root.Add(Stage);
            Root.Add(ChoicesRow);
            Root.Add(Result);
            Root.Add(Nav);

            Stage.RegisterCallback<PointerDownEvent>(OnPointerDown);
            Stage.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            Stage.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Stage.RegisterCallback<PointerCancelEvent>(_ => EndSwipe());
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _variant = context.Variant;
            _variantClass = "card-quiz--" + _variant;
            Root.AddToClassList(_variantClass);
            _swipes = _variant == BuiltInBlocks.KnowledgeCheckTrueFalseSwipe;
            _strings = context.Strings;
            _state = context.State;
            _media = context.Media;
            _poiId = context.Poi?.id ?? "";
            _blockKey = instance.key ?? "";
            _questions.AddRange(KnowledgeCheckRule.Questions(instance, _variant, context.Language, context.FallbackLanguage));
            int firstOpen = -1;
            for (int i = 0; i < _questions.Count; i++)
            {
                int stored = _state?.Answer(_poiId, _blockKey, _questions[i].Row) ?? -1;
                // - an answer stored for a question that since lost an option is no answer
                _chosen.Add(stored >= 0 && stored < _questions[i].Options.Count ? stored : -1);
                if (firstOpen < 0 && _chosen[i] < 0) firstOpen = i;
            }
            SwipeHint.text = _strings?.Get(CardStrings.Keys.KnowledgeSwipeHint) ?? "";
            _previousLabel.text = _strings?.Get(CardStrings.Keys.KnowledgePrevious) ?? "";
            _nextLabel.text = _strings?.Get(CardStrings.Keys.KnowledgeNext) ?? "";
            Nav.style.display = _questions.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            Show(firstOpen >= 0 ? firstOpen : 0);
        }

        public void Unbind()
        {
            EndSwipe();
            foreach (var choice in _shown)
            {
                choice.Button.RemoveFromHierarchy();
                choice.Image.Clear(null);
            }
            _shown.Clear();
            _questions.Clear();
            _chosen.Clear();
            Index = 0;
            _state = null;
            _media = null;
            if (_variantClass != null) Root.RemoveFromClassList(_variantClass);
            _variantClass = null;
        }

        // Show question `index` (clamped): its words, its choices, and -- when it was answered -- the verdict and the explanation
        public void Show(int index)
        {
            if (_questions.Count == 0) return;
            Index = Mathf.Clamp(index, 0, _questions.Count - 1);
            var question = _questions[Index];
            Prompt.text = question.Text;
            string format = _strings?.Get(CardStrings.Keys.KnowledgeQuestionOf) ?? "";
            Counter.text = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, Index + 1, _questions.Count);
            Counter.style.display = _questions.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var old in _shown) old.Button.RemoveFromHierarchy();
            _shown.Clear();
            for (int i = 0; i < question.Options.Count; i++)
            {
                var choice = Take(i);
                BindChoice(choice, question.Options[i]);
                ChoicesRow.Add(choice.Button);
                _shown.Add(choice);
            }
            // - hidden, not removed: each button keeps its side of the row
            Previous.style.visibility = Index > 0 ? Visibility.Visible : Visibility.Hidden;
            Next.style.visibility = Index < _questions.Count - 1 ? Visibility.Visible : Visibility.Hidden;
            ShowVerdict(question, Chosen);
        }

        // Answer the question shown with `choice` (an index into its choices); a question already answered keeps its answer
        public void Answer(int choice)
        {
            if (_questions.Count == 0 || Answered) return;
            var question = _questions[Index];
            if (choice < 0 || choice >= question.Options.Count) return;
            _chosen[Index] = choice;
            _state?.SetAnswer(_poiId, _blockKey, question.Row, choice);
            ShowVerdict(question, choice);
        }

        // The look of a question that is (not yet) answered: marks on the choices and the card, the verdict and the explanation
        private void ShowVerdict(KnowledgeCheckRule.Question question, int chosen)
        {
            bool answered = chosen >= 0;
            bool right = answered && KnowledgeCheckRule.IsCorrect(question, chosen);
            Root.EnableInClassList("card-quiz--answered", answered);
            Stage.EnableInClassList("card-quiz__stage--correct", answered && right);
            Stage.EnableInClassList("card-quiz__stage--wrong", answered && !right);
            foreach (var choice in _shown)
            {
                choice.Button.EnableInClassList("card-quiz__choice--correct", answered && choice.Index == question.Correct);
                choice.Button.EnableInClassList("card-quiz__choice--wrong", answered && choice.Index == chosen && !right);
            }
            Result.style.display = answered ? DisplayStyle.Flex : DisplayStyle.None;
            Result.EnableInClassList("card-quiz__result--correct", answered && right);
            Result.EnableInClassList("card-quiz__result--wrong", answered && !right);
            Verdict.text = _strings?.Get(right ? CardStrings.Keys.KnowledgeCorrect : CardStrings.Keys.KnowledgeWrong) ?? "";
            Explanation.text = answered ? question.Explanation : "";
        }

        // Fill a choice for this look: True / False words, a caption, or a picture with its caption
        private void BindChoice(Choice choice, KnowledgeCheckRule.Option option)
        {
            bool pictures = _variant == BuiltInBlocks.KnowledgeCheckImageChoice;
            string words = option.Text;
            if (_swipes) words = _strings?.Get(choice.Index == KnowledgeCheckRule.ChoiceTrue ? CardStrings.Keys.KnowledgeTrue : CardStrings.Keys.KnowledgeFalse) ?? "";
            choice.Text.text = words;
            choice.Text.style.display = words.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            choice.Image.Root.style.display = pictures ? DisplayStyle.Flex : DisplayStyle.None;
            if (pictures) choice.Image.Show(_media, option.Image, _strings);
            // - the screen reader / desktop tooltip name: what the button says, or the caption of a picture
            choice.Button.tooltip = words;
        }

        private Choice Take(int index)
        {
            while (_pool.Count <= index)
            {
                var choice = new Choice { Index = _pool.Count, Button = new Button(), Text = new Label { pickingMode = PickingMode.Ignore }, Image = new CardImage("card-quiz__picture") };
                choice.Button.AddToClassList("card-quiz__choice");
                choice.Button.AddToClassList("card-tap");
                choice.Text.AddToClassList("card-quiz__choice-text");
                choice.Image.Root.pickingMode = PickingMode.Ignore;
                choice.Button.Add(choice.Image.Root);
                choice.Button.Add(choice.Text);
                var captured = choice;
                choice.Button.clicked += () => Answer(captured.Index);
                _pool.Add(choice);
            }
            return _pool[index];
        }

        private static (Button, Label) NavButton(string className, System.Action onClick)
        {
            var button = new Button(onClick);
            button.AddToClassList("card-quiz__button");
            // - the actions' pill shape (CardParts.uss), so every pill of the card is one shape
            button.AddToClassList("card-pill");
            button.AddToClassList(className);
            button.AddToClassList("card-tap");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("card-quiz__button-label");
            button.Add(label);
            return (button, label);
        }

        // ---- the true / false swipe: a pointer-captured drag on the statement card (so the stack does not scroll meanwhile) ----

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!_swipes || Answered || _questions.Count == 0) return;
            if (evt.button != 0 && evt.pointerType == UnityEngine.UIElements.PointerType.mouse) return;
            Stage.CapturePointer(evt.pointerId);
            IsSwiping = true;
            _swipeStart = evt.position;
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsSwiping || !Stage.HasPointerCapture(evt.pointerId)) return;
            // - the card follows the finger sideways (panel coordinates: the card itself moves, so its local position would not change)
            Stage.style.translate = new Translate(((Vector2)evt.position).x - _swipeStart.x, 0f);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!IsSwiping || !Stage.HasPointerCapture(evt.pointerId)) return;
            Stage.ReleasePointer(evt.pointerId);
            Vector2 travel = (Vector2)evt.position - _swipeStart;
            EndSwipe();
            int choice = KnowledgeCheckRule.SwipeChoice(travel, Stage.layout.width);
            if (choice >= 0) Answer(choice);
            evt.StopPropagation();
        }

        // The card comes back to its place
        private void EndSwipe()
        {
            IsSwiping = false;
            Stage.style.translate = StyleKeyword.Null;
        }
    }
}
