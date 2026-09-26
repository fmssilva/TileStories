// POIEditorToolWindow.DomainTest.cs
//
// Partial: the ONE "Test" sub-foldout every Global Scene domain ends with (_5.1_Editor_Tab.md,
// "Domain Manual Tests"): an optional preview switch (Orientation's Scene-Mode Preview, a Play-Mode
// demo grid), then three collapsed guides -- "How to Scene Test", "How to Playmode Test", "How to
// Device Test". Each domain keeps only its own TestGuideState field, its guide texts and (if any)
// its preview switch; the foldout mechanics live here once.

using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Open/closed state of one domain's Test sub-foldout and its three guides. The Test foldout
        // starts open (its preview switch stays in view), the guides start collapsed.
        private sealed class TestGuideState
        {
            public bool Open = true;
            public bool Scene;
            public bool Playmode;
            public bool Device;
        }

        // One Test sub-foldout: the domain's optional preview switch, then the three guides
        private void DrawDomainTestSubSection(TestGuideState state, string sceneGuide, string playmodeGuide,
            string deviceGuide, Action drawBeforeGuides = null)
        {
            EditorGUILayout.Space(4f);
            state.Open = EditorGUILayout.Foldout(state.Open, "Test", true, EditorStyles.foldoutHeader);
            if (!state.Open) return;

            if (drawBeforeGuides != null)
            {
                drawBeforeGuides();
                EditorGUILayout.Space(4f);
            }

            state.Scene = DrawTestGuideFoldout(state.Scene, "How to Scene Test", sceneGuide);
            state.Playmode = DrawTestGuideFoldout(state.Playmode, "How to Playmode Test", playmodeGuide);
            state.Device = DrawTestGuideFoldout(state.Device, "How to Device Test", deviceGuide);
        }

        // One collapsible guide: a foldout title (a child of "Test", same level as a preview switch)
        // and, when open, the read-only text block one step deeper. An always-visible text block, not
        // a popup: the developer follows the steps while looking at the Scene/Game view, and a popup
        // closes the moment focus moves there. Both rows go through DrawEditorRow, which runs its
        // controls at indentLevel 0 (Foldout and SelectableLabel would otherwise indent twice).
        private bool DrawTestGuideFoldout(bool isOpen, string title, string guideText)
        {
            DrawEditorRow(out _, out _, IndentLevel1);
            isOpen = EditorGUILayout.Foldout(isOpen, title, true, EditorStyles.foldout);
            EditorRowEnd();
            if (!isOpen) return false;

            DrawEditorRow(out float rowWidth, out _, IndentLevel2);
            var guideStyle = EditorStyles.textArea;
            float guideHeight = guideStyle.CalcHeight(new GUIContent(guideText), rowWidth);
            EditorGUILayout.SelectableLabel(guideText, guideStyle,
                GUILayout.Width(rowWidth), GUILayout.Height(guideHeight), GUILayout.ExpandWidth(false));
            EditorRowEnd();
            return true;
        }
    }
}
