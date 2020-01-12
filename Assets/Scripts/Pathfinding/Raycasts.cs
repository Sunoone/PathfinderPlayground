using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public static class Raycasts
    {
        // Shoot rays from the top down, get all colliders in between. Then use those positions for the "grid" for notes.

        /*public RaycastHit2D[] GetColliders(Vector3 position, Vector3 direction, float length, LayerMask ignoreMask)
        {
            Ray ray = new Ray(position, direction);
            var hits = Physics2D.RaycastAll(position, direction, length, ignoreMask);
            return hits;                      
        }*/
    }
}