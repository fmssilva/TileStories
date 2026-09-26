using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The real wall scene must actually CONTAIN the LOD pipeline and the zoom rig, wired to the
    // wall's WallSession. Every LOD/zoom test builds its own objects in code, so without this
    // contract the app shipped with no LODController at all (LOD, clusters and displacement never
    // ran) and an ARZoomController with no WallSession (zoom silently disabled) while every test
    // stayed green. Reads the SAVED scene file: opened additively when it is not already open.
    public class LivingRoomSceneWiringTests
    {
        private const string ScenePath = "Assets/Apps/LivingRoom/LivingRoomScene.unity";
        private const string ClusterPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";

        private static T Only<T>(Scene scene) where T : Component
        {
            var found = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();
            Assert.AreEqual(1, found.Length, "the wall scene must hold exactly one " + typeof(T).Name);
            return found[0];
        }

        private static Object Ref(Object component, string field) =>
            new SerializedObject(component).FindProperty(field).objectReferenceValue;

        [Test]
        public void LivingRoomScene_HasTheLodPipelineAndZoomRig_WiredToTheWall()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var wall = Only<WallSession>(scene);

                var lod = Only<LODController>(scene);
                Assert.AreSame(wall.gameObject, lod.gameObject, "LODController sits on the WallSession object");
                Assert.AreSame(wall, Ref(lod, "_wallSession"), "LODController reads this wall's markers and settings");
                Assert.AreEqual(AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefabPath), Ref(lod, "_clusterPrefab"),
                    "LODController can build clusters (POI_Cluster prefab assigned)");

                var zoom = Only<ARZoomController>(scene);
                Assert.AreSame(wall, Ref(zoom, "_wallSession"), "ARZoomController reads this wall's Zoom settings");
                var gestures = Only<ARZoomGestureInput>(scene);
                Assert.AreSame(zoom, Ref(gestures, "_zoom"), "pinch / double-tap drive the zoom controller");
                Assert.AreSame(wall, Ref(gestures, "_wallSession"), "gestures read this wall's double-tap settings");

                var buttons = Only<ZoomControlView>(scene);
                Assert.AreSame(zoom, Ref(buttons, "_zoom"), "on-screen buttons drive the zoom controller");
                Assert.IsNotNull(Ref(buttons, "_template"), "on-screen buttons have their layout");
                var document = Ref(buttons, "_document") as UIDocument;
                Assert.IsNotNull(document, "on-screen buttons mount into a UIDocument");
                Assert.IsNotNull(document.panelSettings, "that UIDocument uses the shared PanelSettings");
            }
            finally
            {
                if (openedHere) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
