using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Path2d.Pathfinding;
using CustomAttributes;

namespace Path2d.CustomNodeNetwork {

    public abstract class CustomNodeNetworkComponent : MonoBehaviour
    {
        /// <summary>
        /// Finds the nodes this gameobject overlaps, modifies their layer to the gameobject's layer and returns all the nodes modified.
        /// </summary>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        /// <returns>List of all modified nodes</returns>
        public virtual List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork)
        {
            return CreateCustomNodeNetwork(nodeNetwork, gameObject.layer);
        }

        /// <summary>
        /// Finds the nodes this gameobject overlaps, modifies their layer to the specified layer and returns all the nodes modified.
        /// </summary>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        /// <param name="layer">The nodes their new layer</param>
        /// <returns>List of all modified nodes</returns>
        protected virtual List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork, int layer)
        {
            float spacing = nodeNetwork.Spacing;
            int innerNetworkSizeX = Mathf.Max(Mathf.RoundToInt(((transform.localScale.x)) / spacing), 1);
            int innerNetworkSizeY = Mathf.Max(Mathf.RoundToInt(((transform.localScale.y)) / spacing), 1);

            Vector3 size = new Vector3((transform.localScale.x), (transform.localScale.y), 0);
            Vector3 worldBottomLeft = transform.position - (size / 2);
            List<Node> innerNetworkNodes = new List<Node>();
            for (int x = 0; x <= innerNetworkSizeX + 0; x++)
            {
                for (int y = 0; y <= innerNetworkSizeY; y++)
                {
                    Vector3 worldPosition = worldBottomLeft + new Vector3(x * spacing, y * spacing, nodeNetwork.transform.position.z);
                    Node node = nodeNetwork.GetNodeFromWorldPosition(worldPosition, int.MaxValue, true);
                    if (node.LayerValue == NodeNetwork.UnwalkableLayer)
                        continue;

                    if (!innerNetworkNodes.Contains(node) && node.WorldPosition.z >= transform.position.z)
                    {
                        nodeNetwork.MofidyNode(node, layer);
                        innerNetworkNodes.Add(node);
                    }
                }
            }
            return innerNetworkNodes;
        }

#if UNITY_EDITOR
        
        public bool ShowGizmos = true;
        public Color GizmoColor = Color.white;
        
#pragma warning disable 414 // Disables "never used" warning.
        [SerializeField]
        [ReadOnly]
        private int _depthValue = -1;
#pragma warning restore 414

        public virtual void OnDrawGizmos()
        {
            if (!ShowGizmos)
                return;

            // Draws the rectangle used for the overlap within the CreateCustomNodeNetwork method.
            Gizmos.color = GizmoColor;
            Vector3 size = new Vector3((transform.localScale.x), (transform.localScale.y), 0);
            Vector3 position = transform.position;
            Vector3 worldBottomLeft = position - (size / 2);
            Vector3 worldBottomRight = worldBottomLeft + new Vector3(size.x, 0, 0);
            Vector3 worldTopLeft = worldBottomLeft + new Vector3(0, size.y, 0);
            Vector3 worldTopRight = worldBottomLeft + size;

            Gizmos.DrawWireCube(position, size);
        }
#endif
    }

}