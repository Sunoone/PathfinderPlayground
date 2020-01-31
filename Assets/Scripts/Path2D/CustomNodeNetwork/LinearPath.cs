using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Path2d.CustomNodeNetwork
{
    public class LinearPath : CustomNodeNetworkComponent
    {
        private enum ConnectionType
        {
            Full, 
            Single,
            Manual,
        }

#pragma warning disable 649
        [Tooltip("Creates nodes at regular intervals, making the path function like a regular path.")]
        private bool _subDivide = false; // Not implemented yet. Therefore restricting editor access.
        [SerializeField]
        private ConnectionType _connectionType = ConnectionType.Single;
        [SerializeField]
        private Transform[] _pathTransforms;
#pragma warning restore 649

        public override List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork)
        {
            // Create NodeInfo array for all the new nodes that need to be created.
            NodeInfo[] nodeInfoArray = CreateDefaultNodeInfoArray();

            // Create the first node of the linearPathNetwork.
            NodeInfo nodeInfo = nodeInfoArray[0];
            Node currentNode = nodeNetwork.CreateCustomNode(nodeInfo.Layer, nodeInfo.WorldPosition);

            List<Node> linearPathNetwork = new List<Node>();
            linearPathNetwork.Add(currentNode);

            ConnectStartToNetwork(currentNode, nodeNetwork);

            // Create the other nodes and connect them to the linear path network.
            int length = _pathTransforms.Length;
            for (int i = 1; i < length; i++)
            {
                nodeInfo = nodeInfoArray[i];
                // Create new node in the linearPathNetwork.
                currentNode = nodeNetwork.CreateCustomNode(nodeInfo.Layer, nodeInfo.WorldPosition);

                // Connect the last 2 nodes.
                Node previousNode = linearPathNetwork[i - 1];
                currentNode.AddConnection(previousNode);
                previousNode.AddConnection(currentNode);

                linearPathNetwork.Add(currentNode);
            }

            ConnectEndToNetwork(currentNode, nodeNetwork);

            return linearPathNetwork;
        }

        private void ConnectStartToNetwork(Node startNode, NodeNetwork nodeNetwork)
        {
            // Gets the entire area covered by the collider.
            List<Node> innerNetwork = base.CreateCustomNodeNetwork(nodeNetwork);
            if (_pathTransforms.Length == 0 || innerNetwork.Count == 0)
                throw new System.Exception("No pathTransforms or innerNetwork found.");

            // Connect the first Node of the linearPathNetwork to all Nodes of the innerNetwork.
            foreach (var node in innerNetwork)
            {
                startNode.AddConnection(node);
                node.AddConnection(startNode);
            }
        }
        private void ConnectEndToNetwork(Node endNode, NodeNetwork nodeNetwork)
        {
            Node connectionNode;
            // Depending on the connection type, the last Node will search a compatible node below it to create a network connection with.
            switch (_connectionType)
            {
                case ConnectionType.Full:
                    // Connects the last Node and Node below both ways. This creates a 2-way connection.
                    connectionNode = nodeNetwork.GetNodeBelow(endNode);
                    if (connectionNode != null)
                    {
                        endNode.AddConnection(connectionNode);
                        connectionNode.AddConnection(endNode);
                    }
                    else
                        throw new System.Exception("Cannot find node below.");
                    break;
                case ConnectionType.Single:
                    // Connects the last Node and Node below one ways. This creates a 1-way connection from start to end.
                    connectionNode = nodeNetwork.GetNodeBelow(endNode);
                    if (connectionNode != null)
                        endNode.AddConnection(connectionNode);
                    else
                        throw new System.Exception("Cannot find node below.");
                    break;
                case ConnectionType.Manual:
                    // No connection made at all. Create another linearPathNetwork and connect it's end to this linearPathNetwork's end.
                    // This creates a 2-way connection with both having an innerNetwork connected to their respective starts.
                    break;
                default:
                    break;
            }
        }

        // Gets all the info necessary to create Nodes for the pathLinearNetwork.
        private NodeInfo[] GetNodeInfoArray()
        {
            if (!_subDivide)
                return CreateDefaultNodeInfoArray();
            return CreateSubdivideNodeInfoArray();        
        }
        // Creates all the node info based solely on the _pathTransforms array.
        private NodeInfo[] CreateDefaultNodeInfoArray()
        {
            NodeInfo[] nodeInfoArray;
            int length;
            length = _pathTransforms.Length;
            nodeInfoArray = new NodeInfo[length];
            for (int i = 0; i < length; i++)
            {
                nodeInfoArray[i].WorldPosition = _pathTransforms[i].position;
                nodeInfoArray[i].Layer = _pathTransforms[i].gameObject.layer;
            }
            return nodeInfoArray;
        }
        // Creates all the node info with added subdivisions for the _pathTransforms array.
        private NodeInfo[] CreateSubdivideNodeInfoArray()
        {
            throw new System.NotImplementedException();
        }
        
        private struct NodeInfo
        {
            public int Layer;
            public Vector3 WorldPosition;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (_pathTransforms.Length == 0 || !ShowGizmos)
                return;

            Gizmos.color = Color.blue;

            Vector3 currentPosition = _pathTransforms[0].position;
            Gizmos.DrawSphere(currentPosition, 0.1f);

            Vector3 size = new Vector3((transform.localScale.x), (transform.localScale.y), 0);
            Vector3 position = transform.position;
            Vector3 worldBottomLeft = position - (size / 2);
            Vector3 worldBottomRight = worldBottomLeft + new Vector3(size.x, 0, 0);
            Vector3 worldTopLeft = worldBottomLeft + new Vector3(0, size.y, 0);
            Vector3 worldTopRight = worldBottomLeft + size;

            Gizmos.DrawWireCube(position, size);
            Gizmos.DrawLine(worldBottomLeft, currentPosition);
            Gizmos.DrawLine(worldBottomRight, currentPosition);
            Gizmos.DrawLine(worldTopLeft, currentPosition);
            Gizmos.DrawLine(worldTopRight, currentPosition);

            int length = _pathTransforms.Length;
            for (int i = 1; i < length; i++)
            {
                Gizmos.DrawLine(_pathTransforms[i - 1].position, _pathTransforms[i].position);
                Gizmos.DrawSphere(_pathTransforms[i].position, 0.1f);

            }
        }
    }
#endif
}