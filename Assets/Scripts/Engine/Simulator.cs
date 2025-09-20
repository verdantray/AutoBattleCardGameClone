using System;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.Engine;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectABC.Engine
{
    public class Simulator : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                var gameDataAssetHandle = Addressables.LoadAssetAsync<GameDataAsset>(GameConst.Address.GAME_DATA_ASSET);
                var localizationDataAssetHandle = Addressables.LoadAssetAsync<LocalizationDataAsset>(GameConst.Address.LOCALIZATION_DATA_ASSET);

                await Task.WhenAll(gameDataAssetHandle.Task, localizationDataAssetHandle.Task);

                Storage.CreateInstance(gameDataAssetHandle.Result);
                Addressables.Release(gameDataAssetHandle);

                LocalizationHelper.CreateInstance(localizationDataAssetHandle.Result);
                Addressables.Release(localizationDataAssetHandle);

                await CardMaterialLoader.LoadMaterials();
                
                Run();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Simulator has error occured : {e}");
                throw; // TODO 예외 처리
            }
        }

        public async void Run()
        {
            Debug.Log("Simulator has started");
            
            await RunSimulationAsync();
            
            Debug.Log("Simulator has finished");
        }

        private async Task RunSimulationAsync()
        {
            IPlayer[] players = new IPlayer[8];

            // players[0] = new InGamePlayer("내 플레이어");
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = new ScriptedPlayer($"플레이어 {(char)('A' + i)}");
            }
                
            Simulation simulation = new Simulation(players);
            await simulation.RunAsync();
        }
    }
}
