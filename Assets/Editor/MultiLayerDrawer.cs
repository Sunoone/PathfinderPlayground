using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MultiLayerAttribute))]
public class MultiLayerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MultiLayerAttribute multiLayer = attribute as MultiLayerAttribute;
        
        if (property.propertyType == SerializedPropertyType.LayerMask)
        {
            int selectedLayer = property.intValue >> multiLayer.Index;
            List<string> nameList = new List<string>();
            List<int> optionValueList = new List<int>();
            int value = property.intValue;
            for (int i = multiLayer.Index; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);  
                if (layerName != "")
                    nameList.Add(layerName);
            }
            var names = nameList.ToArray();
            selectedLayer = EditorGUI.MaskField(new Rect(position.x, position.y, position.size.x, position.size.y), property.displayName, selectedLayer, names);     
            property.intValue = selectedLayer << multiLayer.Index;
        }
        else
            throw new System.Exception("Using the wrong attribute for a property. Cannot be drawn.");
    }
}
