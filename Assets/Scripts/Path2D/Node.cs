using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Algorithms;

namespace Path2D
{
    public class Node : IHeapItem<Node>
    {
        private static int _sEnclosureCount = 0;

        public int LayerValue { get; private set; }
        public int EnclosureIndex { get; private set; }
        public Vector3Int NetworkPosition { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public int MovementPenalty { get; private set; }

        public List<Node> Connections { get; private set; }
        public bool LockNeighbours { get; set; }

        public int GCost { get; private set; }
        public int HCost { get; private set; }
        public int FCost { get { return GCost + HCost; } }

        public virtual Node Previous { get; set; } // Used to retrace the path.

        public bool CanSimplify { get; set; }
        public int HeapIndex { get; set; }

        public Node(int layerValue, int movementPenalty, Vector3Int networkPosition, Vector3 worldPosition)
        {
            LayerValue = layerValue;
            MovementPenalty = movementPenalty;
            NetworkPosition = networkPosition;
            WorldPosition = worldPosition;
            EnclosureIndex = 0;
            CanSimplify = true;
            Connections = new List<Node>();
        }

        // Creates a node with maximum G and H costs which can be used to compare.
        public static Node CreateCompareNode()
        {
            Node node = new Node(0, 0, Vector3Int.zero, Vector3.zero);
            node.UpdateCosts(int.MaxValue / 2, int.MaxValue / 2);
            return node;
        }

        public void Modify(int layerValue, int movementPenalty)
        {
            LayerValue = layerValue;
            MovementPenalty = movementPenalty;
        }

        public override int GetHashCode()
        {
            return CreateHashCode(NetworkPosition);
        }
        public void Refresh()
        {
            UpdateCosts(0, 0);
        }
        public void UpdateCosts(int hCost, int gCost)
        {
            HCost = hCost;
            GCost = gCost;
        }

        // Connects to neighbouring nodes to this node.
        public void FindAndSetConnections(NodeNetwork nodeNetwork, int depthValue)
        {
            if (LayerValue == NodeNetwork.UnwalkableLayer)
                return;

            for (int x = -1; x <= 1; x++) // Change to 2 for diamond shaped grid.
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    // It will only check neighbours in 2 dimensions. The third dimension has to be manually connected.
                    Vector3Int networkPosition = new Vector3Int(NetworkPosition.x + x, NetworkPosition.y + y, depthValue);
                    int hash = CreateHashCode(networkPosition);
                    if (nodeNetwork.TryGetNodeFromHash(hash, out Node connection))
                        AddConnection(connection);               
                }
                if (EnclosureIndex == 0)
                {
                    _sEnclosureCount++;
                    EnclosureIndex = _sEnclosureCount;
                }
            }
        }
        // Connects to neighouring nodes to this node, while using the NodeNetwork's default DepthValue.
        public void FindAndSetConnections(NodeNetwork nodeNetwork)
        {
            FindAndSetConnections(nodeNetwork, nodeNetwork.DefaultDepthValue);
        }
        // Adds a connection to the Node. Solves all updates and conditions related to the action.
        public bool AddConnection(Node connection)
        {
            if (Connections.Contains(connection) || connection.LayerValue == NodeNetwork.UnwalkableLayer || LayerValue == NodeNetwork.UnwalkableLayer)
            {
                return false;
            }

            Connections.Add(connection);
            connection.UpdateEnclosureIndex(EnclosureIndex);
            UpdateEnclosureIndex(connection.EnclosureIndex);
            return true;
        }
        // Updates the EnclosureIndex if the new index is higher. Recursive to spread it through the entire NodeNetwork.
        public void UpdateEnclosureIndex(int index)
        {
            if (EnclosureIndex >= index)
                return;

            EnclosureIndex = index;
            foreach (var connection in Connections)
                connection.UpdateEnclosureIndex(index);
        }

        public static int CreateHashCode(Vector3Int networkPosition)
        {
            return (networkPosition.x ^ (networkPosition.y) << 12) ^ (networkPosition.z << 24);
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