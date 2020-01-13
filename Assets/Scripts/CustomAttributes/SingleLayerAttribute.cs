using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAttributes
{
    public class SingleLayerAttribute : PropertyAttribute
    {
        public string Filter = "";
        public int Index = 0;
        public SingleLayerAttribute(string filter, int index) { Filter = filter; }
        public SingleLayerAttribute(string filter) { Filter = filter; }
        public SingleLayerAttribute(int index) { Index = index; }
        public SingleLayerAttribute() { }
    }
}
