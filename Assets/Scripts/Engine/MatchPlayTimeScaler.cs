using ProjectABC.Utils;

namespace ProjectABC.Engine
{
    public sealed class MatchPlayTimeScaler : Singleton<MatchPlayTimeScaler>
    {
        public float ScaleTimes { get; private set; } = 1.0f;
        
        public void SetScaleTimes(float scaleTimes) => ScaleTimes = scaleTimes;

        public float GetScaledDuration(float duration)
        {
            return duration / ScaleTimes;
        }

        public static MatchPlayTimeScaler CreateInstance() => CreateInstanceInternal();
    }
}
