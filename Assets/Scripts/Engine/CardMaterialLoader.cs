using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectABC.Engine
{
    public class CardMaterialLoader : MonoBehaviour
    {
        public static CardMaterialLoader Instance { get; private set; } = null;
        
        private static bool HasInstance => (bool)Instance;
        
        private readonly Dictionary<string, Material> _materialCacheMap = new Dictionary<string, Material>();

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static async Task LoadMaterials(CancellationToken token = default)
        {
            try
            {
                while (!HasInstance)
                {
                    await Task.Yield();
                }
            }
            catch (OperationCanceledException e) when (token.IsCancellationRequested)
            {
                Debug.LogError($"Waiting card material loader task is canceled...{e}");
                throw;
            }

            var handle = Addressables.LoadAssetsAsync<Material>("Materials", OnLoadMaterial);
            await handle.Task;
        }

        private static void OnLoadMaterial(Material mat)
        {
            Instance._materialCacheMap[mat.name] = mat;
            // Debug.Log($"{mat.name} has been loaded");
        }

        public List<Material> GetCardMaterials(string cardMaterialKey)
        {
            List<Material> materials = new List<Material>();

            Material faceMat;
            Material backMat = _materialCacheMap[GameConst.AssetPath.CARD_BACK_PATH];
            
            if (string.IsNullOrEmpty(cardMaterialKey) || !_materialCacheMap.TryGetValue(cardMaterialKey, out faceMat))
            {
                faceMat = _materialCacheMap[GameConst.AssetPath.CARD_FRONT_FALLBACK_PATH];
            }
            
            materials.Add(faceMat);
            materials.Add(backMat);
            
            return materials;
        }
    }
}
