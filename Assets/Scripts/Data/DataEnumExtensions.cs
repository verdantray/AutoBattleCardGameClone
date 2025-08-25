using System;

namespace ProjectABC.Data
{
    internal static class DataEnumExtensions
    {
        public static GradeType ParseGradeType(this string text)
        {
            return text switch
            {
                "1st" => GradeType.First,
                "2nd" => GradeType.Second,
                "3rd" => GradeType.Third,
                _ => throw new ArgumentException($"'{text}' is unknown grade type")
            };
        }
    }
}
