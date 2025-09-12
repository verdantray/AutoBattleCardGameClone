#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectABC.Data.Editor
{
    public interface ILocalFieldUpdatable
    {
#if UNITY_EDITOR
        public void UpdateFields(TextAsset textAsset);
#endif
    }
    
    [Serializable]
    public class LocalDataUpdater
    {
        public string updateFieldName;
        public TextAsset[] textAssets;

        public bool TryUpdateData<T>(string fieldName, ICollection<T> collection) where T : ILocalFieldUpdatable, new()
        {
            if (!fieldName.Equals(updateFieldName.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            foreach (var textAsset in textAssets)
            {
                var element = new T();
                element.UpdateFields(textAsset);
                
                collection.Add(element);
            }

            return true;
        }
    }

    [CustomPropertyDrawer(typeof(LocalDataUpdater))]
    public class DataAssetPathPairDrawer : PropertyDrawer
    {
        private readonly float _spacing = EditorGUIUtility.standardVerticalSpacing;
        private readonly float _lineHeight = EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = _lineHeight;

            if (property.isExpanded)
            {
                SerializedProperty updateFieldProp = property.FindPropertyRelative("updateFieldName");
                SerializedProperty textAssetsProp = property.FindPropertyRelative("textAssets");

                height += _spacing + EditorGUI.GetPropertyHeight(updateFieldProp, true);
                height += _spacing + EditorGUI.GetPropertyHeight(textAssetsProp, true);
                height += _spacing + _lineHeight;   // for button height
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.IndentedRect(position);

            Rect line = new Rect(position.x, position.y, position.width, _lineHeight);
            property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);
            line.y += _lineHeight + _spacing;

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            
            SerializedProperty updateFieldProp = property.FindPropertyRelative("updateFieldName");
            SerializedProperty textAssetsProp = property.FindPropertyRelative("textAssets");

            float updateFieldPropHeight = EditorGUI.GetPropertyHeight(updateFieldProp, true);
            Rect updateFieldPropRect = new Rect(line.x, line.y, line.width, updateFieldPropHeight);
            EditorGUI.PropertyField(updateFieldPropRect, updateFieldProp, true);
            line.y += updateFieldPropHeight + _spacing;
            
            float textAssetsPropHeight = EditorGUI.GetPropertyHeight(textAssetsProp, true);
            Rect textAssetsPropRect = new Rect(line.x, line.y, line.width, textAssetsPropHeight);
            EditorGUI.PropertyField(textAssetsPropRect, textAssetsProp, true);
            line.y += textAssetsPropHeight + _spacing;
            
            Rect buttonRect = new  Rect(line.x, line.y, line.width, _lineHeight);
            if (GUI.Button(buttonRect, "Open parent folder of assets"))
            {
                OpenParentFolderForAddAssets(textAssetsProp);
            }
            
            EditorGUI.indentLevel--;
        }

        private void OpenParentFolderForAddAssets(SerializedProperty property)
        {
            string parentPath = EditorUtility.OpenFolderPanel("Select parent folder", Application.dataPath, "");
            DirectoryInfo parentDirectory = new DirectoryInfo(parentPath);

            string relativeParentPath = Path.GetRelativePath(Application.dataPath, parentPath);
            
            var relativePaths = parentDirectory.GetFiles()
                .Where(fileInfo => fileInfo.Extension != ".meta")
                .Select(fileInfo => Path.Combine("Assets", relativeParentPath, fileInfo.Name));

            foreach (var path in relativePaths)
            {
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (!asset)
                {
                    Debug.LogWarning($"{path} is not text asset.");
                    continue;
                }
                
                Undo.RecordObjects(property.serializedObject.targetObjects, "Add textAssets elements");

                property.serializedObject.Update();
                
                property.arraySize++;
                SerializedProperty newElement = property.GetArrayElementAtIndex(property.arraySize - 1);
                newElement.objectReferenceValue = asset;
                
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

#endif