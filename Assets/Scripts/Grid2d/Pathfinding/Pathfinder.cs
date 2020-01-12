using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Diagnostics;
using System;
using Algorithms;

namespace Grid2d.Pathfinding
{
    public enum PathStatus
    {
        Success,
        Partial,
        Fail,
    }

    [RequireComponent(typeof(Grid2d))]
    public class Pathfinder : MonoBehaviour
    {
        private Grid2d _grid;
        public int _currentCheckpointId = 0;
        private void Start()
        {
            _grid = GetComponent<Grid2d>();
        }

        // Starts finding a path. The callback calls a method with the resulting path waypoints and success state.
        public void StartFindPath(Vector3 startPosition, Vector3 targetPosition, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback)
        {
            StartCoroutine(FindPath(startPosition, targetPosition, allowedTerrain, callback));
        }

        private IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback)
        {
            //_currentCheckpointId = 0;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Waypoint[] waypoints = new Waypoint[0];
            PathStatus pathStatus = PathStatus.Fail;

            Node startNode = _grid.GetNodeFromWorldPosition(startPosition);
            Node targetNode = _grid.GetNodeFromWorldPosition(targetPosition);
            Vector3 targetNodePosition = targetNode.WorldPosition;

            //@TODO check entire layermap. Find path requires a parameter for allowed layers.
            if (startNode.LayerValue == Grid2d.UnwalkableLayer || targetNode.LayerValue == Grid2d.UnwalkableLayer)
            {
                callback(waypoints, PathStatus.Fail);
                yield break;
            }

            Heap<Node> openSet = new Heap<Node>(_grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.CurrentItemCount > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    stopWatch.Stop();
                    UnityEngine.Debug.Log("Path found: " + stopWatch.ElapsedMilliseconds + " ms.");
                    pathStatus = PathStatus.Success;

                    // Success goes here.
                    break;
                }

                //@TODO: Add direction bias
                foreach (Node neighbour in _grid.GetNeighbours(currentNode))
                {
                    Node neighbourBranch = neighbour.BranchNode;             
                    if (neighbourBranch != null)
                    {
                        while (neighbourBranch != neighbour)
                        {                 
                            UpdateNeighbour(ref openSet, ref closedSet, allowedTerrain, targetNode, currentNode, neighbourBranch);
                            neighbourBranch = neighbourBranch.Previous;
                        }
                    }
                    UpdateNeighbour(ref openSet, ref closedSet, allowedTerrain, targetNode, currentNode, neighbour);
                }
            }
            yield return null;

            //@TODO: Cleanup
            if (pathStatus == PathStatus.Fail)
            {
                targetNode = FindClosestNodeToTargetPosition(closedSet);
                targetPosition = targetNode.WorldPosition;
                pathStatus = PathStatus.Partial;
            }        
            waypoints = RetracePath(startNode, targetNode);
            if (waypoints.Length == 0)
            {
                callback(waypoints, PathStatus.Fail);
                yield break;
            }
            RestoreTruePosition(targetPosition, ref waypoints[waypoints.Length - 1]);
            callback(waypoints, pathStatus);
        }

        private void UpdateNeighbour(ref Heap<Node> openSet, ref HashSet<Node> closedSet, LayerMask allowedTerrain, Node targetNode, Node currentNode, Node neighbour)
        {
            if (closedSet.Contains(neighbour) || neighbour.LayerValue == Grid2d.UnwalkableLayer || !allowedTerrain.ContainsLayer(neighbour.LayerValue))
                return;

            int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovementPenalty;
            if (newMovementCostToNeighbor < neighbour.GCost || !openSet.Contains(neighbour))
            {
                neighbour.GCost = newMovementCostToNeighbor;
                neighbour.HCost = GetDistance(neighbour, targetNode);
                neighbour.Previous = currentNode;

                if (!openSet.Contains(neighbour))
                    openSet.Add(neighbour);
                else
                    openSet.UpdateItem(neighbour);
            }
        }

        //@TODO: Still has some issues with extremes -> Observation: When hugging a wall, finding a new nearest node seems unresponsive.
        private Node FindClosestNodeToTargetPosition(HashSet<Node> closedSet)
        {
            Node foundNode = null;
            int lowestHCost = int.MaxValue;
            int length = closedSet.Count;
            foreach (var node in closedSet)
            {
                if (node.HCost == 0)
                {
                    continue;
                }

                if (node.HCost < lowestHCost)
                {
                    foundNode = node;
                    lowestHCost = node.HCost;
                }
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
            Waypoint[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;
        }

        private Waypoint[] SimplifyPath(List<Node> path)
        {
            List<Waypoint> waypoints = new List<Waypoint>();
            Vector2 directionOld = Vector2.zero;
            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);
                if (!path[i].CanSimplify || directionNew != directionOld || path[i - 1].LayerValue != path[i].LayerValue)
                {
                    waypoints.Add(new Waypoint(path[i].WorldPosition, path[i].LayerValue));
                }
                directionOld = directionNew;
            }
            return waypoints.ToArray();
        }

        // Overrides the a waypoint's position to be to the specifiedd targetPosition instead of the grid node position.
        private void RestoreTruePosition(Vector3 targetPosition, ref Waypoint waypoint)
        {
            waypoint = new Waypoint(targetPosition, waypoint.LayerValue);
        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distanceX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
            int distanceY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

            if (distanceX < distanceY)
                return 14 * distanceX + 10 * (distanceY - distanceX);
            return 14* distanceY +10 * (distanceX - distanceY);
        }  
    }
}
