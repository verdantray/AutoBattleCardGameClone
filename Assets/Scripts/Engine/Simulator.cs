using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.Engine.Scene;
using ProjectABC.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine
{
    public sealed class Simulator : MonoSingleton<Simulator>
    {
        [Serializable]
        private class RoundPhase
        {
            public int targetRound;
            public GamePhaseAsset[] phases;
        }
        
        [SerializeField] private ABCModel model;
        [SerializeField] private RoundPhase[] roundPhases;
        [SerializeField] private ScriptedPlayerEntry enemyEntry;

        public static ABCModel Model => Instance.model;
        
        protected override bool SetPersistent => false;
        
        private Simulation _simulation;

        private void Start()
        {
            Initialize();
            RunAsync().Forget();
        }

        public void Initialize()
        {
            _simulation ??= new Simulation(new LocalPlayerEntry(), enemyEntry);
            _simulation.ClearGamePhases();
            
            foreach (var roundPhase in roundPhases.OrderBy(roundPhase => roundPhase.targetRound))
            {
                foreach (var gamePhase in roundPhase.phases)
                {
                    _simulation.EnqueueGamePhase(gamePhase);
                }
            }
        }

        public async Task RunAsync()
        {
            try
            {
                Task preloadingTask = GetPreloadingTask();
                await preloadingTask;

                await BlurOffWhileDurationAsync(0.25f);

                await _simulation.RunAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Simulator has error occurred : {e}");
                throw;
            }
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
                Model.cardObjectSpawner.GetInitializingTask(),
            };
            
            return Task.WhenAll(tasks);
        }

        private async Task BlurOffWhileDurationAsync(float duration)
        {
            PersistentWorldCamera.Instance.BlurOff(duration);
            await Task.Delay(TimeSpan.FromSeconds(duration));
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
