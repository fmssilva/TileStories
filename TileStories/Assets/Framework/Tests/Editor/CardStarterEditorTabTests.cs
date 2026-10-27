using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Specific Marker > Card Content > Card Starter (_3.2 step 17b) on the REAL window (PoiEditorWindowHost) and the shipped wall's real
    // points: the row is the section's first and drawing it writes nothing; a real click on the button opens the picker; a real click on a
    // starter's row writes exactly its blocks into an empty point (The Camera) and asks nothing, and ONE real Ctrl+Z gives the whole config
    // back; Start from scratch writes nothing there; on a point with blocks (Lamp - Military, 6) the ONE question names the point by its
    // list title and how many blocks go -- Cancel changes nothing, Confirm replaces them, and one Ctrl+Z gives every block back, walked
    // field by field. Answers are scripted through EditorDecision.Responder: a test never meets a modal (40-testing 4.2.2).
    public class CardStarterEditorTabTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string EmptyPoint = "camera";
        private const string AuthoredPoint = "lamp_military";

        private PoiEditorWindowHost _window;
        private readonly List<DecisionRequest> _asked = new();

        [TearDown]
        public void TearDown()
        {
            EditorDecision.Responder = TestDialogGuard.CancelEverything;
            NamedOptionPickerPopup.RowRectProbe = null;
            HelpInfoButton.DrawnProbe = null;
            EditorPopup.CloseAll();
            _window?.Close();
        }

        private static WallConfigData ShippedConfig() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private POIData Point(string id) => _window.Config.pois.Single(p => p.id == id);
        private int IndexOf(string id) => _window.Config.pois.FindIndex(p => p.id == id);

        // The window on the shipped wall, Specific Marker > `poiId` > Card Content open; every question recorded and answered `answer`
        private IEnumerator Open(string poiId, DecisionAnswer answer = DecisionAnswer.Cancel)
        {
            _asked.Clear();
            EditorDecision.Responder = request => { _asked.Add(request); return answer; };
            _window = new PoiEditorWindowHost(ShippedConfig(), "_showPoiCardContent");
            var tabField = typeof(POIEditorToolWindow).GetField("_selectedTab", Instance);
            _window.SetWindowField("_selectedTab", System.Enum.Parse(tabField.FieldType, "SpecificMarker"));
            var foldouts = (Dictionary<string, bool>)typeof(POIEditorToolWindow).GetField("_poiFoldouts", Instance).GetValue(_window.Editor);
            foldouts[poiId] = true;
            yield return _window.WaitForRepaint();
        }

        // A real click on the Card Starter button, then the picker's rows once drawn (their rects, in the popup's own space)
        private IEnumerator OpenThePicker(List<Rect> rows)
        {
            rows.Clear();
            var seen = new Dictionary<int, Rect>();
            NamedOptionPickerPopup.RowRectProbe = (i, r) => seen[i] = r;
            yield return _window.ScrollTo("Card Starter");
            _window.Click("Card Starter");
            var popup = EditorPopup.FindOpen(CardStarters.PickerKind);
            Assert.IsNotNull(popup, "a real click on the Card Starter button opens the picker");
            // - one row per starter, then one per block template (S3b: the templates offered beside the sets)
            int expected = CardStarters.All.Count + BlockTemplates.All(_window.Config.card_settings, ElementRegistry.Shared).Count;
            for (int frame = 0; frame < 90 && seen.Count < expected; frame++)
            {
                popup.Repaint();
                yield return null;
            }
            Assert.AreEqual(expected, seen.Count, "the picker drew one row per starter and per template");
            for (int i = 0; i < seen.Count; i++) rows.Add(seen[i]);
        }

        // A real click on picker row `index`
        private IEnumerator Pick(int index)
        {
            var rows = new List<Rect>();
            yield return OpenThePicker(rows);
            var popup = EditorPopup.FindOpen(CardStarters.PickerKind);
            popup.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = rows[index].center });
            yield return _window.WaitForRepaint();
            Assert.IsNull(EditorPopup.FindOpen(CardStarters.PickerKind), CardStarters.All[index].Label + ": a pick closes the picker");
        }

        private static int Index(string label) => CardStarters.All.ToList().FindIndex(s => s.Label == label);

        // A point's blocks, walked field by field into one comparable line each (key, kind, look, display and every field's value)
        private static List<string> Walk(POIData poi) =>
            (poi.card?.blocks ?? new List<BlockInstanceData>()).Select(b => b.key + "|" + b.kind + "|" + b.variant + "|" + b.display + "|"
                + string.Join(";", b.fields.Select(f => f.key + "=" + JsonUtility.ToJson(f)))).ToList();

        [UnityTest]
        public IEnumerator TheRow_IsCardContentsFirstRow_ReadsTheStarterTheBlocksFollow_AndDrawingItWritesNothing()
        {
            yield return Open(AuthoredPoint);
            var titles = new List<(string Title, Rect Rect)>();
            HelpInfoButton.DrawnProbe = (title, rect) => titles.Add((title, rect));
            yield return _window.WaitForRepaint();
            HelpInfoButton.DrawnProbe = null;
            Assert.IsFalse(_window.Unsaved, "drawing Card Content, the Card Starter row included, writes nothing");
            Assert.AreEqual(0, _window.UndoStepsLeft);
            float Y(string title) => titles.Last(t => t.Title == title).Rect.y;
            Assert.Less(Y("Card Content"), Y("Card Starter"), "under the section's title row");
            Assert.Less(_window.RectOf("Card Starter").y, _window.RectOf("Card block open#0").y, "above the first block row: the section's first row");
            Assert.Less(_window.RectOf("Card Starter").y, _window.RectOf("Card add block#0").y);
            Assert.IsNull(CardStarters.Matching(Point(AuthoredPoint)), "Lamp - Military's authored card follows no starter: the button reads Custom");
        }

        [UnityTest]
        public IEnumerator EveryStarter_ARealClickOnAnEmptyPoint_WritesExactlyItsBlocks_AsksNothing_AndOneCtrlZGivesTheWholeConfigBack()
        {
            foreach (var starter in CardStarters.All.Where(s => !s.IsEmpty))
            {
                yield return Open(EmptyPoint);
                Assert.IsEmpty(Walk(Point(EmptyPoint)), "precondition: " + EmptyPoint + " has no blocks");
                string before = JsonUtility.ToJson(_window.Config);
                int steps = _window.UndoStepsLeft;

                yield return Pick(Index(starter.Label));
                Assert.IsEmpty(_asked, starter.Label + ": an empty point asks nothing (nothing authored is at stake)");
                var blocks = Point(EmptyPoint).card.blocks;
                CollectionAssert.AreEqual(starter.Entries.Select(e => e.Kind), blocks.Select(b => b.kind), starter.Label + ": exactly its kinds, in order");
                CollectionAssert.AreEqual(starter.Entries.Select(e => e.Variant), blocks.Select(b => b.variant), starter.Label + ": in their looks");
                // - nothing else in the whole config moved: put the point's card back as it was and the two configs are the same text
                var after = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(_window.Config));
                after.pois.Single(p => p.id == EmptyPoint).card = JsonUtility.FromJson<WallConfigData>(before).pois.Single(p => p.id == EmptyPoint).card;
                Assert.AreEqual(before, JsonUtility.ToJson(after), starter.Label + ": only this point's blocks changed");
                Assert.AreEqual(steps + 1, _window.UndoStepsLeft, starter.Label + ": ONE undo step");
                Assert.IsTrue(_window.Unsaved);

                yield return _window.PressUndo();
                Assert.AreEqual(before, JsonUtility.ToJson(_window.Config), starter.Label + ": one real Ctrl+Z gives the whole config back");
                _window.Close();
                _window = null;
            }
        }

        [UnityTest]
        public IEnumerator StartFromScratch_ARealClickOnAnEmptyPoint_WritesNothing_NoUndoStep_StillSaved()
        {
            yield return Open(EmptyPoint);
            string before = JsonUtility.ToJson(_window.Config);
            yield return Pick(Index("Start from scratch"));
            Assert.IsEmpty(_asked, "nothing to ask");
            Assert.AreEqual(before, JsonUtility.ToJson(_window.Config), "nothing written");
            Assert.AreEqual(0, _window.UndoStepsLeft, "no undo step");
            Assert.IsFalse(_window.Unsaved, "the config is still saved");
        }

        [UnityTest]
        public IEnumerator OnAPointWithBlocks_TheQuestionNamesThePointAndTheCount_AndCancelChangesNothing()
        {
            yield return Open(AuthoredPoint, DecisionAnswer.Cancel);
            var walked = Walk(Point(AuthoredPoint));
            Assert.AreEqual(6, walked.Count, "precondition: Lamp - Military's 6 authored blocks");
            string before = JsonUtility.ToJson(_window.Config);

            yield return Pick(Index("Rich point"));
            Assert.AreEqual(1, _asked.Count, "ONE question");
            var question = _asked[0];
            string name = EditorNames.Poi(IndexOf(AuthoredPoint), Point(AuthoredPoint));
            StringAssert.Contains(name + " has 6 blocks", question.Message, "the point by its list title, and how many blocks go");
            StringAssert.Contains("Rich point replaces them with its own " + CardStarters.All[Index("Rich point")].Entries.Count + " blocks", question.Message);
            Assert.AreEqual("Replace 6 Blocks", question.ConfirmLabel, "a verb and the count, never OK");
            Assert.IsNull(question.AlternativeLabel, "Replace or Cancel, nothing else");
            StringAssert.DoesNotContain(AuthoredPoint, question.Message, "never the point's id");
            EditorTextChecks.AssertAscii(question.Title + question.Message + question.ConfirmLabel, "the question");

            Assert.AreEqual(before, JsonUtility.ToJson(_window.Config), "Cancel: nothing changes");
            CollectionAssert.AreEqual(walked, Walk(Point(AuthoredPoint)), "every block as it was");
            Assert.AreEqual(0, _window.UndoStepsLeft, "no undo step");
            Assert.IsFalse(_window.Unsaved);
        }

        [UnityTest]
        public IEnumerator OnAPointWithBlocks_ConfirmReplacesThem_AndOneCtrlZGivesEveryBlockBack_WalkedFieldByField()
        {
            yield return Open(AuthoredPoint, DecisionAnswer.Confirm);
            var walked = Walk(Point(AuthoredPoint));
            string before = JsonUtility.ToJson(_window.Config);
            var simple = CardStarters.All[0];

            yield return Pick(0);
            Assert.AreEqual(1, _asked.Count, "asked first");
            CollectionAssert.AreEqual(simple.Entries.Select(e => e.Kind), Point(AuthoredPoint).card.blocks.Select(b => b.kind), "Confirm: the starter's blocks");
            CollectionAssert.IsEmpty(Point(AuthoredPoint).card.blocks.Select(b => b.key).Intersect(walked.Select(w => w.Split('|')[0])),
                "no replaced key reused");
            Assert.AreEqual(1, _window.UndoStepsLeft, "ONE undo step");

            yield return _window.PressUndo();
            CollectionAssert.AreEqual(walked, Walk(Point(AuthoredPoint)), "one real Ctrl+Z: every block back -- key, kind, look, display, every field");
            Assert.AreEqual(before, JsonUtility.ToJson(_window.Config), "and the whole config exactly as it was");

            // - Start from scratch over blocks asks the same way, with its own verb
            _asked.Clear();
            yield return Pick(Index("Start from scratch"));
            Assert.AreEqual("Remove 6 Blocks", _asked.Single().ConfirmLabel);
            CollectionAssert.IsEmpty(Point(AuthoredPoint).card.blocks, "Confirm: an empty card");
            yield return _window.PressUndo();
            CollectionAssert.AreEqual(walked, Walk(Point(AuthoredPoint)), "and one Ctrl+Z gives them back");
        }

        [Test]
        public void TheQuestion_SaysWhatIsReallyAtStake_InEachCase()
        {
            var one = POIEditorToolWindow.BuildCardStarterQuestion("4. North tower", 1, "Simple point", 5);
            StringAssert.Contains("4. North tower has 1 block on its card", one.Message, "singular");
            Assert.AreEqual("Replace 1 Block", one.ConfirmLabel);
            StringAssert.Contains("Simple point replaces them with its own 5 blocks, with nothing written in them yet", one.Message);
            StringAssert.Contains("one Ctrl+Z gives them back", one.Message, "the way back is said");

            var scratch = POIEditorToolWindow.BuildCardStarterQuestion("4. North tower", 12, "Start from scratch", 0);
            Assert.AreEqual("Remove 12 Blocks", scratch.ConfirmLabel);
            StringAssert.Contains("removes them all", scratch.Message);
            StringAssert.DoesNotContain("its own 0 blocks", scratch.Message, "never a promise of blocks it does not write");
            Assert.AreEqual("Replace this card's blocks?", scratch.Title, "the title names the situation");
        }
    }
}
