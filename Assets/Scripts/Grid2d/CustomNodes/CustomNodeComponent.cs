using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid2d.Pathfinding;

namespace Grid2d.CustomNodes {
    // Manual nodes will be prioritized.
    [RequireComponent(typeof(BoxCollider2D))]
    public class CustomNodeComponent : MonoBehaviour
    {
        [SerializeField][HideInInspector]
        protected Grid2d Grid2d;

        public Node Node { get; private set; }

        public virtual Node CreateNode()
        {
            Grid2d.GetGridPositionFromWorldPosition(transform.position, out int x, out int y);
            Node = new Node(gameObject.layer, transform.position, x, y, 0);
            return Node;
        }

        public virtual Node CreateNode(Vector3 worldPosition, int x, int y)
        {
            Node = new Node(gameObject.layer, worldPosition, x, y, 0);
            return Node;
        }

        // Set the specified layer here
        private void Reset()
        {
            Grid2d = GameObject.FindObjectOfType<Grid2d>();
            if (Grid2d == null)
            {
                Debug.LogError("Cannot find object of type " + typeof(Grid2d).ToString() + ". Destroying " + this.ToString() + " component.");
                DestroyImmediate(this);
            }
        }
    }
}