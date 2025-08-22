using System;

namespace ProjectABC.Data
{
    public enum LocaleType
    {
        Ko,
        En,
        Ja,
    }
    
    [Serializable]
    public record LocalizationData
    {
        public string key;
        public string ko;
        public string en;
        public string ja;
    }
}