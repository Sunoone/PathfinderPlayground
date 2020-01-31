using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class PathAgent : MonoBehaviour
    {
        private const string _defaultName = "default";
        public string AgentName = _defaultName;
        public float Height = 1;
        public float Radius = 1;
    }
}