using System;

namespace ProjectABC.Data
{
    [Serializable]
    public record AdditiveFontBindingEntry : IAssetBindEntry
    {
        public LocaleType targetLocale;
        public string fontAddressableName;
    }
}
