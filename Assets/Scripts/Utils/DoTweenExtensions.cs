using DG.Tweening;

namespace ProjectABC.Utils
{
    public static class DoTweenExtensions
    {
        public static bool IsActiveAndPlaying(this Tween tween)
        {
            return tween.IsActive() && tween.IsPlaying() && !tween.IsComplete();
        }
    }
}
