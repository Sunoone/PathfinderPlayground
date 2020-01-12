using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid3d
{
    // Requires manual values to be synced with created layers.
    public enum NodeType
    {
        Walkable = 0,
        NotWalkable = 8,   
        Climable = 9
    }
    public class Node
    {
        public NodeType NodeType { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public int GridZ { get; private set; }

        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost { get { return GCost + HCost; } }
        public Node Parent { get; set; }

        public Node(NodeType nodeType, Vector3 worldPosition, int gridX, int gridY, int gridZ)
        {
            NodeType = nodeType;
            WorldPosition = worldPosition;
            GridX = gridX;
            GridY = gridY;
            GridZ = gridZ;
        }
    }
}