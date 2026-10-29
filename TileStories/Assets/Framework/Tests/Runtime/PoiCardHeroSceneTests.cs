using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TileStories.LookKit;
using TileStories.Tests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // PoiCardHeroSceneTests's shared ground: its set-up and helpers, so its own tests and its old-navigation ones stand on one fixture
    public abstract class PoiCardHeroSceneFixture : SearchSceneFixture
    {
        protected PoiCardSheetView Sheet => Card.Sheet;

        protected BlockStackView Stack => Sheet.Stack;

        protected HeaderBlockView Header => Stack.HeaderView;

        protected ScrollView Scroll => Stack.Scroll;

        protected float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        protected const string Domain = "Hero";

        // The hero's words over a picture read at 4.5:1 (the project's contrast policy for words over a picture; the cloud is worked out for it)
        protected const float HeroWordsMinRatio = HeroWordsOnPicture.WordsRatio;

        protected ManualAudioOutput _out;

        protected CardAudioService _audio;

        [UnitySetUp]
        public IEnumerator UseASilentAudioDevice()
        {
            _out = new ManualAudioOutput();
            _audio = new CardAudioService(_out, () => Card.Media, () => 0f, () => Session.CardSettings.container.audio_when_another_starts);
            Card.UseAudioService(_audio);
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        protected IEnumerator Open(string poi, string language, SheetStopRule.Stop stop)
        {
            Session.CardSettings.languages = new List<string> { language, language == "en" ? "pt" : "en" };
            CardTestInput.HoldTheWallStill(Sheet);
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "precondition: " + poi + "'s card is open");
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            yield return Rested();
        }

        protected IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
        }

        // Push an edit the way the Editor's live applier does: a private copy of the running config
        protected IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        protected static void SetHeaderField(WallConfigData config, string poi, string key, string value)
        {
            var header = config.pois.First(p => p.id == poi).card.blocks.First(b => b.kind == BuiltInBlocks.HeaderKind);
            var field = header.fields.Find(f => f.key == key);
            if (field == null) header.fields.Add(field = new BlockFieldValue { key = key });
            field.value = value;
        }

        // ---------------- the words: 4.5:1 against the picture about their letters ----------------

        protected static IEnumerable<string> HeroPictures() => new[] { "lamp", "painting", "lamp_military", "lamp_religious" };

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }
    }

    // The header's bar and hero on the REAL LivingRoomScene (_3.3 P4.4a, S4a; HeroRule / HeroMotionRule / HeroActionsRule): the bar is one
    // row whatever the language or the Menu (the stops never move); the hero is the screen -- Full's whole view, the strip waiting until the
    // visitor scrolls -- its words and actions whole at Half (the PT Lamp's "Mostra-me onde fica" included) while the sticky bar waits; real
    // taps on the hero's action and Listen do what the card's own blocks do; the drift, the zoom, the parallax and the words' rise run on
    // the test's clock and stop under Reduce Motion (frame sequences saved for the eye); the glow lies only behind the words, a fade with no
    // edge; and every word keeps 4.5:1 against the picture right about its letters on every hero picture of the wall, EN / PT, light / dark
    public class PoiCardHeroSceneTests : PoiCardHeroSceneFixture
    {
        // ---------------- the title's lines (P4.4b part 2) ----------------

        // The Editor's 3-line notice predicts the hero's lines with HeroTitleFitRule over the display face's glyph advances, on a 390 pt
        // phone's line less the foot's padding: held here against the REAL hero label -- its own resolved face and size, its own width, the
        // lines it really draws -- for titles of one, two and three lines
        [UnityTest]
        public IEnumerator TheTitleFitRule_CountsTheLinesTheRealHeroDraws_OnThePhonesLine()
        {
            var titles = new[] { "The Lamp", "The Palace of the Ribeira in Lisbon", "The Royal Ribeira Palace and its Great Earthquake Ruins" };
            float oneLine = float.NaN;
            var counted = new List<int>();
            foreach (string title in titles)
            {
                yield return Push(config =>
                {
                    var header = config.pois.First(p => p.id == "lamp").card.blocks.First(b => b.kind == BuiltInBlocks.HeaderKind);
                    header.fields.First(f => f.key == BlockStackBuilder.HeaderTitleField).text.First(t => t.lang == "en").value = title;
                });
                if (!Sheet.IsOpen) yield return Open("lamp", "en", SheetStopRule.Stop.Half);
                yield return CardTestInput.Settle(0.2f);
                var label = Header.HeroTitle;
                Assert.AreEqual(title, label.text, "precondition: the pushed title is on the hero");
                var font = label.resolvedStyle.unityFontDefinition.fontAsset;
                Assert.IsNotNull(font, "precondition: the hero's title draws in a UI Toolkit font asset");
                StringAssert.StartsWith("Fraunces", font.faceInfo.familyName, "the display face (R12.3)");
                float size = label.resolvedStyle.fontSize, width = label.contentRect.width;
                // - what the Editor assumes (HeroTitleMeasure): the narrow hero size, the 390 pt line less --ts-space-5 each side
                Assert.AreEqual(CardTestInput.TokenPx("--ts-size-hero-narrow"), size, 0.01f, "a phone's hero uses the narrow size");
                Assert.AreEqual(Sheet.Root.worldBound.width - 2f * CardTestInput.TokenPx("--ts-space-5"), width, OnePixel, "the line is the phone less the foot's padding");
                int predicted = HeroTitleFitRule.Lines(title, width, run => HeroTitleFitRule.AdvanceWidth(font, size, run));
                if (float.IsNaN(oneLine)) oneLine = label.contentRect.height;
                int drawn = Mathf.RoundToInt(label.contentRect.height / oneLine);
                Assert.AreEqual(drawn, predicted, "\"" + title + "\": the rule says " + predicted + " lines, the hero draws " + drawn
                    + " (" + label.contentRect.height + " px of " + oneLine + " per line)");
                counted.Add(drawn);
            }
            Assert.AreEqual(1, counted[0], "precondition: a one-line title");
            Assert.AreEqual(2, counted[1], "precondition: a two-line title");
            Assert.GreaterOrEqual(counted[2], HeroTitleFitRule.NoticeAtLines, "precondition: a title past the notice's threshold (" + counted[2] + " lines)");
        }

        // ---------------- Full: the whole hero is the screen, the card's actions at its foot ----------------

        // _3.3 S4a review: the hero is the whole sheet at Full (its picture runs up under the floating bars) and its words block -- Listen and
        // the card's action with it -- sits at its foot, whatever the stop (so Half shows the picture's top; Full the whole hero)
        [Category("Core")]
        [UnityTest]
        public IEnumerator AtFull_TheWholeHeroIsTheScreen_WithTheCardsActionsAtItsFoot_AndTheStickyBarWaits(
            [Values("lamp", "painting")] string poi, [Values("en", "pt")] string language)
        {
            yield return Open(poi, language, SheetStopRule.Stop.Full);
            Header.Clock = () => 0f;
            Header.Animate();
            Rect view = Scroll.contentViewport.worldBound, hero = Header.HeroPart.worldBound;
            Assert.IsTrue(Header.HasPicture, "precondition: " + poi + "'s header is a picture look");
            Assert.AreEqual(HeroRule.Height(Sheet.Stops, CardTestInput.TokenPx("--ts-hero-min-height")), hero.height, OnePixel,
                "the hero's height is the rule's");
            Assert.AreEqual(Sheet.Stops.Full, hero.height, OnePixel, "...the whole sheet at Full");
            Assert.AreEqual(Sheet.Root.worldBound.yMin, hero.yMin, OnePixel, "its top at the sheet's top: the bars float over its picture");
            Assert.AreEqual(view.yMin, hero.yMin, OnePixel, "...and the view starts there too");
            Assert.LessOrEqual(Header.Foot.worldBound.yMax, view.yMax + OnePixel, "its words and actions end inside the view: " + Header.Foot.worldBound + " in " + view);
            // - a Navigation Bar floating at the bottom from the start lies over the hero's foot: the words rise clear of it (v5)
            float barClear = NavBarRule.ClearsHeroFoot(Session.CardSettings.navigation) ? CardTestInput.TokenPx("--ts-navbar-clearance") : 0f;
            Assert.AreEqual(view.yMax - barClear, Header.Foot.worldBound.yMax, OnePixel, "...at its very foot (HeroRule.WordsBottom), clear of a bottom bar");
            Assert.AreEqual(Sheet.Root.worldBound.width, hero.width, OnePixel, "full width");
            foreach (var label in new[] { Header.Eyebrow, Header.HeroTitle, Header.HeroPart.Q<Label>("card-header-subtitle") })
                Assert.IsTrue(view.Contains(label.worldBound.min) && view.Contains(label.worldBound.max - Vector2.one * OnePixel), label.name + " wholly in view");
            Assert.IsFalse(Header.BarTitleShown, "the hero carries the title, the bar none");
            Assert.IsFalse(Sheet.Chrome.FooterShown && Sheet.Chrome.BottomShown, "the sticky bar waits for the hero");

            var actions = Stack.HeroActionViews;
            bool lamp = poi == "lamp";
            Assert.AreEqual(lamp ? 2 : 1, actions.Count, lamp ? "the sticky action and Listen" : "Listen alone (no sticky action on this card)");
            if (lamp)
            {
                var primary = (ActionsBlockView)actions[0];
                var button = primary.Actions.Single().Button;
                Assert.IsTrue(primary.Root.ClassListContains(BlockStackView.HeroPrimaryClass));
                Assert.AreEqual(language == "en" ? "Show me where it is" : "Mostra-me onde fica", primary.Actions.Single().Label.text);
                Assert.IsTrue(view.Contains(button.worldBound.min) && view.Contains(button.worldBound.max - Vector2.one * OnePixel),
                    "the card's action is on screen (the P4.3 finding: PT lost it): " + button.worldBound + " in " + view);
                Assert.Less(button.Q<Label>().worldBound.height, button.Q<Label>().resolvedStyle.fontSize * 2f, "its words on one line");
                Assert.AreEqual(1, Stack.Footer.Query<Button>(className: "card-action").ToList().Count, "the footer still holds the same action for later");
            }
            var listen = actions.OfType<AudioGuideBlockView>().Single();
            Assert.AreEqual(BuiltInBlocks.AudioGuideHeroChip, listen.Variant);
            // - P4.4b part 2, S4a: the compact pill -- headphones and the guide's length, the words in its name ("Listen, 3:11")
            StringAssert.IsMatch(@"^\d+:\d\d$", listen.Title.text, "the pill shows the guide's length, m:ss");
            Assert.AreEqual((language == "en" ? "Listen, " : "Ouvir, ") + listen.Title.text, listen.PlayButton.tooltip, "its name says Listen");
            Assert.AreEqual(DisplayStyle.Flex, listen.ListenIcon.resolvedStyle.display, "headphones on the pill");
            Assert.AreEqual(CardIcons.Listen, CardIcons.KeyOf(listen.ListenIcon));
            Assert.AreEqual(DisplayStyle.None, listen.PlayGlyph.resolvedStyle.display, "...instead of the play mark (it could read as a video)");
            Assert.IsTrue(view.Contains(listen.PlayButton.worldBound.center), "Listen is on screen");
            if (lamp)
            {
                // - ONE action row (the Architect's review of P4.4a: two stacked rows hid the Lamp's picture)
                var primary = actions[0].Root;
                var words = ((ActionsBlockView)actions[0]).Actions.Single().Label;
                Assert.AreEqual(primary.worldBound.center.y, listen.Root.worldBound.center.y, OnePixel, "the focal action and Listen share one row: "
                    + primary.worldBound + " / " + listen.Root.worldBound);
                Assert.LessOrEqual(primary.worldBound.xMax, listen.Root.worldBound.xMin + OnePixel, "Listen sits beside it, never over it");
                Assert.IsTrue(hero.Contains(primary.worldBound.min) && hero.Contains(listen.Root.worldBound.max - Vector2.one * OnePixel), "both inside the hero");
                float wants = words.MeasureTextSize(words.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;
                Assert.LessOrEqual(wants, words.contentRect.width + OnePixel, "the focal words are whole at 390 pt (" + wants + " in " + words.contentRect.width + ")");
            }
            yield return LookMeasure.Render(Domain, "check", $"full_{poi}_{language}", Sheet.Root);
        }

        // ---------------- real taps ----------------

        [Category("Core")]
        [UnityTest]
        public IEnumerator ARealTapOnTheHerosAction_ShowsThePointOnTheWall_TheCardLowersAndTheSelectionStays()
        {
            // - at Full: the words block, the action with it, sits at the hero's foot (S4a review)
            yield return Open("lamp", "en", SheetStopRule.Stop.Full);
            var button = ((ActionsBlockView)Stack.HeroActionViews[0]).Actions.Single().Button;
            yield return CardTestInput.Tap(button.panel, button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "Show me where it is: the card lowers to Peek");
            Assert.AreEqual("lamp", SelectionEventBus.CurrentPoiId, "the selection stays");
            Assert.IsTrue(Header.BarTitleShown, "and the bar carries the title");
        }

        [UnityTest]
        public IEnumerator ARealTapOnListen_PlaysTheCardsGuide_AndAgainPausesIt()
        {
            yield return Open("painting", "en", SheetStopRule.Stop.Full);
            var listen = Stack.HeroActionViews.OfType<AudioGuideBlockView>().Single();
            var player = Stack.BoundViews.OfType<AudioGuideBlockView>().Single();
            Assert.AreEqual(player.Track.ClipPath, listen.Track.ClipPath, "precondition: Listen is The Painting's own guide (its player's clip)");
            yield return CardTestInput.Tap(listen.PlayButton.panel, listen.PlayButton.worldBound.center);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "a real tap plays");
            Assert.IsTrue(_audio.IsCurrent(player.Track), "the card's guide: one audio, drawn by the player too");
            // - P4.4b part 2: the compact pill shows the running time alone while it plays, and its name becomes Pause
            Assert.AreEqual(AudioTimeRule.Format(_audio.Position), listen.Title.text, "playing: the running time on the pill");
            Assert.AreEqual(DisplayStyle.None, listen.TimeText.resolvedStyle.display, "one time, never a second line");
            Assert.AreEqual("Pause", listen.PlayButton.tooltip, "its name while it plays");
            yield return CardTestInput.Tap(listen.PlayButton.panel, listen.PlayButton.worldBound.center);
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "a second tap pauses");
        }

        // ---------------- motion ----------------

        [UnityTest]
        public IEnumerator ThePan_CrossesTheWholePictureOnTheClock_ReduceMotionHoldsItsCentre_ZoomAndNone()
        {
            yield return Open("lamp", "en", SheetStopRule.Stop.Half);
            Assert.AreEqual(HeroMotionRule.PanZoom, Header.Motion, "Hero Motion's default: pan and zoom (_3.3 S4a)");
            // - the pan alone first (a zoom scales the drawn picture, which this measures in place)
            yield return Push(c => SetHeaderField(c, "lamp", BuiltInBlocks.HeaderHeroMotionField, HeroMotionRule.Pan));
            float t = 0f;
            Header.Clock = () => t;
            Assert.AreEqual(HeroMotionRule.Pan, Header.Motion, "precondition: the pan pushed live");
            Assert.IsTrue(Header.Moving);
            Assert.Greater(Header.Overflow, 20f, "precondition: castle_hero is wider than the hero (" + Header.Picture.Aspect + ")");
            float frame = Header.Frame.worldBound.width;
            Assert.AreEqual(frame + Header.Overflow, Header.Picture.Root.worldBound.width, OnePixel, "the picture as wide as its proportions ask");

            float pictureLeft0 = 0f;
            var steps = new[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
            float period = HeroMotionRule.PeriodFor(Header.Overflow);
            for (int i = 0; i < steps.Length; i++)
            {
                t = steps[i] * period;
                Header.Animate();
                yield return null;
                yield return null;
                float expected = HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, t, Header.Overflow);
                Assert.AreEqual(expected, Header.PanOffset, 0.01f, "the rule's offset at " + t + " s");
                float left = Header.Picture.Root.worldBound.xMin - Header.Frame.worldBound.xMin;
                if (i == 0) pictureLeft0 = left;
                Assert.AreEqual(expected, left - pictureLeft0 + 0f, OnePixel + 0.5f, "drawn where the rule says (transform only) at " + t + " s");
                Assert.LessOrEqual(Header.Picture.Root.worldBound.xMin, Header.Frame.worldBound.xMin + OnePixel, "the frame never shows an empty left edge");
                Assert.GreaterOrEqual(Header.Picture.Root.worldBound.xMax, Header.Frame.worldBound.xMax - OnePixel, "...nor an empty right edge");
                yield return LookMeasure.Render(Domain, "frames", $"pan_{i}_{t:0.0}s", Header.HeroPart);
            }
            Assert.AreEqual(-Header.Overflow, HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, period / 2f, Header.Overflow), 0.01f);
            Assert.LessOrEqual(Header.Overflow / (period / 2f), HeroMotionRule.MaxPanSpeed + 0.01f, "the full-screen crop drifts slowly: " + Header.Overflow + " pt in " + period / 2f + " s");

            // - Reduce Motion (live): the picture holds its centre, nothing is scheduled
            yield return Push(c => c.card_settings.container.reduce_motion = true);
            Header.Clock = () => t;
            Assert.IsFalse(Header.Moving, "reduce motion: the picture does not move by itself");
            foreach (float at in new[] { 3f, 9f })
            {
                t = at;
                Header.Animate();
                Assert.AreEqual(-Header.Overflow / 2f, Header.PanOffset, 0.01f, "its centre, still, at " + at + " s");
            }
            yield return LookMeasure.Render(Domain, "frames", "pan_reduce_motion", Header.HeroPart);

            // - Zoom and None (the header's own Hero Motion, live)
            yield return Push(c => { c.card_settings.container.reduce_motion = false; SetHeaderField(c, "lamp", BuiltInBlocks.HeaderHeroMotionField, HeroMotionRule.Zoom); });
            Header.Clock = () => t;
            Assert.AreEqual(HeroMotionRule.Zoom, Header.Motion);
            t = HeroMotionRule.PanPeriod / 2f;
            Header.Animate();
            Assert.AreEqual(HeroMotionRule.ZoomMax, Header.ZoomScale, 0.001f, "zoom: pushed in half way through");
            Assert.AreEqual(-Header.Overflow / 2f, Header.PanOffset, 0.01f, "...around the centre");
            yield return null;
            Assert.AreEqual(HeroMotionRule.ZoomMax, Header.Media.resolvedStyle.scale.value.x, 0.001f, "drawn as a scale");
            // - Pan And Zoom (the default, live): half way through its period the picture is at its far side AND pushed in
            yield return Push(c => SetHeaderField(c, "lamp", BuiltInBlocks.HeaderHeroMotionField, HeroMotionRule.PanZoom));
            Header.Clock = () => t;
            Assert.IsTrue(Header.Moving, "pan and zoom moves by itself");
            t = HeroMotionRule.PeriodFor(Header.Overflow) / 2f;
            Header.Animate();
            Assert.AreEqual(-Header.Overflow, Header.PanOffset, 0.01f, "pan and zoom: at the far side half way through");
            Assert.AreEqual(HeroMotionRule.ZoomMax, Header.ZoomScale, 0.001f, "...and pushed in");
            yield return null;
            Assert.AreEqual(HeroMotionRule.ZoomMax, Header.Media.resolvedStyle.scale.value.x, 0.001f, "drawn as a scale too");
            yield return Push(c => SetHeaderField(c, "lamp", BuiltInBlocks.HeaderHeroMotionField, HeroMotionRule.None));
            Assert.IsFalse(Header.Moving, "none: still");
            Header.Animate();
            Assert.AreEqual(1f, Header.ZoomScale, 0.001f);
        }

        [UnityTest]
        public IEnumerator ScrollingTheCard_RaisesAndFadesTheWords_AndLagsThePicture_AndTheBarTakesTheTitle()
        {
            yield return Open("painting", "en", SheetStopRule.Stop.Full);
            Header.Clock = () => 0f;
            Header.Animate();
            var offsets = new[] { 0f, 60f, 120f, 180f, 240f, 300f };
            for (int i = 0; i < offsets.Length; i++)
            {
                Scroll.scrollOffset = new Vector2(0f, offsets[i]);
                yield return null;
                yield return null;
                float y = Scroll.scrollOffset.y;
                Assert.AreEqual(offsets[i], y, 0.5f, "precondition: the stack scrolls that far");
                Assert.AreEqual(HeroMotionRule.TextRise(y, false), Header.TextRise, 0.01f, "the words rise at " + y);
                // - the words block (Listen with it) fades as one, by its place in the view (HeroMotionRule.TextOpacity)
                Assert.AreEqual(Header.TextOpacity, Header.Foot.resolvedStyle.opacity, 0.01f, "...and fade as one block");
                if (i == 0) Assert.AreEqual(1f, Header.TextOpacity, 0.01f, "at rest the words, low on the picture, are whole");
                Assert.AreEqual(HeroMotionRule.Parallax(y, Header.Frame.layout.height, false), Header.ParallaxOffset, 0.01f, "the picture lags");
                Assert.AreEqual(HeroRule.BarShowsTitle(Sheet.Stop, y, Header.HeroTitleBottom()), Header.BarTitleShown, "the bar takes the title exactly when the rule says, at " + y);
                yield return LookMeasure.Render(Domain, "frames", $"scroll_{i}_{offsets[i]:0}", Sheet.Root);
            }
            // - the words sit at the foot of the picture since S4a's review: the title leaves the view once the hero is scrolled past
            Scroll.scrollOffset = new Vector2(0f, Header.HeroHeight);
            yield return null;
            yield return null;
            Assert.IsTrue(Header.BarTitleShown, "scrolled past the hero: its title is gone, the bar carries it");
            yield return Push(c => c.card_settings.container.reduce_motion = true);
            Scroll.scrollOffset = new Vector2(0f, 120f);
            yield return null;
            yield return null;
            Assert.AreEqual(1f, Header.Foot.resolvedStyle.opacity, 0.01f, "reduce motion: the words stay whole");
            Assert.AreEqual(0f, Header.TextRise, 0.01f, "...and still");
            Assert.AreEqual(0f, Header.ParallaxOffset, 0.01f, "...the picture too");
        }

        // _3.3 S4a: every hero picture of the test wall, in English and Portuguese, light and dark -- each word of the hero against the picture
        // RIGHT AROUND ITS LETTERS (a ring 2-3 px about its strokes, as drawn: where the halo and the glow darken the picture), at the worst
        // such pixel
        [UnityTest]
        public IEnumerator EveryWordOfTheHero_ReadsAgainstThePictureRightAroundItsLetters([ValueSource(nameof(HeroPictures))] string poi,
            [Values("en", "pt")] string language, [Values("light", "dark")] string mode,
            [Values(SheetStopRule.Stop.Half, SheetStopRule.Stop.Full)] SheetStopRule.Stop stop)
        {
            // - the looks that move by themselves are measured still: the video loop on its poster, the turntable at rest (Reduce Motion);
            //   the image parallax across its drift (start, a quarter, the far side)
            bool pans = poi == "lamp" || poi == "painting";
            if (!pans) yield return Push(c => c.card_settings.container.reduce_motion = true);
            ThemeController.SetMode(mode);
            yield return Open(poi, language, stop);
            Assert.IsTrue(Header.HasPicture, "precondition: " + poi + " has a hero picture");
            float t = 0f;
            Header.Clock = () => t;
            var words = new List<Label> { Header.Eyebrow, Header.HeroTitle, Header.HeroPart.Q<Label>("card-header-subtitle") };
            words.AddRange(Header.Actions.Query<Label>(className: "card-audio__title").ToList());
            words.AddRange(Header.Actions.Query<Label>(className: "card-action__label").ToList());
            words = words.Where(l => l.text.Length > 0 && l.resolvedStyle.display == DisplayStyle.Flex).ToList();
            Assert.GreaterOrEqual(words.Count, 1, "precondition: words to judge (at least the title)");
            var report = new List<string>();
            float worstOfAll = float.MaxValue;
            string worstWhat = "";
            float period = HeroMotionRule.PeriodFor(Header.Overflow);
            foreach (float share in pans ? new[] { 0f, 0.25f, 0.5f } : new[] { 0f })
            {
                t = share * period;
                Header.Animate();
                var colours = words.ToDictionary(l => l, l => l.resolvedStyle.color);
                Texture2D drawn = null, under = null;
                try
                {
                    yield return ThemePixelSet.GrabStill($"words_{poi}_{language}_{mode}_{stop}_{share}", null, r => drawn = r);
                    // - the letters made clear, their halo kept: what is under and around each letter
                    foreach (var l in words) l.style.color = new Color(colours[l].r, colours[l].g, colours[l].b, 0f);
                    yield return ThemePixelSet.GrabStill($"under_{poi}_{language}_{mode}_{stop}_{share}", null, r => under = r);
                    foreach (var l in words)
                    {
                        var area = ThemePixelSet.ScreenRectOf(l, drawn);
                        Assert.Greater(area.width * area.height, 0, "precondition: " + l.name + " covers pixels");
                        // - each word against the picture RIGHT AROUND its letters, as drawn (the shade and the halo are part of it): the
                        //   worst pixel of the ring 2-3 px out from its strokes (ThemePixelSet.RingContrast)
                        var (ratio, worst, letters) = ThemePixelSet.RingContrast(drawn, under, area, colours[l]);
                        Assert.Greater(letters, 0, "precondition: '" + l.text + "' draws letters");
                        string what = l.name.Length > 0 ? l.name : l.ClassListContains("card-audio__title") ? "listen" : "action";
                        if (ratio < worstOfAll) { worstOfAll = ratio; worstWhat = $"{what} '{l.text}' at drift {share:0.00}"; }
                        report.Add($"{poi} {language} {mode} {stop} drift {share:0.00} {what}: {ratio:0.00}:1 (worst pixel about its letters {(Color32)worst})");
                    }
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain + "/words", $"{poi}_{language}_{mode}_{stop}_{share:0.00}.png"), drawn.EncodeToPNG());
                }
                finally
                {
                    foreach (var l in words) l.style.color = StyleKeyword.Null;
                    if (drawn != null) Object.Destroy(drawn);
                    if (under != null) Object.Destroy(under);
                }
            }
            System.IO.File.AppendAllText(TestEvidence.PathFor(Domain + "/words", "ratios.txt"), string.Join("\n", report) + "\n");
            Debug.Log("[Card] Hero words " + poi + " " + language + " " + mode + " " + stop + " strength " + Header.Words.Strength.ToString("0.00") + ": worst " + worstOfAll.ToString("0.00") + ":1 (" + worstWhat + ")");
            Assert.GreaterOrEqual(worstOfAll, HeroWordsMinRatio, "every word of the hero keeps 4.5:1 against the picture about its letters -- worst: " + worstWhat);
        }

        // What Words On Picture's layer laid on the picture, read off two renders of the same hero (with / without the layer, the words and the
        // bar out of both): per pixel the alpha the panel blended (linear light: A = (bare - laid) / (bare - shade), where the bare picture is
        // bright enough to tell) -- at the words (each word grown by the ring a reader sees), next to what the layer's own rule says it draws
        // there, and over the hero's far side (its top-right quarter as seen); the bare picture's mean brightness at the words
        protected sealed class ToneMeasure
        {
            public float Strength, AtWords, Expected, FarSide, Ground;
            // How many of the hero's pixels the layer visibly darkens (a laid alpha over 0.05, where the bare picture can tell)
            public int Words, Far, Laid;
        }

        protected IEnumerator MeasureTone(string poi, SheetStopRule.Stop stop, System.Action<ToneMeasure> result)
        {
            var layer = Header.Words.Painter.Options;
            var m = new ToneMeasure { Strength = Header.Words.Strength };
            float shadeY = layer != null ? ToneRule.Luminance(layer.Colour) : 0f;
            var box = Header.ToneLayer.worldBound;
            var words = Header.Words.Inks.Where(i => i.IsWord).Select(i => new Rect(i.Rect.position + box.position, i.Rect.size)).ToList();
            Assert.GreaterOrEqual(words.Count, 1, "precondition: the hero's words hand their ink");
            var hero = Header.HeroPart.worldBound;
            Texture2D with = null, without = null;
            var labels = Header.HeroPart.Query<Label>().ToList();
            try
            {
                foreach (var l in labels) l.style.visibility = Visibility.Hidden;
                Sheet.Slots.Bar.Root.style.visibility = Visibility.Hidden;
                Header.Actions.style.visibility = Visibility.Hidden;
                yield return ThemePixelSet.GrabStill("tone_" + poi, Header.HeroPart, r => with = r);
                Header.ToneLayer.style.visibility = Visibility.Hidden;
                yield return ThemePixelSet.GrabStill("notone_" + poi, Header.HeroPart, r => without = r);
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/tone", $"{poi}_{stop}_with.png"), with.EncodeToPNG());
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/tone", $"{poi}_{stop}_without.png"), without.EncodeToPNG());

                // - a render pixel's centre in the panel (the render is the hero cut to the screen, rows bottom-up)
                float perPixel = ThemePixelSet.PanelUnitsPerPixel(Header.HeroPart);
                float renderBottom = hero.yMin + with.height * perPixel, seenHeight = with.height * perPixel;
                Vector2 Panel(int x, int y) => new(hero.xMin + (x + 0.5f) * perPixel, renderBottom - (y + 0.5f) * perPixel);
                float ring = HeroWordsOnPicture.Ring;
                Color32[] a = with.GetPixels32(), b = without.GetPixels32();
                double atWords = 0, expected = 0, far = 0, ground = 0;
                int read = 0;
                for (int y = 0; y < with.height; y++)
                    for (int x = 0; x < with.width; x++)
                    {
                        int i = y * with.width + x;
                        var p = Panel(x, y);
                        float bare = ToneRule.Luminance(b[i]), laid = ToneRule.Luminance(a[i]);
                        bool readable = bare - shadeY > 0.05f;
                        float alpha = readable ? Mathf.Clamp01((bare - laid) / (bare - shadeY)) : 0f;
                        if (alpha > 0.05f) m.Laid++;
                        if (words.Any(r => p.x >= r.xMin - ring && p.x <= r.xMax + ring && p.y >= r.yMin - ring && p.y <= r.yMax + ring))
                        {
                            m.Words++;
                            ground += bare;
                            if (readable)
                            {
                                read++;
                                atWords += alpha;
                                expected += layer != null ? ToneRule.AlphaAt(layer, p - box.position) : 0f;
                            }
                        }
                        else if (p.x > hero.xMin + hero.width * 0.75f && p.y < hero.yMin + seenHeight * 0.25f && readable)
                        {
                            m.Far++;
                            far += alpha;
                        }
                    }
                m.AtWords = read > 0 ? (float)(atWords / read) : 0f;
                m.Expected = read > 0 ? (float)(expected / read) : 0f;
                m.FarSide = m.Far > 0 ? (float)(far / m.Far) : float.NaN;
                m.Ground = m.Words > 0 ? (float)(ground / m.Words) : float.NaN;
                System.IO.File.AppendAllText(TestEvidence.PathFor("Premium/P3c/tone", "measures.txt"),
                    $"{poi} {stop} {Header.Words.Mode}: strength {m.Strength:0.000}, laid at the words {m.AtWords:0.000} (the rule's {m.Expected:0.000}) " +
                    $"over {read} px, far side {m.FarSide:0.0000} over {m.Far} px, bare ground {m.Ground:0.000}\n");
            }
            finally
            {
                Header.ToneLayer.style.visibility = StyleKeyword.Null;
                foreach (var l in labels) l.style.visibility = StyleKeyword.Null;
                Header.Actions.style.visibility = StyleKeyword.Null;
                Sheet.Slots.Bar.Root.style.visibility = StyleKeyword.Null;
                if (with != null) Object.Destroy(with);
                if (without != null) Object.Destroy(without);
            }
            result(m);
        }

        // The P3c addendum's Words On Picture at its default, Tone + Halo: the words wear the halo (a close shade and a glow); the tone the
        // panel lays at the words is the one its rule draws at the strength the hero worked out (+-0.06: 8-bit renders); over the hero's far
        // side it lays ~nothing (light falling off, no edge) -- at FULL, where the hero is the screen (the P3c review, second pass a: at Half
        // the visible strip is mostly words and what tone remains may reach its top; Half's own check is the next test). Renders and measures
        // in TestEvidence/Premium/P3c/tone/
        [UnityTest]
        public IEnumerator TheTone_IsDrawnAtTheWorkedOutStrength_AtTheWords_AndIsNothingAtTheFarSide([Values("lamp", "painting")] string poi,
            [Values(SheetStopRule.Stop.Full)] SheetStopRule.Stop stop)
        {
            yield return Open(poi, "en", stop);
            Header.Clock = () => 0f;
            Header.Animate();
            yield return null;
            Assert.AreEqual(HeroWordsRule.ToneHalo, Header.WordsOnPicture, "precondition: the default look");
            Assert.IsFalse(Header.HeroPart.ClassListContains(HeroWordsOnPicture.NoHaloClass), "Tone + Halo: the words wear the halo");
            Assert.Greater(Header.HeroTitle.resolvedStyle.unityTextOutlineWidth, 0f, "...its close shade (the outline)");
            Assert.AreEqual(0f, Header.Foot.resolvedStyle.backgroundColor.a, 0.001f, "no plate behind the words");
            var layer = Header.Words.Painter.Options;
            Assert.IsNotNull(layer, "precondition: the tone is drawn");
            Assert.AreEqual(ToneShape.Radial, layer.Shape);
            Assert.AreEqual(Header.Words.Strength, layer.Strength, 1e-5f, "the painter draws the strength the hero worked out");
            ToneMeasure m = null;
            yield return MeasureTone(poi, stop, r => m = r);
            Assert.Greater(m.Words, 200, "precondition: picture under the words");
            Assert.AreEqual(m.Expected, m.AtWords, 0.06f, $"{poi} {stop}: at the words the panel lays what the tone's rule draws");
            Assert.Greater(m.Far, 500, "precondition: picture on the far side");
            Assert.Less(m.FarSide, 0.01f, $"{poi} {stop}: the tone's alpha at the far side ~0 ({m.FarSide:0.0000})");
        }

        // The halo's second layer (the P3c review, second pass a): a back label under EVERY word of the hero -- the same words, in the same
        // box (+-1 px), inside the tone layer (so it rises and fades with the words) -- and what the two layers lay in the ring a reader sees
        // round the letters (2-3 px out, ThemePixelSet.RingContrast's ring), MEASURED on the render under Halo Only (no tone darkening the
        // picture there): the halo on against the halo off, the letters made clear in both, on The Lamp and The Painting at Full over their
        // bright pictures. Round the title it takes a visible share off the ring's brightest pixel (measured 0.10-0.26); round the small
        // words it reaches ~2 px only (the font's SDF reach), so the tone never counts on it: every word's number is logged
        // (TestEvidence/Premium/P3c/halo/cover.txt). Halo Only keeps the second layer; Edge Scrim draws none
        [UnityTest]
        public IEnumerator TheHalo_IsTwoLayers_ABackLabelUnderEveryWord_AndOnTheRenderItDarkensTheRingRoundTheTitle([Values("lamp", "painting")] string poi)
        {
            yield return Open(poi, "en", SheetStopRule.Stop.Full);
            Header.Clock = () => 0f;
            Header.Animate();
            yield return null;
            yield return null;
            var words = new List<Label>();
            InkRects.ShownLabels(Header.Text, words);
            InkRects.ShownLabels(Header.Actions, words);
            Assert.GreaterOrEqual(words.Count, 2, "precondition: the hero's words (at least the eyebrow and the title)");
            var backs = Header.Words.HaloLabels;
            Assert.AreEqual(words.Count, backs.Count, "a back label under every word");
            Assert.AreEqual(Header.ToneLayer, Header.Words.HaloLayer.parent, "...in the tone layer: it rises and fades with the words");
            for (int i = 0; i < words.Count; i++)
            {
                var front = words[i];
                var back = backs[i];
                Assert.AreEqual(front.text, back.text, "the same words");
                Assert.IsTrue(back.ClassListContains(HeroWordsOnPicture.HaloClass), "...drawn as the halo");
                Assert.AreEqual(CardTestInput.TokenPx("--ts-text-halo-wide-width"), back.resolvedStyle.unityTextOutlineWidth, 0.01f,
                    "'" + front.text + "': the back label's close outline");
                Assert.AreEqual(front.resolvedStyle.fontSize, back.resolvedStyle.fontSize, 0.01f, "'" + front.text + "': the same size");
                Assert.AreEqual(front.resolvedStyle.unityFontDefinition.fontAsset, back.resolvedStyle.unityFontDefinition.fontAsset, "...the same face");
                var box = front.LocalToWorld(front.contentRect);
                // - its CONTENT box (its own box is grown by the halo's reach, so a cut word clips past its glow)
                var under = back.LocalToWorld(back.contentRect);
                Assert.AreEqual(box.xMin, under.xMin, OnePixel, "'" + front.text + "': the back label lies under the word");
                Assert.AreEqual(box.yMin, under.yMin, OnePixel);
                Assert.AreEqual(box.width, under.width, OnePixel);
                Assert.AreEqual(box.height, under.height, OnePixel);
            }

            // - the cover is the halo's own: read under Halo Only (no tone darkening the picture at the words), the same words
            yield return Push(c => SetHeaderField(c, poi, BuiltInBlocks.HeaderWordsOnPictureField, HeroWordsRule.HaloOnly));
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            yield return Open(poi, "en", SheetStopRule.Stop.Full);
            Header.Clock = () => 0f;
            Header.Animate();
            yield return null;
            Assert.IsNull(Header.Words.Painter.Options, "precondition: Halo Only lays no tone");
            words.Clear();
            InkRects.ShownLabels(Header.Text, words);
            InkRects.ShownLabels(Header.Actions, words);
            Assert.AreEqual(words.Count, Header.Words.HaloLabels.Count, "Halo Only: a back label under every word");
            // - three renders: as drawn (to find the letters), the letters clear with the halo (both layers), the letters clear without it
            var colours = words.ToDictionary(l => l, l => l.resolvedStyle.color);
            Texture2D drawn = null, haloed = null, bare = null;
            var report = new List<string>();
            float title = float.NaN;
            try
            {
                yield return ThemePixelSet.GrabStill($"halo_{poi}", null, r => drawn = r);
                foreach (var l in words) l.style.color = new Color(colours[l].r, colours[l].g, colours[l].b, 0f);
                yield return ThemePixelSet.GrabStill($"halo_clear_{poi}", null, r => haloed = r);
                Header.HeroPart.AddToClassList(HeroWordsOnPicture.NoHaloClass);
                yield return null;
                yield return ThemePixelSet.GrabStill($"halo_off_{poi}", null, r => bare = r);
                Assert.IsTrue(CardTokenColours.TryResolve(Header.HeroPart, "--ts-text-halo", out Color halo), "precondition: the theme's halo colour");
                float haloY = ToneRule.Luminance(new Color(halo.r, halo.g, halo.b, 1f));
                foreach (var l in words)
                {
                    var (cover, ring) = RingCover(drawn, haloed, bare, ThemePixelSet.ScreenRectOf(l, drawn), colours[l], haloY);
                    Assert.Greater(ring, 20, "precondition: '" + l.text + "' has a ring of picture bright enough to read the halo on");
                    report.Add($"{poi} Full '{l.text}': the halo takes {cover:0.000} off the ring's brightest pixel ({ring} px in the ring)");
                    if (l == Header.HeroTitle) title = cover;
                }
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/halo", $"{poi}_Full_halo.png"), drawn.EncodeToPNG());
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/halo", $"{poi}_Full_halo_clear.png"), haloed.EncodeToPNG());
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/halo", $"{poi}_Full_halo_off.png"), bare.EncodeToPNG());
            }
            finally
            {
                Header.HeroPart.RemoveFromClassList(HeroWordsOnPicture.NoHaloClass);
                foreach (var l in words) l.style.color = StyleKeyword.Null;
                if (drawn != null) Object.Destroy(drawn);
                if (haloed != null) Object.Destroy(haloed);
                if (bare != null) Object.Destroy(bare);
            }
            System.IO.File.AppendAllText(TestEvidence.PathFor("Premium/P3c/halo", "cover.txt"), string.Join("\n", report) + "\n");
            Debug.Log("[Card] Halo cover " + poi + ": the title " + title.ToString("0.000") + "\n" + string.Join("\n", report));
            Assert.IsFalse(float.IsNaN(title), "precondition: the title was measured");
            // - a twentieth: five times an 8-bit render's noise (~0.01), under the 0.10-0.26 measured round the title across runs (the pan's
            //   start frame lands the brightest texel a pixel either way)
            Assert.GreaterOrEqual(title, 0.05f, "the two-layer halo darkens the ring round the title's letters\n" + string.Join("\n", report));

            // - Edge Scrim draws no second layer
            yield return Push(c => SetHeaderField(c, poi, BuiltInBlocks.HeaderWordsOnPictureField, HeroWordsRule.EdgeScrim));
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            yield return Open(poi, "en", SheetStopRule.Stop.Full);
            Assert.AreEqual(HeroWordsRule.EdgeScrim, Header.WordsOnPicture, "precondition: the look pushed live");
            Assert.AreEqual(DisplayStyle.None, Header.Words.HaloLayer.resolvedStyle.display, "Edge Scrim: no second layer");
            Assert.AreEqual(0, Header.Words.HaloLabels.Count, "...and no back label");
        }

        // What the halo takes off the BRIGHTEST pixel of the ring 2-3 px round the letters of one word (the ring of ThemePixelSet.RingContrast:
        // the letters found where `drawn` differs from `haloed` toward the ink) -- the pixel a word's contrast is judged on, and the one the
        // tone's strength is worked out for (the brightest picture under the word): the drawn alpha of the halo's colour (luminance `haloY`)
        // that brings the ring's brightest bare pixel (`bare`) down to its brightest haloed one (`haloed`). A ring pixel the glow barely
        // reaches over a dark part of the picture never decides it
        private static (float Cover, int Ring) RingCover(Texture2D drawn, Texture2D haloed, Texture2D bare, RectInt area, Color ink, float haloY)
        {
            Color32[] a = drawn.GetPixels32(), h = haloed.GetPixels32(), b = bare.GetPixels32();
            Color32 ink32 = ink;
            int ToInk(Color32 c) => Mathf.Abs(c.r - ink32.r) + Mathf.Abs(c.g - ink32.g) + Mathf.Abs(c.b - ink32.b);
            var inked = new bool[area.width * area.height];
            for (int y = 0; y < area.height; y++)
                for (int x = 0; x < area.width; x++)
                {
                    int i = (area.y + y) * drawn.width + area.x + x;
                    bool changed = Mathf.Abs(a[i].r - h[i].r) + Mathf.Abs(a[i].g - h[i].g) + Mathf.Abs(a[i].b - h[i].b) > 60;
                    if (changed && ToInk(a[i]) < ToInk(h[i]) - 60) inked[y * area.width + x] = true;
                }
            bool Within(int x, int y, int reach)
            {
                for (int dy = -reach; dy <= reach; dy++)
                    for (int dx = -reach; dx <= reach; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < area.width && ny < area.height && inked[ny * area.width + nx]) return true;
                    }
                return false;
            }
            float brightestBare = 0f, brightestHaloed = 0f;
            int ring = 0;
            for (int y = 0; y < area.height; y++)
                for (int x = 0; x < area.width; x++)
                {
                    if (inked[y * area.width + x] || Within(x, y, 1) || !Within(x, y, 3)) continue;
                    int i = (area.y + y) * drawn.width + area.x + x;
                    ring++;
                    brightestBare = Mathf.Max(brightestBare, ToneRule.Luminance((Color)b[i]));
                    brightestHaloed = Mathf.Max(brightestHaloed, ToneRule.Luminance((Color)h[i]));
                }
            // - a ring with no bright picture to tell the halo on (0.1 over the halo's own luminance) proves nothing: no ring
            if (brightestBare - haloY < 0.1f) return (0f, 0);
            return (Mathf.Clamp01((brightestBare - brightestHaloed) / (brightestBare - haloY)), ring);
        }

        // The look's options, each drawing what it says (Words On Picture pushed live, The Painting at Full -- where the far-side check
        // holds): Edge Scrim -- no halo (no glow, no outline) and a scrim rising from the words' foot (darker at the words than over the far
        // side); Halo Only -- the halo and nothing on the picture (the layer draws nothing: the two renders are the same)
        [UnityTest]
        public IEnumerator EveryWordsOnPictureLook_DrawsWhatItSays()
        {
            foreach (string look in new[] { HeroWordsRule.EdgeScrim, HeroWordsRule.HaloOnly })
            {
                yield return Push(c => SetHeaderField(c, "painting", BuiltInBlocks.HeaderWordsOnPictureField, look));
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open("painting", "en", SheetStopRule.Stop.Full);
                Header.Clock = () => 0f;
                Header.Animate();
                yield return null;
                Assert.AreEqual(look, Header.WordsOnPicture, "the look pushed live");
                ToneMeasure m = null;
                yield return MeasureTone("painting_" + look, SheetStopRule.Stop.Full, r => m = r);
                if (look == HeroWordsRule.EdgeScrim)
                {
                    Assert.IsTrue(Header.HeroPart.ClassListContains(HeroWordsOnPicture.NoHaloClass), "Edge Scrim: no halo");
                    Assert.AreEqual(0f, Header.HeroTitle.resolvedStyle.unityTextOutlineWidth, 0.001f, "...no close shade");
                    Assert.AreEqual(ToneShape.Edge, Header.Words.Painter.Options.Shape, "...a scrim");
                    Assert.Greater(m.AtWords, 0.1f, "the scrim darkens the picture at the words");
                    Assert.Less(m.FarSide, 0.01f, "...and nothing far above them");
                }
                else
                {
                    Assert.IsFalse(Header.HeroPart.ClassListContains(HeroWordsOnPicture.NoHaloClass), "Halo Only: the halo");
                    Assert.IsNull(Header.Words.Painter.Options, "...and no layer");
                    Assert.Less(m.AtWords, 0.01f, "nothing laid on the picture");
                }
            }
        }

        // The developer's P3c review of the renders: scrolling, the words rise faster than the picture -- the tone's darkening rises with them
        // but the layer stays over the whole hero, so the picture's foot under the risen words keeps its darkening: on the render, where the
        // layer's foot used to end (the hero's foot + the rise) the darkening runs on with no step (less than 0.08 of alpha across 8 px, a step
        // the eye sees). The Painting at Full, scrolled 140 by the stack
        [UnityTest]
        public IEnumerator Scrolling_TheToneRisesWithTheWords_AndThePicturesFootKeepsItsDarkening_NoCutEdge()
        {
            yield return Open("painting", "en", SheetStopRule.Stop.Full);
            Header.Clock = () => 0f;
            Header.Animate();
            Scroll.scrollOffset = new Vector2(0f, 140f);
            yield return null;
            yield return null;
            Assert.AreEqual(140f, Scroll.scrollOffset.y, 0.5f, "precondition: the stack scrolls that far");
            Assert.Less(Header.TextRise, -10f, "precondition: the words rose past the picture");
            var layer = Header.Words.Painter.Options;
            Assert.IsNotNull(layer, "precondition: the tone is drawn");
            Assert.AreEqual(0f, Header.ToneLayer.resolvedStyle.translate.y, 0.01f, "the layer itself never rises");
            Assert.AreEqual(Header.HeroPart.worldBound.yMax, Header.ToneLayer.worldBound.yMax, OnePixel, "...it covers the hero to its foot");
            Assert.AreEqual(Header.TextRise, Header.Words.Risen, 0.01f, "its gradient rises with the words");
            Assert.AreEqual(Header.TextRise, Header.Words.HaloLayer.resolvedStyle.translate.y, 0.01f, "...and the halo with them");

            // - two renders, words and bar out: with the layer and without it; the laid alpha down columns across the words' side
            float cut = Header.HeroPart.worldBound.yMax + Header.TextRise;
            Assert.Less(cut, Sheet.Root.worldBound.yMax - 20f, "precondition: the old cut line lies on the screen");
            float shadeY = ToneRule.Luminance(layer.Colour);
            var labels = Header.HeroPart.Query<Label>().ToList();
            Texture2D with = null, without = null;
            try
            {
                foreach (var l in labels) l.style.visibility = Visibility.Hidden;
                Sheet.Slots.Bar.Root.style.visibility = Visibility.Hidden;
                Header.Actions.style.visibility = Visibility.Hidden;
                yield return ThemePixelSet.GrabStill("scrolled_tone", null, r => with = r);
                Header.ToneLayer.style.visibility = Visibility.Hidden;
                yield return ThemePixelSet.GrabStill("scrolled_notone", null, r => without = r);
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/tone", "painting_scrolled_with.png"), with.EncodeToPNG());
                var origin = RuntimePanelUtils.ScreenToPanel(Header.HeroPart.panel, Vector2.zero);
                float unit = ThemePixelSet.PanelUnitsPerPixel(Header.HeroPart);
                float Laid(float x, float y)
                {
                    int px = Mathf.RoundToInt((x - origin.x) / unit), py = with.height - 1 - Mathf.RoundToInt((y - origin.y) / unit);
                    float bare = ToneRule.Luminance(without.GetPixel(px, py)), laid = ToneRule.Luminance(with.GetPixel(px, py));
                    return bare - shadeY > 0.05f ? Mathf.Clamp01((bare - laid) / (bare - shadeY)) : float.NaN;
                }
                var hero = Header.HeroPart.worldBound;
                int read = 0;
                foreach (float share in new[] { 0.1f, 0.2f, 0.3f, 0.4f })
                {
                    float x = hero.xMin + hero.width * share, above = Laid(x, cut - 4f), below = Laid(x, cut + 4f);
                    if (float.IsNaN(above) || float.IsNaN(below)) continue;
                    read++;
                    Assert.Less(Mathf.Abs(above - below), 0.08f, $"at {share:0.0} across: the darkening runs on past the old cut ({above:0.000} above, {below:0.000} below)");
                    Assert.Greater(below, 0.02f, $"at {share:0.0} across: the picture's foot under the risen words is still darkened");
                }
                Assert.GreaterOrEqual(read, 2, "precondition: picture bright enough to read the darkening on, round the old cut");
            }
            finally
            {
                Header.ToneLayer.style.visibility = StyleKeyword.Null;
                foreach (var l in labels) l.style.visibility = StyleKeyword.Null;
                Header.Actions.style.visibility = StyleKeyword.Null;
                Sheet.Slots.Bar.Root.style.visibility = StyleKeyword.Null;
                if (with != null) Object.Destroy(with);
                if (without != null) Object.Destroy(without);
            }
        }

        // Tone Spread (the header's field, pushed live): on The Painting at Full the least spread lays its darkening over fewer of the hero's
        // pixels than the most, both drawn at the strength the hero works out -- and at the least, every word still reads at 4.5:1 on the
        // render (the grow keeps the words in the dense part; the strength adapts)
        [UnityTest]
        public IEnumerator ToneSpread_LessDarkensLessOfThePicture_AndEveryWordStillReads()
        {
            var laid = new Dictionary<float, int>();
            foreach (float spread in new[] { HeroWordsRule.SpreadMax, HeroWordsRule.SpreadMin })
            {
                yield return Push(c =>
                {
                    var header = c.pois.First(p => p.id == "painting").card.blocks.First(b => b.kind == BuiltInBlocks.HeaderKind);
                    var field = header.fields.Find(f => f.key == BuiltInBlocks.HeaderToneSpreadField);
                    if (field == null) header.fields.Add(field = new BlockFieldValue { key = BuiltInBlocks.HeaderToneSpreadField });
                    field.number = spread;
                });
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open("painting", "en", SheetStopRule.Stop.Full);
                Header.Clock = () => 0f;
                Header.Animate();
                yield return null;
                Assert.AreEqual(spread, Header.ToneSpread, 1e-4f, "the spread pushed live");
                ToneMeasure m = null;
                yield return MeasureTone("painting_spread_" + spread.ToString("0.0"), SheetStopRule.Stop.Full, r => m = r);
                laid[spread] = m.Laid;
                Assert.AreEqual(m.Expected, m.AtWords, 0.06f, "spread " + spread + ": at the words the panel lays what the rule draws");
            }
            Debug.Log($"[Card] Tone Spread on The Painting at Full: {laid[HeroWordsRule.SpreadMin]} px darkened at {HeroWordsRule.SpreadMin}, " +
                      $"{laid[HeroWordsRule.SpreadMax]} px at {HeroWordsRule.SpreadMax}");
            Assert.Less(laid[HeroWordsRule.SpreadMin], laid[HeroWordsRule.SpreadMax], "less spread darkens less of the picture");

            // - at the least spread every word still reads (the card is open at it now)
            var words = new List<Label>();
            InkRects.ShownLabels(Header.Text, words);
            InkRects.ShownLabels(Header.Actions, words);
            var colours = words.ToDictionary(l => l, l => l.resolvedStyle.color);
            Texture2D drawn = null, under = null;
            try
            {
                yield return ThemePixelSet.GrabStill("spread_least_words", null, r => drawn = r);
                foreach (var l in words) l.style.color = new Color(colours[l].r, colours[l].g, colours[l].b, 0f);
                yield return ThemePixelSet.GrabStill("spread_least_under", null, r => under = r);
                foreach (var l in words)
                {
                    var (ratio, _, letters) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(l, drawn), colours[l]);
                    Assert.Greater(letters, 0, "precondition: '" + l.text + "' draws letters");
                    Assert.GreaterOrEqual(ratio, HeroWordsMinRatio, "at the least spread '" + l.text + "' reads " + ratio.ToString("0.00") + ":1");
                }
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/tone", "painting_spread_least_Full.png"), drawn.EncodeToPNG());
            }
            finally
            {
                foreach (var l in words) l.style.color = StyleKeyword.Null;
                if (drawn != null) Object.Destroy(drawn);
                if (under != null) Object.Destroy(under);
            }
        }

        // The P3c addendum's ADAPTIVE tone: on every hero of the wall at Half the tone is only as strong as its picture needs -- the hero whose
        // picture under the words is darkest gets less than the one whose picture is lightest; a truly dark picture gets none at all
        [UnityTest]
        public IEnumerator ADarkHero_GetsLessToneThanALightOne_TheToneIsOnlyAsStrongAsThePictureNeeds()
        {
            var measured = new List<(string Poi, ToneMeasure M)>();
            foreach (string poi in HeroPictures())
            {
                // - every picture read still (Reduce Motion: the picture at its centre, the video loop on its poster), so what lies under the
                //   words is what is compared -- a pan would carry a dark spot's words over the picture's bright parts too
                yield return Push(c => c.card_settings.container.reduce_motion = true);
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open(poi, "en", SheetStopRule.Stop.Half);
                Header.Clock = () => 0f;
                Header.Animate();
                yield return null;
                ToneMeasure m = null;
                yield return MeasureTone(poi, SheetStopRule.Stop.Half, r => m = r);
                // - a look with no still picture (the turntable) draws the most the tone gives, by design: not a picture to compare
                if (!Header.Words.ReadsPicture)
                {
                    Assert.AreEqual(1f / Header.Words.Painter.Options.Peak, m.Strength, 1e-4f, poi + ": nothing still to read, a full peak");
                    continue;
                }
                Assert.IsFalse(float.IsNaN(m.Ground), "precondition: " + poi + "'s picture under the words read");
                measured.Add((poi, m));
            }
            Assert.GreaterOrEqual(measured.Count, 2, "precondition: two heroes with a still picture to compare");
            string table = string.Join("\n", measured.Select(x => $"{x.Poi}: bare ground {x.M.Ground:0.000}, strength {x.M.Strength:0.000}, laid at the words {x.M.AtWords:0.000}"));
            var darkest = measured.OrderBy(x => x.M.Ground).First();
            var lightest = measured.OrderBy(x => x.M.Ground).Last();
            Assert.Greater(lightest.M.Ground - darkest.M.Ground, 0.05f, "precondition: the wall's heroes differ in brightness under the words\n" + table);
            Assert.Less(darkest.M.AtWords, lightest.M.AtWords, $"the dark hero ({darkest.Poi}) gets less tone than the light one ({lightest.Poi})\n" + table);
            Assert.Less(darkest.M.Strength, lightest.M.Strength, "...because it needs less\n" + table);

            // - every hero of the wall has bright spots under its words: a truly DARK picture (the night-blue fixture, pushed live as The
            //   Lamp's hero) gets no tone at all -- and its words still read past 4.5:1 on the render
            yield return Push(c =>
            {
                c.card_settings.container.reduce_motion = true;
                var image = c.pois.First(p => p.id == "lamp").card.blocks.First(b => b.kind == BuiltInBlocks.HeaderKind)
                    .fields.First(f => f.key == BuiltInBlocks.HeaderImageField);
                image.asset = "images/night_hero.png";
            });
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            yield return Open("lamp", "en", SheetStopRule.Stop.Half);
            Assert.AreEqual("images/night_hero.png", Header.Picture.Path, "precondition: the dark fixture is the hero");
            Assert.IsTrue(Header.Words.ReadsPicture, "precondition: a still picture read");
            Assert.AreEqual(0f, Header.Words.Strength, 1e-5f, "a dark picture: no tone at all\n" + table);
            ToneMeasure night = null;
            yield return MeasureTone("night", SheetStopRule.Stop.Half, r => night = r);
            Assert.Less(night.Ground, darkest.M.Ground, "precondition: darker than every hero of the wall");
            Assert.Less(night.AtWords, 0.01f, "...and the panel lays none");
            var title = Header.HeroTitle;
            var colour = title.resolvedStyle.color;
            Texture2D drawn = null, under = null;
            try
            {
                yield return ThemePixelSet.GrabStill("night_title", null, r => drawn = r);
                title.style.color = new Color(colour.r, colour.g, colour.b, 0f);
                yield return ThemePixelSet.GrabStill("night_title_under", null, r => under = r);
                var (ratio, worst, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(title, drawn), colour);
                Assert.Greater(inked, 0, "precondition: the title draws pixels");
                Assert.GreaterOrEqual(ratio, HeroWordsMinRatio, "unshaded on the dark picture the title reads " + ratio.ToString("0.00") + ":1");
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3c/tone", "night_Half.png"), drawn.EncodeToPNG());
            }
            finally
            {
                title.style.color = StyleKeyword.Null;
                if (drawn != null) Object.Destroy(drawn);
                if (under != null) Object.Destroy(under);
            }
        }

        // P2 PART 5 (v6 wordsBottom / shadeBottom): at Half and at Full the hero's words -- eyebrow, title, line, Listen -- sit at the foot of
        // what the visitor SEES of the hero (not at the hero's own foot, below the half sheet), above the Navigation Bar floating at the bottom,
        // their shade moving with them; the title reads at 4.5:1 on the real render. Renders in TestEvidence/Premium/P2/.
        [UnityTest]
        public IEnumerator AtHalfAndFull_TheWordsSitAtTheFootOfWhatTheVisitorSees_AboveTheBar_AtTheWordsRatio(
            [Values("lamp", "painting")] string poi, [Values(SheetStopRule.Stop.Half, SheetStopRule.Stop.Full)] SheetStopRule.Stop stop)
        {
            Session.CardSettings.container.reduce_motion = true;
            yield return Open(poi, "en", stop);
            Header.Clock = () => 0f;
            Header.Animate();
            yield return CardTestInput.UntilStill(Sheet);
            Assert.IsTrue(Header.HasPicture, "precondition: " + poi + "'s header is a picture look");
            var sheet = Sheet.Root.worldBound;
            var words = Header.Foot.worldBound;
            Assert.AreEqual(Sheet.Stops.HeightOf(stop), sheet.height, OnePixel, "precondition: the sheet rests at " + stop);
            Assert.GreaterOrEqual(words.yMin, sheet.yMin - OnePixel, "the words start inside the visible sheet");
            Assert.LessOrEqual(words.yMax, sheet.yMax + OnePixel, "...and end inside it (at Half: not below the half sheet)");
            var bar = Sheet.Slots.Bar;
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.BottomKey, "precondition: the framework's bar floats at the bottom");
            float barTop = Mathf.Min(bar.Row.worldBound.yMin, bar.Disc.resolvedStyle.display == DisplayStyle.Flex ? bar.Disc.worldBound.yMin : float.MaxValue);
            Assert.LessOrEqual(words.yMax, barTop + OnePixel, "the words end above the bar and its disc");
            // - at the foot of what shows: the bar's clearance under them, no more (the rule's place, from the hero's top = the sheet's top)
            float clearance = CardTestInput.TokenPx("--ts-navbar-clearance");
            Assert.AreEqual(sheet.yMax - clearance, words.yMax, OnePixel, "the words at the visible foot, the bar's clearance under them");
            // - their tone with them: anchored at the words' foot, as laid out at this stop
            var tone = Header.Words.Painter.Options;
            Assert.IsNotNull(tone, "precondition: the tone is drawn");
            float inkFoot = Header.Words.Inks.Max(i => i.Rect.yMax) + Header.ToneLayer.worldBound.yMin;
            Assert.LessOrEqual(inkFoot, words.yMax + OnePixel, "precondition: the ink lies inside the words' block");
            Assert.AreEqual(inkFoot, tone.Anchor.y + Header.ToneLayer.worldBound.yMin, OnePixel, "the tone anchored at the words' ink foot, at this stop");
            // - the title at 4.5:1 on the render
            var title = Header.HeroTitle;
            var colour = title.resolvedStyle.color;
            Texture2D drawn = null, under = null;
            try
            {
                yield return ThemePixelSet.GrabStill($"words_{poi}_{stop}", null, r => drawn = r);
                title.style.color = Color.clear;
                yield return null;
                yield return ThemePixelSet.GrabStill($"words_under_{poi}_{stop}", null, r => under = r);
                var (ratio, worst, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(title, drawn), colour);
                Assert.Greater(inked, 0, "precondition: the title draws pixels");
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P2", $"words_{poi}_{stop}.png"), drawn.EncodeToPNG());
                Assert.GreaterOrEqual(ratio, HeroWordsMinRatio, "the title keeps 4.5:1 at " + stop + ": " + ratio.ToString("0.00") + ":1 (worst " + (Color32)worst + ")");
            }
            finally
            {
                title.style.color = StyleKeyword.Null;
                if (drawn != null) Object.Destroy(drawn);
                if (under != null) Object.Destroy(under);
            }
        }

        // The hero's Listen is minimal: a small headphones mark and the time, no box -- still a whole touch target
        [UnityTest]
        public IEnumerator Listen_IsASmallHeadphonesMarkAndItsTime_NoBox_StillAWholeTouchTarget()
        {
            yield return Open("painting", "en", SheetStopRule.Stop.Half);
            var listen = Stack.HeroActionViews.OfType<AudioGuideBlockView>().Single();
            Assert.AreEqual(0f, listen.Root.resolvedStyle.backgroundColor.a, 0.001f, "no plate");
            Assert.AreEqual(0f, listen.Root.resolvedStyle.borderTopWidth, 0.001f, "no outline");
            var glyph = listen.ListenIcon.Q(className: "card-icon__glyph");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-hero-listen-icon"), glyph.resolvedStyle.width, 0.01f, "a small mark");
            Assert.Less(glyph.resolvedStyle.width, CardTestInput.TokenPx("--ts-touch-target") / 2f, "...well under the button it sits in");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(listen.PlayButton.worldBound.width, listen.PlayButton.worldBound.height),
                "the button is still a whole touch target: " + listen.PlayButton.worldBound);
            Assert.AreEqual(CardTestInput.TokenPx("--ts-hero-listen-text"), listen.Title.resolvedStyle.fontSize, 0.01f, "the time in small words");
        }

        // _3.3 P3a review (b): at rest (no scroll) the hero's words -- eyebrow, title, line, Listen -- are WHOLE at Half and at Full, with the
        // card's motion ON (Reduce Motion would hide the fade): their resolved opacity (the product up to the sheet) is 1 and the ink the
        // visitor sees (the words' colour at that opacity over the ring's worst ground pixel) reads >= 4.5:1 on the real render; a scroll then
        // fades them exactly as HeroMotionRule says for that offset, never brighter as they climb. Measurements in TestEvidence/Premium/P3b/
        [UnityTest]
        public IEnumerator AtRest_TheWordsAreWholeAtTheWordsRatio_AtHalfAndFull_AndOnlyTheScrollFadesThem([Values("lamp", "painting")] string poi,
            [Values(SheetStopRule.Stop.Half, SheetStopRule.Stop.Full)] SheetStopRule.Stop stop)
        {
            Assert.IsFalse(Session.CardSettings.container.reduce_motion, "precondition: the card's motion is on (the default)");
            yield return Open(poi, "en", stop);
            Header.Clock = () => 0f;
            Header.Animate();
            yield return CardTestInput.UntilStill(Sheet);
            Assert.AreEqual(0f, Scroll.scrollOffset.y, 0.5f, "precondition: no scroll");
            var listen = Stack.HeroActionViews.OfType<AudioGuideBlockView>().Single();
            var labels = new List<Label> { Header.Eyebrow, Header.HeroTitle, Header.HeroPart.Q<Label>("card-header-subtitle") }
                .Where(l => l != null && l.resolvedStyle.display == DisplayStyle.Flex && !string.IsNullOrEmpty(l.text)).ToList();
            Assert.GreaterOrEqual(labels.Count, 2, "precondition: the hero's words are drawn");
            float Seen(VisualElement e)
            {
                float opacity = 1f;
                for (var v = e; v != null && v != Sheet.Layer; v = v.parent) opacity *= v.resolvedStyle.opacity;
                return opacity;
            }
            var report = new List<string>();
            foreach (var e in labels.Cast<VisualElement>().Append(listen.Root))
                report.Add($"{poi} {stop} at rest: '{(e is Label l ? l.text : "Listen")}' opacity {Seen(e):0.000}");

            // - the contrast of the ink as SEEN: its colour at its opacity over the ring's worst ground
            float Luminance(Color c) => 0.2126f * c.linear.r + 0.7152f * c.linear.g + 0.0722f * c.linear.b;
            var colours = labels.ToDictionary(l => l, l => l.resolvedStyle.color);
            Texture2D drawn = null, under = null;
            float worst = float.MaxValue;
            try
            {
                yield return ThemePixelSet.GrabStill($"hero_rest_{poi}_{stop}", null, r => drawn = r);
                foreach (var l in labels) l.style.color = Color.clear;
                yield return null;
                yield return ThemePixelSet.GrabStill($"hero_rest_under_{poi}_{stop}", null, r => under = r);
                foreach (var l in labels)
                {
                    var (ratio, ground, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(l, drawn), colours[l]);
                    Assert.Greater(inked, 0, "precondition: '" + l.text + "' draws pixels");
                    var seen = Color.Lerp(ground.linear, colours[l].linear, Seen(l)).gamma;
                    float a = Luminance(seen), b = Luminance(ground);
                    float seenRatio = (Mathf.Max(a, b) + 0.05f) / (Mathf.Min(a, b) + 0.05f);
                    report.Add($"{poi} {stop} at rest: '{l.text}' {ratio:0.00}:1 at full ink, {seenRatio:0.00}:1 as seen (ground {(Color32)ground})");
                    worst = Mathf.Min(worst, seenRatio);
                }
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3b", $"hero_rest_{poi}_{stop}.png"), drawn.EncodeToPNG());
            }
            finally
            {
                foreach (var l in labels) l.style.color = StyleKeyword.Null;
                if (drawn != null) Object.Destroy(drawn);
                if (under != null) Object.Destroy(under);
            }
            System.IO.File.AppendAllText(TestEvidence.PathFor("Premium/P3b", "hero_words_at_rest.txt"), string.Join("\n", report) + "\n");
            foreach (var e in labels.Cast<VisualElement>().Append(listen.Root))
                Assert.AreEqual(1f, Seen(e), 0.001f, $"{poi} {stop}: at rest '{(e is Label l ? l.text : "Listen")}' is whole\n" + string.Join("\n", report));
            Assert.GreaterOrEqual(worst, HeroWordsMinRatio, $"{poi} {stop}: every word reads >= 4.5:1 as seen\n" + string.Join("\n", report));

            // - the scroll fades them, as the rule says for each offset, never brighter as they climb
            float restTop = Header.HeroPart.layout.y + Header.Foot.layout.y;
            // - offsets along the words' own way to their fade's end (the scroll that brings them there: they rise TextRiseFactor faster;
            //   words resting close under the chrome still fade over MinFadeSpan)
            float way = Mathf.Max(restTop - Stack.LandingInset, HeroMotionRule.MinFadeSpan) / (1f + HeroMotionRule.TextRiseFactor);
            float last = 1f;
            foreach (float offset in new[] { 0.15f, 0.35f, 0.55f, 0.75f, 0.9f }.Select(f => Mathf.Round(f * way)))
            {
                Scroll.scrollOffset = new Vector2(0f, offset);
                yield return null;
                yield return null;
                float y = Scroll.scrollOffset.y;
                Assert.AreEqual(offset, y, 0.5f, "precondition: the stack scrolls that far");
                float nowTop = restTop - y + Header.TextRise;
                float expected = HeroMotionRule.TextOpacity(restTop, nowTop, Stack.LandingInset, false);
                Assert.AreEqual(expected, Header.Foot.resolvedStyle.opacity, 0.01f, $"{poi} {stop}: at {y:0} the words fade as the rule says");
                Assert.LessOrEqual(Header.Foot.resolvedStyle.opacity, last + 0.001f, "...never brighter as they climb");
                last = Header.Foot.resolvedStyle.opacity;
            }
            Assert.Less(last, 1f, "precondition: the scroll carried the words into their fade");
        }
    }

    // PoiCardHeroSceneTests's tests whose subject is an old navigation element: they wait for its rebuild on the Navigation Bar's pattern
    [Ignore(TestSkips.OldNavigationElements)]
    public class PoiCardHeroSceneOldNavigationTests : PoiCardHeroSceneFixture
    {
        // ---------------- the bar ----------------

        [UnityTest]
        public IEnumerator TheBar_IsOneRow_SoNoLanguageOrMenuMovesAStop([Values("lamp", "painting")] string poi, [Values("en", "pt")] string language)
        {
            yield return Open(poi, language, SheetStopRule.Stop.Peek);
            var stops = Sheet.Stops;
            var title = Header.Root.Q<Label>("card-header-title");
            float bar = Stack.HeaderSlot.worldBound.height;
            Assert.AreEqual(Sheet.Root.worldBound.yMin + stops.Peek, Header.PeekPart.worldBound.yMax, OnePixel, "the peek stop is the bar's bottom");
            Assert.AreEqual(Sheet.CloseButton.worldBound.yMax, Stack.HeaderSlot.worldBound.yMax, OnePixel, "the bar's row ends with the round buttons");
            Assert.IsTrue(Header.BarTitleShown, "at Peek the bar shows the title");
            Assert.AreEqual(WhiteSpace.NoWrap, title.resolvedStyle.whiteSpace, "one line");
            Assert.Less(title.worldBound.height, title.resolvedStyle.fontSize * 2f, "...drawn as one line");
            Assert.LessOrEqual(title.worldBound.xMax, Sheet.SettingsButton.worldBound.xMin + OnePixel, "it ends before settings");
            Assert.GreaterOrEqual(title.worldBound.xMin, Sheet.CloseButton.worldBound.xMax - OnePixel, "...and starts after the X (v5: X left, settings right)");
            Assert.AreEqual(Sheet.Root.worldBound.center.x, title.worldBound.center.x, OnePixel, "...centred on the card");

            yield return Push(c => c.card_settings.navigation.menu = BuiltInNavElements.DrawerTree);
            Assert.AreEqual(DisplayStyle.Flex, Sheet.MenuButton.resolvedStyle.display, "precondition: the Menu's button shows");
            Assert.AreEqual(stops.Peek, Sheet.Stops.Peek, 0.01f, "a Menu never moves Peek (stop " + Sheet.Stop + ", peek part " + Stack.PeekPart.name
                + " " + Stack.PeekPart.worldBound + ", root " + Sheet.Root.worldBound + ", layer " + Sheet.Layer.layout + ")");
            Assert.AreEqual(stops.Half, Sheet.Stops.Half, 0.01f);
            Assert.AreEqual(bar, Stack.HeaderSlot.worldBound.height, 0.01f, "the bar keeps its height");
            Assert.LessOrEqual(title.worldBound.xMax, Sheet.MenuButton.worldBound.xMin + OnePixel, "the title ends before the Menu's button");
            Assert.Less(title.worldBound.height, title.resolvedStyle.fontSize * 2f, "...still one line");
        }

        // ---------------- the hero is the screen (_3.3 S4a) ----------------

        // At Full the hero fills the whole view under the bar and the strip waits: it comes as soon as the visitor scrolls (the chrome's hero
        // layer, CardChromeRule: no chrome where it would cut the hero in view), and goes again at the top
        [UnityTest]
        public IEnumerator AtFull_TheHeroFillsTheView_AndTheStripComesOnceTheVisitorScrolls([Values("lamp", "painting")] string poi)
        {
            yield return Open(poi, "en", SheetStopRule.Stop.Full);
            Assert.AreEqual(0f, Scroll.scrollOffset.y, 0.5f, "precondition: the stack at its top");
            var view = Scroll.contentViewport.worldBound;
            var hero = Header.HeroPart.worldBound;
            Assert.IsFalse(Sheet.Chrome.TopShown, "at rest the strip waits: the hero is the screen");
            Assert.AreEqual(Sheet.Root.worldBound.yMin, view.yMin, OnePixel, "the view starts at the sheet's top: the bars float over it (S4a review)");
            Assert.AreEqual(view.yMin, hero.yMin, OnePixel, "the hero's top at the view's top");
            Assert.AreEqual(view.height, hero.height, OnePixel, "the hero fills the view: " + hero + " in " + view);
            Assert.AreEqual(Sheet.Root.worldBound.width, Header.Frame.worldBound.width, OnePixel, "full width");
            Assert.GreaterOrEqual(Header.Picture.Root.worldBound.height, hero.height - OnePixel, "the picture covers the hero's height (cropped at its sides)");

            // - a scroll: the strip comes; back at the top it waits again
            float strip = Sheet.Slots.Top.layout.height;
            Assert.Greater(strip, 0f, "precondition: the strip is laid out (hidden, still measured)");
            // - it floats: it comes once the picture scrolled away by the bars and the strip (CardChromeRule, TopFloats)
            Scroll.scrollOffset = new Vector2(0f, Stack.TopInset + strip + 8f);
            for (int frame = 0; frame < 60 && !Sheet.Chrome.TopShown; frame++) yield return null;
            Assert.IsTrue(Sheet.Chrome.TopShown, "scrolled by the bars' and the strip's height: the strip shows");
            Scroll.scrollOffset = Vector2.zero;
            for (int frame = 0; frame < 60 && Sheet.Chrome.TopShown; frame++) yield return null;
            Assert.IsFalse(Sheet.Chrome.TopShown, "back at the top: the hero is the screen again");
        }
    }
}
