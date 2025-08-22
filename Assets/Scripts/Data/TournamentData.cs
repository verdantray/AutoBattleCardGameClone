using System;
using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsToUnity;
using ProjectABC.Data.Editor;

namespace ProjectABC.Data
{
    [Serializable]
    public record RecruitData : IFieldUpdatable
    {
        public int round;
        public LevelType recruitLevelType;
        public int amount;
        
        public void UpdateFields(List<GSTU_Cell> cells)
        {
            foreach (var cell in cells)
            {
                switch (cell.columnId)
                {
                    case "round":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out round);
                        break;
                    case "recruit_level_type":
                        Enum.TryParse(cell.value, true, out recruitLevelType);
                        break;
                    case "amount":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount);
                        break;
                }
            }
        }
    }
    
    [Serializable]
    public record WinPointData : IFieldUpdatable
    {
        public int round;
        public int winPoint;
        public int weight;
        
        public void UpdateFields(List<GSTU_Cell> cells)
        {
            foreach (var cell in cells)
            {
                switch (cell.columnId)
                {
                    case "round":
                        ParseIntValue(cell.value, out round);
                        break;
                    case "win_point":
                        ParseIntValue(cell.value, out winPoint);
                        break;
                    case "weight":
                        ParseIntValue(cell.value, out weight);
                        break;
                }
            }
        }

        private bool ParseIntValue(string toParse, out int parsed)
        {
            return int.TryParse(toParse, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);
        }
    }
}
