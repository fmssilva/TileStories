using System;
using UnityEngine;

namespace TileStories
{
    // The card's AR placement owner (_3.1 step 10B.1): ONE model in the world at a time, a child of the wall's frame (so it moves with the
    // wall when the localisation corrects it), placed where ArPlacementRule says. The card host builds it once over the wall (WallArSurface)
    // and the card's own media; the gallery builds it over a ManualArWall. Placing another model replaces the first; Remove destroys the
    // instance and releases the model it loaded, so nothing is left behind.
    public sealed class ArPlacementService : ICardArPlacement, IDisposable
    {
        private readonly IArWall _wall;
        private readonly Func<IMediaSource> _media;
        private IMediaSource _loadedFrom;

        // The model standing now (null when none), what placed it and where the rule put it
        public GameObject Placed { get; private set; }
        public ArPlacementRequest Current { get; private set; }
        public ArPlacementRule.Placement Pose { get; private set; }

        public event Action Changed;

        public ArPlacementService(IArWall wall, Func<IMediaSource> media)
        {
            _wall = wall;
            _media = media;
            if (_wall != null) _wall.LocalisationChanged += OnLocalisationChanged;
        }

        public bool CanPlace => _wall != null && _wall.IsLocalised && _wall.Root != null;

        public bool IsPlaced(string poiId, string blockKey) =>
            Placed != null && Current != null && Current.Poi?.id == poiId && Current.BlockKey == blockKey;

        public bool Place(ArPlacementRequest request)
        {
            if (!CanPlace || request?.Poi == null || string.IsNullOrEmpty(request.ModelPath)) return false;
            if (!POIPositionResolver.TryResolvePosition(request.Poi, out var poiPosition, logErrors: false)) return false;
            var media = _media?.Invoke();
            var prefab = media?.Load<GameObject>(request.ModelPath);
            if (prefab == null)
            {
                Debug.LogWarning("[Card] place_in_ar: the model '" + request.ModelPath + "' could not be loaded");
                return false;
            }

            RemoveWithoutNotice();
            var model = UnityEngine.Object.Instantiate(prefab, _wall.Root);
            model.name = "ArPlaced_" + request.Poi.id + "_" + request.BlockKey;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            var input = new ArPlacementRule.Input
            {
                PoiPosition = poiPosition,
                PoiRotation = WallSession.AuthoredRotationOf(request.Poi),
                Viewer = _wall.TryGetViewer(out var viewer) ? viewer : (Vector3?)null,
                OffsetCm = request.OffsetCm,
                ScaleMode = request.ScaleMode,
                HeightCm = request.HeightCm,
                MarkerMultiple = request.MarkerMultiple,
                MarkerDiameter = _wall.MarkerDiameterOf(request.Poi),
                WallScale = _wall.Root.lossyScale.x,
                ModelBounds = LocalBoundsOf(model),
            };
            var pose = ArPlacementRule.Place(input);
            model.transform.localPosition = pose.LocalPosition;
            model.transform.localRotation = pose.LocalRotation;
            model.transform.localScale = Vector3.one * pose.LocalScale;

            Placed = model;
            Current = request;
            Pose = pose;
            _loadedFrom = media;
            DevLog.Detail(LogDomain.Card, "[Card] place_in_ar placed '" + request.ModelPath + "' for " + request.Poi.id + "/" + request.BlockKey +
                " at " + pose.Target + " normal " + pose.Normal + " scale " + pose.LocalScale.ToString("F3"));
            Changed?.Invoke();
            return true;
        }

        public void Remove()
        {
            if (Placed == null && Current == null) return;
            RemoveWithoutNotice();
            Changed?.Invoke();
        }

        // The card now shows `poiId` (null: it closed): the placed model goes when ArPlacementRule says so, through the same release path as Remove
        public void SelectionChanged(string poiId)
        {
            if (Current == null || !ArPlacementRule.RemovesOnSelection(Current.Poi?.id, poiId, Current.KeepOnSwitch)) return;
            DevLog.Detail(LogDomain.Card, "[Card] place_in_ar: " + Current.Poi?.id + " -> " + (poiId ?? "(closed)") + ", the model goes");
            Remove();
        }

        // Remove and stop listening to the wall (the host going away)
        public void Dispose()
        {
            RemoveWithoutNotice();
            if (_wall != null) _wall.LocalisationChanged -= OnLocalisationChanged;
        }

        private void RemoveWithoutNotice()
        {
            if (Placed != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(Placed);
                else UnityEngine.Object.DestroyImmediate(Placed);
                DevLog.Detail(LogDomain.Card, "[Card] place_in_ar removed '" + Current?.ModelPath + "'");
            }
            if (_loadedFrom != null && Current != null) _loadedFrom.Release(Current.ModelPath);
            Placed = null;
            Current = null;
            _loadedFrom = null;
        }

        private void OnLocalisationChanged() => Changed?.Invoke();

        // The model's bounds in its OWN frame (every renderer's local bounds carried into the model root's space), at the scale it was
        // instantiated with; a model with no renderer is a point at its origin
        public static Bounds LocalBoundsOf(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            var root = model.transform;
            bool any = false;
            var bounds = new Bounds();
            foreach (var renderer in renderers)
            {
                var local = renderer.localBounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    var point = local.center + Vector3.Scale(local.extents, new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                    var inModel = root.InverseTransformPoint(renderer.transform.TransformPoint(point));
                    if (!any) { bounds = new Bounds(inModel, Vector3.zero); any = true; }
                    else bounds.Encapsulate(inModel);
                }
            }
            return bounds;
        }
    }
}
