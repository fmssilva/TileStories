using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B (_2.6-i): the coordinator drives REAL spawned markers' CanvasGroup alpha
    // through the LOD-coexistent SetVisible(alpha, fade) seam. EditMode can't tick the
    // fade coroutines, so the hide/dim behaviour is asserted here in PlayMode.
    public class FilterCompositionPlayModeTests
    {
        private const string PanelAssetPath =
            "Assets/Framework/Runtime/UI/Shared/PanelSettings.asset";

        private GameObject _docGO;
        private readonly List<GameObject> _spawned = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
                if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
            if (_docGO != null) Object.DestroyImmediate(_docGO);
        }

        private static MarkerView MakeBareMarker(string poiId, Vector3 position)
        {
            var go = new GameObject("marker_" + poiId, typeof(MarkerView), typeof(CanvasGroup));
            var view = go.GetComponent<MarkerView>();
            typeof(MarkerView).GetField("<PoiId>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(view, poiId);
            go.transform.position = position;
            return view;
        }

        private (ResultSetCoordinator coordinator, FilterTrayView tray, WallConfigData config) BuildHarness(string mismatchBehaviour)
        {
            var config = new WallConfigData
            {
                wall_id = "comp_pm_wall",
                filter_mismatch_behaviour = mismatchBehaviour,
                category_styles = new List<CategoryStyleEntry>
                {
                    new CategoryStyleEntry { category = "religious", color_hex = "#FF0000" },
                    new CategoryStyleEntry { category = "civic", color_hex = "#00FF00" },
                },
                pois = new List<POIData>
                {
                    new POIData { id = "pm_relig", name = "Religious One", category = "religious", has_captured_position = true, captured_position = new CapturedPosition { x = 0.2f, y = 0f, z = 0.8f } },
                    new POIData { id = "pm_civic", name = "Civic Hall", category = "civic", has_captured_position = true, captured_position = new CapturedPosition { x = 0.7f, y = 0f, z = 0.3f } },
                },
            };

            var index = new POISearchIndex();
            index.Build(config);

            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelAssetPath);
            Assert.IsNotNull(panel);

            _docGO = new GameObject("comp-pm-doc");
            var doc = _docGO.AddComponent<UIDocument>();
            doc.panelSettings = panel;

            var tray = _docGO.AddComponent<FilterTrayView>();
            tray.Initialize(config);

            var relig = MakeBareMarker("pm_relig", Vector3.zero);
            var civic = MakeBareMarker("pm_civic", Vector3.right);
            _spawned.Add(relig.gameObject);
            _spawned.Add(civic.gameObject);

            var markers = new List<MarkerView> { relig, civic };
            var coordinator = new ResultSetCoordinator(index, config, tray,
                null, null, () => markers);
            return (coordinator, tray, config);
        }

        [UnityTest]
        public IEnumerator HideBehaviour_FullyFadesNonMatchingMarkers()
        {
            var (coordinator, tray, _) = BuildHarness("hide");

            // Activate the "religious" facet through the real toggle callback path.
            var toggle = _docGO.GetComponent<UIDocument>().rootVisualElement.Q<Toggle>("facet-toggle-religious");
            Assert.IsNotNull(toggle, "religious facet toggle missing");
            toggle.value = true;

            yield return new WaitForSeconds(0.4f); // let the 0.15s crossfade finish

            float religAlpha = _spawned[0].GetComponent<CanvasGroup>().alpha;
            float civicAlpha = _spawned[1].GetComponent<CanvasGroup>().alpha;
            Assert.AreEqual(1f, religAlpha, 0.05f, "matching marker must stay fully visible");
            Assert.AreEqual(0f, civicAlpha, 0.05f, "non-matching marker must be fully hidden under 'hide'");

            coordinator.Dispose();
        }
    }
}