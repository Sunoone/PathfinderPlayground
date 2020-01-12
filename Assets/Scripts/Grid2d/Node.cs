using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid2d
{
    public class Node : IHeapItem<Node>
    {
        public int LayerValue { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public int MovementPenalty { get; private set; }

        public int GridX { get; private set; }
        public int GridY { get; private set; }

        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost { get { return GCost + HCost; } }

        public virtual Node Previous { get; set; } // Used to retrace the path.
        public Node BranchNode { get; set; } // Used to provide a branching path from this node.

        public bool CanSimplify { get; protected set; }
        public int HeapIndex { get; set; }

        public Node(int layerValue, Vector3 worldPosition, int gridX, int gridY, int penalty)
        {
            LayerValue = layerValue;
            WorldPosition = worldPosition;
            GridX = gridX;
            GridY = gridY;
            MovementPenalty = penalty;
            CanSimplify = true;
        }

        public int CompareTo(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
                compare = HCost.CompareTo(other.HCost);
            return -compare;
        }
    }
}