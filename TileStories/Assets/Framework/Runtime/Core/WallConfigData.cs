using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    [Serializable]
    public class WallConfigData
    {
        public string wall_id;
        public string wall_name;
        public int immersal_map_id;
        public List<POIData> pois = new();
        public List<CalibrationAnchor> calibration_anchors = new();
    }

[Serializable]
    public class POIData : ISerializationCallbackReceiver
    {
        public string id;
        public string name;
        public string category;
        public float x_norm;
        public float y_norm;

        // Nullable: null means "not yet captured". Presence check, not magnitude -
        // a POI at (0,0,0) is valid and must not be mistaken for unset
        public CapturedPosition captured_position;

        // Explicit presence flag - Unity's JsonUtility may not correctly preserve
        // null for nested [Serializable] classes. This is the authoritative signal.
        public bool has_captured_position;

        // "workflow_a_editor" / "workflow_b_device" / "manual"
        public string captured_position_source;

        // Unix timestamp of when the position was captured
        public long captured_position_timestamp;

        public string summary;

        // Normalize the serialized shape so JsonUtility round-trips preserve
        // the semantic meaning of "uncaptured" via has_captured_position.
        public void OnBeforeSerialize()
        {
            if (!has_captured_position)
            {
                captured_position = null;
            }
        }

        // Restore null for uncaptured POIs even if JsonUtility materializes an
        // empty nested object during deserialization.
        public void OnAfterDeserialize()
        {
            if (!has_captured_position)
            {
                captured_position = null;
                if (string.IsNullOrEmpty(captured_position_source))
                {
                    captured_position_source = null;
                }

                if (captured_position_timestamp < 0)
                {
                    captured_position_timestamp = 0;
                }
            }
        }
    }

    [Serializable]
    public class CapturedPosition
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new(x, y, z);
    }

    [Serializable]
    public class CalibrationAnchor
    {
        public string id;
        public float x_norm;
        public float y_norm;
        public CapturedPosition captured_position;
    }
}
