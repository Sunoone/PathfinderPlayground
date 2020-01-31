using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Persistance;

namespace Pathfinding
{
    // Cannot be created manually. Needs to be generated from a PathNetwork. Data files are used directly and cannot be modified.
    public class PathNetworkData : ScriptableObject
    {
        public SerializableDictionary<int, Node> NetworkNodes; // Lets make it so edge nodes share a position.
        public SerializableDictionary<int, Region> RegionConnections; // Hash uses 2 coordinates.
        // It doesn't matter whether the connection is above, below or same level. It can only connect to 1 edge anyway.
        // It only matters for establishing the connection themselves, as values can pass thresholds.



    }
}
