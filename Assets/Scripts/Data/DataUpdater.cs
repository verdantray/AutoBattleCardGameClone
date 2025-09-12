#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GoogleSheetsToUnity;

namespace ProjectABC.Data.Editor
{
    
    [Serializable]
    public class SpreadSheetReader
    {
        public string sheetName;
        public string startCell;
        public string endCell;

        public void ReadSpreadSheet(string sheetAddress, Action<GstuSpreadSheet> callback, bool mergedCells = false)
        {
            string titleColumn = Regex.Replace(startCell, "[^a-zA-Z]", "");
            int titleRow = int.Parse(Regex.Replace(startCell, "[^0-9]", ""));
            GSTU_Search search = new GSTU_Search(sheetAddress, sheetName, startCell, endCell, titleColumn, titleRow);
            // GSTU_Search search = new GSTU_Search(sheetAddress, sheetName, startCell, endCell);
            
            SpreadsheetManager.Read(search, callback.Invoke, mergedCells);
        }
    }
    
    [Serializable]
    public class DataUpdater
    {
        public string updateFieldName;
        public SpreadSheetReader sheetReader;

        public bool TryUpdateData<T>(string sheetAddress, string fieldName, ICollection<T> collection, bool clearBeforeUpdate) where T : IFieldUpdatable, new()
        {
            if (!fieldName.Equals(updateFieldName.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            
            sheetReader.ReadSpreadSheet(sheetAddress, OnGetSpreadSheet);
            return true;
            
            void OnGetSpreadSheet(GstuSpreadSheet spreadSheet)
            {
                // StringBuilder sb = new StringBuilder();
                if (clearBeforeUpdate)
                {
                    collection.Clear();
                }
                
                foreach (var rowCells in spreadSheet.rows.primaryDictionary.Values)
                {
                    if (rowCells.Exists(cell => cell.columnId == cell.value))
                    {
                        continue;
                    }
                    
                    // sb.Append($"{id}");
                    //
                    // foreach (var cell in rowCells)
                    // {
                    //     sb.Append($"/ {cell.columnId} : {cell.value}");
                    // }
                    //
                    // Debug.Log(sb);
                    // sb.Clear();
                    
                    var element = new T();
                    element.UpdateFields(rowCells);
                    
                    collection.Add(element);
                }
                
                // Debug.Log(collection.Count);
            }
        }
    }
}

#endif