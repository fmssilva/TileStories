using System;
using System.Collections.Generic;

namespace TileStories
{
    [Serializable]
    public class WallConfigData
    {
        public string wall_id;
        public string wall_name;
        public int immersal_map_id;
        public List<POIData> pois = new();
    }

    [Serializable]
    public class POIData
    {
        public string id;
        public string name;
        public string category;
        public float x_norm;
        public float y_norm;
        public CapturedPosition captured_position;
        public string summary;
    }

    [Serializable]
    public class CapturedPosition
    {
        public float x;
        public float y;
        public float z;
    }
}
