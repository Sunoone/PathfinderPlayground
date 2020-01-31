using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Path2D.Pathfinding;
using CustomAttributes;

namespace Path2D.CustomNodes {

    public abstract class CustomNodeComponent : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable 414
        [SerializeField][ReadOnly]
        private int _depthValue = -1;
#pragma warning restore 414
        [SerializeField][HideInInspector]
        private NodeNetworkAgent _nodeNetworkAgent;
        [SerializeField][HideInInspector]
        private float _spacing;
#endif


        public virtual List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork)
        {
            float spacing = nodeNetwork.Spacing;
            int innerNetworkSizeX = Mathf.Max(Mathf.RoundToInt(((transform.localScale.x)) / spacing), 1);
            int innerNetworkSizeY = Mathf.Max(Mathf.RoundToInt(((transform.localScale.y)) / spacing), 1);

            Vector3 size = new Vector3((transform.localScale.x), (transform.localScale.y), 0);
            Vector3 worldBottomLeft = transform.position - (size/2);
            List<Node> innerNetworkNodes = new List<Node>();
            for (int x = 0; x <= innerNetworkSizeX + 0; x++)
            {
                for (int y = 0; y <= innerNetworkSizeY; y++)
                {
                    Vector3 worldPosition = worldBottomLeft + new Vector3(x * spacing, y * spacing, nodeNetwork.transform.position.z);
                    Node node = nodeNetwork.GetNodeFromWorldPosition(worldPosition, int.MaxValue);
                    if (node.LayerValue == NodeNetwork.UnwalkableLayer)
                        continue;

                    if (!innerNetworkNodes.Contains(node) && node.WorldPosition.z >= transform.position.z)
                    {
                        innerNetworkNodes.Add(node);
                    }
                }
            }
            return innerNetworkNodes;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _nodeNetworkAgent = FindObjectOfType<NodeNetwork>().Agent;
        }

        public bool ShowGizmos = true;
        public virtual void OnDrawGizmos()
        {
            if (!ShowGizmos)
                return;

            Vector3 size = new Vector3((transform.localScale.x), (transform.localScale.y), 0);
            Vector3 position = transform.position;
            Vector3 worldBottomLeft = position - (size / 2);
            Vector3 worldBottomRight = worldBottomLeft + new Vector3(size.x, 0, 0);
            Vector3 worldTopLeft = worldBottomLeft + new Vector3(0, size.y, 0);
            Vector3 worldTopRight = worldBottomLeft + size;

            Gizmos.DrawWireCube(position, size);
        }
    }
#endif
}