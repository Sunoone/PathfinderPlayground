using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pathfinding.Collision
{    
    [RequireComponent(typeof(BoxCollider2D))]
    public class PathBoxCollider : PathCollider
    {     
        [SerializeField]
        private PathNetwork _pathNetwork;
        
        private Vector2 _size;
        private Vector2 _offset;
        [SerializeField]
        [HideInInspector]
        private BoxCollider2D _boxCollider2d;

        // Only need to use the edge. So it can shoot rays for every edge position on the grid. Then connect that to the outside neighbour. 
        // How will this update? Skip the update for now? 

        // They can raycast 1 pathnetwork space outside. Then check the height difference for hit results. If it is within the maximum jump height, connect.

        private void OnEnable()
        {
            Extremes = GetExtremes();
            _size = _boxCollider2d.size;
            _offset = _boxCollider2d.offset;
        }


        protected override void Reset()
        {
            _pathNetwork = FindObjectOfType<PathNetwork>();
            if (_pathNetwork == null)
            {
                Debug.LogError("Requires component " + typeof(PathNetwork).ToString() + " within the scene.");
                DestroyImmediate(this);
                return;     
            }
            _boxCollider2d = GetComponent<BoxCollider2D>();
            base.Reset();
        }

        public Vector3[] GetExtremes()
        {
            Vector3 offset = new Vector3(_offset.x, _offset.y, 0);
            return _pathNetwork.GetExtremes(transform.position + offset, _size);
        }
    }
}
