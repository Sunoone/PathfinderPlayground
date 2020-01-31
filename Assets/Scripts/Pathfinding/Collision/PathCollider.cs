using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

namespace Pathfinding.Collision
{
    public abstract class PathCollider : MonoBehaviour
    {
        [SingleLayer][SerializeField]
        private LayerMask _layer;
        public int Layer { get => _layer; }
        [SerializeField]
        private bool _isStatic = true;
        public bool IsStatic { get => _isStatic; }
        public Vector3[] Extremes { get; protected set; }
        public List<Node> OverlappedNodes = new List<Node>();

        private void Start()
        {
            if (gameObject.layer != (PathNetwork.CustomLayer | gameObject.layer))
            {
                gameObject.SetActive(false);
                throw new System.Exception("Cannot use " + GetType() + ", layer is not set to " + LayerMask.LayerToName(PathNetwork.CustomLayer) + ". Disabling component.");
            }
        }

        protected virtual void Reset()
        {
            _layer = gameObject.layer;
            gameObject.layer = PathNetwork.CustomLayer;
        }
    }
}