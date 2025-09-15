using System;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.InGame;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectABC.InGame
{
    public class Simulator : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                if (!Storage.HasInstance)
                {
                    var gameDataAssetHandle = Addressables.LoadAssetAsync<GameDataAsset>(GameConst.Address.GAME_DATA_ASSET);
                    var localizationDataAssetHandle = Addressables.LoadAssetAsync<LocalizationDataAsset>(GameConst.Address.LOCALIZATION_DATA_ASSET);

                    await Task.WhenAll(gameDataAssetHandle.Task, localizationDataAssetHandle.Task);

                    Storage.CreateInstance(gameDataAssetHandle.Result);
                    Addressables.Release(gameDataAssetHandle);

                    LocalizationHelper.CreateInstance(localizationDataAssetHandle.Result);
                    Addressables.Release(localizationDataAssetHandle);
                }

                await CardMaterialLoader.LoadMaterials();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Simulator has error occured : {e}");
                throw; // TODO 예외 처리
            }
            finally
            {
                Run();
            }
        }

        public async void Run()
        {
            try
            {
                await RunSimulationAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Simulator has error occured : {e}");
                throw; // TODO 예외 처리
            }
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
