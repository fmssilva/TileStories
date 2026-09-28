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

        // The POI Detail Card (_3.1): its own PoiCard object with its own UIDocument drawn above the search UI,
        // bound to the wall and to both card stylesheets (tokens + layout)
        [Test]
        public void LivingRoomScene_HasThePoiCard_AboveTheSearchUi_WiredToTheWall()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var wall = Only<WallSession>(scene);
                var card = Only<PoiCardHost>(scene);
                Assert.AreEqual("PoiCard", card.gameObject.name);
                Assert.AreSame(wall, Ref(card, "wallSession"), "the card shows this wall's POIs");
                Assert.AreEqual("Assets/Framework/Runtime/UI/Cards/CardTokens.uss", AssetDatabase.GetAssetPath(Ref(card, "tokens")));
                Assert.AreEqual("Assets/Framework/Runtime/UI/Cards/PoiCard.uss", AssetDatabase.GetAssetPath(Ref(card, "cardStyle")));
                Assert.AreEqual("Assets/Framework/Runtime/UI/Cards/CardStrings.asset", AssetDatabase.GetAssetPath(Ref(card, "strings")),
                    "the card's UI texts come from the framework string table (+ the wall's Card Texts)");

                var cardDocument = card.GetComponent<UIDocument>();
                var searchDocument = Only<SearchUIHost>(scene).GetComponent<UIDocument>();
                Assert.AreSame(Only<SearchUIHost>(scene), Ref(card, "searchUI"), "the card tells this scene's search UI when it covers the top (6C)");
                Assert.AreSame(searchDocument.panelSettings, cardDocument.panelSettings, "one shared runtime panel");
                Assert.AreEqual(2, cardDocument.sortingOrder, "sort order 2");
                Assert.Greater(cardDocument.sortingOrder, searchDocument.sortingOrder, "the card draws above the search UI");
            }
            finally
            {
                if (openedHere) EditorSceneManager.CloseScene(scene, true);
            }
        }

        // Every block stylesheet on disk (CardParts.uss + one per family, _3.1 step 6C) is added by BOTH card hosts -- the
        // wall's PoiCardHost and the Phase A CardGalleryHarness -- in the same order, CardParts first. A family .uss added
        // without wiring would leave its blocks unstyled in one scene while the other looked right.
        [Test]
        public void BothCardHosts_AddEveryBlockStylesheet_InTheSameOrder()
        {
            const string cards = "Assets/Framework/Runtime/UI/Cards";
            var onDisk = System.IO.Directory.GetFiles(cards, "*.uss", System.IO.SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .Where(p => p != cards + "/CardTokens.uss" && p != cards + "/PoiCard.uss")
                .OrderBy(p => p).ToList();
            Assert.GreaterOrEqual(onDisk.Count, 5, "not vacuous: CardParts + the About, Stories, Visit and Meta families");

            foreach (var (path, type) in new[] { (ScenePath, typeof(PoiCardHost)), ("Assets/Dev/CardGallery/CardGalleryScene.unity", typeof(CardGalleryHarness)) })
            {
                var scene = SceneManager.GetSceneByPath(path);
                bool openedHere = !scene.isLoaded;
                if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    var host = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren(type, true)).Single();
                    var list = new SerializedObject(host).FindProperty("blockStyles");
                    var wired = Enumerable.Range(0, list.arraySize).Select(i => AssetDatabase.GetAssetPath(list.GetArrayElementAtIndex(i).objectReferenceValue)).ToList();
                    // - a wall's own kinds bring their own stylesheets (_3.1 step 11): the wall scene may add sheets from Assets/Apps, and only there,
                    //   after every framework one; the Phase A gallery is the framework's alone and names no app
                    var appSheets = wired.Where(w => w.StartsWith("Assets/Apps/")).ToList();
                    var frameworkSheets = wired.Where(w => !w.StartsWith("Assets/Apps/")).ToList();
                    CollectionAssert.AreEquivalent(onDisk, frameworkSheets, path + ": every block stylesheet, nothing missing or extra");
                    Assert.AreEqual(cards + "/CardParts.uss", wired[0], path + ": the shared parts first, the families after");
                    CollectionAssert.AreEqual(frameworkSheets.Concat(appSheets).ToList(), wired, path + ": the app's own sheets come after the framework's");
                    if (type == typeof(CardGalleryHarness)) CollectionAssert.IsEmpty(appSheets, "the framework's gallery names no app");
                }
                finally
                {
                    if (openedHere) EditorSceneManager.CloseScene(scene, true);
                }
            }
        }
    }
}
