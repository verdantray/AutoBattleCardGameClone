using ProjectABC.Engine;
using UnityEditor;
using UnityEngine;

namespace ProjectABC.Editor
{
    [CustomPropertyDrawer(typeof(ScaledTime))]
    public sealed class ScaledTimeDrawer : PropertyDrawer
    {
        private readonly string _timePropName = "time";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty timeProp = property.FindPropertyRelative(_timePropName);
            
            EditorGUI.BeginProperty(position, label, property);
            timeProp.floatValue = EditorGUI.FloatField(position, label, timeProp.floatValue);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
