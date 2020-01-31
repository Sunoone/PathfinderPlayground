using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Algorithms;

namespace Path2D.Pathfinding
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
        private NodeNetwork _nodeNetwork;
        public int _currentCheckpointId = 0;
        private void Start()
        {
            _nodeNetwork = GetComponent<NodeNetwork>();
        }

        // Starts finding a path. The callback calls a method with the resulting path waypoints and success state.
        public void StartFindPath(Vector3 startPosition, Vector3 targetPosition, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback)
        {
            StartCoroutine(FindPath(startPosition, targetPosition, allowedTerrain, callback, true));
        }

        private IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback, bool compareEnclosure)
        {      
            Waypoint[] waypoints = new Waypoint[0];
            PathStatus pathStatus = PathStatus.Fail;

            Node startNode = _nodeNetwork.GetNodeFromWorldPosition(startPosition, allowedTerrain);
            Node targetNode = _nodeNetwork.GetNodeFromWorldPosition(targetPosition, int.MaxValue);

            if (startNode == null ||
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
                targetPosition = targetNode.WorldPosition;
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
                Vector2 directionNew = new Vector2(path[i - 1].NetworkPosition.x - path[i].NetworkPosition.x, path[i - 1].NetworkPosition.y - path[i].NetworkPosition.y);
                if (!path[i].CanSimplify || directionNew != directionOld || path[i - 1].LayerValue != path[i].LayerValue || path[i-1].WorldPosition.z != path[i].WorldPosition.z)
                {
                    waypoints.Add(new Waypoint(path[i].WorldPosition, path[i].LayerValue));
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
