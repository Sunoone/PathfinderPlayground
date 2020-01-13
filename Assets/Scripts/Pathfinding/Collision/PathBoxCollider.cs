using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

namespace Pathfinding.Collision
{    
    [RequireComponent(typeof(BoxCollider2D))]
    public class PathBoxCollider : PathCollider
    {
        [SerializeField]
        private PathNetwork _pathNetwork;
        [SerializeField]
        private bool _isStatic = true;
        private Vector2 _size;
        private Vector2 _offset;
        [SerializeField]
        [HideInInspector]
        private BoxCollider2D _boxCollider2d;

        private void OnEnable()
        {
            _size = _boxCollider2d.size;
            _offset = _boxCollider2d.offset;
        }

        private void Reset()
        {
            _pathNetwork = FindObjectOfType<PathNetwork>();
            if (_pathNetwork == null)
            {
                Debug.LogError("Requires component " + typeof(PathNetwork).ToString() + " within the scene.");
                DestroyImmediate(this);
                return;     
            }
            _boxCollider2d = GetComponent<BoxCollider2D>();
        }

        //private List<int> _keys;

        public override void SetNodes()
        {
            // Problem with non-statics: Will not delete old nodes.
            Vector3[] extremes = GetBoxExtremes();
            Vector3Int BottomLeft = _pathNetwork.GetNetworkPositionFromWorldPosition(extremes[0]);
            Vector3Int TopRight = _pathNetwork.GetNetworkPositionFromWorldPosition(extremes[1]);

            int xMin = Mathf.RoundToInt(BottomLeft.x);
            int xMax = Mathf.RoundToInt(TopRight.x);

            int yMin = Mathf.RoundToInt(BottomLeft.y);
            int yMax = Mathf.RoundToInt(TopRight.y);

            int z = BottomLeft.z;

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    Vector3Int networkPosition = new Vector3Int(x, y, z);
                    //_keys.Add(Node.GetHashForNetworkPosition(networkPosition));
                    Node node = new Node(gameObject.layer, networkPosition, Vector3.zero, 0);                 
                    _pathNetwork.UpdateNode(node);
                }
            }
        }

        private Vector3[] GetBoxExtremes()
        {
            // Take rotation into account.
            // Extremes do not work for rotations...
            Vector3[] extremes = new Vector3[2];
            Vector3 halfSize = new Vector3(_size.x / 2, _size.y / 2, 0);
            Vector3 offset = new Vector3(_offset.x, _offset.y, 0);
            extremes[0] = transform.position + offset - halfSize;
            extremes[1] = transform.position + offset + halfSize;
            return extremes;
        }
    }
}
