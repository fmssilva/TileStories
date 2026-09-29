using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The mini-player on the wall (_3.1 step 9A): a small bar at the bottom of the screen while audio started in a card plays on after the
    // card closed (Keep Audio Playing). A round play / pause button, and -- one tap target -- the audio's title and time: a tap on that
    // asks for the audio's card to be opened again (OpenRequested, the host selects the point), a thin progress line under them. It holds
    // no audio state: it draws what the audio owner says and asks it to pause or resume. Only classes here; PoiCard.uss draws it with the
    // card tokens (the bar carries the token classes itself, like the full-screen view).
    public sealed class MiniPlayerView
    {
        public VisualElement Root { get; }
        public Button PlayButton { get; }
        public CardIcons.VectorGlyph PlayGlyph { get; }
        public Button OpenButton { get; }
        public Label Title { get; }
        public Label TimeText { get; }
        public VisualElement BarFill { get; }

        // Raised when the visitor taps the title: the point whose card the audio belongs to
        public event Action<string> OpenRequested;

        private readonly ICardAudio _audio;
        private CardStrings _strings;

        // The bar is added to `layer` (the card's full-area layer); it stays hidden until Refresh says the audio is active and no card is open
        public MiniPlayerView(VisualElement layer, ICardAudio audio)
        {
            _audio = audio;
            Root = new VisualElement { name = "poi-card-mini" };
            Root.AddToClassList("ts-card");
            Root.AddToClassList("ts-theme-default");
            Root.AddToClassList("poi-card-mini");

            PlayButton = new Button { name = "poi-card-mini-play" };
            PlayButton.AddToClassList("poi-card-mini__play");
            PlayGlyph = CardIcons.CreateVector(CardIcons.Shape.Play);
            PlayGlyph.Filled = true;
            PlayGlyph.AddToClassList("card-audio__glyph");
            PlayButton.Add(PlayGlyph);
            PlayButton.clicked += () =>
            {
                if (_audio.Current != null) _audio.Toggle(_audio.Current);
            };

            OpenButton = new Button { name = "poi-card-mini-open" };
            OpenButton.AddToClassList("poi-card-mini__open");
            Title = new Label { pickingMode = PickingMode.Ignore };
            Title.AddToClassList("poi-card-mini__title");
            TimeText = new Label { pickingMode = PickingMode.Ignore };
            TimeText.AddToClassList("poi-card-mini__time");
            OpenButton.Add(Title);
            OpenButton.Add(TimeText);
            OpenButton.clicked += () =>
            {
                var current = _audio.Current;
                if (current != null) OpenRequested?.Invoke(current.PoiId);
            };

            var bar = new VisualElement { pickingMode = PickingMode.Ignore };
            bar.AddToClassList("poi-card-mini__bar");
            BarFill = new VisualElement { pickingMode = PickingMode.Ignore };
            BarFill.AddToClassList("poi-card-mini__fill");
            bar.Add(BarFill);

            Root.Add(PlayButton);
            Root.Add(OpenButton);
            Root.Add(bar);
            Root.style.display = DisplayStyle.None;
            layer.Add(Root);
        }

        public bool IsShown => Root.style.display == DisplayStyle.Flex;

        // The words of the card the audio came from (its language): the tooltips and accessible names
        public void SetStrings(CardStrings strings) => _strings = strings;

        // Show or hide the bar, and draw the audio as it stands
        public void Refresh(bool visible)
        {
            Root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) return;
            var current = _audio.Current;
            bool playing = _audio.State == CardAudioState.Playing;
            PlayGlyph.Kind = playing ? CardIcons.Shape.Pause : CardIcons.Shape.Play;
            PlayButton.tooltip = _strings?.Get(playing ? CardStrings.Keys.AudioPause : CardStrings.Keys.AudioPlay) ?? "";
            OpenButton.tooltip = _strings?.Get(CardStrings.Keys.MiniPlayerOpen) ?? "";
            Title.text = current?.Title ?? "";
            float position = _audio.Position, length = _audio.Length;
            TimeText.text = length > 0f ? AudioTimeRule.Format(position) + " / " + AudioTimeRule.Format(length) : AudioTimeRule.Format(position);
            BarFill.style.width = Length.Percent(AudioTimeRule.Fraction(position, length) * 100f);
        }
    }
}
