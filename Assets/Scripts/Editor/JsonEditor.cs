using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectABC.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectABC.Editor
{
    public class JsonEditor : EditorWindow
    {
        private JsonValue _root = new JsonValue(JsonType.Object);
        private Vector2 _scroll;
        
        [MenuItem("ABC Utility/Json Editor")]
        private static void Open()
        {
            GetWindow<JsonEditor>(true, "Json Editor");
        }

        private void OnGUI()
        {
            DrawToolBar();
            
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUI.indentLevel = 0;
            DrawRootSelector();
            
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            DrawValue("JSON Root", _root);
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New"))
            {
                _root = new JsonValue(JsonType.Object);
            }

            if (GUILayout.Button("Import Json..."))
            {
                ImportFromFile();
            }

            if (GUILayout.Button("Export Json..."))
            {
                ExportToFile();
            }

            if (GUILayout.Button("Copy Json..."))
            {
                CopyJsonToClipboard();
            }

            if (GUILayout.Button("Paste & Import"))
            {
                PasteAndImport();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRootSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Root Type");
            
            var newType = (JsonType)EditorGUILayout.EnumPopup(_root.type);
            if (newType != _root.type)
            {
                ChangeType(_root, newType);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        #region Recursive Drawers

        private void DrawValue(string label, JsonValue value)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(label);
                    
                    GUILayout.FlexibleSpace();
                    
                    var newType = (JsonType)EditorGUILayout.EnumPopup(value.type);
                    if (newType != value.type)
                    {
                        ChangeType(value, newType);
                    }
                }
                
                EditorGUI.indentLevel++;
                switch (value.type)
                {
                    case JsonType.String:
                        value.strValue = EditorGUILayout.TextField("Value", value.strValue ?? string.Empty);
                        break;
                    case JsonType.Integer:
                        value.intValue = EditorGUILayout.IntField("Value", value.intValue);
                        break;
                    case JsonType.Float:
                        value.floatValue = EditorGUILayout.FloatField("Value", value.floatValue);
                        break;
                    case JsonType.Bool:
                        value.boolValue = EditorGUILayout.Toggle("Value", value.boolValue);
                        break;
                    case JsonType.Object:
                        DrawObject(value.obj);
                        break;
                    case JsonType.Array:
                        DrawArray(value.arr);
                        break;
                    default:
                    case JsonType.Null:
                        EditorGUILayout.LabelField("null");
                        break;
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawObject(JsonObject obj)
        {
            obj ??= new JsonObject();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add Field", GUILayout.Width(110)))
                {
                    obj.fields.Add(new JsonField());
                }
                
                GUILayout.FlexibleSpace();
            }
            
            for (int i = 0; i < obj.fields.Count; i++)
            {
                var field = obj.fields[i];

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        field.key = EditorGUILayout.TextField(field.key ?? string.Empty);
                        
                        if (GUILayout.Button("▲")) Move(obj.fields, i, i-1);
                        if (GUILayout.Button("▼")) Move(obj.fields, i, i+1);
                        if (GUILayout.Button("⎘")) obj.fields.Insert(i+1, CloneField(field));
                        if (GUILayout.Button("✕")) { obj.fields.RemoveAt(i); break; }
                    }
                    
                    DrawValue(null, field.value);
                }
            }
            
            var duplicates = GetDuplicateKeys(obj);
            if (duplicates.Count > 0)
            {
                EditorGUILayout.HelpBox("Duplicate keys: " + string.Join(", ", duplicates), MessageType.Warning);
            }
        }

        private void DrawArray(List<JsonValue> arr)
        {
            if (arr == null) arr = new List<JsonValue>();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add Element"))
                {
                    arr.Add(new JsonValue(JsonType.String) { strValue = string.Empty });
                }
                
                GUILayout.FlexibleSpace();
            }


            for (int i = 0; i < arr.Count; i++)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"[{i}]");
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("▲")) Move(arr, i, i - 1);
                        if (GUILayout.Button("▼")) Move(arr, i, i + 1);
                        if (GUILayout.Button("⎘")) arr.Insert(i + 1, CloneValue(arr[i]));
                        if (GUILayout.Button("✕")) { arr.RemoveAt(i); break; }
                    }

                    DrawValue(null, arr[i]);
                }
            }
        }

        #endregion

        #region Helper Functions

        private void ChangeType(JsonValue value, JsonType newType)
        {
            value.SetType(newType);
            switch (newType)
            {
                case JsonType.String: value.strValue ??= string.Empty; break;
                case JsonType.Integer: value.intValue = 0; break;
                case JsonType.Float: value.floatValue = 0.0f; break;
                case JsonType.Bool: value.boolValue = false; break;
                case JsonType.Object: value.obj ??= new JsonObject(); break;
                case JsonType.Array: value.arr ??= new List<JsonValue>(); break;
                default:
                case JsonType.Null: break;
            }
        }

        private static void Move<T>(List<T> list, int from, int to)
        {
            if (from < 0 || from >= list.Count) return;
            if (to < 0 || to >= list.Count) return;
            
            (list[from], list[to]) = (list[to], list[from]);
        }

        private static JsonField CloneField(JsonField field)
        {
            return new JsonField { key = field.key, value = CloneValue(field.value) };
        }

        private static JsonValue CloneValue(JsonValue value)
        {
            var newValue = new JsonValue
            {
                type = value.type,
                strValue = value.strValue,
                intValue = value.intValue,
                floatValue = value.floatValue,
                boolValue = value.boolValue,
            };

            if (value.type == JsonType.Object)
            {
                newValue.obj = new JsonObject { fields = new List<JsonField>() };
                foreach (var jsonField in value.obj.fields)
                {
                    newValue.obj.fields.Add(CloneField(jsonField));
                }
            }
            else if (value.type == JsonType.Array)
            {
                newValue.arr = new List<JsonValue>();
                foreach (var jsonValue in value.arr)
                {
                    newValue.arr.Add(CloneValue(jsonValue));
                }
            }

            return newValue;
        }

        private static List<string> GetDuplicateKeys(JsonObject obj)
        {
            var map = new Dictionary<string, int>();
            foreach (var field in obj.fields)
            {
                var key = field.key ?? string.Empty;
                map[key] = map.TryGetValue(key, out int count) ? count + 1 : 1;
            }

            var duplicates = new List<string>();

            foreach (var (key, count) in map)
            {
                if (count > 1 && !string.IsNullOrEmpty(key))
                {
                    duplicates.Add(key);
                }
            }

            return duplicates;
        }

        #endregion

        #region Import / Export

        private void ExportToFile()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Json",
                Application.dataPath,
                "data",
                "json"
            );

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                string json = _root.WriteJson(true);
                File.WriteAllText(path, json, new UTF8Encoding(false));
                
                AssetDatabase.Refresh();
                
                Debug.Log($"{nameof(JsonEditor)} : Exported Json to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(JsonEditor)} : Export failed... {e}");
            }
        }

        private void CopyJsonToClipboard()
        {
            try
            {
                EditorGUIUtility.systemCopyBuffer = _root.WriteJson(true);
                ShowNotification(new GUIContent("Json copied!"));
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(JsonEditor)} : Copy failed.. {e}");
            }
        }

        private void PasteAndImport()
        {
            try
            {
                string clipboard = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrWhiteSpace(clipboard))
                {
                    ShowNotification(new GUIContent("Clipboard is empty"));
                    return;
                }

                _root = clipboard.ParseToJsonValue();
                Repaint();
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(JsonEditor)} : Import failed.. {e}");
            }
        }

        private void ImportFromFile()
        {
            string path = EditorUtility.OpenFilePanel("Import Json",  Application.dataPath, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                string text = File.ReadAllText(path);
                _root = text.ParseToJsonValue();
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(JsonEditor)} : Import Failed.. {e}");
            }
        }

        #endregion
    }
}
