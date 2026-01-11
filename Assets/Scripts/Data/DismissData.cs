using System;

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Globalization;
using GoogleSheetsToUnity;

#endif

namespace ProjectABC.Data
{
    [Serializable]
    public record DismissData : IFieldUpdatable
    {
        public int round;
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
                    case "amount":
                        int.TryParse(cell.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount);
                        break;
                }
            }
        }
#endif
    }
}
