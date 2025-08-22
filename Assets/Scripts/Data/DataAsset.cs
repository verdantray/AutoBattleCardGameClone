using UnityEngine;

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using UnityEditor;

#endif

namespace ProjectABC.Data.Editor
{
    public interface IFieldUpdatable
    {
        public void UpdateFields(List<GSTU_Cell> cells);
    }
    
    public abstract class DataAsset : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private string sheetAddress;
        [SerializeField] private DataUpdater[] dataUpdaters;

        public abstract void UpdateDataFromSheet();

        protected void UpdateData<T>(string fieldName, ICollection<T> collection) where T : IFieldUpdatable, new()
        {
            foreach (var updater in dataUpdaters)
            {
                if (updater.TryUpdateData(sheetAddress, fieldName, collection))
                {
                    OnDataUpdated(fieldName);
                    break;
                }
            }
        }

        private void OnDataUpdated(string fieldName)
        {
            Undo.RecordObject(this, $"Update {name}.{fieldName}");
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataAsset), editorForChildClasses: true)]
    public class DataAssetEditor : UnityEditor.Editor
    {
        private readonly string[] propertyNames = { "sheetAddress", "dataUpdaters" };
        private static bool _foldOut = true;
        
        private SerializedProperty[] serializedProperties = null;
        
        private void OnEnable()
        {
            serializedProperties = new SerializedProperty[propertyNames.Length];

            for (int i = 0; i < propertyNames.Length; i++)
            {
                serializedProperties[i] = serializedObject.FindProperty(propertyNames[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, propertyNames);
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 2.0f);
            DrawRemains();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRemains()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            _foldOut = EditorGUILayout.Foldout(_foldOut, "Update Data from sheets");
            if (_foldOut)
            {
                foreach (SerializedProperty property in serializedProperties)
                {
                    EditorGUILayout.PropertyField(property);
                }
            
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
            
                if (GUILayout.Button("Update data from spread sheets"))
                {
                    EditorCoroutineRunner.StartCoroutine(UpdateDataRoutine());
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private IEnumerator UpdateDataRoutine()
        {
            yield return GoogleAuthrisationHelper.CheckForRefreshOfToken();
            
            ((DataAsset)target).UpdateDataFromSheet();
        }
    }
    
#endif
}
