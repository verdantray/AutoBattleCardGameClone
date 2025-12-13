using UnityEngine;

namespace ProjectABC.Engine.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            FitSizeDeltaToSafeArea(Screen.safeArea);
        }

        public void FitSizeDeltaToSafeArea(Rect safeArea)
        {
            Vector2 minAnchor = safeArea.position;
            Vector2 maxAnchor = safeArea.position + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
    }
}
