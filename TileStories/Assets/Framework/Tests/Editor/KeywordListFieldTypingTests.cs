using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // Every Search Keywords cell (taxonomy tables, keyword fields, a POI's Others, synonym rows) is a
    // comma-separated text field that re-parses its list on every keystroke. The worry: parsing "castle,"
    // gives [castle], which joins back to "castle" -- would the comma vanish before the next word is
    // typed? This types into the REAL cell (DrawKeywordListField inside a real OnGUI, a real click to
    // focus it, real KeyDown character events) and reads the resulting list. No mocks.
    public class KeywordListFieldTypingTests
    {
        private static readonly MethodInfo Draw = typeof(POIEditorToolWindow).GetMethod("DrawKeywordListField",
            BindingFlags.NonPublic | BindingFlags.Static);

        private sealed class Host : EditorWindow
        {
            public static List<string> Keywords = new();
            public static Rect FieldRect;
            public static int Repaints;

            private void OnGUI()
            {
                Keywords = (List<string>)Draw.Invoke(null, new object[] { Keywords, new[] { GUILayout.Width(320f) } });
                if (Event.current.type == EventType.Repaint)
                {
                    FieldRect = GUILayoutUtility.GetLastRect();
                    Repaints++;
                }
            }
        }

        private Host _host;

        [SetUp]
        public void SetUp()
        {
            Assert.IsNotNull(Draw, "DrawKeywordListField must exist");
            Host.Keywords = new List<string>();
            Host.Repaints = 0;
            _host = ScriptableObject.CreateInstance<Host>();
            _host.ShowUtility();
            _host.position = new Rect(60f, 60f, 420f, 120f);
            // - an IMGUI text field only edits while its window has focus: without this the click lands but
            //   every keystroke is dropped whenever Unity is not the foreground application
            _host.Focus();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) _host.Close();
            GUIUtility.keyboardControl = 0;
        }

        private IEnumerator Repaint()
        {
            int before = Host.Repaints;
            _host.Repaint();
            for (int i = 0; i < 120 && Host.Repaints == before; i++)
                yield return null;
            Assert.Greater(Host.Repaints, before, "the host window must have repainted");
        }

        private IEnumerator Type(string text)
        {
            foreach (char c in text)
            {
                _host.SendEvent(new Event { type = EventType.KeyDown, character = c, keyCode = KeyCode.None });
                yield return Repaint();
            }
        }

        [UnityTest]
        public IEnumerator TypingTwoKeywordsWithACommaBetween_GivesTwoKeywords()
        {
            yield return Repaint();
            Vector2 centre = Host.FieldRect.center;
            _host.SendEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = centre });
            _host.SendEvent(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = centre });
            yield return Repaint();
            Assert.AreNotEqual(0, GUIUtility.keyboardControl, "precondition: the click must focus the keyword field");

            yield return Type("castle, tower");

            CollectionAssert.AreEqual(new[] { "castle", "tower" }, Host.Keywords,
                "typing 'castle, tower' must give two keywords (the comma must survive until the next word)");
        }
    }
}
