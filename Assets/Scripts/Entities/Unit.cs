using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid2d.Pathfinding;

public class Unit : MonoBehaviour
{
    public LayerMask WalkableTerrain;

    MeshRenderer _meshRenderer;
    public Transform Target;
    public float Speed = 50;
    Waypoint[] _path;
    int _targetIndex;

    [SerializeField]
    private PathRequester _pathRequester;
    private IEnumerator _pathCoroutine;

    private Color _color;

    private void Start()
    {
        if (_pathRequester == null)
            _pathRequester = FindObjectOfType<PathRequester>();

        _meshRenderer = GetComponent<MeshRenderer>();
        _color = _meshRenderer.material.color;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Needs to accurately get the raytrace depth.
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPosition.z = transform.position.z;
            _pathRequester.RequestPath(transform.position, newPosition, WalkableTerrain, PathFound);
        }
    }

    private void PathFound(Waypoint[] path, PathStatus pathStatus)
    {
        if (pathStatus != PathStatus.Fail)
        {
            if (_pathCoroutine != null)
                StopCoroutine(_pathCoroutine);

            _path = path;
            _pathCoroutine = FollowPath();
            StartCoroutine(_pathCoroutine);
        }
    }

    private IEnumerator FollowPath()
    {
        int length = _path.Length;
        for (_targetIndex = 0; _targetIndex < length; _targetIndex++)
        {
            Waypoint currentWaypoint = _path[_targetIndex];
            while (transform.position != currentWaypoint.Position)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.Position, Speed * Time.deltaTime);
                yield return null;
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (_path != null)
            for (int i = _targetIndex; i < _path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(_path[i].Position, Vector3.one/10);
                if (i == _targetIndex)
                    Gizmos.DrawLine(transform.position, _path[i].Position);
                else
                    Gizmos.DrawLine(_path[i - 1].Position, _path[i].Position);
            }
    }
}
