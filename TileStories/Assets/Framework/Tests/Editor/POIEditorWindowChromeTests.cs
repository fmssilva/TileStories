using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 structural guards for the POI Editor window's chrome decluttering:
    // (1) the redundant "POI Editor" toolbar title label is gone (the window tab
    // already carries the name), (2) the two big inline yellow Warning HelpBoxes
    // below the action buttons are gone (state feedback moves to popups), and
    // (3) the long category-symbols explanatory block is gone (the per-column (i)
    // buttons now carry that help). All three are source-text checks -- the same
    // pattern as POIEditorVisualHierarchyTests -- so a future agent cannot
    // reintroduce the clutter silently.
    public class POIEditorWindowChromeTests
    {
        private static string ReadSource(string assetsRelativePath)
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, assetsRelativePath));
        }

        private static int CountOccurrences(string source, string sub)
        {
            if (string.IsNullOrEmpty(sub)) return 0;
            int count = 0;
            int pos = 0;
            while ((pos = source.IndexOf(sub, pos, StringComparison.Ordinal)) >= 0)
            {
                count++;
                pos += sub.Length;
            }
            return count;
        }

        // The toolbar title label ("POI Editor") and its DrawToolbar method must not
        // exist; the window tab already shows the name.
        [Test]
        public void ToolbarTitle_Removed()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\POIEditorToolWindow.cs");
            Assert.IsFalse(src.Contains("GUILayout.Label(\"POI Editor\""),
                "The redundant 'POI Editor' title label should be removed (the tab already names the window)");
            Assert.IsFalse(src.Contains("private void DrawToolbar()"),
                "The DrawToolbar method should be removed entirely, not left dead");
        }

        // The two big inline yellow Warning boxes below the action buttons must be
        // gone: rig-contains-markers and config-unsaved-changes are now handled via
        // dialog/popup, not permanent inline warnings. The green/red "Rig matches
        // config.json" status label and its DrawSyncAndWarnings method are gone too.
        [Test]
        public void InlineYellowWarnings_Removed()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\POIEditorToolWindow.cs");
            Assert.IsFalse(src.Contains("Rig currently contains generated markers"),
                "The inline rig-marker Warning HelpBox must be removed");
            Assert.IsFalse(src.Contains("Config has unsaved changes"),
                "The inline unsaved-changes Warning HelpBox must be removed");
            Assert.AreEqual(0, CountOccurrences(src, "MessageType.Warning"),
                "No MessageType.Warning HelpBoxes should remain in the window shell file");

            Assert.IsFalse(src.Contains("Rig matches config.json"),
                "The sync-status label must be removed");
            Assert.IsFalse(src.Contains("Rig OUT OF SYNC"),
                "The out-of-sync status label must be removed");
            Assert.IsFalse(src.Contains("DrawSyncAndWarnings"),
                "The now-empty DrawSyncAndWarnings method must be deleted entirely");
        }

        // The long category-symbols explanatory HelpBox and its mini-label in the
        // symbol selector must be gone; column (i) buttons carry the help instead.
        [Test]
        public void CategorySymbolsExplanation_Removed()
        {
            string src = ReadSource(@"Framework\Editor\POIEditor\Shared\POIEditorToolWindow.SymbolTable.cs");
            Assert.IsFalse(src.Contains("Symbols are Sprite assets"),
                "The long category-symbols HelpBox must be removed");
            Assert.IsFalse(src.Contains("The ObjectField shows every Sprite in the project"),
                "The object-picker mini-label must be removed (the (i) button covers it)");
        }

        // The SelectIcon getter must resolve to a real texture (native assign icon
        // when available, otherwise the bundled select.png fallback) and must probe
        // via the SILENT FindTexture -- never IconContent, whose unknown-name lookup
        // logs "Unable to load the icon" on Unity 6 where ProjectAssign is gone.
        [Test]
        public void SelectIcon_ResolvesTextureWithoutLoggingProbe()
        {
            var icon = global::TileStories.Editor.POIEditorToolWindow.SelectIcon;
            Assert.IsNotNull(icon, "SelectIcon must return a GUIContent");
            Assert.IsNotNull(icon.image, "SelectIcon must resolve to a real texture (native or bundled PNG)");

            string src = ReadSource(@"Framework\Editor\POIEditor\POIEditorToolWindow.Constants.cs");
            Assert.IsTrue(src.Contains("EditorGUIUtility.FindTexture(\"ProjectAssign\")"),
                "SelectIcon must probe the native icon silently via FindTexture");
            Assert.IsFalse(src.Contains("IconContent(\"ProjectAssign\")"),
                "SelectIcon must not use IconContent for the native probe -- it logs 'Unable to load the icon' on Unity 6");
        }
    }
}
