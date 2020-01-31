using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
namespace Pathfinding.Collision
{
    public class PathBridgeCollider : PathBoxCollider
    {
        //[SerializeField]
        //[Tooltip("Allows the nodes to be decide their own positions, rather than relying on the PathNetwork to solve the positions.")]
        //private bool _desyncGrid = true;


        // Creates the inner nodes on this collider. Uses the same gridspacing
        private void CreateInnerNetwork()
        {
            // Using the extremes and pathnetwork (spacing) to create all the internal nodes.
        }
        // Connects the inner network the the other network by casting around the edges and connecting compatible nodes.
        private void RaycastEdge()
        {

            // Will raycast on all the positions with spacing outside the edges.  
        }
    }
}

