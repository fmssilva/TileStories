using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S2c): the font bake-off for P4 -- one mock block (title, lede, body, caption, button) at 390 pt in light
    // mode, in three pairs, each again under a white "glare" layer at 35 % (sun on the screen). Renders for the developer's pick and
    // the body text's contrast measured on the PIXELS (the project renders in linear colour, so a 35 % layer is not a 35 % sRGB mix).
    public class S2FontBakeOffTests
    {
        private const string Uxml = "Assets/Dev/StyleSpike/S2Fonts/BakeOffBlock.uxml";
        private const string Uss = "Assets/Dev/StyleSpike/S2Fonts/BakeOff.uss";
        private const float GlareOpacity = 0.35f;

        private static readonly (string cls, string name)[] Pairs =
        {
            ("pair-fraunces", "1  Fraunces SemiBold + Atkinson Hyperlegible Next"),
            ("pair-sourceserif", "2  Source Serif 4 SemiBold + Inter"),
            ("pair-playfair", "3  Playfair Display SemiBold + Plus Jakarta Sans (Stitch reference)"),
        };

        [UnityTest]
        public IEnumerator ThreePairs_PlainAndUnderGlare_RenderedAndMeasured()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(SpikeStageTests.PanelSettingsPath);
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Uss);
            Assert.IsNotNull(tree, "BakeOffBlock.uxml missing -- SETUP"); Assert.IsNotNull(sheet, "BakeOff.uss missing -- SETUP");

            var plain = new List<Texture2D>();
            var glared = new List<Texture2D>();
            using var stage = new SpikeStage(panel, Color.black);
            stage.AddSheet(sheet);
            try
            {
                foreach (var (cls, name) in Pairs)
                {
                    stage.Root.Clear();
                    stage.Root.ClearClassList();
                    stage.Root.AddToClassList(cls);
                    tree.CloneTree(stage.Root);
                    var ground = stage.Root.Q("bakeoff-ground");
                    ground.style.flexGrow = 1;
                    stage.Root.Q<Label>("bo-pair").text = name;
                    for (int i = 0; i < 4; i++) yield return null;

                    var block = stage.Root.Q("bo-block");
                    var body = stage.Root.Q<Label>("bo-body");
                    var title = stage.Root.Q<Label>("bo-title");
                    Assert.Greater(body.resolvedStyle.height, 17f, $"{cls}: the body did not lay out -- SETUP");
                    Assert.IsTrue(block.worldBound.yMax <= stage.Root.worldBound.yMax, $"{cls}: the block runs off the 844 pt frame ({block.worldBound.yMax:0})");

                    Texture2D a = null;
                    yield return SpikeEvidence.Grab(t => a = t);
                    plain.Add(a);
                    float plainRatio = BodyContrast(stage.Root, block, body, a, out Color ink, out Color paper);

                    var glare = new VisualElement { pickingMode = PickingMode.Ignore };
                    glare.AddToClassList("bo-glare");
                    glare.style.opacity = GlareOpacity;
                    ground.Add(glare);
                    for (int i = 0; i < 2; i++) yield return null;
                    Texture2D b = null;
                    yield return SpikeEvidence.Grab(t => b = t);
                    glared.Add(b);
                    float glareRatio = BodyContrast(stage.Root, block, body, b, out Color inkG, out Color paperG);

                    // what a 35 % mix in sRGB (the designer's mental model) would have given for the same ink
                    Color srgbMix = Color.Lerp(ink, Color.white, GlareOpacity);
                    float srgbRatio = UIAccessibility.ContrastRatio(srgbMix, Color.white);
                    SpikeEvidence.Note("S2c", $"{cls}: title h {title.resolvedStyle.height:0}, body h {body.resolvedStyle.height:0}, block bottom {block.worldBound.yMax:0}; " +
                        $"body ink {(Color32)ink} on {(Color32)paper} = {plainRatio:0.00}:1; glare ink {(Color32)inkG} on {(Color32)paperG} = {glareRatio:0.00}:1 " +
                        $"(a 35 % sRGB mix would give {srgbRatio:0.00}:1)");
                    SpikeEvidence.Save(a, $"S2c_bakeoff_{cls}");
                    SpikeEvidence.Save(b, $"S2c_bakeoff_{cls}_glare");
                    Assert.Greater(plainRatio, glareRatio, $"{cls}: the glare layer must lower the measured contrast -- SETUP (glare not drawn)");
                }

                var all = new List<Texture2D>(plain); all.AddRange(glared);
                var sheetTex = SpikeEvidence.SideBySide(all);
                SpikeEvidence.Save(sheetTex, "S2c_bakeoff_all_plain_then_glare");
                Object.Destroy(sheetTex);
            }
            finally
            {
                foreach (var t in plain) Object.Destroy(t);
                foreach (var t in glared) Object.Destroy(t);
            }
        }

        // The body text's rendered contrast: its darkest pixel (a glyph's core) against the block's own paper just inside its padding
        private static float BodyContrast(VisualElement root, VisualElement block, Label body, Texture2D tex, out Color ink, out Color paper)
        {
            var r = body.worldBound;
            ink = Color.white;
            for (float y = r.yMin + 2; y < r.yMax - 2; y += 1f)
                for (float x = r.xMin + 2; x < r.xMax - 2; x += 1f)
                {
                    var c = SpikeEvidence.PixelAt(root, new Vector2(x, y), tex);
                    if (c.grayscale < ink.grayscale) ink = c;
                }
            paper = SpikeEvidence.PixelAt(root, new Vector2(block.worldBound.xMin + 8, block.worldBound.yMin + 8), tex);
            return UIAccessibility.ContrastRatio(ink, paper);
        }
    }
}
