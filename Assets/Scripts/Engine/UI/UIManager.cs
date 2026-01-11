using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine.UI
{
    [Serializable]
    public record UIBindingEntry : IAssetBindEntry
    {
        public AssetReference uiReference;
    }

    public abstract class UIElement : MonoBehaviour
    {
        public virtual void OnOpen()
        {
            
        }

        public virtual void OnClose()
        {
            
        }

        public abstract void Refresh();

        public async Task WaitUntilCloseAsync()
        {
            while (gameObject.activeInHierarchy)
            {
                await Task.Yield();
            }
        }
    }
    
    public sealed class UIManager : MonoSingleton<UIManager>, IAssetBinder<UIBindingEntry>
    {
        [SerializeField] private Canvas canvas;
        
        private readonly Dictionary<Type, UIElement> _uiElements = new Dictionary<Type, UIElement>();
        
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _uiHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
        private readonly Queue<AssetHandleSchedule<UIBindingEntry>> _schedules = new Queue<AssetHandleSchedule<UIBindingEntry>>();
        
        protected override bool SetPersistent => true;

        public Canvas Canvas => canvas;

        public T OpenUI<T>() where T : UIElement
        {
            T uiObject = GetUI<T>();
            
            if (uiObject != null && !uiObject.gameObject.activeInHierarchy)
            {
                uiObject.gameObject.SetActive(true);
                uiObject.OnOpen();
                uiObject.transform.SetAsLastSibling();
            }

            return uiObject;
        }

        public T GetUI<T>() where T : UIElement
        {
            if (!_uiElements.ContainsKey(typeof(T)))
            {
                // Debug.LogWarning($"{nameof(UIManager)} : can't open {nameof(T)}");
                return null;
            }
            
            UIElement element = _uiElements[typeof(T)];
            return element as T;
        }

        public bool CloseUI<T>() where T : UIElement
        {
            if (!_uiElements.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"{nameof(UIManager)} : can't close {nameof(T)}");
                return false;
            }
            
            UIElement element = _uiElements[typeof(T)];
            element.OnClose();
            element.gameObject.SetActive(false);

            return true;
        }

        public bool IsOpen<T>() where T : UIElement
        {
            if (!_uiElements.TryGetValue(typeof(T), out UIElement element))
            {
                Debug.LogWarning($"{nameof(UIManager)} : can't find {nameof(T)}");
                return false;
            }

            return element.gameObject.activeInHierarchy;
        }

        public void Refresh()
        {
            foreach (var uiElement in _uiElements.Values)
            {
                if (!uiElement.gameObject.activeInHierarchy)
                {
                    continue;
                }
                
                uiElement.Refresh();
            }
        }
        
        private void Update()
        {
            if (_schedules.Count == 0)
            {
                return;
            }

            if (!_schedules.Peek().Precondition(this))
            {
                return;
            }
            
            var schedule = _schedules.Dequeue();
            schedule.Run(this);
        }

        public bool IsAllBindingHandlesLoaded()
        {
            return _uiHandles.Values.All(handle => IAssetBinder.CheckHandleLoaded(handle));
        }

        public void AddAssetBindingEntry(UIBindingEntry entry)
        {
            AssetBindSchedule<UIBindingEntry> bindSchedule = new AssetBindSchedule<UIBindingEntry>(entry);
            _schedules.Enqueue(bindSchedule);
        }

        public bool RemoveAssetBindingEntry(UIBindingEntry entry)
        {
            if (!_uiHandles.ContainsKey(entry.uiReference.AssetGUID))
            {
                return false;
            }
            
            AssetReleaseSchedule<UIBindingEntry> releaseSchedule = new AssetReleaseSchedule<UIBindingEntry>(entry);
            _schedules.Enqueue(releaseSchedule);

            return true;
        }

        public bool TryGetBindingHandle(UIBindingEntry entry, out AsyncOperationHandle handle)
        {
            string key = entry.uiReference.AssetGUID;
            bool result = _uiHandles.TryGetValue(key, out var handleT);
            
            handle = handleT;
            return result;
        }

        public void BindAsset(UIBindingEntry entry)
        {
            if (_uiHandles.ContainsKey(entry.uiReference.AssetGUID))
            {
                Debug.LogWarning($"{nameof(UIManager)} : {entry.uiReference.AssetGUID} already exists");
                return;
            }

            var handle = Addressables.InstantiateAsync(entry.uiReference, canvas.transform);
            handle.Completed += RegisterUIElementOnCompleted;
            
            _uiHandles.Add(entry.uiReference.AssetGUID, handle);
        }

        private void RegisterUIElementOnCompleted(AsyncOperationHandle<GameObject> handle)
        {
            UIElement uiElement = handle.Result.GetComponent<UIElement>();
            Type elementType = uiElement.GetType();
            
            TMPFontSwapTool.SwapFontToFallbackApplied(uiElement.GetComponentsInChildren<TMP_Text>(true));
            
            uiElement.gameObject.SetActive(false);
            _uiElements.Add(elementType, uiElement);
        }

        public void ReleaseAsset(UIBindingEntry entry)
        {
            string key = entry.uiReference.AssetGUID;
            
            if (!_uiHandles.TryGetValue(key, out var handle))
            {
                Debug.LogWarning($"{nameof(UIManager)} : {key} not bind, can't release");
                return;
            }

            if (!handle.IsDone)
            {
                Debug.LogWarning($"{nameof(UIManager)} : {key} asset is loading yet...");
                return;
            }

            GameObject loadedUIObject = handle.Result;
            var kvPair = _uiElements.FirstOrDefault(pair => pair.Value.gameObject == loadedUIObject);
            if (kvPair.Key != null)
            {
                _uiElements.Remove(kvPair.Key);
                Destroy(kvPair.Value);
            }
            else
            {
                Debug.LogWarning($"UIElement not found binding by {key}");
            }

            _uiHandles.Remove(key);
            handle.Release();
        }
    }
}
