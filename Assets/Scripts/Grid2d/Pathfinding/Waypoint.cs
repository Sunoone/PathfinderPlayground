using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid2d.Pathfinding
{
    public struct Waypoint
    {
        public Vector3 Position { get; private set; }
        public int LayerValue { get; private set; }

        public Waypoint(Vector3 position, int layerValue)
        {
            Position = position;
            LayerValue = layerValue;
        }
    }
}