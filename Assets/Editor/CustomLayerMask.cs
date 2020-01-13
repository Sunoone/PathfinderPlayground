using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CustomAttributes;

[CustomPropertyDrawer(typeof(CustomLayerMaskAttribute))]
public class CustomLayerMaskDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CustomLayerMaskAttribute multiLayer = attribute as CustomLayerMaskAttribute;      
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            int selectedLayer = property.intValue;         
            List<int> optionValueList = new List<int>();
            int value = property.intValue;
            selectedLayer = EditorGUI.MaskField(new Rect(position.x, position.y, position.size.x, position.size.y), property.displayName, selectedLayer, multiLayer.Names);
            property.intValue = selectedLayer;
        }
        else
            throw new System.Exception("Using the wrong attribute for a property. Cannot be drawn.");
    }
}

