using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the meta family on the real LivingRoomScene: sources.
    public partial class PoiCardSceneTests
    {
        [UnityTest]
        public IEnumerator TheLamp_Sources_ListAndVerified_InTheCardsLanguage()
        {
            LiveSettings.languages = new System.Collections.Generic.List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            var sources = Sheet.Stack.BoundViews.OfType<SourcesBlockView>().ToList();
            Assert.AreEqual(2, sources.Count, "list and with_confidence");
            Assert.AreEqual("Fontes", Sheet.Stack.HeadingOf(sources[0]).text, "the framework's Portuguese default heading");
            Assert.AreEqual("O Grande Panorama de Lisboa, pormenor do castelo (painel de azulejos, fotografia)", sources[0].Rows[0].Title.text);
            Assert.AreEqual("OpenStreetMap contributors", sources[0].Rows[3].Author.text);
            Assert.IsFalse(CardTestInput.IsShown(sources[0].Confidence, sources[0].Root), "list: no chip");
            Assert.AreEqual("Verificado", sources[1].Confidence.text, "with_confidence + verified: the Portuguese chip");
            Sheet.Stack.Scroll.ScrollTo(sources[1].Root);
            yield return CardTestInput.Settle(0.2f);
            yield return Capture("Card_Lamp_Sources");
        }
    }
}
