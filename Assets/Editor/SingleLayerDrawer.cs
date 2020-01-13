using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomAttributes;

[CustomPropertyDrawer(typeof(SingleLayerAttribute))]
public class SingleLayerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SingleLayerAttribute singleLayer = attribute as SingleLayerAttribute;
        
        if (property.propertyType == SerializedPropertyType.LayerMask)
        {
            int selectedLayer = property.intValue;
            List<string> nameList = new List<string>();
            List<int> optionValueList = new List<int>();
            int value = property.intValue;
            for (int i = singleLayer.Index; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "" && (singleLayer.Filter == "" || layerName.Contains(singleLayer.Filter)))
                {
                    nameList.Add(layerName);
                    optionValueList.Add(i);
                }
            }
            var names = nameList.ToArray();
            var optionValues = optionValueList.ToArray();
            selectedLayer = EditorGUI.IntPopup(new Rect(position.x, position.y, position.size.x, position.size.y), property.displayName, selectedLayer, names, optionValues);
            property.intValue = selectedLayer;
        }
        else
            throw new System.Exception("Using the wrong attribute for a property. Cannot be drawn.");
    }
}
