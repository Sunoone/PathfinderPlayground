using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding.Collision;


namespace Pathfinding
{
    [System.Serializable]
    public class Region
    {
        public LayerMask Unwalkable;
        // SectionIds share the same value if the regions can access each other. It prevents searching path's which are not connected in any way.
        public int EnclosureId;

        private PathNetwork _pathNetwork;
        public Vector3 WorldPosition { get; private set; }
        private float _gridSpacing;

        public List<Node> Nodes = new List<Node>();
        public Vector3[] Extremes;
        private int _regionIndex;

        public Region(PathNetwork pathNetwork, Vector3 worldPosition, Vector3 regionSize, float gridSpacing, PathAgent pathAgent)
        {
            _pathNetwork = pathNetwork;
            WorldPosition = worldPosition;
            _gridSpacing = gridSpacing;
            Extremes = _pathNetwork.GetExtremes(WorldPosition, regionSize);
            GenerateNodes(pathAgent, -Vector3.forward);
        }     
        
        //@TODO: Support region up.
        //@TODO: Support 6 directions.
        public void GenerateNodes(PathAgent pathAgent, Vector3 nodeUp)
        {
            Vector3 rayPosition = Extremes[0];
            Vector3 endPosition = Extremes[1];
            // Other regions do not know of the distribution. Therefore regions should perhaps be scaled to the spacing.
            // Regions that go out of boundaries should still spawn and solve what they can within boundaries.
            for (; rayPosition.x <= endPosition.x; rayPosition.x += _gridSpacing)
            {           
                rayPosition.y = Extremes[0].y;
                for (int rowCount = 0; rayPosition.y <= endPosition.y; rayPosition.y += _gridSpacing, rowCount++)
                {
                    Vector3 rayOrigin = rayPosition;

                    // Shoves the entire row by half a spacing to form a diamond pattern. 
                    if (rowCount % 2 == 0)
                        rayOrigin.x += (_gridSpacing / 2);

                    Collider2D[] colliders = Physics2D.OverlapCircleAll(rayOrigin, pathAgent.Radius);
                    //RaycastHit2D[] colliders = Physics2D.RaycastAll(rayOrigin, Vector3.forward, pathAgent.Height);
                    if (colliders.Length == 0)
                        continue;

                    Debug.Log(colliders.Length);

                    int priorityLayer = int.MaxValue;
                    Collider2D priorityCollider = null;

                    foreach (var collider in colliders)
                    {
                        rayOrigin.z = collider.transform.position.z;
                        if (!collider.bounds.Contains(rayOrigin))
                            continue;

                        int colliderLayer = collider.gameObject.layer;
                        if (colliderLayer == PathNetwork.UnwalkableLayer || colliderLayer == PathNetwork.CustomLayer)
                        {
                            priorityLayer = colliderLayer;
                            priorityCollider = collider;
                            break;
                        }
                        if (colliderLayer < priorityLayer)
                        {
                            priorityLayer = colliderLayer;
                            priorityCollider = collider;               
                        }
                    }

                    if (priorityCollider == null)
                        continue;

                    switch (priorityLayer)
                    {
                        case PathNetwork.CustomLayer:
                            Debug.Log("Custom");
                            colliders = QuickSortByHeight(colliders);                  
                            int length = colliders.Length - 2;
                            for (int i = 0; i <= length; i++)
                            {
                                PathCollider pathCollider = colliders[i].transform.GetComponent<PathCollider>();
                                if (pathCollider != null)
                                {                                
                                    if (pathCollider.IsStatic)
                                        continue;

                                    Collider2D collider = colliders[i + 1];
                                    priorityLayer = collider.transform.gameObject.layer;
                                    if (PathNetwork.UnwalkableLayer == priorityLayer)
                                        break;

                                    // When you click it, the custom node should still be returned because of the pathCollider's collider.
                                    Debug.Log("CHECK");
                                    Node node = CreateNode(rayOrigin, collider, collider.transform.gameObject.layer, nodeUp);
                                    pathCollider.OverlappedNodes.Add(node);
                                    break;
                                }
                            }
                            break;
                        case PathNetwork.UnwalkableLayer:
                            Debug.Log("Break.");
                            break;
                        default:
                            Debug.Log("Normal");
                            CreateNode(rayOrigin, priorityCollider, priorityLayer, nodeUp);
                            break;
                    }
                }
            }
        }

