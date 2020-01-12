using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Grid2d.Pathfinding
{
    public class PathRequester : MonoBehaviour
    {
        private Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
        private PathRequest _currentPathRequest;
        private Pathfinder _pathfinder;
        private bool _isProcessingPath;

        private void OnEnable()
        {
            _pathfinder = GetComponent<Pathfinder>();
        }

        public void RequestPath(Vector3 pathStart, Vector3 pathEnd, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback)
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, allowedTerrain, callback);
            _pathRequestQueue.Enqueue(newRequest);
            TryProcessNext();
        }

        private void TryProcessNext()
        {
            if (!_isProcessingPath && _pathRequestQueue.Count > 0)
            {
                _currentPathRequest = _pathRequestQueue.Dequeue();
                _isProcessingPath = true;

                _pathfinder.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd, _currentPathRequest.AllowedTerrain, FinishedProcessingPath);
            }
        }

        public void FinishedProcessingPath(Waypoint[] path, PathStatus pathStatus)
        {
            _currentPathRequest.Callback(path, pathStatus);
            _isProcessingPath = false;
            TryProcessNext();
        }

        private struct PathRequest
        {
            public Vector3 PathStart;
            public Vector3 PathEnd;
            public LayerMask AllowedTerrain;
            public Action<Waypoint[], PathStatus> Callback;

            public PathRequest(Vector3 pathStart, Vector3 pathEnd, LayerMask allowedTerrain, Action<Waypoint[], PathStatus> callback)
            {
                PathStart = pathStart;
                PathEnd = pathEnd;
                AllowedTerrain = allowedTerrain;
                Callback = callback;
            }
        }
    }
}
