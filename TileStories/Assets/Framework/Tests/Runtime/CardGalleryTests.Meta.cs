using System.Collections;
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
    // Phase A promises of the meta family: sources.
    public partial class CardGalleryTests
    {
        // ---------------- sources ----------------

        [UnityTest]
        public IEnumerator Sources_TheConfidenceChip_FollowsContentStatus_OnlyInTheWithConfidenceLook()
        {
            SourcesBlockView sources = null;
            yield return ShowBlock("sources_with_confidence_short", v => sources = (SourcesBlockView)v);
            Assert.AreEqual("Sources", _harness.Sheet.Stack.HeadingOf(sources).text, "no heading written: the kind's default heading from the card strings table (6C)");
            Assert.IsTrue(CardTestInput.IsShown(sources.Confidence, sources.Root));
            Assert.AreEqual("Verified", sources.Confidence.text);
            Assert.IsTrue(sources.Confidence.ClassListContains("card-sources__chip--verified"));
            Assert.GreaterOrEqual(CardTestInput.Contrast(sources.Confidence.resolvedStyle.color, CardTestInput.EffectiveBackground(sources.Confidence)),
                UIAccessibility.MinRatioNormalText, "the chip reads");

            yield return ShowBlock("sources_with_confidence_long", v => sources = (SourcesBlockView)v);
            Assert.AreEqual("Draft", sources.Confidence.text);
            Assert.IsTrue(sources.Confidence.ClassListContains("card-sources__chip--draft"));
            Assert.AreEqual(3, sources.Rows.Count);
            Assert.AreEqual("CC BY 4.0", sources.Rows[2].Licence.text);

            yield return ShowBlock("sources_with_confidence_partial", v => sources = (SourcesBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(sources.Confidence, sources.Root), "no Content Status: no chip");
            Assert.IsFalse(CardTestInput.IsShown(sources.Rows[0].Author, sources.Root), "no author: none shown");
            Assert.IsFalse(CardTestInput.IsShown(sources.Rows[0].Licence, sources.Root));

            yield return ShowBlock("sources_list_short", v => sources = (SourcesBlockView)v);
            Assert.IsFalse(CardTestInput.IsShown(sources.Confidence, sources.Root), "the list look never shows the chip, even when verified");
        }
    }
}
