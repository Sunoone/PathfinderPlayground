using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Grid3d
{
    // Almost never need 3d unless it is some kind of space game. 
    public class Grid3d : MonoBehaviour
    {
        public Transform Player;

        public LayerMask CheckLayermasks;

        public Vector3 GridWorldSize;
        public float NodeRadius;
        private Node[,,] _grid;

        private float _nodeDiameter;
        int _gridSizeX, _gridSizeY, _gridSizeZ;

        private void Start()
        {
            _nodeDiameter = NodeRadius * 2;
            _gridSizeX = Mathf.Max(Mathf.RoundToInt(GridWorldSize.x / _nodeDiameter), 1);
            _gridSizeY = Mathf.Max(Mathf.RoundToInt(GridWorldSize.y / _nodeDiameter), 1);
            _gridSizeZ = Mathf.Max(Mathf.RoundToInt(GridWorldSize.z / _nodeDiameter), 1);

            CreateGrid();
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        int checkX = node.GridX + x;
                        int checkY = node.GridY + y;
                        int checkZ = 0;// node.GridZ + z;

                        // @TODO: Add support for z axis

                        if ((checkX >= 0 && checkX < _gridSizeX) && (checkY >= 0 && checkY < _gridSizeY))
                        {
                            neighbours.Add(_grid[checkX, checkY, checkZ]);
                        }


                    }
                }
            }
            return neighbours;
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = Mathf.Clamp01((worldPosition.x + GridWorldSize.x / 2) / GridWorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.y + GridWorldSize.y / 2) / GridWorldSize.y);
            float percentZ = Mathf.Clamp01((worldPosition.z + GridWorldSize.z / 2) / GridWorldSize.z);

            int x = Mathf.Max(Mathf.RoundToInt((_gridSizeX - 1) * percentX), 0);
            int y = Mathf.Max(Mathf.RoundToInt((_gridSizeY - 1) * percentY), 0);
            int z = Mathf.Max(Mathf.RoundToInt((_gridSizeZ - 1) * percentZ), 0);
            return _grid[x, y, z];
        }

        private void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY, _gridSizeZ];
            Vector3 worldBottomLeft = transform.position - Vector3.right * GridWorldSize.x / 2 - Vector3.up * GridWorldSize.y / 2 - Vector3.forward * GridWorldSize.z / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    for (int z = 0; z < _gridSizeZ; z++)
                    {
                        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + NodeRadius) +
                            Vector3.up * (y * _nodeDiameter + NodeRadius) +
                            Vector3.forward * (z * _nodeDiameter + NodeRadius);
                        NodeType nodeType = GetNodeTypeForCollisions(Physics.OverlapSphere(worldPoint, NodeRadius, CheckLayermasks));                        
                        _grid[x, y, z] = new Node(nodeType, worldPoint,x, y, z);
                    }
                }
            }
        }

        private NodeType GetNodeTypeForCollisions(Collider[] collisions)
        {
            NodeType nodeType = NodeType.Walkable;
            int lowestLayer = int.MaxValue;
            foreach (var collider in collisions)
            {
                int layer = collider.gameObject.layer;
                if (layer >= 8 && layer <= 9 && layer < lowestLayer)
                {
                    nodeType = (NodeType)layer;
                    lowestLayer = layer;
                }
            }
            return nodeType;
        }

        public List<Node> path;
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, GridWorldSize.y, GridWorldSize.z));

            if (_grid != null)
            {

                Node playerNode = NodeFromWorldPoint(Player.position);
                
                foreach (Node node in _grid)
                {

                    

                    switch (node.NodeType)
                    {
                        case NodeType.Walkable:
                            Gizmos.color = Color.white;
                            break;
                        case NodeType.NotWalkable:
                            Gizmos.color = Color.red;
                            break;
                        case NodeType.Climable:
                            Gizmos.color = Color.cyan;
                            break;
                        default:
                            Gizmos.color = Color.grey;
                            break;
                    }

                    if (path != null)
                    {
                        if (path.Contains(node))
                            Gizmos.color = Color.black;
                    }

                    if (playerNode == node)
                    {
                        Gizmos.color = Color.magenta;
                    }

                    Gizmos.DrawSphere(node.WorldPosition, NodeRadius);//, Vector3.one * (_nodeDiameter - .1f));
                }
            }
        }
    }
}
