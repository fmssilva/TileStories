using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // The measuring instrument of a change of LOOK (_3.3 P4.3, the app's faces -> TestEvidence/Typeface; P4.4a the header and the hero ->
    // TestEvidence/Hero, HeroMeasureSceneTests): the same renders and numbers taken before and after, then put side by side. Not a suite
    // member: it runs only while TestEvidence/<domain>/MEASURING exists, and that file's one
    // word names the phase ("before" or "after"); each phase writes TestEvidence/<domain>/<phase>/ (renders + measures.txt), and the
    // "after" phase also writes compare/<name>.png = before | after for every render both phases hold. Renders: the P4.1 gallery set
    // (ThemePixelSetTests' entries) and the real card -- The Lamp and The Painting, English and Portuguese, at Peek, Half and Full,
    // cropped to the sheet -- plus one block under the P0 glare layer (35 % white, _3.3 R8 S2c). Numbers: the sheet's three stops and the
    // header's heights, per card and language.
    internal static class LookMeasure
    {
        // The type change of P4.3 and the header / hero of P4.4a measure into their own folders
        public const string Typeface = "Typeface";
        public const string Hero = "Hero";
        // The palettes of P4.4b: phases before (today's dark), light and dark -- the same renders in each mode
        public const string Palette = "Palette";
        private const int Gap = 8;

        // The domain the gallery and card renders measure into: Palette while its switch file exists, else Typeface
        public static string ActiveLook => File.Exists(Path.Combine(TestEvidence.Root, Palette, "MEASURING")) ? Palette : Typeface;

        // The phase the switch file names, or ignore the test when there is none
        public static string RequirePhase(string domain)
        {
            string switchFile = Path.Combine(TestEvidence.Root, domain, "MEASURING");
            if (!File.Exists(switchFile))
                Assert.Ignore("A measurement: write the phase into TestEvidence/" + domain + "/MEASURING to run it (LookMeasure)");
            string phase = File.ReadAllText(switchFile).Trim();
            var phases = domain == Palette ? new[] { "before", "light", "dark" } : new[] { "before", "after" };
            Assert.Contains(phase, phases, "TestEvidence/" + domain + "/MEASURING names the phase: " + string.Join(" / ", phases));
            return phase;
        }

        // Save the still screen (or `cropTo`) as <phase>/<name>.png; in a later phase also compare/<phase>/<name>.png beside its before
        public static IEnumerator Render(string domain, string phase, string name, VisualElement cropTo = null)
        {
            Texture2D render = null;
            yield return ThemePixelSet.GrabStill(name, cropTo, r => render = r);
            try
            {
                File.WriteAllBytes(TestEvidence.PathFor(domain + "/" + phase, name + ".png"), render.EncodeToPNG());
                string before = Path.Combine(TestEvidence.Root, domain, "before", name + ".png");
                if (phase != "before" && File.Exists(before)) WriteSideBySide(domain, before, render, phase == "after" ? name : phase + "_" + name);
            }
            finally { Object.Destroy(render); }
        }

        // Hold the hero's pan still for a render (since P4.4a a wide hero picture pans: the picture would never hold still)
        public static void FreezeHero(PoiCardSheetView sheet)
        {
            var header = sheet.Stack.HeaderView;
            if (header != null) header.Clock = () => 0f;
            CardTestInput.HoldTheWallStill(sheet);
            // - the palette phases render the card in their own mode (before = today's tree, untouched)
            if (ActiveLook == Palette)
            {
                string phase = File.ReadAllText(Path.Combine(TestEvidence.Root, Palette, "MEASURING")).Trim();
                if (phase != "before") ThemeController.SetMode(phase);
            }
        }

        // One line of measures.txt for this phase
        public static void Note(string domain, string phase, string line)
        {
            File.AppendAllText(TestEvidence.PathFor(domain + "/" + phase, "measures.txt"), line + "\n");
            Debug.Log("[" + domain + "] " + phase + ": " + line);
        }

        // before | gap | after on one canvas, tops aligned (a taller one keeps its height)
        private static void WriteSideBySide(string domain, string beforePath, Texture2D after, string name)
        {
            var before = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(before.LoadImage(File.ReadAllBytes(beforePath)), name + ": the before render loads");
                int w = before.width + Gap + after.width, h = Mathf.Max(before.height, after.height);
                var sheet = new Texture2D(w, h, TextureFormat.RGBA32, false);
                var fill = Enumerable.Repeat((Color32)Color.magenta, w * h).ToArray();
                sheet.SetPixels32(fill);
                // - textures run bottom-up: align the TOPS
                sheet.SetPixels(0, h - before.height, before.width, before.height, before.GetPixels());
                sheet.SetPixels(before.width + Gap, h - after.height, after.width, after.height, after.GetPixels());
                sheet.Apply();
                File.WriteAllBytes(TestEvidence.PathFor(domain + "/compare", name + ".png"), sheet.EncodeToPNG());
                Object.Destroy(sheet);
            }
            finally { Object.Destroy(before); }
        }
    }

    // The P4.1 gallery set (block looks, a block under glare, the full view), rendered for the type comparison
    public class TypefaceMeasureGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardNavGallery/CardNavGalleryScene.unity";
        private CardNavGalleryHarness _harness;
        private string _phase;

        private static readonly string[] BlockEntries =
        {
            "header_image_parallax_short_half", "rich_text_drop_cap_long", "status_ring_tokens", "gallery_carousel_short",
            "knowledge_check_multiple_choice_short", "audio_guide_player_short",
        };

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _phase = LookMeasure.RequirePhase(LookMeasure.ActiveLook);
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardNavGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the navigation gallery scene holds CardNavGalleryHarness");
            _harness.Gallery.AutoAdvanceAudio = false;
            _harness.Gallery.AutoAdvanceVideo = false;
            LookMeasure.FreezeHero(_harness.Gallery.Sheet);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BlockLooks([ValueSource(nameof(BlockEntries))] string name)
        {
            int index = CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            Assert.GreaterOrEqual(index, 0, "precondition: the gallery defines " + name);
            _harness.Gallery.Show(index);
            LookMeasure.FreezeHero(_harness.Gallery.Sheet);
            yield return CardTestInput.Settle();
            yield return LookMeasure.Render(LookMeasure.ActiveLook, _phase, "block_" + name);
        }

        // The rich text block under the P0 glare layer: a white sheet at 35 % over the whole screen (sun on the glass, _3.3 R8 S2c)
        [UnityTest]
        public IEnumerator ABlockUnderGlare()
        {
            _harness.Gallery.Show(CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == "rich_text_drop_cap_long"));
            yield return CardTestInput.Settle();
            var root = _harness.Gallery.Sheet.Root.panel.visualTree;
            var glare = new VisualElement { pickingMode = PickingMode.Ignore };
            glare.style.position = Position.Absolute;
            glare.style.left = 0; glare.style.top = 0; glare.style.right = 0; glare.style.bottom = 0;
            glare.style.backgroundColor = Color.white;
            glare.style.opacity = 0.35f;
            root.Add(glare);
            try { yield return LookMeasure.Render(LookMeasure.ActiveLook, _phase, "glare_rich_text_drop_cap_long"); }
            finally { glare.RemoveFromHierarchy(); }
        }

        [UnityTest]
        public IEnumerator TheFullView()
        {
            _harness.Show(CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == "Nav_text_tabs_big"));
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            sheet.OpenFullView(sheet.Nav.Model.Sections[0].Targets[0]);
            yield return CardTestInput.Settle();
            Assert.IsTrue(sheet.FullView.IsOpen, "precondition: the full view opened");
            yield return LookMeasure.Render(LookMeasure.ActiveLook, _phase, "full_view");
        }
    }

    // The real card on the wall: two cards x two languages x three stops, cropped to the sheet, with the stops and header heights noted
    public class TypefaceMeasureSceneTests : SearchSceneFixture
    {
        private static readonly string[] Pois = { "lamp", "painting" };
        private static readonly string[] Languages = { "en", "pt" };

        [UnityTest]
        public IEnumerator TheCard_AtPeekHalfAndFull([ValueSource(nameof(Pois))] string poi, [ValueSource(nameof(Languages))] string language)
        {
            string phase = LookMeasure.RequirePhase(LookMeasure.ActiveLook);
            // - the card opens in the first language of the wall's list
            Session.CardSettings.languages = new List<string> { language, language == "en" ? "pt" : "en" };
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            var sheet = Card.Sheet;
            Assert.IsTrue(sheet.IsOpen, "precondition: " + poi + "'s card is open");
            LookMeasure.FreezeHero(sheet);

            sheet.SetStop(SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle();
            var header = sheet.Stack.HeaderSlot;
            var title = sheet.Root.Q(className: "card-header__title");
            Assert.IsNotNull(title, "precondition: the card has its title");
            var stops = sheet.Stops;
            LookMeasure.Note(LookMeasure.ActiveLook, phase, $"{poi} {language}: stops peek {stops.Peek:0.0} half {stops.Half:0.0} full {stops.Full:0.0}; " +
                $"header (expanded, at Half) {header.layout.height:0.0}; peek part {sheet.Stack.PeekPart.layout.height:0.0}; title {title.layout.height:0.0}");

            foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
            {
                sheet.SetStop(stop);
                yield return CardTestInput.Settle();
                yield return LookMeasure.Render(LookMeasure.ActiveLook, phase, $"card_{poi}_{language}_{stop.ToString().ToLowerInvariant()}", sheet.Root);
            }
        }
    }
}
