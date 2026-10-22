using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Dev.StyleSpike.Tests
{
    // DEV-ONLY (_3.3 P0): proves the spike's own plumbing before any number is read from it -- the stage really draws on the card's
    // PanelSettings at the pinned 390 x 844 frame, and the profiler counters really count (a 0 would make every cost row a lie).
    public class SpikeStageTests
    {
        public const string PanelSettingsPath = "Assets/Framework/Runtime/UI/Shared/PanelSettings.asset";

        [UnityTest]
        public IEnumerator TheStage_DrawsAPanelElementAtThePinnedFrame_AndTheCountersCount()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panel, "the card's PanelSettings is missing -- SETUP is wrong");
            using var stage = new SpikeStage(panel, Color.black);
            var box = new VisualElement();
            box.style.position = Position.Absolute;
            box.style.left = 100; box.style.top = 200; box.style.width = 100; box.style.height = 100;
            box.style.backgroundColor = new Color(1f, 0f, 0f, 1f);
            stage.Root.Add(box);

            yield return null;
            yield return null;
            Assert.AreEqual(new Vector2(390, 844), new Vector2(stage.Root.resolvedStyle.width, stage.Root.resolvedStyle.height),
                "the panel is not the pinned 390 x 844 frame -- SETUP (frame pin) is wrong");

            Texture2D tex = null;
            yield return SpikeEvidence.Grab(t => tex = t);
            try
            {
                var inside = SpikeEvidence.PixelAt(stage.Root, new Vector2(150, 250), tex);
                var outside = SpikeEvidence.PixelAt(stage.Root, new Vector2(50, 50), tex);
                Assert.Greater(inside.r, 0.95f, $"the red box is not under its own centre: {inside}");
                Assert.Less(outside.r, 0.05f, $"the clear colour is not around it: {outside}");
            }
            finally { Object.Destroy(tex); }

            CostSample cost = default;
            yield return SpikeCost.Measure(30, c => cost = c);
            SpikeEvidence.Note("stage", "empty stage + one box: " + cost);
            Assert.Greater(cost.Batches, 0, "the Batches counter reads 0 -- every cost row would be meaningless");
            Assert.Greater(cost.UpdatePanelsMs, 0, "the UIElementsUpdatePanels marker reads 0 -- the UI cost rows would be meaningless");
            Assert.Greater(cost.RepaintPanelsMs, 0, "the UIElementsRepaintPanels marker reads 0");
        }
    }
}
