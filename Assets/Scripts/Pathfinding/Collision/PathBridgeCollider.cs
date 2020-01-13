using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
namespace Pathfinding.Collision
{
    public class PathBridgeCollider : PathBoxCollider
    {
        [SerializeField]
        [CustomLayerMask("Left", "Right", "Top", "Bottom")]
        private int _connectionSides;
    }
}

