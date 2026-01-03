using System;
using ProjectABC.Data.Editor;

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsToUnity;

#endif

namespace ProjectABC.Data
{
    [Serializable]
    public record RecruitData : IFieldUpdatable
    {
        public int round;
        public GradeType recruitGradeType;
        public int amount;
        
#if UNITY_EDITOR
        public bool IsValid => round > 0 && amount > 0;
        
        public void UpdateFields(List<GSTU_Cell> cells)
        {
            foreach (var cell in cells)
            {
                switch (cell.columnId)
                {
                    case "round":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out round);
                        break;
                    case "recruit_grade_type":
                        recruitGradeType = cell.value.ParseGradeType();
                        break;
                    case "amount":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount);
                        break;
                }
            }
        }
        
#endif
    }
    
    [Serializable]
    public record WinPointData : IFieldUpdatable
    {
        public int round;
        public int winPoint;
        public int weight;
        
#if UNITY_EDITOR
        public bool IsValid => round > 0 && winPoint > 0;
        
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
#endif
    }
}
