using System;

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
        Council = 1 << 1,
        Coastline = 1 << 2,
        Band = 1 << 3,
        GameDevelopment = 1 << 4,
        HauteCuisine = 1 << 5,
        Unregistered = 1 << 6,
        TraditionExperience = 1 << 7,
        Examiners = 1 << 8,
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
        public string cardEffectId;
        public string imagePath;

#if UNITY_EDITOR
        public bool IsValid => !string.IsNullOrEmpty(id);
        
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
                    case "card_effect_id":
                        cardEffectId = cell.value;
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
