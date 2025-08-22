using System;
using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsToUnity;
using ProjectABC.Data.Editor;

namespace ProjectABC.Data
{
    [Flags]
    public enum SetType
    {
        City = 2^0,
        Castle = 2^1,
        AmusementPark = 2^2,
        OuterSpace = 2^3,
        Studio = 2^4,
        Sea = 2^5,
        HauntedHouse = 2^6,
    }
    
    public enum LevelType
    {
        S,
        A,
        B,
        C,
    }
    
    [Serializable]
    public record CardData : IFieldUpdatable
    {
        public string id;
        public SetType setType;
        public LevelType levelType;
        public int basePower;
        public int amount;
        public string nameKey;
        public string descKey;
        public string imagePath;
        
        public void UpdateFields(List<GSTU_Cell> cells)
        {
            foreach (GSTU_Cell cell in cells)
            {
                switch (cell.columnId)
                {
                    case "id":
                        id = cell.value;
                        break;
                    case "set_type":
                        Enum.TryParse(cell.value, true, out setType);
                        break;
                    case "level_type":
                        Enum.TryParse(cell.value, true, out levelType);
                        break;
                    case "base_power":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out basePower);
                        break;
                    case "amount":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount);
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
    }
}
