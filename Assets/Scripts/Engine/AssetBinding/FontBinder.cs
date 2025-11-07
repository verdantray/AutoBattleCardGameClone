using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace ProjectABC.Engine
{
    [Serializable]
    public record AdditiveFontBindingEntry : IAssetBindEntry
    {
        public LocaleType targetLocale;
        public string fontAddressableName;
    }
    
    public sealed class FontBinder : MonoBehaviour, IAssetBinder<AdditiveFontBindingEntry>
    {
        [SerializeField] private string[] basicFontAddressableNames;
        
        private readonly Dictionary<string, AsyncOperationHandle<TMP_FontAsset>> _basicFontHandles = new Dictionary<string, AsyncOperationHandle<TMP_FontAsset>>();
        private readonly Dictionary<string, AsyncOperationHandle<TMP_FontAsset>> _additiveFontHandles = new Dictionary<string, AsyncOperationHandle<TMP_FontAsset>>();

        private readonly Dictionary<TMP_FontAsset, TMP_FontAsset> _cloneMap = new Dictionary<TMP_FontAsset, TMP_FontAsset>();
        
        private readonly Dictionary<LocaleType, List<AdditiveFontBindingEntry>> _fontBindingEntriesMap = new Dictionary<LocaleType, List<AdditiveFontBindingEntry>>();
        private readonly Queue<AssetHandleSchedule<AdditiveFontBindingEntry>> _schedules = new Queue<AssetHandleSchedule<AdditiveFontBindingEntry>>();

        // TODO : get locale from user setting
        private LocaleType CurrentLocaleSetting { get; } = LocaleType.Ko;

        private void Awake()
        {
            SceneManager.sceneLoaded += SwapFontOnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SwapFontOnSceneLoaded;
        }

        private void Start()
        {
            BindBasicFonts();
        }
        
        // TODO : subscribe user setting change event
        private void OnLocaleSettingChanged(LocaleType changedLocale)
        {
            AssetBindSchedule<AdditiveFontBindingEntry> bindChangedLocaleSchedule
                = new AssetBindSchedule<AdditiveFontBindingEntry>(_fontBindingEntriesMap[changedLocale]);
            _schedules.Enqueue(bindChangedLocaleSchedule);

            foreach (LocaleType unuseLocale in _fontBindingEntriesMap.Keys.Where(locale => locale != changedLocale))
            {
                AssetReleaseSchedule<AdditiveFontBindingEntry> releaseSchedule =
                    new AssetReleaseSchedule<AdditiveFontBindingEntry>(_fontBindingEntriesMap[unuseLocale]);
                
                _schedules.Enqueue(releaseSchedule);
            }
        }

        public bool TryGetCloneFontAsset(TMP_FontAsset fontAsset, out TMP_FontAsset cloned)
        {
            return _cloneMap.TryGetValue(fontAsset, out cloned);
        }

        private void SwapFontOnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            string[] validSceneNames = new[]
            {
                GameConst.SceneName.IN_GAME,
            };

            if (!validSceneNames.Contains(scene.name))
            {
                return;
            }

            var all = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            TMPFontSwapTool.SwapFontToFallbackApplied(all);
            
            // Debug.Log($"{nameof(FontBinder)} : swap font of components in {scene.name} scene");
        }
        
        private void BindBasicFonts()
        {
            foreach (string basicFontAddressableName in basicFontAddressableNames)
            {
                if (_basicFontHandles.ContainsKey(basicFontAddressableName))
                {
                    Debug.LogWarning($"{nameof(FontBinder)} : {basicFontAddressableName} already bind..");
                    continue;
                }
                
                var handle = Addressables.LoadAssetAsync<TMP_FontAsset>(basicFontAddressableName);
                handle.Completed += CloneBasicFontsOnCompleted;
                
                _basicFontHandles.Add(basicFontAddressableName, handle);
            }
        }

        private void CloneBasicFontsOnCompleted(AsyncOperationHandle<TMP_FontAsset> handle)
        {
            TMP_FontAsset originFont = handle.Result;

            if (_cloneMap.TryGetValue(originFont, out TMP_FontAsset clone) && clone != null)
            {
                Debug.Log($"{nameof(FontBinder)} : {originFont.name} has already been bind");
                return;
            }
            
            clone = Instantiate(originFont);
            clone.name = originFont.name + " (Runtime)";
            clone.hideFlags = HideFlags.DontSave | HideFlags.DontSaveInEditor;
                
            _cloneMap[originFont] = clone;
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
        
        public Task GetAssetLoadingTask()
        {
            List<Task> tasks = new List<Task>();
            
            var basicFontLoadingTask = _basicFontHandles.Values
                .Where(FindNotCompleteHandlesYet)
                .Select(handle => handle.Task);
            
            tasks.AddRange(basicFontLoadingTask);

            var additiveFontLoadingTask = _additiveFontHandles.Values
                .Where(FindNotCompleteHandlesYet)
                .Select(handle => handle.Task);
            
            tasks.AddRange(additiveFontLoadingTask);

            return Task.WhenAll(tasks);
            
            bool FindNotCompleteHandlesYet(AsyncOperationHandle<TMP_FontAsset> handle)
            {
                return handle.IsValid() && !handle.IsDone;
            }
        }

        public bool IsAllBindingHandlesLoaded()
        {
            return _basicFontHandles.Values.All(handle => IAssetBinder.CheckHandleLoaded(handle))
                   && _additiveFontHandles.Values.All(handle => IAssetBinder.CheckHandleLoaded(handle));
        }

        public void AddAssetBindingEntry(AdditiveFontBindingEntry entry)
        {
            if (!_fontBindingEntriesMap.TryGetValue(entry.targetLocale, out var list))
            {
                list = new List<AdditiveFontBindingEntry>();
                _fontBindingEntriesMap.Add(entry.targetLocale, list);
            }
            
            _fontBindingEntriesMap[entry.targetLocale].Add(entry);

            if (entry.targetLocale != CurrentLocaleSetting)
            {
                return;
            }
            
            AssetBindSchedule<AdditiveFontBindingEntry> bindSchedule = new AssetBindSchedule<AdditiveFontBindingEntry>(entry);
            _schedules.Enqueue(bindSchedule);
        }

        public bool RemoveAssetBindingEntry(AdditiveFontBindingEntry entry)
        {
            if (!_fontBindingEntriesMap.TryGetValue(entry.targetLocale, out var list))
            {
                list = new List<AdditiveFontBindingEntry>();
                _fontBindingEntriesMap.Add(entry.targetLocale, list);
            }
            
            bool isRemoved = _fontBindingEntriesMap[entry.targetLocale].Remove(entry);

            if (isRemoved)
            {
                AssetReleaseSchedule<AdditiveFontBindingEntry> releaseSchedule = new AssetReleaseSchedule<AdditiveFontBindingEntry>(entry);
                _schedules.Enqueue(releaseSchedule);
            }

            return isRemoved;
        }

        public bool TryGetBindingHandle(AdditiveFontBindingEntry entry, out AsyncOperationHandle handle)
        {
            bool result = _additiveFontHandles.TryGetValue(entry.fontAddressableName, out var handleT);
            
            handle = handleT;
            return result;
        }

        public void BindAsset(AdditiveFontBindingEntry entry)
        {
            if (_additiveFontHandles.ContainsKey(entry.fontAddressableName))
            {
                Debug.LogWarning($"{nameof(FontBinder)} : {entry.fontAddressableName} is already bind");
                return;
            }

            var handle = Addressables.LoadAssetAsync<TMP_FontAsset>(entry.fontAddressableName);
            handle.Completed += RegisterFallbackFontTableOnCompleted;
            
            _additiveFontHandles.Add(entry.fontAddressableName, handle);
        }
        
        private void RegisterFallbackFontTableOnCompleted(AsyncOperationHandle<TMP_FontAsset> handle)
        {
            TMP_FontAsset fallbackFont = handle.Result;
            fallbackFont.ReadFontAssetDefinition();
            // Debug.Log($"{nameof(FontBinder)} : register fallback '{fallbackFont.name}'");

            foreach (var clonedBaseFont in _cloneMap.Values)
            {
                if (clonedBaseFont.fallbackFontAssetTable.Contains(fallbackFont))
                {
                    continue;
                }
                
                clonedBaseFont.fallbackFontAssetTable.Add(fallbackFont);
                clonedBaseFont.ReadFontAssetDefinition();
            }
        }

        public void ReleaseAsset(AdditiveFontBindingEntry entry)
        {
            string addressableName = entry.fontAddressableName;
            if (!_additiveFontHandles.TryGetValue(addressableName, out var handle))
            {
                Debug.LogWarning($"{nameof(FontBinder)} : {addressableName} not bind, can't release");
                return;
            }

            if (!handle.IsDone)
            {
                Debug.LogWarning($"{nameof(FontBinder)} : {addressableName} asset is loading yet...");
                return;
            }

            foreach (var clonedBaseFont in _cloneMap.Values)
            {
                clonedBaseFont.fallbackFontAssetTable.Remove(handle.Result);
            }

            _additiveFontHandles.Remove(addressableName);
            handle.Release();
        }
    }
}
