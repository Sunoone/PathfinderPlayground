using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Path2D.Pathfinding;

public class Unit : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField]
    private NodeNetworkAgent _nodeNetworkAgent;
#pragma warning restore 649
    public LayerMask AllowedTerrainMask;

    MeshRenderer _meshRenderer;
    public Transform Target;
    public float Speed = 50;
    Waypoint[] _path;
    int _targetIndex;

    private int _actionIndex;

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
            Vector3 rayPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(rayPoint, Vector3.forward);
            rayPoint.z = hit.collider.transform.position.z;
            _pathRequester.RequestPath(transform.position, rayPoint, AllowedTerrainMask, PathFound);
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

            switch (_actionIndex)
            {
                case 9:
                    Speed = 1;
                    break;
                case 10:
                    Speed = 1;
                    break;
                default:
                    Speed = 5;
                    break;
            }

            Vector3 newPosition = currentWaypoint.Position;
            if (transform.position.z > newPosition.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, currentWaypoint.Position.z);
            else
                newPosition.z = transform.position.z;
            while (transform.position != newPosition)
            {           
                transform.position = Vector3.MoveTowards(transform.position, newPosition, Speed * Time.deltaTime);
                yield return null;
            }

            _actionIndex = currentWaypoint.LayerValue;
            Debug.Log(_actionIndex);
            transform.position = currentWaypoint.Position;
        }

    }
    private void SolveClimbingCheckPoint()
    {

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
