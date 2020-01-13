using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Algorithms;
using System.Diagnostics;
using Pathfinding.Collision;

namespace Pathfinding
{
    // Sets up the network for all paths using nodes and regions.
    public class PathNetwork : MonoBehaviour
    {
        public PathCollider[] Colliders { get; private set; }
        //@TODO: Store the data
        private Dictionary<int, Node> _networkNodes = new Dictionary<int, Node>();
        private Dictionary<int, Node> _borderNodes = new Dictionary<int, Node>();

        public int RandomRange = 10;
        [SerializeField]
        private Vector3 _pathNetworkSize;
        [SerializeField]
        private Vector3 _regionSize;
        [SerializeField]
        private Vector3 _gridSpacing;
        [SerializeField]
        private LayerMask _collisionMask;

        public List<Region> Regions { get; private set; }

        private int _sizeX, _sizeY, _sizeZ;
        private void Start()
        {
            Colliders = FindObjectsOfType<PathCollider>();

            _sizeX = Mathf.Max(Mathf.RoundToInt(_pathNetworkSize.x / _gridSpacing.x), 1);
            _sizeY = Mathf.Max(Mathf.RoundToInt(_pathNetworkSize.y / _gridSpacing.y), 1);
            _sizeZ = Mathf.Max(Mathf.RoundToInt(_pathNetworkSize.z / _gridSpacing.z), 1);
            InitRegions();
        }

        private void InitRegions()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

#if UNITY_EDITOR
            if (_pathNetworkSize.x <= 0 || _pathNetworkSize.y <= 0)
                throw new System.Exception("PathingNetworkSize too small.");
#endif

            Regions = new List<Region>();
            Vector3 halfPathingNetworkSize = _pathNetworkSize / 2;
            Vector3 halfRegionSize = _regionSize / 2;

            Vector3 startPosition = transform.position - halfPathingNetworkSize + halfRegionSize;
            Vector3 endPosition = transform.position + halfPathingNetworkSize - halfRegionSize;
            for (float x = startPosition.x; x  <= endPosition.x; x += _regionSize.x)
            {
                for (float y = startPosition.y; y <= endPosition.y; y += _regionSize.y)
                {
                    Region region = new Region(this, new Vector3(x, y, transform.position.z), _regionSize, _gridSpacing, _collisionMask);
                    Regions.Add(region);
                }
            }
            stopWatch.Stop();
            UnityEngine.Debug.Log("Created regions: " + stopWatch.ElapsedMilliseconds + " ms.");

            foreach (var region in Regions)
            {
                region.SetNeighboursForNodes();
            }
            //foreach()
        }

        private void SetNeighbours()
        {

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                for (int i = 0; i < RandomRange; i++)
                {
                    RebuildRandomRegion();
                }
                stopWatch.Stop();
                UnityEngine.Debug.Log(RandomRange + " regions updates: " + stopWatch.ElapsedMilliseconds + " ms.");
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;
                if (TryGetNodeForWorldPosition(position, out Node node))
                {
                    node.NodeGizmoColor = Color.magenta;
                    int count = 0;
                    foreach (var neighbour in node.Connections)
                    {
                        neighbour.NodeGizmoColor = Color.cyan;
                        count++;
                    }
                    UnityEngine.Debug.Log("Clicked: " + count);
                }
            }
        }

        public void RebuildRandomRegion()
        {
            int randomIndex = Random.Range(0, Regions.Count - 1);
            Regions[randomIndex].UpdateRegion();
        }

        public void UpdateNode(Node node)
        {
            int hash = node.GetHashCode();
            if (!_networkNodes.ContainsKey(hash))
                _networkNodes.Add(hash, node);
            else if (node.CompareLayer(_networkNodes[hash]) >= 0)
            {
                UnityEngine.Debug.LogError("Already exists: " + hash + ", " + node.NetworkPosition);
                _networkNodes[hash] = node;
            }
        }

        public Vector3Int GetNetworkPositionFromWorldPosition(Vector3 worldPosition)
        {
            float percentX = Mathf.Clamp01((worldPosition.x + _pathNetworkSize.x / 2) / _pathNetworkSize.x);
            float percentY = Mathf.Clamp01((worldPosition.y + _pathNetworkSize.y / 2) / _pathNetworkSize.y);
            float percentZ = Mathf.Clamp01((worldPosition.z + _pathNetworkSize.y / 2) / _pathNetworkSize.z);

            Vector3Int networkPosition = Vector3Int.zero;
            networkPosition.x = Mathf.Max(Mathf.RoundToInt((_sizeX - 1) * percentX), 0);
            networkPosition.y = Mathf.Max(Mathf.RoundToInt((_sizeY - 1) * percentY), 0);
            networkPosition.z = Mathf.Max(Mathf.RoundToInt((_sizeZ - 1) * percentZ), 0);

            UnityEngine.Debug.Log("Network Position: " + networkPosition);

            return networkPosition;
        }
        public bool TryGetNodeForWorldPosition(Vector3 worldPosition, out Node node)
        {
            Vector3Int networkPosition = GetNetworkPositionFromWorldPosition(worldPosition);
            return TryGetNodeForNodePosition(networkPosition, out node);
        }   
        public bool TryGetNodeForNodePosition(Vector3Int networkPosition, out Node node)
        {
            int hash = Node.GetHashForNetworkPosition(networkPosition);
            if (_networkNodes.ContainsKey(hash))
            {
                node = _networkNodes[hash];
                return true;
            }
            UnityEngine.Debug.LogWarning("Did not find " + networkPosition);
            node = null;
            return false;
        }
        public int GetNodeHashForWorldPosition(Vector3 worldPosition)
        {
            Vector3Int networkPosition = GetNetworkPositionFromWorldPosition(worldPosition);
            return Node.GetHashForNetworkPosition(networkPosition);
        }
        
        public Node GetClosestNode(Vector3 worldPosition)
        {
            // Does not check for nulls.

            // Find best suited region.
            Region nearestRegion = Regions[0];
            float shortestDistance = (worldPosition - Regions[0].WorldPosition).sqrMagnitude;
            int regionsLength = Regions.Count;
            for (int i = 1; i < regionsLength; i++)
            {
                float distance = (worldPosition - Regions[i].WorldPosition).sqrMagnitude;
                if (distance < shortestDistance)
                {
                    nearestRegion = Regions[i];
                    shortestDistance = distance;
                }
            }
            Vector3Int networkPosition = GetNetworkPositionFromWorldPosition(worldPosition);
            return nearestRegion.GetClosestNode(networkPosition);
            // Find best suited node.
        }

        
#if UNITY_EDITOR
        public bool ShowNetwork = false;
        public bool ShowRegions = false;
        public bool ShowNodes = false;
        private void OnDrawGizmos()
        {
            if (ShowNetwork)
                Gizmos.DrawWireCube(transform.position, _pathNetworkSize);
            foreach (var region in Regions)
            {
                if (ShowRegions)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(region.WorldPosition, _regionSize);
                }
                foreach (var node in region.Nodes)
                {
                    if (ShowNodes)
                    {
                        Gizmos.color = node.NodeGizmoColor;
                        Gizmos.DrawWireCube(node.WorldPosition, _regionSize/5);
                    }
                }
            }
        }   
#endif


    }
}