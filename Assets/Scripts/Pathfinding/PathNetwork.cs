using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Algorithms;
using System.Diagnostics;
using Pathfinding.Collision;
using CustomAttributes;

namespace Pathfinding
{
    public class Dictionary<K1, K2, T> : Dictionary<K1, Dictionary<K2, T>> { }

    // Sets up the network for all paths using nodes and regions.
    public class PathNetwork : MonoBehaviour
    {
        
        // This layer is setup manually in every project. One this layer value has been decided, do not modify it.
        public const int CustomLayer = 12; // This layer check bypasses GetComponent null value checks.
        public const int UnwalkableLayer = 8;

        public PathAgent Character;
        public PathCollider[] Colliders { get; private set; }
        //@TODO: Store the data
        // - Diamond shapes
        private Dictionary<int, Node> _networkNodes = new Dictionary<int, Node>();
        private Dictionary<Vector3, Vector3, Node> _borders = new Dictionary<Vector3, Vector3, Node>();

#pragma warning disable 649
        [SerializeField]
        private Vector3 _pathNetworkSize;
        [SerializeField]
        private Vector3 _regionSize;

        [SerializeField]
        private LayerMask _collisionMask;
#pragma warning restore 649

        public List<Region> Regions { get; private set; }
        private const string _defaultName = "default";
        public string NetworkName = _defaultName;
        private int _sizeX, _sizeY, _sizeZ;
        private void Start()
        {
            if (NetworkName == _defaultName)
                throw new System.Exception("NetworkName cannot be equal to " + _defaultName + ".");

            Colliders = FindObjectsOfType<PathCollider>();


            CreateNodeNetwork(Character);
        }

        private float _gridSpacing = 0;
        private void CreateNodeNetwork(PathAgent pathAgent)
        {
            _gridSpacing = pathAgent.Radius;
            if (_gridSpacing <= 0)
                throw new System.Exception("pathAgent.Radius <= 0.");
            if (_regionSize.x < _gridSpacing ||
                _regionSize.y < _gridSpacing ||
                _regionSize.z < _gridSpacing)
                throw new System.Exception("One or more values in _regionSize is smaller than _gridSpacing.");

            _sizeX = Mathf.Max(Mathf.RoundToInt(_pathNetworkSize.x / _gridSpacing), 1);
            _sizeY = Mathf.Max(Mathf.RoundToInt(_pathNetworkSize.y / _gridSpacing), 1);
            _sizeZ = Mathf.Max(Mathf.RoundToInt(_pathNetworkSize.z / _gridSpacing), 1);

            // First raytrace from character height. See where regions need to be made.
            // After regions are made, they are now self sustained.
            // However, with dynamics involved, new regions might have to be created.
            // usually, all 2d region should've been solved. This shouldn't hinder any flying units, etc. Therefore it should be ok.

            // Can use sphere detection for the regions.
            // Easiest way to see if there is anything within there. Could even be square.
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            if (_pathNetworkSize.x <= 0 || _pathNetworkSize.y <= 0)
                throw new System.Exception("PathingNetworkSize too small.");

            Regions = new List<Region>();
            Vector3 halfPathingNetworkSize = _pathNetworkSize / 2;
            Vector3 halfRegionSize = _regionSize / 2;
            
            Vector3 startPosition = transform.position - halfPathingNetworkSize + halfRegionSize;    
            Vector3 endPosition = transform.position + halfPathingNetworkSize - halfRegionSize;

            Vector3 regionOrigin = startPosition;
            Vector3 boxSize = _regionSize - new Vector3(_gridSpacing, _gridSpacing, _gridSpacing);

            // Regionsize / spacing == even works. Uneven doesn't.
            for (; regionOrigin.x <= endPosition.x; regionOrigin.x += _regionSize.x)
            {
                regionOrigin.y = startPosition.y;
                for (; regionOrigin.y <= endPosition.y; regionOrigin.y += _regionSize.y)
                {
                    regionOrigin.z = startPosition.z;
                    for (; regionOrigin.z <= endPosition.z; regionOrigin.z += _regionSize.y)
                    {
                        Collider2D[] colliders = Physics2D.OverlapBoxAll(regionOrigin, boxSize, 0, _collisionMask);
                        if (colliders.Length == 0)
                            continue;

                        Region region = new Region(this, regionOrigin, _regionSize, _gridSpacing, pathAgent);
                        Regions.Add(region);
                    }
                    

                }
            }
            stopWatch.Stop();
            UnityEngine.Debug.Log("Created regions: " + stopWatch.ElapsedMilliseconds + " ms.");



            foreach (var region in Regions)
            {
                // Connect right and below. > continue to other region.
                //region.SetNeighboursForNodes();
            }
        }

