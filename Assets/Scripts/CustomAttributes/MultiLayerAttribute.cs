using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAttributes
{
    public class MultiLayerAttribute : PropertyAttribute
    {
        public int Index = 0;
        public MultiLayerAttribute(int index) { Index = index; }
        public MultiLayerAttribute() { }
    }
}
