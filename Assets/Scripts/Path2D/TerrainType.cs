using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

namespace Path2d
{
    [System.Serializable]
    public class TerrainType
    {
        [SingleLayer()]
        public LayerMask TerrainMask;
        public int TerrainPenalty;
    }
}