using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Algorithms;

namespace Path2d.Pathfinding
{
    public enum PathStatus
    {
        Success,
        Partial,
        Fail,
    }

    [RequireComponent(typeof(NodeNetwork))]
    public class Pathfinder : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool ShowDebugTimer = true;
#endif
        private NodeNetwork _nodeNetwork;
        private void Start()
        {
            _nodeNetwork = GetComponent<NodeNetwork>();
        }

        // Starts finding a path. The callback calls a method with the resulting path waypoints and success state.
        public void StartFindPath(Vector3 startPosition, Vector3 targetPosition, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback)
        {
            Node startNode = _nodeNetwork.GetNodeFromWorldPosition(startPosition, allowedTerrain, true);
            Node targetNode = _nodeNetwork.GetNodeFromWorldPosition(targetPosition, int.MaxValue, true);

            if (startNode == null)
                startNode = _nodeNetwork.GetShiftedNodeFromWorldPosition(startPosition, allowedTerrain, false);

            StartFindPath(startNode, targetNode, allowedTerrain, callback);
            //StartCoroutine(FindPath(startPosition, targetPosition, allowedTerrain, callback, false));
        }
        public void StartFindPath(Node startNode, Node targetNode, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback)
        {
            StartCoroutine(FindPath(startNode, targetNode, allowedTerrain, callback, false));
        }

        private IEnumerator FindPath(Node startNode, Node targetNode, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback, bool compareEnclosure)
        {
#if UNITY_EDITOR
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
#endif
            Waypoint[] waypoints = new Waypoint[0];
            PathStatus pathStatus = PathStatus.Fail;

            if (startNode == null || targetNode == null || startNode.LayerValue == NodeNetwork.UnwalkableLayer || 
                compareEnclosure && targetNode.LayerValue != NodeNetwork.UnwalkableLayer && startNode.EnclosureIndex != targetNode.EnclosureIndex)
            {
                callback(waypoints, PathStatus.Fail);
                yield break;
            }

            Heap<Node> openSet = new Heap<Node>(_nodeNetwork.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.CurrentItemCount > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathStatus = PathStatus.Success;
                    break;
                }            
                foreach (Node connectedNode in currentNode.Connections)
                {
                    UpdateConnection(ref openSet, ref closedSet, allowedTerrain, targetNode, currentNode, connectedNode);
                }
            }
            yield return null;

            if (pathStatus == PathStatus.Fail)
            {
                targetNode = FindClosestNode(closedSet);
                pathStatus = PathStatus.Partial;
            }
            foreach (var node in closedSet)
                node.Refresh();

            waypoints = RetracePath(startNode, targetNode);
            if (waypoints.Length == 0)
            {
                callback(waypoints, PathStatus.Fail);
                yield break;
            }
            callback(waypoints, pathStatus);

#if UNITY_EDITOR
            if (ShowDebugTimer)
            {
                stopWatch.Stop();
                Debug.Log("Found path: " + stopWatch.ElapsedMilliseconds + " ms.");
            }
#endif
        }

        private void UpdateConnection(ref Heap<Node> openSet, ref HashSet<Node> closedSet, LayerMask allowedTerrain, Node targetNode, Node currentNode, Node connectedNode)
        {
            if (closedSet.Contains(connectedNode) || !allowedTerrain.ContainsLayer(connectedNode.LayerValue))
                return;

            int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, connectedNode) + connectedNode.MovementPenalty;
            if (newMovementCostToNeighbor < connectedNode.GCost || !openSet.Contains(connectedNode))
            {
                connectedNode.UpdateCosts(GetDistance(connectedNode, targetNode), newMovementCostToNeighbor);
                connectedNode.Previous = currentNode;

                if (!openSet.Contains(connectedNode))
                    openSet.Add(connectedNode);
                else
                    openSet.UpdateItem(connectedNode);
            }
        }

        // Uses the closedSet to filter out the closest Node to the target destination.
        private Node FindClosestNode(HashSet<Node> closedSet)
        {
            Node foundNode = Node.CreateCompareNode();
            foreach (var node in closedSet)
            {
                if (node.HCost > 0 && node.HCost < foundNode.HCost)
                    foundNode = node;
            }
            
            return foundNode;
        }

        private Waypoint[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                if (currentNode.Previous == null)
                    Debug.LogError("nope.");
                currentNode = currentNode.Previous;
            }
            if (path.Count == 0)
                return new Waypoint[0];
            Waypoint[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;
        }

        private Waypoint[] SimplifyPath(List<Node> path)
        {
            List<Waypoint> waypoints = new List<Waypoint>();
            Vector2 directionOld = Vector2.zero;
            int length = path.Count;
            for (int i = 1; i < length; i++)
            {
                Node previousNode = path[i - 1];
                Node currentNode = path[i];

                Vector2 directionNew = new Vector2(previousNode.NetworkPosition.x - currentNode.NetworkPosition.x, previousNode.NetworkPosition.y - currentNode.NetworkPosition.y);
                if (previousNode.LayerValue != currentNode.LayerValue || previousNode.WorldPosition.z != currentNode.WorldPosition.z)
                {
                    waypoints.Add(new Waypoint(previousNode.WorldPosition, previousNode.LayerValue));
                    waypoints.Add(new Waypoint(currentNode.WorldPosition, currentNode.LayerValue));
                }
                else if (!currentNode.CanSimplify || directionNew != directionOld)
                {
                    waypoints.Add(new Waypoint(currentNode.WorldPosition, currentNode.LayerValue));
                }          
                directionOld = directionNew;
            }
            Node lastNode = path[length - 1];
            waypoints.Add(new Waypoint(lastNode.WorldPosition, lastNode.LayerValue));
            return waypoints.ToArray();
        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distanceX = Mathf.Abs(nodeA.NetworkPosition.x - nodeB.NetworkPosition.x);
            int distanceY = Mathf.Abs(nodeA.NetworkPosition.y - nodeB.NetworkPosition.y);

            if (distanceX < distanceY)
                return 14 * distanceX + 10 * (distanceY - distanceX) + 14;
            return 14* distanceY + 10 * (distanceX - distanceY) + 14;
        }  
    }
}
