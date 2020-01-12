using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid2d.CustomNodes;

namespace Grid2d
{
    //@TODO: Remove grid and replace with neihgbouring node hashes/arrays
    public class Grid2d : MonoBehaviour
    {
        // Set these up according to your project speficiations.
        public const int UnwalkableLayer = 8;
        
#if UNITY_EDITOR
        public bool OnlyDisplayGridGizmos = false;
#endif
        private LayerMask _fullMask;

        [MultiLayer(9)]
        public LayerMask InteractableTerrain;

        public Vector2 GridWorldSize;
        public float NodeRadius;
        private Node[,] _grid;

        [Tooltip("Start the layernames with an _")]
        public TerrainType[] WalkableRegions;
        private Dictionary<int, int> _walkableRegionsDictionary = new Dictionary<int, int>();

        private float _nodeDiameter;
        int _gridSizeX, _gridSizeY;

        public int MaxSize { get { return _gridSizeX * _gridSizeY; } }

        private void Awake()
        {
            _nodeDiameter = NodeRadius * 2;
            _gridSizeX = Mathf.Max(Mathf.RoundToInt(GridWorldSize.x / _nodeDiameter), 1);
            _gridSizeY = Mathf.Max(Mathf.RoundToInt(GridWorldSize.y / _nodeDiameter), 1);

            _fullMask = InteractableTerrain | (1 << UnwalkableLayer);
            foreach (var region in WalkableRegions)
            {

                if (!_walkableRegionsDictionary.ContainsKey(region.TerrainMask.value))
                {
                    // Adds any terrain with a penalty to the dictionary.
                    _walkableRegionsDictionary.Add(region.TerrainMask.value, region.TerrainPenalty);
                    // Adds the terrain to the fullmask. This way they do not need to be manually added to the InteractableTerrain mask.
                    if (!InteractableTerrain.ContainsLayer(region.TerrainMask.value))
                    {
                        _fullMask |= (1 << region.TerrainMask.value);
                    }
                }
            }
            CreateGrid();
        }

        private void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize * 2 + 1;
            int kernelExtends = (kernelSize - 1) / 2;

            int[,] penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
            int[,] penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = kernelExtends; x <= kernelExtends; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernelExtends);
                    penaltiesHorizontalPass[0, y] += _grid[sampleX, y].MovementPenalty;
                    // Left here
                }
            }
        }

        // Gets the neihbours of the node. 
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    // @TODO: Add support for z axis

                    if ((checkX >= 0 && checkX < _gridSizeX) && (checkY >= 0 && checkY < _gridSizeY))
                    {
                        neighbours.Add(_grid[checkX, checkY]);
                    }
                }
            }
            return neighbours;
        }

        // Gets the nearest node to worldposition.
        public Node GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            GetGridPositionFromWorldPosition(worldPosition, out int x, out int y);
            return _grid[x, y];
        }
        public void GetGridPositionFromWorldPosition(Vector3 worldPosition, out int x, out int y)
        {
            float percentX = Mathf.Clamp01((worldPosition.x + GridWorldSize.x / 2) / GridWorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.y + GridWorldSize.y / 2) / GridWorldSize.y);

            x = Mathf.Max(Mathf.RoundToInt((_gridSizeX - 1) * percentX), 0);
            y = Mathf.Max(Mathf.RoundToInt((_gridSizeY - 1) * percentY), 0);
        }
        public Vector3 GetWorldPositionFromGridPosition(int x, int y)
        {
            Vector3 worldBottomLeft = transform.position - Vector3.right * GridWorldSize.x / 2 - Vector3.up * GridWorldSize.y / 2;
            return worldBottomLeft + Vector3.right * (x * _nodeDiameter + NodeRadius) + Vector3.up * (y * _nodeDiameter + NodeRadius);
        }

        private void CreateGrid()
        {       
            _grid = new Node[_gridSizeX, _gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * GridWorldSize.x / 2 - Vector3.up * GridWorldSize.y / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {

                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + NodeRadius) + Vector3.up * (y * _nodeDiameter + NodeRadius);

                    _grid[x, y] = CreateNode(worldPoint, x, y);
                    
                   
                }
            }
        }

        // Every project should setup the correct layer priorities.
        private Node CreateNode(Vector3 worldPoint, int x, int y)
        {
            Collider2D[] collisions = Physics2D.OverlapCircleAll(worldPoint, NodeRadius, _fullMask);
            int layer = 0;
            int lowestLayer = int.MaxValue;
            foreach (var collider in collisions)
            {
                CustomNodeComponent customNode = collider.GetComponent<CustomNodeComponent>();
                if (customNode != null)
                    return customNode.CreateNode(worldPoint, x, y);

                int objectLayer = collider.gameObject.layer;   
                if (objectLayer < lowestLayer)
                    layer = lowestLayer = objectLayer;
            }

            int movementPenalty = 0;
            if (_walkableRegionsDictionary.ContainsKey(layer))
                movementPenalty = _walkableRegionsDictionary[layer];
            return new Node(layer, worldPoint, x, y, movementPenalty);
        }

#if UNITY_EDITOR     
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, GridWorldSize.y));
            if (_grid == null || !OnlyDisplayGridGizmos)
                return;

            foreach (Node node in _grid)
            {

                Gizmos.color = Color.white;

                //Gizmos.DrawSphere(GetWorldPositionFromGridPosition(node.GridX, node.GridY), _nodeDiameter - .1f);
                Gizmos.DrawSphere(node.WorldPosition, NodeRadius);//, Vector3.one * (_nodeDiameter - .1f));
            }
            Gizmos.color = Color.white;
        }
#endif
    }

    [System.Serializable]
    public class TerrainType
    {
        [SingleLayer("_")]
        public LayerMask TerrainMask;
        public int TerrainPenalty;
    }
}
