using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Path2d.CustomNodeNetwork
{
    public class Gate : CustomNodeNetworkComponent
    {
        [SerializeField]
        private bool _unlocked = false;
        private List<Node> _innerNetworkNodes;

        /// <summary>
        /// Finds the nodes this gameobject overlaps, modifies their layer to the gameobject's layer or unwalkable, depending whether it is unlocked. Finally, it returns all the nodes modified.
        /// </summary>
        /// <param name="nodeNetwork">NodeNetwork which will be modified</param>
        /// <returns>List of all modified nodes</returns>
        public override List<Node> CreateCustomNodeNetwork(NodeNetwork nodeNetwork)
        {
            int layer = (_unlocked) ? gameObject.layer : NodeNetwork.UnwalkableLayer;
            _innerNetworkNodes = CreateCustomNodeNetwork(nodeNetwork, layer);
            return _innerNetworkNodes;
        }

        /// <summary>
        /// Modifies the layer of all the nodes' overlapped by this gameobject to unwalkable.
        /// </summary>
        [ContextMenu("Lock")]
        private void Lock()
        {
            _unlocked = false;
            int lockLayer = NodeNetwork.UnwalkableLayer;
            foreach (var node in _innerNetworkNodes)
                node.Modify(lockLayer, node.MovementPenalty);
        }

        /// <summary>
        /// Modifies the layer of all the nodes' overlapped by this gameobject to this gameobject's layer.
        /// </summary>
        [ContextMenu("Unlock")]
        private void Unlock()
        {
            _unlocked = true;
            int lockLayer = gameObject.layer;
            foreach (var node in _innerNetworkNodes)
                node.Modify(lockLayer, node.MovementPenalty);
        }
    }
}
