using TMPro;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace ProjectABC.Engine
{
    public static class TMPFontSwapTool
    {
        public static void SwapFontToFallbackApplied(params TMP_Text[] textComponents)
        {
            if (textComponents.Length == 0 || !GlobalAssetBinder.HasInstance)
            {
                Debug.Log($"{nameof(TMPFontSwapTool)} : Not swapped to any components...");
                return;
            }

            foreach (var textComponent in textComponents)
            {
#if UNITY_EDITOR
                if (EditorUtility.IsPersistent(textComponent))
                {
                    // ignore prefab / assets
                    continue;
                }
#endif
                SwapFontInTextComponent(textComponent);
            }
        }

        private static void SwapFontInTextComponent(TMP_Text textComponent)
        {
            var fontBinder = GlobalAssetBinder.Instance.FontBinder;
            if (textComponent == null || !fontBinder.TryGetCloneFontAsset(textComponent.font, out var clone))
            {
                return;
            }

            textComponent.font = clone;
            
            if (textComponent.fontSharedMaterial != null && textComponent.fontSharedMaterial.mainTexture != clone.atlasTexture)
            {
                textComponent.fontSharedMaterial = clone.material;
            }
        }
    }
}
