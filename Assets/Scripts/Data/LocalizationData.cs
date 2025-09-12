using System;
using System.Collections.Generic;
using ProjectABC.Data.Editor;

#if UNITY_EDITOR

using GoogleSheetsToUnity;

#endif

namespace ProjectABC.Data
{
    public enum LocaleType
    {
        Ko,
        En,
        Ja,
    }
    
    [Serializable]
    public record LocalizationData : IFieldUpdatable
    {
        public string key;
        public string ko;
        public string en;
        public string ja;

        public string this[LocaleType type] => type switch
        {
            LocaleType.Ko => ko,
            LocaleType.En => en,
            LocaleType.Ja => ja,
            _ => en
        };

#if UNITY_EDITOR
        public void UpdateFields(List<GSTU_Cell> cells)
        {
            foreach (var cell in cells)
            {
                switch (cell.columnId)
                {
                    case "key":
                        key = cell.value;
                        break;
                    case "ko":
                        ko = cell.value;
                        break;
                    case "en":
                        en = cell.value;
                        break;
                    case "ja":
                        ja = cell.value;
                        break;
                }
            }
        }
#endif
    }
}