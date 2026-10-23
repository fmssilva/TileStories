using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The header's bar and hero on the REAL LivingRoomScene (_3.3 P4.4a, HeroRule / HeroMotionRule / HeroActionsRule): the bar is one row
    // whatever the language or the Menu (the stops never move); at Half the whole hero is in view with the card's actions -- the PT Lamp's
    // "Mostra-me onde fica" included -- and the sticky bar waits for it; real taps on the hero's action and Listen do what the card's own
    // blocks do; the pan, the zoom, the parallax and the words' rise run on the test's clock and stop under Reduce Motion (frame sequences
    // saved for the eye); and the scrim keeps every word of the hero at 7:1 against its WORST background pixel on every hero picture of the
    // wall, measured on the render with the words hidden, across the pan
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
            Assert.LessOrEqual(title.worldBound.xMax, Sheet.LanguageButton.worldBound.xMin + OnePixel, "it ends before the buttons");

            yield return Push(c => c.card_settings.navigation.menu = BuiltInNavElements.DrawerTree);
            Assert.AreEqual(DisplayStyle.Flex, Sheet.MenuButton.resolvedStyle.display, "precondition: the Menu's button shows");
            Assert.AreEqual(stops.Peek, Sheet.Stops.Peek, 0.01f, "a Menu never moves Peek (stop " + Sheet.Stop + ", peek part " + Stack.PeekPart.name
                + " " + Stack.PeekPart.worldBound + ", root " + Sheet.Root.worldBound + ", layer " + Sheet.Layer.layout + ")");
            Assert.AreEqual(stops.Half, Sheet.Stops.Half, 0.01f);
            Assert.AreEqual(bar, Stack.HeaderSlot.worldBound.height, 0.01f, "the bar keeps its height");
            Assert.LessOrEqual(title.worldBound.xMax, Sheet.MenuButton.worldBound.xMin + OnePixel, "the title ends before the Menu's button");
            Assert.Less(title.worldBound.height, title.resolvedStyle.fontSize * 2f, "...still one line");
        }

        // ---------------- Half: the whole hero, with the card's actions ----------------

        [UnityTest]
        public IEnumerator AtHalf_TheWholeHeroIsInView_WithTheCardsActions_AndTheStickyBarWaits(
            [Values("lamp", "painting")] string poi, [Values("en", "pt")] string language)
        {
            yield return Open(poi, language, SheetStopRule.Stop.Half);
            Header.Clock = () => 0f;
            Header.Animate();
            Rect view = Scroll.contentViewport.worldBound, hero = Header.HeroPart.worldBound;
            Assert.IsTrue(Header.HasPicture, "precondition: " + poi + "'s header is a picture look");
            Assert.AreEqual(HeroRule.Height(Sheet.Stops, CardTestInput.TokenPx("--ts-hero-min-height")), hero.height, OnePixel, "the hero is HeroRule's height (its words fit in it)");
            Assert.GreaterOrEqual(hero.height, 280f - OnePixel, "never under 280");
            Assert.GreaterOrEqual(hero.yMin, view.yMin - OnePixel, "its top in view");
            Assert.LessOrEqual(hero.yMax, view.yMax + OnePixel, "its foot in view: " + hero + " in " + view);
            Assert.AreEqual(Sheet.Root.worldBound.width, hero.width, OnePixel, "full width");
            foreach (var label in new[] { Header.Eyebrow, Header.HeroTitle, Header.HeroPart.Q<Label>("card-header-subtitle") })
                Assert.IsTrue(view.Contains(label.worldBound.min) && view.Contains(label.worldBound.max - Vector2.one * OnePixel), label.name + " wholly in view");
            Assert.IsFalse(Header.BarTitleShown, "the hero carries the title, the bar none");
            Assert.IsFalse(Sheet.Chrome.BottomShown, "the sticky bar waits for the hero");

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
                    "the card's action is on screen at Half (the P4.3 finding: PT lost it): " + button.worldBound + " in " + view);
                Assert.Less(button.Q<Label>().worldBound.height, button.Q<Label>().resolvedStyle.fontSize * 2f, "its words on one line");
                Assert.AreEqual(1, Stack.Footer.Query<Button>(className: "card-action").ToList().Count, "the footer still holds the same action for later");
            }
            var listen = actions.OfType<AudioGuideBlockView>().Single();
            Assert.AreEqual(BuiltInBlocks.AudioGuideHeroChip, listen.Variant);
            StringAssert.StartsWith(language == "en" ? "Listen " : "Ouvir ", listen.Title.text, "Listen with the guide's length");
            Assert.IsTrue(view.Contains(listen.PlayButton.worldBound.center), "Listen is on screen");
            yield return LookMeasure.Render(Domain, "check", $"half_{poi}_{language}", Sheet.Root);
        }

        // ---------------- real taps ----------------

        [UnityTest]
        public IEnumerator ARealTapOnTheHerosAction_ShowsThePointOnTheWall_TheCardLowersAndTheSelectionStays()
        {
            yield return Open("lamp", "en", SheetStopRule.Stop.Half);
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
            yield return Open("painting", "en", SheetStopRule.Stop.Half);
            var listen = Stack.HeroActionViews.OfType<AudioGuideBlockView>().Single();
            var player = Stack.BoundViews.OfType<AudioGuideBlockView>().Single();
            Assert.AreEqual(player.Track.ClipPath, listen.Track.ClipPath, "precondition: Listen is The Painting's own guide (its player's clip)");
            yield return CardTestInput.Tap(listen.PlayButton.panel, listen.PlayButton.worldBound.center);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "a real tap plays");
            Assert.IsTrue(_audio.IsCurrent(player.Track), "the card's guide: one audio, drawn by the player too");
            Assert.AreEqual("Listen", listen.Title.text, "playing: Listen beside the running time");
            Assert.AreEqual(DisplayStyle.Flex, listen.TimeText.resolvedStyle.display);
            yield return CardTestInput.Tap(listen.PlayButton.panel, listen.PlayButton.worldBound.center);
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "a second tap pauses");
        }

        // ---------------- motion ----------------

        [UnityTest]
        public IEnumerator ThePan_CrossesTheWholePictureOnTheClock_ReduceMotionHoldsItsCentre_ZoomAndNone()
        {
            yield return Open("lamp", "en", SheetStopRule.Stop.Half);
            float t = 0f;
            Header.Clock = () => t;
            Assert.AreEqual(HeroMotionRule.Pan, Header.Motion, "precondition: Hero Motion's default");
            Assert.IsTrue(Header.Moving);
            Assert.Greater(Header.Overflow, 20f, "precondition: castle_hero is wider than the hero (" + Header.Picture.Aspect + ")");
            float frame = Header.Frame.worldBound.width;
            Assert.AreEqual(frame + Header.Overflow, Header.Picture.Root.worldBound.width, OnePixel, "the picture as wide as its proportions ask");

            float pictureLeft0 = 0f;
            var steps = new[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
            for (int i = 0; i < steps.Length; i++)
            {
                t = steps[i] * HeroMotionRule.PanPeriod;
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
            Assert.AreEqual(-Header.Overflow, HeroMotionRule.PanOffset(HeroMotionRule.Pan, false, HeroMotionRule.PanPeriod / 2f, Header.Overflow), 0.01f);

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
                Assert.AreEqual(HeroMotionRule.TextOpacity(y, false), Header.Text.resolvedStyle.opacity, 0.01f, "...and fade");
                Assert.AreEqual(HeroMotionRule.Parallax(y, Header.Frame.layout.height, false), Header.ParallaxOffset, 0.01f, "the picture lags");
                Assert.AreEqual(HeroRule.BarShowsTitle(Sheet.Stop, y, Header.HeroTitleBottom()), Header.BarTitleShown, "the bar takes the title exactly when the rule says, at " + y);
                yield return LookMeasure.Render(Domain, "frames", $"scroll_{i}_{offsets[i]:0}", Sheet.Root);
            }
            Assert.IsTrue(Header.BarTitleShown, "scrolled 300: the hero's title is gone, the bar carries it");
            yield return Push(c => c.card_settings.container.reduce_motion = true);
            Scroll.scrollOffset = new Vector2(0f, 120f);
            yield return null;
            yield return null;
            Assert.AreEqual(1f, Header.Text.resolvedStyle.opacity, 0.01f, "reduce motion: the words stay whole");
            Assert.AreEqual(0f, Header.TextRise, 0.01f, "...and still");
            Assert.AreEqual(0f, Header.ParallaxOffset, 0.01f, "...the picture too");
        }

        // ---------------- the scrim: 7:1 at the words' worst pixel ----------------

        private static IEnumerable<string> HeroPictures() => new[] { "lamp", "painting", "lamp_military", "lamp_religious" };

        [UnityTest]
        public IEnumerator TheScrim_KeepsEveryWordOfTheHeroAt7To1_AgainstItsWorstPixel([ValueSource(nameof(HeroPictures))] string poi)
        {
            // - the looks that move by themselves are measured still: the video loop on its poster, the turntable at rest (Reduce Motion);
            //   the image parallax across its pan (start, middle, end)
            bool pans = poi == "lamp" || poi == "painting";
            if (!pans) yield return Push(c => c.card_settings.container.reduce_motion = true);
            yield return Open(poi, "en", SheetStopRule.Stop.Full);
            Assert.IsTrue(Header.HasPicture, "precondition: " + poi + " has a hero picture");
            float t = 0f;
            Header.Clock = () => t;
            var words = new List<Label> { Header.Eyebrow, Header.HeroTitle, Header.HeroPart.Q<Label>("card-header-subtitle") };
            words.AddRange(Header.Actions.Query<Label>(className: "card-audio__title").ToList());
            words = words.Where(l => l.text.Length > 0 && l.resolvedStyle.display == DisplayStyle.Flex).ToList();
            Assert.GreaterOrEqual(words.Count, 1, "precondition: words to judge (at least the title)");
            var report = new List<string>();
            var failures = new List<string>();
            float worstOfAll = float.MaxValue;
            foreach (float share in pans ? new[] { 0f, 0.25f, 0.5f } : new[] { 0f })
            {
                t = share * HeroMotionRule.PanPeriod;
                Header.Animate();
                var colours = words.ToDictionary(l => l, l => l.resolvedStyle.color);
                // - the words hidden (laid out, not drawn): what is under them is the picture and the scrim alone
                foreach (var l in words) l.style.visibility = Visibility.Hidden;
                Texture2D shot = null;
                try
                {
                    yield return ThemePixelSet.GrabStill($"scrim_{poi}_{share}", null, r => shot = r);
                    foreach (var l in words)
                    {
                        var area = ThemePixelSet.ScreenRectOf(l, shot);
                        Assert.Greater(area.width * area.height, 0, "precondition: " + l.name + " covers pixels");
                        Color worst = Color.black;
                        float worstLum = -1f;
                        foreach (var c in shot.GetPixels(area.x, area.y, area.width, area.height))
                        {
                            float lum = 0.2126f * Lin(c.r) + 0.7152f * Lin(c.g) + 0.0722f * Lin(c.b);
                            if (lum > worstLum) { worstLum = lum; worst = c; }
                        }
                        float ratio = UIAccessibility.ContrastRatio(colours[l], worst);
                        worstOfAll = Mathf.Min(worstOfAll, ratio);
                        string what = l.name.Length > 0 ? l.name : "listen";
                        report.Add($"{poi} pan {share:0.00} {what}: {ratio:0.00}:1 (worst background {(Color32)worst}, area {area})");
                        if (ratio < Aaa) failures.Add($"{poi}: '{l.text}' over its worst pixel {(Color32)worst} at pan {share}: {ratio:0.00}:1 (area {area}, box {l.worldBound})");
                    }
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain + "/scrim", $"{poi}_{share:0.00}.png"), shot.EncodeToPNG());
                }
                finally
                {
                    foreach (var l in words) l.style.visibility = StyleKeyword.Null;
                    if (shot != null) Object.Destroy(shot);
                }
            }
            System.IO.File.AppendAllText(TestEvidence.PathFor(Domain + "/scrim", "ratios.txt"), string.Join("\n", report) + "\n");
            Debug.Log("[Card] Scrim " + poi + ": worst word " + worstOfAll.ToString("0.00") + ":1");
            CollectionAssert.IsEmpty(failures, "every word of the hero keeps 7:1 at its worst pixel");
        }

        private static float Lin(float c) => c <= 0.03928f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);
    }
}
