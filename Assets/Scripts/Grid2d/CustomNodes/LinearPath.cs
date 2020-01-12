using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid2d.CustomNodes
{
    public class LinearPath : CustomNodeComponent
    {
        [SerializeField][Tooltip("When false, the path cannot be simplified -> The Unit has to walk and complete the path towards each position.")]
        private bool _canSimplify = false;
#pragma warning disable 649
        [SerializeField]
        private Transform[] _positions;
#pragma warning restore 649
        private const int additionalLength = 1;

        //@TODO: Allow midway stops.
        public override Node CreateNode(Vector3 worldPosition, int inX, int inY)
        {
            Debug.Log("Created custom node.");
            // Sets up the entire node path.
            int length = _positions.Length + additionalLength;
            Node[] nodes = new Node[length];

            nodes[0] = new Node(gameObject.layer, worldPosition, inX, inY, 0);

            //@TODO Set the correct Gcosts and HCosts between the nodes.
            for (int i = 1; i < length; i++)
            {
                Vector3 position = _positions[i - additionalLength].position;
                Node previous = nodes[i - 1];
                Grid2d.GetGridPositionFromWorldPosition(position, out int x, out int y);
                nodes[i] = new LinearPathNode(gameObject.layer, position, x, y, 0, previous, _canSimplify); // Only last index should be true.       
            }
            nodes[0].BranchNode = nodes[length - 1];
            return nodes[0];
        }

#if UNITY_EDITOR
        [SerializeField]
        private Color _gizmoColor = Color.blue;
        public void OnDrawGizmos()
        {
            if (_positions.Length == 0 || _positions[0] == null)
                return;
            
            Gizmos.color = _gizmoColor;
            Gizmos.DrawLine(transform.position, _positions[0].position);
            for (int i = 1; i < _positions.Length; i++)
            {
                if (_positions[i] == null)
                    return;

                Gizmos.DrawSphere(_positions[i - 1].position, 0.2f);
                Gizmos.DrawLine(_positions[i].position, _positions[i-1].position);
            }
        }
#endif
    }
}