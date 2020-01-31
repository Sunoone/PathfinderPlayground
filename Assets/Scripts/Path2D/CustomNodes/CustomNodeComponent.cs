using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Path2D.Pathfinding;
using CustomAttributes;

namespace Path2D.CustomNodes {
    // Manual nodes will be prioritized.
    [RequireComponent(typeof(BoxCollider2D))][ExecuteInEditMode]
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
        [SerializeField][HideInInspector]
        protected BoxCollider2D BoxCollider2d;

#endif


        public virtual List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork)
        {
            float spacing = nodeNetwork.Spacing;
            int innerNetworkSizeX = Mathf.Max(Mathf.RoundToInt(((BoxCollider2d.size.x * transform.localScale.x)) / spacing), 1);
            int innerNetworkSizeY = Mathf.Max(Mathf.RoundToInt(((BoxCollider2d.size.y * transform.localScale.y)) / spacing), 1);

            Vector3 size = new Vector3((BoxCollider2d.size.x * transform.localScale.x), (BoxCollider2d.size.y * transform.localScale.y), 0);
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

        private void Reset()
        {
#if UNITY_EDITOR
            _nodeNetworkAgent = FindObjectOfType<NodeNetwork>().Agent;
#endif
            BoxCollider2d = GetComponent<BoxCollider2D>();
        }
    }
}