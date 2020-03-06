using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Path2d.CustomNodeNetwork
{
    public class LinearPath : CustomNodeNetworkComponent
    {
        private enum ConnectionType
        {
            TwoWay, 
            OneWay,
            Manual,
        }

#pragma warning disable 649
        [SerializeField][Tooltip("Creates nodes at regular intervals, making the path function like a regular path.")]
        private bool _subDivide = false; // Not implemented yet. Therefore restricting editor access.
        [SerializeField][Tooltip("Uses its scale to create an entry zone instead of an entry point.")]
        private bool _useScale = true;
        [SerializeField]
        private ConnectionType _connectionType = ConnectionType.OneWay;
        [SerializeField]
        private Transform[] _pathTransforms;
#pragma warning restore 649

        List<Node> linearPathNetwork = new List<Node>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        /// <returns></returns>
        public override List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork)
        {
            // Create NodeInfo array for all the new nodes that need to be created.
            List<NodeInfo> nodeInfoList = GetNodeInfoArray(nodeNetwork);

            // Create the first node of the linearPathNetwork.
            NodeInfo nodeInfo = nodeInfoList[0];
            Node currentNode = nodeNetwork.CreateCustomNode(nodeInfo.Layer, nodeInfo.WorldPosition);

            linearPathNetwork = new List<Node>();
            linearPathNetwork.Add(currentNode);

            ConnectStartToNetwork(currentNode, nodeNetwork);

            // Create the other nodes and connect them to the linear path network.
            int length = nodeInfoList.Count;
            for (int i = 1; i < length; i++)
            {
                nodeInfo = nodeInfoList[i];
                // Create new node in the linearPathNetwork.
                currentNode = nodeNetwork.CreateCustomNode(nodeInfo.Layer, nodeInfo.WorldPosition);
                currentNode.CanSimplify = false;

                // Connect the last 2 nodes.
                Node previousNode = linearPathNetwork[i - 1];
                currentNode.AddConnection(previousNode);
                previousNode.AddConnection(currentNode);

                linearPathNetwork.Add(currentNode);
            }

            ConnectEndToNetwork(currentNode, nodeNetwork);

            return linearPathNetwork;
        }

        /// <summary>
        /// Connects the first node of the linear path to the NodeNetwork
        /// </summary>
        /// <param name="startNode">First node of the linear path</param>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        private void ConnectStartToNetwork(Node startNode, NodeNetwork nodeNetwork)
        {
            if (!_useScale)
            {
                Node connectionNode = nodeNetwork.GetNodeBelow(startNode);
                if (connectionNode == startNode)
                    Debug.LogError("We have a problem.");
                Debug.Log("ConnectionNode: " + connectionNode.WorldPosition);
                if (connectionNode != null)
                {
                    startNode.AddConnection(connectionNode);
                    connectionNode.AddConnection(startNode);
                }
                return;
            }
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

        /// <summary>
        /// Connects the last node of the linear path to the NodeNetwork
        /// </summary>
        /// <param name="startNode">Last node of the linear path</param>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        private void ConnectEndToNetwork(Node endNode, NodeNetwork nodeNetwork)
        {
            Node connectionNode;
            
            // Depending on the connection type, the last Node will search a compatible node below it to create a network connection with.
            switch (_connectionType)
            {
                case ConnectionType.TwoWay:
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
                case ConnectionType.OneWay:
                    // Connects the last Node and Node below one ways. This creates a 1-way connection from start to end.
                    connectionNode = nodeNetwork.GetNodeBelow(endNode);
                    if (connectionNode != null)
                    {
                        endNode.AddConnection(connectionNode);
                    }
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

        /// <summary>
        /// Gets all the info necessary to create Nodes for the pathLinearNetwork.
        /// </summary>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        /// <returns></returns>
        private List<NodeInfo> GetNodeInfoArray(NodeNetwork nodeNetwork)
        {
            if (!_subDivide)
                return CreateDefaultNodeInfoArray();
            return CreateSubdivideNodeInfoArray(nodeNetwork);        
        }

        /// <summary>
        /// Creates all the node info based solely on the _pathTransforms array.
        /// </summary>
        /// <returns></returns>
        private List<NodeInfo> CreateDefaultNodeInfoArray()
        {
            int length = _pathTransforms.Length;
            List<NodeInfo> nodeInfoList = new List<NodeInfo>();
            for (int i = 0; i < length; i++)
            {
                NodeInfo nodeInfo = new NodeInfo(_pathTransforms[i].gameObject.layer, _pathTransforms[i].position);
                nodeInfoList.Add(nodeInfo);
            }
            return nodeInfoList;
        }

        /// <summary>
        /// Creates all the node info with added subdivisions for the _pathTransforms array.
        /// </summary>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        /// <returns></returns>
        private List<NodeInfo> CreateSubdivideNodeInfoArray(NodeNetwork nodeNetwork)
        {
            int length = _pathTransforms.Length;
            List<NodeInfo> nodeInfoList = new List<NodeInfo>();
            NodeInfo nodeInfo;
            for (int i = 1; i < length; i++)
            {
                int subdivisionCount = Mathf.Max(Mathf.FloorToInt((_pathTransforms[i].position - _pathTransforms[i - 1].position).magnitude / nodeNetwork.Spacing), 1);          

                Vector3 previousPosition = _pathTransforms[i - 1].position;
                Vector3 currentPosition = _pathTransforms[i].position;

                Vector3 interval = Vector3.zero;
                interval.x = (currentPosition.x - previousPosition.x) / subdivisionCount;
                interval.y = (currentPosition.y - previousPosition.y) / subdivisionCount;
                interval.z = (currentPosition.z - previousPosition.z) / subdivisionCount;

                // Subdivision nodes cannot be created if their NetorkPosition already exists. It will update the excisting one.
                for (int j = 0; j < subdivisionCount; j++)
                {            
                    nodeInfo = new NodeInfo(_pathTransforms[i- 1].gameObject.layer, previousPosition + j * interval);
                    nodeInfoList.Add(nodeInfo);
                }
               
                if (i == length -1)
                {
                    nodeInfo = new NodeInfo(_pathTransforms[i].gameObject.layer, _pathTransforms[i].position);
                    nodeInfoList.Add(nodeInfo);
                }
            }
            
            return nodeInfoList;
        }
        
        /// <summary>
        /// Info used to contruct new nodes for the linearPath.
        /// </summary>
        private struct NodeInfo
        {
            public int Layer { get; private set; }
            public Vector3 WorldPosition { get; private set; }
            public NodeInfo(int layer, Vector3 worldPosition)
            {
                Layer = layer;
                WorldPosition = worldPosition;
            }            
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (_pathTransforms.Length == 0 || !ShowGizmos)
                return;

            Gizmos.color = GizmoColor;

            Vector3 currentPosition = _pathTransforms[0].position;
            Gizmos.DrawSphere(currentPosition, 0.1f);

            if (_useScale)
            {
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
            }

            int length = _pathTransforms.Length;
            for (int i = 1; i < length; i++)
            {
                Gizmos.DrawLine(_pathTransforms[i - 1].position, _pathTransforms[i].position);
                Gizmos.DrawSphere(_pathTransforms[i].position, 0.1f);

            }

            length = linearPathNetwork.Count;
            for (int i = 1; i < length; i++)
            {
                Gizmos.DrawLine(linearPathNetwork[i - 1].WorldPosition, linearPathNetwork[i].WorldPosition);
                Gizmos.DrawSphere(linearPathNetwork[i].WorldPosition, 0.1f);
            }
        }
    }
#endif
}