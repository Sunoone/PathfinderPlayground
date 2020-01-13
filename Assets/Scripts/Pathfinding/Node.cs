using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Algorithms;

namespace Pathfinding
{
    public class Node : IHeapItem<Node>
    {
#if UNITY_EDITOR
        public Color NodeGizmoColor = Color.black;
#endif

        // Only noddes with the same enclosure id are connected. Therefore, not path can be found between different EnclosureIds values.
        public int EnclosureId { get; set; }
        public List<Node> Connections { get; private set; }
        //@TODO Reaccess is a thing that needs to be solve.
        public int ConnectionIndex { get; private set; }

        public int LayerValue { get; private set; }
        public int Penalty { get; private set; }

        public Vector3Int NetworkPosition { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public int GCost { get; set; } // Traveled cost
        public int HCost { get; set; } // Distance left cost
        public int FCost { get { return GCost + HCost; } }

        public Node Previous { get; set; } // Used to retrace the path.

        public bool CanSimplify { get; protected set; }
        public int HeapIndex { get; set; }

        // Please place the PathingNetwork origins on whole numbers.
        public Node(int layerValue, Vector3Int networkPosition, Vector3 worldPosition, int penalty)
        {
            LayerValue = layerValue;       
            NetworkPosition = networkPosition;
            WorldPosition = worldPosition;
            Penalty = penalty;
            CanSimplify = true;
        }
        ~Node()
        {
            //@TOD: Removing itself from the lists/dictionaries
        }
    
        public void UpdateNodeLayer(int layerValue, int penalty)
        {
            LayerValue = layerValue;
            Penalty = penalty;
        }

        //@TODO: Test this method.
        public void SortConnectionsWithDirectionBias()
        {
            ConnectionIndex = 0;
            Connections.Quicksort();
        }

        public int CompareTo(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
                compare = HCost.CompareTo(other.HCost);
            return -compare;
        }
        public int CompareLayer(Node other)
        {
            return LayerValue.CompareTo(other.LayerValue);
        }

        public void SetAvailableNeighbours(PathNetwork pathingNetwork)
        {
            int count = 0;
            Connections = new List<Node>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    Vector3Int gridPosition = NetworkPosition;
                    gridPosition.x += x;
                    gridPosition.y += y;
                    if (pathingNetwork.TryGetNodeForNodePosition(gridPosition, out Node neighbour))
                    {
                        Connections.Add(neighbour);
                        count++;
                    }
                }
            }
            Debug.Log("Neighbour count: " + count);
        }



        public override int GetHashCode()
        {
            return GetHashForNetworkPosition(NetworkPosition);
        }

        public static int GetHashForNetworkPosition(Vector3Int networkPosition)
        {
            // Change the amount of shifting depending on the relation between the gridSpacing and regionSize.
            // In the editor, also allow the regions to be a max of x times bigger than the gridSpacing to ensure there is enough room for the hash.
            return (networkPosition.x ^ (networkPosition.y) << 8) ^ (networkPosition.z << 16);
        }       
    }
}
