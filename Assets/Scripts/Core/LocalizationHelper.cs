using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectABC.Data;
using ProjectABC.Utils;

namespace ProjectABC.Core
{
    public class LocalizationHelper : Singleton<LocalizationHelper>
    {
        private IReadOnlyDictionary<string, LocalizationData> _localizationMap;
        private LocaleType _localeType = LocaleType.Ko;

        // ExplicitCapture option will make match group only named
        // Groups[1], Groups[2], ... is not exists on ExplicitCapture
        private readonly Regex _localizationPattern = new Regex(
            GameConst.Localization.INNER_MATCHING_PATTERN,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture
        );
        
        public void SetLocaleType(LocaleType localeType) => _localeType = localeType;
        
        public string Localize(string key)
        {
            string localeText = GetLocaleTextOrKey(key);
            return ResolveLocalizationInText(localeText);
        }
        
        public string Localize(string key, params object[] args)
        {
            return string.Format(Localize(key), args);
        }

        private string GetLocaleTextOrKey(string key)
        {
            return _localizationMap.TryGetValue(key, out var localizationData)
                ? localizationData[_localeType]
                : key;
        }

        private string ResolveLocalizationInText(string text, int maxIterations = 8)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            for (int i = 0; i < maxIterations; i++)
            {
                string replaced = _localizationPattern.Replace(text, MatchEvaluator);

                if (replaced == text)
                {
                    return text;
                }

                text = replaced;
            }

            return text;
        }

        private string MatchEvaluator(Match match)
        {
            string innerLocalizationKey = match.Groups[GameConst.Localization.MATCHING_KEY].Value;
            return GetLocaleTextOrKey(innerLocalizationKey);
        }

        private void Initialize(LocalizationDataAsset localizationDataAsset)
        {
            _localizationMap = localizationDataAsset.LocalizationData.ToDictionary(KeySelector);
        }
        
        private static string KeySelector(LocalizationData data) => data.key;

        public static LocalizationHelper CreateInstance(LocalizationDataAsset localizationDataAsset)
        {
            LocalizationHelper instance = CreateInstanceInternal();
            instance.Initialize(localizationDataAsset);

            return instance;
        }
    }
}
