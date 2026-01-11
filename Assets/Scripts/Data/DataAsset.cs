using UnityEngine;

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using UnityEditor;

#endif

namespace ProjectABC.Data
{
    public interface IFieldUpdatable
    {
#if UNITY_EDITOR
        public bool IsValid { get; }
        public void UpdateFields(List<GSTU_Cell> cells);
#endif
    }
    
    public interface ILocalFieldUpdatable
    {
#if UNITY_EDITOR
        public void UpdateFields(TextAsset textAsset);
#endif
    }

    public abstract class DataAsset : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private string sheetAddress;
        [SerializeField] private DataUpdater[] dataUpdaters;

        public abstract void UpdateDataFromSheet();

        protected void UpdateData<T>(string fieldName, ICollection<T> collection, bool clearBeforeUpdate = true) where T : IFieldUpdatable, new()
        {
            foreach (var updater in dataUpdaters)
            {
                updater.TryUpdateData(sheetAddress, fieldName, collection, clearBeforeUpdate);
            }
        }

        // private void OnDataUpdated(string fieldName)
        // {
        //     Undo.RecordObject(this, $"Update {name}.{fieldName}");
        // }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataAsset), editorForChildClasses: true)]
    public class DataAssetEditor : UnityEditor.Editor
    {
        private static bool _foldOut = true;
        
        protected virtual string[] PropertyNames => new[] { "sheetAddress", "dataUpdaters" };
        protected SerializedProperty[] SerializedProperties = null;
        
        private void OnEnable()
        {
            SerializedProperties = new SerializedProperty[PropertyNames.Length];

            for (int i = 0; i < PropertyNames.Length; i++)
            {
                SerializedProperties[i] = serializedObject.FindProperty(PropertyNames[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, PropertyNames);
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 2.0f);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            DrawRemains();
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawRemains()
        {
            _foldOut = EditorGUILayout.Foldout(_foldOut, "Update Data from sheets");
            if (_foldOut)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(SerializedProperties[0]);
                EditorGUILayout.PropertyField(SerializedProperties[1]);
            
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
            
                if (GUILayout.Button("Update data from spread sheets"))
                {
                    EditorCoroutineRunner.StartCoroutine(UpdateDataRoutine());
                }

                EditorGUI.indentLevel--;
            }
        }

        private IEnumerator UpdateDataRoutine()
        {
            yield return GoogleAuthrisationHelper.CheckForRefreshOfToken();
            
            ((DataAsset)target).UpdateDataFromSheet();
        }
    }
    
#endif
}
