using NUnit.Framework;
using UnityEngine;
using DemoView = TileStories.Editor.POIEditorToolWindow.DemoView;

namespace TileStories.Editor.Tests
{
    // Only one Play Mode demo view can be seen at a time: the effects and outline grids each draw a
    // full-screen camera over the main one (two grids tie; any grid hides the LOD demo field, which the
    // main camera sees). MakeThisTheOnlyActiveDemoView keeps exactly the one just turned on. Every
    // (view turned on) x (other views already on) combination is checked on the real window method.
    public class DemoGridExclusivityTests
    {
        private static (POIEditorToolWindow window, WallConfigData config) NewWindow(bool effects, bool outline, bool field, bool displacement, bool search)
        {
            var window = ScriptableObject.CreateInstance<POIEditorToolWindow>();
            var config = new WallConfigData
            {
                effect_defaults = new EffectDefaults { preview = new EffectDefaults.EffectPreviewSettings { enabled = effects } },
                outline_preview = new OutlinePreviewSettings { enabled = outline },
                demo_field = new DemoFieldSettings { enabled = field },
                displacement_demo = new DisplacementDemoSettings { enabled = displacement },
                search_demo = new SearchDemoSettings { enabled = search },
            };
            typeof(POIEditorToolWindow).GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(window, config);
            return (window, config);
        }

        private static bool IsOn(WallConfigData c, DemoView v) => v switch
        {
            DemoView.EffectsGrid => c.effect_defaults.preview.enabled,
            DemoView.OutlineGrid => c.outline_preview.enabled,
            DemoView.DisplacementDemo => c.displacement_demo.enabled,
            DemoView.SearchDemo => c.search_demo.enabled,
            _ => c.demo_field.enabled,
        };

        [Test]
        public void TurningOneViewOn_TurnsEveryOtherViewOff(
            [Values("EffectsGrid", "OutlineGrid", "LodDemoField", "DisplacementDemo", "SearchDemo")] string viewName)
        {
            var view = (DemoView)System.Enum.Parse(typeof(DemoView), viewName);
            // everything on: the view the developer just ticked must be the only one left
            var (window, config) = NewWindow(true, true, true, true, true);
            try
            {
                window.MakeThisTheOnlyActiveDemoView(view, justTurnedOn: true);
                foreach (DemoView other in System.Enum.GetValues(typeof(DemoView)))
                    Assert.AreEqual(other == view, IsOn(config, other), view + " turned on -> " + other);
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test]
        public void NotJustTurnedOn_NeverTouchesTheOtherSwitches(
            [Values("EffectsGrid", "OutlineGrid", "LodDemoField", "DisplacementDemo", "SearchDemo")] string viewName)
        {
            var view = (DemoView)System.Enum.Parse(typeof(DemoView), viewName);
            // re-ticking an already-on view, or ticking one off, must never touch the others
            var (window, config) = NewWindow(true, true, true, true, true);
            try
            {
                window.MakeThisTheOnlyActiveDemoView(view, justTurnedOn: false);
                foreach (DemoView other in System.Enum.GetValues(typeof(DemoView)))
                    Assert.IsTrue(IsOn(config, other), other + " untouched by an unrelated edit");
            }
            finally { Object.DestroyImmediate(window); }
        }
    }
}
