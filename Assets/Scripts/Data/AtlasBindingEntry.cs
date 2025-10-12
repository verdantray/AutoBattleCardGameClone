using System;

namespace ProjectABC.Data
{
    [Serializable]
    public record AtlasBindingEntry : IAssetBindEntry
    {
        public string atlasCommonName;

        public AtlasBindingEntry(string addressableName)
        {
            string[] split = addressableName.Split(AtlasIdentifier.IDENTIFIER_SEPARATOR);
            atlasCommonName = split.Length > 1
                ? string.Join(AtlasIdentifier.IDENTIFIER_SEPARATOR, split[..^2])
                : addressableName;
        }

        public AtlasIdentifier GetIdentifier(AtlasQuality quality)
        {
            return new AtlasIdentifier(atlasCommonName, quality);
        }
    }
    
    public enum AtlasQuality
    {
        None,
        Low,
        Medium,
        High,
    }
    
    public record AtlasIdentifier
    {
        public const char IDENTIFIER_SEPARATOR = '_';
        
        public readonly string AtlasName;
        public readonly AtlasQuality QualitySuffix;

        public AtlasIdentifier(string atlasCommonName, AtlasQuality qualitySuffix)
        {
            AtlasName = atlasCommonName;
            QualitySuffix = qualitySuffix;
        }

        public AtlasIdentifier(string addressableName)
        {
            string[] split = addressableName.Split(IDENTIFIER_SEPARATOR);
            
            if (Enum.TryParse(split.Length > 1 ? split[^1] : string.Empty, true, out QualitySuffix))
            {
                AtlasName = string.Join(IDENTIFIER_SEPARATOR, split[..^2]);
            }
            else
            {
                AtlasName = addressableName;
                QualitySuffix = AtlasQuality.None;
            }
        }

        public string GetAddressableName()
        {
            return QualitySuffix != AtlasQuality.None
                ? $"{AtlasName}{IDENTIFIER_SEPARATOR}{QualitySuffix.ToString().ToLowerInvariant()}"
                : AtlasName;
        }

        public static bool IsVariants(AtlasIdentifier a, AtlasIdentifier b)
        {
            return a != null && b != null && a != b && a.AtlasName == b.AtlasName;
        }
    }
}
