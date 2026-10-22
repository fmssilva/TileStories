using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TextCoreFontAsset = UnityEngine.TextCore.Text.FontAsset;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0 S2a): can ONE font asset serve both UI Toolkit (the card) and a TMP marker label on 6000.3, or does each font
    // key need a TextCore FontAsset AND a TMP_FontAsset made from the same .ttf? Proven on real elements: a Label is measured with
    // each candidate against a no-font control (a dropped face falls back to the theme font and measures like the control).
    public class S2FontSharingTests
    {
        private const string FontDir = "Assets/Dev/StyleSpike/Fonts/Fraunces/";
        private const string Sample = "Castelo de Sao Jorge, Paco da Ribeira";   // ASCII: the measurement needs no accents
        private const float FontSize = 28f;

        private SpikeStage _stage;
        private GameObject _canvas;

        [TearDown]
        public void TearDown()
        {
            _stage?.Dispose();
            if (_canvas != null) Object.Destroy(_canvas);
        }

        [Test]
        public void TheTwoFontAssetTypes_AreUnrelated()
        {
            Assert.IsFalse(typeof(TextCoreFontAsset).IsAssignableFrom(typeof(TMP_FontAsset)), "a TMP_FontAsset is not a TextCore FontAsset");
            Assert.IsFalse(typeof(TMP_FontAsset).IsAssignableFrom(typeof(TextCoreFontAsset)), "a TextCore FontAsset is not a TMP_FontAsset");
            SpikeEvidence.Note("S2a", $"TMP_FontAsset : {typeof(TMP_FontAsset).BaseType.Name} : {typeof(TMP_FontAsset).BaseType.BaseType.Name}; FontAsset : {typeof(TextCoreFontAsset).BaseType.FullName}");
        }

        [UnityTest]
        public IEnumerator ALabel_TakesTheTextCoreAssetOrTheTtf_ButDropsATmpAssetGivenByUss_AndTmpDrawsItsOwnAsset()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(SpikeStageTests.PanelSettingsPath);
            var uitk = AssetDatabase.LoadAssetAtPath<TextCoreFontAsset>(FontDir + "Fraunces-SemiBold UITK.asset");
            var tmp = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir + "Fraunces-SemiBold TMP.asset");
            var ttf = AssetDatabase.LoadAssetAtPath<Font>(FontDir + "Fraunces-SemiBold.ttf");
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dev/StyleSpike/S2Fonts/FontByUss.uss");
            Assert.IsNotNull(uitk, "UITK asset missing -- SETUP"); Assert.IsNotNull(tmp, "TMP asset missing -- SETUP");
            Assert.IsNotNull(ttf, "ttf missing -- SETUP"); Assert.IsNotNull(sheet, "FontByUss.uss missing -- SETUP");

            _stage = new SpikeStage(panel, Color.white);
            _stage.AddSheet(sheet);
            var control = Line(null, 20);
            var byCode = Line(null, 70);
            byCode.style.unityFontDefinition = FontDefinition.FromSDFFont(uitk);
            var byUssUitk = Line("spike-font-uitk", 120);
            var byUssTmp = Line("spike-font-tmp", 170);
            var byUssTtf = Line("spike-font-ttf", 220);
            var byUssDefTtf = Line("spike-font-def-ttf", 320);
            var byCodeTtf = Line(null, 270);
            byCodeTtf.style.unityFontDefinition = FontDefinition.FromFont(ttf);

            // the marker side: a real TMP label on a screen overlay canvas, lower half
            _canvas = new GameObject("S2a Canvas", typeof(Canvas), typeof(CanvasScaler));
            _canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var tmpGo = new GameObject("TMP label", typeof(RectTransform), typeof(TextMeshProUGUI));
            tmpGo.transform.SetParent(_canvas.transform, false);
            var tmpText = tmpGo.GetComponent<TextMeshProUGUI>();
            tmpText.font = tmp;
            tmpText.fontSize = FontSize;
            tmpText.color = Color.black;
            tmpText.text = Sample;
            var rt = tmpGo.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.25f);
            rt.sizeDelta = new Vector2(1000, 60);

            for (int i = 0; i < 5; i++) yield return null;

            float w(Label l) => l.resolvedStyle.width;
            SpikeEvidence.Note("S2a", $"label widths: control {w(control):0.0}, FromSDFFont(UITK) {w(byCode):0.0}, uss UITK {w(byUssUitk):0.0}, " +
                $"uss TMP {w(byUssTmp):0.0}, uss -unity-font ttf {w(byUssTtf):0.0}, uss -unity-font-definition ttf {w(byUssDefTtf):0.0}, " +
                $"FromFont(ttf) {w(byCodeTtf):0.0}; TMP preferred width {tmpText.preferredWidth:0.0}");
            SpikeEvidence.Note("S2a", $"uss TMP resolved fontAsset={(byUssTmp.resolvedStyle.unityFontDefinition.fontAsset != null)} font={(byUssTmp.resolvedStyle.unityFontDefinition.font != null)}");
            yield return SpikeEvidence.Capture("S2a_font_sharing");

            Assert.Greater(w(control), 0, "the control line has no width -- SETUP");
            Assert.Greater(Mathf.Abs(w(control) - w(byCode)), 0.5f, "FromSDFFont(TextCore asset) must change the face");
            Assert.AreEqual(w(byCode), w(byUssUitk), 0.5f, "the USS url to the TextCore asset gives the same face as code");
            // measured on 6000.3: worse than a fallback -- the label draws NOTHING, silently (no import warning, no log)
            Assert.Less(w(byUssTmp), FontSize, "a TMP asset given by USS leaves the label blank (narrower than one glyph)");
            Assert.IsNull(byUssTmp.resolvedStyle.unityFontDefinition.fontAsset, "no TextCore asset resolves from a TMP url");
            Assert.AreEqual(w(control), w(byUssTtf), 0.5f, "-unity-font: url(ttf) loses to the theme's -unity-font-definition");
            Assert.Greater(Mathf.Abs(w(control) - w(byCodeTtf)), 0.5f, "a plain Font (.ttf) also serves a Label (FromFont)");
            Assert.AreEqual(tmp, tmpText.font, "the TMP label keeps its TMP asset");
            Assert.Greater(tmpText.textInfo.characterCount, 0, "the TMP label laid out characters");
        }

        // A black 28 px line of the sample at a fixed top, optionally carrying one USS class
        private Label Line(string ussClass, float top)
        {
            var l = new Label(Sample);
            if (ussClass != null) l.AddToClassList(ussClass);
            l.style.position = Position.Absolute;
            l.style.left = 8; l.style.top = top;
            l.style.fontSize = FontSize;
            l.style.color = Color.black;
            l.style.whiteSpace = WhiteSpace.NoWrap;
            _stage.Root.Add(l);
            return l;
        }
    }
}
