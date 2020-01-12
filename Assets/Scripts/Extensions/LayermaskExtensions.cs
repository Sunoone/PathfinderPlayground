using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayermaskExtensions
{
    public static bool ContainsLayer(this LayerMask layermask, int layer)
    {
        return layermask == (layermask | (1 << layer));
    }
}
