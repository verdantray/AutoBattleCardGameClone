using System;
using ProjectABC.Data.Editor;

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsToUnity;

#endif

namespace ProjectABC.Data
{
    [Flags]
    public enum ClubType
    {
        Council = 2^0,
        Coastline = 2^1,
        Band = 2^2,
        GameDevelopment = 2^3,
        HauteCuisine = 2^4,
        Unregistered = 2^5,
        TraditionExperience = 2^6,
    }
    
    public enum GradeType
    {
        First,
        Second,
        Third,
    }
    
    [Serializable]
    public record CardData : IFieldUpdatable
    {
        public string id;
        public ClubType clubType;
        public GradeType gradeType;
        public int basePower;
        public int amount;
        public string titleKey;
        public string nameKey;
        public string descKey;
        public string imagePath;

#if UNITY_EDITOR
        public void UpdateFields(List<GSTU_Cell> cells)
        {
            foreach (GSTU_Cell cell in cells)
            {
                switch (cell.columnId)
                {
                    case "id":
                        id = cell.value;
                        break;
                    case "club_type":
                        Enum.TryParse(cell.value, true, out clubType);
                        break;
                    case "grade_type":
                        gradeType = cell.value.ParseGradeType();
                        break;
                    case "base_power":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out basePower);
                        break;
                    case "amount":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount);
                        break;
                    case "title_key":
                        titleKey = cell.value;
                        break;
                    case "name_key":
                        nameKey = cell.value;
                        break;
                    case "desc_key":
                        descKey = cell.value;
                        break;
                    case "image_path":
                        imagePath = cell.value;
                        break;
                }
            }
        }
#endif
    }
}
