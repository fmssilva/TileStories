using System;
using UnityEngine;

namespace TileStories
{
    // What a place_in_ar block asks to place (_3.1 step 10B.1): which block of which POI, the model and the block's choices
    public sealed class ArPlacementRequest
    {
        public POIData Poi;
        public string BlockKey;
        public string ModelPath;
        public string Anchor = ArPlacementRule.AnchorPoiOnWall;
        public float OffsetCm;
        public string ScaleMode = ArPlacementRule.ScaleRealSize;
        public float HeightCm;
        public float MarkerMultiple;
    }

    // The card's ONE AR placement owner (_3.1 step 10B.1), beside Audio / Video / Preview and reached the same way (BlockBindContext.ArPlacement):
    // ONE model stands in the world at a time; placing another replaces it; Remove (the card's chip, the card closing, the host going away)
    // takes it away and leaves nothing behind.
    public interface ICardArPlacement
    {
        // Whether a model can be placed now: the wall is localised (IWallTracker)
        bool CanPlace { get; }

        // Whether this block of this POI is the one placed now
        bool IsPlaced(string poiId, string blockKey);

        // Something changed: a model placed or removed, the wall localised or lost
        event Action Changed;

        // Place this block's model (replacing whatever stands): false when it cannot (not localised, no such model, no position)
        bool Place(ArPlacementRequest request);

        // Take the placed model away (nothing placed: nothing happens)
        void Remove();
    }

    // The wall the placement stands on (_3.1 step 10B.1): whether it is localised, its frame (the POI positions' frame), where the viewer
    // stands in it and a POI marker's size. The wall scene's is WallArSurface (WallSession + IWallTracker); the gallery and tests hand a
    // ManualArWall.
    public interface IArWall
    {
        bool IsLocalised { get; }
        event Action LocalisationChanged;
        Transform Root { get; }
        bool TryGetViewer(out Vector3 wallPosition);
        // The diameter of this POI's marker in the wall frame (its hierarchy level's size)
        float MarkerDiameterOf(POIData poi);
    }
}
