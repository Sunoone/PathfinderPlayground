using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Algorithms;

namespace Path2d
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

        /// <summary>
        /// Creates a node with maximum G and H costs which can be used to compare.
        /// </summary>
        /// <returns></returns>
        public static Node CreateCompareNode()
        {
            Node node = new Node(0, 0, Vector3Int.zero, Vector3.zero);
            node.UpdateCosts(int.MaxValue / 2, int.MaxValue / 2);
            return node;
        }

        /// <summary>
        /// Modifies the node's pathfinding properties
        /// </summary>
        /// <param name="layerValue">New layer value</param>
        /// <param name="movementPenalty">New movement panelty</param>
        public void Modify(int layerValue, int movementPenalty)
        {
            LayerValue = layerValue;
            MovementPenalty = movementPenalty;
        }

        public override int GetHashCode()
        {
            return CreateHashCode(NetworkPosition);
        }

        /// <summary>
        /// Resets the HCost and GCost of the node to 0.
        /// </summary>
        public void Refresh()
        {
            UpdateCosts(0, 0);
        }
        /// <summary>
        /// Updates the HCost and GCost of the node.
        /// </summary>
        /// <param name="hCost">New HCost</param>
        /// <param name="gCost">New GCost</param>
        public void UpdateCosts(int hCost, int gCost)
        {
            HCost = hCost;
            GCost = gCost;
        }

        /// <summary>
        /// Connects to neighbouring nodes to this node.
        /// </summary>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        /// <param name="depthValue">Manual depth value</param>
        public void FindAndSetConnections(NodeNetwork nodeNetwork)
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
                    Vector3Int networkPosition = new Vector3Int(NetworkPosition.x + x, NetworkPosition.y + y, NetworkPosition.z);
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

        /// <summary>
        /// Adds a connection to the Node. Solves all updates and conditions related to the action.
        /// </summary>
        /// <param name="connection">Node to connect to</param>
        /// <returns></returns>
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

        /// <summary>
        /// // Updates the EnclosureIndex if the new index is higher. Recursive to spread it through the entire NodeNetwork.
        /// </summary>
        /// <param name="enclosureIndex">New enclosure index</param>
        public void UpdateEnclosureIndex(int enclosureIndex)
        {
            if (EnclosureIndex >= enclosureIndex)
                return;

            EnclosureIndex = enclosureIndex;
            foreach (var connection in Connections)
                connection.UpdateEnclosureIndex(enclosureIndex);
        }

        /// <summary>
        /// Creates a hashcode for all the necessary information of a node.
        /// </summary>
        /// <param name="networkPosition">Position from which to simulate the hashcode</param>
        /// <returns></returns>
        public static int CreateHashCode(Vector3Int networkPosition)
        {
            return (networkPosition.x ^ (networkPosition.y) << 12) ^ (networkPosition.z << 24);
        }

        /// <summary>
        /// Compares two nodes their values. 
        /// </summary>
        /// <param name="other">Other node</param>
        /// <returns></returns>
        public int CompareTo(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
                compare = HCost.CompareTo(other.HCost);
            return -compare;
        }
    }
}