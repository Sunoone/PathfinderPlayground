using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid2d.CustomNodes
{
    // Checkpoints have to be accessed in order. This allows for "passing gateways" without unwalkable terrain.
    public class Checkpoint : CustomNodeComponent
    {
        [SerializeField]
        private Checkpoint[] _checkpoints;

        /*public override Node CreateNode(Vector3 worldPosition, int x, int y)
        {
            return new Node(gameObject.layer, worldPosition, x, y, 0, checkPointId);
        }*/
    }
}