        private Collider2D[] QuickSortByHeight(Collider2D[] array)
        {

            int left = 0;
            int right = array.Length - 1;

            Quicksort(array, left, right);
            for (int i = 0; i < array.Length; i++)
            {
                Debug.Log("Element " + i.ToString() + ": " + array[i].transform.position.z);
            }
            return array;
        }
        private Collider2D[] Quicksort(Collider2D[] array, int left, int right)
        {
            if (left > right || left < 0 || right < 0) return null;

            int index = Partition(array, left, right);
            if (index != -1)
            {
                Quicksort(array, left, index - 1);
                Quicksort(array, index + 1, right);
            }
            return array;
        }
        private int Partition(Collider2D[] array, int left, int right)
        {
            if (left > right) return -1;

            int end = left;

            Collider2D pivot = array[right];    // choose last one to pivot, easy to code
            for (int i = left; i < right; i++)
            {
                if (array[i].transform.position.z < pivot.transform.position.z)
                {
                    Swap(array, i, end);
                    end++;
                }
            }
            Swap(array, end, right);
            return end;
        }
        public void Swap(Collider2D[] array, int indexA, int indexB)
        {
            Collider2D temp = array[indexA];
            array[indexA] = array[indexB];
            array[indexB] = temp;
            
        }

        // I do not like the jumping.
        private Node CreateNode(Vector3 circleOverlapPoint, Collider2D collider,  int layer, Vector3 nodeUp)
        {
            Vector3 worldPosition = circleOverlapPoint;
            worldPosition.z = collider.transform.position.z;
            Vector3Int networkPosition = _pathNetwork.GetNetworkPositionFromWorldPosition(worldPosition);
            Node node = _pathNetwork.CreateNode(layer, networkPosition, worldPosition, nodeUp);
            Nodes.Add(node);
            return node;
        }

        // Updates can be done without rays. Rays are only necessary to set the nodes' z value initially.

        // What is a beautifull way to assign neighbours...
        private void GenerateNodes(PathCollider[] pathColliders)
        {
            int length = pathColliders.Length;
            for (int i = 0; i < length; i++)
            {
                PathCollider pathCollider = pathColliders[i];
                int layer = pathCollider.gameObject.layer;
                Vector3Int networkPosition = _pathNetwork.GetNetworkPositionFromWorldPosition(pathCollider.transform.position);
                Node node = _pathNetwork.CreateNode(layer, networkPosition, pathCollider.transform.position, Vector3.forward);
                _pathNetwork.UpdateNode(node);
                Nodes.Add(node);
            }
        }

        // Every project should setup the correct layer priorities.
        private Node CreateNode(Vector3 worldPoint, int x, int y, int z = 0)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector3.forward);
           
            int layer = 0;
            int lowestLayer = int.MaxValue;
            foreach (var hit in hits)
            {
                int objectLayer = hit.transform.gameObject.layer;
                switch (objectLayer)
                {
                    case PathNetwork.CustomLayer:
                        
                        // Custom stuff here. Get the component and execute logic..?
                        break;
                    default:
                        if (objectLayer < lowestLayer)
                            layer = lowestLayer = objectLayer;
                        break;
                }
            }

            int movementPenalty = 0;
            //if (_walkableRegionsDictionary.ContainsKey(layer))
           //     movementPenalty = _walkableRegionsDictionary[layer];
            return new Node(layer, new Vector3Int(x, y, z),  worldPoint, -Vector3.forward, movementPenalty);
        }

        private List<PathCollider> _pathColliders;


        // With the use of the overlap, the other stuff can be rebuild.
        private bool TryGetPathColliderOverlap(PathCollider pathCollider, out Vector3[] overlapExtremes)
        {
            Vector3[] colliderExtremes = pathCollider.Extremes;
            Vector3[] pathNetworkExtremes = Extremes;

            if (colliderExtremes[1].x < pathNetworkExtremes[0].x || colliderExtremes[0].x > pathNetworkExtremes[1].x ||
                colliderExtremes[1].y < pathNetworkExtremes[0].y && colliderExtremes[0].y > pathNetworkExtremes[1].y ||
                colliderExtremes[1].z < pathNetworkExtremes[0].z || colliderExtremes[0].z > pathNetworkExtremes[1].z)
            {
                overlapExtremes = null;
                return false;
            }

            overlapExtremes = new Vector3[2];
            overlapExtremes[0].x = (colliderExtremes[0].x < pathNetworkExtremes[0].x) ? pathNetworkExtremes[0].x : colliderExtremes[0].x;
            overlapExtremes[0].y = (colliderExtremes[0].y < pathNetworkExtremes[0].y) ? pathNetworkExtremes[0].y : colliderExtremes[0].y;
            overlapExtremes[0].z = colliderExtremes[0].z;

            overlapExtremes[1].x = (colliderExtremes[1].x > pathNetworkExtremes[1].x) ? pathNetworkExtremes[1].x : colliderExtremes[1].x;
            overlapExtremes[1].y = (colliderExtremes[1].y > pathNetworkExtremes[1].y) ? pathNetworkExtremes[1].y : colliderExtremes[1].y;
            overlapExtremes[1].z = colliderExtremes[1].z;
            return true;
        }

        public void SetNeighboursForNodes()
        {
            foreach (var node in Nodes)
            {
                node.SetAvailableNeighbours(_pathNetwork);
            }
        }

        private void UpdateNodes(PathCollider[] dynamicColliders)
        {
            for (int i = 0; i < dynamicColliders.Length; i++)
            {

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