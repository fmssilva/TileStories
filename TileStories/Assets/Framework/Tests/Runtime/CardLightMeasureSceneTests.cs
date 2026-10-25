using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The cost of the card's moving light (_3.3 P4.4c2, R8 S6's method: the engine's counters, never the Editor's frame time), run only while
    // TestEvidence/Light/MEASURING exists: The Lamp at Full, the light running, at the offset that shows the most sheets at once, measured
    // over 60 frames each -- batches, SetPass calls and UI Toolkit's CPU (UIElementsUpdatePanels + UIElementsRepaintPanels):
    //   before = the light layers hidden (the card as P4.4c1 drew it), after = the card as it ships (the glass of the sheets near the view
    //   + the rim light). The gating itself was decided on a run with every sheet lit (the Build log has those numbers).
    // Editor numbers rank variants on one machine; a phone's cost is the device check (S9). Written to TestEvidence/Light/cost.txt.
    public class CardLightMeasureSceneTests : SearchSceneFixture
    {
        private const int Frames = 60;
        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;

        private struct Cost
        {
            public double Batches, SetPass, UiMs;
            public override string ToString() => $"batches {Batches:0.0}, setpass {SetPass:0.0}, UI cpu {UiMs:0.000} ms";
        }

        private static IEnumerator Measure(System.Action<Cost> got)
        {
            for (int i = 0; i < 10; i++) yield return null;
            using var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            using var setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            using var update = ProfilerRecorder.StartNew(new ProfilerCategory("PlayerLoop"), "PreLateUpdate.UIElementsUpdatePanels");
            using var repaint = ProfilerRecorder.StartNew(new ProfilerCategory("PlayerLoop"), "PostLateUpdate.UIElementsRepaintPanels");
            var cost = new Cost();
            for (int i = 0; i < Frames; i++)
            {
                yield return null;
                cost.Batches += batches.LastValue;
                cost.SetPass += setPass.LastValue;
                cost.UiMs += (update.LastValue + repaint.LastValue) * 1e-6;
            }
            cost.Batches /= Frames;
            cost.SetPass /= Frames;
            cost.UiMs /= Frames;
            got(cost);
        }

        private bool InView(SectionSheets.Sheet sheet)
        {
            var view = Stack.Scroll.contentViewport.worldBound;
            var r = sheet.Root.worldBound;
            return sheet.Root.resolvedStyle.display == DisplayStyle.Flex && r.yMax > view.yMin && r.yMin < view.yMax;
        }

        private static void Note(string line)
        {
            File.AppendAllText(TestEvidence.PathFor("Light", "cost.txt"), line + "\n");
            Debug.Log("[Card] cost " + line);
        }

        [UnityTest]
        public IEnumerator TheLight_Cost_OnTheLampAtFull()
        {
            if (!File.Exists(Path.Combine(TestEvidence.Root, "Light", "MEASURING")))
                Assert.Ignore("A measurement: create TestEvidence/Light/MEASURING to run it");
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            SelectionEventBus.Select("lamp");
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            CardTestInput.HoldTheWallStill(Sheet);
            LookMeasure.FreezeHero(Sheet);

            // - the offset showing the most sheets at once (short sheets cluster in places)
            float range = NavScrollRule.Range(Stack.Scroll.contentContainer.layout.height, Stack.Scroll.contentViewport.layout.height);
            float best = 0f;
            int most = 0;
            for (float offset = 0f; offset <= range; offset += 60f)
            {
                Stack.Scroll.scrollOffset = new Vector2(0f, offset);
                yield return null;
                int n = Stack.Sheets.All.Count(InView);
                if (n > most) { most = n; best = offset; }
            }
            Stack.Scroll.scrollOffset = new Vector2(0f, best);
            yield return CardTestInput.Settle(0.6f);
            var sheets = Stack.Sheets.All.Where(s => s.Root.resolvedStyle.display == DisplayStyle.Flex && !s.Run.Bare).ToList();
            Assert.Greater(most, 1, "precondition: several sheets in view");
            Assert.IsNotNull(Stack.RimLight.Layer.parent, "precondition: the rim light runs");

            Cost after = default, before = default;
            yield return Measure(c => after = c);

            foreach (var s in sheets) s.Light.style.display = DisplayStyle.None;
            Stack.RimLight.Layer.style.display = DisplayStyle.None;
            yield return Measure(c => before = c);

            foreach (var s in sheets) s.Light.style.display = StyleKeyword.Null;
            Stack.RimLight.Layer.style.display = StyleKeyword.Null;
            Stack.RimLight.Update();
            int drawn = sheets.Count(s => s.Light.resolvedStyle.display == DisplayStyle.Flex);

            Note($"The Lamp at Full, offset {best:0} ({most} sheets in view, {sheets.Count} sheets with glass on the card, {drawn} drawn near the view), {Frames} frames:");
            Note("  before (no light layers):                    " + before);
            Note("  after (the glass near the view + the rim):   " + after);
            Assert.GreaterOrEqual(after.Batches, before.Batches, "a custom material does not batch with UI Toolkit's own shader");
        }
    }
}
