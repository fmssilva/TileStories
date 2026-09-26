using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Shared set-up for the Select, Filter & Search PlayMode suites: the REAL wall scene (LivingRoomScene --
    // its WallSession, mock camera, EventSystem, zoom rig and SearchUI object with SearchUIHost + SearchUI.uss,
    // the shipped config) is loaded, the tests act the way a visitor or the live Editor push does (real
    // raycast taps, real panel events, WallSession.ApplySearchSettings / ApplySearchDemo), and it is unloaded
    // after. Renders of the real Game view (UI included) are saved as Assets/Screenshots/Search_*.png.
    public abstract class SearchSceneFixture
    {
        private const string ScenePath = "Assets/Apps/LivingRoom/LivingRoomScene.unity";

        protected WallSession Session;
        protected SearchUIHost Host;
        protected Camera Cam;
        private readonly List<string> _unexpectedErrors = new();
        private string _savedRecent;

        private void CollectUnexpectedErrors(string message, string stack, LogType type)
        {
            if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                && !message.StartsWith("[ARFoundationSupport]"))
                _unexpectedErrors.Add(type + ": " + message);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            _unexpectedErrors.Clear();
            Application.logMessageReceived += CollectUnexpectedErrors;
            _savedRecent = PlayerPrefs.GetString(RecentSearchesManager.PREFS_KEY, null);
            PlayerPrefs.DeleteKey(RecentSearchesManager.PREFS_KEY);
            PlayerPrefs.DeleteKey(ViewModeControl.LastViewPrefsKey);
            SelectionEventBus.ResetState();
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the wall scene by path.");
#endif
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (mb != null && mb.GetType().FullName == "Immersal.XR.ImmersalSession") mb.enabled = false;
            ARZoomState.SetZoom(1f, 1f, 4f);
            for (int frame = 0; frame < 300; frame++)
            {
                Session = Object.FindFirstObjectByType<WallSession>();
                Host = Object.FindFirstObjectByType<SearchUIHost>();
                if (Session != null && Session.SpawnedMarkers.Count > 0 && Host != null && Host.Root != null) break;
                yield return null;
            }
            Assert.IsNotNull(Session, "the wall scene has a WallSession");
            Assert.IsNotNull(Host, "the wall scene has the search UI (SearchUI object with a SearchUIHost)");
            Assert.IsNotNull(Host.Root, "precondition: the search UI was built when the wall spawned");
            Cam = Camera.main;
            // - LOD off: every POI stays visible, so what a test sees is the search's doing alone
            Session.ApplyLodSettings(new LodSettings { enabled = false });
            yield return Wait(0.8f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            LogAssert.ignoreFailingMessages = true;
            Application.logMessageReceived -= CollectUnexpectedErrors;
            SelectionEventBus.ResetState();
            ARZoomState.SetZoom(1f, 1f, 4f);
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();
            if (_savedRecent == null) PlayerPrefs.DeleteKey(RecentSearchesManager.PREFS_KEY);
            else PlayerPrefs.SetString(RecentSearchesManager.PREFS_KEY, _savedRecent);
            PlayerPrefs.DeleteKey(ViewModeControl.LastViewPrefsKey);

            var wallScene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterSearchSceneTest"));
            if (wallScene.IsValid() && wallScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(wallScene);
            yield return null;
            CollectionAssert.IsEmpty(_unexpectedErrors, "the wall scene logged errors");
        }

        // The running wall's config (a copy to edit and push, like the Editor's live push does)
        protected WallConfigData ConfigCopy()
        {
            var config = (WallConfigData)typeof(WallSession)
                .GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(Session);
            return JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(config));
        }

        // Edit the Select, Filter & Search settings and push them live (WallSession.ApplySearchSettings)
        protected IEnumerator ApplySearch(System.Action<SelectFilterSearchSettings> edit)
        {
            var copy = ConfigCopy();
            edit(copy.select_filter_search);
            Session.ApplySearchSettings(copy);
            yield return null;
        }

        protected static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }

        protected MarkerView Marker(string id) => Session.SpawnedMarkers.First(m => m != null && m.PoiId == id);

        // The ids whose markers are fully shown / at a given selection alpha
        protected HashSet<string> MarkersAt(float alpha) =>
            new(Session.SpawnedMarkers.Where(m => m != null && Mathf.Abs(m.SelectionAlpha - alpha) < 1e-3f).Select(m => m.PoiId));

        protected HashSet<string> ListIds() => new(Host.List.Rows.Select(r => r.PoiId));

        protected HashSet<string> DotIds() => new(Host.Minimap.Positions.Keys.Where(Host.Minimap.IsDotShown));

        // Press a UI Toolkit button / toggle through the live panel (the submit path a click also ends in)
        protected static void Press(VisualElement element)
        {
            Assert.IsNotNull(element, "the element to press exists");
            Assert.IsNotNull(element.panel, element.name + " is on a live panel");
            using var e = NavigationSubmitEvent.GetPooled();
            e.target = element;
            element.SendEvent(e);
        }

        // A real tap where a finger would: everything under the screen point, topmost first, gets the click
        protected static GameObject TapScreen(Vector2 screenPoint)
        {
            var eventSystem = EventSystem.current;
            Assert.IsNotNull(eventSystem, "the wall scene has an EventSystem (uGUI taps need one)");
            var data = new PointerEventData(eventSystem) { position = screenPoint, button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            eventSystem.RaycastAll(data, hits);
            if (hits.Count == 0) return null;
            data.pointerCurrentRaycast = hits[0];
            data.pointerPress = hits[0].gameObject;
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, data, ExecuteEvents.pointerClickHandler);
            return hits[0].gameObject;
        }

        // The screen point of a marker's symbol, or null when it is off screen or behind the camera
        protected Vector2? ScreenPointOf(MarkerView marker)
        {
            Vector3 sp = Cam.WorldToScreenPoint(marker.transform.position);
            if (sp.z <= 0f || sp.x < 0f || sp.y < 0f || sp.x > Screen.width || sp.y > Screen.height) return null;
            return new Vector2(sp.x, sp.y);
        }

        // Save what the Game view shows (3D and UI) under Assets/Screenshots
        protected static IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                string dir = Path.Combine(Application.dataPath, "Screenshots");
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "Search_" + name + ".png"), tex.EncodeToPNG());
            }
            finally { Object.Destroy(tex); }
        }
    }
}
