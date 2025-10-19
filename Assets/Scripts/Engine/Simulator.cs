using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine
{
    public sealed class Simulator : MonoSingleton<Simulator>
    {
        [SerializeField] private ABCModel model;
        [SerializeField] private GamePhaseAsset[] phases;

        public static ABCModel Model => Instance.model;
        protected override bool SetPersistent => false;
        
        private Simulation _simulation;

        private async void Start()
        {
            try
            {
                Initialize();
                await RunAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(Simulator)} : An error occured.. {e}");
            }
        }

        public void Initialize()
        {
            _simulation ??= new Simulation();
            _simulation.ClearGamePhases();

            foreach (var phase in phases)
            {
                _simulation.EnqueueGamePhase(phase);
            }
        }

        public async Task RunAsync()
        {
            Task preloadingTask = GetPreloadingTask();
            await preloadingTask;
            
            await _simulation.RunAsync();
        }

        public void Stop()
        {
            _simulation?.Stop();
        }

        private static Task GetPreloadingTask()
        {
            List<Task> tasks = new List<Task>
            {
                GetDataAssetsLoadingTask(),
                SceneLoader.Instance.GetPostSceneLoadingTask(),
                GlobalAssetBinder.Instance.GetAssetBindingTask(),
                Model.cardObjectSpawner.GetInitializingTask(),
            };
            
            return Task.WhenAll(tasks);
        }

        private static Task GetDataAssetsLoadingTask()
        {
            Task gameDataAssetLoadingTask = Task.CompletedTask;
            if (!Storage.HasInstance)
            {
                var handle = Addressables.LoadAssetAsync<GameDataAsset>(GameConst.Address.GAME_DATA_ASSET);
                handle.Completed += OnGameDataAssetLoaded;

                gameDataAssetLoadingTask = handle.Task;
            }

            Task localizationDataAssetLoadingTask = Task.CompletedTask;
            if (!LocalizationHelper.HasInstance)
            {
                var handle = Addressables.LoadAssetAsync<LocalizationDataAsset>(GameConst.Address.LOCALIZATION_DATA_ASSET);
                handle.Completed += OnLocalizationDataAssetLoaded;
                
                localizationDataAssetLoadingTask = handle.Task;
            }

            return Task.WhenAll(gameDataAssetLoadingTask, localizationDataAssetLoadingTask);
        }

        private static void OnGameDataAssetLoaded(AsyncOperationHandle<GameDataAsset> handle)
        {
            Storage.CreateInstance(handle.Result);
            handle.Release();
        }

        private static void OnLocalizationDataAssetLoaded(AsyncOperationHandle<LocalizationDataAsset> handle)
        {
            LocalizationHelper.CreateInstance(handle.Result);
            handle.Release();
        }
    }
}
