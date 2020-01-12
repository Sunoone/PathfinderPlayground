using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid2d.CustomNodes
{
    // This node has a fixed path when retraced. 
    public class LinearPathNode : Node
    {       
        private Node _previous;
        public override Node Previous
        {
            get { return _previous; }
            set
            {
                if (_lockPrevious)
                    return;
                _previous = value;
                _lockPrevious = true;
            }
        }
        private bool _lockPrevious = false;

        public LinearPathNode(int layerValue, Vector3 worldPosition, int gridX, int gridY, int penalty, Node previous, bool canSimplify) : base(layerValue, worldPosition, gridX, gridY, penalty)
        {
            Previous = previous;
            CanSimplify = canSimplify;
        }
    }
}
