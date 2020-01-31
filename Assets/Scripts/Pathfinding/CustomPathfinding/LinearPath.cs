using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearPath : MonoBehaviour
{
    public Transform PathPositions;
    public bool Fill; // If filled, it will use the PathNetwork settings to create nodes in between. This allows the user to stand anywhere on the linear path instead of only the manual placed nodes.
}
