using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Path2d.CustomNodeNetwork
{
    public class BridgePath : CustomNodeNetworkComponent
    {
        public override List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork)
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
                        node.Connections.Clear();
                        nodeNetwork.MofidyNode(node, gameObject.layer);
                        innerNetworkNodes.Add(node);
                    }
                }
            }
            return innerNetworkNodes;
        }
    }
}
