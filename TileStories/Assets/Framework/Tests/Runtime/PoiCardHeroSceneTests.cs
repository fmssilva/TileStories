using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TileStories.Tests;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The header's bar and hero on the REAL LivingRoomScene (_3.3 P4.4a, S4a; HeroRule / HeroMotionRule / HeroActionsRule): the bar is one
    // row whatever the language or the Menu (the stops never move); the hero is the screen -- Full's whole view, the strip waiting until the
    // visitor scrolls -- its words and actions whole at Half (the PT Lamp's "Mostra-me onde fica" included) while the sticky bar waits; real
    // taps on the hero's action and Listen do what the card's own blocks do; the drift, the zoom, the parallax and the words' rise run on
    // the test's clock and stop under Reduce Motion (frame sequences saved for the eye); the glow lies only behind the words, a fade with no
    // edge; and every word keeps 7:1 against the picture right about its letters on every hero picture of the wall, EN / PT, light / dark
    public class PoiCardHeroSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private HeaderBlockView Header => Stack.HeaderView;
        private ScrollView Scroll => Stack.Scroll;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private const string Domain = "Hero";
        private const float Aaa = 7f;

        private ManualAudioOutput _out;
        private CardAudioService _audio;

        [UnitySetUp]
        public IEnumerator UseASilentAudioDevice()
        {
            _out = new ManualAudioOutput();
            _audio = new CardAudioService(_out, () => Card.Media, () => 0f, () => Session.CardSettings.container.audio_when_another_starts);
            Card.UseAudioService(_audio);
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        private IEnumerator Open(string poi, string language, SheetStopRule.Stop stop)
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

        private IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
        }

        // Push an edit the way the Editor's live applier does: a private copy of the running config
        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private static void SetHeaderField(WallConfigData config, string poi, string key, string value)
        {
            var header = config.pois.First(p => p.id == poi).card.blocks.First(b => b.kind == BuiltInBlocks.HeaderKind);
            var field = header.fields.Find(f => f.key == key);
            if (field == null) header.fields.Add(field = new BlockFieldValue { key = key });
            field.value = value;
        }

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

        // ---------------- the bar ----------------

        [Ignore(TestSkips.OldNavigationElements)]
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

        // ---------------- the words: 7:1 against the picture about their letters ----------------

        private static IEnumerable<string> HeroPictures() => new[] { "lamp", "painting", "lamp_military", "lamp_religious" };

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // _3.3 S4a: every hero picture of the test wall, in English and Portuguese, light and dark -- each word of the hero against the picture
        // RIGHT AROUND ITS LETTERS (a ring 2-3 px about its strokes, as drawn: where the halo and the glow darken the picture), at the worst
        // such pixel
        [UnityTest]
        public IEnumerator EveryWordOfTheHero_ReadsAgainstThePictureRightAroundItsLetters([ValueSource(nameof(HeroPictures))] string poi,
            [Values("en", "pt")] string language, [Values("light", "dark")] string mode)
        {
            // - the looks that move by themselves are measured still: the video loop on its poster, the turntable at rest (Reduce Motion);
            //   the image parallax across its drift (start, a quarter, the far side)
            bool pans = poi == "lamp" || poi == "painting";
            if (!pans) yield return Push(c => c.card_settings.container.reduce_motion = true);
            ThemeController.SetMode(mode);
            yield return Open(poi, language, SheetStopRule.Stop.Full);
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
                    yield return ThemePixelSet.GrabStill($"words_{poi}_{language}_{mode}_{share}", null, r => drawn = r);
                    // - the letters made clear, their halo kept: what is under and around each letter
                    foreach (var l in words) l.style.color = new Color(colours[l].r, colours[l].g, colours[l].b, 0f);
                    yield return ThemePixelSet.GrabStill($"under_{poi}_{language}_{mode}_{share}", null, r => under = r);
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
                        report.Add($"{poi} {language} {mode} drift {share:0.00} {what}: {ratio:0.00}:1 (worst pixel about its letters {(Color32)worst})");
                    }
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain + "/words", $"{poi}_{language}_{mode}_{share:0.00}.png"), drawn.EncodeToPNG());
                }
                finally
                {
                    foreach (var l in words) l.style.color = StyleKeyword.Null;
                    if (drawn != null) Object.Destroy(drawn);
                    if (under != null) Object.Destroy(under);
                }
            }
            System.IO.File.AppendAllText(TestEvidence.PathFor(Domain + "/words", "ratios.txt"), string.Join("\n", report) + "\n");
            Debug.Log("[Card] Hero words " + poi + " " + language + " " + mode + ": worst " + worstOfAll.ToString("0.00") + ":1 (" + worstWhat + ")");
            Assert.GreaterOrEqual(worstOfAll, Aaa, "every word of the hero keeps 7:1 against the picture about its letters -- worst: " + worstWhat);
        }

        // ---------------- the hero is the screen (_3.3 S4a) ----------------

        // At Full the hero fills the whole view under the bar and the strip waits: it comes as soon as the visitor scrolls (the chrome's hero
        // layer, CardChromeRule: no chrome where it would cut the hero in view), and goes again at the top
        [Ignore(TestSkips.OldNavigationElements)]
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

        // v5's words shade lies only behind the words: the hero drawn with and without it differs from the shade's top down and nowhere above
        // it; it fades in (its top rows change almost nothing), then holds its measured floor behind every word and down to the hero's foot;
        // the band between the top shade's foot and the shade's top is the bare picture
        [UnityTest]
        public IEnumerator TheWordsShade_LiesOnlyBehindTheWords_FadesInAboveThem_ThePictureAboveIsClean([Values("lamp", "painting")] string poi)
        {
            // - at Full: the words (and their shade) sit at the hero's foot (S4a review)
            yield return Open(poi, "en", SheetStopRule.Stop.Full);
            Header.Clock = () => 0f;
            Header.Animate();
            Assert.AreEqual(0f, Header.Foot.resolvedStyle.backgroundColor.a, 0.001f, "the foot itself is clear");
            Assert.AreEqual(0f, Header.Text.resolvedStyle.backgroundColor.a, 0.001f, "no plate behind the words");
            // - the pair a USS length cannot express as one: the shade reaches up exactly its ramp's height
            Assert.AreEqual(-CardTestInput.TokenPx("--ts-hero-shade-ramp"), Header.Shade.resolvedStyle.top, OnePixel, "the shade starts one ramp above the words");
            var floor = Header.Shade.Q("card-hero-shade-floor");
            Assert.That(floor.resolvedStyle.backgroundColor.a, Is.GreaterThanOrEqualTo(0.9f), "the floor behind the words is the measured scrim");
            var hero = Header.HeroPart.worldBound;
            var shade = Header.Shade.worldBound;
            Assert.LessOrEqual(shade.yMin, Header.Foot.worldBound.yMin - CardTestInput.TokenPx("--ts-hero-shade-ramp") + OnePixel, "the ramp ends where the words start");
            Assert.GreaterOrEqual(shade.yMax, hero.yMax, "the floor reaches the hero's foot");
            float topShadeFoot = CardTestInput.UnderTheTopShade(Header.HeroPart);
            Assert.Greater(shade.yMin - topShadeFoot, hero.height / 4f, "a clean band of picture between the two shades: " + topShadeFoot + " .. " + shade.yMin);
            Texture2D with = null, without = null;
            try
            {
                var labels = Header.HeroPart.Query<Label>().ToList();
                foreach (var l in labels) l.style.visibility = Visibility.Hidden;
                // - the Navigation Bar lies over the hero's foot: not the subject here, out of both renders
                Sheet.Slots.Bar.Root.style.visibility = Visibility.Hidden;
                Header.Actions.style.visibility = Visibility.Hidden;
                yield return ThemePixelSet.GrabStill("shade_" + poi, Header.HeroPart, r => with = r);
                Header.Shade.style.visibility = Visibility.Hidden;
                yield return ThemePixelSet.GrabStill("noshade_" + poi, Header.HeroPart, r => without = r);
                Header.Shade.style.visibility = StyleKeyword.Null;
                foreach (var l in labels) l.style.visibility = StyleKeyword.Null;
                Header.Actions.style.visibility = StyleKeyword.Null;
                Sheet.Slots.Bar.Root.style.visibility = StyleKeyword.Null;
                System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain + "/shade", poi + "_with.png"), with.EncodeToPNG());
                System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain + "/shade", poi + "_without.png"), without.EncodeToPNG());

                // - the shade's top in the render (rows bottom-up), a pixel of rounding about it. The render is the hero cut to the screen:
                //   its bottom row is the hero's top plus the render's height
                float perPixel = ThemePixelSet.PanelUnitsPerPixel(Header.HeroPart);
                float renderBottom = hero.yMin + with.height * perPixel;
                int Row(float y) => Mathf.RoundToInt((renderBottom - y) / perPixel);
                int top = Row(shade.yMin) + 1, wordsTop = Row(Header.Foot.worldBound.yMin);
                Color32[] a = with.GetPixels32(), b = without.GetPixels32();
                int changedAbove = 0, firstRowsChanged = 0, darkenedBehindWords = 0, behindWords = 0;
                for (int y = 0; y < with.height; y++)
                    for (int x = 0; x < with.width; x++)
                    {
                        int i = y * with.width + x;
                        int delta = Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b);
                        if (y > top) { if (delta > 6) changedAbove++; continue; }
                        // - its first rows: a ramp starts from (almost) nothing, a rectangle would start at its full tint
                        if (y >= top - 1 && delta > 12) firstRowsChanged++;
                        // - behind the words: every pixel the picture had light enough to darken is darkened
                        if (y <= wordsTop && b[i].r + b[i].g + b[i].b > 60) { behindWords++; if (a[i].r + a[i].g + a[i].b < (b[i].r + b[i].g + b[i].b) / 2) darkenedBehindWords++; }
                    }
                Assert.AreEqual(0, changedAbove, "not one pixel of the picture changes above the shade");
                Assert.AreEqual(0, firstRowsChanged, "the shade fades in: no rectangle edge at its top");
                Assert.Greater(behindWords, with.width * 10, "precondition: picture behind the words to judge");
                Assert.AreEqual(behindWords, darkenedBehindWords, "behind the words the picture is at most half as bright: the floor");
            }
            finally
            {
                if (with != null) Object.Destroy(with);
                if (without != null) Object.Destroy(without);
            }
        }

        // P2 PART 5 (v6 wordsBottom / shadeBottom): at Half and at Full the hero's words -- eyebrow, title, line, Listen -- sit at the foot of
        // what the visitor SEES of the hero (not at the hero's own foot, below the half sheet), above the Navigation Bar floating at the bottom,
        // their shade moving with them; the title reads at 7:1 on the real render. Renders in TestEvidence/Premium/P2/.
        [UnityTest]
        public IEnumerator AtHalfAndFull_TheWordsSitAtTheFootOfWhatTheVisitorSees_AboveTheBar_At7To1(
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
            // - their shade with them: its floor behind the words
            Assert.LessOrEqual(Header.Shade.worldBound.yMin, words.yMin, "the shade starts above the words");
            Assert.GreaterOrEqual(Header.Shade.worldBound.yMax, words.yMax, "...and lies behind them");
            // - the title at 7:1 on the render
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
                Assert.GreaterOrEqual(ratio, Aaa, "the title keeps 7:1 at " + stop + ": " + ratio.ToString("0.00") + ":1 (worst " + (Color32)worst + ")");
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

    }
}
