using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Path2d.CustomNodeNetwork;
using CustomAttributes;

namespace Path2d
{
    //@TODO: Remove grid and replace with neihgbouring node hashes/arrays
    public class NodeNetwork : MonoBehaviour
    {
        public const int UnwalkableLayer = 8;
        public const int CustomLayer = 12;

#if UNITY_EDITOR
        public bool OnlyDisplayGridGizmos = false;
#endif
#pragma warning disable 649
        [SerializeField]
        private NodeNetworkAgent _agent;
#pragma warning restore 649
        public NodeNetworkAgent Agent { get { return _agent; } }

        public Vector2 _nodeNetworkSize;
        public float Spacing { get; private set; }
        public int DefaultDepthValue { get; private set; }

        
        private Dictionary<int, Node> _nodeNetwork;
        private List<Node> _nodesInNetwork = new List<Node>();

        private int _gridSizeX, _gridSizeY;
        public int MaxSize { get { return _gridSizeX * _gridSizeY; } }

        private void Start()
        {
            CreateNodeNetwork();
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
                    //penaltiesHorizontalPass[0, y] += _grid[sampleX, y].MovementPenalty;
                    // Left here
                }
            }
        }

        #region Get methods
        // Gets the network position from the world position.
        private Vector3Int GetNetworkPositionFromWorldPosition(Vector3 worldPosition)
        {
            float percentX = Mathf.Clamp01((worldPosition.x + _nodeNetworkSize.x / 2) / _nodeNetworkSize.x);
            float percentY = Mathf.Clamp01((worldPosition.y + _nodeNetworkSize.y / 2) / _nodeNetworkSize.y);

            Vector3Int networkPosition = Vector3Int.zero;
            networkPosition.x = Mathf.Max(Mathf.RoundToInt((_gridSizeX - 1) * percentX), 0);
            networkPosition.y = Mathf.Max(Mathf.RoundToInt((_gridSizeY - 1) * percentY), 0);
            networkPosition.z = Mathf.RoundToInt((worldPosition.z) / Spacing);
            return networkPosition;
        }
        // Gets the node from the worldPosition. Will check all depths till DefaultDepthValue. Filters through a layermask.
        public Node GetNodeFromWorldPosition(Vector3 worldPosition, LayerMask allowedTerrain, bool searchDepth)
        {
            Vector3Int networkPosition = GetNetworkPositionFromWorldPosition(worldPosition);
            return GetNodeForNetworkPosition(networkPosition, allowedTerrain, searchDepth);
        }
        // Rounding innaccuraries can create a rare issue in which x positions get shifted by 1. This is a temporary solution.
        public Node GetShiftedNodeFromWorldPosition(Vector3 worldPosition, LayerMask allowedTerrain, bool searchDepth)
        {
            worldPosition.x += Spacing;
            Node node = GetNodeFromWorldPosition(worldPosition, allowedTerrain, searchDepth);
            if (node != null)
                return node;
            worldPosition.x -= Spacing * 2;
            return GetNodeFromWorldPosition(worldPosition, allowedTerrain, searchDepth);
        }
        // Heavy method that gets the closest node to the worldPosition. Filters through a LayerMask
        public Node FindClosestNode(Vector3 worldPosition, LayerMask allowedTerrain)
        {
            float shortestDistance = float.MaxValue;
            Node closestNode = null;
            // Distance is checked with the x and y only. Z is ignored.
            Vector3 comparePosition = worldPosition;
            comparePosition.z = 0;
            foreach (var item in _nodeNetwork)
            {
                if (allowedTerrain != (allowedTerrain | 1 << item.Value.LayerValue))
                    continue;

                Vector3 itemComparePosition = item.Value.WorldPosition;
                itemComparePosition.z = 0;
                float distance = (comparePosition - itemComparePosition).sqrMagnitude;
                if (distance < shortestDistance)
                {
                    closestNode = item.Value;
                    shortestDistance = distance;
                }
            }
            return closestNode;
        }
        // Checks every Node potentially behind the used Node.
        public Node GetNodeBelow(Node node)
        {
            return GetNodeBelow(node, _agent.NetworkLayerMask);
        }
        public Node GetNodeBelow(Node node, LayerMask allowedTerrain)
        {
            Vector3Int networkPosition = node.NetworkPosition;
            networkPosition.z++;
            return GetNodeForNetworkPosition(networkPosition, allowedTerrain, true);
        }
        // Checks every DepthValue from networkPosition. Returns the first found Node. Filters through LayerMask.
        private Node GetNodeForNetworkPosition(Vector3Int networkPosition, LayerMask allowedTerrain, bool searchDepth)
        {
            for (; networkPosition.z <= DefaultDepthValue; networkPosition.z++)
            {
                int hash = Node.CreateHashCode(networkPosition);
                if (TryGetNodeFromHash(hash, out Node node))
                {
                    if (allowedTerrain == (allowedTerrain | 1 << node.LayerValue))
                        return node;
                }
                if (!searchDepth)
                    break;
            }
            return null;
        }
        // Tries to get the Node through a hash.
        public bool TryGetNodeFromHash(int hash, out Node node)
        {
            if (_nodeNetwork.ContainsKey(hash))
            {
                node = _nodeNetwork[hash];
                return true;
            }
            node = null;
            return false;
        }
        // Tries to get the node through a world position. Does not itterate through DepthValues. Does not filter through LayerMask.
        private bool TryGetNodeFromWorldPosition(Vector3 worldPosition, out Node node)
        {
            Vector3Int networkPosition = GetNetworkPositionFromWorldPosition(worldPosition);
            int hash = Node.CreateHashCode(networkPosition);
            return TryGetNodeFromHash(hash, out node);
        }       
        #endregion

        // Method that will create the entire NodeNetwork using a NodeNetworkAgent for it's network's spacing of Nodes.
        public void CreateNodeNetwork()
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            _nodeNetwork = new Dictionary<int, Node>();

            Spacing = Mathf.Min(_agent.Size.x, _agent.Size.y);
            DefaultDepthValue = Mathf.RoundToInt(transform.position.z / Spacing);
            _gridSizeX = Mathf.Max(Mathf.RoundToInt(_nodeNetworkSize.x / Spacing), 1);
            _gridSizeY = Mathf.Max(Mathf.RoundToInt(_nodeNetworkSize.y / Spacing), 1);
            Vector3 worldBottomLeft = transform.position - new Vector3(_nodeNetworkSize.x / 2, _nodeNetworkSize.y / 2, 0);
           
            for (int x = 0; x <= _gridSizeX; x++)
            {          
                for (int y = 0; y <= _gridSizeY; y++)
                {
                    Vector3 overlapOrigin = worldBottomLeft + new Vector3(x * Spacing, y * Spacing, 0);
                    Vector3Int networkPosition = new Vector3Int(x, y, DefaultDepthValue);
                    if (TryCreateNode(networkPosition, overlapOrigin, out Node node))
                        _nodesInNetwork.Add(node);
                }
            }         
            ConnectNodeNetwork(_nodesInNetwork);
            SolveCustomNodes(_nodesInNetwork);

            stopWatch.Stop();
            UnityEngine.Debug.Log("Created network: " + stopWatch.ElapsedMilliseconds + " ms.");
        }

        // Tries to create a node through collision checks.
        public bool TryCreateNode(Vector3Int networkPosition, Vector3 worldPosition, out Node node)
        {
            //@TODO: Doesn't take height as the priority. This should be changed.
            Collider2D[] colliders = Physics2D.OverlapBoxAll(worldPosition, _agent.Size, 0, _agent.NetworkLayerMask);
            if (colliders.Length == 0)
            {
                node = null;
                return false;
            }

            // Sort collisions by height. Closest compatible collision to camera has highest priority.
            QuickSortByHeight(colliders);
            int layer = colliders[0].gameObject.layer;
            
            worldPosition.z = colliders[0].transform.position.z;
            node = CreateNode(layer, networkPosition, worldPosition);
            return true;
        }

        // Will always create a Node. Should not be used manually due to hash conflicts.
        private Node CreateNode(int layer, Vector3Int networkPosition, Vector3 worldPosition)
        {
            _agent.WalkableTerrainTypesDictionary.TryGetValue(layer, out int movementPenalty);
            Node node = new Node(layer, movementPenalty, networkPosition, worldPosition);
            int hash = node.GetHashCode();
            _nodeNetwork.Add(hash, node);
            return node;
        }
        // Will create or modift a Node. When making nodes manually, use this method.
        public Node CreateCustomNode(int layer, Vector3 worldPosition)
        {
            Vector3Int networkPosition = GetNetworkPositionFromWorldPosition(worldPosition);
            int hash = Node.CreateHashCode(networkPosition);
            if (!TryGetNodeFromHash(hash, out Node node))
                node = CreateNode(layer, networkPosition, worldPosition);
            else
                MofidyNode(node, layer);
            return node;
        }
        // Will only modify an existing Node.
        public void MofidyNode(Node node, int layer)
        {
            _agent.WalkableTerrainTypesDictionary.TryGetValue(layer, out int movementPenalty);
            node.Modify(layer, movementPenalty);
        }

        // CustomNodeComponents within the scene will be gathered and activated. This creates custom customized networks.
        private void SolveCustomNodes(List<Node> nodesInNetwork)
        {
            CustomNodeNetworkComponent[] customNodeComponents = FindObjectsOfType<CustomNodeNetworkComponent>();
            foreach (var customNodeComponent in customNodeComponents)
            {
                List<Node> newNodes = customNodeComponent.CreateCustomNodeNetwork(this);
                foreach (var node in newNodes)
                {
                    int hash = node.GetHashCode();
                    if (!_nodeNetwork.ContainsKey(hash))
                        _nodeNetwork.Add(hash, node);
                    else
                        _nodeNetwork[hash] = node;
                }
            }
        }

        // Creates the basic connections between Nodes within the NodeNetwork.
        private void ConnectNodeNetwork(List<Node> nodesToResolve)
        {       
            foreach (var node in nodesToResolve)
                node.FindAndSetConnections(this);
        }

       

        #region QuickSortColliders
        private void QuickSortByHeight(Collider2D[] array)
        {
            int left = 0;
            int right = array.Length - 1;

            Quicksort(array, left, right);
        }
        private void Quicksort(Collider2D[] array, int left, int right)
        {
            if (left > right || left < 0 || right < 0)
                return;

            int index = Partition(array, left, right);
            if (index != -1)
            {
                Quicksort(array, left, index - 1);
                Quicksort(array, index + 1, right);
            }
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
        #endregion

        private void OnValidate()
        {
            _agent.UpdateLayerMask();
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Spacing = Mathf.Min(_agent.Size.x, _agent.Size.y);
            Gizmos.DrawWireCube(transform.position, new Vector3(_nodeNetworkSize.x, _nodeNetworkSize.y));
            if (_nodeNetwork == null || !OnlyDisplayGridGizmos)
                return;

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position, _nodeNetworkSize);

            foreach (var pair in _nodeNetwork)
            {
                if (pair.Value.LayerValue == UnwalkableLayer)
                    Gizmos.color = Color.red;
                else if (pair.Value.LayerValue == 9)
                    Gizmos.color = Color.blue;
                else if (pair.Value.LayerValue > 0 && pair.Value.LayerValue < UnwalkableLayer)
                    Gizmos.color = Color.magenta;
                else
                    Gizmos.color = Color.white;
                //Gizmos.DrawSphere(GetWorldPositionFromGridPosition(node.GridX, node.GridY), _nodeDiameter - .1f);
                Gizmos.DrawSphere(pair.Value.WorldPosition, Spacing / 2);//, Vector3.one * (_nodeDiameter - .1f));
            }
            Gizmos.color = Color.white;
        }
#endif
    }
}
