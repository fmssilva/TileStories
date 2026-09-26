using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Every Global Scene domain ends in the SAME Test sub-foldout (Shared/POIEditorToolWindow.
    // DomainTest.cs): one TestGuideState per domain -- Test open by default, its three guides
    // collapsed (_5.1_Editor_Tab.md, "Domain Manual Tests"). Walked by reflection so a new domain is
    // checked automatically, and no domain can grow its own private copy of the foldout logic again.
    public class DomainTestFoldoutTests
    {
        private const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Instance;

        [Test]
        public void EveryDomainsTestFoldout_OpensByDefault_WithItsThreeGuidesCollapsed()
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            try
            {
                var stateType = typeof(POIEditorToolWindow).GetNestedType("TestGuideState", BindingFlags.NonPublic);
                Assert.IsNotNull(stateType, "TestGuideState must exist");
                var fields = typeof(POIEditorToolWindow).GetFields(Instance).Where(f => f.FieldType == stateType).ToList();
                CollectionAssert.IsSupersetOf(fields.Select(f => f.Name).ToList(), new[]
                {
                    "_labelsAndFontsTest", "_markerTest", "_orientationTest", "_badgeTest", "_outlineTest", "_effectsTest", "_hierarchyTest",
                    "_lodTest", "_zoomTest", "_displacementTest"
                }, "every visual domain has its Test sub-foldout state");

                foreach (var field in fields)
                {
                    object state = field.GetValue(window);
                    Assert.IsNotNull(state, field.Name + " must be constructed at field-init time");
                    Assert.IsTrue((bool)stateType.GetField("Open").GetValue(state), field.Name + ": Test foldout defaults open");
                    foreach (string guide in new[] { "Scene", "Playmode", "Device" })
                        Assert.IsFalse((bool)stateType.GetField(guide).GetValue(state), field.Name + ": " + guide + " guide defaults collapsed");
                }
            }
            finally { Object.DestroyImmediate(window); }
        }

        // The foldout mechanics exist once: no other file draws "How to ... Test" titles itself
        [Test]
        public void TheGuideFoldouts_AreDrawnOnlyByTheSharedHelper()
        {
            string root = Path.Combine(Application.dataPath, "Framework/Editor/POIEditor");
            var offenders = new List<string>();
            foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (file.EndsWith("POIEditorToolWindow.DomainTest.cs")) continue;
                string src = File.ReadAllText(file);
                if (src.Contains("\"How to Scene Test\"") || src.Contains("DrawTestGuideFoldout("))
                    offenders.Add(Path.GetFileName(file));
            }
            Assert.IsEmpty(offenders, "use DrawDomainTestSubSection instead: " + string.Join(", ", offenders));
        }
    }
}