        private void Save(PathAgent pathAgent)
        {
            // Store all the nodes.
        }

        private void Load(PathAgent pathAgent)
        {
            // Load the correct file using the parameter and recreating the entire pathnetwork.
            // Load all the nodes.
        }

        private void InitRegions()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            if (_pathNetworkSize.x <= 0 || _pathNetworkSize.y <= 0)
                throw new System.Exception("PathingNetworkSize too small.");

            Regions = new List<Region>();
            Vector3 halfPathingNetworkSize = _pathNetworkSize / 2;
            Vector3 halfRegionSize = _regionSize / 2;

            Vector3 startPosition = transform.position - halfPathingNetworkSize + halfRegionSize;
            Vector3 endPosition = transform.position + halfPathingNetworkSize - halfRegionSize;
            for (float x = startPosition.x; x  <= endPosition.x; x += _regionSize.x)
            {
                for (float y = startPosition.y; y <= endPosition.y; y += _regionSize.y)
                {
                    Region region = new Region(this, new Vector3(x, y, transform.position.z), _regionSize, _gridSpacing, null);
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

        private bool TryGetPathColliderOverlap(PathCollider pathCollider, out Vector3[] overlapExtremes)
        {
            Vector3[] colliderExtremes = pathCollider.Extremes;
            Vector3[] pathNetworkExtremes = GetExtremes(transform.position, _pathNetworkSize);

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

        private void SetNeighbours()
        {

        }
        private int RandomRange = 10;
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
                        _networkNodes[neighbour].NodeGizmoColor = Color.cyan;
                        count++;
                    }
                    UnityEngine.Debug.Log("Clicked: " + count);
                }
            }
        }

        public void RebuildRandomRegion()
        {
            int randomIndex = Random.Range(0, Regions.Count - 1);
            //Regions[randomIndex].UpdateRegion();
        }

        public void UpdateNode(Node node)
        {
            int hash = node.GetHashCode();
            if (!_networkNodes.ContainsKey(hash))
                _networkNodes.Add(hash, node);
            else if (node.CompareLayer(_networkNodes[hash]) >= 0)
            {
                //UnityEngine.Debug.LogError("Already exists: " + hash + ", " + node.NetworkPosition);
                _networkNodes[hash] = node;
            }
        }

        public Node CreateNode(int layer, Vector3Int networkPosition, Vector3 worldPosition, Vector3 nodeUp)
        {
            int penalty = GetPenalty(layer);
            Node node = new Node(layer, networkPosition, worldPosition, nodeUp, penalty); // WROOONG
            UpdateNode(node);
            return node;
        }
        private int GetPenalty(int layer)
        {
            return 0;
        }
        

        public Vector3Int GetNetworkPositionFromWorldPosition(Vector3 worldPosition)
        {
            float percentX = Mathf.Clamp01((worldPosition.x + _pathNetworkSize.x / 2) / _pathNetworkSize.x);
            float percentY = Mathf.Clamp01((worldPosition.y + _pathNetworkSize.y / 2) / _pathNetworkSize.y);
            float percentZ = Mathf.Clamp01((worldPosition.z + _pathNetworkSize.z / 2) / _pathNetworkSize.z);

            Vector3Int networkPosition = Vector3Int.zero;
            networkPosition.x = Mathf.Max(Mathf.RoundToInt((_sizeX - 1) * percentX), 0);
            networkPosition.y = Mathf.Max(Mathf.RoundToInt((_sizeY - 1) * percentY), 0);
            networkPosition.z = Mathf.Max(Mathf.RoundToInt((_sizeZ - 1) * percentZ), 0);

            //UnityEngine.Debug.Log("Network Position: " + networkPosition);

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

        public Vector3[] GetExtremes(Vector3 position, Vector3 size)
        {
            // Take rotation into account.
            // Extremes do not work for rotations...
            Vector3[] extremes = new Vector3[2];
            Vector3 offsetA = new Vector3(size.x / 2, size.y / 2, 0);
            Vector3 offsetB = new Vector3(size.x / 2, size.y / 2, size.z);
            extremes[0] = position - offsetA;
            extremes[1] = position + offsetB;
            return extremes;
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
                        Gizmos.DrawSphere(node.WorldPosition, _gridSpacing / 3);
                    }
                }
            }
        }   
#endif
    }
}

