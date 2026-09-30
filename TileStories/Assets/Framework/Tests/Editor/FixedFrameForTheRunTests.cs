using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // _3.1 10A.2c.4: CardTestInput.FixedGameViewSize (what FixedFrameForTheRun pins for the whole run) used to find and
    // pin only the FIRST open Game view. PanelSettings' "Match Width Or Height" scaling reads the REAL Game view's own
    // pixel size, so a second, unpinned Game view (a developer's own floating one, left open from something else) broke
    // pixel-based card tests exactly as surely as no pin at all (10A.2b.2, found again the same way). The fix pins
    // every open Game view; this test proves it with a real second window, not a single mocked one.
    public class FixedFrameForTheRunTests
    {
        private EditorWindow _extraGameView;
        private EditorWindow _previouslyFocused;

        [TearDown]
        public void TearDown()
        {
            if (_extraGameView != null) _extraGameView.Close();
            _extraGameView = null;
            // - Show() below focuses the new window, same as a developer opening one by hand; give focus back so a
            //   later real-click test in the same run (40-testing 4.2.3) does not lose it to this one's own cleanup
            if (_previouslyFocused != null) _previouslyFocused.Focus();
            _previouslyFocused = null;
        }

        [Test]
        public void PinningTheFrame_SetsTheSameFixedSize_OnEveryOpenGameView_NotJustTheFirst()
        {
            _previouslyFocused = EditorWindow.focusedWindow;
            var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            Assert.IsNotNull(gameViewType, "UnityEditor.GameView must exist to open a second one for this test");
            _extraGameView = (EditorWindow)ScriptableObject.CreateInstance(gameViewType);
            _extraGameView.Show();

            var allGameViews = Resources.FindObjectsOfTypeAll(typeof(EditorWindow))
                .Where(w => w.GetType().Name == "GameView")
                .Cast<EditorWindow>()
                .ToList();
            Assert.GreaterOrEqual(allGameViews.Count, 2, "a second, real Game view window is open for this test");

            var selectedSizeIndexProperty = gameViewType.GetProperty("selectedSizeIndex",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // - remember every window's REAL original size so it can be put back exactly, whatever this test does to
            //   it; forcing a "definitely different" baseline first would make FixedGameViewSize capture that forced
            //   value as "previous" and restore to IT on Dispose, corrupting the Editor's own layout after the test
            var originalIndices = allGameViews.Select(gv => (int)selectedSizeIndexProperty.GetValue(gv, null)).ToList();
            try
            {
                // - force every window to a size that is definitely not the fixed test size first, so the assert
                //   below cannot pass by coincidence (both windows already happening to share whatever size they had)
                foreach (var gv in allGameViews) selectedSizeIndexProperty.SetValue(gv, 0, null);

                using (new CardTestInput.FixedGameViewSize(390, 844))
                {
                    var indices = allGameViews.Select(gv => (int)selectedSizeIndexProperty.GetValue(gv, null)).ToList();
                    Assert.AreNotEqual(0, indices[0], "the pin actually moved the size away from the forced baseline");
                    for (int i = 1; i < indices.Count; i++)
                        Assert.AreEqual(indices[0], indices[i],
                            $"'{allGameViews[i].titleContent.text}' was pinned to the same fixed size as '{allGameViews[0].titleContent.text}'");
                }
            }
            finally
            {
                for (int i = 0; i < allGameViews.Count; i++)
                    if (allGameViews[i] != null) selectedSizeIndexProperty.SetValue(allGameViews[i], originalIndices[i], null);
            }
        }
    }
}
