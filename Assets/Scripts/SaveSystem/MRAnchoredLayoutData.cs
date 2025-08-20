using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class MRAnchoredLayoutData
    {
        public List<AnchoredFurnitureData> furniture = new();
        public List<AnchoredSurfaceData> surfaces = new();
    }

    [Serializable]
    public class AnchoredFurnitureData
    {
        public string modelId;
        public string spatialAnchorId; // UUID of the OVRSpatialAnchor
        public Quaternion relativeRotation;
        public Vector3 scale;
    }

    [Serializable]
    public class AnchoredSurfaceData
    {
        public string surfaceType;
        public Color color;
    }
}