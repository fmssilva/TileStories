using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Where a dev-only demo that must be seen through the REAL camera lives (the LOD demo field, the
    // displacement demo, the search demo) and how the camera gets there. In the Editor the demo is built on an empty STAGE far from the scene (nothing of the wall's own scene, such as a scanned room
    // mesh, can cover it) and the camera is moved to the stage's start, then put back when the field is
    // switched off. A device cannot move its AR camera, so a development build uses the pose in front of
    // the camera instead. Either way the pose is chosen once, when the field turns on, and kept across
    // rebuilds: a live edit never moves the field to wherever the developer has walked.
    //
    // Plain C#: WallSession owns one per demo and calls Rebuild on every change; the demo's own spawner
    // decides WHAT exists, this class only WHERE and how the camera gets there.
    public class DemoFieldStage
    {
        // The Editor stages: high above any scene, each demo at its own height (the effects / outline demo
        // grids use 5000 m / 6500 m)
        public static readonly Vector3 EditorStagePosition = new Vector3(0f, 3500f, 0f);
        public static readonly Vector3 DisplacementDemoStagePosition = new Vector3(0f, 4200f, 0f);
        public static readonly Vector3 SearchDemoStagePosition = new Vector3(0f, 5700f, 0f);

        private readonly Vector3 _editorStagePosition;

        private readonly List<MarkerView> _markers = new();
        private Pose? _fieldPose;         // chosen when the field turns on, kept until it turns off
        private Pose? _cameraPoseBefore;  // Editor: where the camera was before it moved to the stage

        // The field's root, or null while the field is off
        public GameObject Root { get; private set; }

        // The field's markers (empty while off)
        public IReadOnlyList<MarkerView> Markers => _markers;

        public bool IsOn => Root != null;

        // The LOD demo field's stage; pass another position for another demo
        public DemoFieldStage() : this(EditorStagePosition) { }

        public DemoFieldStage(Vector3 editorStagePosition)
        {
            _editorStagePosition = editorStagePosition;
        }

        // Pure: where the field is placed. Editor = the empty stage, looking along +z; a device = in
        // front of the camera, turned like the camera around the vertical only.
        public static Pose ChooseFieldPose(bool isEditor, Transform camera) =>
            ChooseFieldPose(isEditor, camera, EditorStagePosition);

        public static Pose ChooseFieldPose(bool isEditor, Transform camera, Vector3 editorStagePosition) =>
            isEditor ? new Pose(editorStagePosition, Quaternion.identity) : DemoFieldSpawner.FieldPose(camera);

        // Pure: only the Editor's mock camera can be moved; a device's AR camera follows the phone
        public static bool MovesCamera(bool isEditor) => isEditor;

        // Build (or rebuild, or remove) the demo: `spawn` builds it at the given pose, adds every marker
        // that should run (LOD, displacement) to the list and returns the demo's root
        public void Rebuild(bool shouldExist, Camera camera, Func<Pose, List<MarkerView>, GameObject> spawn)
        {
            DestroyRoot();
            if (!shouldExist)
            {
                LeaveStage(camera);
                return;
            }

            if (_fieldPose == null) EnterStage(camera);
            Root = spawn(_fieldPose.Value, _markers);
        }

        // Remove the field and give the camera back its pose (the wall scene is left as it was)
        public void Remove(Camera camera)
        {
            DestroyRoot();
            LeaveStage(camera);
        }

        private void EnterStage(Camera camera)
        {
            bool isEditor = Application.isEditor;
            _fieldPose = ChooseFieldPose(isEditor, camera.transform, _editorStagePosition);
            if (!MovesCamera(isEditor)) return;
            _cameraPoseBefore = new Pose(camera.transform.position, camera.transform.rotation);
            camera.transform.SetPositionAndRotation(_fieldPose.Value.position, _fieldPose.Value.rotation);
        }

        private void LeaveStage(Camera camera)
        {
            if (_cameraPoseBefore != null && camera != null)
                camera.transform.SetPositionAndRotation(_cameraPoseBefore.Value.position, _cameraPoseBefore.Value.rotation);
            _cameraPoseBefore = null;
            _fieldPose = null;
        }

        private void DestroyRoot()
        {
            _markers.Clear();
            if (Root == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(Root);
            else UnityEngine.Object.DestroyImmediate(Root);
            Root = null;
        }
    }
}
