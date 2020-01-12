using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Algorithms;
using System.Diagnostics;

namespace Pathfinding
{
    // Sets up the network for all paths using nodes and regions.
    public class PathingNetwork : MonoBehaviour
    {
        public int RandomRange = 10;
        [SerializeField]
        public Vector3 _pathingNetworkSize;
        [SerializeField]
        public Vector3 _regionSize;
        [SerializeField]
        public Vector3 _gridSpacing;
        [SerializeField]
        public LayerMask _collisionMask;

        public List<Region> Regions { get; private set; }

        private void Start()
        {
            InitRegions();
        }

        private void InitRegions()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

#if UNITY_EDITOR
            if (_pathingNetworkSize.x <= 0 || _pathingNetworkSize.y <= 0)
                throw new System.Exception("PathingNetworkSize too small.");
#endif

            Regions = new List<Region>();
            Vector3 halfPathingNetworkSize = _pathingNetworkSize / 2;
            Vector3 halfRegionSize = _regionSize / 2;

            Vector3 startPosition = transform.position - halfPathingNetworkSize + halfRegionSize;
            Vector3 endPosition = transform.position + halfPathingNetworkSize - halfRegionSize;
            for (float x = startPosition.x; x  <= endPosition.x; x += _regionSize.x)
            {
                for (float y = startPosition.y; y <= endPosition.y; y += _regionSize.y)
                {
                    Region region = new Region(new Vector3(x, y, transform.position.z), _regionSize, _gridSpacing, _collisionMask);
                    Regions.Add(region);
                }
            }

            stopWatch.Stop();
            UnityEngine.Debug.Log("Created regions: " + stopWatch.ElapsedMilliseconds + " ms.");
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
        }

        public void RebuildRandomRegion()
        {
            int randomIndex = Random.Range(0, Regions.Count - 1);
            Regions[randomIndex].UpdateRegion();
        }

        public int PositionToHash(Vector3 position)
        {
            return -1;
        }



        public Node GetClosestNode(Vector3 position)
        {
            // Does not check for nulls.

            // Find best suited region.
            Region nearestRegion = Regions[0];
            float shortestDistance = (position - Regions[0].WorldPosition).sqrMagnitude;
            int regionsLength = Regions.Count;
            for (int i = 1; i < regionsLength; i++)
            {
                float distance = (position - Regions[i].WorldPosition).sqrMagnitude;
                if (distance < shortestDistance)
                {
                    nearestRegion = Regions[i];
                    shortestDistance = distance;
                }
            }
            return nearestRegion.GetClosestNode(position);
            // Find best suited node.
        }


#if UNITY_EDITOR
        public bool ShowNetwork = false;
        public bool ShowRegions = false;
        public bool ShowNodes = false;
        private void OnDrawGizmos()
        {
            if (ShowNetwork)
                Gizmos.DrawWireCube(transform.position, _pathingNetworkSize);
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
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireCube(node.WorldPosition, _gridSpacing);
                    }
                }
            }
        }

       
#endif


    }
}