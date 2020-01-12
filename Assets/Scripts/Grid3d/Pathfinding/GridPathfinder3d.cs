using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Grid3d.Pathfinding
{
    public enum PathStatus
    {
        Success,
        Partial,
        Fail,
    }

    public class GridPathfinder3d : MonoBehaviour
    {
        public Transform Seeker, Target;

        private Grid3d _grid;

        private void Start()
        {
            _grid = GetComponent<Grid3d>();
        }

        private void Update()
        {
            FindPath(Seeker.position, Target.position);
        }


        public void SetDestination(Vector3 position)
        {
            // Validate position
        }

        private void FindPath(Vector3 startPosition, Vector3 targetPosition)
        {
            Node startNode = _grid.NodeFromWorldPoint(startPosition);
            Node targetNode = _grid.NodeFromWorldPoint(targetPosition);

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while(openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 0; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost || openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    RetracePath(startNode, targetNode);
                    return;
                }

                foreach (Node neighbour in _grid.GetNeighbours(currentNode))
                {
                    if (closedSet.Contains(neighbour))
                        continue;

                    switch (neighbour.NodeType)
                    {
                        case NodeType.Walkable:
                            break;
                        case NodeType.NotWalkable:
                            continue;
                        case NodeType.Climable:
                            break;
                        default:
                            break;
                    }

                    int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbor < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbor;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }

        private void RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            path.Reverse();

            _grid.path = path;

        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distanceX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
            int distanceY = Mathf.Abs(nodeA.GridY - nodeB.GridY);
            int distanceZ = Mathf.Abs(nodeA.GridZ - nodeB.GridZ);

            if (distanceX < distanceY)
            {
                if (distanceY < distanceZ)
                {
                    return 17 * distanceX + 14 * (distanceY - distanceX) + 10 * (distanceZ - distanceY - distanceX); // x < y < z
                }
                else if (distanceX < distanceZ)
                {
                    return 17 * distanceX + 14 * (distanceZ - distanceX) + 10 * (distanceY - distanceZ - distanceX); // x < z < y
                }
                else
                {
                    return 17 * distanceZ + 14 * (distanceX - distanceZ) + 10 * (distanceY - distanceX - distanceZ); // z < x < y
                }
            }
            else 
            {
                if (distanceX < distanceZ)
                {
                    return 17 * distanceY + 14 * (distanceX - distanceY) + 10 * (distanceZ - distanceX - distanceY); // y < x < z
                }
                else if (distanceY < distanceZ)
                {
                    return 17 * distanceY + 14 * (distanceZ - distanceY) + 10 * (distanceX - distanceZ - distanceY); // y < z < x
                }      
                else
                {
                    return 17 * distanceZ + 14 * (distanceY - distanceZ) + 10 * (distanceX - distanceY - distanceZ); // z < y < x
                }
            }
        }
    }
}
