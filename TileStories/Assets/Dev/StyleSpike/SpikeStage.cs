using System;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TileStories.Dev.StyleSpike
{
    // DEV-ONLY (_3.3 P0 capability spike; this assembly compiles only in the Editor and development builds, nothing in Framework
    // references it). One place every spike rig stands on: a camera and a UIDocument on the card's own PanelSettings, so what the
    // spike measures is what the card would get.
    public sealed class SpikeStage : IDisposable
    {
        public Camera Camera { get; private set; }
        public UIDocument Document { get; private set; }
        public VisualElement Root => Document.rootVisualElement;

        private readonly GameObject _cameraObject;
        private readonly GameObject _documentObject;

        // Build a clear-colour camera and a full-screen panel (the panel is built only once the document is enabled)
        public SpikeStage(PanelSettings panelSettings, Color clearColour, string name = "StyleSpike")
        {
            _cameraObject = new GameObject(name + " Camera");
            Camera = _cameraObject.AddComponent<Camera>();
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = clearColour;
            Camera.tag = "MainCamera";

            _documentObject = new GameObject(name + " UI");
            _documentObject.SetActive(false);           // set the panel before OnEnable builds it
            Document = _documentObject.AddComponent<UIDocument>();
            Document.panelSettings = panelSettings;
            _documentObject.SetActive(true);

            Root.style.flexGrow = 1;
        }

        // Add a style sheet to the panel root (the spike's own USS, or a shipped one read as-is)
        public void AddSheet(StyleSheet sheet)
        {
            Debug.Assert(sheet != null, "[StyleSpike] a null style sheet was passed to the stage");
            Root.styleSheets.Add(sheet);
        }

        // Tear the rig down (both objects)
        public void Dispose()
        {
            if (_documentObject != null) Object.Destroy(_documentObject);
            if (_cameraObject != null) Object.Destroy(_cameraObject);
        }
    }
}
