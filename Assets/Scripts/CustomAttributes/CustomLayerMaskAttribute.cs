using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAttributes
{
    public class CustomLayerMaskAttribute : PropertyAttribute
    {
        public string[] Names;
        public CustomLayerMaskAttribute(params string[] names) { Names = names; }
    }
}
