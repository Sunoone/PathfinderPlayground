using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Algorithms;

namespace Pathfinding
{
    public class Node : IHeapItem<Node>
    {
        public static Dictionary<int, Node> Nodes = new Dictionary<int, Node>();
        public static Dictionary<int, Node> BorderNodes = new Dictionary<int, Node>();

        // Only noddes with the same enclosure id are connected. Therefore, not path can be found between different EnclosureIds values.
        public int EnclosureId { get; set; }
        public Node[] Connections;
        //@TODO Reaccess is a thing that needs to be solve.
        public int ConnectionIndex { get; private set; }

        public int LayerValue { get; private set; }
        public int Penalty { get; private set; }

        public Vector3 WorldPosition { get; private set; }        

        public int GCost { get; set; } // Traveled cost
        public int HCost { get; set; } // Distance left cost
        public int FCost { get { return GCost + HCost; } }

        public Node Previous { get; set; } // Used to retrace the path.

        public bool CanSimplify { get; protected set; }
        public int HeapIndex { get; set; }

        public Node(int layerValue, Vector3 worldPosition, int penalty)
        {
            LayerValue = layerValue;
            WorldPosition = worldPosition;
            Penalty = penalty;
            CanSimplify = true;
        }
        public void UpdateNodeLayer(int layerValue, int penalty)
        {
            LayerValue = layerValue;
            Penalty = penalty;
        }

        public Node[] GetConnections()
        {
            return null;
        }

        public int CompareTo(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
                compare = HCost.CompareTo(other.HCost);
            return -compare;
        }

        public static int HashFunction(Vector3 worldPosition)
        {
            // Only bordernodes are in the global dictionary. They need know if there are attached to another border node.
            // 


            return 0;
        }

        //@TODO: Test this method.
        public void SortConnectionsWithDirectionBias()
        {
            ConnectionIndex = 0;
            Connections.Quicksort();
        }

        

        
    }
}
