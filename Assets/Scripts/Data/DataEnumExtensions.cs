using System;

namespace ProjectABC.Data
{
    public static class DataEnumExtensions
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

        public static string GradeTypeToOrdinalString(this GradeType gradeType)
        {
            return gradeType switch
            {
                GradeType.First => "1st",
                GradeType.Second => "2nd",
                GradeType.Third => "3rd",
                _ => throw new ArgumentException($"'{gradeType}' is unknown grade type")
            };
        }

        public static string GetLocalizationKey(this GradeType gradeType)
        {
            return $"grade_{gradeType.ToString().ToLowerInvariant()}";
        }

        public static string GetLocalizationKey(this ClubType clubType)
        {
            return $"club_{clubType.ToString().ToLowerInvariant()}";
        }
    }
}
