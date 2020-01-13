using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding.Collision;


namespace Pathfinding
{
    // Region is a macro space of nodes. It allows for single region replacements.
    // Nested...? 
    public class Region
    {
        public LayerMask Unwalkable;

        // If any dynamic object is in this region, it will be dynamic.
        private bool _static = false;

        // SectionIds share the same value if the regions can access each other. It prevents searching path's which are not connected in any way.
        public int EnclosureId;

        private PathNetwork _pathingNetwork;
        public Vector3 WorldPosition { get; private set; }
        private Vector3 _regionSize;
        private Vector3 _gridSpacing;

        public Region[] NeighbouringRegions;
        public List<Node> Nodes = new List<Node>();
        private List<Vector3> _rayOrigins;
        private int width;
        private int height;
        private int depth;

        public Region(PathNetwork pathingNetwork, Vector3 worldPosition, Vector3 regionSize, Vector3 gridSpacing, PathCollider[] colliders)
        {
#if UNITY_EDITOR
            if (regionSize.x <= 0 || regionSize.y <= 0)
                throw new Exception("RegionSize too small.");
            if (gridSpacing.x <= 0 || gridSpacing.y <= 0)
                throw new Exception("GridSpacing too small.");
#endif
            _pathingNetwork = pathingNetwork;
            WorldPosition = worldPosition;
            _regionSize = regionSize;
            _gridSpacing = gridSpacing;
            CalculateRayPositions();
            GenerateNodes(colliders);
        }   

        // 1 corner inside
        // 1 edge inside
        // Envelop
        
        public Node GetNodeForGridSpace(int x, int y)
        {
            if (x > width || x < 0 || y > height)
                throw new System.IndexOutOfRangeException();
            return Nodes[((y > 0) ? (y - 1) * width : 0) + x];
        }

        private void CalculateRayPositions()
        {
            width = Mathf.Max(Mathf.RoundToInt(_regionSize.x / _gridSpacing.x), 1);
            height = Mathf.Max(Mathf.RoundToInt(_regionSize.y / _gridSpacing.y), 1);
            depth = Mathf.Max(Mathf.RoundToInt(_regionSize.z / _gridSpacing.z), 1);

            Debug.Log(width + ", " + height);
            _rayOrigins = new List<Vector3>();
            Vector3 halfRegionSize = _regionSize / 2;
            Vector3 halfGridSpacingSize = _gridSpacing / 2;
            Vector3 topRight = WorldPosition + halfRegionSize;
            Vector3 bottomLeft = WorldPosition - halfRegionSize + halfGridSpacingSize;

            Vector3 rayPosition = topRight;
            float z = topRight.z;
            for (int x = 0; x < width; x++)
            {              
                rayPosition.x = bottomLeft.x + (x * _gridSpacing.x);
                for (int y = 0; y < height; y++)
                {
                    rayPosition.y = bottomLeft.y + (y * _gridSpacing.y);
                    _rayOrigins.Add(rayPosition);
                }
            }
        }

        private Vector3 WorldToGridPosition(Vector3 worldPosition)
        {
            return Vector3.zero;
        }

        // What is a beautifull way to assign neighbours...
        private void GenerateNodes(PathCollider[] colliders)
        {




            int length = _rayOrigins.Count;
            for (int i = 0; i < length; i++)
            {
                if (i == 0 || ((i + 1) % width == 0) || ((i + 1) % width == width -1) || i == length - 1)
                {
                    // The borders. Irrelevant for now. But eventually need links to each other.
                }
                RaycastHit2D[] hits = Physics2D.RaycastAll(_rayOrigins[i], Vector3.forward, _regionSize.z, _collisionMask);
                foreach (var hit in hits)
                {
                    int layer = hit.transform.gameObject.layer;

                    /*CustomNode customNode = hit.transform.GetComponent<CustomNode>();
                    if (customNode != null)
                    {
                        throw new NotImplementedException();
                    }*/
                    int penalty = 0;
                    Vector3Int networkPosition =_pathingNetwork.GetNetworkPositionFromWorldPosition(hit.point);
                    Node node = new Node(layer, networkPosition, hit.point, penalty);
                    _pathingNetwork.UpdateNode(node);                   
                    Nodes.Add(node);
                }
            }
            
        }      

        public void SetNeighboursForNodes()
        {
            foreach (var node in Nodes)
            {
                node.SetAvailableNeighbours(_pathingNetwork);
            }
        }

        // Rebuilding everytime in its entirety. As every node connection needs to be updated.
        public void UpdateRegion() // Add support for pathfinding agents.  
        {
            if (_static)
                return;

            UpdateNodes();
        }

        private void UpdateNodes(PathCollider[] dynamicColliders)
        {
            int length = _rayOrigins.Count;
            for (int i = 0; i < length; i++)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(_rayOrigins[i], Vector3.forward, _regionSize.z, _collisionMask);
                foreach (var hit in hits)
                {
                    Vector3Int gridPosition = _pathingNetwork.GetNetworkPositionFromWorldPosition(hit.point);
                 
                    int layer = hit.transform.gameObject.layer;
                    int penalty = 0;

                    if (_pathingNetwork.TryGetNodeForNodePosition(gridPosition, out Node node))
                    {                                
                        node.UpdateNodeLayer(layer, penalty);
                    }
                    else
                    {
                        node = new Node(layer, gridPosition, hit.point, penalty);
                        _pathingNetwork.UpdateNode(node);
                        Nodes.Add(node);
                    }
                }
            }
        }

        public Node GetClosestNode(Vector3Int gridPosition)
        {
            Node nearestNode = Nodes[0];
            float shortestDistance = (gridPosition - Nodes[0].NetworkPosition).sqrMagnitude;
            int length = Nodes.Count;
            for (int i = 1; i < length; i++)
            {
                float distance = (gridPosition - Nodes[i].NetworkPosition).sqrMagnitude;
                if (distance < shortestDistance)
                {
                    nearestNode = Nodes[i];
                    shortestDistance = distance;
                }
            }
            return nearestNode;
        }


        // Call when any change has taken place in the specific region.
        public void RegionChanged()
        {
            CheckAndSolveSplits();
        }

        // Checks whether the region is cut (in real-time)
        private void CheckAndSolveSplits()
        {
            bool split = false;
            // Only check nodes in the same region.
            // Check whether you can access all nodes. 
            // - Check all possibilites from a biased node.
            // - Checked nodes are stored in a list.
            // - Therefore unchecked nodes are inaccessible.
            // - From there update the section. -> Now do the same in the unchecked nodes. This allows for sections to split multiple times.
            if (split)
                UpdateSectionIds();
        }


        // Check neihbouring regions. If they have a different SectionId, find which one is more common and override all the less common id's.
        private void UpdateSectionIds()
        {

        }
    }
}